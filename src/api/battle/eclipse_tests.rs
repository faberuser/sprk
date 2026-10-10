use super::tests::{account, call, setup};
use super::*;

async fn deck(s: &AppState, u: &Value, heroes: Value) -> Value {
    let args = serde_urlencoded::to_string([("HeroInfos", heroes.to_string())]).unwrap();
    call(s, u, "eclipse/set_eclipse_deck", &args).await
}
const ENTER:&str="ChapterIndex=8201&DungeonIndex=1&DungeonDifficulty=0&EnterTicketCount=2&OnlineGameSpeedRatio=1";
const END: &str = "ChapterIndex=8201&DungeonIndex=1&DungeonDifficulty=0&Completed=false&Star=0";
#[tokio::test]
async fn eclipse_snapshots_include_only_the_equipped_soul_weapon() {
    let (s, u) = setup().await;
    super::tests::unlock_godking(&s, &u).await;
    let a = account(&u);
    sqlx::query("INSERT INTO equip_items(account_id,slot_index,item_index,created_time) VALUES(?,90001,1001,'2026-10-03')")
        .bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE heroes SET equip_item_slot_index_1=90001 WHERE account_id=? AND hero_index=1")
        .bind(a).execute(&s.db).await.unwrap();
    let plain = deck(&s, &u, json!([{"DeckIndex":1,"HeroIndices":"1"}])).await;
    assert!(plain["DeckResults"][0]["CachedHeroInfos"][0]["EquipItemInfo1"]["SoulWeaponInfo"].is_null());
    let soul = json!({"EquipItemSlotIndex":90001,"ItemIndex":1001,"Grade":2,"ReinforceLevel":20,"OptionRatio1":500,"OptionRatio2":500});
    crate::api::extensions::put(&mut s.db.acquire().await.unwrap(), a, "soul", 90001, &soul)
        .await
        .unwrap();
    let saved = deck(&s, &u, json!([{"DeckIndex":1,"HeroIndices":"1"}])).await;
    assert_eq!(saved["DeckResults"][0]["CachedHeroInfos"][0]["EquipItemInfo1"]["SoulWeaponInfo"], soul);
    let begin = call(&s, &u, "campaign/begin_campaign", ENTER).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    assert_eq!(begin["EclipseBattleInfo"]["DeckList"][0]["HeroInfos"][0]["EquipItemInfo1"]["SoulWeaponInfo"], soul);
    assert!(begin["EclipseBattleInfo"]["DeckList"][0]["HeroInfos"][0]["EquipItemInfo2"]["SoulWeaponInfo"].is_null());
}

fn report(id: i64, deck: i64, status: &str, wave: i64) -> String {
    format!("MatchIndex={id}&CurrentDeckIndex={deck}&LastStatus={status}&MaxWaveIndex={wave}&CurrentChapterIndex=8201&CurrentDungeonIndex=1&CurrentWaveIndex=1&CurrentCreatureLevel=0&PlayTime=1&TotalDamage=100")
}
async fn stamina(s: &AppState, u: &Value) -> i64 {
    n(
        &call(s, u, "eclipse/get_eclipse_info", "").await["EclipseInfo"]["EclipseStaminaResult"],
        "NewValue",
    )
}

