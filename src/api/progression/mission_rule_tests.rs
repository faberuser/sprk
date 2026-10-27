use super::*;
use crate::{api::account::user, models::item::ItemGrant};

async fn setup() -> (AppState,user::LoginResponse) {
    let db=sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
    crate::database::create_tables(&db).await.unwrap();
    let s=AppState::new(db,crate::tables::test_tables());
    let u=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=mission-rule-audit")).await.unwrap().0;
    sqlx::query("UPDATE user_info SET gold=100000000").execute(&s.db).await.unwrap();
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
        sqlx::query("UPDATE progression_login SET days=?,last_day=date('now') WHERE account_id=?")
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
    sqlx::query("UPDATE heroes SET equip_item_slot_index_1=?,equip_item_slot_index_5=? WHERE account_id=? AND hero_index=1")
        .bind(weapon).bind(artifact).bind(a).execute(&s.db).await.unwrap();
    let v=view(&s,a).await;
    for id in [10233,1000370,5002440,5002580,1000400] {assert_eq!(v.quest(&s,quest(&s,id)),1,"equipped: {id}");}
    assert_eq!(v.quest(&s,quest(&s,1000450)),0,"UW is not a Unique Treasure");
    assert_eq!(v.quest(&s,quest(&s,1000380)),0,"UW is not a Class Unique Weapon");
    sqlx::query("UPDATE heroes SET equip_item_slot_index_5=0 WHERE account_id=? AND hero_index=1")
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
    let inventory:Value=serde_json::from_str(include_str!("../../../config/mission-rule-gaps.json")).unwrap();
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
    let path=std::path::Path::new(env!("CARGO_MANIFEST_DIR")).join("target/mission-rule-audit.json");
    std::fs::write(&path,serde_json::to_vec_pretty(&report).unwrap()).unwrap();
    println!("Mission rule audit: {} definitions, {unresolved} unresolved. {}",report["definitions"],path.display());
}
