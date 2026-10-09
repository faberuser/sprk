use super::*;
use crate::api::account::user;
use crate::database;
use crate::tables::GameTables;
use std::{path::Path, sync::OnceLock};
pub(crate) async fn setup() -> AppState {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    AppState::new(
        db,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    )
}
pub(crate) async fn login(s: &AppState, id: &str) -> Value {
    let u = json!(
        user::test_login(State(s.clone()), Bytes::from(format!("LoginId={id}")))
            .await
            .unwrap()
            .0
    );
    sqlx::query("UPDATE user_info SET team_level=60,gold=100000000,gem=100000 WHERE account_id=?")
        .bind(n(&u["UserInfo"], "AccountId"))
        .execute(&s.db)
        .await
        .unwrap();
    u
}
pub(crate) async fn call(s: &AppState, u: &Value, path: &str, args: &str) -> Value {
    execute_request(
        s,
        path,
        Bytes::from(format!(
            "SessionKey={}&{args}",
            u["UserInfo"]["SessionKey"].as_str().unwrap()
        )),
    )
    .await
    .unwrap()
}
pub(crate) async fn create(s: &AppState, u: &Value, name: &str, way: i64) -> i64 {
    let v = call(
        s,
        u,
        "guild/create_guild",
        &format!("name={name}&logo=1&logoBackground=1&joinWay={way}&reqTeamLevel=1&countryCode=VESPA"),
    )
    .await;
    assert_eq!(v["Result"], "Success", "{v}");
    n(&v, "GuildId")
}
#[tokio::test]
async fn guild_applications_roles_and_master_transfer_validate_ownership() {
    let s = setup().await;
    let u = login(&s, "master").await;
    let v = login(&s, "member").await;
    let w = login(&s, "outsider").await;
    let g = create(&s, &u, "GuildOne", 2).await;
    let a = n(&v["UserInfo"], "AccountId");
    assert_eq!(
        call(&s, &v, "guild/request_join_guild", &format!("GuildId={g}")).await["JoinWay"],
        2
    );
    assert_ne!(
        call(
            &s,
            &w,
            "guild/accept_join_request",
            &format!("AccountId={a}")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "guild/accept_join_request",
            &format!("AccountId={a}")
        )
        .await["Result"],
        "Success"
    );
    let members = call(
        &s,
        &u,
        "guild/get_all_guild_member_info",
        &format!("GuildId={g}"),
    )
    .await;
    assert_eq!(members["MemberInfos"][0]["Rank"], 1);
    assert_eq!(members["MemberInfos"][1]["Rank"], 3);
    assert_ne!(
        call(&s, &v, "guild/destroy_guild", "").await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "guild/delegate_master",
            &format!("DelegateAccountId={a}")
        )
        .await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "guild/kick_guildmember", &format!("AccountId={a}")).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "guild/withdraw_guild", "").await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &v, "guild/destroy_guild", "").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn guild_contribution_limits_and_attendance_are_persistent() {
    let s = setup().await;
    let u = login(&s, "donor").await;
    create(&s, &u, "Donators", 1).await;
    for _ in 0..5 {
        let v = call(&s, &u, "guild/contribute_guild", "").await;
        assert_eq!(v["Result"], "Success", "{v}");
    }
    assert_ne!(
        call(&s, &u, "guild/contribute_guild", "").await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "guild/set_guild_attendance", "").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "guild/set_guild_attendance", "").await["Result"],
        "Success"
    );
    let first = call(&s, &u, "guild/send_guild_attendance_reward", "").await;
    assert_eq!(first["Result"], "Success", "{first}");
    call(&s, &u, "guild/send_guild_attendance_reward", "").await;
    let count: i64 =
        sqlx::query_scalar("SELECT count(*) FROM mails WHERE title='Guild attendance reward'")
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let relog = login(&s, "donor").await;
    assert_eq!(relog["UserInfo"]["GuildPoint"], 600);
    assert_eq!(
        call(&s, &relog, "guild/get_guild_attendance", "").await["GuildAttendanceInfo"]["Daily"],
        1
    );
    assert_eq!(
        call(&s, &relog, "guild/destroy_guild", "").await["Result"],
        "Success"
    );
    create(&s, &relog, "NextGuild", 1).await;
    assert_ne!(
        call(&s, &relog, "guild/contribute_guild", "").await["Result"],
        "Success"
    );
    let body = Bytes::from(format!(
        "SessionKey={}&ChapterIndex=6002&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]",
        relog["UserInfo"]["SessionKey"].as_str().unwrap()
    ));
    let blocked = super::super::battle::execute_request(&s, "campaign/begin_campaign", body)
        .await
        .unwrap();
    assert_ne!(blocked["Result"], "Success");
}
#[tokio::test]
async fn arena_honor_rank_is_separate_from_victory_and_combat_stays_gated() {
    let s = setup().await;
    let u = login(&s, "honor-ranker").await;
    let a = n(&u["UserInfo"], "AccountId");
    sqlx::query("UPDATE arena_scores SET score=1600,wins=7 WHERE account=? AND kind=0")
        .bind(a).execute(&s.db).await.unwrap();
    let honor = call(&s, &u, "match/get_match_rank", "ArenaType=BanPick").await;
    assert_eq!(honor["Result"], "Success", "{honor}");
    assert!(honor["BattleInfo"].is_null());
    assert_eq!(honor["BattleBanPickInfo"]["MatchScore"], 1000);
    assert_eq!(honor["BattleBanPickInfo"]["SeasonWin"], 0);
    let tier = n(&honor["BattleBanPickInfo"], "TierIndex");
    assert!(s.tables.arena_guild.rows("GlobalBanPickTier").iter().any(|v| n(v, "Index") == tier));
    sqlx::query("UPDATE arena_scores SET score=1234,wins=2 WHERE account=? AND kind=1")
        .bind(a).execute(&s.db).await.unwrap();
    let ranks = call(&s, &u, "global_arena/get_world_ranker", "ArenaType=BanPick&StartRank=0&EndRank=0").await;
    assert_eq!(ranks["Rankers"][0]["MatchScore"], 1234, "{ranks}");
    let normal = call(&s, &u, "match/get_match_rank", "ArenaType=Normal").await;
    assert_eq!(normal["BattleInfo"]["MatchScore"], 1600);
    assert_eq!(normal["BattleInfo"]["SeasonWin"], 7);
    assert_ne!(call(&s, &u, "match/register_match", "ArenaType=BanPick&HeroIndices=1").await["Result"], "Success");
    let relogged = login(&s, "honor-ranker").await;
    let honor = call(&s, &relogged, "match/get_match_rank", "ArenaType=BanPick").await;
    assert_eq!(honor["BattleBanPickInfo"]["MatchScore"], 1234);
}
#[tokio::test]
async fn arena_offline_flow_rejects_unentered_and_replayed_results() {
    let s = setup().await;
    let u = login(&s, "arena-one").await;
    let _v = login(&s, "arena-two").await;
    let end = "Win=1&PlayTime=30&AliveHeroIndices=[1]";
    assert_ne!(
        call(&s, &u, "match/set_offline_match_result", end).await["Result"],
        "Success"
    );
    let entry = "ArenaType=Normal&HeroIndices=1&LeaderHeroIndex=1";
    let first = call(&s, &u, "match/register_match", entry).await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(call(&s, &u, "match/register_match", entry).await, first);
    let wait = call(&s, &u, "match/wait_match", "PlayOfflineMatch=true").await;
    assert_eq!(wait["Result"], "WaitMore", "{wait}");
    assert_eq!(wait["SwordResult"]["AddValue"], -1);
    // Replay creation dereferences the opponent's battle snapshot after combat.
    assert_eq!(wait["MatchedNpcInfo"]["BattleInfo"]["MatchScore"], 1000);
    assert!(wait["MatchedNpcInfo"]["TierIndex"].as_i64().unwrap() > 0);
    assert_eq!(wait["MatchedNpcInfo"]["TierIndex"], wait["MatchedNpcInfo"]["BattleInfo"]["TierIndex"]);
    assert_eq!(
        call(&s, &u, "match/wait_match", "PlayOfflineMatch=true").await,
        wait
    );
    assert_ne!(
        call(
            &s,
            &u,
            "match/set_offline_match_result",
            "Win=1&AliveHeroIndices=[2]"
        )
        .await["Result"],
        "Success"
    );
    let result = call(&s, &u, "match/set_offline_match_result", end).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(result["MatchResult"]["GainedMatchScore"], 20);
    assert_eq!(result["MatchResult"]["NewTotalRank"], 0);
    assert_eq!(result["MatchResult"]["NewTierRank"], 0);
    assert_eq!(result["BattleInfo"]["Rank"], 0);
    let snapshot: String = sqlx::query_scalar("SELECT data FROM community_state WHERE owner=? AND kind='arena_daily'")
        .bind(n(&u["UserInfo"], "AccountId")).fetch_one(&s.db).await.unwrap();
    assert_eq!(serde_json::from_str::<Value>(&snapshot).unwrap()["Rank"], 1);
    assert_ne!(
        call(&s, &u, "match/set_offline_match_result", end).await["Result"],
        "Success"
    );
    let u = login(&s, "arena-one").await;
    assert_eq!(u["BattleInfo"]["MatchScore"], 1020);
}
#[tokio::test]
async fn arena_mirror_opponent_has_distinct_battle_identity() {
    let s = setup().await;
    let u = login(&s, "arena-only-account").await;
    let entry = call(&s, &u, "match/register_match", "HeroIndices=1&LeaderHeroIndex=1").await;
    assert_eq!(entry["Result"], "Success");
    let wait = call(&s, &u, "match/wait_match", "PlayOfflineMatch=true").await;
    assert_eq!(wait["MatchedNpcInfo"]["UserInfo"]["AccountId"], -n(&u["UserInfo"], "AccountId"));
    assert!(!wait["MatchedNpcInfo"]["HeroInfos"].as_object().unwrap().is_empty());
    assert_eq!(call(&s, &u, "match/wait_match", "PlayOfflineMatch=true").await, wait);
}
#[tokio::test]
async fn guild_raid_damage_advances_shared_boss_and_charges_once() {
    let s = setup().await;
    let u = login(&s, "raider").await;
    create(&s, &u, "Raiders", 1).await;
    let initial = call(&s,&u,"guild_raid/get_guild_raid_list","").await;
    let boss=initial["GuildRaidInfos"].as_array().unwrap().iter().find(|v|n(v,"ChapterIndex")==6002).unwrap();
    assert_eq!(boss["MonsterHp0"],53_372_402_134i64);
    let scores=call(&s,&u,"guild_raid/get_guild_raid_member_score_list","").await;
    assert_eq!(scores["GuildRaidMemberTotalScores"][0]["Score"],0);
    let invoke = |path: &str, args: &str| {
        let s = s.clone();
        let body = Bytes::from(format!(
            "SessionKey={}&{args}",
            u["UserInfo"]["SessionKey"].as_str().unwrap()
        ));
        let path = path.to_owned();
        async move {
            super::super::battle::execute_request(&s, &path, body)
                .await
                .unwrap()
        }
    };
    let entry = "ChapterIndex=6002&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
    let first = invoke("campaign/begin_campaign", entry).await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(first["StaminaResult"]["AddValue"], -1);
    assert_eq!(invoke("campaign/begin_campaign", entry).await, first);
    // Match Unity's zero TotalDamage, string-valued creature fields and extra
    // URL escaping. Invalid/duplicate boss records must not consume the entry.
    for creatures in [
        json!([{"Index":"900","Key":"900_0_1","TeamId":"1","Hp":"-1"}]),
        json!([{"Index":"900","Key":"900_0_1","TeamId":"1","Hp":"0"},{"Index":"900","Key":"900_0_1","TeamId":"1","Hp":"0"}]),
    ] {
        let encoded = serde_urlencoded::to_string([("CreatureInfoString", creatures.to_string())]).unwrap();
        let result = invoke("campaign/end_campaign", &format!("ChapterIndex=6002&DungeonIndex=1&DungeonDifficulty=1&Completed=true&TotalDamage=0&{encoded}")).await;
        assert_ne!(result["Result"], "Success", "{result}");
    }
    let creatures = json!([{"Index":"900","Key":"900_0_1","TeamId":"1","Hp":"0"}]).to_string();
    let encoded = serde_urlencoded::to_string([("CreatureInfoString", urlencoding::encode(&creatures).to_string())]).unwrap();
    let end = format!("ChapterIndex=6002&DungeonIndex=1&DungeonDifficulty=1&Completed=true&TotalDamage=0&{encoded}");
    let result = invoke("campaign/end_campaign", &end).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(result["GuildRaidMemberTotalScoreInfo"]["Score"],53_372_402_134i64);
    assert_ne!(
        invoke("campaign/end_campaign", &end).await["Result"],
        "Success"
    );
    let list = call(&s, &u, "guild_raid/get_guild_raid_list", "").await;
    let current = list["GuildRaidInfos"]
        .as_array()
        .unwrap()
        .iter()
        .find(|v| n(v, "ChapterIndex") == 6002)
        .unwrap();
    assert_eq!(current["DungeonIndex"], 2);
    assert_eq!(current["MonsterHp0"],104_803_241_581i64);
    let count: i64 =
        sqlx::query_scalar("SELECT count(*) FROM mails WHERE title='Guild raid boss defeated'")
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let a = n(&u["UserInfo"], "AccountId");
    let saved: String=sqlx::query_scalar("SELECT reward_currencies FROM mails WHERE account_id=? AND title='Guild raid boss defeated'").bind(a).fetch_one(&s.db).await.unwrap();
    assert!(saved.contains("GuildPoint"));
}

