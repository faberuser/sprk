use super::*;
use crate::{api::account::user, models::item::ItemGrant};

async fn setup() -> (AppState,user::LoginResponse) {
    let db=crate::database::test_pool().await;
    let s=AppState::new(db,crate::tables::test_tables());
    let u=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=mission-rule-audit")).await.unwrap().0;
    crate::db::query("UPDATE user_info SET gold=100000000").execute(&s.db).await.unwrap();
    (s,u)
}
fn quest(s:&AppState,id:i64)->&Value {
    s.tables.progression.sub_quests.iter().find(|r|r["QuestIndex"]==id).unwrap()
}
async fn view(s:&AppState,a:i64)->View {View::load(&mut s.db.acquire().await.unwrap(),a).await.unwrap()}
async fn grant(s:&AppState,a:i64,id:i32,count:i32,star:i32)->Rewards {
    let mut tx=s.db.begin().await.unwrap();let mut r=Rewards::default();
    item::give(&mut tx,s,a,ItemGrant{index:id,count,star,custom:0},&mut r).await.unwrap();
    tx.commit().await.unwrap();r
}
async fn claim(s:&AppState,u:&user::LoginResponse,id:i64)->Value {
    handle(s.clone(),Bytes::from(format!("SessionKey={}&SubQuestIndices=[{id}]",u.user_info.session_key)),"complete_sub_quest").await.unwrap().0
}
async fn action(s:&AppState,a:i64,kind:&str,item:i64,arg:i64,count:i64) {
    record(&mut s.db.acquire().await.unwrap(),a,kind,item,arg,count).await.unwrap();
}

#[tokio::test]
async fn mission_rules_login_days_enforce_each_native_target_and_claim_once() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    assert_ne!(claim(&s,&u,5001490).await["Result"],"Success","Day 7 must not complete on day 1");
    for days in [1,6,7,14] {
        crate::db::query("UPDATE progression_login SET days=$1,last_day=sprk_date('now') WHERE account_id=$2")
            .bind(days).bind(a).execute(&s.db).await.unwrap();
        let v=view(&s,a).await;
        let rows:Vec<_>=s.tables.progression.sub_quests.iter().filter(|r|r["Kind"]=="LoginDaily").collect();
        assert_eq!(rows.len(),24);
        for r in rows {
            let required=r["Variable3"][0].as_str().unwrap().parse::<i64>().unwrap();
            assert_eq!(v.quest(&s,r),(days>=required) as i64,"day {days}, quest {}",r["QuestIndex"]);
        }
    }
    assert_eq!(claim(&s,&u,5001490).await["Result"],"Success");
    assert_ne!(claim(&s,&u,5001490).await["Result"],"Success");
}

#[tokio::test]
async fn mission_rules_equipment_requires_correct_type_and_actual_equipping() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let id=s.tables.extensions.rows("EquipItem").iter()
        .find(|r|n(r,"PartType")==5 && n(r,"DetailIndex")==90002 && s.tables.hero_shop.items.contains_key(&(n(r,"ItemIndex") as i32)))
        .map(|r|n(r,"ItemIndex") as i32).unwrap();
    let artifact=grant(&s,a,id,1,0).await.equipment[0].slot_index;
    let weapon=grant(&s,a,1001,1,0).await.equipment[0].slot_index;
    let v=view(&s,a).await;
    for id in [10233,1000370,5002440,5002580,1000400] {assert_eq!(v.quest(&s,quest(&s,id)),0,"ownership alone: {id}");}
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=$1,equip_item_slot_index_5=$2 WHERE account_id=$3 AND hero_index=1")
        .bind(weapon).bind(artifact).bind(a).execute(&s.db).await.unwrap();
    let v=view(&s,a).await;
    for id in [10233,1000370,5002440,5002580,1000400] {assert_eq!(v.quest(&s,quest(&s,id)),1,"equipped: {id}");}
    assert_eq!(v.quest(&s,quest(&s,1000450)),0,"UW is not a Unique Treasure");
    assert_eq!(v.quest(&s,quest(&s,1000380)),0,"UW is not a Class Unique Weapon");
    crate::db::query("UPDATE heroes SET equip_item_slot_index_5=0 WHERE account_id=$1 AND hero_index=1")
        .bind(a).execute(&s.db).await.unwrap();
    assert_eq!(view(&s,a).await.quest(&s,quest(&s,10233)),0);
}

