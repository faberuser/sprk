use super::*;

// Keep legacy database roles: master=2, admin=1, member=0. Convert at the wire boundary.
pub(super) async fn membership(db: &mut SqliteConnection, a: i64) -> Result<(i64, i64)> {
    let r = sqlx::query("SELECT guild_id,role FROM guild_members WHERE account_id=?")
        .bind(a)
        .fetch_optional(db)
        .await?
        .ok_or_else(|| rule("NoGuild"))?;
    Ok((r.get("guild_id"), r.get("role")))
}
pub(super) fn admin(role: i64) -> Result<()> {
    if role < 1 {
        Err(rule("InvalidRank"))
    } else {
        Ok(())
    }
}
pub(super) async fn contents_available(db: &mut SqliteConnection, a: i64) -> Result<()> {
    let p = get(db, a, "withdraw", 0).await?;
    if now() < n(&p, "ContentsAt") {
        return Err(rule("ContentsDisabled"));
    }
    Ok(())
}
fn master(role: i64) -> Result<()> {
    if role != 2 {
        Err(rule("InvalidRank"))
    } else {
        Ok(())
    }
}
pub(super) async fn state(db: &mut SqliteConnection, s: &AppState, g: i64) -> Result<Value> {
    let r=sqlx::query("SELECT g.*,a.nick AS master_nick,(SELECT count(*) FROM guild_members m WHERE m.guild_id=g.guild_id) AS members FROM guilds g JOIN accounts a ON a.account_id=g.master_account_id WHERE g.guild_id=?").bind(g).fetch_optional(&mut *db).await?.ok_or_else(||rule("NoGuild"))?;
    let mut v = get(db, g, "guild", 0).await?;
    if v.is_null() {
        v = json!({"Logo":1,"Back":1,"Flag":0,"JoinWay":1,"ReqTeamLevel":1,"Introduce":"","CountryCode":"US","ActivityPoint":r.get::<i64,_>("exp"),"SuppressPoint":0,"Wood":0,"Stone":0,"Metal":0,"SkillInfos":[],"SkillInitTime":null,"AutoApplySuppress":0});
    }
    let level = r.get::<i64, _>("level");
    v["Id"] = json!(g);
    v["Name"] = json!(r.get::<String, _>("name"));
    v["Notice"] = json!(r.get::<Option<String>, _>("notice").unwrap_or_default());
    v["Level"] = json!(level);
    v["CurMember"] = json!(r.get::<i64, _>("members"));
    v["MasterNick"] = json!(r
        .get::<Option<String>, _>("master_nick")
        .unwrap_or_default());
    if v["GuildBuildingInfos"].is_null() {
        v["GuildBuildingInfos"]=json!(s.tables.arena_guild.rows("GuildBuilding").iter().filter(|v|n(v,"Level")==1).map(|r|json!({"BuildingIndex":n(r,"Index"),"BuildingLevel":if n(r,"ReqGuildLevel")<=level{1}else{0},"Wood":0,"Stone":0,"Metal":0,"UpdatedTime":null})).collect::<Vec<_>>());
    }
    let period = contribution_day(s);
    if v["ContributionDay"] != period {
        v["ContributionDay"] = json!(period);
        v["ContributedGem"] = json!(0);
    }
    let votes=sqlx::query("SELECT json_extract(c.data,'$.ReqGuildSkill') AS skill,COUNT(*) AS votes FROM community_state c JOIN guild_members m ON m.account_id=c.owner AND m.guild_id=c.idx WHERE c.kind='guild_member' AND c.idx=? AND json_extract(c.data,'$.ReqGuildSkill')>0 GROUP BY skill").bind(g).fetch_all(&mut *db).await?;
    let mut skills = v["SkillInfos"].as_array().cloned().unwrap_or_default();
    for skill in &mut skills {
        skill["ReqCount"] = json!(0);
    }
    for vote in votes {
        let id = vote.get::<i64, _>("skill");
        if !skills.iter().any(|v| n(v, "SkillIndex") == id) {
            skills.push(json!({"SkillIndex":id,"GuildSkillIndex":id,"SkillLevel":0,"GuildSkillLevel":0,"ReqCount":0,"UpdatedTime":null}));
        }
        if let Some(skill) = skills.iter_mut().find(|v| n(v, "SkillIndex") == id) {
            skill["ReqCount"] = json!(vote.get::<i64, _>("votes").min(255));
        }
    }
    v["SkillInfos"] = json!(skills);
    put(db, g, "guild", 0, &v).await?;
    Ok(v)
}
pub(super) fn contribution_day(s: &AppState) -> String {
    time(now() - constant(s, "GuildContributeResetHour", 3) * 3600)[..10].into()
}
pub(super) async fn member(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let (g, role) = membership(db, a).await?;
    let mut u = user(db, a).await?;
    let mut v = get(db, a, "guild_member", g).await?;
    if v.is_null() {
        v = json!({"AccActivityPoint":0,"AccSuppressPoint":0,"AccWood":0,"AccStone":0,"AccMetal":0,"ReqGuildSkill":0,"BidRaidIndex":0,"BidRaidRewardItemIndex":0,"BidRaidRewardTime":null});
    }
    if v["Day"] != contribution_day(s) {
        v["Day"] = json!(contribution_day(s));
    }
    let contribution = get(db, a, "guild_contribution", 0).await?;
    v["ContributeCount"] = json!(
        settings(s, "GuildContributionsPerDay", 5)
            - if contribution["Day"] == contribution_day(s) {
                n(&contribution, "Count")
            } else {
                0
            }
    );
    v["RaidEnterCount"] = super::guild_ticket(db, s, a, 0).await?["NewValue"].clone();
    v["Rank"] = json!(3 - role);
    put(db, a, "guild_member", g, &v).await?;
    merge(&mut u, v);
    Ok(u)
}
pub(super) async fn save(db: &mut SqliteConnection, g: i64, v: &Value) -> Result<()> {
    sqlx::query("UPDATE guilds SET exp=?,level=?,notice=? WHERE guild_id=?")
        .bind(n(v, "ActivityPoint"))
        .bind(n(v, "Level"))
        .bind(v["Notice"].as_str().unwrap_or(""))
        .bind(g)
        .execute(&mut *db)
        .await?;
    put(db, g, "guild", 0, v).await
}
async fn name(db: &mut SqliteConnection, s: &AppState, name: &str) -> Result<()> {
    let len = name.chars().count() as i64;
    if len < constant(s, "MinGuildNameLength", 4) {
        return Err(rule("TooShortName"));
    }
    if len > constant(s, "MaxGuildNameLength", 12) {
        return Err(rule("TooLongName"));
    }
    if name.trim() != name
        || name
            .chars()
            .any(|c| c.is_control() || matches!(c, '<' | '>' | '\n' | '\r'))
    {
        return Err(rule("InvalidName"));
    }
    if sqlx::query_scalar::<_, i64>("SELECT count(*) FROM guilds WHERE name=? COLLATE NOCASE")
        .bind(name)
        .fetch_one(db)
        .await?
        > 0
    {
        return Err(rule("AlreadyExistName"));
    }
    Ok(())
}
fn text(r: &Request, k: &str, max: usize) -> Result<String> {
    let v = r.text(k);
    if v.chars().count() > max || v.chars().any(|c| c.is_control() && c != '\n') {
        return Err(rule("InvalidGuildIntroduce"));
    }
    Ok(v.to_string())
}
fn settings_fields(s: &AppState, r: &Request, create: bool) -> Result<Value> {
    let fields = if create {
        [
            "logo",
            "logoBackground",
            "joinWay",
            "reqTeamLevel",
            "introduce",
            "countryCode",
        ]
    } else {
        [
            "Logo",
            "Back",
            "JoinWay",
            "TeamLevel",
            "Introduce",
            "CountryCode",
        ]
    };
    let logo = r.number(fields[0], 1)?;
    let back = r.number(fields[1], 1)?;
    let way = r.number(fields[2], 1)?;
    let level = r.number(fields[3], 1)?;
    if !(0..=constant(s, "MaxGuildIcon", 10)).contains(&logo)
        || !(0..=constant(s, "MaxGuildBG", 9)).contains(&back)
        || !matches!(way, 1 | 2)
        || !(1..=255).contains(&level)
    {
        return Err(rule("InvalidValue"));
    }
    let country = if r.text(fields[5]).is_empty() {
        "US"
    } else {
        r.text(fields[5])
    };
    if country.len() != 2 || !country.bytes().all(|b| b.is_ascii_alphabetic()) {
        return Err(rule("InvalidValue"));
    }
    Ok(
        json!({"Logo":logo,"Back":back,"JoinWay":way,"ReqTeamLevel":level,"Introduce":text(r,fields[4],constant(s,"MaxGuildIntroduceLength",26) as usize)?,"CountryCode":country.to_uppercase()}),
    )
}
async fn joinable(db: &mut SqliteConnection, s: &AppState, a: i64, g: i64) -> Result<Value> {
    if membership(db, a).await.is_ok() {
        return Err(rule("AlreadyJoined"));
    }
    let p = get(db, a, "withdraw", 0).await?;
    if now() < n(&p, "RejoinAt") {
        return Err(rule("NotElapsedGuildRequestCoolTime"));
    }
    let v = state(db, s, g).await?;
    let max = n(
        row(s, "GuildLevel", &[("Level", n(&v, "Level"))])?,
        "MaxMember",
    );
    if n(&v, "CurMember") >= max {
        return Err(rule("MaxMember"));
    }
    if n(&user(db, a).await?, "TeamLevel") < n(&v, "ReqTeamLevel") {
        return Err(rule("LowTeamLevel"));
    }
    Ok(v)
}
async fn join(db: &mut SqliteConnection, a: i64, g: i64) -> Result<()> {
    sqlx::query("INSERT INTO guild_members(guild_id,account_id,role,contribution) VALUES(?,?,0,0)")
        .bind(g)
        .bind(a)
        .execute(&mut *db)
        .await?;
    sqlx::query("DELETE FROM guild_requests WHERE account=?")
        .bind(a)
        .execute(&mut *db)
        .await?;
    sqlx::query("UPDATE guilds SET member_count=(SELECT count(*) FROM guild_members WHERE guild_id=?) WHERE guild_id=?").bind(g).bind(g).execute(db).await?;
    Ok(())
}
async fn withdraw(db: &mut SqliteConnection, s: &AppState, a: i64, g: i64) -> Result<Value> {
    let mut p = get(db, a, "withdraw", 0).await?;
    if p.is_null() {
        p = json!({});
    }
    let count = if now() - n(&p, "At") > constant(s, "GuildPenaltyResetTimeMin", 21600) * 60 {
        1
    } else {
        n(&p, "Count") + 1
    };
    let penalty = s
        .tables
        .arena_guild
        .rows("GuildPenalty")
        .iter()
        .filter(|v| n(v, "WithdrawCount") <= count)
        .max_by_key(|v| n(v, "WithdrawCount"));
    let wait = penalty.map(|v| n(v, "RejoinTimeMin")).unwrap_or(0);
    p = json!({"Count":count,"At":now(),"RejoinAt":now()+wait*60,"ContentsAt":now()+penalty.map(|v|n(v,"RestrictTimeMin")).unwrap_or(0)*60});
    put(db, a, "withdraw", 0, &p).await?;
    sqlx::query("DELETE FROM guild_members WHERE account_id=? AND guild_id=?")
        .bind(a)
        .bind(g)
        .execute(&mut *db)
        .await?;
    sqlx::query("DELETE FROM guild_arena_decks WHERE account=?")
        .bind(a)
        .execute(&mut *db)
        .await?;
    sqlx::query("UPDATE guilds SET member_count=(SELECT count(*) FROM guild_members WHERE guild_id=?) WHERE guild_id=?").bind(g).bind(g).execute(db).await?;
    Ok(json!({"GuildWithdrawCount":count,"GuildWithdrawTime":time(now())}))
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    let mut out = item::success();
    match action {
        "check_guild_name" => {
            name(db, s, r.text("name")).await?;
        }
        "create_guild" => {
            if membership(db, a).await.is_ok() {
                return Err(rule("AlreadyJoined"));
            }
            if now() < n(&get(db, a, "withdraw", 0).await?, "RejoinAt") {
                return Err(rule("NotElapsedGuildRequestCoolTime"));
            }
            name(db, s, r.text("name")).await?;
            let fields = settings_fields(s, r, true)?;
            if n(&user(db, a).await?, "TeamLevel") < settings(s, "GuildCreateTeamLevel", 10) {
                return Err(rule("NotEnoughTeamLevel"));
            }
            out["CurrencyResult"] =
                hero::currency(db, a, "Gold", -constant(s, "GuildCreateGold", 3000000)).await?;
            let max = n(row(s, "GuildLevel", &[("Level", 1)])?, "MaxMember");
            let g = sqlx::query(
                "INSERT INTO guilds(name,master_account_id,max_members,notice) VALUES(?,?,?,'')",
            )
            .bind(r.text("name"))
            .bind(a)
            .bind(max)
            .execute(&mut *db)
            .await?
            .last_insert_rowid();
            join(db, a, g).await?;
            sqlx::query("UPDATE guild_members SET role=2 WHERE account_id=?")
                .bind(a)
                .execute(&mut *db)
                .await?;
            let mut info = state(db, s, g).await?;
            merge(&mut info, fields);
            save(db, g, &info).await?;
            out["GuildId"] = json!(g);
        }
        "get_guildlist" | "get_join_requested_guilds" => {
            let ids: Vec<i64> = if action == "get_join_requested_guilds" {
                sqlx::query_scalar(
                    "SELECT guild_id FROM guild_requests WHERE account=? ORDER BY created",
                )
                .bind(a)
                .fetch_all(&mut *db)
                .await?
            } else {
                sqlx::query_scalar(
                    "SELECT guild_id FROM guilds ORDER BY level DESC,exp DESC,guild_id LIMIT 50",
                )
                .fetch_all(&mut *db)
                .await?
            };
            let mut infos = Vec::new();
            for id in ids {
                infos.push(state(db, s, id).await?);
            }
            out["guildInfos"] = json!(infos);
        }
        "search_guild" => {
            let g: Option<i64> =
                sqlx::query_scalar("SELECT guild_id FROM guilds WHERE name=? COLLATE NOCASE")
                    .bind(r.text("name"))
                    .fetch_optional(&mut *db)
                    .await?;
            out["Info"] = state(db, s, g.ok_or_else(|| rule("NoGuild"))?).await?;
        }
        "get_all_guild_member_info" => {
            let g = r.number("GuildId", 0)?;
            state(db, s, g).await?;
            let ids:Vec<i64>=sqlx::query_scalar("SELECT account_id FROM guild_members WHERE guild_id=? ORDER BY role DESC,account_id").bind(g).fetch_all(&mut *db).await?;
            let mut members = vec![];
            for id in ids {
                members.push(member(db, s, id).await?);
            }
            out["MemberInfos"] = json!(members);
        }
        "request_join_guild" => {
            let g = r.number("GuildId", 0)?;
            let info = joinable(db, s, a, g).await?;
            let way = n(&info, "JoinWay");
            if way == 1 {
                join(db, a, g).await?;
                out["guildInfo"] = state(db, s, g).await?;
            } else {
                let count: i64 =
                    sqlx::query_scalar("SELECT count(*) FROM guild_requests WHERE account=?")
                        .bind(a)
                        .fetch_one(&mut *db)
                        .await?;
                let received: i64 =
                    sqlx::query_scalar("SELECT count(*) FROM guild_requests WHERE guild_id=?")
                        .bind(g)
                        .fetch_one(&mut *db)
                        .await?;
                if count >= constant(s, "MaxJoinGuildRequest", 5) {
                    return Err(rule("MaxJoinRequest"));
                }
                if received >= constant(s, "MaxRecvJoinGuildRequest", 50) {
                    return Err(rule("GuildMaxJoinRequest"));
                }
                if sqlx::query(
                    "INSERT OR IGNORE INTO guild_requests(account,guild_id,created) VALUES(?,?,?)",
                )
                .bind(a)
                .bind(g)
                .bind(now())
                .execute(db)
                .await?
                .rows_affected()
                    != 1
                {
                    return Err(rule("AlreadyJoinRequested"));
                }
            }
            out["JoinWay"] = json!(way);
        }
        "cancel_request_join_guild" => {
            if sqlx::query("DELETE FROM guild_requests WHERE account=? AND guild_id=?")
                .bind(a)
                .bind(r.number("GuildId", 0)?)
                .execute(db)
                .await?
                .rows_affected()
                == 0
            {
                return Err(rule("GuildJoinNotRequested"));
            }
        }
        _ => {
            let (g, role) = membership(db, a).await?;
            let mut info = state(db, s, g).await?;
            match action {
                "get_guildbasicinfo" => {
                    out["info"] = info;
                }
                "get_join_request_players" => {
                    admin(role)?;
                    let ids: Vec<i64> = sqlx::query_scalar(
                        "SELECT account FROM guild_requests WHERE guild_id=? ORDER BY created",
                    )
                    .bind(g)
                    .fetch_all(&mut *db)
                    .await?;
                    let mut p = vec![];
                    for id in ids {
                        p.push(user(db, id).await?);
                    }
                    out["players"] = json!(p);
                }
                "accept_join_request" | "reject_join_request" => {
                    admin(role)?;
                    let target = r.number("AccountId", 0)?;
                    if sqlx::query_scalar::<_, i64>(
                        "SELECT count(*) FROM guild_requests WHERE account=? AND guild_id=?",
                    )
                    .bind(target)
                    .bind(g)
                    .fetch_one(&mut *db)
                    .await?
                        == 0
                    {
                        return Err(rule("GuildJoinNotRequested"));
                    }
                    if action == "accept_join_request" {
                        joinable(db, s, target, g).await?;
                        join(db, target, g).await?;
                        out["MemberInfo"] = member(db, s, target).await?;
                    } else {
                        sqlx::query("DELETE FROM guild_requests WHERE account=? AND guild_id=?")
                            .bind(target)
                            .bind(g)
                            .execute(db)
                            .await?;
                    }
                }
                "apply_guild_setting_change" => {
                    admin(role)?;
                    let fields = settings_fields(s, r, false)?;
                    if n(&fields, "JoinWay") != n(&info, "JoinWay")
                        && sqlx::query_scalar::<_, i64>(
                            "SELECT count(*) FROM guild_requests WHERE guild_id=?",
                        )
                        .bind(g)
                        .fetch_one(&mut *db)
                        .await?
                            > 0
                    {
                        return Err(rule("GuildJoinRequestExist"));
                    }
                    merge(&mut info, fields);
                    info["Notice"] = json!(text(
                        r,
                        "Notice",
                        constant(s, "MaxGuildNoticeLength", 200) as usize
                    )?);
                    let flag = int(r, "Flag")?;
                    if flag != n(&info, "Flag") {
                        return Err(rule("InvalidRank"));
                    }
                    save(db, g, &info).await?;
                }
                "send_guild_notice" => {
                    admin(role)?;
                    info["Notice"] = json!(text(
                        r,
                        "Notice",
                        constant(s, "MaxGuildNoticeLength", 200) as usize
                    )?);
                    save(db, g, &info).await?;
                }
                "set_guild_admin_rank" => {
                    master(role)?;
                    let target = r.number("AccountId", 0)?;
                    let (tg, tr) = membership(db, target).await?;
                    let rank = int(r, "GuildRank")?;
                    if tg != g || target == a || tr == 2 || !matches!(rank, 2 | 3) {
                        return Err(rule("InvalidRank"));
                    }
                    if rank == 2 && tr != 1 {
                        let max = n(
                            row(s, "GuildLevel", &[("Level", n(&info, "Level"))])?,
                            "MaxAdmin",
                        );
                        if sqlx::query_scalar::<_, i64>(
                            "SELECT count(*) FROM guild_members WHERE guild_id=? AND role=1",
                        )
                        .bind(g)
                        .fetch_one(&mut *db)
                        .await?
                            >= max
                        {
                            return Err(rule("MaxGuildAdmin"));
                        }
                    }
                    sqlx::query(
                        "UPDATE guild_members SET role=? WHERE account_id=? AND guild_id=?",
                    )
                    .bind(3 - rank)
                    .bind(target)
                    .bind(g)
                    .execute(db)
                    .await?;
                }
                "delegate_master" => {
                    master(role)?;
                    let target = r.number("DelegateAccountId", 0)?;
                    let (tg, _) = membership(db, target).await?;
                    if tg != g || target == a {
                        return Err(rule("InvalidRank"));
                    }
                    sqlx::query("UPDATE guild_members SET role=CASE WHEN account_id=? THEN 2 ELSE 0 END WHERE guild_id=? AND account_id IN (?,?)").bind(target).bind(g).bind(target).bind(a).execute(&mut *db).await?;
                    sqlx::query("UPDATE guilds SET master_account_id=? WHERE guild_id=?")
                        .bind(target)
                        .bind(g)
                        .execute(db)
                        .await?;
                }
                "kick_guildmember" => {
                    admin(role)?;
                    let target = r.number("AccountId", 0)?;
                    let (tg, tr) = membership(db, target).await?;
                    if tg != g || role <= tr || target == a {
                        return Err(rule("InvalidTargetRank"));
                    }
                    withdraw(db, s, target, g).await?;
                }
                "withdraw_guild" => {
                    if role == 2 {
                        return Err(rule("InvalidRank"));
                    }
                    merge(&mut out, withdraw(db, s, a, g).await?);
                }
                "destroy_guild" => {
                    master(role)?;
                    if n(&info, "CurMember") != 1 {
                        return Err(rule("GuildMemberExist"));
                    }
                    merge(&mut out, withdraw(db, s, a, g).await?);
                    sqlx::query("DELETE FROM guild_requests WHERE guild_id=?")
                        .bind(g)
                        .execute(&mut *db)
                        .await?;
                    sqlx::query("DELETE FROM guilds WHERE guild_id=?")
                        .bind(g)
                        .execute(db)
                        .await?;
                }
                _ => return progression::execute(db, s, a, g, role, r, action, info).await,
            }
        }
    }
    Ok(out)
}