#[tokio::test]
async fn guild_raid_placeholder_health_migrates_once_and_dead_boss_stays_dead() {
    let s=setup().await;let u=login(&s,"hp-migrate").await;let g=create(&s,&u,"Migrate",1).await;
    call(&s,&u,"guild_raid/get_guild_raid_list","").await;
    sqlx::query("UPDATE community_state SET data=json_remove(json_set(data,'$.MonsterHp0',500000000),'$.HpSchema') WHERE owner=? AND kind='guild_raid' AND idx=6002").bind(g).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE community_state SET data=json_remove(json_set(data,'$.MonsterHp0',0,'$.IsOngoing',0),'$.HpSchema') WHERE owner=? AND kind='guild_raid' AND idx=6003").bind(g).execute(&s.db).await.unwrap();
    for _ in 0..2 {
        let v=call(&s,&u,"guild_raid/get_guild_raid_list","").await;
        let bosses=v["GuildRaidInfos"].as_array().unwrap();
        assert_eq!(bosses.iter().find(|v|n(v,"ChapterIndex")==6002).unwrap()["MonsterHp0"],26_686_201_067i64);
        assert_eq!(bosses.iter().find(|v|n(v,"ChapterIndex")==6003).unwrap()["MonsterHp0"],-1);
    }
}

