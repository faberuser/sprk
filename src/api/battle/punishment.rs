//! Build-1558 Divine Punishment payouts and selected filled-shard lifecycle.
use super::*;
use crate::models::item::ItemGrant;
use std::collections::BTreeMap;

pub(super) fn automatic_affixes(s: &AppState, group: i64, level: i64) -> Vec<i64> {
    // Native GetPunishmentAffixIndices adds level-based affixes to every node,
    // separately from the boss's dragon-clear domain skills.
    s.tables.battle.rows("PunishmentAutoAffix").iter()
        .filter(|row| n(row,"GroupIndex")==group && level>=n(row,"MinLevel"))
        .map(|row| n(row,"Index")).collect()
}

pub(super) fn scaled(s: &AppState, index: i64, level: i64) -> Result<i64> {
    if !(1..=i32::MAX as i64).contains(&level) {
        return Err(rule("InvalidValue"));
    }
    let data = row(s, "Infinity", &[("Index", index)])?;
    let increments = data["IncreaseValues"]
        .as_array()
        .cloned()
        .unwrap_or_default();
    let steps = level - 1;
    let mut value = n(data, "StartValue");
    for increment in increments.iter().take(steps as usize) {
        value = value
            .checked_add(increment.as_i64().ok_or_else(|| rule("InvalidValue"))?)
            .ok_or_else(|| rule("InvalidValue"))?;
    }
    if steps > increments.len() as i64 {
        let tail = increments.last().and_then(Value::as_i64).unwrap_or(0);
        value = value
            .checked_add(
                tail.checked_mul(steps - increments.len() as i64)
                    .ok_or_else(|| rule("InvalidValue"))?,
            )
            .ok_or_else(|| rule("InvalidValue"))?;
    }
    value
        .checked_mul(if n(data, "Multiplier") == 0 {
            1
        } else {
            n(data, "Multiplier")
        })
        .filter(|v| (0..=i32::MAX as i64).contains(v))
        .ok_or_else(|| rule("InvalidValue"))
}

fn ratio_count(value: i64, roll: i64) -> i64 {
    // A per-mille rate over 100% awards whole items plus the residual chance.
    value / 1000 + i64::from(roll < value % 1000)
}

pub(super) fn reward_definition<'a>(
    s: &'a AppState,
    id: i64,
    checks: &BTreeSet<i64>,
    boss: bool,
) -> Result<&'a Value> {
    s.tables
        .battle
        .rows("PunishmentRaidReward")
        .iter()
        .find(|v| {
            if n(v, "RaidIndex") != id {
                return false;
            }
            let required: BTreeSet<i64> = v["ClearCheckIndex"]
                .as_array()
                .into_iter()
                .flatten()
                .filter_map(Value::as_i64)
                .collect();
            if boss {
                v["Active"] == true && required == *checks
            } else {
                n(v, "RewardType") == 2 && required.is_subset(checks)
            }
        })
        .ok_or_else(|| rule("RewardDataNotFound"))
}

pub(super) async fn grant(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    definition: &Value,
    level: i64,
    rewards: &mut Rewards,
) -> Result<()> {
    let group = row(
        s,
        "InfinityRewardGroup",
        &[("Index", n(definition, "InfinityRewardGroupIndex"))],
    )?;
    let mut amounts = BTreeMap::<i32, i64>::new();
    for id in group["RewardIndices"]
        .as_array()
        .ok_or_else(|| rule("RewardDataNotFound"))?
    {
        let data = row(
            s,
            "InfinityReward",
            &[(
                "Index",
                id.as_i64().ok_or_else(|| rule("RewardDataNotFound"))?,
            )],
        )?;
        let value = scaled(s, n(data, "InfinityIndex"), level)?;
        let amount = if data["IsRatio"] == true {
            use rand::Rng;
            ratio_count(value, rand::thread_rng().gen_range(0..1000))
        } else {
            value
        };
        if amount == 0 {
            continue;
        }
        if n(data, "RewardType") != 1 {
            return Err(rule("InvalidItemType"));
        }
        let index = i32::try_from(n(data, "ItemIndex")).map_err(|_| rule("ItemDataNotFound"))?;
        let total = amounts.entry(index).or_default();
        *total = total
            .checked_add(amount)
            .filter(|v| *v <= i32::MAX as i64)
            .ok_or_else(|| rule("InvalidValue"))?;
    }
    for (index, count) in amounts {
        item::give(
            db,
            s,
            a,
            ItemGrant {
                index,
                count: count as i32,
                star: 0,
                custom: 0,
            },
            rewards,
        )
        .await?;
    }
    Ok(())
}

