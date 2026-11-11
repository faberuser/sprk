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

    async fn use_item(s: &AppState, u: &Value, id: i32, potion: bool) -> Value {
        sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,?,1) ON CONFLICT(account_id,item_index) DO UPDATE SET count=count+1")
            .bind(account(u)).bind(id).execute(&s.db).await.unwrap();
        let body=Bytes::from(format!("SessionKey={}&ItemIndex={id}&ItemCount=1",u["UserInfo"]["SessionKey"].as_str().unwrap()));
        let response=if potion {item::use_potion_item(State(s.clone()),body).await} else {item::use_booster_item(State(s.clone()),body).await};
        let result=response.unwrap().0;
        assert_eq!(result["Result"],"Success","{result}");
        result
    }

    #[tokio::test]
    async fn timed_loot_booster_activates_extends_persists_and_expires_without_ruby_cost() {
        let (s,u)=fixture().await;let a=account(&u);
        let before=gems(&s,a).await;
        let used=use_item(&s,&u,110005,false).await;
        assert_eq!(used["ItemResult"]["AddCount"],-1);
        assert_eq!(used["ItemTimeDurationInfo"]["ItemIndex"],110005);
        use_item(&s,&u,110005,false).await;
        let duration:i64=sqlx::query_scalar("SELECT strftime('%s',end_time)-strftime('%s',start_time) FROM item_boosters WHERE account_id=? AND item_index=110005")
            .bind(a).fetch_one(&s.db).await.unwrap();
        assert_eq!(duration,7200);
        assert!(item::booster_login(&s,a).await.unwrap().iter().any(|v|v["ItemIndex"]==110005));
        assert_eq!(call(&s,&u,"campaign/begin_campaign",ENTRY).await["Result"],"Success");
        let out=call(&s,&u,"campaign/end_campaign",END).await;
        assert_eq!(out["Result"],"Success","{out}");
        assert_eq!(fragments(&out),4);
        assert_eq!(out["ItemResults"][0]["AddBoosterCount"],2);
        assert_eq!(out["EquipItemInfos"].as_array().unwrap().len(),2);
        assert_eq!(out["EquipItemBoostInfos"].as_array().unwrap().len(),1);
        assert_eq!(gems(&s,a).await,before+137,"The timed item never charges rubies");
        sqlx::query("UPDATE item_boosters SET end_time=datetime('now','-1 second') WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        assert_eq!(call(&s,&u,"campaign/begin_campaign",ENTRY).await["Result"],"Success");
        let plain=call(&s,&u,"campaign/end_campaign",END).await;
        assert_eq!(fragments(&plain),2);
        assert_eq!(plain["EquipItemInfos"].as_array().unwrap().len(),1);
        assert_eq!(plain["EquipItemBoostInfos"],json!([]));
        assert_eq!(gems(&s,a).await,before+137);
    }

    #[tokio::test]
    async fn timed_loot_booster_cookie_and_paid_loot_stack_additively() {
        let (s,u)=fixture().await;let a=account(&u);
        let before=gems(&s,a).await;
        use_item(&s,&u,110005,false).await;
        use_item(&s,&u,41110,true).await; // Strawberry Cookie Stick, separate +100% timer.
        assert_eq!(call(&s,&u,"campaign/begin_campaign",&format!("{ENTRY}&ItemBoosterCount=1")).await["Result"],"Success");
        let out=call(&s,&u,"campaign/end_campaign",END).await;
        assert_eq!(out["Result"],"Success","{out}");
        assert_eq!(fragments(&out),8,"Base +100% scroll +100% cookie +100% paid option");
        assert_eq!(out["ItemResults"][0]["AddBoosterCount"],6);
        assert_eq!(out["EquipItemInfos"].as_array().unwrap().len(),4);
        assert_eq!(out["EquipItemBoostInfos"].as_array().unwrap().len(),3);
        assert_eq!(gems(&s,a).await,before+137-20);
        assert_eq!(out["CurrencyResults"].as_array().unwrap().iter()
            .filter(|v|v["CurrencyType"]=="Gold").map(|v|n(v,"AddValue")).sum::<i64>(),100);
    }

    #[tokio::test]
    async fn timed_loot_booster_keeps_first_clear_attachments_single() {
        let (mut s,u)=fixture().await;
        let dungeon=std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle)
            .tables.get_mut("CampaignDungeon").unwrap().iter_mut()
            .find(|v|n(v,"ChapterIndex")==1 && n(v,"DungeonIndex")==1).unwrap();
        dungeon["FirstClearRewardIndex_Normal"]=dungeon["DropRewardIndex_Normal"].clone();
        use_item(&s,&u,110005,false).await;
        assert_eq!(call(&s,&u,"campaign/begin_campaign",ENTRY).await["Result"],"Success");
        let out=call(&s,&u,"campaign/end_campaign",END).await;
        assert_eq!(out["Result"],"Success","{out}");
        assert_eq!(fragments(&out),6,"Four normal boosted fragments plus two first-clear fragments");
        assert_eq!(out["EquipItemInfos"].as_array().unwrap().len(),3);
        assert_eq!(out["ItemResults"].as_array().unwrap().iter().map(|v|n(v,"AddBoosterCount")).sum::<i64>(),2);
    }

    #[tokio::test]
    async fn timed_loot_booster_applies_to_challenge_tower_repeats_and_sweep_rewards_only() {
        let (s,u)=fixture().await;let a=account(&u);
        use_item(&s,&u,110005,false).await;
        let floor=s.tables.battle.rows("TowerFloor").iter().find(|f| {
            s.tables.battle.rows("CampaignDungeon").iter().any(|d|n(d,"ChapterIndex")==n(f,"ChapterIndex") && n(d,"DungeonIndex")==n(f,"DungeonIndex") && n(d,"BattleType")==14)
        }).unwrap();
        let request=Request::parse(format!("ChapterIndex={}&DungeonIndex={}&DungeonDifficulty=1",n(floor,"ChapterIndex"),n(floor,"DungeonIndex")).as_bytes()).unwrap();
        let mut db=s.db.acquire().await.unwrap();
        assert_eq!(super::percent(&mut db,&s,a,&request,&json!({})).await.unwrap(),0,"Tower first clears are excluded");
        let mut progress=dungeons::tower(&mut db,&s,a,n(floor,"TowerIndex")).await.unwrap();
        progress["CompletedFloor"]=floor["Floor"].clone();
        put(&mut db,a,"tower",n(floor,"TowerIndex"),&progress).await.unwrap();
        assert_eq!(super::percent(&mut db,&s,a,&request,&json!({})).await.unwrap(),100,"Tower repeat rewards use the timer");
        // Sweeps share the ordinary completion helper, while offline dispatch
        // uses its separate unboosted helper. Do not conflate those two modes.
        let request=Request::parse(ENTRY.as_bytes()).unwrap();
        let out=campaign::complete(&mut db,&s,a,&request,&[],3,1).await.unwrap();
        assert_eq!(fragments(&out),4);
    }

    #[tokio::test]
    async fn timed_loot_booster_obeys_archived_battle_types_and_free_party_exclusion() {
        let (mut s,u)=fixture().await;let a=account(&u);
        use_item(&s,&u,110005,false).await;
        use_item(&s,&u,110019,false).await;
        for battle in [1,10,14,22,28,37,39,12,13,33,34,35,36,40,41,42] {
            let row=std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle)
                .tables.get_mut("CampaignDungeon").unwrap().iter_mut()
                .find(|v|n(v,"ChapterIndex")==1 && n(v,"DungeonIndex")==1).unwrap();
            row["BattleType"]=json!(battle);
            let request=Request::parse(ENTRY.as_bytes()).unwrap();
            let value=super::percent(&mut s.db.acquire().await.unwrap(),&s,a,&request,&json!({})).await.unwrap();
            let expected=if [1,10,14,22].contains(&battle) {200} else if [28,37,39].contains(&battle) {100} else {0};
            assert_eq!(value,expected,"battle type {battle}");
            assert_eq!(super::percent(&mut s.db.acquire().await.unwrap(),&s,a,&request,&json!({"PartyCampaign":true,"PartyUseStamina":false})).await.unwrap(),0);
        }
    }

    #[tokio::test]
    async fn timed_loot_booster_does_not_apply_to_dispatch_start_claim_or_cancel() {
        for cancel in [false,true] {
            let (s,u)=fixture().await;let a=account(&u);
            put(&mut s.db.acquire().await.unwrap(),a,"dungeon",campaign::key(1,1),&json!({"FirstRewardedDiff":2,"LootRewardedDiff":2,"MaxStar":13})).await.unwrap();
            use_item(&s,&u,110005,false).await;
            let before=gems(&s,a).await;
            let start=call(&s,&u,"dispatch/start_dispatch","ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount=3&DeckIndex=1&ItemBoosterCount=3").await;
            assert_eq!(start["Result"],"Success","{start}");
            let slot=n(&start["DispatchBattleInfo"],"SlotIndex");
            crate::api::battle::tests::prepared_dispatch(&s,&u,slot,&[1,0,1]).await;
            let mut job=get(&mut s.db.acquire().await.unwrap(),a,"dispatch",slot).await.unwrap();
            job["FinishTimestamp"]=json!(now()-1);
            put(&mut s.db.acquire().await.unwrap(),a,"dispatch",slot,&job).await.unwrap();
            // A second eligible loot buff activated only at collection cannot boost it either.
            use_item(&s,&u,41110,true).await;
            let path=if cancel {"dispatch/cancel_dispatch"} else {"dispatch/request_complete_dispatch"};
            let done=call(&s,&u,path,&format!("SlotIndex={slot}")).await;
            assert_eq!(done["Result"],"Success","{done}");
            assert_eq!(done["DispatchBattleInfo"]["WinCount"],2);
            assert_eq!(fragments(&done),4,"Only the two successful unboosted drops");
            assert!(done["ItemResults"].as_array().unwrap().iter().all(|v|n(v,"AddBoosterCount")==0));
            assert_eq!(gems(&s,a).await,before,"Dispatch cannot select or charge the paid loot option");
            let count:i64=sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=20001").bind(a).fetch_one(&s.db).await.unwrap();
            let _=call(&s,&u,path,&format!("SlotIndex={slot}")).await;
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT count FROM items WHERE account_id=? AND item_index=20001").bind(a).fetch_one(&s.db).await.unwrap(),count,"Retry cannot duplicate loot");
        }
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

/// Online reward boosters add their percentages. Dispatch deliberately calls
/// campaign::complete_dispatch with zero instead: dispatch never inherits item boosters
/// from either dispatch start or collection time.
pub(super) async fn percent(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &Value,
) -> Result<i64> {
    if !campaign::party_loot_enabled(entry) || boolean(r, "ScenarioDungeon", false)? {
        return Ok(0);
    }
    if let Some(floor) = dungeons::floor(s, r) {
        let progress = dungeons::tower(db, s, a, n(floor,"TowerIndex")).await?;
        if n(&progress,"CompletedFloor") < n(floor,"Floor") {
            return Ok(0);
        }
    }
    let battle = campaign::dungeon(s, r)?["BattleType"].clone();
    item::normalize_booster_timers(db, s, a).await?;
    let timers: Vec<i32> = sqlx::query_scalar(
        "SELECT item_index FROM item_boosters WHERE account_id=? AND start_time<=datetime('now') AND end_time>datetime('now')",
    ).bind(a).fetch_all(db).await?;
    let mut percent = if active(entry) { 100_i64 } else { 0 };
    for id in timers {
        if let Some(b) = s.tables.inventory.booster_effect(id) {
            if n(b,"Type") == 2 && b["IsOnetime"] != true
                && b["BattleTypes"].as_array().is_some_and(|types| types.contains(&battle)) {
                percent = percent.checked_add(n(b,"Value").max(0)).ok_or_else(||rule("InvalidValue"))?;
            }
        }
    }
    Ok(percent)
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