#[tokio::test]
async fn guild_booty_can_be_bought_from_both_native_shops_and_persists() {
    let s=setup().await;let u=login(&s,"booty-buyer").await;let g=create(&s,&u,"Booty",1).await;
    let a=n(&u["UserInfo"],"AccountId");
    let id=s.tables.get_item_index("STAMINA_POTION_S").unwrap();
    {
        let mut db=s.db.acquire().await.unwrap();hero::currency(&mut db,a,"GuildPoint",10000).await.unwrap();
        for (key,equip) in [(1,false),(2,false)] {
            put(&mut db,g,"guild_booty",key,&json!({"Id":key,"ShopIndex":4,"ItemIndex":id,"ItemCount":3,"Count":3,"Equipment":equip,"Price":10,"Expires":now()+86400,"CreatedTime":time(now())})).await.unwrap();
        }
    }
    for (key,shop) in [(1,20),(2,4)] {
        let args=format!("Id={key}&ShopIndex={shop}&ItemIndex={id}&Count=3");
        let bad=call(&s,&u,"guild_raid/buy_guild_raid_booty_item",&format!("Id={key}&ShopIndex={shop}&ItemIndex={id}&Count=4")).await;
        assert_ne!(bad["Result"],"Success");
        let bought=call(&s,&u,"guild_raid/buy_guild_raid_booty_item",&args).await;
        assert_eq!(bought["Result"],"Success","{bought}");
        assert_eq!(bought["GuildPointResult"]["AddValue"],-30);
        assert_ne!(call(&s,&u,"guild_raid/buy_guild_raid_booty_item",&args).await["Result"],"Success");
    }
    assert_eq!(call(&s,&u,"guild_raid/get_guild_raid_all_booty_items","").await["ItemInfos"],json!([]));
    let qty:i64=sqlx::query_scalar("SELECT SUM(json_extract(data,'$.ItemCount')) FROM community_state WHERE owner=? AND kind='guild_booty'").bind(g).fetch_one(&s.db).await.unwrap();
    assert_eq!(qty,0);
}