pub(super) async fn validate_shards(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    definition: &Value,
) -> Result<()> {
    let id = int(r, "FlaskItemIndex")?;
    let count = int(r, "FlaskItemCount")?;
    if !(0..=1000).contains(&count) || id < 0 || id == 0 && count > 0 {
        return Err(rule("InvalidItemCount"));
    }
    // Repeated begin requests must retain the buff selection already reserved.
    if let Some(saved) = sqlx::query_scalar::<_, String>(
        "SELECT entry FROM battle_runs WHERE account=? AND completed=0 AND started>=?",
    )
    .bind(a)
    .bind(now() - settings(s, "BattleExpirySeconds", 14400))
    .fetch_optional(&mut *db)
    .await?
    {
        let saved: Value = read_json(&saved)?;
        if n(&saved, "ChapterIndex") == int(r, "ChapterIndex")?
            && n(&saved, "DungeonIndex") == int(r, "DungeonIndex")?
        {
            let previous = Request(read_value(saved["Request"].clone())?);
            if int(&previous, "FlaskItemIndex")? != id
                || int(&previous, "FlaskItemCount")? != count
                || previous.number("Level", 1)? != r.number("Level", 1)?
            {
                return Err(rule("InvalidValue"));
            }
            return Ok(());
        }
    }
    if count == 0 {
        return Ok(());
    }
    let shard = row(s, "ShardItem", &[("Index", id)])?;
    let listed = shard["AvailableRaidIndices"]
        .as_array()
        .is_some_and(|v| v.contains(&definition["RaidIndex"]));
    // Build 1558's Solenis shard rows retain Apocalypsion's boss ID (1005).
    // The native picker uses GroupIndex; allow only the corresponding Solenis
    // boss, never the other group or its side nodes. Preserve the source rows.
    let legacy_solenis_boss = n(shard, "GroupIndex") == 101002
        && n(definition, "RaidIndex") == 1008
        && shard["AvailableRaidIndices"] == json!([1005]);
    if n(shard, "GroupIndex") != n(definition, "GroupIndex")
        || n(shard, "Duration") <= 0
        || !(listed || legacy_solenis_boss)
    {
        return Err(rule("InvalidItem"));
    }
    let owned: i64=sqlx::query_scalar("SELECT COALESCE((SELECT count FROM items WHERE account_id=? AND item_index=? AND locked=0),0)")
        .bind(a).bind(id).fetch_one(&mut *db).await?;
    if owned - entry_costs::held(db, a, "Item", id).await? < count {
        return Err(rule("ItemNotOwned"));
    }
    Ok(())
}

pub(super) async fn prepare_shards(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &mut Value,
) -> Result<()> {
    let count = int(r, "FlaskItemCount")?;
    if count == 0 {
        return Ok(());
    }
    let id = int(r, "FlaskItemIndex")?;
    let shard = row(s, "ShardItem", &[("Index", id)])?;
    let owner = uuid::Uuid::new_v4().to_string();
    sqlx::query("INSERT INTO battle_entry_holds(account,owner,family,kind,amount,expires) VALUES(?,?,'Item',?,?,?)")
        .bind(a).bind(&owner).bind(id).bind(count).bind(now()+settings(s,"BattleExpirySeconds",14400)).execute(&mut *db).await?;
    entry["PunishmentShard"] = json!({"Owner":owner,"Index":id,"Count":count,"Duration":n(shard,"Duration"),"IsWithDraw":n(shard,"IsWithDraw")});
    Ok(())
}