#[tokio::test]
async fn mission_rules_equipment_actions_keep_item_filters_and_star_targets() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let treasure=s.tables.extensions.rows("EquipItem").iter().find(|r|n(r,"DetailIndex")==90004).map(|r|n(r,"ItemIndex")).unwrap();
    let legendary=s.tables.extensions.rows("EquipItem").iter().find(|r|n(r,"DetailIndex")==6001).map(|r|n(r,"ItemIndex")).unwrap();
    action(&s,a,"RenewEquipOption",0,0,100).await;
    action(&s,a,"RenewEquipOption",1001,0,1).await;
    let v=view(&s,a).await;
    assert_eq!(v.quest(&s,quest(&s,1000460)),0,"generic/weapon actions cannot prove UT reforging");
    assert_eq!(v.quest(&s,quest(&s,1000340)),0,"generic/weapon actions cannot prove legendary reforging");
    action(&s,a,"RenewEquipOption",treasure,0,1).await;
    let v=view(&s,a).await;
    assert_eq!(v.quest(&s,quest(&s,1000460)),1);
    assert_eq!(v.quest(&s,quest(&s,1000340)),0);
    action(&s,a,"RenewEquipOption",legendary,0,1).await;
    assert_eq!(view(&s,a).await.quest(&s,quest(&s,1000340)),1);
    action(&s,a,"AwakenGear",1001,1,1).await;
    assert_eq!(view(&s,a).await.quest(&s,quest(&s,10471)),0,"one star is below the two-star awakening target");
    action(&s,a,"AwakenGear",1001,2,1).await;
    assert_eq!(view(&s,a).await.quest(&s,quest(&s,10471)),1);
    let unknown=json!({"Kind":"RenewEquipOption","Goal":"UNVERIFIED_NEW_GOAL","StateValue":null,"ReqProgress":1});
    assert_eq!(view(&s,a).await.quest_value(&s,&unknown),None);
    assert_eq!(view(&s,a).await.quest(&s,&unknown),0);
}

#[tokio::test]
async fn mission_rules_shop_potion_and_npc_filters_reject_unrelated_history() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    action(&s,a,"BuyShopItem",11,123,1).await;
    let stamina=s.tables.inventory.potions.iter().find(|(_,r)|n(r,"ActionType")==1).map(|(id,_)|*id).unwrap();
    action(&s,a,"UsePotionItem",stamina as i64,0,10).await;
    let exp=s.tables.inventory.potions.iter().find(|(_,r)|n(r,"ActionType")==3).map(|(id,_)|*id).unwrap();
    let wb=s.tables.progression.sub_quests.iter().find(|r|r["Goal"]=="MAIN_MISSION_GOAL_36_1").unwrap();
    let v=view(&s,a).await;
    assert_eq!(v.quest(&s,wb),0);
    assert_eq!(v.quest(&s,quest(&s,10201)),0,"stamina potions are not EXP potions");
    action(&s,a,"BuyShopItem",12,123,1).await;
    action(&s,a,"UsePotionItem",exp as i64,0,2).await;
    let v=view(&s,a).await;
    assert_eq!(v.quest(&s,wb),1);
    assert_eq!(v.quest(&s,quest(&s,10201)),2);
    let kinship=s.tables.progression.sub_quests.iter().find(|r|r["Goal"]=="MILESTONE_MISSION_GOAL_105").unwrap();
    let npc=kinship["Variable3"][0].as_str().unwrap().strip_prefix("CREATURE_NAME_").unwrap().parse::<i64>().unwrap();
    action(&s,a,"GiveFriendlyPoint",npc+1,0,3000).await;
    assert_eq!(view(&s,a).await.quest(&s,kinship),0);
    action(&s,a,"GiveFriendlyPoint",npc,0,3000).await;
    assert_eq!(view(&s,a).await.quest(&s,kinship),3000);
}

