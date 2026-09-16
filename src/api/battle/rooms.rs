use super::*;
pub(super) async fn remove_account(db: &mut SqliteConnection, a: i64) -> Result<()> {
    leave(db, a, a, None).await
}

async fn room(db: &mut SqliteConnection, id: i64, family: &str) -> Result<Value> {
    let v: Option<String> =
        sqlx::query_scalar("SELECT data FROM battle_rooms WHERE id=? AND family=?")
            .bind(id)
            .bind(family)
            .fetch_optional(db)
            .await?;
    v.map(|s| read_json(&s).map_err(Into::into))
        .unwrap_or_else(|| Err(rule("RoomNotExist")))
}
async fn members(db: &mut SqliteConnection, id: i64) -> Result<Vec<i64>> {
    Ok(sqlx::query_scalar(
        "SELECT account FROM battle_room_members WHERE room=? ORDER BY joined,account",
    )
    .bind(id)
    .fetch_all(db)
    .await?)
}
async fn save(db: &mut SqliteConnection, id: i64, v: &Value) -> Result<()> {
    sqlx::query("UPDATE battle_rooms SET master=?,data=?,updated=? WHERE id=?")
        .bind(n(v, "MasterAccountId"))
        .bind(v.to_string())
        .bind(now())
        .bind(id)
        .execute(db)
        .await?;
    Ok(())
}
async fn prune(db: &mut SqliteConnection, s: &AppState) -> Result<()> {
    let expired: Vec<i64> =
        sqlx::query_scalar("SELECT account FROM battle_room_members WHERE updated<?")
            .bind(now() - settings(s, "RoomTimeoutSeconds", 120))
            .fetch_all(&mut *db)
            .await?;
    for a in expired {
        leave(db, a, a, None).await?;
    }
    Ok(())
}
async fn leave(
    db: &mut SqliteConnection,
    actor: i64,
    target: i64,
    requested: Option<i64>,
) -> Result<()> {
    let membership:Option<(i64,i64,String)>=sqlx::query_as("SELECT r.id,r.master,r.data FROM battle_room_members m JOIN battle_rooms r ON r.id=m.room WHERE m.account=?").bind(actor).fetch_optional(&mut *db).await?;
    let (id, master, text) = membership.ok_or_else(|| rule("RoomNotExist"))?;
    if requested.is_some_and(|r| r != id) || actor != target && actor != master {
        return Err(rule("RoomNotExist"));
    }
    let count = sqlx::query("DELETE FROM battle_room_members WHERE room=? AND account=?")
        .bind(id)
        .bind(target)
        .execute(&mut *db)
        .await?
        .rows_affected();
    if count != 1 {
        return Err(rule("RoomNotExist"));
    }
    let ids = members(db, id).await?;
    if ids.is_empty() {
        sqlx::query("DELETE FROM battle_rooms WHERE id=?")
            .bind(id)
            .execute(db)
            .await?;
        return Ok(());
    }
    let mut v: Value = read_json(&text)?;
    v["CurPartyMember"] = json!(ids.len());
    if master == target {
        let next = ids[0];
        let nick: String = sqlx::query_scalar("SELECT nick FROM accounts WHERE account_id=?")
            .bind(next)
            .fetch_one(&mut *db)
            .await?;
        v["MasterAccountId"] = json!(next);
        v["MasterNick"] = json!(nick);
    }
    save(db, id, &v).await
}
fn definition<'a>(s: &'a AppState, r: &Request, family: &str) -> Result<&'a Value> {
    if family == "raid" {
        let v = row(
            s,
            "Raid",
            &[
                ("Index", int(r, "RaidIndex")?),
                ("Level", int(r, "RaidLevel")?),
            ],
        )?;
        if v["IsOpen"] != true {
            return Err(rule("NoRaidData"));
        }
        Ok(v)
    } else {
        let v = row(
            s,
            "PartyDungeon",
            &[
                ("ChapterIndex", int(r, "ChapterIndex")?),
                ("DungeonIndex", int(r, "DungeonIndex")?),
            ],
        )?;
        let diff = int(r, "DungeonDifficulty")?;
        let key = ["None", "Easy", "Normal", "Hard", "Hell"]
            .get(diff as usize)
            .ok_or_else(|| rule("InvalidRoomInfo"))?;
        if v[*key] != true || n(v, "DungeonType") != int(r, "DungeonType")? {
            return Err(rule("InvalidRoomInfo"));
        }
        Ok(v)
    }
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
) -> Result<Value> {
    prune(db, s).await?;
    let (family, action) = path.split_once('/').unwrap();
    let mut out = item::success();
    if action.starts_with("create_") {
        let def = definition(s, r, family)?;
        let existing: bool =
            sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_room_members WHERE account=?)")
                .bind(a)
                .fetch_one(&mut *db)
                .await?;
        if existing {
            return Err(rule("AlreadyJoined"));
        }
        let opened = int(r, "Opened")?;
        if opened > 1 {
            return Err(rule("InvalidRoomInfo"));
        }
        let nick: String = sqlx::query_scalar("SELECT nick FROM accounts WHERE account_id=?")
            .bind(a)
            .fetch_one(&mut *db)
            .await?;
        let mut v = json!({"RoomNo":0,"RaidIndex":int(r,"RaidIndex")?,"RaidLevel":int(r,"RaidLevel")?,"RaidMonsterLevel":int(r,"RaidMonsterLevel")?,"GoalIndex":int(r,"GoalIndex")?,"DungeonType":int(r,"DungeonType")?,"ChapterIndex":n(def,"ChapterIndex"),"DungeonIndex":n(def,"DungeonIndex"),"DungeonDifficulty":int(r,"DungeonDifficulty")?,"Opened":opened,"MasterAccountId":a,"MasterNick":nick,"CurPartyMember":1,"IsAutoRoot":false,"AffixIndices":ids(r,"AffixIndices",20)?,"Status":0,"Capacity":n(def,"PlayerCount").clamp(1,8)});
        let id =
            sqlx::query("INSERT INTO battle_rooms(family,master,data,updated) VALUES(?,?,?,?)")
                .bind(family)
                .bind(a)
                .bind(v.to_string())
                .bind(now())
                .execute(&mut *db)
                .await?
                .last_insert_rowid();
        v["RoomNo"] = json!(id);
        save(db, id, &v).await?;
        sqlx::query("INSERT INTO battle_room_members(account,room,joined,updated) VALUES(?,?,?,?)")
            .bind(a)
            .bind(id)
            .bind(now())
            .bind(now())
            .execute(db)
            .await?;
        out["RoomNo"] = json!(id);
        return Ok(out);
    }
    if action.starts_with("search_") || action.starts_with("wait_") {
        let texts: Vec<String> = sqlx::query_scalar(
            "SELECT data FROM battle_rooms WHERE family=? ORDER BY id LIMIT 100",
        )
        .bind(family)
        .fetch_all(&mut *db)
        .await?;
        let rooms: Vec<Value> = texts.iter().map(|v| read_json(v)).collect::<Result<_>>()?;
        let rooms: Vec<Value> = rooms
            .into_iter()
            .filter(|v| {
                n(v, "Opened") == 1
                    && n(v, "Status") == 0
                    && n(v, "CurPartyMember") < n(v, "Capacity")
                    && ["RaidIndex", "ChapterIndex", "DungeonIndex", "DungeonType"]
                        .iter()
                        .all(|key| {
                            r.number(key, 0).unwrap_or(0) == 0
                                || r.number(key, 0).ok() == Some(n(v, key))
                        })
            })
            .collect();
        if action.starts_with("wait_") {
            let v = rooms.first().ok_or_else(|| rule("RoomNotExist"))?;
            out["JoinedRaidRoomInfo"] = join(db, a, n(v, "RoomNo"), family).await?;
        } else {
            out["RoomInfos"] = json!(rooms);
            out["RepeatCoolTime"] = json!(1);
        }
        return Ok(out);
    }
    let id = r.number("RoomNo", 0)?;
    let mut v = room(db, id, family).await?;
    if action.starts_with("join_") {
        out["JoinedRaidRoomInfo"] = join(db, a, id, family).await?;
        return Ok(out);
    }
    if !members(db, id).await?.contains(&a) {
        return Err(rule("RoomNotExist"));
    }
    if action.starts_with("ping_") {
        sqlx::query("UPDATE battle_room_members SET updated=? WHERE account=?")
            .bind(now())
            .bind(a)
            .execute(db)
            .await?;
        return Ok(out);
    }
    if action.starts_with("leave_") {
        let target = r.number("AccountId", a)?;
        leave(db, a, if target == 0 { a } else { target }, Some(id)).await?;
        return Ok(out);
    }
    if action.starts_with("report_") {
        let reason = int(r, "Reason")?;
        put(
            db,
            a,
            "room_report",
            id,
            &json!({"Reason":reason,"Time":time(now())}),
        )
        .await?;
        return Ok(out);
    }
    if n(&v, "MasterAccountId") != a {
        return Err(rule("RoomNotExist"));
    }
    if action.starts_with("delegate_") {
        let target = r.number("CurMasterId", 0)?;
        if r.number("PrevMasterId", 0)? != a || !members(db, id).await?.contains(&target) {
            return Err(rule("NotDelegateAccountId"));
        }
        v["MasterAccountId"] = json!(target);
        v["MasterNick"] = json!(
            sqlx::query_scalar::<_, String>("SELECT nick FROM accounts WHERE account_id=?")
                .bind(target)
                .fetch_one(&mut *db)
                .await?
        );
    } else if action.starts_with("change_") {
        if n(&v, "Status") != 0 {
            return Err(rule("InvalidRoomInfo"));
        }
        let mut args = r.0.clone();
        for key in [
            "RaidIndex",
            "RaidLevel",
            "DungeonType",
            "ChapterIndex",
            "DungeonIndex",
            "DungeonDifficulty",
        ] {
            args.entry(key.into())
                .or_insert_with(|| n(&v, key).to_string());
        }
        let req = Request(args);
        let def = definition(s, &req, family)?;
        for key in [
            "RaidLevel",
            "RaidMonsterLevel",
            "GoalIndex",
            "DungeonType",
            "ChapterIndex",
            "DungeonIndex",
            "DungeonDifficulty",
            "Status",
        ] {
            if req.0.contains_key(key) {
                v[key] = json!(int(&req, key)?);
            }
        }
        if n(&v, "Status") > 1 {
            return Err(rule("InvalidRoomInfo"));
        }
        v["Capacity"] = json!(n(def, "PlayerCount").clamp(1, 8));
        if n(&v, "CurPartyMember") > n(&v, "Capacity") {
            return Err(rule("InvalidRoomInfo"));
        }
    } else {
        return Err(rule("Fail"));
    }
    save(db, id, &v).await?;
    Ok(out)
}
async fn join(db: &mut SqliteConnection, a: i64, id: i64, family: &str) -> Result<Value> {
    let mut v = room(db, id, family).await?;
    let old: Option<i64> =
        sqlx::query_scalar("SELECT room FROM battle_room_members WHERE account=?")
            .bind(a)
            .fetch_optional(&mut *db)
            .await?;
    if old == Some(id) {
        return Ok(v);
    }
    if old.is_some() {
        return Err(rule("AlreadyJoined"));
    }
    let count = members(db, id).await?.len() as i64;
    if count >= n(&v, "Capacity") || n(&v, "Status") != 0 || n(&v, "Opened") != 1 {
        return Err(rule("InvalidRoomInfo"));
    }
    sqlx::query("INSERT INTO battle_room_members(account,room,joined,updated) VALUES(?,?,?,?)")
        .bind(a)
        .bind(id)
        .bind(now())
        .bind(now())
        .execute(&mut *db)
        .await?;
    v["CurPartyMember"] = json!(count + 1);
    save(db, id, &v).await?;
    Ok(v)
}
pub(super) async fn validate_battle(
    db: &mut SqliteConnection,
    _s: &AppState,
    a: i64,
    r: &Request,
) -> Result<()> {
    let id = r.number("RaidRoomNo", 0)?.max(r.number("MultiRoomNo", 0)?);
    if id > 0 {
        let valid: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM battle_room_members WHERE account=? AND room=?)",
        )
        .bind(a)
        .bind(id)
        .fetch_one(db)
        .await?;
        if !valid {
            return Err(rule("WrongMember"));
        }
    }
    // The extracted client requires a separate real-time battle service for multiplayer.
    // Do not charge entry or grant results until that service can confirm the match.
    if id > 0
        || r.number("MultiplayMasterId", 0)? > 0
        || !matches!(r.text("MultiplayMemberIds"), "" | "[]" | "null")
    {
        return Err(rule("BattleServerNotFound"));
    }
    Ok(())
}
pub(super) async fn finish(
    _db: &mut SqliteConnection,
    _s: &AppState,
    _a: i64,
    _r: &Request,
    _won: bool,
    _out: &mut Value,
) -> Result<()> {
    Ok(())
}
pub(super) async fn reward(
    _db: &mut SqliteConnection,
    _s: &AppState,
    _a: i64,
    _r: &Request,
    _action: &str,
) -> Result<Value> {
    Err(rule("MultiplayInfoNotFound"))
}