pub(super) async fn settle_shards(
    db: &mut SqliteConnection,
    a: i64,
    entry: &mut Value,
    won: bool,
    out: &mut Value,
) -> Result<()> {
    let shard = entry["PunishmentShard"].clone();
    let Some(owner) = shard["Owner"].as_str() else {
        return Ok(());
    };
    if shard["Settled"] == true {
        return Ok(());
    }
    entry_costs::clear_hold(db, a, owner).await?;
    // Follow the local victory-settlement policy. The archived withdrawal
    // charge flags are all zero; charge only durations started on victory.
    if won || n(&shard, "IsWithDraw") != 0 {
        let seconds = n(entry, "PunishmentShardElapsedSeconds").max(1);
        let duration = n(&shard, "Duration").max(1);
        let used =
            (seconds / duration + i64::from(seconds % duration != 0)).min(n(&shard, "Count"));
        let result = item::consume(db, a, n(&shard, "Index") as i32, used as i32).await?;
        let mut items = out["ItemResults"].as_array().cloned().unwrap_or_default();
        items.push(result);
        out["ItemResults"] = json!(items);
    }
    entry["PunishmentShard"]["Settled"] = json!(true);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::battle::tests::{account, call, setup};

    async fn fixture() -> (AppState, Value) {
        let (s, u) = setup().await;
        put(
            &mut s.db.acquire().await.unwrap(),
            account(&u),
            "dungeon",
            campaign::key(11, 10),
            &json!({"FirstRewardedDiff":1}),
        )
        .await
        .unwrap();
        sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?")
            .bind(account(&u))
            .execute(&s.db)
            .await
            .unwrap();
        sqlx::query("UPDATE user_info SET stamina=500000 WHERE account_id=?")
            .bind(account(&u))
            .execute(&s.db)
            .await
            .unwrap();
        (s, u)
    }
    async fn count(s: &AppState, a: i64, id: i64) -> i64 {
        sqlx::query_scalar(
            "SELECT COALESCE((SELECT count FROM items WHERE account_id=? AND item_index=?),0)",
        )
        .bind(a)
        .bind(id)
        .fetch_one(&s.db)
        .await
        .unwrap()
    }
    fn entry(group: i64, dungeon: i64, level: i64, extra: &str) -> String {
        let chapter = if group == 101001 { 70000 } else { 70001 };
        format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&GroupIndex={group}&DungeonType=Boss&Level={level}&HeroIndices=1{extra}")
    }
    fn end(group: i64, dungeon: i64, won: bool, seconds: i64) -> String {
        let chapter = if group == 101001 { 70000 } else { 70001 };
        format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&GroupIndex={group}&DungeonType=Boss&Completed={won}&Star={}&AliveHeroIndices=1&PureBattleTime={seconds}",if won{3}else{0})
    }
    async fn open(s: &AppState, u: &Value, group: i64, level: i64) -> Value {
        call(
            s,
            u,
            "punishment_raid/open_punishment_raid",
            &format!("GroupIndex={group}&Level={level}&DungeonType=2"),
        )
        .await
    }
    #[test]
    fn punishment_automatic_affixes_follow_native_enhancement_thresholds() {
        let tables=crate::tables::test_tables();
        let rows=tables.battle.rows("PunishmentAutoAffix");
        let active=|group,level| rows.iter().filter(|r|n(r,"GroupIndex")==group && n(r,"MinLevel")<=level)
            .map(|r|n(r,"Index")).collect::<BTreeSet<_>>();
        assert!(active(101001,1).is_empty());
        assert_eq!(active(101001,2),BTreeSet::from([10001,10004]));
        assert_eq!(active(101001,5),BTreeSet::from([10001,10004,10005,1937001]));
        assert_eq!(active(101002,2),BTreeSet::from([10024,10026]));
        for group in [101001,101002] {
            assert_eq!(active(group,40).len(),25);
            assert_eq!(active(group,40),active(group,100));
        }
    }

    #[tokio::test]
    async fn punishment_enhanced_apocalypsion_accepts_native_modifiers_after_prior_clear() {
        let (s,u)=fixture().await;
        let a=account(&u);
        put(&mut s.db.acquire().await.unwrap(),a,"punishment_open",101001,
            &json!({"GroupIndex":101001,"DungeonType":2,"IsOpen":0,"ClearLevel":1,"ClearCount":1,"Day":day()})).await.unwrap();
        assert_eq!(open(&s,&u,101001,2).await["Result"],"Success");
        let domains="&AffixIndices=5001&AffixIndices=5002&AffixIndices=5003&AffixIndices=5004";
        let modifiers="&AffixIndices=10001&AffixIndices=10004";
        let boss=entry(101001,5,2,&format!("{domains}{modifiers}"));
        for invalid in [
            entry(101001,5,2,domains),
            entry(101001,5,2,modifiers),
            format!("{boss}&AffixIndices=10001"),
            format!("{boss}&AffixIndices=10005"),
            format!("{boss}&AffixIndices=10024"),
        ] {
            assert_ne!(call(&s,&u,"contents/begin_content",&invalid).await["Result"],"Success");
        }
        // The same enhancement modifiers apply to dragon nodes too.
        let dragon=entry(101001,1,2,modifiers);
        assert_eq!(call(&s,&u,"contents/begin_content",&dragon).await["Result"],"Success");
        assert_eq!(call(&s,&u,"contents/end_content",&end(101001,1,false,1)).await["Result"],"Success");
        let begun=call(&s,&u,"contents/begin_content",&boss).await;
        assert_eq!(begun["Result"],"Success","{begun}");
        assert_eq!(call(&s,&u,"contents/begin_content",&boss).await,begun);
        let won=call(&s,&u,"contents/end_content",&end(101001,5,true,1)).await;
        assert_eq!(won["Result"],"Success","{won}");
        assert_eq!(won["OpenPunishmentRaidInfos"][0]["ClearLevel"],2);
        assert_eq!(open(&s,&u,101001,3).await["Result"],"Success");
        let next=entry(101001,5,3,&format!("{domains}{modifiers}"));
        assert_eq!(call(&s,&u,"contents/begin_content",&next).await["Result"],"Success");
    }

    #[test]
    fn punishment_per_mille_rates_keep_fractional_and_whole_rewards() {
        assert_eq!(ratio_count(200, 199), 1);
        assert_eq!(ratio_count(200, 200), 0);
        assert_eq!(ratio_count(1000, 999), 1);
        assert_eq!(ratio_count(2160, 159), 3);
        assert_eq!(ratio_count(2160, 160), 2);
        assert_eq!(ratio_count(0, 0), 0);
    }

    #[tokio::test]
    async fn punishment_archive_curves_and_every_active_clear_combination() {
        let (s, _) = fixture().await;
        // Frozen build-1558 preview values; include array-tail extrapolation.
        for (index, level, expected) in [
            (10100, 1, 4),
            (10100, 5, 20),
            (10100, 10, 48),
            (10100, 100, 2100),
            (30001, 100, 1411),
            (40001, 5, 23),
            (40002, 5, 51),
            (50000, 1, 200),
            (50000, 50, 1000),
        ] {
            assert_eq!(scaled(&s, index, level).unwrap(), expected);
        }
        assert!(scaled(&s, 10100, 0).is_err());
        for row in s
            .tables
            .battle
            .rows("PunishmentRaidReward")
            .iter()
            .filter(|r| r["Active"] == true || n(r, "RewardType") == 2)
        {
            let checks = row["ClearCheckIndex"]
                .as_array()
                .unwrap()
                .iter()
                .filter_map(Value::as_i64)
                .collect();
            let selected =
                reward_definition(&s, n(row, "RaidIndex"), &checks, row["Active"] == true).unwrap();
            assert_eq!(selected["Index"], row["Index"]);
        }
        assert!(reward_definition(
            &s,
            1005,
            &[1105, 1101, 1102, 1103].into_iter().collect(),
            true
        )
        .is_err());
        assert!(reward_definition(&s, 1008, &[2003, 1101].into_iter().collect(), true).is_err());
    }

    #[tokio::test]
    async fn punishment_native_payouts_give_matching_blood_and_fragments_once() {
        let (s, u) = fixture().await;
        let a = account(&u);
        let before_box = count(&s, a, 91029).await;
        let before_old_fragment = count(&s, a, 45121).await;
        assert_eq!(open(&s, &u, 101001, 1).await["Result"], "Success");
        for (dungeon, extra) in [
            (1, ""),
            (2, ""),
            (5, "&AffixIndices=5003&AffixIndices=5004"),
        ] {
            let begin = entry(101001, dungeon, 1, extra);
            let finish = end(101001, dungeon, true, 1);
            assert_eq!(
                call(&s, &u, "contents/begin_content", &begin).await["Result"],
                "Success"
            );
            let reward = call(&s, &u, "contents/end_content", &finish).await;
            assert_eq!(reward["Result"], "Success", "{reward}");
            let before = count(&s, a, 45128).await;
            assert_ne!(
                call(&s, &u, "contents/end_content", &finish).await["Result"],
                "Success"
            );
            assert_eq!(count(&s, a, 45128).await, before);
        }
        assert_eq!(count(&s, a, 45128).await, 6);
        assert_eq!(count(&s, a, 45122).await, 60);
        assert_eq!(count(&s, a, 45129).await, 0);
        assert_eq!(count(&s, a, 91029).await, before_box);
        assert_eq!(count(&s, a, 45121).await, before_old_fragment);
        assert_eq!(open(&s, &u, 101002, 1).await["Result"], "Success");
        assert_eq!(
            call(&s, &u, "contents/begin_content", &entry(101002, 3, 1, "")).await["Result"],
            "Success"
        );
        assert_eq!(
            call(&s, &u, "contents/end_content", &end(101002, 3, true, 1)).await["Result"],
            "Success"
        );
        assert_eq!(count(&s, a, 45129).await, 4);
        assert_eq!(count(&s, a, 45131).await, 42);
        assert_eq!(count(&s, a, 45128).await, 6);
        assert_eq!(count(&s, a, 45122).await, 60);
    }

    #[tokio::test]
    async fn punishment_scaled_native_level_shards_failure_retry_and_atomic_reward_rollback() {
        let (mut s, u) = fixture().await;
        let a = account(&u);
        assert_ne!(open(&s, &u, 101002, 6).await["Result"], "Success");
        assert_eq!(open(&s, &u, 101002, 5).await["Result"], "Success");
        sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES (?,40251,4)")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        let begin = entry(101002, 3, 5, "&FlaskItemIndex=40251&FlaskItemCount=3&AffixIndices=10024&AffixIndices=10026&AffixIndices=10027&AffixIndices=1937002");
        for invalid in [
            begin.replace("40251", "40241"),
            begin.replace("Count=3", "Count=5"),
            begin.replace("Level=5", "Level=4"),
            entry(101002, 1, 5, "&FlaskItemIndex=40251&FlaskItemCount=1&AffixIndices=10024&AffixIndices=10026&AffixIndices=10027&AffixIndices=1937002"),
        ] {
            assert_ne!(
                call(&s, &u, "contents/begin_content", &invalid).await["Result"],
                "Success"
            );
        }
        let result = call(&s, &u, "contents/begin_content", &begin).await;
        assert_eq!(result["Result"], "Success", "{result}");
        assert_eq!(call(&s, &u, "contents/begin_content", &begin).await, result);
        assert_ne!(
            call(
                &s,
                &u,
                "contents/begin_content",
                &begin.replace("Count=3", "Count=2")
            )
            .await["Result"],
            "Success"
        );
        assert!(
            item::consume(&mut s.db.acquire().await.unwrap(), a, 40251, 2)
                .await
                .is_err(),
            "Selected shards cannot be spent elsewhere"
        );
        assert_eq!(
            call(&s, &u, "contents/end_content", &end(101002, 3, false, 31)).await["Result"],
            "Success"
        );
        assert_eq!(count(&s, a, 40251).await, 4);
        assert_eq!(count(&s, a, 45129).await, 0);
        assert_eq!(
            call(&s, &u, "contents/begin_content", &begin).await["Result"],
            "Success"
        );
        let inventory =
            std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).inventory);
        inventory.items.get_mut(&45129).unwrap()["ItemMaxCap"] = json!(1);
        let finish = end(101002, 3, true, 31);
        assert_ne!(
            call(&s, &u, "contents/end_content", &finish).await["Result"],
            "Success"
        );
        assert_eq!(count(&s, a, 45129).await, 0);
        assert_eq!(count(&s, a, 40251).await, 4);
        let active: i64 = sqlx::query_scalar("SELECT completed FROM battle_runs WHERE account=?")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
        assert_eq!(active, 0);
        std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).inventory)
            .items
            .get_mut(&45129)
            .unwrap()["ItemMaxCap"] = json!(0);
        let win = call(&s, &u, "contents/end_content", &finish).await;
        assert_eq!(win["Result"], "Success", "{win}");
        assert_eq!(count(&s, a, 45129).await, 23);
        assert_eq!(count(&s, a, 45131).await, 51);
        assert_eq!(
            count(&s, a, 40251).await,
            2,
            "31 seconds starts two 30-second shards"
        );
        assert_ne!(
            call(&s, &u, "contents/end_content", &finish).await["Result"],
            "Success"
        );
        assert_eq!(count(&s, a, 40251).await, 2);
        assert_eq!(
            open(&s, &u, 101002, 10).await["Result"],
            "Success",
            "Highest clear plus five unlocks the next levels"
        );
    }
}
