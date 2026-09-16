use super::*;
use chrono::Datelike;
fn week() -> String {
    let d = chrono::Utc::now().date_naive();
    (d - chrono::Duration::days(d.weekday().num_days_from_monday() as i64)).to_string()
}
pub(super) async fn mail_reward(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    index: i64,
    title: &str,
) -> Result<()> {
    if index <= 0 {
        return Ok(());
    }
    let def = s
        .tables
        .get_reward(index as i32)
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    let mut items = vec![];
    let mut currencies = vec![];
    for drop in def.roll_items(&s.tables.reward_string_pool) {
        let (code, filter) = crate::tables::parse_item_code(&drop.item_code);
        if matches!(
            code.as_str(),
            "Gold"
                | "Gem"
                | "GuildPoint"
                | "GuildArenaPoint"
                | "PvpCoin"
                | "WorldBossPoint"
                | "RaidPoint"
        ) {
            currencies.push(json!({"CurrencyType":code,"Amount":drop.count}));
            continue;
        }
        let (id, count, star) = if let Some(id) = s.tables.get_item_index(&code) {
            (id, drop.count, drop.star_min)
        } else {
            s.tables
                .roll_item_from_group_code(&code, &filter)
                .map(|(id, count, star, _)| (id, count * drop.count, star))
                .ok_or_else(|| rule("ItemDataNotFound"))?
        };
        if star != 0 || drop.custom_option_index != 0 {
            return Err(rule("ItemDataNotFound"));
        }
        items.push(json!({"ItemIndex":id,"ItemCount":count}));
    }
    sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_gold,reward_gem,reward_items,reward_currencies,expires_at) VALUES(?,'Guild',?,'Guild reward',?,?,?,?,?)").bind(a).bind(title).bind(def.roll_gold()).bind(def.roll_gem()).bind(json!(items).to_string()).bind(json!(currencies).to_string()).bind(time(now()+7*86400)).execute(db).await?;
    Ok(())
}
async fn attendance(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    g: i64,
    action: &str,
    info: &mut Value,
) -> Result<Value> {
    let mut member = get(db, a, "attendance", g).await?;
    if member.is_null() {
        member = json!({"GuildId":g,"AccountId":a,"LastAttendanceTime":null,"Successive":0,"SuccessiveCount":0,"MaxSuccessiveCount":0,"Daily":0,"Weekly":0});
    }
    if member["Week"] != week() {
        member["Week"] = json!(week());
        member["Weekly"] = json!(0);
        for d in 1..=7 {
            member[format!("Day{d}")] = json!(0);
        }
    }
    if member["Day"] != day() {
        member["Day"] = json!(day());
        member["Daily"] = json!(0);
    }
    if action == "set_guild_attendance" {
        claim(db, a, "guild_attend", 0, &day()).await?;
        let yesterday = time(now() - 86400)[..10].to_string();
        let successive = if member["LastDay"] == yesterday {
            n(&member, "Successive") + 1
        } else {
            1
        };
        member["LastDay"] = json!(day());
        member["LastAttendanceTime"] = json!(time(now()));
        member["UpdatedTime"] = json!(time(now()));
        member["Daily"] = json!(1);
        member["Weekly"] = json!(n(&member, "Weekly") + 1);
        member["Successive"] = json!(successive);
        member["SuccessiveCount"] = json!(successive);
        member["MaxSuccessiveCount"] = json!(n(&member, "MaxSuccessiveCount").max(successive));
        member[format!("Day{}", chrono::Utc::now().weekday().number_from_monday())] = json!(1);
        // A separate immutable record keeps totals correct when members leave.
        claim(db, a, "guild_attend_total", g, &day()).await?;
        hero::currency(
            db,
            a,
            "GuildPoint",
            settings(s, "GuildAttendancePoint", 100),
        )
        .await?;
        info["ActivityPoint"] =
            json!(n(info, "ActivityPoint") + settings(s, "GuildAttendanceActivity", 100));
        guild::save(db, g, info).await?;
    }
    put(db, a, "attendance", g, &member).await?;
    let daily:i64=sqlx::query_scalar("SELECT count(*) FROM community_claims WHERE kind='guild_attend_total' AND target=? AND period=?").bind(g).bind(day()).fetch_one(&mut *db).await?;
    let weekly:i64=sqlx::query_scalar("SELECT count(*) FROM community_claims WHERE kind='guild_attend_total' AND target=? AND period>=? AND period<=?").bind(g).bind(week()).bind(day()).fetch_one(&mut *db).await?;
    let total = json!({"GuildId":g,"Daily":daily,"Weekly":weekly,"UpdatedTime":time(now()),"LastDaily":daily,"LastDailyUpdatedTime":time(now()),"LastWeekly":weekly,"LastWeeklyUpdatedTime":time(now())});
    if action == "send_guild_attendance_reward" {
        if member["LastDay"] != day() {
            return Err(rule("ActionNotReady"));
        }
        for def in s.tables.arena_guild.rows("GuildAttendanceReward") {
            let kind = n(def, "Index");
            let amount = match kind {
                1 => daily,
                2 => weekly,
                3 => n(&member, "Weekly"),
                4 => n(&member, "Successive"),
                _ => continue,
            };
            if amount < n(def, "Day") {
                continue;
            }
            if kind == 3 && amount != n(def, "Day") {
                continue;
            }
            let period = match kind {
                1 | 3 => day(),
                2 => week(),
                _ => "all".into(),
            };
            let target = kind * 1000000 + n(def, "Day");
            let inserted=sqlx::query("INSERT OR IGNORE INTO community_claims(account,kind,target,period) VALUES(?,'guild_attendance_reward',?,?)").bind(a).bind(target).bind(period).execute(&mut *db).await?.rows_affected();
            if inserted == 1 {
                mail_reward(db, s, a, n(def, "RewardIndex"), "Guild attendance reward").await?;
            }
        }
    }
    Ok(json!({"GuildAttendanceInfo":total,"GuildMemberAttendanceInfo":member}))
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    g: i64,
    role: i64,
    r: &Request,
    action: &str,
    mut info: Value,
) -> Result<Value> {
    let mut out = item::success();
    match action {
        "get_guild_attendance" | "set_guild_attendance" | "send_guild_attendance_reward" => {
            return attendance(db, s, a, g, action, &mut info).await
        }
        "contribute_guild" => {
            let mut m = guild::member(db, s, a).await?;
            if n(&m, "ContributeCount") <= 0 {
                return Err(rule("NoContributeCount"));
            }
            let cost = constant(s, "ReqGemContributeGuild", 100);
            let before = n(&info, "ContributedGem");
            let after = before + cost;
            let max = s
                .tables
                .arena_guild
                .rows("GuildContributeReward")
                .iter()
                .map(|v| n(v, "ContributedGem"))
                .max()
                .unwrap_or(50000);
            if after > max {
                return Err(rule("MaxContributedGem"));
            }
            out["CurrencyResult"] = hero::currency(db, a, "Gem", -cost).await?;
            let mut activity = settings(s, "GuildContributionActivity", 1000);
            for threshold in s.tables.arena_guild.rows("GuildContributeReward") {
                if n(threshold, "ContributedGem") > before
                    && n(threshold, "ContributedGem") <= after
                {
                    activity += n(threshold, "RewardGuildActivityPoint");
                }
            }
            let points = hero::currency(
                db,
                a,
                "GuildPoint",
                settings(s, "GuildContributionPoint", 100),
            )
            .await?;
            out["GuildPointResult"] = points;
            info["ActivityPoint"] = json!(n(&info, "ActivityPoint") + activity);
            info["ContributedGem"] = json!(after);
            m["AccActivityPoint"] = json!(n(&m, "AccActivityPoint") + activity);
            m["ContributeCount"] = json!(n(&m, "ContributeCount") - 1);
            put(db, a, "guild_contribution", 0, &json!({"Day":guild::contribution_day(s),"Count":settings(s,"GuildContributionsPerDay",5)-n(&m,"ContributeCount")})).await?;
            for kind in ["Wood", "Stone", "Metal"] {
                let amount = n(
                    &s.tables.arena_guild.rules["GuildContributionMaterials"],
                    kind,
                )
                .max(0);
                info[kind] = json!(n(&info, kind) + amount);
            }
            put(db, a, "guild_member", g, &m).await?;
            sqlx::query("UPDATE guild_members SET contribution=contribution+? WHERE account_id=? AND guild_id=?").bind(activity).bind(a).bind(g).execute(&mut *db).await?;
            out["ContributedGem"] = json!(after);
            out["GuildActivityPointResult"] = json!({"NewValue":n(&info,"ActivityPoint")});
            out["GuildMemberAccActivityPointResult"] =
                json!({"AddValue":activity,"NewValue":n(&m,"AccActivityPoint")});
        }
        "open_guild_building" => {
            let v = get(db, a, "guild_visit", g).await?;
            let mut visit = v.clone();
            if visit.is_null() {
                visit = json!({"GuildMemberRewardedTime":null});
            }
            visit["OpenGuildTime"] = json!(time(now()));
            put(db, a, "guild_visit", g, &visit).await?;
            out["OpenGuildTime"] = visit["OpenGuildTime"].clone();
            out["GuildMemberRewardedTime"] = visit["GuildMemberRewardedTime"].clone();
            out["GuildBuildingInfos"] = info["GuildBuildingInfos"].clone();
        }
        "level_up_guild" => {
            guild::admin(role)?;
            let current = n(&info, "Level");
            if int(r, "Level")? != current + 1 {
                return Err(rule("InvalidGuildLevel"));
            }
            let next = row(s, "GuildLevel", &[("Level", current + 1)])
                .map_err(|_| rule("MaxGuildLevel"))?;
            let cost = n(
                row(s, "GuildLevel", &[("Level", current)])?,
                "ActivityPoint",
            );
            if n(&info, "ActivityPoint") < cost {
                return Err(rule("NotEnoughActivityPoint"));
            }
            info["ActivityPoint"] = json!(n(&info, "ActivityPoint") - cost);
            info["Level"] = json!(current + 1);
            sqlx::query("UPDATE guilds SET max_members=? WHERE guild_id=?")
                .bind(n(next, "MaxMember"))
                .bind(g)
                .execute(&mut *db)
                .await?;
            if let Some(building) = info["GuildBuildingInfos"]
                .as_array_mut()
                .and_then(|v| v.iter_mut().find(|v| n(v, "BuildingIndex") == 1))
            {
                building["BuildingLevel"] = json!(current + 1);
                out["GuildBuildingInfo"] = building.clone();
            }
            out["GuildActivityPointResult"] = json!({"NewValue":n(&info,"ActivityPoint")});
        }
        "invest_guild_building" | "level_up_guild_building" => {
            guild::admin(role)?;
            let id = int(r, "BuildingIndex")?;
            if id == 1 {
                return Err(rule("InvalidBuildingIndex"));
            }
            let buildings = info["GuildBuildingInfos"]
                .as_array()
                .ok_or_else(|| rule("GuildBuildingDataNotFound"))?;
            let pos = buildings
                .iter()
                .position(|v| n(v, "BuildingIndex") == id)
                .ok_or_else(|| rule("InvalidBuildingIndex"))?;
            let mut building = buildings[pos].clone();
            let level = n(&building, "BuildingLevel");
            let next = row(s, "GuildBuilding", &[("Index", id), ("Level", level + 1)])
                .map_err(|_| rule("MaxGuildBuildingLevel"))?;
            if n(next, "ReqGuildLevel") > n(&info, "Level") {
                return Err(rule("InvalidGuildLevel"));
            }
            if action == "invest_guild_building" {
                let mut any = false;
                for kind in ["Wood", "Stone", "Metal"] {
                    let amount = int(r, kind)?;
                    any |= amount > 0;
                    let need = n(next, &format!("Req{kind}"));
                    if n(&building, kind) + amount > need {
                        return Err(rule(&format!("Exceed{kind}")));
                    }
                    if n(&info, kind) < amount {
                        return Err(rule(&format!("NotEnough{kind}")));
                    }
                    building[kind] = json!(n(&building, kind) + amount);
                    info[kind] = json!(n(&info, kind) - amount);
                }
                if !any {
                    return Err(rule("InvalidValue"));
                }
            } else {
                if int(r, "BuildingLevel")? != level + 1 {
                    return Err(rule("InvalidBuildingLevel"));
                }
                for kind in ["Wood", "Stone", "Metal"] {
                    let remaining = (n(next, &format!("Req{kind}")) - n(&building, kind)).max(0);
                    if n(&info, kind) < remaining {
                        return Err(rule(&format!("NotEnough{kind}")));
                    }
                    info[kind] = json!(n(&info, kind) - remaining);
                    building[kind] = json!(0);
                }
                building["BuildingLevel"] = json!(level + 1);
            }
            building["UpdatedTime"] = json!(time(now()));
            info["GuildBuildingInfos"][pos] = building.clone();
            out["GuildBuildingInfo"] = building;
            for kind in ["Wood", "Stone", "Metal"] {
                out[kind] = info[kind].clone();
            }
        }
        "request_level_up_guild_skill" => {
            let id = int(r, "GuildSkillIndex")?;
            row(s, "GuildSkill", &[("GuildSkillIndex", id)])?;
            let mut m = guild::member(db, s, a).await?;
            if n(&m, "ReqGuildSkill") == id {
                return Err(rule("AlreadyGuildSkillLevelUpRequested"));
            }
            m["ReqGuildSkill"] = json!(id);
            put(db, a, "guild_member", g, &m).await?;
        }
        "level_up_guild_skill" => {
            guild::admin(role)?;
            let id = int(r, "SkillIndex")?;
            let skills = info["SkillInfos"].as_array().cloned().unwrap_or_default();
            let level = skills
                .iter()
                .find(|v| n(v, "GuildSkillIndex") == id)
                .map(|v| n(v, "GuildSkillLevel"))
                .unwrap_or(0);
            if int(r, "SkillLevel")? != level + 1 {
                return Err(rule("InvalidSkillLevel"));
            }
            let next = row(
                s,
                "GuildSkillLevel",
                &[("GuildSkillIndex", id), ("GuildSkillLevel", level + 1)],
            )
            .map_err(|_| rule("MaxSkillLevel"))?;
            let training = info["GuildBuildingInfos"]
                .as_array()
                .into_iter()
                .flatten()
                .find(|v| {
                    n(v, "BuildingIndex") == constant(s, "GuildTrainingCenterBuildingIndex", 2)
                })
                .map(|v| n(v, "BuildingLevel"))
                .unwrap_or(0);
            if training < n(next, "TrainingCenterLevel") {
                return Err(rule("NotEnoughTrainingCenterLevel"));
            }
            for (kind, error) in [
                ("ActivityPoint", "NotEnoughActivityPoint"),
                ("SuppressPoint", "NotEnoughSuppressPoint"),
            ] {
                let cost = n(next, kind);
                if n(&info, kind) < cost {
                    return Err(rule(error));
                }
                info[kind] = json!(n(&info, kind) - cost);
                out[format!("Guild{kind}Result")] = json!({"NewValue":n(&info,kind)});
            }
            let mut skills = skills;
            skills.retain(|v| n(v, "GuildSkillIndex") != id);
            let previous = info["SkillInfos"]
                .as_array()
                .into_iter()
                .flatten()
                .find(|v| n(v, "GuildSkillIndex") == id);
            let ap =
                previous.map(|v| n(v, "ActivitySpent")).unwrap_or(0) + n(next, "ActivityPoint");
            let sp =
                previous.map(|v| n(v, "SuppressSpent")).unwrap_or(0) + n(next, "SuppressPoint");
            skills.push(json!({"GuildSkillIndex":id,"SkillIndex":id,"EffectSkillIndex":n(next,"SkillIndex"),"SkillLevel":level+1,"GuildSkillLevel":level+1,"ReqCount":0,"UpdatedTime":time(now()),"ActivitySpent":ap,"SuppressSpent":sp}));
            info["SkillInfos"] = json!(skills);
            sqlx::query("UPDATE community_state SET data=json_set(data,'$.ReqGuildSkill',0) WHERE kind='guild_member' AND idx=? AND json_extract(data,'$.ReqGuildSkill')=?").bind(g).bind(id).execute(&mut *db).await?;
            if id <= 6 {
                info[format!("Skill{id}Level")] = json!(level + 1);
            }
        }
        "init_guild_skill" => {
            guild::admin(role)?;
            if info["SkillInfos"]
                .as_array()
                .is_none_or(|v| v.iter().all(|s| n(s, "GuildSkillLevel") == 0))
            {
                return Err(rule("AlreadyGuildSkillInit"));
            }
            if now() < n(&info, "SkillResetAt") + constant(s, "InitGuildSkillCoolTime", 10080) * 60
            {
                return Err(rule("InitCoolTimeNotElapsed"));
            }
            let skills = info["SkillInfos"].as_array().unwrap();
            let ap: i64 = skills.iter().map(|v| n(v, "ActivitySpent")).sum();
            let sp: i64 = skills.iter().map(|v| n(v, "SuppressSpent")).sum();
            info["ActivityPoint"] = json!(n(&info, "ActivityPoint") + ap);
            info["SuppressPoint"] = json!(n(&info, "SuppressPoint") + sp);
            info["SkillInfos"] = json!([]);
            for id in 1..=6 {
                info[format!("Skill{id}Level")] = json!(0);
            }
            info["SkillResetAt"] = json!(now());
            info["SkillInitTime"] = json!(time(now()));
            out["GuildActivityPointResult"] = json!({"NewValue":n(&info,"ActivityPoint")});
            out["GuildSuppressPointResult"] = json!({"NewValue":n(&info,"SuppressPoint")});
        }
        "give_reward_guild_member" => {
            let visit = get(db, a, "guild_visit", g).await?;
            if now() - n(&visit, "RewardAt") < settings(s, "GuildMemberRewardSeconds", 86400) {
                return Err(rule("NotElapsedGuildMemberRewardedTime"));
            }
            let building = info["GuildBuildingInfos"]
                .as_array()
                .into_iter()
                .flatten()
                .find(|v| n(v, "BuildingIndex") == 4)
                .ok_or_else(|| rule("ContentsDisabled"))?;
            let def = row(
                s,
                "GuildBuilding",
                &[("Index", 4), ("Level", n(building, "BuildingLevel"))],
            )?;
            let index = n(def, "GuildRaidRewardIndex");
            if index <= 0 {
                return Err(rule("ContentsDisabled"));
            }
            let mut rw = Rewards::default();
            reward(db, s, a, index, &mut rw).await?;
            merge(&mut out, rewards(db, s, a, rw).await?);
            let mut visit = visit;
            if visit.is_null() {
                visit = json!({});
            }
            visit["RewardAt"] = json!(now());
            visit["GuildMemberRewardedTime"] = json!(time(now()));
            put(db, a, "guild_visit", g, &visit).await?;
            out["GuildMemberRewardedTime"] = visit["GuildMemberRewardedTime"].clone();
        }
        _ => return Err(rule("ContentsDisabled")),
    }
    guild::save(db, g, &info).await?;
    Ok(out)
}