#[tokio::test]
async fn guild_restored_shop_stock_matches_current_building_level_and_prices() {
    let s=setup().await;let u=login(&s,"guild-shop-buyer").await;create(&s,&u,"Shopper",1).await;
    let a=n(&u["UserInfo"],"AccountId");
    {let mut db=s.db.acquire().await.unwrap();for currency in ["GuildPoint","GuildArenaPoint"] {hero::currency(&mut db,a,currency,1_000_000).await.unwrap();}}
    let request=|shop|Bytes::from(format!("SessionKey={}&ShopIndex={shop}",u["UserInfo"]["SessionKey"].as_str().unwrap()));
    for shop in [4,20,21,27] {
        let v=crate::api::inventory::shop::get_shop_list(State(s.clone()),request(shop)).await.unwrap().0;
        assert_eq!(v["Result"],"Success","{v}");
        let items=v["ShopItems"].as_array().unwrap();
        if shop==20 {assert!(items.is_empty());continue;}
        assert!(!items.is_empty());
        if shop==27 {assert_eq!(items.len(),10);}
        if shop==4 {for row in items {let index=n(row,"ListNo");let def=s.tables.hero_shop.shop_items.iter().find(|v|n(v,"ShopIndex")==4&&n(v,"Index")==index).unwrap();assert_eq!(def["GroupIndex"],1);}}
        let row=items.iter().find(|v|v["ItemCode"]=="STAMINA_POTION_S"||v["ItemCode"]=="PRESENT_NPC_COMMON_3"||v["ItemCode"]=="CRYSTAL_1").unwrap_or(&items[0]);
        let body=Bytes::from(format!("SessionKey={}&ShopIndex={shop}&ShopItemIndex={}&ShopItemPurchaseCount=1",u["UserInfo"]["SessionKey"].as_str().unwrap(),n(row,"ListNo")));
        let bought=crate::api::inventory::shop::buy_shop_item(State(s.clone()),body).await.unwrap().0;
        assert_eq!(bought["Result"],"Success","shop {shop}: {bought}");
        let again=crate::api::inventory::shop::get_shop_list(State(s.clone()),request(shop)).await.unwrap().0;
        assert_eq!(again["ShopItems"].as_array().unwrap().iter().find(|v|n(v,"ListNo")==n(row,"ListNo")).unwrap()["Purchased"],1);
    }
}
#[tokio::test]
async fn guild_levels_buildings_and_skills_charge_target_level_once() {
    let s = setup().await;
    let u = login(&s, "builder").await;
    let g = create(&s, &u, "Builders", 1).await;
    {
        let mut db = s.db.acquire().await.unwrap();
        let mut v = guild::state(&mut db, &s, g).await.unwrap();
        v["ActivityPoint"] = json!(10000000);
        v["Wood"] = json!(1000000);
        v["Stone"] = json!(1000000);
        v["Metal"] = json!(1000000);
        guild::save(&mut db, g, &v).await.unwrap();
    }
    let level = call(&s, &u, "guild/level_up_guild", "Level=2").await;
    assert_eq!(level["Result"], "Success", "{level}");
    assert_ne!(
        call(&s, &u, "guild/level_up_guild", "Level=2").await["Result"],
        "Success"
    );
    let shop = call(
        &s,
        &u,
        "guild/level_up_guild_building",
        "BuildingIndex=3&BuildingLevel=2",
    )
    .await;
    assert_eq!(shop["Result"], "Success", "{shop}");
    assert_ne!(
        call(
            &s,
            &u,
            "guild/level_up_guild_building",
            "BuildingIndex=3&BuildingLevel=2"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "guild/level_up_guild_building",
            "BuildingIndex=2&BuildingLevel=2"
        )
        .await["Result"],
        "Success"
    );
    let skill = call(
        &s,
        &u,
        "guild/level_up_guild_skill",
        "SkillIndex=1&SkillLevel=2",
    )
    .await;
    assert_eq!(skill["Result"], "Success", "{skill}");
    {
        let mut db = s.db.acquire().await.unwrap();
        let info = guild::state(&mut db, &s, g).await.unwrap();
        assert_eq!(info["Skill1Level"], 2);
        let skill = info["SkillInfos"].as_array().unwrap().iter().find(|v| n(v,"SkillIndex")==1).unwrap();
        assert_eq!(skill["ActivitySpent"], 100000);
        assert_eq!(skill["EffectSkillIndex"], 70000002);
        assert_eq!(guild_reward_boost(&mut db, &s, n(&u["UserInfo"],"AccountId")).await.unwrap(), (3,3));
    }
    assert_ne!(
        call(
            &s,
            &u,
            "guild/level_up_guild_skill",
            "SkillIndex=1&SkillLevel=2"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "guild/init_guild_skill", "").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "guild/init_guild_skill", "").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn guild_arena_decks_matches_and_replay_protection() {
    let mut s = setup().await;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).arena_guild).rules
        ["GuildApplyDays"] = json!(0);
    let u = login(&s, "attacker").await;
    let v = login(&s, "defender").await;
    let g = create(&s, &u, "Attackers", 1).await;
    let enemy = create(&s, &v, "Defenders", 1).await;
    let season = arena::season(&s).0;
    for player in [&u, &v] {
        let empty = call(&s, player, "guild_arena/get_guild_arena_deck", &format!("SeasonIndex={season}")).await;
        assert_eq!(empty["DeckInfos"].as_array().unwrap().len(), 5);
        assert_eq!(empty["DeckInfos"][0]["HeroIndices"], json!([]));
        let d = call(
            &s,
            player,
            "guild_arena/set_guild_arena_deck",
            &format!("SeasonIndex={season}&HeroIndices1=1"),
        )
        .await;
        assert_eq!(d["Result"], "Success", "{d}");
        let skills = call(&s, player, "guild_arena/set_guild_arena_deck_skill",
            &format!("SeasonIndex={season}&SkillSlotIndices1=0&SkillSlotIndices1=2")).await;
        assert_eq!(skills["Result"], "Success", "{skills}");
        let saved = call(&s, player, "guild_arena/get_guild_arena_deck",
            &format!("SeasonIndex={season}")).await;
        assert_eq!(saved["DeckInfos"][0]["SkillSlotIndices"], json!([0, 2]));
        assert_eq!(
            call(
                &s,
                player,
                "guild_arena/apply_guild_arena",
                &format!("SessionIndex={season}")
            )
            .await["Result"],
            "Success"
        );
    }
    let entry=format!("SessionIndex={season}&EnemyGuildId={enemy}&EnemyAccountId={}&EnemyDeckIndex=1&HeroIndices=[1]",n(&v["UserInfo"],"AccountId"));
    let start = call(&s, &u, "guild_arena/match_guild_arena", &entry).await;
    assert_eq!(start["Result"], "Success", "{start}");
    assert_eq!(
        call(&s, &u, "guild_arena/match_guild_arena", &entry).await,
        start
    );
    let result = call(
        &s,
        &u,
        "guild_arena/set_guild_arena_match_result",
        "Win=1&PlayTime=10&AliveHeroIndices=[1]",
    )
    .await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_ne!(
        call(&s, &u, "guild_arena/set_guild_arena_match_result", "Win=1").await["Result"],
        "Success"
    );
    let rank = call(
        &s,
        &u,
        "guild_arena/get_guild_arena_ranker",
        &format!("SeasonIndex={season}&PageNo=0"),
    )
    .await;
    assert_eq!(rank["MyRankerInfo"]["GuildId"], g);
    assert_eq!(rank["MyRankerInfo"]["SessionScore"], 1);
    let records = call(
        &s,
        &u,
        "guild_arena/get_guild_arena_member_record",
        &format!("SessionIndex={season}&PageNo=0"),
    )
    .await;
    assert_eq!(records["MemberRecords"].as_array().unwrap().len(), 1);
    assert_eq!(
        records["MemberRecords"][0]["LeftTeam"]["AccountId"],
        u["UserInfo"]["AccountId"]
    );
    let old_start = arena::season(&s).1;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).arena_guild).rules
        ["SeasonEpoch"] = json!(time(old_start - season * 7 * 86400));
    let _ = login(&s, "attacker").await;
    let _ = login(&s, "attacker").await;
    let count: i64 =
        sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE title='Guild arena season reward'")
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let history = call(
        &s,
        &login(&s, "attacker").await,
        "guild_arena/get_guild_arena_ranker_record",
        &format!("SeasonIndex={season}&PageNo=0"),
    )
    .await;
    assert_eq!(history["RankerRecords"][0]["Win"], 1);
}

