//! Per-player Acquire Loot x2: reserve one stage's ruby price, settle once.
use super::*;

pub(super) async fn prepare(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &mut Value,
    out: &mut Value,
    receives_loot: bool,
) -> Result<()> {
    let count = r.number("ItemBoosterCount", 0)?;
    if !(0..=i32::MAX as i64).contains(&count) {
        return Err(rule("InvalidValue"));
    }
    if count == 0 || !receives_loot {
        return Ok(());
    }
    let (id, booster) = s
        .tables
        .inventory
        .boosters
        .iter()
        .find(|(_, b)| b["IsOnetime"] == true && n(b, "Type") == 2)
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    let dungeon = campaign::dungeon(s, r)?;
    if boolean(r, "ScenarioDungeon", false)?
        || !booster["BattleTypes"]
            .as_array()
            .is_some_and(|types| types.contains(&dungeon["BattleType"]))
    {
        return Ok(());
    }
    if let Some(floor) = dungeons::floor(s, r) {
        let progress = dungeons::tower(db, s, a, n(floor, "TowerIndex")).await?;
        if n(&progress, "CompletedFloor") < n(floor, "Floor") {
            return Ok(());
        }
    }
    // Older InventorySupport exports omit BuyGem. The installed ItemTable's
    // BOOSTER_ONETIME_ITEM price is 20 rubies (also stated in its description).
    let price = item::data(s, *id)?["BuyGem"].as_i64().unwrap_or(20);
    if price <= 0 || n(booster, "Value") != 100 {
        return Err(rule("InvalidCost"));
    }
    let balance: i64 = sqlx::query_scalar("SELECT gem+pay_gem FROM user_info WHERE account_id=?")
        .bind(a)
        .fetch_one(&mut *db)
        .await?;
    if balance - super::entry_costs::held(db, a, "Currency", 4).await? < price {
        return Err(rule("NotEnoughGem"));
    }
    let owner = uuid::Uuid::new_v4().to_string();
    sqlx::query("INSERT INTO battle_entry_holds(account,owner,family,kind,amount,expires) VALUES(?,?,'Currency',4,?,?)")
        .bind(a).bind(&owner).bind(price).bind(now()+settings(s,"BattleExpirySeconds",14400))
        .execute(&mut *db).await?;
    let previous: Option<String> =
        sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=? AND completed=1")
            .bind(a)
            .fetch_optional(&mut *db)
            .await?;
    let previous = previous
        .as_deref()
        .map(read_json::<Value>)
        .transpose()?
        .unwrap_or(Value::Null);
    let max = if n(&previous["ItemBoosterResult"], "RemainCount") == count {
        n(&previous["ItemBoosterResult"], "MaxCount").max(count)
    } else {
        count
    };
    entry["LootBooster"] = json!({"Count":count,"MaxCount":max,"Price":price,"Owner":owner});
    out["ItemBoosterResult"] = json!({"RemainCount":count,"MaxCount":max});
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::battle::tests::{account, call, setup};
    const ENTRY: &str = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
    const END: &str = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&Completed=true&Star=3&AliveHeroIndices=[1]";

    async fn fixture() -> (AppState, Value) {
        let (mut s, u) = setup().await;
        let dungeon = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle)
            .tables
            .get_mut("CampaignDungeon")
            .unwrap()
            .iter_mut()
            .find(|v| n(v, "ChapterIndex") == 1 && n(v, "DungeonIndex") == 1)
            .unwrap();
        let drop = n(dungeon, "DropRewardIndex_Normal") as i32;
        dungeon["FirstClearRewardIndex_Normal"] = json!(0);
        dungeon["FirstRewardIndex"] = json!(0);
        dungeon["FirstClearGem_Normal"] = json!(137);
        let reward = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).rewards)
            .entries
            .iter_mut()
            .find(|v| v.index == drop)
            .unwrap();
        // Guaranteed fragment x2 and armor x1: no random drop-rate comparison.
        *reward = crate::tables::RewardData {
            index: drop,
            gold_rate: 1000,
            gold_min: 100,
            gold_max: 100,
            sub_data_list: vec![
                json!([1, [1, 0], 1000, 2, null, 2, 0, 0, 0]),
                json!([2, [66, 0], 1000, 1, null, 1, 0, 0, 0]),
            ],
            ..Default::default()
        };
        assert_eq!(s.tables.reward_string_pool.get(1), Some("FRAGMENT_1"));
        assert_eq!(
            s.tables.reward_string_pool.get(66),
            Some("ARMOR_HEAVY_8_6_4")
        );
        (s, u)
    }
    async fn gems(s: &AppState, a: i64) -> i64 {
        sqlx::query_scalar("SELECT gem+pay_gem FROM user_info WHERE account_id=?")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap()
    }
    fn fragments(out: &Value) -> i64 {
        out["ItemResults"]
            .as_array()
            .unwrap()
            .iter()
            .filter(|v| n(v, "ItemIndex") == 20001)
            .map(|v| n(v, "AddCount"))
            .sum()
    }

    #[tokio::test]
    async fn loot_booster_doubles_drops_charges_once_and_decrements_repeat_count() {
        let (s, u) = fixture().await;
        let a = account(&u);
        let before = gems(&s, a).await;
        let begin = call(
            &s,
            &u,
            "campaign/begin_campaign",
            &format!("{ENTRY}&ItemBoosterCount=3"),
        )
        .await;
        assert_eq!(begin["Result"], "Success", "{begin}");
        assert_eq!(
            begin["ItemBoosterResult"],
            json!({"RemainCount":3,"MaxCount":3})
        );
        assert_eq!(gems(&s, a).await, before, "Reserve only this stage's cost");
        assert_eq!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=3")
            )
            .await,
            begin
        );
        let out = call(
            &s,
            &u,
            "campaign/end_campaign",
            &format!("{END}&ItemBoosterCount=0"),
        )
        .await;
        assert_eq!(out["Result"], "Success", "{out}");
        assert_eq!(fragments(&out), 4);
        assert_eq!(out["ItemResults"][0]["AddBoosterCount"], 2);
        assert_eq!(out["EquipItemInfos"].as_array().unwrap().len(), 2);
        assert_eq!(out["EquipItemBoostInfos"].as_array().unwrap().len(), 1);
        assert_eq!(
            out["ItemBoosterResult"],
            json!({"RemainCount":2,"MaxCount":3})
        );
        assert_eq!(
            gems(&s, a).await,
            before + 137 - 20,
            "First-clear rubies stay single; cost is 20, not 3*20"
        );
        let gold: i64 = out["CurrencyResults"]
            .as_array()
            .unwrap()
            .iter()
            .filter(|v| v["CurrencyType"] == "Gold")
            .map(|v| n(v, "AddValue"))
            .sum();
        let after = gems(&s, a).await;
        let _ = call(&s, &u, "campaign/end_campaign", END).await;
        assert_eq!(gems(&s, a).await, after, "Retry cannot charge again");
        assert_eq!(
            call(&s, &u, "campaign/begin_campaign", ENTRY).await["Result"],
            "Success"
        );
        let plain = call(&s, &u, "campaign/end_campaign", END).await;
        assert_eq!(fragments(&plain), 2);
        assert_eq!(plain["EquipItemInfos"].as_array().unwrap().len(), 1);
        assert_eq!(plain["EquipItemBoostInfos"], json!([]));
        assert_eq!(gems(&s, a).await, after);
        assert_eq!(
            plain["CurrencyResults"]
                .as_array()
                .unwrap()
                .iter()
                .filter(|v| v["CurrencyType"] == "Gold")
                .map(|v| n(v, "AddValue"))
                .sum::<i64>(),
            gold,
            "Loot booster does not double gold"
        );
        // Consecutive boosted repeats retain the initial maximum.
        assert_eq!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=3")
            )
            .await["Result"],
            "Success"
        );
        assert_eq!(
            call(&s, &u, "campaign/end_campaign", END).await["ItemBoosterResult"]["RemainCount"],
            2
        );
        assert_eq!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=2")
            )
            .await["Result"],
            "Success"
        );
        assert_eq!(
            call(&s, &u, "campaign/end_campaign", END).await["ItemBoosterResult"],
            json!({"RemainCount":1,"MaxCount":3})
        );
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_entry_holds")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            0
        );
    }

    #[tokio::test]
    async fn loot_booster_reserves_paid_rubies_releases_failures_and_rejects_insufficient_funds() {
        let (s, u) = fixture().await;
        let a = account(&u);
        sqlx::query("UPDATE user_info SET gem=5,pay_gem=15 WHERE account_id=?")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        assert_eq!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=1")
            )
            .await["Result"],
            "Success"
        );
        let mut db = s.db.begin().await.unwrap();
        assert!(
            item::money(&mut db, a, "Gem", -1).await.is_err(),
            "Reserved rubies cannot be spent elsewhere"
        );
        db.rollback().await.unwrap();
        let failed = call(
            &s,
            &u,
            "campaign/end_campaign",
            &format!("{ENTRY}&Completed=false&Star=0"),
        )
        .await;
        assert_eq!(failed["Result"], "Success", "{failed}");
        assert_eq!(failed["ItemBoosterResult"]["RemainCount"], 1);
        assert_eq!(gems(&s, a).await, 20);
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_entry_holds")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            0
        );
        assert_eq!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=1")
            )
            .await["Result"],
            "Success"
        );
        let mut db = s.db.begin().await.unwrap();
        let raw: String = sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?")
            .bind(a)
            .fetch_one(&mut *db)
            .await
            .unwrap();
        let mut entry: Value = read_json(&raw).unwrap();
        super::super::entry_costs::settle(&mut db, &s, a, &mut entry, false, &mut json!({}))
            .await
            .unwrap();
        sqlx::query("UPDATE battle_runs SET completed=1,entry=? WHERE account=?")
            .bind(entry.to_string())
            .bind(a)
            .execute(&mut *db)
            .await
            .unwrap();
        db.commit().await.unwrap();
        assert_eq!(gems(&s, a).await, 20, "Abandonment costs nothing");
        let mut db = s.db.begin().await.unwrap();
        let mut progress = campaign::progress(&mut db, &s, a, 1, 1).await.unwrap();
        progress["FirstRewardedDiff"] = json!(2);
        progress["LootRewardedDiff"] = json!(2);
        put(&mut db, a, "dungeon", campaign::key(1, 1), &progress)
            .await
            .unwrap();
        db.commit().await.unwrap();
        assert_eq!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=1")
            )
            .await["Result"],
            "Success"
        );
        let win = call(&s, &u, "campaign/end_campaign", END).await;
        assert_eq!(win["Result"], "Success");
        assert_eq!(
            gems(&s, a).await,
            0,
            "The stage charge consumes 5 free and 15 paid rubies"
        );
        sqlx::query("UPDATE user_info SET gem=4,pay_gem=15 WHERE account_id=?")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        assert_ne!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=1")
            )
            .await["Result"],
            "Success"
        );
        assert_eq!(gems(&s, a).await, 19);
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_entry_holds")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            0
        );
        assert_ne!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &format!("{ENTRY}&ItemBoosterCount=-1")
            )
            .await["Result"],
            "Success"
        );
    }
}

pub(super) fn active(entry: &Value) -> bool {
    n(&entry["LootBooster"], "Count") > 0 && campaign::party_loot_enabled(entry)
}

pub(super) async fn settle(
    db: &mut SqliteConnection,
    a: i64,
    entry: &mut Value,
    won: bool,
    out: &mut Value,
) -> Result<()> {
    let booster = entry["LootBooster"].clone();
    let Some(owner) = booster["Owner"].as_str() else {
        return Ok(());
    };
    if booster["Settled"] == true {
        return Ok(());
    }
    super::entry_costs::clear_hold(db, a, owner).await?;
    if won {
        let result = item::money(db, a, "Gem", -n(&booster, "Price")).await?;
        for key in ["CurrencyResults2", "ReservedCurrencyResults"] {
            let mut values = out[key].as_array().cloned().unwrap_or_default();
            values.push(result.clone());
            out[key] = json!(values);
        }
    }
    let result =
        json!({"RemainCount":n(&booster,"Count")-i64::from(won),"MaxCount":n(&booster,"MaxCount")});
    out["ItemBoosterResult"] = result.clone();
    entry["ItemBoosterResult"] = result;
    entry["LootBooster"]["Settled"] = json!(true);
    Ok(())
}