#[tokio::test]
async fn mission_rules_valid_failed_awakening_counts_attempt_but_not_success() {
    let (mut s,u)=setup().await;let a=u.user_info.account_id;
    for row in std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).extensions).0.get_mut("EquipAwakenRatio").unwrap() {
        row["SuccessRatio"]=json!(0);row["FailBonusRatio"]=json!(0);
    }
    let slot=grant(&s,a,1001,1,0).await.equipment[0].slot_index;
    grant(&s,a,5301,1,0).await;
    let request=||Bytes::from(format!("SessionKey={}&EquipItemSlotIndex={slot}&MaterialItemInfors=[5301,1]",u.user_info.session_key));
    let send=||crate::api::extensions::handle(State(s.clone()),axum::extract::OriginalUri("/equip/awaken_equip".parse().unwrap()),request());
    let reply=send().await.unwrap().0;
    assert_eq!(reply["Result"],"Success","{reply}");assert_eq!(reply["Success"],false);
    let v=view(&s,a).await;
    assert_eq!(v.quest(&s,quest(&s,5003110)),1,"Try Awakening counts a valid failed roll");
    assert_eq!(v.quest(&s,quest(&s,1000410)),0,"failure does not reach one-star UW");
    assert_eq!(v.metric("AwakenGear","all",None,None),0);
    assert_ne!(send().await.unwrap().0["Result"],"Success","insufficient material is not an attempt");
    assert_eq!(view(&s,a).await.metric("AwakenGearAttempt","all",None,None),1);
}

#[tokio::test]
async fn mission_rules_simple_action_targets_deliver_rewards_once() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    action(&s,a,"ArtifactRestore",0,0,4).await;
    assert_ne!(claim(&s,&u,1000360).await["Result"],"Success");
    action(&s,a,"ArtifactRestore",0,0,1).await;
    assert_eq!(claim(&s,&u,1000360).await["Result"],"Success");
    assert_ne!(claim(&s,&u,1000360).await["Result"],"Success");
    for (kind,id) in [("EquipRune",5003120),("SoulRestore",5003140),("SoulWeaponRenewOption",1000510)] {
        assert_eq!(view(&s,a).await.quest(&s,quest(&s,id)),0);
        action(&s,a,kind,0,0,1).await;
        assert_eq!(view(&s,a).await.quest(&s,quest(&s,id)),1);
    }
}

fn audit_rows(s:&AppState,v:&View)->Vec<Value> {
    let mut rows=vec![];
    for (family,definitions) in [("SubQuests",&s.tables.progression.sub_quests),("ClearMissions",&s.tables.progression.clear_missions),("NewbieMissions",&s.tables.progression.newbie_missions)] {
        for r in definitions {
            rows.push(json!({"family":family,"id":n(r,if family=="SubQuests" {"QuestIndex"}else{"Index"}),
                "step":r["Step"],"kind":r["Kind"],"goal":r["Goal"],"state_value":r["StateValue"],"required_progress":r["ReqProgress"],
                "display_target":r["Variable3"],"client_only":r["ClientOnly"],
                "rule":if family=="NewbieMissions" && r["Kind"].is_null(){"entitlement_only"}
                    else if v.quest_value(s,r).is_some(){"implemented"}else{"unresolved"}}));
        }
    }
    rows
}

#[tokio::test]
async fn mission_rules_coverage_inventory_detects_new_or_regressed_missing_rules() {
    use std::collections::BTreeMap;
    let (s,u)=setup().await;let v=view(&s,u.user_info.account_id).await;
    let mut actual=BTreeMap::<(String,String,String),i64>::new();
    for r in audit_rows(&s,&v).iter().filter(|r|r["rule"]=="unresolved") {
        let key=(r["family"].as_str().unwrap_or("").to_owned(),r["kind"].as_str().unwrap_or("").to_owned(),r["goal"].as_str().unwrap_or("").to_owned());
        *actual.entry(key).or_default()+=1;
    }
    let inventory:Value=serde_json::from_str(include_str!("../../../../config/mission-rule-gaps.json")).unwrap();
    assert_eq!(inventory["schema_version"],1);
    let expected:BTreeMap<_,_>=inventory["rules"].as_array().unwrap().iter().map(|r|
        ((r["family"].as_str().unwrap().to_owned(),r["kind"].as_str().unwrap().to_owned(),r["goal"].as_str().unwrap().to_owned()),n(r,"definitions"))).collect();
    assert_eq!(actual,expected,"Mission coverage changed: implement verified rules or explicitly document remaining gaps in config/mission-rule-gaps.json");
}