#[tokio::test]
async fn arena_pages_and_reward_rollover_are_persistent() {
    let s = setup().await;
    let u = login(&s, "rollover").await;
    let a = n(&u["UserInfo"], "AccountId");
    let first = call(&s, &u, "match/get_match_ranker", "StartRank=0&EndRank=0").await;
    assert_eq!(first["MatchRankers"].as_array().unwrap().len(), 1);
    assert_eq!(first["MatchRankers"][0]["UserInfo"]["AccountId"], a);
    assert!(
        call(&s, &u, "match/get_match_ranker", "StartRank=1&EndRank=1").await["MatchRankers"]
            .as_array()
            .unwrap()
            .is_empty()
    );
    {
        let mut db = s.db.acquire().await.unwrap();
        put(
            &mut db,
            a,
            "arena_daily",
            now() / 86400 - 1,
            &json!({"Day":time(now()-86400)[..10],"Rank":1,"TierIndex":70}),
        )
        .await
        .unwrap();
        sqlx::query(
            "INSERT INTO arena_scores(account,kind,season,score,wins) VALUES(?,0,?,1100,1)",
        )
        .bind(a)
        .bind(arena::season(&s).0 - 1)
        .execute(&mut *db)
        .await
        .unwrap();
    }
    login(&s, "rollover").await;
    login(&s, "rollover").await;
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE sender='Arena'")
        .fetch_one(&s.db)
        .await
        .unwrap();
    assert_eq!(count, 2);
}