#[tokio::test]
async fn eclipse_native_entry_result_rewards_and_retry_are_persistent() {
    let (s, u) = setup().await;
    super::tests::unlock_godking(&s, &u).await;
    assert_eq!(
        deck(&s, &u, json!([{"DeckIndex":"1","HeroIndices":"1"}])).await["Result"],
        "Success"
    );
    let info = call(&s, &u, "eclipse/get_eclipse_info", "").await;
    assert_eq!(info["EclipseInfo"]["ChapterIndex"], 8201);
    let before = stamina(&s, &u).await;
    let begin = call(&s, &u, "campaign/begin_campaign", ENTER).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    assert_eq!(
        begin["EclipseBattleInfo"]["DeckList"][0]["HeroInfos"][0]["HeroIndex"],
        1
    );
    assert_eq!(
        begin["EclipseBattleInfo"]["DungeonList"]
            .as_array()
            .unwrap()
            .len(),
        18
    );
    assert_eq!(begin["StaminaResult"]["AddValue"], -2);
    assert_eq!(stamina(&s, &u).await, before - 2);
    assert_eq!(call(&s, &u, "campaign/begin_campaign", ENTER).await, begin);
    assert_eq!(stamina(&s, &u).await, before - 2);
    assert_ne!(deck(&s, &u, json!([])).await["Result"], "Success");
    assert_ne!(
        call(&s, &u, "campaign/end_campaign", END).await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "campaign/begin_campaign",
            "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]"
        )
        .await["Result"],
        "Success"
    );
    let id = n(&begin["EclipseBattleInfo"], "MatchIndex");
    let result = report(id, 1, "BattleEnd", 3);
    assert_eq!(
        call(&s, &u, "eclipse/save_eclipse_result", &result).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "eclipse/save_eclipse_result", &result).await["Result"],
        "Success"
    );
    let end = call(&s, &u, "campaign/end_campaign", END).await;
    assert_eq!(end["Result"], "Success", "{end}");
    assert_eq!(end["EclipseDungeonInfo"]["MaxWaveIndex"], 2);
    assert!(
        end["ItemResults"].as_array().is_some_and(|v| !v.is_empty())
            || end["CurrencyResults"]
                .as_array()
                .is_some_and(|v| !v.is_empty()),
        "{end}"
    );
    assert_eq!(call(&s, &u, "campaign/end_campaign", END).await, end);
    assert_ne!(
        call(
            &s,
            &u,
            "eclipse/give_up_eclipse_dungeon",
            &format!("MatchIndex={id}")
        )
        .await["Result"],
        "Success"
    );
    let claims: i64 = sqlx::query_scalar(
        "SELECT COUNT(*) FROM battle_reward_claims WHERE account=? AND kind='eclipse'",
    )
    .bind(account(&u))
    .fetch_one(&s.db)
    .await
    .unwrap();
    assert_eq!(claims, 1);
    let restored = call(&s, &u, "eclipse/get_eclipse_info", "").await;
    assert_eq!(restored["EclipseInfo"]["IsPlayEclipse"], false);
    assert_eq!(restored["EclipseInfo"]["MaxWaveIndex"], 2);
    assert_eq!(
        restored["EclipseInfo"]["DeckResults"][0]["ClearMaxWaveIndex"],
        2
    );
    let second = call(&s, &u, "campaign/begin_campaign", ENTER).await;
    assert_eq!(second["Result"], "Success", "{second}");
    assert_ne!(second["EclipseBattleInfo"]["MatchIndex"], id);
    assert_ne!(
        call(&s, &u, "eclipse/save_eclipse_result", &result).await["Result"],
        "Success"
    );
}

#[tokio::test]
async fn eclipse_team_switch_does_not_charge_again_and_online_entry_stays_online() {
    let (s, u) = setup().await;
    super::tests::unlock_godking(&s, &u).await;
    sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,2,2,1,1)")
        .bind(account(&u))
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        deck(
            &s,
            &u,
            json!([{"DeckIndex":"1","HeroIndices":"1"},{"DeckIndex":"2","HeroIndices":"2"}])
        )
        .await["Result"],
        "Success"
    );
    let before = stamina(&s, &u).await;
    let online = format!(
        "{ENTER}&MultiplayMasterId={}&MultiplayMemberIds=[{}]",
        account(&u),
        account(&u)
    );
    assert_ne!(
        call(&s, &u, "campaign/begin_campaign", &online).await["Result"],
        "Success"
    );
    assert_eq!(stamina(&s, &u).await, before);
    let begin = call(&s, &u, "campaign/begin_campaign", ENTER).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    let id = n(&begin["EclipseBattleInfo"], "MatchIndex");
    assert_ne!(
        call(
            &s,
            &u,
            "eclipse/save_eclipse_result",
            &report(id, 1, "BattleEnd", 3)
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "eclipse/save_eclipse_result",
            &report(id, 2, "BattleTeamEndOff", 3)
        )
        .await["Result"],
        "Success"
    );
    let end = call(&s, &u, "campaign/end_campaign", END).await;
    assert_eq!(end["Result"], "Success", "{end}");
    assert_eq!(end["EclipseDungeonInfo"]["DeckIndex"], 2);
    assert!(end["ItemResults"].as_array().unwrap().is_empty());
    let next = call(&s, &u, "campaign/begin_campaign", &ENTER.replace("EnterTicketCount=2", "EnterTicketCount=0")).await;
    assert_eq!(next["Result"], "Success", "{next}");
    assert_eq!(next["EclipseBattleInfo"]["CurrentDeckIndex"], 2);
    assert_eq!(next["StaminaResult"]["AddValue"], 0);
    assert_eq!(stamina(&s, &u).await, before - 2);
    assert_eq!(
        call(
            &s,
            &u,
            "eclipse/save_eclipse_result",
            &report(id, 2, "BattleEnd", 2)
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "campaign/end_campaign", END).await["Result"],
        "Success"
    );
}