#[tokio::test]
#[ignore = "Explicit mission rule inventory; writes a diagnostic report under target only"]
async fn export_mission_rule_audit() {
    let (s,u)=setup().await;let v=view(&s,u.user_info.account_id).await;
    let rows=audit_rows(&s,&v);
    let unresolved=rows.iter().filter(|r|r["rule"]=="unresolved").count();
    let report=json!({"note":"Definition coverage, not proof of original server parity. Includes retired/event/paid/client-only definitions. Unresolved rules stay pending; no live account data is included.",
        "definitions":rows.len(),"unresolved":unresolved,"rows":rows});
    let path=std::path::Path::new(env!("CARGO_MANIFEST_DIR")).join("../target/mission-rule-audit.json");
    std::fs::write(&path,serde_json::to_vec_pretty(&report).unwrap()).unwrap();
    println!("Mission rule audit: {} definitions, {unresolved} unresolved. {}",report["definitions"],path.display());
}

#[tokio::test]
async fn mission_rules_raid_tab_milestones_follow_their_raid_and_stage() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let progress=|v:&View,id:i64| v.quest(&s,quest(&s,id));
    let clear=|c:i64,d:i64,count:i64| action(&s,a,"ClearDungeon",c,d,count);
    let v=view(&s,a).await;
    for id in [1001160,1001200,1001220,1001450,1001470,1001600,1002500,1002940] {assert_eq!(progress(&v,id),0,"{id} starts pending");}
    // Fire Dragon single T7 Stage 2 (Level 10) is below T8 Stage 1.
    clear(905,10,1).await;
    assert_eq!(progress(&view(&s,a).await,1001160),0);
    assert_ne!(claim(&s,&u,1001160).await["Result"],"Success");
    // Fire Dragon multi T8 Stage 3 completes T8 Stage 1 and 3, not Frost.
    clear(901,13,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1001160),progress(&v,1001180),progress(&v,1001230)),(1,1,0));
    assert_eq!(claim(&s,&u,1001160).await["Result"],"Success");
    // Single hard mode Stage 4 reports the shared hard raid 11.
    clear(931,4,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1001190),progress(&v,1001220),progress(&v,1001260)),(1,1,0));
    // Field raids: Lava counts clears of any stage; Stage 2 needs Level 2.
    clear(8,100,49).await;clear(8,101,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1001450),progress(&v,1001470),progress(&v,1001490)),(50,0,0));
    assert_eq!(claim(&s,&u,1001450).await["Result"],"Success");
    clear(8,111,1).await;
    assert_eq!(progress(&view(&s,a).await,1001470),1);
    // Technomagic and Technomagic Enchantment raids by boss and stage.
    clear(10,2106,1).await;clear(10,4103,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1002500),progress(&v,1002440),progress(&v,1002940),progress(&v,1002910)),(1,0,1,0));
    // Challenge Raid rows count their own mode, five clears per node in order.
    clear(921,1,7).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1001600),progress(&v,1001610),progress(&v,1001620),progress(&v,1001660)),(7,2,0,0),"raw progress; the client sees it capped at 5");
    assert_eq!(claim(&s,&u,1001600).await["Result"],"Success");
    assert_ne!(claim(&s,&u,1001610).await["Result"],"Success","Multi 2 needs clears 6-10");
    clear(941,1,5).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1001660),progress(&v,1001670),progress(&v,1001610)),(5,0,2),"Single clears never count for Multi");
    clear(951,1,3).await;
    assert_eq!(claim(&s,&u,1001610).await["Result"],"Success");
    // Enchant goals only count that element's scrolls.
    action(&s,a,"Enchant",1,45029,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1001200),progress(&v,1001270)),(1,0));
}