#[tokio::test]
async fn conquest_registration_and_native_session_are_available() {
    let mut s = setup().await;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).arena_guild).rules["GuildConquestTestTime"]=json!(1767628800+3600);
    let u = login(&s, "suppression").await;
    create(&s, &u, "Conquest", 1).await;
    let info = call(&s, &u, "guild_suppress/get_guild_suppress_session_info", "").await;
    assert_eq!(info["GuildSuppressSessionInfo"]["State"], "Apply");
    assert_eq!(info["GuildSuppressApplied"], false);
    assert_eq!(info["GuildSuppressPlayInfos"].as_array().unwrap().len(),3);
    assert_eq!(info["GuildSuppressPlayInfos"][0]["MonsterHp0"],100001423152872_i64);
    let definitions = info["SPRKConquestSessionDefinitions"].as_array().unwrap();
    assert_eq!(definitions.len(), 1);
    let definition = &definitions[0];
    assert_eq!(definition["SessionIndex"], 10000);
    assert_eq!(definition["SeasonIndex"], 10000);
    assert_eq!(definition["ViewRankSeason"], 10000);
    assert_eq!(definition["GlobalApplyStart"], "2026-01-05 16:00:00");
    assert_eq!(definition["GlobalApplyEnd"], info["GuildSuppressSessionInfo"]["ApplyEndTime"]);
    assert_eq!(definition["GlobalBattleStart"], info["GuildSuppressSessionInfo"]["BattleStartedTime"]);
    assert_eq!(definition["GlobalBattleEnd"], info["GuildSuppressSessionInfo"]["BattleEndTime"]);
    assert_eq!(definition["SuppressDungeonIndex"], json!([290,291,292]));
    assert_eq!(definition["Level"], json!([1,2,3]));
    assert_eq!(definition["SingleLevel"], json!([101,102,103]));
    assert_eq!(
        call(&s, &u, "guild_suppress/apply_guild_suppress", "").await["Result"],
        "Success"
    );
    assert_eq!(call(&s,&u,"guild_suppress/get_guild_suppress_session_info","").await["GuildSuppressApplied"],true);
    assert_eq!(call(&s,&u,"guild_suppress/apply_guild_suppress","").await["Result"],"AlreadyApplied");
    // Weekly rollover remains unbounded on the server; archived requests also
    // carry their definitions so the client never needs a generated table row.
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).arena_guild).rules["GuildConquestTestTime"]=json!(1767628800+3*604800+3600);
    let later = call(&s, &u, "guild_suppress/get_guild_suppress_session_info", "SessionIndex=10000").await;
    let definitions = later["SPRKConquestSessionDefinitions"].as_array().unwrap();
    assert_eq!(definitions.iter().map(|v|n(v,"SessionIndex")).collect::<Vec<_>>(), vec![10000,10002,10003]);
    assert_eq!(definitions[2]["GlobalApplyStart"], "2026-01-26 16:00:00");
    assert_eq!(definitions[2]["GlobalApplyEnd"], later["GuildSuppressSessionInfo"]["ApplyEndTime"]);
}

