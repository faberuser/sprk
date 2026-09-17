//! An adapter contract, not a combat simulator. Unsupported callbacks fail closed.
use super::*;
pub(super) fn authenticate(s: &AppState, h: &HeaderMap) -> Result<()> {
    let expected = s
        .battle_service_key
        .as_ref()
        .as_ref()
        .ok_or_else(|| ServerError::Authentication("Battle service is disabled".into()))?;
    let supplied = h
        .get("x-battle-service-key")
        .and_then(|v| v.to_str().ok())
        .unwrap_or("");
    let mismatch = expected
        .as_bytes()
        .iter()
        .zip(supplied.as_bytes())
        .fold(0u8, |acc, (a, b)| acc | (a ^ b));
    if supplied.len() != expected.len() || mismatch != 0 {
        return Err(ServerError::Authentication(
            "Invalid battle service credentials".into(),
        ));
    }
    Ok(())
}
pub(super) async fn recover(db: &mut SqliteConnection, a: i64) -> Result<Value> {
    let row = sqlx::query("SELECT * FROM battle_runs WHERE account=?")
        .bind(a)
        .fetch_optional(&mut *db)
        .await?;
    let Some(row) = row else {
        return Ok(json!({"Battle":null}));
    };
    let run = row.get::<String, _>("run_id");
    let entry: Value = serde_json::from_str(row.get("entry")).map_err(|_| rule("Fail"))?;
    let result: Option<String> =
        sqlx::query_scalar("SELECT response FROM service_results WHERE run=? AND account=?")
            .bind(&run)
            .bind(a)
            .fetch_optional(&mut *db)
            .await?;
    let result = result
        .map(|v| serde_json::from_str::<Value>(&v))
        .transpose()
        .map_err(|_| rule("Fail"))?;
    let begin: Value = serde_json::from_str(row.get("begin_response")).map_err(|_| rule("Fail"))?;
    Ok(
        json!({"Battle":{"RunId":run,"Started":row.get::<i64,_>("started"),"Completed":row.get::<i64,_>("completed")!=0,"ServiceOwned":entry["ServiceOwned"]==true,"ServiceRequired":entry["ServiceRequired"]==true,"ChapterIndex":entry["ChapterIndex"],"DungeonIndex":entry["DungeonIndex"],"DungeonDifficulty":entry["DungeonDifficulty"],"Heroes":entry["Heroes"],"BeginResponse":begin,"Result":result}}),
    )
}
async fn bound_run(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    h: &HeaderMap,
) -> Result<(String, Value)> {
    let id = h
        .get("x-battle-run-id")
        .and_then(|v| v.to_str().ok())
        .filter(|v| !v.is_empty())
        .ok_or_else(|| rule("Fail"))?;
    let row = sqlx::query(
        "SELECT entry,started FROM battle_runs WHERE account=? AND run_id=? AND completed=0",
    )
    .bind(a)
    .bind(id)
    .fetch_optional(db)
    .await?
    .ok_or_else(|| rule("Fail"))?;
    if now() - row.get::<i64, _>("started")
        > s.tables.battle.rules["BattleExpirySeconds"]
            .as_i64()
            .unwrap_or(14400)
    {
        return Err(rule("Fail"));
    }
    Ok((
        id.to_string(),
        serde_json::from_str(row.get("entry")).map_err(|_| rule("Fail"))?,
    ))
}
fn coordinates(r: &Request, entry: &Value) -> Result<()> {
    for k in ["ChapterIndex", "DungeonIndex", "DungeonDifficulty"] {
        if r.number(k, -1)? != n(entry, k) {
            return Err(rule("Fail"));
        }
    }
    Ok(())
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    h: &HeaderMap,
    path: &str,
) -> Result<Value> {
    match path {
        "internal/b2m_save_replay" => super::records::execute(db, s, a, r, path, true).await,
        "internal/b2m_ping" | "internal/b2m_get_gameserver_list" => {
            let name = r.text("BattleServerName");
            if name.is_empty() || name.len() > 128 {
                return Err(rule("Fail"));
            }
            let port = r.number("Port", 0)?;
            if !(1..=65535).contains(&port) {
                return Err(rule("Fail"));
            }
            // Persist only health/registration metadata, never service credentials.
            let data = json!({"Name":name,"Address":r.text("Address"),"PublicIp":r.text("PublicIp"),"PrivateIp":r.text("PrivateIp"),"Port":port,"ServerVersion":r.text("ServerVersion"),"CpuUsage":r.text("CpuUsage"),"SessionCu":r.number("SessionCu",0)?});
            sqlx::query("INSERT INTO service_servers(name,data,seen) VALUES(?,?,?) ON CONFLICT(name) DO UPDATE SET data=excluded.data,seen=excluded.seen").bind(name).bind(data.to_string()).bind(now()).execute(&mut *db).await?;
            sqlx::query("DELETE FROM service_servers WHERE seen<?")
                .bind(now() - 86400)
                .execute(db)
                .await?;
            Ok(
                json!({"GameServerHosts":s.tables.services.rules["GameServerHosts"].as_array().cloned().unwrap_or_default(),"IsBlocked":false,"MultithreadEnabled":false,"ErrorMessage":"","BattleIntegrityCheckInfos":[],"ContentsValues":[],"ServerValues":[],"ServerConstantValues":s.tables.services.rows("Constant")}),
            )
        }
        "internal/b2m_get_match_info" => {
            let id = r.number("MatchUid", 0)?;
            let row =
                sqlx::query("SELECT * FROM arena_runs WHERE id=? AND kind=0 AND status='battle'")
                    .bind(id)
                    .fetch_optional(&mut *db)
                    .await?
                    .ok_or_else(|| rule("Fail"))?;
            let mut data: Value =
                serde_json::from_str(row.get("data")).map_err(|_| rule("Fail"))?;
            if n(&data, "Expires") < now() {
                return Err(rule("Fail"));
            }
            data["ServiceOwned"] = json!(true);
            sqlx::query("UPDATE arena_runs SET data=? WHERE id=?")
                .bind(data.to_string())
                .bind(id)
                .execute(db)
                .await?;
            Ok(
                json!({"MatchInfo":{"Uid":id,"ArenaType":"Normal","SeasonIndex":data["Season"],"AccountInfos":[data["Register"]["AccountInfo"],data["Wait"]["MatchedNpcInfo"]],"BattleServerAddress":"","BattleServerPort":0,"ChapterIndex":1000,"DungeonIndex":1,"DungeonDifficulty":"Easy","OnlineGameSpeedRatio":100,"ExtraData":null,"CanCheat":false}}),
            )
        }
        "internal/b2g_get_match_hero_info" => {
            let id = h
                .get("x-battle-run-id")
                .and_then(|v| v.to_str().ok())
                .and_then(|v| v.strip_prefix("arena:"))
                .and_then(|v| v.parse::<i64>().ok())
                .ok_or_else(|| rule("Fail"))?;
            let raw: Option<String> = sqlx::query_scalar(
                "SELECT data FROM arena_runs WHERE id=? AND account=? AND status='battle'",
            )
            .bind(id)
            .bind(a)
            .fetch_optional(&mut *db)
            .await?;
            let data: Value = serde_json::from_str(&raw.ok_or_else(|| rule("Fail"))?)
                .map_err(|_| rule("Fail"))?;
            if data["ServiceOwned"] != true || n(&data, "Expires") < now() {
                return Err(rule("Fail"));
            }
            let mut heroes = vec![];
            for hero in r.ids("HeroIndices")? {
                if !data["Heroes"]
                    .as_array()
                    .is_some_and(|v| v.contains(&json!(hero)))
                {
                    return Err(rule("Fail"));
                }
                heroes.push(crate::api::community::cached_hero(db, a, hero).await?);
            }
            Ok(json!({"HeroInfos":heroes}))
        }
        "internal/b2g_set_match_result" | "internal/b2g_set_match_cancel" => {
            let id = r.number("MatchUid", 0)?;
            let row = sqlx::query("SELECT * FROM arena_runs WHERE id=? AND account=? AND kind=0")
                .bind(id)
                .bind(a)
                .fetch_optional(&mut *db)
                .await?
                .ok_or_else(|| rule("Fail"))?;
            let mut data: Value =
                serde_json::from_str(row.get("data")).map_err(|_| rule("Fail"))?;
            if r.number("ArenaType", 0)? != 0
                || r.number("SeasonIndex", 0)? != n(&data, "Season")
                || data["ServiceOwned"] != true
            {
                return Err(rule("Fail"));
            }
            let cancel = path.ends_with("cancel");
            let canonical = json!(r.0);
            if row.get::<String, _>("status") == "complete"
                || row.get::<String, _>("status") == "canceled"
            {
                if data["ServiceRequest"] != canonical || data["ServiceCanceled"] != cancel {
                    return Err(rule("Fail"));
                }
                return Ok(if cancel {
                    json!({})
                } else {
                    json!({"MatchResult":data["End"]["MatchResult"]})
                });
            }
            if row.get::<String, _>("status") != "battle" || n(&data, "Expires") < now() {
                return Err(rule("Fail"));
            }
            if cancel {
                data["ServiceRequest"] = canonical;
                data["ServiceCanceled"] = json!(true);
                sqlx::query("UPDATE arena_runs SET status='canceled',data=? WHERE id=?")
                    .bind(data.to_string())
                    .bind(id)
                    .execute(db)
                    .await?;
                return Ok(json!({}));
            }
            let response = crate::api::community::service_match_result(db, s, a, r).await?;
            // Reload to retain the score/reward receipt written by the arena implementation.
            let raw: String = sqlx::query_scalar("SELECT data FROM arena_runs WHERE id=?")
                .bind(id)
                .fetch_one(&mut *db)
                .await?;
            data = serde_json::from_str(&raw).map_err(|_| rule("Fail"))?;
            data["ServiceRequest"] = canonical;
            data["ServiceCanceled"] = json!(false);
            sqlx::query("UPDATE arena_runs SET data=? WHERE id=?")
                .bind(data.to_string())
                .bind(id)
                .execute(db)
                .await?;
            Ok(json!({"MatchResult":response["MatchResult"]}))
        }
        "internal/b2g_server_error_log" | "internal/b2g_set_abuser" => {
            if r.0.values().map(|v| v.len()).sum::<usize>() > 65536 {
                return Err(rule("Fail"));
            }
            let allowed = s.tables.services.contracts[path]["Request"]
                .as_object()
                .ok_or_else(|| rule("Fail"))?;
            let report: serde_json::Map<String, Value> =
                r.0.iter()
                    .filter(|(k, _)| allowed.contains_key(*k) && !k.to_lowercase().contains("key"))
                    .map(|(k, v)| (k.clone(), json!(v)))
                    .collect();
            sqlx::query("INSERT INTO service_reports(kind,data,created) VALUES(?,?,?)")
                .bind(path)
                .bind(Value::Object(report).to_string())
                .bind(now())
                .execute(&mut *db)
                .await?;
            sqlx::query("DELETE FROM service_reports WHERE id IN (SELECT id FROM service_reports ORDER BY id DESC LIMIT -1 OFFSET 1000)").execute(db).await?;
            Ok(json!({}))
        }
        "internal/b2g_battle_start" => {
            let (id, mut entry) = bound_run(db, s, a, h).await?;
            coordinates(r, &entry)?;
            if n(&entry, "GuildId") > 0 {
                return Err(rule("Fail"));
            }
            entry["ServiceOwned"] = json!(true);
            sqlx::query("UPDATE battle_runs SET entry=? WHERE account=? AND run_id=?")
                .bind(entry.to_string())
                .bind(a)
                .bind(id)
                .execute(db)
                .await?;
            Ok(json!({}))
        }
        "internal/b2g_battle_cancel" => {
            let (id, entry) = bound_run(db, s, a, h).await?;
            coordinates(r, &entry)?;
            if entry["ServiceOwned"] != true {
                return Err(rule("Fail"));
            }
            sqlx::query("UPDATE battle_runs SET completed=1 WHERE account=? AND run_id=?")
                .bind(a)
                .bind(id)
                .execute(db)
                .await?;
            Ok(json!({}))
        }
        "internal/b2g_get_hero_info" => {
            let (_, entry) = bound_run(db, s, a, h).await?;
            let mut out = json!({"RaidIndex":0,"RaidLevel":0,"RemainBossHp":0,"ChapterIndex":entry["ChapterIndex"],"DungeonIndex":entry["DungeonIndex"],"DungeonDifficulty":entry["DungeonDifficulty"],"AdminLevel":0,"GuildSkills":[],"AccountBuffs":[],"ClassBuffDataBases":[],"ExtraStatDataBases":[],"PetStatDataBases":[]});
            for (key, output) in [
                ("HeroIndices", "HeroInfos"),
                ("AiHeroIndices", "AiHeroInfos"),
                ("GroupHeroIndices", "GroupHeroInfos"),
            ] {
                let mut heroes = vec![];
                for id in r.ids(key)? {
                    if !entry["Heroes"]
                        .as_array()
                        .is_some_and(|v| v.contains(&json!(id)))
                    {
                        return Err(rule("Fail"));
                    }
                    let mut hero = entry["DeckSnapshot"]["Heroes"]
                        .as_array()
                        .and_then(|v| v.iter().find(|v| n(v, "HeroIndex") == id))
                        .cloned()
                        .ok_or_else(|| rule("Fail"))?;
                    for part in 1..=10 {
                        let slot = n(&hero, &format!("EquipItemSlotIndex{part}"));
                        hero[format!("EquipItemInfo{part}")] = entry["DeckSnapshot"]["EquipItems"]
                            .as_array()
                            .and_then(|v| v.iter().find(|v| n(v, "SlotIndex") == slot))
                            .cloned()
                            .unwrap_or(Value::Null);
                    }
                    heroes.push(hero);
                }
                out[output] = json!(heroes);
            }
            Ok(out)
        }
        "internal/b2g_set_campaign_result" => {
            let id = h
                .get("x-battle-run-id")
                .and_then(|v| v.to_str().ok())
                .ok_or_else(|| rule("Fail"))?;
            let canonical = json!(r.0).to_string();
            let previous =
                sqlx::query("SELECT request FROM service_results WHERE run=? AND account=?")
                    .bind(id)
                    .bind(a)
                    .fetch_optional(&mut *db)
                    .await?;
            if let Some(row) = previous {
                if row.get::<String, _>("request") == canonical {
                    return Ok(json!({}));
                }
                return Err(rule("Fail"));
            }
            let (_, entry) = bound_run(db, s, a, h).await?;
            if entry["ServiceOwned"] != true || n(&entry, "GuildId") > 0 {
                return Err(rule("Fail"));
            }
            let win = match r.text("Win") {
                "true" | "True" | "1" => true,
                "false" | "False" | "0" => false,
                _ => return Err(rule("Fail")),
            };
            let seconds = r.number("PlayTime", 0)?;
            if !(0..=86400).contains(&seconds) {
                return Err(rule("Fail"));
            }
            let alive = r.ids("AliveHeroIndices")?;
            let mut request = Request(std::collections::HashMap::new());
            for key in [
                "ChapterIndex",
                "DungeonIndex",
                "DungeonDifficulty",
                "ScenarioDungeon",
            ] {
                request.0.insert(key.into(), entry[key].to_string());
            }
            request.0.insert("Completed".into(), win.to_string());
            request
                .0
                .insert("AliveHeroIndices".into(), json!(alive).to_string());
            let party = entry["Heroes"].as_array().map(Vec::len).unwrap_or(0);
            let star = if !win {
                0
            } else if alive.len() == party {
                3
            } else if alive.len() * 2 >= party {
                2
            } else {
                1
            };
            request.0.insert("Star".into(), star.to_string());
            request
                .0
                .insert("PureBattleTime".into(), seconds.to_string());
            request.0.insert(
                "TotalDamage".into(),
                r.number("TotalDamage", 0)?.to_string(),
            );
            let response = crate::api::battle::end_authoritative(db, s, a, &request).await?;
            sqlx::query("INSERT INTO service_results(run,account,request,response,created) VALUES(?,?,?,?,?)").bind(id).bind(a).bind(canonical).bind(response.to_string()).bind(now()).execute(db).await?;
            Ok(json!({}))
        }
        // No standalone simulator or live arena/guild-suppression runtime is present.
        // Never acknowledge these operations or grant rewards from fabricated results.
        _ => Err(rule("Fail")),
    }
}