#[tokio::test]
async fn mission_rules_battle_tab_milestones_follow_their_content_and_stage() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let progress=|v:&View,id:i64| v.quest(&s,quest(&s,id));
    let clear=|c:i64,d:i64,count:i64| action(&s,a,"ClearDungeon",c,d,count);
    let ids=[1000650,1000670,11031,1000700,1000710,1000760,1000820,1000880,1000890,1000920,1001080,1001090,1001110,1001120];
    let v=view(&s,a).await;
    for id in ids {assert_eq!(progress(&v,id),0,"{id} starts pending");}
    // Royal Underground Labyrinth: floor B3 of the Priest labyrinth (1007).
    let floor=|tower:i64,f:i64| s.tables.battle.rows("TowerFloor").iter()
        .find(|r|n(r,"TowerIndex")==tower && n(r,"Floor")==f).map(|r|(n(r,"ChapterIndex"),n(r,"DungeonIndex"))).unwrap();
    let challenge=floor(21,3);clear(challenge.0,challenge.1,5).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1000650),progress(&v,11031)),(0,0),"Tower of Challenge floors are not the Labyrinth");
    let (c,d)=floor(1007,3);clear(c,d,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1000650),progress(&v,1000670),progress(&v,1000680),progress(&v,11031)),(1,1,0,1));
    // World Boss damage: best single battle against that boss, any season.
    for (boss,day,score) in [(3,"2026-01-01",20_000_000_000_i64),(3,"2026-01-02",20_000_000_000)] {
        crate::db::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',$1,1,$2,$3,$4)")
            .bind(boss).bind(a).bind(day).bind(score).execute(&s.db).await.unwrap();
    }
    assert_eq!(progress(&view(&s,a).await,1000700),0,"30B needs one battle, not a sum of days");
    crate::db::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',97,1,$1,'2026-01-03',40000000000)")
        .bind(a).execute(&s.db).await.unwrap();
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1000700),progress(&v,1000710),progress(&v,1000760),progress(&v,1000820)),(1,0,0,0));
    // God King trials: stage N or a harder one of the same gate.
    clear(100000,3,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1000880),progress(&v,1000890),progress(&v,1000900),progress(&v,1000920)),(1,1,0,0));
    // Shakmeh: story stage 6 and Devourer stage 2; Otherworldly Darkness untouched.
    clear(50000,6,1).await;clear(50000,502,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1001080),progress(&v,1001090),progress(&v,1001100),progress(&v,1001110),progress(&v,1001120)),(1,1,0,1,0));
    assert_eq!(claim(&s,&u,1001080).await["Result"],"Success");
}

#[tokio::test]
async fn mission_rules_guideline_growth_and_guild_milestones_use_their_filters() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let progress=|v:&View,id:i64| v.quest(&s,quest(&s,id));
    let ids=[11221,10832,11101,10951,11491,11102,11281,10781,10782,1000390,1000440,1000250,1000260,1000350,1002070,1002090,1002130];
    let v=view(&s,a).await;
    for id in ids {assert_eq!(progress(&v,id),0,"{id} starts pending");}
    // Field Raid Lava, Repositorium, Fire Dragon entry, Eclipse, Challenge Stage.
    action(&s,a,"ClearDungeon",8,100,2).await;action(&s,a,"ClearDungeon",5100,1,2).await;
    action(&s,a,"EnterDungeon",902,11,1).await;
    action(&s,a,"EndEclipse",0,0,1).await;action(&s,a,"EnterDungeon",97,11,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,11221),progress(&v,11421),progress(&v,10832),progress(&v,11101),progress(&v,10951),progress(&v,11491)),(2,0,2,0,1,0),
        "Frost Dragon and substory stages do not count");
    action(&s,a,"EnterDungeon",931,4,1).await;action(&s,a,"EnterDungeon",97,12,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,11101),progress(&v,11491)),(1,1));
    // Shops: Raid Shop item 335 is the Baby Dragon box, 336 the Dragon box; Hero Inn shop is 10.
    action(&s,a,"BuyShopItem",11,336,1).await;action(&s,a,"BuyShopItem",10,1,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,11102),progress(&v,1000250),progress(&v,11281),progress(&v,1000260)),(0,1,1,0),
        "a stamina potion is not a kinship material");
    action(&s,a,"BuyShopItem",11,335,1).await;action(&s,a,"BuyShopItem",10,7,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,11102),progress(&v,1000260)),(1,1));
    // Crafting counts the named item only.
    action(&s,a,"CraftItem",0,0,5).await;action(&s,a,"CraftItem",9999,15,3).await;action(&s,a,"CraftItem",2017,63,1).await;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,10781),progress(&v,10782),progress(&v,1000390),progress(&v,1000440),progress(&v,1002070)),(3,0,0,1,0));
    // Tower of Challenge clears (50 needed), not Labyrinth floors.
    let floor=|tower:i64| s.tables.battle.rows("TowerFloor").iter().find(|r|n(r,"TowerIndex")==tower).map(|r|(n(r,"ChapterIndex"),n(r,"DungeonIndex"))).unwrap();
    let (c,d)=floor(1001);action(&s,a,"ClearDungeon",c,d,60).await;
    assert_eq!(progress(&view(&s,a).await,1000350),0);
    let (c,d)=floor(21);action(&s,a,"ClearDungeon",c,d,50).await;
    assert_eq!(progress(&view(&s,a).await,1000350),50);
    // Guild: Conquest rounds ranked 1st and Guild War attacks.
    action(&s,a,"GuildSuppressRank",2,1,1).await;action(&s,a,"GuildSuppressRank",1,2,2).await;
    crate::db::query("INSERT INTO guild_arena_records(guild_id,enemy_guild,account,enemy,season,win,data) VALUES(1,2,$1,3,1,0,'{}')")
        .bind(a).execute(&s.db).await.unwrap();
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1002090),progress(&v,1002130)),(2,1),"a lost attack still counts; 2nd place does not");
    assert_eq!(claim(&s,&u,1002130).await["Result"],"Success");
}