#[tokio::test]
async fn eclipse_rejects_forged_progress_and_can_abandon_expired_sessions() {
    let (s, u) = setup().await;
    super::tests::unlock_godking(&s, &u).await;
    deck(&s, &u, json!([{"DeckIndex":"1","HeroIndices":"1"}])).await;
    let begin = call(&s, &u, "campaign/begin_campaign", ENTER).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    let id = n(&begin["EclipseBattleInfo"], "MatchIndex");
    for invalid in [
        report(id, 2, "BattleEnd", 2),
        report(id, 1, "BattleContinue", 401),
        format!("{}&AccountId=999999", report(id, 1, "BattleEnd", 2)),
        report(id, 1, "BattleEnd", 2).replace("CurrentChapterIndex=8201", "CurrentChapterIndex=1"),
    ] {
        assert_ne!(
            call(&s, &u, "eclipse/save_eclipse_result", &invalid).await["Result"],
            "Success",
            "{invalid}"
        );
    }
    let mut run = get(
        &mut s.db.acquire().await.unwrap(),
        account(&u),
        "eclipse",
        0,
    )
    .await
    .unwrap();
    run["Expires"] = json!(now() - 1);
    put(
        &mut s.db.acquire().await.unwrap(),
        account(&u),
        "eclipse",
        0,
        &run,
    )
    .await
    .unwrap();
    assert_ne!(
        call(
            &s,
            &u,
            "eclipse/save_eclipse_result",
            &report(id, 1, "BattleEnd", 2)
        )
        .await["Result"],
        "Success"
    );
    let abandoned = call(
        &s,
        &u,
        "eclipse/give_up_eclipse_dungeon",
        &format!("MatchIndex={id}"),
    )
    .await;
    assert_eq!(abandoned["Result"], "Success", "{abandoned}");
    assert_eq!(abandoned["EclipseDungeonInfo"]["Status"], "BattleGiveUp");
    assert_eq!(
        call(&s, &u, "eclipse/get_eclipse_info", "").await["EclipseInfo"]["IsPlayEclipse"],
        false
    );
}

#[tokio::test]
async fn eclipse_accepts_native_double_encoded_deck_json() {
    let (s,u)=setup().await;
    let value=json!([{"DeckIndex":"1","HeroIndices":"1","CachedHeroInfos":[],"ClearMaxWaveIndex":"0"}]).to_string();
    let encoded=urlencoding::encode(&value);
    let args=serde_urlencoded::to_string([("HeroInfos",encoded.as_ref())]).unwrap();
    let saved=call(&s,&u,"eclipse/set_eclipse_deck",&args).await;
    assert_eq!(saved["Result"],"Success","{saved}");
    assert_eq!(saved["DeckResults"][0]["HeroIndices"],"1");
    let bad=json!([{"DeckIndex":"1","HeroIndices":"1,1"}]).to_string();
    let args=serde_urlencoded::to_string([("HeroInfos",urlencoding::encode(&bad).as_ref())]).unwrap();
    assert_ne!(call(&s,&u,"eclipse/set_eclipse_deck",&args).await["Result"],"Success");
    assert_eq!(call(&s,&u,"eclipse/get_eclipse_deck","").await["DeckResults"][0]["HeroIndices"],"1");
}

#[tokio::test]
async fn eclipse_damage_accepts_native_long_without_allowing_negative_or_regression() {
    let (s,u)=setup().await;
    super::tests::unlock_godking(&s,&u).await;
    assert_eq!(deck(&s,&u,json!([{"DeckIndex":1,"HeroIndices":"1"}])).await["Result"],"Success");
    let begin=call(&s,&u,"campaign/begin_campaign",ENTER).await;
    assert_eq!(begin["Result"],"Success");
    let id=n(&begin["EclipseBattleInfo"],"MatchIndex");
    let high=report(id,1,"BattleContinue",1).replace("TotalDamage=100","TotalDamage=3000000000");
    assert_eq!(call(&s,&u,"eclipse/save_eclipse_result",&high).await["Result"],"Success");
    assert_eq!(call(&s,&u,"eclipse/save_eclipse_result",&high).await["Result"],"Success");
    for bad in ["-1","2147483647","9223372036854775808"] {
        assert_ne!(call(&s,&u,"eclipse/save_eclipse_result",&high.replace("3000000000",bad)).await["Result"],"Success");
    }
    let end=report(id,1,"BattleEnd",1).replace("TotalDamage=100","TotalDamage=3000000001");
    assert_eq!(call(&s,&u,"eclipse/save_eclipse_result",&end).await["Result"],"Success");
    assert_eq!(call(&s,&u,"campaign/end_campaign",END).await["Result"],"Success");
}