#[tokio::test]
async fn portal_guild_raid_polling_without_membership_is_empty() {
    let s = setup().await;
    let u = login(&s, "portal_no_guild").await;
    for _ in 0..2 {
        let list = call(&s, &u, "guild_raid/get_guild_raid_list", "").await;
        assert_eq!(list["Result"], "Success", "{list}");
        assert_eq!(list["GuildRaidInfos"], json!([]));
        // Native RequestGuildRaidInfo chains this request after the raid list.
        let scores = call(&s, &u, "guild_raid/get_guild_raid_member_score_list", "").await;
        assert_eq!(scores["Result"], "Success", "{scores}");
        assert_eq!(scores["GuildRaidMemberTotalScores"], json!([]));
    }
    let denied = call(&s, &u, "guild_raid/ping_guild_raid", "").await;
    assert_ne!(denied["Result"], "Success", "{denied}");
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM community_state WHERE kind='guild_raid'")
        .fetch_one(&s.db).await.unwrap();
    assert_eq!(count, 0);
    create(&s, &u, "Portal", 1).await;
    let list = call(&s, &u, "guild_raid/get_guild_raid_list", "").await;
    assert_eq!(list["Result"], "Success", "{list}");
    assert!(!list["GuildRaidInfos"].as_array().unwrap().is_empty());
}

#[tokio::test]
async fn arena_ranks_are_zero_based_and_pages_keep_first_player() {
    let s = setup().await;
    let first = login(&s, "rank-first").await;
    let second = login(&s, "rank-second").await;
    for (user, rank) in [(&first, 0), (&second, 1)] {
        let info = call(&s, user, "match/get_match_rank", "").await;
        assert_eq!(info["RankResult"]["TotalRank"], rank, "{info}");
        assert_eq!(info["BattleInfo"]["TierRank"], rank);
        let page = call(&s, user, "match/get_match_ranker", &format!("StartRank={rank}&EndRank={rank}")).await;
        assert_eq!(page["MatchRankers"].as_array().unwrap().len(), 1);
        assert_eq!(page["MatchRankers"][0]["Rank"], rank);
        assert_eq!(page["MatchRankers"][0]["AccountId"], user["UserInfo"]["AccountId"]);
    }
    let registered = call(&s, &second, "match/register_match", "ArenaType=Normal&HeroIndices=1&LeaderHeroIndex=1").await;
    assert_eq!(registered["AccountInfo"]["Rank"], 1);
}