#[tokio::test]
async fn mission_rules_equip_milestones_require_the_named_gear_worn() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let progress=|v:&View,id:i64| v.quest(&s,quest(&s,id));
    let find=|f:&dyn Fn(&Value)->bool| s.tables.extensions.rows("EquipItem").iter()
        .find(|r|f(r) && s.tables.hero_shop.items.contains_key(&(n(r,"ItemIndex") as i32))).map(|r|n(r,"ItemIndex") as i32).unwrap();
    let mana=find(&|r|n(r,"PartType")==7 && n(r,"DetailIndex")!=90004);
    let chaos=find(&|r|n(r,"SetIndex")==102 && n(r,"PartType")==6);
    let unique=find(&|r|n(r,"PartType")==7 && n(r,"DetailIndex")==90004);
    let mana=grant(&s,a,mana,1,0).await.equipment[0].slot_index;
    let chaos=grant(&s,a,chaos,1,0).await.equipment[0].slot_index;
    let unique=grant(&s,a,unique,1,0).await.equipment[0].slot_index;
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1000430),progress(&v,1000470),progress(&v,1002220)),(0,0,0),"owning is not wearing");
    crate::db::query("UPDATE heroes SET equip_item_slot_index_7=$1,equip_item_slot_index_6=$2 WHERE account_id=$3 AND hero_index=1")
        .bind(unique).bind(chaos).bind(a).execute(&s.db).await.unwrap();
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1000430),progress(&v,1000470),progress(&v,1002220)),(0,0,1),"a primary Unique Treasure is neither");
    crate::db::query("UPDATE heroes SET equip_item_slot_index_7=$1,equip_item_slot_index_8=$2 WHERE account_id=$3 AND hero_index=1")
        .bind(mana).bind(unique).bind(a).execute(&s.db).await.unwrap();
    let v=view(&s,a).await;
    assert_eq!((progress(&v,1000430),progress(&v,1000470)),(1,1));
}

#[tokio::test]
async fn mission_rules_weekly_eclipse_key_counts_keys_spent_this_week() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let row=s.tables.progression.achievements.iter().find(|r|n(r,"Index")==1715).unwrap();
    assert_eq!(view(&s,a).await.achievement(&s,row),(0,1));
    crate::db::query("INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES($1,'2000-01-01','ConsumeEclipseKey',0,0,5)")
        .bind(a).execute(&s.db).await.unwrap();
    assert_eq!(view(&s,a).await.achievement(&s,row),(0,1),"an old week does not count");
    action(&s,a,"ConsumeEclipseKey",0,0,1).await;
    assert_eq!(view(&s,a).await.achievement(&s,row),(1,1));
}

#[tokio::test]
async fn mission_rules_god_king_trial_goal_counts_trials_not_substories() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    // Guideline 10931: "Challenge the Trial of the God King {1} time(s)".
    action(&s,a,"ClearDungeon",97,2,2).await;
    assert_eq!(view(&s,a).await.quest(&s,quest(&s,10931)),0,"substory stages are not God King trials");
    action(&s,a,"ClearDungeon",100001,1,2).await;
    assert_eq!(view(&s,a).await.quest(&s,quest(&s,10931)),2);
}
