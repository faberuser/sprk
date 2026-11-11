use crate::models::item::ItemGrant;
use super::*;
use crate::api::account::user;

async fn setup() -> (AppState, user::LoginResponse) {
    let db = crate::database::test_pool().await;
    let state = AppState::new(db, crate::tables::test_tables());
    let u = user::test_login(
        State(state.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    crate::db::query("UPDATE user_info SET gold=100000000,gem=100000")
        .execute(&state.db)
        .await
        .unwrap();
    (state, u)
}
async fn call(s: &AppState, u: &user::LoginResponse, action: &str, args: &str) -> Value {
    let path = if action.contains('/') {
        format!("/{action}")
    } else if action.starts_with("valance_") || action == "confirm_valance_enchant" {
        format!("/valance/{action}")
    } else if action.contains("equip_storage_slot") {
        format!("/equip_storage_slot/{action}")
    } else {
        format!("/equip/{action}")
    };
    handle(
        State(s.clone()),
        axum::extract::OriginalUri(path.parse().unwrap()),
        Bytes::from(format!("SessionKey={}&{args}", u.user_info.session_key)),
    )
    .await
    .unwrap()
    .0
}

#[tokio::test]
async fn option_enhancement_accepts_native_per_step_cost_and_limits_batches() {
    // T8 legendary gear: the native request sends 1,152 dust even when the
    // confirmation popup quotes the total for many steps.
    for (action, dust_steps, gold_steps, expected_steps) in [
        ("max_upgrade_equip_option", 50, 50, 40),
        ("max_upgrade_equip_option", 3, 50, 3),
        ("max_upgrade_equip_option", 50, 2, 2),
        ("upgrade_equip_option", 50, 50, 1),
    ] {
        let (s, u) = setup().await;
        let a = u.user_info.account_id;
        let eq = give(&s, a, 810009, 1).await.equipment.remove(0);
        give(&s, a, 43055, 1152 * dust_steps).await;
        // Replace any initial inventory dust when imposing the material limit.
        crate::db::query("UPDATE items SET count=$1 WHERE account_id=$2 AND item_index=43055")
            .bind(1152 * dust_steps + 7).bind(a).execute(&s.db).await.unwrap();
        crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2")
            .bind(768000 * gold_steps + 11).bind(a).execute(&s.db).await.unwrap();
        crate::db::query("UPDATE equip_items SET option_index_1=101,option_index_2=101,option_index_3=101,option_index_4=101,option_index_5=0,option_step_1=0,option_step_2=0,option_step_3=0,option_step_4=0 WHERE slot_index=$1")
            .bind(eq.slot_index).execute(&s.db).await.unwrap();
        let out = call(&s, &u, action, &format!("EquipItemSlotIndex={}&MaterialItemIndex=43055&MaterialItemCount=1152", eq.slot_index)).await;
        assert_eq!(out["Result"], "Success", "{out}");
        let result = &out["ResultEquipItem"];
        assert_eq!((1..=4).map(|i| n(result, &format!("OptionStep{i}"))).sum::<i64>(), expected_steps);
        for i in 1..=4 { assert!(n(result, &format!("OptionStep{i}")) <= 10); }
        assert_eq!(gold(&s).await, 768000 * (gold_steps as i64 - expected_steps) + 11);
        assert_eq!(out["CurrencyResult"]["AddValue"], -768000 * expected_steps);
        assert_eq!(out["ItemResult"]["AddCount"], -1152 * expected_steps);
        let dust: i64 = crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=43055")
            .bind(a).fetch_one(&s.db).await.unwrap();
        assert_eq!(dust, 1152 * (dust_steps as i64 - expected_steps) + 7);
        let persisted: i64 = crate::db::query_scalar("SELECT option_step_1+option_step_2+option_step_3+option_step_4 FROM equip_items WHERE slot_index=$1")
            .bind(eq.slot_index).fetch_one(&s.db).await.unwrap();
        assert_eq!(persisted, expected_steps);
    }
}

#[tokio::test]
async fn option_enhancement_errors_are_native_and_preserve_inventory() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let eq = give(&s, a, 810009, 1).await.equipment.remove(0);
    give(&s, a, 43055, 10000).await;
    crate::db::query("UPDATE equip_items SET option_index_1=101,option_step_1=9,option_index_2=0,option_index_3=0,option_index_4=0,option_index_5=101,option_step_5=9 WHERE slot_index=$1")
        .bind(eq.slot_index).execute(&s.db).await.unwrap();
    let before_gold = gold(&s).await;
    let before_dust: i64 = crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=43055")
        .bind(a).fetch_one(&s.db).await.unwrap();
    for (slot, material, count, expected) in [
        (eq.slot_index, 43055, 2304, "MaterialItemCountMismatch"),
        (eq.slot_index, 43056, 1152, "MaterialItemIndexMismatch"),
        (i32::MAX, 43055, 1152, "EquipNotOwned"),
    ] {
        let out = call(&s, &u, "max_upgrade_equip_option", &format!("EquipItemSlotIndex={slot}&MaterialItemIndex={material}&MaterialItemCount={count}")).await;
        assert_eq!(out["Result"], expected, "{out}");
    }
    assert_eq!(gold(&s).await, before_gold);
    // Failed resource checks must leave both option slots and balances intact.
    for (gold, dust, expected) in [(0, before_dust, "NotEnoughGold"), (before_gold, 0, "NotEnoughMaterialItem")] {
        crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2")
            .bind(gold).bind(a).execute(&s.db).await.unwrap();
        crate::db::query("UPDATE items SET count=$1 WHERE account_id=$2 AND item_index=43055")
            .bind(dust).bind(a).execute(&s.db).await.unwrap();
        let out = call(&s, &u, "max_upgrade_equip_option", &format!("EquipItemSlotIndex={}&MaterialItemIndex=43055&MaterialItemCount=1152", eq.slot_index)).await;
        assert_eq!(out["Result"], expected, "{out}");
        let unchanged: (i64, i64) = crate::db::query_as("SELECT option_step_1,option_step_5 FROM equip_items WHERE slot_index=$1")
            .bind(eq.slot_index).fetch_one(&s.db).await.unwrap();
        assert_eq!(unchanged, (9, 9));
    }
    crate::db::query("UPDATE items SET count=$1 WHERE account_id=$2 AND item_index=43055")
        .bind(before_dust).bind(a).execute(&s.db).await.unwrap();
    let out = call(&s, &u, "max_upgrade_equip_option", &format!("EquipItemSlotIndex={}&MaterialItemIndex=43055&MaterialItemCount=1152", eq.slot_index)).await;
    assert_eq!(out["Result"], "Success", "{out}");
    assert_eq!(out["ResultEquipItem"]["OptionStep1"], 10);
    assert_eq!(out["ResultEquipItem"]["OptionStep5"], 10);
    assert_eq!(gold(&s).await, before_gold - 2 * 768000);
    let dust: i64 = crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=43055")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(dust, before_dust - 2 * 1152);
    let out = call(&s, &u, "max_upgrade_equip_option", &format!("EquipItemSlotIndex={}&MaterialItemIndex=43055&MaterialItemCount=1152", eq.slot_index)).await;
    assert_eq!(out["Result"], "NoUpgradableOptionSlot", "{out}");
    assert_eq!(gold(&s).await, before_gold - 2 * 768000);
}

#[tokio::test]
async fn public_awakening_stones_obey_equipment_type() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let large_gold=7_782_963_002_i64;
    crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2").bind(large_gold).bind(a).execute(&s.db).await.unwrap();
    let eq = give(&s, a, 1001, 1).await.equipment.remove(0);
    give(&s, a, 5301, 1).await;
    let r = call(
        &s,
        &u,
        "awaken_equip",
        &format!(
            "EquipItemSlotIndex={}&MaterialItemInfors=[5301,1]",
            eq.slot_index
        ),
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    let price=-n(&r["CurrencyResult"],"AddValue");
    assert!(price>0,"Awakening must charge its gold price: {r}");
    assert_eq!(r["CurrencyResult"]["NewValue"],large_gold-price);
    assert_eq!(gold(&s).await,large_gold-price);
    assert_eq!(r["ResultEquipItem"]["Star"], 1);
}

#[tokio::test]
async fn awakening_stones_check_the_entire_unique_weapon_roster() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    crate::db::query("UPDATE user_info SET gold=10000000000 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let weapons:Vec<_>=s.tables.extensions.rows("EquipItem").iter()
        .filter(|r|r["IsOpenSoulWeapon"]==true && n(r,"PartType")==1).cloned().collect();
    assert!(weapons.iter().any(|r|n(r,"ItemIndex")==1080));
    for weapon in weapons {
        let id=n(&weapon,"ItemIndex") as i32;
        let npc=weapon["CreatureIndex"].as_array().into_iter().flatten().filter_map(Value::as_i64).any(|i|
            s.tables.extensions.rows("NPCFriendlyPoint").iter().any(|r|n(r,"HeroIndex")==i)
            && s.tables.hero_shop.heroes.get(&(i as i32)).is_some_and(|h|n(h,"OpenType")==1));
        for stone in [5301,5315] {
            let eq=give(&s,a,id,1).await.equipment.remove(0);
            give(&s,a,stone,1).await;
            let out=call(&s,&u,"awaken_equip",&format!("EquipItemSlotIndex={}&MaterialItemInfors=[{stone},1]",eq.slot_index)).await;
            assert_eq!(out["Result"]=="Success",stone==5301 || !npc,"weapon {id}, stone {stone}: {out}");
        }
    }
}
#[tokio::test]
async fn technomagic_migration_preserves_first_skill_in_inventory_and_storage() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let data=s.tables.extensions.rows("EquipItem").iter().find(|v|n(v,"EquipType")==1 && n(v,"EquipGrade")==1).unwrap();
    let id=n(data,"ItemIndex") as i32;
    for storage in [0,1] {
        let eq=give(&s,a,id,1).await.equipment.remove(0);
        crate::db::query("UPDATE equip_items SET extra_option_index_1=123,extra_option_step_1=4,extra_option_index_2=456,extra_option_step_2=7,inventory_type=$1 WHERE slot_index=$2")
            .bind(storage).bind(eq.slot_index).execute(&s.db).await.unwrap();
    }
    for _ in 0..2 {schema::migrate_technomagic(&s.db,&s.tables).await.unwrap();}
    let rows:Vec<(i64,i64,i64,i64)>=crate::db::query_as("SELECT extra_option_index_1,extra_option_step_1,extra_option_index_2,extra_option_step_2 FROM equip_items WHERE item_index=$1 AND account_id=$2")
        .bind(id).bind(a).fetch_all(&s.db).await.unwrap();
    assert_eq!(rows,vec![(123,4,0,0);2]);
    let mut eq=EquipItemInfo::default();
    valance::unique_options(&s,data,&mut eq).unwrap();
    assert!(eq.extra_option_index_1>0);assert_eq!(eq.extra_option_index_2,0);
}
#[tokio::test]
async fn restore_soul_stone_persists_choices_and_rejects_replayed_claims() {
    let (mut s, u) = setup().await;
    assert_eq!(u.misc_info.as_ref().unwrap().extra["RestoreSoulStoneIndices"], "");
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).extensions).0.get_mut("LocalSoulStoneRestore").unwrap()[0]["SoulStoneChancePer10000"] = json!(10000);
    let a = u.user_info.account_id;
    give(&s, a, 45080, 400).await;
    give(&s, a, 5251, 1).await;
    let r = call(&s, &u, "restore_soul_stone", "SoulStoneRestoreIndex=1").await;
    assert_eq!(r["Result"], "Success", "{r}");
    let ids = r["SelectSoulStoneIndices"].as_array().unwrap();
    assert_eq!(ids.len(), 3);
    let login = user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    let misc = login.misc_info.unwrap().extra;
    assert_eq!(misc["SoulStoneMileage"], 0);
    assert_eq!(misc["RestoreSoulStoneIndices"], ids.iter().map(|id| id.as_i64().unwrap().to_string()).collect::<Vec<_>>().join(","));
    assert_ne!(
        call(&s, &u, "restore_soul_stone", "SoulStoneRestoreIndex=1").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "confirm_soul_stone", "SoulStoneItemIndex=1001").await["Result"],
        "Success"
    );
    let args = format!("SoulStoneItemIndex={}", ids[0]);
    assert_eq!(
        call(&s, &u, "confirm_soul_stone", &args).await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "confirm_soul_stone", &args).await["Result"],
        "Success"
    );
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test"))
        .await.unwrap().0;
    assert_eq!(login.misc_info.unwrap().extra["RestoreSoulStoneIndices"], "");
}

#[tokio::test]
async fn soul_judgment_login_without_pending_choices_does_not_show_a_soul_stone() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    // An account with prior judgments still has a restore record, even when
    // there is no Soul Stone left to choose. Preserve its mileage on reconnect.
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "soul_restore",
        0,
        &json!({"Mileage":9,"Choices":[]}),
    )
    .await
    .unwrap();
    let login = user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    let misc = login.misc_info.unwrap().extra;
    assert_eq!(misc["SoulStoneMileage"], 9);
    assert_eq!(misc["RestoreSoulStoneIndices"], "");
}
#[tokio::test]
async fn valance_identification_and_enchantment_restore_pending_choices() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut eq = give(&s, a, 911301, 1).await.equipment.remove(0);
    eq.identified = 0;
    save_equip(&mut s.db.acquire().await.unwrap(), a, &eq)
        .await
        .unwrap();
    let args = format!("EquipItemSlotIndices={}", eq.slot_index);
    let r = call(&s, &u, "valance_identified", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert!(
        r["EquipItemResults"][0]["ExtraOptionIndex1"]
            .as_i64()
            .unwrap()
            > 0
    );
    assert_ne!(
        call(&s, &u, "valance_identified", &args).await["Result"],
        "Success"
    );
    assert_eq!(crate::db::query_scalar::<i64>("SELECT SUM(value)::bigint FROM progression_metrics WHERE account_id=$1 AND kind='IdentifiedEquip'")
        .bind(a).fetch_one(&s.db).await.unwrap(),1);
    let enchant = s
        .tables
        .extensions
        .rows("ValanceEnchant")
        .iter()
        .find(|v| n(v, "PartType") == 3 && n(v, "SubType") == 10)
        .unwrap();
    let enchant_id = n(enchant, "ItemIndex");
    give(&s, a, enchant_id as i32, 2).await;
    let r = call(
        &s,
        &u,
        "valance_enchant",
        &format!(
            "EquipItemSlotIndex={}&ConsumeItemIndex={enchant_id}",
            eq.slot_index
        ),
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["ResultEquipItem"]["EnchantOptionIndex1"], r["NewValanceEnchantOptionInfo"]["EnchantOptionIndex1"]);
    let pending_count: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM equipment_pending WHERE account_id=$1 AND slot_index=$2")
        .bind(a).bind(eq.slot_index).fetch_one(&s.db).await.unwrap();
    assert_eq!(pending_count, 0);
    let r = call(&s, &u, "valance_enchant",
        &format!("EquipItemSlotIndex={}&ConsumeItemIndex={enchant_id}", eq.slot_index)).await;
    assert_eq!(r["Result"], "Success", "{r}");
    let expected = r["NewValanceEnchantOptionInfo"]["EnchantOptionIndex1"].clone();
    let login = user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(
        login.misc_info.unwrap().extra["ValanceEnchantOptionInfo"]["EnchantOptionIndex1"],
        expected
    );
    let r = call(
        &s,
        &u,
        "confirm_valance_enchant",
        &format!("EquipItemSlotIndex={}&IsNew=true", eq.slot_index),
    )
    .await;
    assert_eq!(r["ResultEquipItem"]["EnchantOptionIndex1"], expected, "{r}");
    assert_ne!(
        call(
            &s,
            &u,
            "confirm_valance_enchant",
            &format!("EquipItemSlotIndex={}&IsNew=true", eq.slot_index)
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn team_and_class_buff_upgrades_validate_price_points_and_replays() {
    let (s, u) = setup().await;
    crate::db::query("UPDATE user_info SET team_level=2")
        .execute(&s.db)
        .await
        .unwrap();
    let args="BuffTeamLevel=2&OptionIndex=1&BonusOptionIndex=0&Reinforce=1&ReinforcePrice=100&ReinforcePriceType=Gem";
    assert_ne!(
        call(
            &s,
            &u,
            "reinforce_team_level_buff",
            &args.replace("Price=100", "Price=1")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "reinforce_team_level_buff", args).await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "reinforce_team_level_buff", args).await["Result"],
        "Success"
    );
    crate::db::query("UPDATE heroes SET level=100,star=5,transcend=5 WHERE hero_index=1")
        .execute(&s.db)
        .await
        .unwrap();
    let r = call(&s, &u, "get_class_buff_info", "").await;
    assert_eq!(r["Result"], "Success", "{r}");
    let target = s
        .tables
        .extensions
        .rows("ClassBuff")
        .iter()
        .find(|r| {
            n(r, "TagType") == 1 && n(r, "ClassBuffLevel") == 1 && n(r, "NeedClassBuffIndex") == 0
        })
        .unwrap();
    let args = format!(
        "TagType=1&ClassBuffIndex={}&ClassBuffLevel=1",
        n(target, "ClassBuffIndex")
    );
    let r = call(&s, &u, "level_up_class_buff", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_ne!(
        call(&s, &u, "level_up_class_buff", &args).await["Result"],
        "Success"
    );
}

#[tokio::test]
async fn team_buffs_unlock_on_login_and_lobby_without_resetting_paid_upgrades() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    assert!(u.team_level_buff_infos.is_empty(), "Level one has no buffs");
    crate::db::query("UPDATE user_info SET team_level=10 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let upgraded = json!({"OpenBuffLevel":2,"BuffReinforce":1,"UpdatedTime":"2026-01-01T00:00:00Z"});
    put(&mut *s.db.acquire().await.unwrap(),a,"team_buff",2,&upgraded).await.unwrap();
    let before: (i64,i64) = crate::db::query_as("SELECT gold,gem FROM user_info WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap();
    let login = user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    let expected: BTreeSet<i64> = s.tables.inventory.team_levels.iter()
        .filter(|r| n(r,"Level")<=10 && (n(r,"TeamLevelBuffOptionIndex")>0 || n(r,"TeamLevelBuffBonusOptionIndex")>0))
        .map(|r| n(r,"Level")).collect();
    assert_eq!(login.team_level_buff_infos.iter().map(|v|n(v,"OpenBuffLevel")).collect::<BTreeSet<_>>(),expected);
    for info in &login.team_level_buff_infos {
        if n(info,"OpenBuffLevel")==2 { assert_eq!(info,&upgraded); }
        else { assert_eq!(info["BuffReinforce"],0); }
    }
    let lobby = first_lobby(&s,a).await.unwrap();
    assert_eq!(lobby["TeamLevelBuffInfos"],json!(login.team_level_buff_infos));
    assert_eq!(crate::db::query_as::<(i64,i64)>("SELECT gold,gem FROM user_info WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap(),before);
    assert!(buffs::unlock_team_buffs(&mut *s.db.acquire().await.unwrap(),&s,a).await.unwrap().is_empty());
    // New accounts can bootstrap the same free records through first-lobby too.
    crate::db::query("DELETE FROM extension_state WHERE account_id=$1 AND kind='team_buff'").bind(a).execute(&s.db).await.unwrap();
    assert_eq!(first_lobby(&s,a).await.unwrap()["TeamLevelBuffInfos"].as_array().unwrap().len(),expected.len());
}

#[tokio::test]
async fn team_buff_unlocks_follow_level_up_in_native_response_once() {
    use axum::{Router,routing::post,middleware,body::{Body,to_bytes},http::Request as HttpRequest};
    use tower::ServiceExt;
    let (s,u)=setup().await;
    let app=Router::new().route("/test/team-exp",post(|State(state):State<AppState>,body:Bytes| async move {
        let req=Request::parse(&body)?;
        let a=req.account(&state)?;
        let mut tx=state.db.begin().await?;
        let mut rewards=Rewards::default();
        crate::api::tutorial::team_exp(&mut tx,&state,a,req.number("Amount",0)?,&mut rewards).await?;
        tx.commit().await?;
        Ok::<_,ServerError>(Json(json!({"BaseResult":"Success","Result":"Success","TeamExpResultInfos":rewards.team_exp})))
    })).layer(middleware::from_fn_with_state(s.clone(),crate::api::inventory::hero_level::notify)).with_state(s.clone());
    let exp=s.tables.tutorials.support.team_levels.iter().find(|r|r.level==1).unwrap().local_exp;
    let send=|amount:i64|app.clone().oneshot(HttpRequest::post("/test/team-exp").body(Body::from(format!("SessionKey={}&Amount={amount}",u.user_info.session_key))).unwrap());
    let response=send(exp).await.unwrap();
    let out:Value=serde_json::from_slice(&to_bytes(response.into_body(),1024*1024).await.unwrap()).unwrap();
    assert_eq!(out["TeamExpResultInfos"][0]["NewLevel"],2);
    assert_eq!(out["ReserveTeamLevelBuffInfos"][0]["OpenBuffLevel"],2);
    assert_eq!(out["ReserveTeamLevelBuffInfos"][0]["BuffReinforce"],0);
    assert_eq!(out["ReserveTeamLevelBuffInfos"].as_array().unwrap().len(),1);
    let response=send(0).await.unwrap();
    let out:Value=serde_json::from_slice(&to_bytes(response.into_body(),1024*1024).await.unwrap()).unwrap();
    assert!(out.get("ReserveTeamLevelBuffInfos").is_none(),"Unchanged buffs must not be re-notified");
}
#[tokio::test]
async fn punishment_rune_crafting_equipping_preservation_and_break_are_atomic() {
    // The game sends repeated WWWForm fields; API clients also use JSON arrays.
    for native_form in [false, true] {
        let (s, u) = setup().await;
        let a = u.user_info.account_id;
        for (id, count) in [(45005, 10000), (45123, 1), (45128, 150), (100005, 11)] {
            give(&s, a, id, count).await;
        }
        let ingredients = vec![100005; 10];
        let r = call(
            &s,
            &u,
            "punishment_rune/craft_punishment_rune",
            &format!(
                "PunishmentRuneIndex=100262&{}",
                if native_form {
                    ingredients.iter().map(|id| format!("IngredientRuneIndices={id}")).collect::<Vec<_>>().join("&")
                } else { format!("IngredientRuneIndices={}", json!(ingredients)) }
            ),
        )
        .await;
        assert_eq!(r["Result"], "Success", "{r}");
        let slot = n(&r["EquipItemResult"], "SlotIndex");
        assert!(r["EquipItemResult"]["PunishmentRuneOption"].is_object());
        let args =
            format!("HeroIndex=1&RunePage=1&SlotNum=1&ItemIndex=100262&EquipItemSlotIndex={slot}");
        assert_eq!(call(&s, &u, "equip_rune", &args).await["Result"], "Success");
        let rolled = r["EquipItemResult"]["PunishmentRuneOption"].clone();
        {
            let mut db = s.db.acquire().await.unwrap();
            let cached = crate::api::community::cached_hero(&mut db, a, 1).await.unwrap();
            assert_eq!(cached["PunishmentRuneOptionInfos"], json!([rolled.clone()]));
            let saved = crate::api::services::records::snapshot(&mut db, a, &[1]).await.unwrap();
            assert_eq!(saved["Heroes"][0]["PunishmentRuneOptionInfos"], json!([rolled.clone()]));
        }
        let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test"))
            .await.unwrap().0;
        let reloaded = login.equip_items.iter().find(|e| e.slot_index as i64 == slot).unwrap();
        assert_eq!(reloaded.punishment_rune_option.as_ref(), Some(&rolled));
        assert_eq!(reloaded.inventory_type, 3);
        assert!(login.hero_rune_page_infos.iter().any(|page| page["EquipItemSlotIndex"] == slot));
        let body = Bytes::from(format!(
            "SessionKey={}&EquipItemSlotIndices=[{slot}]",
            u.user_info.session_key
        ));
        assert_ne!(
            item::break_rune(State(s.clone()), body.clone())
                .await
                .unwrap()
                .0["Result"],
            "Success"
        );
        assert_eq!(
            call(
                &s,
                &u,
                "unequip_rune",
                "HeroIndex=1&RunePage=1&SlotNum=1&IsPreserve=true"
            )
            .await["Result"],
            "Success"
        );
        {
            let mut db = s.db.acquire().await.unwrap();
            let cached = crate::api::community::cached_hero(&mut db, a, 1).await.unwrap();
            assert_eq!(cached["PunishmentRuneOptionInfos"], json!([]));
            assert_eq!(punishment::available(&mut db, a, slot).await.unwrap().punishment_rune_option.as_ref(), Some(&rolled));
        }
        let result = item::break_rune(State(s.clone()), body).await.unwrap().0;
        assert_eq!(result["Result"], "Success", "{result}");
        assert!(
            punishment::available(&mut s.db.acquire().await.unwrap(), a, slot)
                .await
                .is_err()
        );
    }
}
#[tokio::test]
async fn flask_fill_and_cancel_never_duplicate_rewards() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 40001, 1).await;
    assert_eq!(
        call(&s, &u, "item/use_flask_item", "HeroIndex=1&ItemIndex=40001").await["Result"],
        "Success"
    );
    let mut tx = s.db.begin().await.unwrap();
    let r = consumables::fill_flask(&mut tx, &s, a, 1, 200)
        .await
        .unwrap()
        .unwrap();
    assert_eq!(r.0["FilledFlaskCount"], 1);
    assert!(consumables::fill_flask(&mut tx, &s, a, 1, 200)
        .await
        .unwrap()
        .is_none());
    tx.commit().await.unwrap();
    assert_ne!(
        call(&s, &u, "item/cancel_flask_item", "HeroIndex=1").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn costume_unlocks_reject_forged_prices_and_survive_reconnect() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let args = "HeroIndex=1&HairCostumeIndex=220114&BuyGem=0&BuyGold=0";
    assert_ne!(
        call(&s, &u, "buy_customizing_costumes", args).await["Result"],
        "Success"
    );
    crate::db::query("INSERT INTO costumes(account_id,costume_index) VALUES($1,114)")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    assert_ne!(
        call(
            &s,
            &u,
            "buy_customizing_costumes",
            &args.replace("BuyGem=0", "BuyGem=1")
        )
        .await["Result"],
        "Success"
    );
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "hair", 220114)
        .await
        .unwrap()
        .is_null());
    let r = call(&s, &u, "buy_customizing_costumes", args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["HairCostumeInfo"]["HairCostumeIndex"], 220114);
    let login = user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(login.hair_costume_infos[0]["HairCostumeIndex"], 220114);
    assert_eq!(
        login
            .heroes
            .iter()
            .find(|h| h.hero_index == 1)
            .unwrap()
            .details["HairCostumeIndex"],
        220114
    );
    let r = call(&s, &u, "reset_all_customizing_costumes", "HeroIndex=1").await;
    assert_eq!(r["HeroCostumeResult"]["HairCostumeIndex"], 0);
}

#[tokio::test]
async fn accessory_sale_charges_existing_price_and_persists_without_double_charge() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let data = row(&s, "AccessoryCostume", &[("Index", 3100013)]).unwrap();
    assert_eq!(data["IsBuy"], true);
    let price = n(data, "ReqBuyGem");
    assert_eq!(price, 500); // Preserve the table price, not the unpriced fallback.
    // JM_NShared_PositionInfo serializes all coordinates as strings.
    let pos = json!({"3100013":{"PositionX":"0","PositionY":"0","PositionZ":"0",
        "RotationX":"0","RotationY":"0","RotationZ":"0","Scale":"1"}});
    // RequestInternal escapes values before WWWForm escapes them again.
    let escaped = urlencoding::encode(&pos.to_string()).into_owned();
    let wire = urlencoding::encode(&escaped);
    let args = format!("HeroIndex=1&AccessoryCostumePositionInfo={wire}&BuyGold=0&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0");
    let wrong = call(&s, &u, "hero/buy_customizing_costumes", &format!("{args}&BuyGem=1")).await;
    assert_ne!(wrong["Result"], "Success");
    let key = cosmetics::accessory_key(1, 3100013).unwrap();
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "accessory", key)
        .await
        .unwrap()
        .is_null());
    let bought = call(
        &s,
        &u,
        "hero/buy_customizing_costumes",
        &format!("{args}&BuyGem={price}"),
    )
    .await;
    assert_eq!(bought["Result"], "Success", "{bought}");
    let again = call(&s, &u, "hero/buy_customizing_costumes", &format!("{args}&BuyGem=0")).await;
    assert_eq!(again["Result"], "Success", "{again}");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.user_info.gem, 100000 - price);
    assert!(login.player_accessory_costume_infos.iter().any(|v| v["HeroIndex"] == 1 && v["AccessoryCostumeIndex"] == 3100013));
    // Previously reward-only accessories are now purchasable at their table price.
    let body_pos = pos.to_string().replace("3100013", "3110025");
    let escaped = urlencoding::encode(&body_pos).into_owned();
    let wire = urlencoding::encode(&escaped);
    let bought = call(&s, &login, "hero/buy_customizing_costumes",
        &format!("HeroIndex=1&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0&AccessoryCostumePositionInfo={wire}&BuyGem=4000&BuyGold=0")).await;
    assert_eq!(bought["Result"], "Success", "{bought}");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.user_info.gem, 100000 - price - 4000);
    assert!(login.player_accessory_costume_infos.iter().any(|v| v["HeroIndex"] == 1 && v["AccessoryCostumeIndex"] == 3110025));
    let selected = hero::change_avatar_hero(State(s.clone()),
        Bytes::from(format!("SessionKey={}&HeroIndex=1&AvatarHeroIndex=11", u.user_info.session_key)))
        .await.unwrap().0;
    assert_eq!(selected["Result"], "Success");
    let part = n(data, "PartType");
    let position_key = format!("PositionInfo{part}");
    let slot_key = format!("AccessoryCostumeIndex{part}");
    let expected = &pos["3100013"];
    assert_eq!(selected["PlayerAvatarHeroInfo"][&slot_key], 3100013);
    assert_eq!(serde_json::from_str::<Value>(selected["PlayerAvatarHeroInfo"][&position_key].as_str().unwrap()).unwrap(), *expected);
    let snapshot = selected["PlayerAvatarHeroInfo"].clone();
    // Position edits change the equipped accessory, never the saved lobby look.
    let mut edited = pos.clone();
    edited["3100013"]["Scale"] = json!("1.5");
    edited["3100013"]["PositionX"] = json!("0.2");
    let encoded = urlencoding::encode(&edited.to_string()).into_owned();
    let edited_result = call(&s, &login, "hero/edit_accessory_costume_position",
        &format!("HeroIndex=1&AccessoryCostumePositionInfo={}", urlencoding::encode(&encoded))).await;
    assert_eq!(edited_result["Result"], "Success");
    assert_eq!(hero::avatar_info(&mut s.db.acquire().await.unwrap(), &s, a).await.unwrap(), snapshot);
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.player_avatar_hero_info, snapshot);
    let equipped = login.player_accessory_costume_infos.iter().find(|v| v["AccessoryCostumeIndex"] == 3100013).unwrap();
    assert_eq!(serde_json::from_str::<Value>(equipped["PositionInfo"].as_str().unwrap()).unwrap(), edited["3100013"]);
    let unset = call(&s, &login, "hero/buy_customizing_costumes",
        "HeroIndex=1&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0&UnsetAccessoryCostumeIndices=3100013&BuyGold=0&BuyGem=0").await;
    assert_eq!(unset["Result"], "Success");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.player_avatar_hero_info, snapshot);
    assert_eq!(hero::info(&mut s.db.acquire().await.unwrap(), a, 1).await.unwrap()[&slot_key], 0);
    assert!(login.player_accessory_costume_infos.iter().any(|v| v["AccessoryCostumeIndex"] == 3100013));
    // Selecting the same portrait again deliberately replaces the snapshot.
    let reselected = hero::change_avatar_hero(State(s.clone()),
        Bytes::from(format!("SessionKey={}&HeroIndex=1&AvatarHeroIndex=11", login.user_info.session_key)))
        .await.unwrap().0;
    assert_eq!(reselected["Result"], "Success");
    let empty_snapshot = reselected["PlayerAvatarHeroInfo"].clone();
    assert_eq!(empty_snapshot[&slot_key], 0);
    assert_eq!(empty_snapshot[&position_key], "");
    let equipped = call(&s, &login, "hero/buy_customizing_costumes", &format!("{args}&BuyGem=0")).await;
    assert_eq!(equipped["Result"], "Success", "{equipped}");
    let reset = call(&s, &login, "hero/reset_all_customizing_costumes", "HeroIndex=1").await;
    assert_eq!(reset["Result"], "Success");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.player_avatar_hero_info, empty_snapshot);
}

#[tokio::test]
async fn multiple_accessories_persist_remove_individually_and_reset_together() {
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    let mut selected = Vec::new();
    for part in 1..=4 {
        let compatible: Vec<_> = s.tables.extensions.rows("AccessoryCostume").iter()
            .filter(|v| n(v,"PartType")==part && v["IsBuy"]==true
                && !v["UnableHero"].as_array().is_some_and(|v| v.contains(&json!(1))))
            .take(2).cloned().collect();
        assert_eq!(compatible.len(),2,"Missing accessories for part {part}");
        selected.extend(compatible);
    }
    let mut total = 0;
    for (index, data) in selected.iter().enumerate() {
        let id=n(data,"Index");
        let position=json!({id.to_string():{"PositionX":(index as f64/10.0).to_string(),"PositionY":"0","PositionZ":"0",
            "RotationX":"0","RotationY":"0","RotationZ":"0","Scale":"1"}});
        let price=n(data,"ReqBuyGem");total+=price;
        let response=call(&s,&u,"hero/buy_customizing_costumes",&format!(
            "HeroIndex=1&BuyGold=0&BuyGem={price}&AccessoryCostumePositionInfo={}",urlencoding::encode(&position.to_string()))).await;
        assert_eq!(response["Result"],"Success","{response}");
        assert_eq!(response["HeroCostumeResultInfo"]["SprkAccessories"].as_array().unwrap().len(),index+1);
        if index == 0 {
            // Simulate an existing installation with only the native scalar slot.
            let mut legacy=hero::details(&mut s.db.acquire().await.unwrap(),account,1).await.unwrap();
            legacy.as_object_mut().unwrap().remove("SprkAccessories");
            hero::save_details(&mut s.db.acquire().await.unwrap(),account,1,&legacy).await.unwrap();
        }
    }
    // A single request may contain multiple accessories for the same part.
    let mut batch=json!({});
    for data in &selected[..2] {batch[n(data,"Index").to_string()]=json!({"PositionX":"0.3","PositionY":"0","PositionZ":"0",
        "RotationX":"0","RotationY":"0","RotationZ":"0","Scale":"1.2"});}
    let reapplied=call(&s,&u,"hero/buy_customizing_costumes",&format!("HeroIndex=1&BuyGold=0&BuyGem=0&AccessoryCostumePositionInfo={}",urlencoding::encode(&batch.to_string()))).await;
    assert_eq!(reapplied["Result"],"Success","{reapplied}");
    assert_eq!(reapplied["HeroCostumeResultInfo"]["SprkAccessories"].as_array().unwrap().len(),8);
    let login=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.user_info.gem,100000-total);
    let saved=hero::info(&mut s.db.acquire().await.unwrap(),account,1).await.unwrap();
    let hero_login=serde_json::to_value(login.heroes.iter().find(|v| v.hero_index==1).unwrap()).unwrap();
    assert_eq!(hero_login["SprkAccessories"],saved["SprkAccessories"]);
    let head=saved["SprkAccessories"].as_array().unwrap().iter().find(|v| n(v,"Index")==n(&selected[0],"Index")).unwrap();
    assert_eq!(serde_json::from_str::<Value>(head["PositionInfo"].as_str().unwrap()).unwrap()["Scale"],"1.2");
    let avatar=hero::change_avatar_hero(State(s.clone()),Bytes::from(format!(
        "SessionKey={}&HeroIndex=1&AvatarHeroIndex=11",u.user_info.session_key))).await.unwrap().0;
    assert_eq!(avatar["PlayerAvatarHeroInfo"]["SprkAccessories"],saved["SprkAccessories"]);
    // Remove the earlier head item; its sibling and every other part survive.
    let id=n(&selected[0],"Index");
    let removed=call(&s,&u,"hero/buy_customizing_costumes",&format!("HeroIndex=1&BuyGold=0&BuyGem=0&UnsetAccessoryCostumeIndices={id}")).await;
    let remaining=removed["HeroCostumeResultInfo"]["SprkAccessories"].as_array().unwrap();
    assert_eq!(remaining.len(),7);
    assert!(!remaining.iter().any(|v| n(v,"Index")==id));
    assert_eq!(removed["HeroCostumeResultInfo"]["AccessoryCostumeIndex1"],selected[1]["Index"]);
    assert_eq!(hero::avatar_info(&mut s.db.acquire().await.unwrap(),&s,account).await.unwrap(),avatar["PlayerAvatarHeroInfo"]);
    let reset=call(&s,&u,"hero/reset_all_customizing_costumes","HeroIndex=1").await;
    assert_eq!(reset["Result"],"Success");
    assert_eq!(reset["HeroCostumeResult"]["SprkAccessories"],json!([]));
    let cleared=hero::info(&mut s.db.acquire().await.unwrap(),account,1).await.unwrap();
    assert_eq!(cleared["SprkAccessories"],json!([]));
    for part in 1..=6 {assert_eq!(cleared[format!("AccessoryCostumeIndex{part}")],0);}
    assert_eq!(list(&mut s.db.acquire().await.unwrap(),account,"accessory").await.unwrap().len(),8);
}

#[tokio::test]
async fn all_accessories_can_be_purchased_with_atomic_price_and_balance_checks() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    crate::db::query("UPDATE user_info SET gem=10000000 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let rows=s.tables.extensions.rows("AccessoryCostume");
    let mut spent=0;
    let mut acquired=0;
    for data in rows {
        if data["UnableHero"].as_array().is_some_and(|v|v.contains(&json!(1))) { continue; }
        let id=n(data,"Index");let price=n(data,"ReqBuyGem");
        assert!(price>0,"Accessory {id} must have an explicit price");
        let position=json!({id.to_string():{"PositionX":"0","PositionY":"0","PositionZ":"0","RotationX":"0","RotationY":"0","RotationZ":"0","Scale":"1"}}).to_string();
        let escaped=urlencoding::encode(&position).into_owned();let wire=urlencoding::encode(&escaped);
        let args=format!("HeroIndex=1&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0&AccessoryCostumePositionInfo={wire}&BuyGold=0");
        let key=cosmetics::accessory_key(1,id).unwrap();
        // A forged quote must roll back the ownership created before payment checks.
        assert_ne!(
            call(
                &s,
                &u,
                "hero/buy_customizing_costumes",
                &format!("{args}&BuyGem=0")
            )
            .await["Result"],
            "Success"
        );
        assert!(get(&mut s.db.acquire().await.unwrap(), a, "accessory", key)
            .await
            .unwrap()
            .is_null());
        let result = call(
            &s,
            &u,
            "hero/buy_customizing_costumes",
            &format!("{args}&BuyGem={price}"),
        )
        .await;
        assert_eq!(result["Result"], "Success", "Accessory {id}: {result}");
        spent += price;
        acquired += 1;
        assert_eq!(
            call(
                &s,
                &u,
                "hero/buy_customizing_costumes",
                &format!("{args}&BuyGem=0")
            )
            .await["Result"],
            "Success"
        );
    }
    assert_eq!(acquired,rows.len());
    let login=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.user_info.gem as i64,10000000-spent);
    assert_eq!(login.player_accessory_costume_infos.len(),acquired);
    // Purchases remain per hero. An empty wallet cannot acquire the same item for hero 2.
    hero::recruit_at(&mut s.db.acquire().await.unwrap(), &s, a, 2, 1, 1, 0)
        .await
        .unwrap();
    let data = rows
        .iter()
        .find(|r| r["PreviewableWhenOwned"] == true && n(r, "PartType") == 1)
        .unwrap();
    let id = n(data, "Index");
    let price = n(data, "ReqBuyGem");
    crate::db::query("UPDATE user_info SET gem=0,pay_gem=0 WHERE account_id=$1")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let position=json!({id.to_string():{"PositionX":"0","PositionY":"0","PositionZ":"0","RotationX":"0","RotationY":"0","RotationZ":"0","Scale":"1"}}).to_string();
    let escaped = urlencoding::encode(&position).into_owned();
    let wire = urlencoding::encode(&escaped);
    assert_eq!(call(&s,&login,"hero/buy_customizing_costumes",&format!("HeroIndex=2&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0&AccessoryCostumePositionInfo={wire}&BuyGold=0&BuyGem={price}")).await["Result"],"NotEnoughGem");
    assert!(get(
        &mut s.db.acquire().await.unwrap(),
        a,
        "accessory",
        cosmetics::accessory_key(2, id).unwrap()
    )
    .await
    .unwrap()
    .is_null());
    assert_eq!(
        crate::db::query_scalar::<i64>("SELECT gem FROM user_info WHERE account_id=$1")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap(),
        0
    );
}
#[tokio::test]
async fn equipment_presets_keep_paid_slots_without_locking_saved_items() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let eq = give(&s, a, 890203, 1).await.equipment.remove(0);
    let args = format!(
        "EquipStorageSlotIndex=1&HeroIndex=1&HeroPartIndex=[2]&EquipItemSlotIndex=[{}]&Name=Armor",
        eq.slot_index
    );
    let r = call(&s, &u, "add_equip_storage_slot", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert!(
        material(&mut s.db.acquire().await.unwrap(), a, eq.slot_index as i64)
            .await
            .is_ok()
    );
    let r = call(
        &s,
        &u,
        "set_equip_storage_slot",
        "EquipStorageSlotIndex=1&HeroIndex=1",
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["HeroInfo"]["EquipItemSlotIndex2"], eq.slot_index);
    let r = call(&s, &u, "buy_equip_storage_slot", "").await;
    assert_eq!(
        r["NewEquipStorageSlotInfo"]["EquipStorageSlotIndex"], 5,
        "{r}"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "reset_equip_storage_slot",
            "EquipStorageSlotIndex=5"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "buy_equip_storage_slot", "").await["NewEquipStorageSlotInfo"]
            ["EquipStorageSlotIndex"],
        6
    );
    let lobby = first_lobby(&s, a).await.unwrap();
    assert_eq!(lobby["EquipPresetInfos"].as_array().unwrap().len(), 6);
}
#[tokio::test]
async fn gear_custom_native_save_load_preserves_pairs_empty_slots_and_hero_gear() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let weapon = give(&s, a, 1001, 1).await.equipment.remove(0);
    let armor = give(&s, a, 890203, 1).await.equipment.remove(0);
    let secondary = give(&s, a, 70004, 1).await.equipment.remove(0);
    let artifact = give(&s, a, 151001, 1).await.equipment.remove(0);
    let accessory = give(&s, a, 81004, 1).await.equipment.remove(0);
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=$1,equip_item_slot_index_3=$2,equip_item_slot_index_7=777 WHERE account_id=$3 AND hero_index=1")
        .bind(weapon.slot_index).bind(accessory.slot_index).bind(a).execute(&s.db).await.unwrap();
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,equip_item_slot_index_2) VALUES ($1,2,2,$2)")
        .bind(a).bind(armor.slot_index).execute(&s.db).await.unwrap();
    let renamed = call(&s, &u, "change_equip_storage_slot_name", "EquipStorageSlotIndex=1&Name=MyGear").await;
    assert_eq!(renamed["Result"], "Success");
    // Native order is 2,4,3,6,5, with repeated form keys and zeros for empty slots.
    let args = format!("EquipStorageSlotIndex=1&HeroIndex=1&HeroPartIndex=2&EquipItemSlotIndex={}&HeroPartIndex=4&EquipItemSlotIndex={}&HeroPartIndex=3&EquipItemSlotIndex=0&HeroPartIndex=6&EquipItemSlotIndex=0&HeroPartIndex=5&EquipItemSlotIndex={}",
        armor.slot_index, secondary.slot_index, artifact.slot_index);
    let saved = call(&s, &u, "add_equip_storage_slot", &args).await;
    assert_eq!(saved["Result"], "Success", "{saved}");
    assert_eq!(saved["EquipPresetInfo"]["Name"], "MyGear");
    let expected = json!([0,armor.slot_index,0,secondary.slot_index,artifact.slot_index,0,0,0,0,0]);
    assert_eq!(saved["EquipPresetInfo"]["EquipItemSlotIndex"], expected);
    assert_eq!(saved["EquipPresetInfo"]["EquipItemUid"][1], armor.uid);
    assert_eq!(saved["EquipPresetInfo"]["EquipItemUid"][3], secondary.uid);
    let loaded = call(&s, &u, "set_equip_storage_slot", "EquipStorageSlotIndex=1&HeroIndex=1").await;
    assert_eq!(loaded["Result"], "Success", "{loaded}");
    let h = &loaded["HeroInfo"];
    for (part, slot) in [(1,weapon.slot_index),(2,armor.slot_index),(3,0),(4,secondary.slot_index),(5,artifact.slot_index),(6,0),(7,777)] {
        assert_eq!(h[format!("EquipItemSlotIndex{part}")], slot, "part {part}");
    }
    assert_eq!(loaded["UnEquippedSlotIndices"], json!([armor.slot_index, accessory.slot_index]));
    assert_eq!(hero::info(&mut s.db.acquire().await.unwrap(), a, 2).await.unwrap()["EquipItemSlotIndex2"],0);
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    let lobby = first_lobby(&s, login.user_info.account_id).await.unwrap();
    assert_eq!(lobby["EquipPresetInfos"][0]["EquipItemSlotIndex"], expected);
    assert_eq!(login.heroes[0].equip_item_slot_index_1, weapon.slot_index);
}

#[tokio::test]
async fn preset_disposal_discards_pending_rolls_and_clears_references_immediately() {
    for action in ["sell", "grind"] {
        let (s, u) = setup().await;
        let a = u.user_info.account_id;
        let saved = give(&s, a, 70004, 1).await.equipment.remove(0);
        let current = give(&s, a, 70004, 1).await.equipment.remove(0);
        let armor = give(&s, a, 890203, 1).await.equipment.remove(0);
        crate::db::query("UPDATE heroes SET equip_item_slot_index_4=$1 WHERE account_id=$2 AND hero_index=1")
            .bind(saved.slot_index).bind(a).execute(&s.db).await.unwrap();
        let saved_result = call(&s, &u, "add_equip_storage_slot", &format!(
            "EquipStorageSlotIndex=1&HeroIndex=1&HeroPartIndex=[2,4]&EquipItemSlotIndex=[{},{}]",
            armor.slot_index, saved.slot_index)).await;
        assert_eq!(saved_result["Result"], "Success", "{saved_result}");
        crate::db::query("UPDATE heroes SET equip_item_slot_index_4=$1 WHERE account_id=$2 AND hero_index=1")
            .bind(current.slot_index).bind(a).execute(&s.db).await.unwrap();
        crate::db::query("INSERT INTO equipment_pending(account_id,slot_index,kind,data) VALUES($1,$2,'enchant','{\"Index\":5088,\"Slot\":1,\"Step\":0}')")
            .bind(a).bind(saved.slot_index).execute(&s.db).await.unwrap();
        let snapshot=json!([{"HeroIndex":1,"EquipItemSlotIndex":[0,0,0,saved.slot_index],"EquipItemUid":["","","",saved.uid],"EquipItemRunePage":[0,0,0,2]}]);
        for owner in [a,a+100] {
            crate::db::query("INSERT INTO hero_presets(account_id,storage_key,data) VALUES($1,'disposal',$2)")
                .bind(owner).bind(snapshot.to_string()).execute(&s.db).await.unwrap();
        }
        let locked=give(&s,a,70004,1).await.equipment.remove(0);
        crate::db::query("UPDATE equip_items SET locked=1 WHERE slot_index=$1").bind(locked.slot_index).execute(&s.db).await.unwrap();
        let rejected=if action=="sell" {
            item::sell_equip(State(s.clone()),Bytes::from(format!("SessionKey={}&EquipItemSlotIndices=[{},{}]",u.user_info.session_key,saved.slot_index,locked.slot_index))).await.unwrap().0
        } else {
            call(&s,&u,"break_equip",&format!("EquipItemSlotIndices=[{},{}]",saved.slot_index,locked.slot_index)).await
        };
        assert_ne!(rejected["Result"],"Success","{rejected}");
        let mut db=s.db.acquire().await.unwrap();
        assert_eq!(get(&mut db,a,"equip_storage",1).await.unwrap()["EquipItemSlotIndex4"],saved.slot_index,"failed batches must roll back preset cleanup");
        assert_eq!(crate::db::query_scalar::<i64>("SELECT count(*) FROM equipment_pending WHERE account_id=$1 AND slot_index=$2").bind(a).bind(saved.slot_index).fetch_one(&mut *db).await.unwrap(),1);
        drop(db);
        let result = if action == "sell" {
            item::sell_equip(State(s.clone()), Bytes::from(format!("SessionKey={}&EquipItemSlotIndices=[{}]", u.user_info.session_key, saved.slot_index)))
                .await.unwrap().0
        } else {
            call(&s, &u, "break_equip", &format!("EquipItemSlotIndices=[{}]", saved.slot_index)).await
        };
        assert_eq!(result["Result"], "Success", "{action}: {result}");
        let mut db = s.db.acquire().await.unwrap();
        assert_eq!(get(&mut db,a,"equip_storage",1).await.unwrap()["EquipItemSlotIndex4"], 0);
        assert_eq!(crate::db::query_scalar::<i64>("SELECT count(*) FROM equipment_pending WHERE account_id=$1 AND slot_index=$2").bind(a).bind(saved.slot_index).fetch_one(&mut *db).await.unwrap(),0);
        for owner in [a,a+100] {
            let value:String=crate::db::query_scalar("SELECT data FROM hero_presets WHERE account_id=$1 AND storage_key='disposal'").bind(owner).fetch_one(&mut *db).await.unwrap();
            let value:Value=serde_json::from_str(&value).unwrap();
            assert_eq!(value[0]["EquipItemSlotIndex"][3],if owner==a {0}else{saved.slot_index});
            assert_eq!(value[0]["EquipItemUid"][3],if owner==a {""}else{saved.uid.as_str()});
            assert_eq!(value[0]["EquipItemRunePage"][3],if owner==a {0}else{2});
        }
        drop(db);
        let loaded = call(&s, &u, "set_equip_storage_slot", "EquipStorageSlotIndex=1&HeroIndex=1").await;
        assert_eq!(loaded["Result"], "Success", "{action}: {loaded}");
        assert_eq!(loaded["HeroInfo"]["EquipItemSlotIndex4"], 0);
        assert_eq!(loaded["HeroInfo"]["EquipItemSlotIndex2"], armor.slot_index);
        assert_eq!(loaded["EquipPresetInfo"]["EquipItemUid"][3], "");
        let mut db = s.db.acquire().await.unwrap();
        assert_eq!(get(&mut db,a,"equip_storage",1).await.unwrap()["EquipItemSlotIndex4"], 0);
        drop(db);
        assert_eq!(call(&s,&u,"set_equip_storage_slot","EquipStorageSlotIndex=1&HeroIndex=1").await["Result"],"Success");
        // A genuine equipped item is still protected from both operations.
        crate::db::query("UPDATE heroes SET equip_item_slot_index_4=$1 WHERE account_id=$2 AND hero_index=1")
            .bind(current.slot_index).bind(a).execute(&s.db).await.unwrap();
        assert_ne!(call(&s,&u,"break_equip",&format!("EquipItemSlotIndices=[{}]", current.slot_index)).await["Result"],"Success");
        assert_ne!(item::sell_equip(State(s.clone()),Bytes::from(format!("SessionKey={}&EquipItemSlotIndices=[{}]",u.user_info.session_key,current.slot_index)))
            .await.unwrap().0["Result"],"Success");
    }
}

#[tokio::test]
async fn deleted_gear_presets_reconcile_on_rename_and_login() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let eq=give(&s,a,70004,1).await.equipment.remove(0);
    for index in [1,2] {
        assert_eq!(call(&s,&u,"add_equip_storage_slot",&format!("EquipStorageSlotIndex={index}&HeroIndex=1&HeroPartIndex=[4]&EquipItemSlotIndex=[{}]",eq.slot_index)).await["Result"],"Success");
    }
    crate::db::query("DELETE FROM equip_items WHERE account_id=$1 AND slot_index=$2").bind(a).bind(eq.slot_index).execute(&s.db).await.unwrap();
    let renamed=call(&s,&u,"change_equip_storage_slot_name","EquipStorageSlotIndex=1&Name=AfterSale").await;
    assert_eq!(renamed["Result"],"Success","{renamed}");
    assert_eq!(renamed["EquipPresetInfo"]["EquipItemSlotIndex"][3],0);
    assert_eq!(renamed["EquipPresetInfo"]["EquipItemUid"][3],"");
    let login=storage_login(&s,a).await.unwrap();
    assert!(login.iter().all(|v|n(v,"EquipItemSlotIndex4")==0));
    let lobby=first_lobby(&s,a).await.unwrap();
    assert_eq!(lobby["EquipPresetInfos"][0]["EquipItemUid"][3],"");
}

#[tokio::test]
async fn gear_custom_invalid_native_pairs_rollback_saved_preset() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let armor = give(&s, a, 890203, 1).await.equipment.remove(0);
    let args = format!("EquipStorageSlotIndex=1&HeroIndex=1&Name=Saved&HeroPartIndex=2&EquipItemSlotIndex={}",armor.slot_index);
    let saved = call(&s, &u, "add_equip_storage_slot", &args).await;
    assert_eq!(saved["Result"], "Success", "{saved}");
    for fields in [
        format!("HeroPartIndex=2&HeroPartIndex=2&EquipItemSlotIndex={}&EquipItemSlotIndex=0",armor.slot_index),
        format!("HeroPartIndex=2&HeroPartIndex=4&EquipItemSlotIndex={}",armor.slot_index),
        format!("HeroPartIndex=2&HeroPartIndex=4&EquipItemSlotIndex={0}&EquipItemSlotIndex={0}",armor.slot_index),
        "HeroPartIndex=2&EquipItemSlotIndex=-1".into(),
        "HeroPartIndex=2&EquipItemSlotIndex=0".into(),
        format!("HeroPartIndex=1&EquipItemSlotIndex={}",armor.slot_index),
        format!("HeroPartIndex=4&EquipItemSlotIndex={}",armor.slot_index),
        "HeroPartIndex=2&EquipItemSlotIndex=2147483647".into(),
    ] {
        let rejected = call(&s, &u, "add_equip_storage_slot", &format!("EquipStorageSlotIndex=1&HeroIndex=1&Name=Changed&{fields}")).await;
        assert_ne!(rejected["Result"], "Success", "{fields}: {rejected}");
        let lobby = first_lobby(&s, a).await.unwrap();
        assert_eq!(lobby["EquipPresetInfos"][0], saved["EquipPresetInfo"]);
    }
    crate::db::query("UPDATE equip_items SET inventory_type=1 WHERE account_id=$1 AND slot_index=$2")
        .bind(a).bind(armor.slot_index).execute(&s.db).await.unwrap();
    let before = hero::info(&mut s.db.acquire().await.unwrap(), a, 1).await.unwrap();
    let rejected = call(&s, &u, "set_equip_storage_slot", "EquipStorageSlotIndex=1&HeroIndex=1").await;
    assert_ne!(rejected["Result"], "Success");
    assert_eq!(hero::info(&mut s.db.acquire().await.unwrap(), a, 1).await.unwrap(),before);
}

#[tokio::test]
async fn npc_gifts_and_claims_validate_step_and_charge_once() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 143098, 1000).await;
    let r = call(
        &s,
        &u,
        "give_gift_item",
        "HeroIndex=90&ItemIndices=[143098]&ItemCount=[1000]",
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["FriendlyInfo"]["FriendlyPoint"], 100000);
    let r = call(&s, &u, "recv_reward_friendly_point", "HeroIndex=90&Step=1").await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_ne!(
        call(&s, &u, "recv_reward_friendly_point", "HeroIndex=90&Step=1").await["Result"],
        "Success"
    );
}

#[tokio::test]
async fn npc_juno_reward_progress_survives_native_login_json() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 143098, 2000).await;
    let gift = call(&s, &u, "npc/give_gift_item",
        "HeroIndex=94&ItemIndices=143098&ItemCount=1000").await;
    assert_eq!(gift["Result"], "Success", "{gift}");
    let recruit = call(&s, &u, "npc/recv_reward_friendly_point", "HeroIndex=94&Step=1").await;
    assert_eq!(recruit["Result"], "Success", "{recruit}");
    assert_eq!(recruit["FriendlyInfo"]["Step"], 2);
    assert!(recruit["HeroInfos"].as_array().unwrap().iter().any(|h| h["HeroIndex"] == 94));

    // Match the case-sensitive field read by JM_NShared_Login_Response, rather
    // than checking only the Rust struct and overlooking the wire contract.
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test"))
        .await.unwrap().0;
    let wire = serde_json::to_value(&login).unwrap();
    let infos = wire["NPCFriendlyInfos"].as_array().expect("native NPCFriendlyInfos login field");
    assert_eq!(infos.iter().find(|info| info["HeroIndex"] == 94).unwrap(), &recruit["FriendlyInfo"]);
    assert!(wire.get("NpcFriendlyInfos").is_none());
    assert_ne!(call(&s, &login, "npc/recv_reward_friendly_point", "HeroIndex=94&Step=1").await["Result"], "Success");
    assert_ne!(call(&s, &login, "npc/recv_reward_friendly_point", "HeroIndex=94&Step=2").await["Result"], "Success");

    let gift = call(&s, &login, "npc/give_gift_item",
        "HeroIndex=94&ItemIndices=143098&ItemCount=1000").await;
    assert_eq!(gift["Result"], "Success", "{gift}");
    let weapon = call(&s, &login, "npc/recv_reward_friendly_point", "HeroIndex=94&Step=2").await;
    assert_eq!(weapon["Result"], "Success", "{weapon}");
    assert!(weapon["EquipItemResults"].as_array().unwrap().iter().any(|e| e["ItemIndex"] == 1094));
    assert_eq!(weapon["FriendlyInfo"]["Step"], 2);
    assert_eq!(weapon["FriendlyInfo"]["EndRewardCount"], 1);
    let again = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test"))
        .await.unwrap().0;
    let wire = serde_json::to_value(again).unwrap();
    assert_eq!(wire["NPCFriendlyInfos"].as_array().unwrap().iter()
        .find(|info| info["HeroIndex"] == 94).unwrap(), &weapon["FriendlyInfo"]);
}

#[tokio::test]
async fn npc_valance_final_reward_repeats_without_invalid_step_or_free_retry() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let cost=n(s.tables.extensions.find("NPCFriendlyPoint",&[("HeroIndex",111),("Step",1)]).unwrap(),"ReqFriendlyPoint");
    put(&mut s.db.acquire().await.unwrap(),a,"npc",111,&json!({"HeroIndex":111,"Step":1,"FriendlyPoint":cost,"EndRewardCount":0})).await.unwrap();
    let claim=call(&s,&u,"recv_reward_friendly_point","HeroIndex=111&Step=1").await;
    assert_eq!(claim["Result"],"Success","{claim}");
    assert_eq!(claim["FriendlyInfo"],json!({"HeroIndex":111,"Step":1,"FriendlyPoint":0,"EndRewardCount":1}));
    assert!(!claim["EquipItemResults"].as_array().unwrap().is_empty());
    assert_ne!(call(&s,&u,"recv_reward_friendly_point","HeroIndex=111&Step=1").await["Result"],"Success");
    give(&s,a,143041,(cost/1000) as i32).await;
    let gift=call(&s,&u,"give_gift_item",&format!("HeroIndex=111&ItemIndices=[143041]&ItemCount=[{}]",cost/1000)).await;
    assert_eq!(gift["Result"],"Success","{gift}");
    let again=call(&s,&u,"recv_reward_friendly_point","HeroIndex=111&Step=1").await;
    assert_eq!(again["Result"],"Success","{again}");
    assert_eq!(again["FriendlyInfo"]["EndRewardCount"],2);
    assert_eq!(again["FriendlyInfo"]["Step"],1);
}

#[tokio::test]
async fn npc_valance_login_repairs_old_claim_once_without_granting_rewards() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let cost=n(s.tables.extensions.find("NPCFriendlyPoint",&[("HeroIndex",111),("Step",1)]).unwrap(),"ReqFriendlyPoint");
    put(&mut s.db.acquire().await.unwrap(),a,"npc",111,&json!({"HeroIndex":111,"Step":2,"FriendlyPoint":cost+9000,"EndRewardCount":0})).await.unwrap();
    let before:i64=crate::db::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap();
    let login=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    let info=login.npc_friendly_infos.iter().find(|r|r["HeroIndex"]==111).unwrap();
    assert_eq!(*info,json!({"HeroIndex":111,"Step":1,"FriendlyPoint":9000,"EndRewardCount":1}));
    let again=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(again.npc_friendly_infos,login.npc_friendly_infos);
    let after:i64=crate::db::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(before,after);
}

#[tokio::test]
async fn npc_limited_final_reward_still_stops_at_its_configured_cap() {
    let (mut s,u)=setup().await;
    let a=u.user_info.account_id;
    let tables=std::sync::Arc::make_mut(&mut s.tables);
    let rows=std::sync::Arc::make_mut(&mut tables.extensions).0.get_mut("NPCFriendlyPoint").unwrap();
    let row=rows.iter_mut().find(|r|r["HeroIndex"]==111 && r["Step"]==1).unwrap();
    row["EndRewardRepeatCount"]=json!(1);
    let cost=n(row,"ReqFriendlyPoint");
    put(&mut s.db.acquire().await.unwrap(),a,"npc",111,&json!({"HeroIndex":111,"Step":1,"FriendlyPoint":cost*2,"EndRewardCount":0})).await.unwrap();
    assert_eq!(call(&s,&u,"recv_reward_friendly_point","HeroIndex=111&Step=1").await["Result"],"Success");
    let saved=get(&mut s.db.acquire().await.unwrap(),a,"npc",111).await.unwrap();
    assert_ne!(call(&s,&u,"recv_reward_friendly_point","HeroIndex=111&Step=1").await["Result"],"Success");
    assert_eq!(get(&mut s.db.acquire().await.unwrap(),a,"npc",111).await.unwrap(),saved);
}
#[tokio::test]
async fn reforge_same_stat_requires_a_strictly_larger_value() {
    let (mut s,u)=setup().await;
    // UT Max HP at 12%: exactly 13% and 14% remain valid for the same option.
    assert_eq!(equipment::reforge_step_range(&s,9002,9002,2,5,true).unwrap(),Some((3,4)));
    assert_eq!(equipment::reforge_step_range(&s,9002,9002,3,5,true).unwrap(),Some((4,4)));
    assert_eq!(equipment::reforge_step_range(&s,9002,9002,4,5,true).unwrap(),None);
    assert_eq!(equipment::reforge_step_range(&s,9002,9002,2,5,false).unwrap(),None);
    // Another HP row has a different range: comparing step numbers is unsafe.
    assert_eq!(equipment::reforge_step_range(&s,209,9002,2,5,true).unwrap(),Some((7,10)));
    assert_eq!(equipment::reforge_step_range(&s,9002,209,9,5,true).unwrap(),Some((4,4)));
    assert_eq!(equipment::reforge_step_range(&s,209,9002,4,5,true).unwrap(),None);
    let different=&s.tables.inventory.options[&9003];
    assert_eq!(equipment::reforge_step_range(&s,9003,9002,4,5,true).unwrap(),Some((0,n(different,"Steps"))));
    let a=u.user_info.account_id;
    let mut eq=give(&s,a,101001,1).await.equipment.remove(0);
    eq.star=5;eq.option_index_1=9002;eq.option_step_1=2;eq.option_index_2=9001;
    save_equip(&mut s.db.acquire().await.unwrap(),a,&eq).await.unwrap();
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).inventory).option_groups.insert(11,vec![9002]);
    let args=format!("EquipItemSlotIndex={}&OptionSlotIndex=1",eq.slot_index);
    let rolled=call(&s,&u,"renew_equip_option",&args).await;
    assert_eq!(rolled["Result"],"Success","{rolled}");
    assert_eq!(rolled["NewOptionIndex"],9002);
    assert!([3,4].contains(&n(&rolled,"NewOptionStep")));
    let accepted=call(&s,&u,"renew_confirm_equip_option",&format!("{args}&IsNew=true&ConfirmOptionIndex=9002")).await;
    assert_eq!(accepted["Result"],"Success","{accepted}");
    assert_eq!(accepted["ResultEquipItem"]["OptionStep1"],rolled["NewOptionStep"]);
    eq=equip(&mut s.db.acquire().await.unwrap(),a,eq.slot_index as i64).await.unwrap();
    eq.option_step_1=4;
    save_equip(&mut s.db.acquire().await.unwrap(),a,&eq).await.unwrap();
    let gems:i64=crate::db::query_scalar("SELECT gem FROM user_info WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap();
    // No valid outcome: preserve balances, counters and pending state.
    let rejected=call(&s,&u,"renew_equip_option",&args).await;
    assert_ne!(rejected["Result"],"Success");
    assert_eq!(crate::db::query_scalar::<i64>("SELECT gem FROM user_info WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap(),gems);
    assert_eq!(json!(equip(&mut s.db.acquire().await.unwrap(),a,eq.slot_index as i64).await.unwrap()),json!(eq));
    assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM equipment_pending WHERE account_id=$1 AND slot_index=$2").bind(a).bind(eq.slot_index).fetch_one(&s.db).await.unwrap(),0);
    // At capped HP, a different stat is the only eligible result.
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).inventory).option_groups.insert(11,vec![9002,9003]);
    let rolled=call(&s,&u,"renew_equip_option",&args).await;
    assert_eq!(rolled["Result"],"Success","{rolled}");
    assert_eq!(rolled["NewOptionIndex"],9003);
    let kept=call(&s,&u,"renew_confirm_equip_option",&format!("{args}&IsNew=false&ConfirmOptionIndex=9002")).await;
    assert_eq!(kept["Result"],"Success","{kept}");
    assert_eq!(kept["ResultEquipItem"]["OptionIndex1"],9002);
    assert_eq!(kept["ResultEquipItem"]["OptionStep1"],4);
    // Rounded values may stay equal across steps; only the first actual gain qualifies.
    let options=&mut std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).inventory).options;
    options.get_mut(&209).unwrap()["MinValues"]=json!(vec![100;6]);
    options.get_mut(&209).unwrap()["MaxValues"]=json!(vec![101;6]);
    assert_eq!(equipment::reforge_step_range(&s,209,9002,0,5,true).unwrap(),Some((10,10)));
}

#[tokio::test]
async fn enchant_same_stat_strictly_improves_and_capped_rolls_are_not_charged() {
    let (mut s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut eq = give(&s, a, 1001, 1).await.equipment.remove(0);
    eq.star = 5;
    give(&s, a, 45021, 20).await;
    {
        let tables = std::sync::Arc::make_mut(&mut s.tables);
        let rows = &mut std::sync::Arc::make_mut(&mut tables.extensions).0;
        rows.get_mut("EquipItem").unwrap().iter_mut()
            .find(|r| n(r, "ItemIndex") == 1001).unwrap()["EnchantOptionItemCodes"] = json!(["ENCHANTSCROLL_RED_3"]);
        let options = &mut std::sync::Arc::make_mut(&mut tables.inventory).options;
        for (id, stat) in [(5011, 8), (5012, 8), (5013, 9)] {
            // Deliberately different star ranges catch accidental use of gear stars.
            options.insert(id, json!({"Type":stat,"Ratio":10,"Steps":60,
                "MinValues":[50,500,500,500,500,500],"MaxValues":[110,1100,1100,1100,1100,1100]}));
        }
    }
    for slot in 1..=3 {
        let mut value = json!(eq);
        value[format!("EnchantOptionIndex{slot}")] = json!(5011);
        value[format!("EnchantOptionStep{slot}")] = json!(45); // 95 / 110
        eq = serde_json::from_value(value).unwrap();
        save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
        {
            let rows = &mut std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).extensions).0;
            let scroll = rows.get_mut("EquipEnchantOptionItem").unwrap().iter_mut()
                .find(|r| n(r, "ItemIndex") == 45021).unwrap();
            for i in 1..=3 { scroll[format!("EnchantOptionIndex{i}")] = json!([]); }
            scroll[format!("EnchantOptionIndex{slot}")] = json!([5012]);
        }
        let args = format!("EquipItemSlotIndex={}&EnchantItemIndex=45021", eq.slot_index);
        for _ in 0..8 {
            let rolled = call(&s, &u, "enchant_equip", &args).await;
            assert_eq!(rolled["Result"], "Success", "{rolled}");
            assert_eq!(rolled["NewEnchantOptionIndex"], 5012);
            assert!((46..=60).contains(&n(&rolled, "NewEnchantOptionStep")), "{rolled}");
            let kept = call(&s, &u, "confirm_equip_enchant_option", &format!(
                "EquipItemSlotIndex={}&IsNew=false&ConfirmEnchantOptionIndex=5011", eq.slot_index)).await;
            assert_eq!(kept["Result"], "Success", "{kept}");
        }
        // Accepting the pending roll persists its exact value.
        give(&s, a, 45021, 20).await;
        let rolled = call(&s, &u, "enchant_equip", &args).await;
        let accepted = call(&s, &u, "confirm_equip_enchant_option", &format!(
            "EquipItemSlotIndex={}&IsNew=true&ConfirmEnchantOptionIndex=5012", eq.slot_index)).await;
        assert_eq!(accepted["Result"], "Success", "{accepted}");
        assert_eq!(accepted["ResultEquipItem"][format!("EnchantOptionStep{slot}")], rolled["NewEnchantOptionStep"]);
        let mut value = accepted["ResultEquipItem"].clone();
        value[format!("EnchantOptionStep{slot}")] = json!(60);
        eq = serde_json::from_value(value).unwrap();
        save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
        let before_gold = gold(&s).await;
        let before_items: i64 = crate::db::query_scalar("SELECT SUM(count)::bigint FROM items WHERE account_id=$1 AND item_index=45021")
            .bind(a).fetch_one(&s.db).await.unwrap();
        let rejected = call(&s, &u, "enchant_equip", &args).await;
        assert_ne!(rejected["Result"], "Success", "{rejected}");
        assert_eq!(gold(&s).await, before_gold);
        let after_items: i64 = crate::db::query_scalar("SELECT SUM(count)::bigint FROM items WHERE account_id=$1 AND item_index=45021")
            .bind(a).fetch_one(&s.db).await.unwrap();
        assert_eq!(before_items, after_items);
        assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM equipment_pending WHERE account_id=$1")
            .bind(a).fetch_one(&s.db).await.unwrap(), 0);
        let rows = &mut std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).extensions).0;
        rows.get_mut("EquipEnchantOptionItem").unwrap().iter_mut()
            .find(|r| n(r, "ItemIndex") == 45021).unwrap()[format!("EnchantOptionIndex{slot}")] = json!([5011,5012,5013]);
        let rolled = call(&s, &u, "enchant_equip", &args).await;
        assert_eq!(rolled["Result"], "Success", "{rolled}");
        assert_eq!(rolled["NewEnchantOptionIndex"], 5013);
        let kept = call(&s, &u, "confirm_equip_enchant_option", &format!(
            "EquipItemSlotIndex={}&IsNew=false&ConfirmEnchantOptionIndex=5012", eq.slot_index)).await;
        assert_eq!(kept["Result"], "Success", "{kept}");
    }
}

#[tokio::test]
async fn technomagic_reforge_charges_materials_for_both_options_without_currency() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    for detail in [9001, 9002, 9003, 9011, 9012, 9013] {
        let data = s.tables.extensions.rows("EquipItem").iter()
            .find(|r| n(r, "DetailIndex") == detail).unwrap();
        let mut eq = give(&s, a, n(data, "ItemIndex") as i32, 1).await.equipment.remove(0);
        eq.identified = 0;
        save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
        let identified = call(&s, &u, "valance_identified",
            &format!("EquipItemSlotIndices={}", eq.slot_index)).await;
        assert_eq!(identified["Result"], "Success", "{identified}");
        let mut current = identified["EquipItemResults"][0].clone();
        let price = row(&s, "EquipItemDetail", &[("DetailIndex", detail)]).unwrap();
        let fragment = n(price, "RenewItemIndex2") as i32;
        give(&s, a, 2200, 2).await;
        give(&s, a, fragment, 600).await;
        let currencies: (i64, i64) = crate::db::query_as("SELECT gold,gem FROM user_info WHERE account_id=$1")
            .bind(a).fetch_one(&s.db).await.unwrap();
        for slot in 1..=2 {
            let args = format!("EquipItemSlotIndex={}&OptionSlotIndex={slot}", eq.slot_index);
            let r = call(&s, &u, "renew_equip_option", &args).await;
            assert_eq!(r["Result"], "Success", "detail={detail}, slot={slot}: {r}");
            assert_eq!(r["ItemResults"].as_array().unwrap().len(), 2);
            assert!(n(&r, "NewOptionIndex") > 0);
            let r = call(&s, &u, "renew_confirm_equip_option",
                &format!("{args}&IsNew=true&ConfirmOptionIndex={}", n(&r, "NewOptionIndex"))).await;
            assert_eq!(r["Result"], "Success", "{r}");
            for other in 1..=2 {
                if other != slot {
                    for field in ["OptionIndex", "OptionStep"] {
                        let key = format!("{field}{other}");
                        assert_eq!(r["ResultEquipItem"][&key], current[&key]);
                    }
                }
                for field in ["ExtraOptionIndex", "ExtraOptionStep"] {
                    let key = format!("{field}{other}");
                    assert_eq!(r["ResultEquipItem"][&key], current[&key]);
                }
            }
            current = r["ResultEquipItem"].clone();
            for id in [2200, fragment] {
                let remaining: i64 = crate::db::query_scalar("SELECT COALESCE(SUM(count)::bigint,0) FROM items WHERE account_id=$1 AND item_index=$2")
                    .bind(a).bind(id).fetch_one(&s.db).await.unwrap();
                assert_eq!(remaining, (2-slot) * if id == 2200 { 1 } else { 300 });
            }
        }
        assert_eq!(crate::db::query_as::<(i64, i64)>("SELECT gold,gem FROM user_info WHERE account_id=$1")
            .bind(a).fetch_one(&s.db).await.unwrap(), currencies);
        // An unfunded retry must leave the item and pending choice unchanged.
        let rejected = call(&s, &u, "renew_equip_option",
            &format!("EquipItemSlotIndex={}&OptionSlotIndex=1", eq.slot_index)).await;
        assert_ne!(rejected["Result"], "Success");
        let saved = equip(&mut s.db.acquire().await.unwrap(), a, eq.slot_index as i64).await.unwrap();
        assert_eq!(json!(saved), current);
        let pending: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM equipment_pending WHERE account_id=$1")
            .bind(a).fetch_one(&s.db).await.unwrap();
        assert_eq!(pending, 0);
    }
}

#[tokio::test]
async fn technomagic_reforge_rolls_back_ticket_when_fragments_are_insufficient() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut eq = give(&s, a, 911301, 1).await.equipment.remove(0);
    eq.identified = 0;
    save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
    let identified = call(&s, &u, "valance_identified",
        &format!("EquipItemSlotIndices={}", eq.slot_index)).await;
    assert_eq!(identified["Result"], "Success", "{identified}");
    let price = row(&s, "EquipItemDetail", &[("DetailIndex", n(meta(&s, eq.item_index).unwrap(), "DetailIndex"))]).unwrap();
    let fragment = n(price, "RenewItemIndex2") as i32;
    give(&s, a, 2200, 1).await;
    give(&s, a, fragment, 299).await;
    let args = format!("EquipItemSlotIndex={}&OptionSlotIndex=2", eq.slot_index);
    let rejected = call(&s, &u, "renew_equip_option", &args).await;
    assert_ne!(rejected["Result"], "Success");
    for (id, expected) in [(2200, 1), (fragment, 299)] {
        assert_eq!(crate::db::query_scalar::<i64>("SELECT COALESCE(SUM(count)::bigint,0) FROM items WHERE account_id=$1 AND item_index=$2")
            .bind(a).bind(id).fetch_one(&s.db).await.unwrap(), expected);
    }
    assert_eq!(json!(equip(&mut s.db.acquire().await.unwrap(), a, eq.slot_index as i64).await.unwrap()),
        identified["EquipItemResults"][0]);
    give(&s, a, fragment, 1).await;
    let retried = call(&s, &u, "renew_equip_option", &args).await;
    assert_eq!(retried["Result"], "Success", "{retried}");
}

#[tokio::test]
async fn rerolls_recover_pending_options_and_reject_forged_confirmation() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let eq = give(&s, a, 62032, 1).await.equipment.remove(0);
    let r = call(
        &s,
        &u,
        "renew_equip_option",
        &format!("EquipItemSlotIndex={}&OptionSlotIndex=1", eq.slot_index),
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    let option = n(&r, "NewOptionIndex");
    let args = format!(
        "EquipItemSlotIndex={}&OptionSlotIndex=1&IsNew=true&ConfirmOptionIndex={option}",
        eq.slot_index
    );
    assert_ne!(
        call(
            &s,
            &u,
            "renew_confirm_equip_option",
            &args.replace(
                &format!("ConfirmOptionIndex={option}"),
                "ConfirmOptionIndex=2147483647"
            )
        )
        .await["Result"],
        "Success"
    );
    let pending = misc(&mut s.db.acquire().await.unwrap(), a).await.unwrap();
    assert_eq!(pending["RenewOptionResultIndex"], option);
    let r = call(&s, &u, "renew_confirm_equip_option", &args).await;
    assert_eq!(r["ResultEquipItem"]["OptionIndex1"], option, "{r}");
    assert_ne!(
        call(&s, &u, "renew_confirm_equip_option", &args).await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn valance_craft_honors_owned_hero_material_discount() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,star) VALUES($1,111,111,5)")
        .bind(a).execute(&s.db).await.unwrap();
    give(&s, a, 990001, 3400).await;
    let r = call(&s, &u, "valance_craft",
        "SetType=Gargoria&CreatureTagType=Knight&PartType=Armor&SubPartType=None&SelectEquipOptionIndices=[]").await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["EquipItemResult"]["Identified"], 0);
    let remaining: i64 = crate::db::query_scalar("SELECT COALESCE(SUM(count)::bigint,0) FROM items WHERE account_id=$1 AND item_index=990001")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(remaining, 0);
    assert_eq!(gold(&s).await, 91000000);
}

#[tokio::test]
async fn valance_craft_selected_skill_is_identified_and_uses_discounted_materials() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,star) VALUES($1,111,111,5)")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    give(&s, a, 990001, 45900).await;
    give(&s, a, 990000, 34).await;
    let r = call(&s, &u, "valance_craft",
        "SetType=Gargoria&CreatureTagType=Knight&PartType=Armor&SubPartType=None&SelectEquipOptionIndices=8101").await;
    assert_eq!(r["Result"], "Success", "{r}");
    let eq = &r["EquipItemResult"];
    assert_eq!(eq["ExtraOptionIndex1"], 8101);
    assert_eq!(
        eq["ExtraOptionStep1"],
        s.tables.inventory.options[&8101]["Steps"]
    );
    assert_eq!(eq["ExtraOptionIndex2"], 0);
    assert_eq!(eq["ExtraOptionStep2"], 0);
    assert_eq!(eq["Identified"], 1);
    let saved = equip(&mut s.db.acquire().await.unwrap(), a, n(eq, "SlotIndex"))
        .await
        .unwrap();
    assert_eq!(json!(saved), *eq);
    assert_eq!(gold(&s).await, 91000000);
    for id in [990001, 990000] {
        let remaining: i64 = crate::db::query_scalar(
            "SELECT COALESCE(SUM(count)::bigint,0) FROM items WHERE account_id=$1 AND item_index=$2",
        )
        .bind(a)
        .bind(id)
        .fetch_one(&s.db)
        .await
        .unwrap();
        assert_eq!(remaining, 0, "material {id}");
    }
    // A paid selection must not be unidentified and rerolled by identification.
    let rejected = call(
        &s,
        &u,
        "valance_identified",
        &format!("EquipItemSlotIndices=[{}]", saved.slot_index),
    )
    .await;
    assert_ne!(rejected["Result"], "Success", "{rejected}");
    let unchanged = equip(
        &mut s.db.acquire().await.unwrap(),
        a,
        saved.slot_index as i64,
    )
    .await
    .unwrap();
    assert_eq!(json!(unchanged), *eq);
    assert_eq!(gold(&s).await, 91000000);
}

#[tokio::test]
async fn valance_craft_selected_skills_work_for_every_selectable_recipe() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    crate::db::query("UPDATE user_info SET gold=1000000000000 WHERE account_id=$1")
        .bind(a).execute(&s.db).await.unwrap();
    for id in [990001, 990002, 990003, 990000] { give(&s, a, id, 100000000).await; }
    let mut covered = BTreeSet::new();
    let mut crafted = 0;
    for recipe in s.tables.extensions.rows("ValanceCraft") {
        let Some(groups) = recipe["EquipOptionGroupIndex"].as_array() else { continue };
        if groups.is_empty() { continue; }
        let group = groups[0].as_i64().unwrap() as i32;
        let chosen = s.tables.inventory.option_groups[&group][0];
        covered.insert((n(recipe, "SetType"), group));
        // Native WWWForm scalar and JSON-array forms must select the same skill.
        for selection in [chosen.to_string(), format!("[{chosen}]")] {
            let before = gold(&s).await;
            let args = format!("SetType={}&CreatureTagType={}&PartType={}&SubPartType={}&SelectEquipOptionIndices={selection}",
                n(recipe,"SetType"), n(recipe,"CreatureTagType"), n(recipe,"EquipPartType"), n(recipe,"EquipItemSubType"));
            let r = call(&s, &u, "valance_craft", &args).await;
            assert_eq!(r["Result"], "Success", "{args}: {r}");
            let eq = &r["EquipItemResult"];
            assert_eq!(eq["Identified"], 1, "{args}");
            assert_eq!(eq["ExtraOptionIndex1"], chosen, "{args}");
            assert_eq!(eq["ExtraOptionStep1"], s.tables.inventory.options[&chosen]["Steps"]);
            assert_eq!(eq["ExtraOptionIndex2"], 0);
            assert_eq!(eq["ExtraOptionStep2"], 0);
            let data = &s.tables.inventory.equipment[&(n(eq,"ItemIndex") as i32)];
            let mut normal = BTreeSet::new();
            normal.extend(data["OptionIndex"].as_array().into_iter().flatten().filter_map(Value::as_i64));
            for g in data["OptionGroupIndex"].as_array().into_iter().flatten().filter_map(Value::as_i64) {
                normal.extend(s.tables.inventory.option_groups[&(g as i32)].iter().map(|v| *v as i64));
            }
            for slot in 1..=n(data,"OptionCount").clamp(0,4) {
                let option = n(eq,&format!("OptionIndex{slot}"));
                assert!(normal.contains(&option), "normal stat replaced by a skill: {args}: {eq}");
                assert_ne!(option, chosen as i64);
            }
            let saved = equip(&mut s.db.acquire().await.unwrap(), a, n(eq,"SlotIndex")).await.unwrap();
            assert_eq!(json!(saved), *eq);
            assert_eq!(gold(&s).await, before - n(recipe,"ReqGold"));
            assert_eq!(r["ItemResults"][0]["AddCount"], -n(recipe,"MaterialItemCount") - n(recipe,"AdditionalMaterialItemCount"));
            assert_eq!(r["ItemResults"][1]["AddCount"], -n(recipe,"SubMaterialItemCount"));
            crate::db::query("DELETE FROM equip_items WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
            crafted += 1;
        }
    }
    assert_eq!(covered, (1..=3).flat_map(|set| (10001..=10004).map(move |group| (set,group))).collect());
    assert_eq!(crafted, 294, "every selectable recipe in both request forms");
    println!("Validated {crafted} selected-skill crafts across every selectable recipe and both request forms");
}

#[tokio::test]
async fn valance_craft_selected_skill_failures_roll_back_all_costs_and_rewards() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 990001, 54000).await;
    give(&s, a, 990000, 40).await;
    let args = "SetType=Gargoria&CreatureTagType=Knight&PartType=Armor&SubPartType=None";
    async fn snapshot(s: &AppState, a: i64) -> (i64, Vec<(i32,i32)>, i64, i64) {
        let materials = crate::db::query_as("SELECT item_index,count FROM items WHERE account_id=$1 ORDER BY item_index,item_id")
            .bind(a).fetch_all(&s.db).await.unwrap();
        let equipment = crate::db::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=$1")
            .bind(a).fetch_one(&s.db).await.unwrap();
        let crafts = crate::db::query_scalar("SELECT COALESCE(SUM(value)::bigint,0) FROM progression_metrics WHERE account_id=$1 AND kind='CraftEquip'")
            .bind(a).fetch_one(&s.db).await.unwrap();
        (gold(s).await, materials, equipment, crafts)
    }
    for selection in ["[8201]", "[101]", "[0]", "[-1]", "[2147483647]", "[8101,8101]", "[8101,8102]", "8101&SelectEquipOptionIndices=8102"] {
        let before = snapshot(&s,a).await;
        let r = call(&s,&u,"valance_craft",&format!("{args}&SelectEquipOptionIndices={selection}")).await;
        assert_ne!(r["Result"], "Success", "{selection}: {r}");
        assert_eq!(snapshot(&s,a).await, before, "{selection}");
    }
    // Valid choices with insufficient gold, base/additional material, or sub-material.
    for (gold, material, sub) in [(8999999,54000,40), (100000000,53999,40), (100000000,54000,39)] {
        crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2").bind(gold).bind(a).execute(&s.db).await.unwrap();
        for (id,count) in [(990001,material),(990000,sub)] {
            crate::db::query("UPDATE items SET count=$1 WHERE account_id=$2 AND item_index=$3")
                .bind(count).bind(a).bind(id).execute(&s.db).await.unwrap();
        }
        let before = snapshot(&s,a).await;
        let r = call(&s,&u,"valance_craft",&format!("{args}&SelectEquipOptionIndices=[8101]")).await;
        assert_ne!(r["Result"], "Success", "{r}");
        assert_eq!(snapshot(&s,a).await, before);
    }
}

#[tokio::test]
async fn valance_craft_uses_recipe_and_preserves_materials_on_failure() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 990001, 1000).await;
    let args = "SetType=1&CreatureTagType=0&PartType=0&SubPartType=20&SelectEquipOptionIndices=[]";
    assert_ne!(
        call(
            &s,
            &u,
            "valance_craft",
            &args.replace("=[]", "=[2147483647]")
        )
        .await["Result"],
        "Success"
    );
    let r = call(&s, &u, "valance_craft", args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["EquipItemResult"]["Identified"], 0);
    assert_eq!(gold(&s).await, 99000000);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT SUM(value)::bigint FROM progression_metrics WHERE account_id=$1 AND kind='CraftEquip'")
        .bind(a).fetch_one(&s.db).await.unwrap(),1);
}
#[tokio::test]
async fn tickets_change_names_and_awakenings_without_replay() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let ticket = *s
        .tables
        .hero_shop
        .items
        .iter()
        .find(|(_, v)| n(v, "Type") == 24)
        .unwrap()
        .0;
    give(&s, a, ticket, 1).await;
    let args = format!("ItemIndex={ticket}&Nick=Extensions");
    let r = call(&s, &u, "item/use_nick_change_item", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_ne!(
        call(
            &s,
            &u,
            "item/use_nick_change_item",
            &args.replace("Extensions", "NewName")
        )
        .await["Result"],
        "Success"
    );
    let nick: String = crate::db::query_scalar("SELECT nick FROM accounts WHERE account_id=$1")
        .bind(a)
        .fetch_one(&s.db)
        .await
        .unwrap();
    assert_eq!(nick, "Extensions");
    let mut source = give(&s, a, 1001, 1).await.equipment.remove(0);
    let target = give(&s, a, 1002, 1).await.equipment.remove(0);
    source.star = 4;
    save_equip(&mut s.db.acquire().await.unwrap(), a, &source)
        .await
        .unwrap();
    give(&s, a, 2210, 1).await;
    let args = format!(
        "ItemIndex=2210&SourceEquipItemSlotIndex={}&TargetEquipItemSlotIndex={}",
        source.slot_index, target.slot_index
    );
    let r = call(&s, &u, "item/awaken_transition", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["TargetEquipItemResult"]["Star"], 4);
    assert_eq!(r["SourceEquipItemResult"]["Star"], 0);
    assert_ne!(
        call(&s, &u, "item/awaken_transition", &args).await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn extra_option_selector_only_accepts_its_equipment_option_pool() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 2840, 1).await;
    let data = meta(&s, 912101).unwrap();
    let group = data["UniqueOptionGroupIndex"][0].as_i64().unwrap() as i32;
    let extra = s.tables.inventory.option_groups[&group][0];
    let mut eq = EquipItemInfo::new(0, 912101, 0, s.server_time_str());
    item::make_options(&s, 912101, &mut eq, &[]).unwrap();
    let options = vec![eq.option_index_1, eq.option_index_2];
    let body = |extra| {
        Bytes::from(format!("SessionKey={}&ItemIndex=2840&ItemCount=1&EquipOptionItemIndex=912101&EquipOptionIndices={}&EquipExtraOptionIndices=[{extra}]",u.user_info.session_key,json!(options)))
    };
    assert_ne!(
        item::use_equip_option_select_item(State(s.clone()), body(2147483647))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let r = item::use_equip_option_select_item(State(s.clone()), body(extra))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success", "{r}");
}
async fn give(s: &AppState, account: i64, id: i32, count: i32) -> Rewards {
    let mut tx = s.db.begin().await.unwrap();
    item::init(&mut tx, s, account).await.unwrap();
    let mut rewards = Rewards::default();
    item::give(
        &mut tx,
        s,
        account,
        ItemGrant {
            index: id,
            count,
            star: 0,
            custom: 0,
        },
        &mut rewards,
    )
    .await
    .unwrap();
    tx.commit().await.unwrap();
    rewards
}
#[tokio::test]
async fn accessory_ownership_is_per_hero_and_compensation_requires_every_eligible_hero() {
    let (mut s, u) = setup().await;
    let a = u.user_info.account_id;
    // A two-hero selector makes the all-owned boundary explicit.
    let tables = std::sync::Arc::make_mut(&mut s.tables);
    let selector = std::sync::Arc::make_mut(&mut tables.extensions)
        .0
        .get_mut("AccessorySelectItem")
        .unwrap()
        .iter_mut()
        .find(|v| n(v, "ItemIndex") == 3100017)
        .unwrap();
    selector["TargetHero"] = json!([1, 2]);
    hero::recruit_at(&mut s.db.acquire().await.unwrap(), &s, a, 2, 1, 1, 0)
        .await
        .unwrap();
    give(&s, a, 3100017, 3).await;
    let position = json!({"PositionX":0,"PositionY":0,"PositionZ":0,"RotationX":0,"RotationY":0,"RotationZ":0,"Scale":1});
    let args = format!("ItemIndex=3100017&AccessoryCostumeIndex=3100017&PositionInfo={position}");
    let r = call(
        &s,
        &u,
        "item/use_accessory_select_item",
        &format!("{args}&HeroIndex=1"),
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    let compensation = format!("{args}&HeroIndex=0&GetOtherReward=true");
    assert_ne!(
        call(&s, &u, "item/use_accessory_select_item", &compensation).await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "item/use_accessory_select_item",
            &format!("{args}&HeroIndex=1")
        )
        .await["Result"],
        "Success"
    );
    let positions = json!({"3100017":position});
    assert_ne!(
        call(
            &s,
            &u,
            "hero/edit_accessory_costume_position",
            &format!("HeroIndex=2&AccessoryCostumePositionInfo={positions}")
        )
        .await["Result"],
        "Success"
    );
    let r = call(
        &s,
        &u,
        "item/use_accessory_select_item",
        &format!("{args}&HeroIndex=2"),
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    let login = user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(login.player_accessory_costume_infos.len(), 2);
    for hero_id in [1, 2] {
        assert_eq!(
            login
                .heroes
                .iter()
                .find(|h| h.hero_index == hero_id)
                .unwrap()
                .details["AccessoryCostumeIndex1"],
            3100017
        );
    }
    let r = call(&s, &u, "item/use_accessory_select_item", &compensation).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_ne!(
        call(&s, &u, "item/use_accessory_select_item", &compensation).await["Result"],
        "Success"
    );
}

async fn gold(s: &AppState) -> i64 {
    crate::db::query_scalar("SELECT gold FROM user_info")
        .fetch_one(&s.db)
        .await
        .unwrap()
}
#[tokio::test]
async fn artifact_awakening_accepts_native_material_forms_and_persists_once() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    for quoted in [false,true] {
        let rewards=give(&s,a,150044,2).await;
        let x=rewards.equipment[0].slot_index;let y=rewards.equipment[1].slot_index;
        let materials=if quoted {format!("[\"{y}\"]")} else {y.to_string()};
        let args=format!("EquipItemSlotIndex={x}&MaterialSlotIndices={materials}");
        let before=gold(&s).await;
        let result=call(&s,&u,"awaken_equip",&args).await;
        assert_eq!(result["Result"],"Success","{result}");
        assert_eq!(result["Success"],true);
        assert_eq!(result["ResultEquipItem"]["Star"],1);
        assert_eq!(gold(&s).await,before-50000);
        let saved:i64=crate::db::query_scalar("SELECT star FROM equip_items WHERE slot_index=$1").bind(x).fetch_one(&s.db).await.unwrap();
        assert_eq!(saved,1);
        assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM equip_items WHERE slot_index=$1").bind(y).fetch_one(&s.db).await.unwrap(),0);
        assert_ne!(call(&s,&u,"awaken_equip",&args).await["Result"],"Success");
        assert_eq!(gold(&s).await,before-50000);
    }
    let target=give(&s,a,1001,1).await.equipment[0].slot_index;
    give(&s,a,5301,1).await;
    let result=call(&s,&u,"awaken_equip",&format!("EquipItemSlotIndex={target}&MaterialItemInfors=5301&MaterialItemInfors=1")).await;
    assert_eq!(result["Result"],"Success","{result}");
    assert_eq!(result["ResultEquipItem"]["Star"],1);
}

async fn assert_awakening_rejects_locked_material(item_index: i32, detail_type: i64, stone_type: i64) {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let data = meta(&s, item_index).unwrap();
    let detail = row(&s, "EquipItemDetail", &[("DetailIndex", n(data, "DetailIndex"))]).unwrap();
    assert_eq!(n(detail, "EquipItemDetailType"), detail_type);
    let price = row(&s, "EquipAwakenPrice", &[
        ("DetailIndex", n(data, "DetailIndex")), ("Tier", n(data, "Tier")), ("Star", 0),
    ]).unwrap();
    assert!(n(price, "ReqGold") > 0);
    let equips = give(&s, a, item_index, 3).await.equipment;
    let target = equips[0].slot_index;
    let unlocked = equips[1].slot_index;
    let locked = equips[2].slot_index;
    crate::db::query("UPDATE equip_items SET locked=1 WHERE account_id=$1 AND slot_index=$2")
        .bind(a).bind(locked).execute(&s.db).await.unwrap();
    let stone = s.tables.extensions.rows("AwakenStone").iter()
        .find(|r| n(r, "Type") == stone_type && r["IsAllowNPC"] == true
            && r["ItemIndices"].as_array().is_none_or(Vec::is_empty))
        .map(|r| n(r, "ItemIndex") as i32);
    assert_eq!(stone.is_some(), stone_type != 0);
    let stone_args = if let Some(id) = stone {
        give(&s, a, id, 2).await;
        format!("&MaterialItemInfors=[{id},1]")
    } else { String::new() };
    let before_gold = gold(&s).await;
    let before_equips: Vec<Value> = crate::db::query("SELECT * FROM equip_items WHERE account_id=$1 ORDER BY slot_index")
        .bind(a).fetch_all(&s.db).await.unwrap().iter()
        .map(|r| json!(EquipItemInfo::from_row(r))).collect();
    let before_items: Vec<(i32, i32, i32)> = crate::db::query_as("SELECT item_index,count,locked FROM items WHERE account_id=$1 ORDER BY item_index")
        .bind(a).fetch_all(&s.db).await.unwrap();

    // Check the actual validation reason, independently of native result mapping.
    let args = format!("EquipItemSlotIndex={target}&MaterialSlotIndices=[{unlocked},{locked}]{stone_args}");
    let req = Request::parse_with_integer_arrays(args.as_bytes(), &["MaterialSlotIndices", "MaterialItemInfors"]).unwrap();
    let mut tx = s.db.begin().await.unwrap();
    let err = equipment::execute(&mut tx, &s, a, &req, "awaken_equip").await.unwrap_err();
    assert!(matches!(err, ServerError::InvalidRequest(ref code) if code == "LockedEquip"), "{err:?}");
    tx.rollback().await.unwrap();

    for materials in [
        locked.to_string(),
        format!("[{unlocked},{locked}]"),
        format!("[\"{locked}\",\"{unlocked}\"]"),
        format!("{unlocked}&MaterialSlotIndices={locked}"),
    ] {
        let out = call(&s, &u, "awaken_equip", &format!(
            "EquipItemSlotIndex={target}&MaterialSlotIndices={materials}{stone_args}"
        )).await;
        // LockedEquip is not mapped to the native LockedEquipItemExists here.
        assert_eq!(out["Result"], "Fail", "item {item_index}: {out}");
        assert_eq!(gold(&s).await, before_gold);
        let after_equips: Vec<Value> = crate::db::query("SELECT * FROM equip_items WHERE account_id=$1 ORDER BY slot_index")
            .bind(a).fetch_all(&s.db).await.unwrap().iter()
            .map(|r| json!(EquipItemInfo::from_row(r))).collect();
        assert_eq!(after_equips, before_equips, "item {item_index}: equipment changed");
        let after_items: Vec<(i32, i32, i32)> = crate::db::query_as("SELECT item_index,count,locked FROM items WHERE account_id=$1 ORDER BY item_index")
            .bind(a).fetch_all(&s.db).await.unwrap();
        assert_eq!(after_items, before_items, "item {item_index}: inventory changed");
    }

    // The same batch must be valid once unlocked, not rejected for another rule.
    crate::db::query("UPDATE equip_items SET locked=0 WHERE account_id=$1 AND slot_index=$2")
        .bind(a).bind(locked).execute(&s.db).await.unwrap();
    let out = call(&s, &u, "awaken_equip", &args).await;
    assert_eq!(out["Result"], "Success", "item {item_index}: {out}");
    assert_eq!(out["ResultEquipItem"]["Star"], 1);
    assert_eq!(gold(&s).await, before_gold - n(price, "ReqGold"));
    let remaining: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=$1 AND slot_index IN ($2,$3)")
        .bind(a).bind(unlocked).bind(locked).fetch_one(&s.db).await.unwrap();
    assert_eq!(remaining, 0);
    if let Some(id) = stone {
        let before = before_items.iter().find(|r| r.0 == id).unwrap().1;
        let after: i32 = crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=$2")
            .bind(a).bind(id).fetch_one(&s.db).await.unwrap();
        assert_eq!(after, before - 1);
    }
}

#[tokio::test]
async fn awakening_locked_artifact_material_preserves_inventory_and_gold() {
    assert_awakening_rejects_locked_material(150044, 12, 0).await;
}

#[tokio::test]
async fn awakening_locked_unique_weapon_material_preserves_inventory_and_gold() {
    assert_awakening_rejects_locked_material(1001, 11, 42).await;
}

#[tokio::test]
async fn awakening_locked_unique_treasure_material_preserves_inventory_and_gold() {
    assert_awakening_rejects_locked_material(102014, 14, 43).await;
}

#[tokio::test]
async fn awakening_validates_materials_and_charges_once() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let r = give(&s, a, 1001, 2).await;
    let x = r.equipment[0].slot_index;
    let y = r.equipment[1].slot_index;
    for materials in [
        format!("[{x}]"),
        format!("[{y},{y}]"),
        "[2147483647]".into(),
    ] {
        let r = call(
            &s,
            &u,
            "awaken_equip",
            &format!("EquipItemSlotIndex={x}&MaterialSlotIndices={materials}"),
        )
        .await;
        assert_ne!(r["Result"], "Success", "{r}");
        assert_eq!(gold(&s).await, 100000000);
    }
    let r = call(
        &s,
        &u,
        "awaken_equip",
        &format!("EquipItemSlotIndex={x}&MaterialSlotIndices=[{y}]"),
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["Success"], true);
    assert_eq!(r["ResultEquipItem"]["Star"], 1);
    assert_eq!(gold(&s).await, 99950000);
    let r = call(
        &s,
        &u,
        "awaken_equip",
        &format!("EquipItemSlotIndex={x}&MaterialSlotIndices=[{y}]"),
    )
    .await;
    assert_ne!(r["Result"], "Success");
    assert_eq!(gold(&s).await, 99950000);
}
#[tokio::test]
async fn soul_weapon_advancement_ticket_unlocks_plain_weapon_and_persists() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let slot = give(&s, a, 1001, 1).await.equipment[0].slot_index;
    give(&s, a, 5350, 2).await;
    let args = format!("ItemIndex=5350&TargetEquipItemSlotIndex={slot}");
    let r = call(&s, &u, "item/soul_weapon_ability_ticket", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["ItemResult"]["AddCount"], -1);
    assert_eq!(r["ItemResult"]["NewCount"], 1);
    assert_eq!(r["ItemRefundsResults"], json!([]), "No investment to refund on a plain UW");
    let soul = &r["TargetSoulWeaponInfo"];
    assert_eq!(soul["Grade"], 1);
    assert_eq!(soul["ReinforceLevel"], 0);
    assert_eq!(soul["OptionStat1"], 8000);
    assert_eq!(soul["OptionStat2"], 250000);
    assert_eq!(gold(&s).await, 100000000);
    let retry = call(&s, &u, "item/soul_weapon_ability_ticket", &args).await;
    assert_eq!(retry["Result"], "AlreadySameOrGreaterGrade");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.soul_weapon_infos, vec![soul.clone()]);
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=5350")
        .bind(a).fetch_one(&s.db).await.unwrap(), 1);
}

#[tokio::test]
async fn soul_weapon_advancement_ticket_preserves_ether_and_refunds_existing_investment() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let slot = give(&s, a, 1001, 1).await.equipment[0].slot_index;
    give(&s, a, 111001, 1).await;
    assert_eq!(call(&s, &u, "soul_weapon_liberation", &format!("EquipItemSlotIndex={slot}")).await["Result"], "Success");
    let mut original = get(&mut s.db.acquire().await.unwrap(), a, "soul", slot as i64).await.unwrap();
    original["ReinforceLevel"] = json!(5);
    original["Exp"] = json!(123);
    original["ReinforceRatio"] = json!(456);
    put(&mut s.db.acquire().await.unwrap(), a, "soul", slot as i64, &original).await.unwrap();
    give(&s, a, 5350, 1).await;
    let args = format!("ItemIndex=5350&TargetEquipItemSlotIndex={slot}");
    let r = call(&s, &u, "item/soul_weapon_ability_ticket", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    for field in ["ReinforceLevel", "Exp", "ReinforceRatio", "OptionRatio1", "OptionRatio2"] {
        assert_eq!(r["TargetSoulWeaponInfo"][field], original[field], "{field}");
    }
    let refunds = r["ItemRefundsResults"].as_array().unwrap();
    assert_eq!(refunds.len(), 1);
    assert_eq!(refunds[0]["AddCount"], 700);
    let refund_id = refunds[0]["ItemIndex"].as_i64().unwrap();
    let before: i32 = crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=$2")
        .bind(a).bind(refund_id).fetch_one(&s.db).await.unwrap();
    assert_ne!(call(&s, &u, "item/soul_weapon_ability_ticket", &args).await["Result"], "Success");
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=$2")
        .bind(a).bind(refund_id).fetch_one(&s.db).await.unwrap(), before);
    // A valid higher upgrade can calculate refunds, then fail ticket consumption.
    // Neither the refund nor the upgraded soul may survive that transaction.
    assert_eq!(call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5364&TargetEquipItemSlotIndex={slot}")).await["Result"], "ItemNotOwned");
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=$2")
        .bind(a).bind(refund_id).fetch_one(&s.db).await.unwrap(), before);
    assert_eq!(get(&mut s.db.acquire().await.unwrap(), a, "soul", slot as i64).await.unwrap(), r["TargetSoulWeaponInfo"]);
}

#[tokio::test]
async fn soul_weapon_advancement_ticket_rejections_leave_inventory_and_souls_unchanged() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let slot = give(&s, a, 1001, 1).await.equipment[0].slot_index;
    give(&s, a, 5350, 1).await;
    let args = format!("ItemIndex=5350&TargetEquipItemSlotIndex={slot}");
    crate::db::query("UPDATE equip_items SET locked=1 WHERE account_id=$1 AND slot_index=$2")
        .bind(a).bind(slot).execute(&s.db).await.unwrap();
    assert_eq!(call(&s, &u, "item/soul_weapon_ability_ticket", &args).await["Result"], "LockedEquipItemExists");
    crate::db::query("UPDATE equip_items SET locked=0 WHERE account_id=$1 AND slot_index=$2")
        .bind(a).bind(slot).execute(&s.db).await.unwrap();
    assert_eq!(call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5352&TargetEquipItemSlotIndex={slot}")).await["Result"], "SoulWeaponInfoNotFound");
    assert_eq!(call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5355&TargetEquipItemSlotIndex={slot}")).await["Result"], "NotAvailableEquipItemindex");
    // With no ticket to consume, creation and any preceding refunds must roll back.
    assert_eq!(call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5364&TargetEquipItemSlotIndex={slot}")).await["Result"], "ItemNotOwned");
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "soul", slot as i64).await.unwrap().is_null());
    assert_eq!(call(&s, &u, "item/soul_weapon_ability_ticket", &args).await["Result"], "Success");
    let duplicate = give(&s, a, 1001, 1).await.equipment[0].slot_index;
    assert_eq!(call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5350&TargetEquipItemSlotIndex={duplicate}")).await["Result"], "IsAlreadyLiberation");
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "soul", duplicate as i64).await.unwrap().is_null());
}

#[tokio::test]
async fn soul_weapon_jumping_ticket_preserves_higher_grade_and_ether_ticket_preserves_grade() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let slot = give(&s, a, 1001, 1).await.equipment[0].slot_index;
    give(&s, a, 5364, 1).await;
    let r = call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5364&TargetEquipItemSlotIndex={slot}")).await;
    assert_eq!(r["Result"], "Success", "{r}");
    give(&s, a, 5357, 1).await;
    let r = call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5357&TargetEquipItemSlotIndex={slot}")).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["TargetSoulWeaponInfo"]["Grade"], 2);
    assert_eq!(r["TargetSoulWeaponInfo"]["ReinforceLevel"], 15);
    give(&s, a, 5356, 1).await;
    let r = call(&s, &u, "item/soul_weapon_ability_ticket",
        &format!("ItemIndex=5356&TargetEquipItemSlotIndex={slot}")).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["TargetSoulWeaponInfo"]["Grade"], 2);
    assert_eq!(r["TargetSoulWeaponInfo"]["ReinforceLevel"], 20);
    assert_eq!(r["TargetSoulWeaponInfo"]["Exp"], 0);
    assert_eq!(r["TargetSoulWeaponInfo"]["ReinforceRatio"], 0);
    assert!(!r["ItemRefundsResults"].as_array().unwrap().is_empty());
}

#[tokio::test]
async fn sanctum_missions_follow_soul_ownership_and_successful_ether_enhancement() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    async fn progress(s: &AppState, a: i64, quest: i64) -> Value {
        let mut db = s.db.acquire().await.unwrap();
        crate::api::progression::snapshot(&mut db, s, a).await.unwrap()["SubQuestInfos"]
            .as_array().unwrap().iter().find(|q| q["SubQuestIndex"] == quest).unwrap()["Progress"].clone()
    }
    async fn claim(s: &AppState, u: &user::LoginResponse, quest: i64) -> Value {
        crate::api::progression::complete_sub_quest(State(s.clone()),
            Bytes::from(format!("SessionKey={}&SubQuestIndices=[{quest}]", u.user_info.session_key)))
            .await.unwrap().0
    }
    let slot = give(&s, a, 1001, 1).await.equipment[0].slot_index;
    let args = format!("EquipItemSlotIndex={slot}");
    assert_eq!(progress(&s, a, 10892).await, 0, "A Unique Weapon alone is not a Soul Weapon");
    assert_ne!(claim(&s, &u, 10892).await["Result"], "Success");
    assert_ne!(call(&s, &u, "soul_weapon_liberation", &args).await["Result"], "Success");
    assert_eq!(progress(&s, a, 10892).await, 0);
    give(&s, a, 111001, 1).await;
    assert_eq!(call(&s, &u, "soul_weapon_liberation", &args).await["Result"], "Success");
    assert_eq!(progress(&s, a, 10892).await, 1);
    assert_eq!(progress(&s, a, 10952).await, 0);
    assert_ne!(claim(&s, &u, 10952).await["Result"], "Success");
    assert_ne!(call(&s, &u, "soul_weapon_reinforce", &args).await["Result"], "Success");
    assert_eq!(progress(&s, a, 10952).await, 0);
    give(&s, a, 5100, 50).await;
    assert_eq!(call(&s, &u, "soul_weapon_ether_injection",
        &format!("{args}&InjectionItemInfos=[5100,50]")).await["Result"], "Success");
    assert_eq!(progress(&s, a, 10952).await, 0, "Injected Ether alone does not satisfy level 1");
    let enhanced = call(&s, &u, "soul_weapon_reinforce", &args).await;
    assert_eq!(enhanced["Result"], "Success", "{enhanced}");
    assert_eq!(enhanced["ReinforceSuccess"], true);
    assert_eq!(progress(&s, a, 10952).await, 1);
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    for quest in [10892, 10952] {
        assert_eq!(progress(&s, a, quest).await, 1);
        assert_eq!(claim(&s, &login, quest).await["Result"], "Success");
        assert_ne!(claim(&s, &login, quest).await["Result"], "Success", "Reward must only be granted once");
    }
}

#[tokio::test]
async fn soul_weapon_ether_accepts_native_arrays_and_rolls_back_failed_injections() {
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    let slot = give(&s, account, 1001, 1).await.equipment[0].slot_index;
    give(&s, account, 111001, 1).await;
    assert_eq!(call(&s, &u, "soul_weapon_liberation", &format!("EquipItemSlotIndex={slot}")).await["Result"], "Success");
    give(&s, account, 5100, 10).await;
    give(&s, account, 5103, 10).await;
    let original = get(&mut s.db.acquire().await.unwrap(), account, "soul", slot as i64).await.unwrap();
    let initial_gold = gold(&s).await;
    let inventory = || async {
        crate::db::query_as::<(i32, i32)>("SELECT item_index,count FROM items WHERE account_id=$1 AND item_index IN (5100,5103) ORDER BY item_index")
            .bind(account).fetch_all(&s.db).await.unwrap()
    };
    let initial_inventory = inventory().await;
    for materials in [
        "InjectionItemInfos=5100&InjectionItemInfos=1&InjectionItemInfos=5103", // missing count
        "InjectionItemInfos=[5100,1,5100,1]", // duplicate ether
        "InjectionItemInfos=[5100,0]",
        "InjectionItemInfos=[5100,-1]",
        "InjectionItemInfos=[5100,100001]",
        "InjectionItemInfos=[5100,1,5103,11]", // first debit must roll back
        "InjectionItemInfos=[5100,1,99999999,1]",
    ] {
        let response = call(&s, &u, "soul_weapon_ether_injection", &format!("EquipItemSlotIndex={slot}&{materials}")).await;
        assert_ne!(response["Result"], "Success", "{response}");
        assert_eq!(gold(&s).await, initial_gold);
        assert_eq!(inventory().await, initial_inventory);
        assert_eq!(get(&mut s.db.acquire().await.unwrap(), account, "soul", slot as i64).await.unwrap(), original);
    }
    for materials in [
        "InjectionItemInfos=5100&InjectionItemInfos=1&InjectionItemInfos=5103&InjectionItemInfos=1",
        "InjectionItemInfos=[5100,1,5103,1]",
        "InjectionItemInfos=[\"5100\",\"1\",\"5103\",\"1\"]",
    ] {
        let before = gold(&s).await;
        let response = call(&s, &u, "soul_weapon_ether_injection", &format!("EquipItemSlotIndex={slot}&{materials}")).await;
        assert_eq!(response["Result"], "Success", "{response}");
        assert_eq!(response["CurrencyResult"]["AddValue"], -480000);
        assert_eq!(gold(&s).await, before - 480000);
        assert_eq!(response["RemovedItemResultInfos"].as_array().unwrap().len(), 2);
        for removed in response["RemovedItemResultInfos"].as_array().unwrap() {
            assert_eq!(removed["AddCount"], -1);
        }
    }
    let saved = get(&mut s.db.acquire().await.unwrap(), account, "soul", slot as i64).await.unwrap();
    assert_eq!(saved["Exp"], 330);
    assert_eq!(inventory().await, vec![(5100, 7), (5103, 7)]);
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.soul_weapon_infos[0]["Exp"], 330);
}

#[tokio::test]
async fn soul_liberation_is_atomic_and_equipment_cannot_be_sacrificed() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let slot = give(&s, a, 1001, 1).await.equipment[0].slot_index;
    let args = format!("EquipItemSlotIndex={slot}");
    assert_ne!(
        call(&s, &u, "soul_weapon_liberation", &args).await["Result"],
        "Success"
    );
    assert_eq!(gold(&s).await, 100000000);
    give(&s, a, 111001, 1).await;
    let r = call(&s, &u, "soul_weapon_liberation", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(gold(&s).await, 90000000);
    assert_ne!(
        call(&s, &u, "soul_weapon_liberation", &args).await["Result"],
        "Success"
    );
    assert_eq!(gold(&s).await, 90000000);
    assert!(material(&mut s.db.acquire().await.unwrap(), a, slot as i64)
        .await
        .is_err());
    let mut stored = get(&mut s.db.acquire().await.unwrap(), a, "soul", slot as i64)
        .await.unwrap();
    stored["OptionStat1"] = json!(123456789);
    stored["OptionStat2"] = json!(123456789);
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "soul",
        slot as i64,
        &stored,
    )
    .await
    .unwrap();
    let login = user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(login.soul_weapon_infos.len(), 1);
    // Build-1558 coefficients need the original /1000 Soul Weapon formula.
    // Login refreshes cached stats without changing saved progression.
    assert_eq!(login.soul_weapon_infos[0]["OptionStat1"], 4000);
    assert_eq!(login.soul_weapon_infos[0]["OptionStat2"], 125000);
    assert_eq!(
        get(&mut s.db.acquire().await.unwrap(), a, "soul", slot as i64)
            .await
            .unwrap(),
        stored
    );
}
#[tokio::test]
async fn soul_weapon_max_stats_match_archived_native_units() {
    let (s, _) = setup().await;
    // Native 4.86.6 ARM64 GetSoulWeaponBaseStat at 0x30d6b38, using
    // build-1558 Knight coefficients and a real A2/E20 allocation.
    let mut info = json!({"ItemIndex":1001,"Grade":2,"ReinforceLevel":20,
        "OptionRatio1":699,"OptionRatio2":301,
        "OptionBonusRatio1":171,"OptionBonusRatio2":170});
    soul::stats(&s, &mut info).unwrap();
    assert_eq!(info["OptionStat1"], 159483);
    assert_eq!(info["BonusOptionStat1"], 27271);
    assert_eq!(info["OptionStat2"], 2146130);
    assert_eq!(info["BonusOptionStat2"], 364842);
}
#[tokio::test]
async fn all_extended_equipment_columns_survive_database_reload() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut eq = give(&s, a, 1001, 1).await.equipment.remove(0);
    eq.enchant_option_index_1 = 1234;
    eq.enchant_option_step_1 = 3;
    eq.extra_option_index_1 = 5678;
    eq.option_index_5 = 7654;
    let mut db = s.db.acquire().await.unwrap();
    save_equip(&mut db, a, &eq).await.unwrap();
    let loaded = equip(&mut db, a, eq.slot_index as i64).await.unwrap();
    assert_eq!(json!(loaded), json!(eq));
}
#[tokio::test]
async fn rune_pages_consume_preserve_and_restore_without_duplication() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 100001, 1).await;
    let args = "HeroIndex=1&RunePage=1&SlotNum=1&ItemIndex=100001&EquipItemSlotIndex=0";
    let r = call(&s, &u, "equip_rune", args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    let r = call(
        &s,
        &u,
        "unequip_rune",
        "HeroIndex=1&RunePage=1&SlotNum=1&IsPreserve=true",
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    let count: i32 =
        crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=100001")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let r = call(
        &s,
        &u,
        "unequip_rune",
        "HeroIndex=1&RunePage=1&SlotNum=1&IsPreserve=true",
    )
    .await;
    assert_ne!(r["Result"], "Success", "{r}");
}

#[tokio::test]
async fn locked_rune_equips_and_preserves_inventory_lock() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 100001, 2).await;
    let lock = item::set_lock_rune_item(State(s.clone()), Bytes::from(format!(
        "SessionKey={}&RuneItemIndex=100001&Locked=1", u.user_info.session_key
    ))).await.unwrap().0;
    assert_eq!(lock["Result"], "Success");
    let args = "HeroIndex=1&RunePage=1&SlotNum=1&ItemIndex=100001&EquipItemSlotIndex=0";
    let equipped = call(&s, &u, "equip_rune", args).await;
    assert_eq!(equipped["Result"], "Success", "{equipped}");
    assert_eq!(equipped["ItemResults"][0]["Locked"], 1);
    assert_eq!(equipped["ItemResults"][0]["NewCount"], 1);
    let locked: (i32,i32) = crate::db::query_as("SELECT count,locked FROM items WHERE account_id=$1 AND item_index=100001")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(locked, (1,1));
    let break_request=Bytes::from(format!("SessionKey={}&ItemIndices=[100001]&Counts=[1]",u.user_info.session_key));
    let break_result=item::break_rune(State(s.clone()), break_request).await.unwrap().0;
    assert_ne!(break_result["Result"], "Success");
    let relogin=user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert!(relogin.hero_rune_page_infos.iter().any(|r| r["HeroIndex"]==1 && r["SlotNum"]==1 && r["ItemIndex"]==100001));
    assert_eq!(relogin.items.iter().find(|r|r.item_index==100001).unwrap().locked,1);
    let restored=call(&s,&u,"unequip_rune","HeroIndex=1&RunePage=1&SlotNum=1&IsPreserve=true").await;
    assert_eq!(restored["Result"],"Success","{restored}");
    let locked: (i32,i32) = crate::db::query_as("SELECT count,locked FROM items WHERE account_id=$1 AND item_index=100001")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(locked, (2,1));
    assert_eq!(restored["ItemResults"][0]["Locked"],1);
    // The lock must survive even when equipping the last copy leaves a
    // zero-count inventory row until the rune is preserved and returned.
    crate::db::query("UPDATE items SET count=1 WHERE account_id=$1 AND item_index=100001").bind(a).execute(&s.db).await.unwrap();
    let last=call(&s,&u,"equip_rune",args).await;
    assert_eq!(last["Result"],"Success","{last}");
    assert_eq!(last["ItemResults"][0]["NewCount"],0);
    assert_eq!(last["ItemResults"][0]["Locked"],1);
    let returned=call(&s,&u,"unequip_rune","HeroIndex=1&RunePage=1&SlotNum=1&IsPreserve=true").await;
    assert_eq!(returned["Result"],"Success","{returned}");
    let locked: (i32,i32)=crate::db::query_as("SELECT count,locked FROM items WHERE account_id=$1 AND item_index=100001").bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(locked,(1,1));
}

#[tokio::test]
async fn locked_punishment_rune_equips_once_and_retains_lock_and_rolls() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    for (id,count) in [(45005,10000),(45123,1),(45128,150),(100005,11)] {give(&s,a,id,count).await;}
    let ingredients=vec![100005;10];
    let crafted=call(&s,&u,"punishment_rune/craft_punishment_rune",&format!("PunishmentRuneIndex=100262&IngredientRuneIndices={}",json!(ingredients))).await;
    assert_eq!(crafted["Result"],"Success","{crafted}");
    let slot=n(&crafted["EquipItemResult"],"SlotIndex");
    let rolled=crafted["EquipItemResult"]["PunishmentRuneOption"].clone();
    crate::db::query("UPDATE equip_items SET locked=1 WHERE account_id=$1 AND slot_index=$2").bind(a).bind(slot).execute(&s.db).await.unwrap();
    let args=format!("HeroIndex=1&RunePage=1&SlotNum=1&ItemIndex=100262&EquipItemSlotIndex={slot}");
    let result=call(&s,&u,"equip_rune",&args).await;
    assert_eq!(result["Result"],"Success","{result}");
    let duplicate=call(&s,&u,"equip_rune",&format!("HeroIndex=1&RunePage=1&SlotNum=2&ItemIndex=100262&EquipItemSlotIndex={slot}")).await;
    assert_eq!(duplicate["Result"],"AlreadyEquippedRune","{duplicate}");
    let same_slot=call(&s,&u,"equip_rune",&args).await;
    assert_eq!(same_slot["Result"],"AlreadyEquippedRune","{same_slot}");
    let destroy=call(&s,&u,"unequip_rune","HeroIndex=1&RunePage=1&SlotNum=1&IsPreserve=false").await;
    assert_ne!(destroy["Result"],"Success");
    let login=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    let loaded=login.equip_items.iter().find(|r|r.slot_index as i64==slot).unwrap();
    assert_eq!(loaded.locked,1);assert_eq!(loaded.punishment_rune_option.as_ref(),Some(&rolled));
    let restored=call(&s,&u,"unequip_rune","HeroIndex=1&RunePage=1&SlotNum=1&IsPreserve=true").await;
    assert_eq!(restored["Result"],"Success","{restored}");
    let locked:i32=crate::db::query_scalar("SELECT locked FROM equip_items WHERE account_id=$1 AND slot_index=$2").bind(a).bind(slot).fetch_one(&s.db).await.unwrap();
    assert_eq!(locked,1);
    let request=Bytes::from(format!("SessionKey={}&EquipItemSlotIndices=[{slot}]",u.user_info.session_key));
    assert_ne!(item::break_rune(State(s.clone()),request).await.unwrap().0["Result"],"Success");
}

#[test]
fn soul_judgment_reward_group_matches_pre_doomsday_archive() {
    let tables = crate::tables::test_tables();
    let config = tables
        .extensions
        .find("LocalSoulStoneRestore", &[("Index", 1)])
        .unwrap();
    assert_eq!(config["SoulStoneChancePer10000"], 2000);
    assert_eq!(config["EtherItemGroup"], "SOUL_APPRISE_GROUP[2:3:4]");
    // Archived build1558 ItemGroupTable, also referenced by RewardTable 65011.
    let group = tables.item_groups.get("SOUL_APPRISE_GROUP").unwrap();
    assert_eq!(group.total_rate, 1000);
    let actual: Vec<_> = group
        .sub_data_list
        .iter()
        .map(|r| (r.item_code.as_str(), r.rate, r.item_min, r.item_max))
        .collect();
    assert_eq!(
        actual,
        vec![
            ("SOULSTONE_RANDOM_ITEM_GROUP", 200, 1, 1),
            ("SOULWEAPON_ETHER_2", 250, 15, 15),
            ("SOULWEAPON_ETHER_3", 300, 10, 10),
            ("SOULWEAPON_ETHER_4", 250, 5, 5),
        ]
    );
}

#[tokio::test]
async fn soul_judgment_grants_each_archived_ether_tier_and_quantity() {
    for (code, count) in [
        ("SOULWEAPON_ETHER_2", 15),
        ("SOULWEAPON_ETHER_3", 10),
        ("SOULWEAPON_ETHER_4", 5),
    ] {
        let (mut s, u) = setup().await;
        let a = u.user_info.account_id;
        let tables = std::sync::Arc::make_mut(&mut s.tables);
        std::sync::Arc::make_mut(&mut tables.extensions)
            .0
            .get_mut("LocalSoulStoneRestore")
            .unwrap()[0]["SoulStoneChancePer10000"] = json!(0);
        // Force each real group outcome, retaining its native item/count and
        // the production Ether filter. No probabilistic assertions are needed.
        let group = std::sync::Arc::make_mut(&mut tables.item_groups)
            .entries
            .get_mut("SOUL_APPRISE_GROUP")
            .unwrap();
        for row in &mut group.sub_data_list {
            row.rate = if row.item_code == code { 1 } else { 0 };
        }
        group.total_rate = 1;
        let index = s.tables.get_item_index(code).unwrap();
        give(&s, a, 45080, 200).await;
        give(&s, a, 5251, 1).await;
        let before: i64 = crate::db::query_scalar(
            "SELECT COALESCE((SELECT count FROM items WHERE account_id=$1 AND item_index=$2),0)",
        )
        .bind(a)
        .bind(index)
        .fetch_one(&s.db)
        .await
        .unwrap();
        let out = call(&s, &u, "restore_soul_stone", "SoulStoneRestoreIndex=1").await;
        assert_eq!(out["Result"], "Success", "{out}");
        assert_eq!(out["ItemResults"].as_array().unwrap().len(), 1);
        assert_eq!(out["ItemResults"][0]["ItemIndex"], index);
        assert_eq!(out["ItemResults"][0]["AddCount"], count);
        assert_eq!(out["ItemResults"][0]["NewCount"], before + count);
        assert_eq!(out["SoulStoneMileage"], 1);
        assert_eq!(out["SelectSoulStoneIndices"], json!([]));
        assert_eq!(out["RemovedItemResultInfos"].as_array().unwrap().len(), 1);
        assert_eq!(out["RemovedItemResultInfos"][0]["ItemIndex"], 45080);
        let after: i64 =
            crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=$2")
                .bind(a)
                .bind(index)
                .fetch_one(&s.db)
                .await
                .unwrap();
        assert_eq!(after, before + count);
        let protection: i64 =
            crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=5251")
                .bind(a)
                .fetch_one(&s.db)
                .await
                .unwrap();
        assert_eq!(protection, 1);
    }
}

#[tokio::test]
async fn soul_judgment_ether_mileage_and_protection_follow_native_flow() {
    let (mut s, u) = setup().await;
    let a = u.user_info.account_id;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).extensions)
        .0
        .get_mut("LocalSoulStoneRestore")
        .unwrap()[0]["SoulStoneChancePer10000"] = json!(0);
    give(&s, a, 45080, 400).await;
    give(&s, a, 5251, 1).await;
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "soul_restore",
        0,
        &json!({"Mileage":19,"Choices":[]}),
    )
    .await
    .unwrap();
    let args = "SoulStoneRestoreIndex=1";
    let ether = call(&s, &u, "restore_soul_stone", args).await;
    assert_eq!(ether["Result"], "Success", "{ether}");
    assert_eq!(ether["SoulStoneMileage"], 20);
    assert_eq!(ether["SelectSoulStoneIndices"], json!([]));
    let ether_index = n(&ether["ItemResults"][0], "ItemIndex");
    let expected = [
        ("SOULWEAPON_ETHER_2", 15),
        ("SOULWEAPON_ETHER_3", 10),
        ("SOULWEAPON_ETHER_4", 5),
    ]
    .into_iter()
    .find(|(code, _)| s.tables.get_item_index(code).map(i64::from) == Some(ether_index))
    .unwrap();
    assert_eq!(ether["ItemResults"][0]["AddCount"], expected.1);
    let protection: i64 =
        crate::db::query_scalar("SELECT count FROM items WHERE account_id=$1 AND item_index=5251")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(protection, 1);
    assert_ne!(
        call(&s, &u, "restore_soul_stone", args).await["Result"],
        "Success"
    );
    let bonus = call(
        &s,
        &u,
        "get_soul_stone_mileage_reward",
        "SoulStoneRestoreIndex=0",
    )
    .await;
    assert_eq!(bonus["Result"], "Success", "{bonus}");
    assert_eq!(bonus["SoulStoneMileage"], 0);
    assert_eq!(bonus["SelectSoulStoneIndices"].as_array().unwrap().len(), 3);
    let choice = format!("SoulStoneItemIndex={}", bonus["SelectSoulStoneIndices"][0]);
    assert_eq!(
        call(&s, &u, "confirm_soul_stone", &choice).await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "get_soul_stone_mileage_reward",
            "SoulStoneRestoreIndex=0"
        )
        .await["Result"],
        "Success"
    );
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).extensions)
        .0
        .get_mut("LocalSoulStoneRestore")
        .unwrap()[0]["SoulStoneChancePer10000"] = json!(10000);
    let direct = call(&s, &u, "restore_soul_stone", args).await;
    assert_eq!(direct["Result"], "Success", "{direct}");
    assert_eq!(direct["SelectSoulStoneIndices"], json!([]));
    assert_eq!(direct["ItemResults"].as_array().unwrap().len(), 1);
    assert_eq!(direct["SoulStoneMileage"], 0);
}

#[tokio::test]
async fn native_equipment_grind_single_and_batch_persist_rewards_and_removal() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    for number in [1,3,150] {
        let equipment=give(&s,a,1001,number).await.equipment;
        assert_eq!(equipment.len(),number as usize);
        let args=equipment.iter().map(|eq|format!("EquipItemSlotIndices={}",eq.slot_index)).collect::<Vec<_>>().join("&");
        let r=call(&s,&u,"break_equip",&args).await;
        assert_eq!(r["Result"],"Success","{r}");
        assert!(!r["ItemResults"].as_array().unwrap().is_empty());
        let again=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
        assert!(again.equip_items.iter().all(|eq|equipment.iter().all(|e|e.slot_index!=eq.slot_index)));
        let final_counts:std::collections::BTreeMap<_,_> = r["ItemResults"].as_array().unwrap().iter()
            .map(|reward|(n(reward,"ItemIndex"),n(reward,"NewCount"))).collect();
        for (id,count) in final_counts {
            assert_eq!(again.items.iter().find(|i|i.item_index as i64==id).unwrap().count as i64,count);
        }
    }
}

#[tokio::test]
async fn native_equipment_grind_rejects_locked_duplicates_and_rolls_back_entire_batch() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let equipment=give(&s,a,1001,2).await.equipment;
    let first=equipment[0].slot_index;
    let second=equipment[1].slot_index;
    crate::db::query("UPDATE equip_items SET locked=1 WHERE account_id=$1 AND slot_index=$2").bind(a).bind(second).execute(&s.db).await.unwrap();
    let before:i64=crate::db::query_scalar("SELECT COALESCE(SUM(count)::bigint,0) FROM items WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap();
    let result=call(&s,&u,"break_equip",&format!("EquipItemSlotIndices={first}&EquipItemSlotIndices={second}")).await;
    assert_eq!(result["Result"],"LockedEquipItemExists","{result}");
    let result=call(&s,&u,"break_equip",&format!("EquipItemSlotIndices={first}&EquipItemSlotIndices={first}")).await;
    assert_ne!(result["Result"],"Success");
    assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM equip_items WHERE account_id=$1 AND slot_index IN ($2,$3)").bind(a).bind(first).bind(second).fetch_one(&s.db).await.unwrap(),2);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT COALESCE(SUM(count)::bigint,0) FROM items WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap(),before);
}

#[tokio::test]
async fn accessory_items_accept_native_encoded_positions_and_survive_relogin() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let position = json!({"PositionX":"0", "PositionY":"0", "PositionZ":"0", "RotationX":"0", "RotationY":"0", "RotationZ":"0", "Scale":"1"});
    // RequestInternal escapes strings, then WWWForm escapes them again.
    let raw = position.to_string();
    let escaped = urlencoding::encode(&raw).into_owned();
    let wire = urlencoding::encode(&escaped);
    let mut used = 0;
    let mut granted = std::collections::HashSet::new();
    for selector in s.tables.extensions.rows("AccessorySelectItem") {
        let accessory = n(selector, "AccessoryIndex");
        let data = row(&s, "AccessoryCostume", &[("Index", accessory)]).unwrap();
        if selector["TargetHero"].as_array().is_some_and(|v| !v.is_empty() && !v.contains(&json!(1)))
            || data["UnableHero"].as_array().is_some_and(|v| v.contains(&json!(1)))
            || !granted.insert(accessory) { continue; }
        let id = n(selector, "ItemIndex") as i32;
        give(&s, a, id, 2).await;
        let args = format!("ItemIndex={id}&AccessoryCostumeIndex={accessory}&HeroIndex=1&PositionInfo={wire}");
        let result = call(&s, &u, "item/use_accessory_select_item", &args).await;
        assert_eq!(result["Result"], "Success", "Native accessory {id}: {result}");
        assert_eq!(result["PlayerAccessoryCostumeInfo"]["HeroIndex"], 1);
        assert_eq!(result["PlayerAccessoryCostumeInfo"]["AccessoryCostumeIndex"], accessory);
        let info: Value = serde_json::from_str(result["PlayerAccessoryCostumeInfo"]["PositionInfo"].as_str().unwrap()).unwrap();
        assert_eq!(info, position);
        assert_ne!(call(&s, &u, "item/use_accessory_select_item", &args).await["Result"], "Success");
        assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=$2")
            .bind(a).bind(id).fetch_one(&s.db).await.unwrap(), 1);
        used += 1;
    }
    assert!(used > 100, "Expected the restored accessory catalog, got {used}");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.player_accessory_costume_infos.len(), used);
}

#[tokio::test]
async fn accessory_item_invalid_native_positions_do_not_consume_or_grant() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 3100017, 1).await;
    for position in ["%notjson".to_string(), "{}".to_string(),
        json!({"PositionX":"0","PositionY":"0","PositionZ":"0","RotationX":"0","RotationY":"0","RotationZ":"0","Scale":"0"}).to_string()] {
        let escaped = urlencoding::encode(&position).into_owned();
        let wire = urlencoding::encode(&escaped);
        let args = format!("ItemIndex=3100017&AccessoryCostumeIndex=3100017&HeroIndex=1&PositionInfo={wire}");
        assert_ne!(call(&s, &u, "item/use_accessory_select_item", &args).await["Result"], "Success");
        assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=3100017")
            .bind(a).fetch_one(&s.db).await.unwrap(), 1);
        assert!(get(&mut s.db.acquire().await.unwrap(), a, "accessory", cosmetics::accessory_key(1, 3100017).unwrap()).await.unwrap().is_null());
    }
}

#[tokio::test]
async fn exp_flasks_reattach_same_type_until_stock_runs_out_and_persist() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 40001, 3).await;
    assert_eq!(call(&s, &u, "item/use_flask_item", "HeroIndex=1&ItemIndex=40001").await["Result"], "Success");
    for completed in 1..=3 {
        let mut tx = s.db.begin().await.unwrap();
        let (result, items) = consumables::fill_flask(&mut tx, &s, a, 1, 200).await.unwrap().unwrap();
        assert_eq!(result["FilledFlaskCount"], 1);
        assert_eq!(result["Info"]["ItemIndex"], if completed < 3 { 40001 } else { 0 });
        assert_eq!(result["Info"]["NewExp"], 0);
        let full = items.iter().find(|i| i["ItemIndex"] == 40005).unwrap();
        assert_eq!(full["NewCount"], completed);
        if completed < 3 {
            let empty = items.iter().find(|i| i["ItemIndex"] == 40001).unwrap();
            assert_eq!(empty["AddCount"], -1);
            assert_eq!(empty["NewCount"], 2 - completed);
        } else {
            assert!(!items.iter().any(|i| i["ItemIndex"] == 40001));
            assert!(consumables::fill_flask(&mut tx, &s, a, 1, 200).await.unwrap().is_none());
        }
        tx.commit().await.unwrap();
        let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
        let h = login.heroes.iter().find(|h| h.hero_index == 1).unwrap();
        assert_eq!(h.details["FlaskItemIndex"], if completed < 3 { 40001 } else { 0 });
        assert_eq!(h.details["FlaskExp"], 0);
    }
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=40001").bind(a).fetch_one(&s.db).await.unwrap(), 0);
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=40005").bind(a).fetch_one(&s.db).await.unwrap(), 3);
}

#[tokio::test]
async fn cursed_ruby_flasks_equip_fill_from_stamina_cancel_and_reattach() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    for id in [40101,40103,40105,40111,40113,40115] {
        let row=s.tables.extensions.find("FlaskItem",&[("ItemIndex",id)]).unwrap();
        let full=n(row,"PotionItemIndex") as i32;let capacity=n(row,"FlaskExp");
        give(&s,a,id as i32,2).await;
        let args=format!("HeroIndex=1&ItemIndex={id}");
        let equipped=call(&s,&u,"item/use_flask_item",&args).await;
        assert_eq!(equipped["Result"],"Success","{equipped}");assert_eq!(equipped["FlaskResult"]["ItemIndex"],id);
        assert_eq!(equipped["ItemResult"]["NewCount"],1);
        assert_ne!(call(&s,&u,"item/use_flask_item",&args).await["Result"],"Success");
        let mut tx=s.db.begin().await.unwrap();
        assert!(consumables::fill_flask(&mut tx,&s,a,1,capacity).await.unwrap().is_none());
        let (progress,items)=consumables::fill_stamina_flask(&mut tx,&s,a,1,1).await.unwrap().unwrap();
        assert_eq!(progress["Info"]["NewExp"],1);assert!(items.is_empty());tx.commit().await.unwrap();
        let login=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
        let hero=login.heroes.iter().find(|h|h.hero_index==1).unwrap();
        assert_eq!(hero.details["FlaskItemIndex"],id);assert_eq!(hero.details["FlaskExp"],1);
        let cancelled=call(&s,&u,"item/cancel_flask_item","HeroIndex=1").await;
        assert_eq!(cancelled["Result"],"Success");assert_eq!(cancelled["ItemResult"]["NewCount"],2);
        assert_ne!(call(&s,&u,"item/cancel_flask_item","HeroIndex=1").await["Result"],"Success");
        assert_eq!(call(&s,&u,"item/use_flask_item",&args).await["Result"],"Success");
        for remaining in [1,0] {
            let mut tx=s.db.begin().await.unwrap();
            let (result,items)=consumables::fill_stamina_flask(&mut tx,&s,a,1,capacity).await.unwrap().unwrap();
            assert_eq!(result["FilledFlaskIndex"],full);assert_eq!(result["FilledFlaskCount"],1);
            assert_eq!(result["Info"]["ItemIndex"],if remaining==1 {id} else {0});
            assert!(items.iter().any(|r|r["ItemIndex"]==full && r["AddCount"]==1));
            tx.commit().await.unwrap();
        }
        let mut tx=s.db.begin().await.unwrap();
        assert!(consumables::fill_stamina_flask(&mut tx,&s,a,1,capacity).await.unwrap().is_none());tx.commit().await.unwrap();
    }
    give(&s,a,40201,1).await;
    assert_ne!(call(&s,&u,"item/use_flask_item","HeroIndex=1&ItemIndex=40201").await["Result"],"Success");
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=40201").bind(a).fetch_one(&s.db).await.unwrap(),1);
}

#[tokio::test]
async fn exp_flask_replacement_preserves_partial_progress_and_can_be_cancelled_once() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 40001, 2).await;
    assert_eq!(call(&s, &u, "item/use_flask_item", "HeroIndex=1&ItemIndex=40001").await["Result"], "Success");
    let mut tx = s.db.begin().await.unwrap();
    let (partial, items) = consumables::fill_flask(&mut tx, &s, a, 1, 50).await.unwrap().unwrap();
    assert_eq!(partial["Info"]["NewExp"], 50);
    assert_eq!(partial["FilledFlaskCount"], 0);
    assert!(items.is_empty());
    let (full, _) = consumables::fill_flask(&mut tx, &s, a, 1, 150).await.unwrap().unwrap();
    assert_eq!(full["Info"]["ItemIndex"], 40001);
    assert_eq!(full["Info"]["NewExp"], 0);
    tx.commit().await.unwrap();
    let cancel = call(&s, &u, "item/cancel_flask_item", "HeroIndex=1").await;
    assert_eq!(cancel["Result"], "Success");
    assert_eq!(cancel["ItemResult"]["ItemIndex"], 40001);
    assert_eq!(cancel["ItemResult"]["NewCount"], 1);
    assert_ne!(call(&s, &u, "item/cancel_flask_item", "HeroIndex=1").await["Result"], "Success");
}

#[tokio::test]
async fn exp_flask_completion_keeps_locked_replacement_stock_and_never_switches_types() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 40001, 2).await;
    give(&s, a, 40002, 3).await;
    assert_eq!(call(&s, &u, "item/use_flask_item", "HeroIndex=1&ItemIndex=40001").await["Result"], "Success");
    crate::db::query("UPDATE items SET locked=1 WHERE account_id=$1 AND item_index=40001").bind(a).execute(&s.db).await.unwrap();
    let mut tx = s.db.begin().await.unwrap();
    let (result, items) = consumables::fill_flask(&mut tx, &s, a, 1, 200).await.unwrap().unwrap();
    assert_eq!(result["FilledFlaskCount"], 1);
    assert_eq!(result["Info"]["ItemIndex"], 0);
    assert_eq!(items.len(), 1);
    tx.commit().await.unwrap();
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=40001").bind(a).fetch_one(&s.db).await.unwrap(), 1);
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=40002").bind(a).fetch_one(&s.db).await.unwrap(), 3);
}

#[tokio::test]
async fn technomagic_restoration_accepts_max_star_locked_gear_and_native_costs() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut eq = give(&s, a, 911101, 1).await.equipment.remove(0);
    eq.star = 5;
    eq.identified = 1;
    eq.locked = 1;
    save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
    give(&s, a, 990000, 203).await;
    let result = call(&s, &u, "upgrade_equip_tier", &format!("EquipItemSlotIndex={}", eq.slot_index)).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(result["ResultEquipItem"]["ItemIndex"], 901101);
    assert_eq!(result["ResultEquipItem"]["Star"], 0);
    assert_eq!(result["ResultEquipItem"]["Locked"], 1);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT gold FROM user_info WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap(), 90000000);
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=990000").bind(a).fetch_one(&s.db).await.unwrap(), 163);
}

#[tokio::test]
async fn technomagic_awaken_charges_displayed_target_star_costs_and_preserves_failed_attempts() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    give(&s,a,990101,5000).await;
    let stones=|s:AppState|async move {
        crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=990101")
            .bind(a).fetch_one(&s.db).await.unwrap()
    };
    for (id,base) in [(911101,10),(901101,15)] {
        let mut eq=give(&s,a,id,1).await.equipment.remove(0);
        eq.identified=1;eq.locked=1;
        save_equip(&mut s.db.acquire().await.unwrap(),a,&eq).await.unwrap();
        let args=format!("EquipItemSlotIndex={}",eq.slot_index);
        for star in 0..5 {
            let expected=base*(1<<star);
            let price=i64::from(expected)*100000;
            crate::db::query("UPDATE user_info SET gold=100000000 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
            crate::db::query("UPDATE items SET count=$1 WHERE account_id=$2 AND item_index=990101").bind(expected-1).bind(a).execute(&s.db).await.unwrap();
            let rejected=call(&s,&u,"valance_awaken",&args).await;
            assert_ne!(rejected["Result"],"Success","{id} {star}: {rejected}");
            assert_eq!(stones(s.clone()).await,expected-1);assert_eq!(gold(&s).await,100000000);
            assert_eq!(equip(&mut s.db.acquire().await.unwrap(),a,eq.slot_index as i64).await.unwrap().star,star);
            crate::db::query("UPDATE items SET count=5000 WHERE account_id=$1 AND item_index=990101").bind(a).execute(&s.db).await.unwrap();
            crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2").bind(price-1).bind(a).execute(&s.db).await.unwrap();
            assert_ne!(call(&s,&u,"valance_awaken",&args).await["Result"],"Success");
            assert_eq!(stones(s.clone()).await,5000);assert_eq!(gold(&s).await,price-1);
            crate::db::query("UPDATE user_info SET gold=100000000 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
            let result=call(&s,&u,"valance_awaken",&args).await;
            assert_eq!(result["Result"],"Success","{result}");
            assert_eq!(result["ResultEquipItem"]["Star"],star+1);
            assert_eq!(result["ItemResults"][0]["AddCount"],-expected);
            assert_eq!(stones(s.clone()).await,5000-expected);
            assert_eq!(gold(&s).await,100000000-price);
            assert_eq!(result["ResultEquipItem"]["Locked"],1);
        }
        let before_gold=gold(&s).await;let before_stones=stones(s.clone()).await;
        assert_eq!(call(&s,&u,"valance_awaken",&args).await["Result"],"CannotAwakenMore");
        assert_eq!(gold(&s).await,before_gold);assert_eq!(stones(s.clone()).await,before_stones);
    }
}

#[tokio::test]
async fn technomagic_awaken_to_target_star_charges_every_step_atomically() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    give(&s,a,990101,5000).await;
    let stones=|s:AppState|async move {
        crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=990101")
            .bind(a).fetch_one(&s.db).await.unwrap()
    };
    let mut eq=give(&s,a,911101,1).await.equipment.remove(0);
    eq.identified=1;eq.star=1;
    save_equip(&mut s.db.acquire().await.unwrap(),a,&eq).await.unwrap();
    let slot=eq.slot_index;
    let star=|s:AppState|async move {equip(&mut s.db.acquire().await.unwrap(),a,slot as i64).await.unwrap().star};
    // Stars 2..=4 cost 20+40+80 stones and 100,000 gold per stone.
    let (need,price)=(140,14_000_000i64);
    for target in [1,6,-1] {
        let result=call(&s,&u,"valance_awaken",&format!("EquipItemSlotIndex={slot}&TargetStar={target}")).await;
        assert_eq!(result["Result"],"CannotAwakenMore","{target}: {result}");
    }
    let args=format!("EquipItemSlotIndex={slot}&TargetStar=4");
    crate::db::query("UPDATE items SET count=$1 WHERE account_id=$2 AND item_index=990101").bind(need-1).bind(a).execute(&s.db).await.unwrap();
    crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2").bind(price).bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s,&u,"valance_awaken",&args).await["Result"],"Success");
    assert_eq!((stones(s.clone()).await,gold(&s).await,star(s.clone()).await),(need-1,price,1));
    crate::db::query("UPDATE items SET count=5000 WHERE account_id=$1 AND item_index=990101").bind(a).execute(&s.db).await.unwrap();
    crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2").bind(price-1).bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s,&u,"valance_awaken",&args).await["Result"],"Success");
    assert_eq!((stones(s.clone()).await,gold(&s).await,star(s.clone()).await),(5000,price-1,1));
    crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2").bind(price+7).bind(a).execute(&s.db).await.unwrap();
    let result=call(&s,&u,"valance_awaken",&args).await;
    assert_eq!(result["Result"],"Success","{result}");
    assert_eq!(result["ResultEquipItem"]["Star"],4);
    assert_eq!(result["ItemResults"].as_array().unwrap().len(),1);
    assert_eq!(result["ItemResults"][0]["AddCount"],-need);
    assert_eq!((stones(s.clone()).await,gold(&s).await,star(s.clone()).await),(5000-need,7,4));
    // Omitting TargetStar keeps the native one-star request.
    crate::db::query("UPDATE user_info SET gold=100000000 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let result=call(&s,&u,"valance_awaken",&format!("EquipItemSlotIndex={slot}")).await;
    assert_eq!(result["ResultEquipItem"]["Star"],5,"{result}");
    assert_eq!(stones(s.clone()).await,5000-need-160);
}

#[tokio::test]
async fn technomagic_restoration_rejections_use_native_messages_and_preserve_costs() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut eq = give(&s, a, 911101, 1).await.equipment.remove(0);
    eq.star = 4; eq.identified = 1;
    save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
    give(&s, a, 990000, 203).await;
    let args = format!("EquipItemSlotIndex={}", eq.slot_index);
    for star in [0, 1, 4, 6, 10] {
        eq.star = star;
        save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
        assert_eq!(call(&s, &u, "upgrade_equip_tier", &args).await["Result"], "NotEquipMaxStar");
    }
    assert_eq!(call(&s, &u, "upgrade_equip_tier", "EquipItemSlotIndex=2147483647").await["Result"], "EquipNotOwned");
    eq.star = 5;
    save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
    crate::db::query("UPDATE items SET locked=1 WHERE account_id=$1 AND item_index=990000").bind(a).execute(&s.db).await.unwrap();
    assert_eq!(call(&s, &u, "upgrade_equip_tier", &args).await["Result"], "NotEnoughMaterialItem");
    crate::db::query("UPDATE items SET locked=0 WHERE account_id=$1 AND item_index=990000").bind(a).execute(&s.db).await.unwrap();
    crate::db::query("UPDATE user_info SET gold=9999999 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    assert_eq!(call(&s, &u, "upgrade_equip_tier", &args).await["Result"], "NotEnoughGold");
    assert_eq!(crate::db::query_scalar::<i64>("SELECT gold FROM user_info WHERE account_id=$1").bind(a).fetch_one(&s.db).await.unwrap(), 9999999);
    assert_eq!(crate::db::query_scalar::<i32>("SELECT count FROM items WHERE account_id=$1 AND item_index=990000").bind(a).fetch_one(&s.db).await.unwrap(), 203);
    assert_eq!(equip(&mut s.db.acquire().await.unwrap(), a, eq.slot_index as i64).await.unwrap().item_index, 911101);
}

#[tokio::test]
async fn technomagic_restoration_preserves_equipped_hero_for_locked_and_unlocked_gear() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 990000, 500).await;
    for locked in [0, 1] {
        let mut eq = give(&s, a, 911101, 1).await.equipment.remove(0);
        eq.star = 5; eq.identified = 1; eq.locked = locked;
        save_equip(&mut s.db.acquire().await.unwrap(), a, &eq).await.unwrap();
        crate::db::query("UPDATE heroes SET equip_item_slot_index_2=$1 WHERE account_id=$2 AND hero_index=1")
            .bind(eq.slot_index).bind(a).execute(&s.db).await.unwrap();
        let result = call(&s, &u, "upgrade_equip_tier", &format!("EquipItemSlotIndex={}", eq.slot_index)).await;
        assert_eq!(result["Result"], "Success", "{result}");
        assert_eq!(result["EquipHeroIndex"], 1, "The native client unequips the item unless its owner is returned");
        let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
        let hero = login.heroes.iter().find(|h| h.hero_index==1).unwrap();
        assert_eq!(hero.equip_item_slot_index_2, eq.slot_index);
        assert_eq!(result["ResultEquipItem"]["Locked"], locked);
    }
}

#[tokio::test]
async fn multiplayer_hero_snapshots_carry_visible_accessories() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    // A legacy single-slot appearance has no SprkAccessories list and no
    // native transforms. The battle worker used to re-encode those as `{}`.
    crate::api::heroes::save_details(&mut db, a, 1,
        &json!({"AccessoryCostumeIndex1":3100013,"AccessoryCostumeIndex4":3110025})).await.unwrap();
    let custom = json!({"PositionX":"0.25","PositionY":"0","PositionZ":"0",
        "RotationX":"0","RotationY":"90","RotationZ":"0","Scale":"1.5"});
    put(&mut db, a, "accessory", cosmetics::accessory_key(1, 3110025).unwrap(),
        &json!({"HeroIndex":1,"AccessoryCostumeIndex":3110025,"PositionInfo":custom.to_string()})).await.unwrap();
    let cached = crate::api::community::cached_hero(&mut db, a, 1).await.unwrap();
    assert_eq!(cached["AccessoryPositionInfo1"]["Scale"], "1");
    assert_eq!(cached["AccessoryPositionInfo4"], custom);
    assert!(cached["AccessoryPositionInfo2"].is_null());
    let list: Vec<_> = cached["SprkAccessories"].as_array().unwrap().iter()
        .map(|v| (n(v, "Index"), n(v, "Part"))).collect();
    assert_eq!(list, vec![(3100013, 1), (3110025, 4)]);
    assert_eq!(cached["SprkAccessories"][1]["PositionInfo"], custom.to_string());
    // An explicit multi-accessory list is authoritative and kept intact.
    let stacked = json!([{"Index":3100013,"Part":1,"PositionInfo":""},{"Index":3100201,"Part":1,"PositionInfo":custom.to_string()}]);
    crate::api::heroes::save_details(&mut db, a, 1,
        &json!({"AccessoryCostumeIndex1":3100201,"SprkAccessories":stacked})).await.unwrap();
    let cached = crate::api::community::cached_hero(&mut db, a, 1).await.unwrap();
    assert_eq!(cached["SprkAccessories"], stacked);
    assert_eq!(cached["AccessoryPositionInfo1"], custom);
}

#[tokio::test]
async fn batch_break_clears_every_reference_once_and_grants_summed_materials() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut slots = vec![];
    for _ in 0..3 { slots.push(give(&s, a, 70004, 1).await.equipment.remove(0)); }
    let armor = give(&s, a, 890203, 1).await.equipment.remove(0);
    let stored = call(&s, &u, "add_equip_storage_slot", &format!(
        "EquipStorageSlotIndex=1&HeroIndex=1&HeroPartIndex=[2,4]&EquipItemSlotIndex=[{},{}]",
        armor.slot_index, slots[0].slot_index)).await;
    assert_eq!(stored["Result"], "Success", "{stored}");
    let preset = json!([{"HeroIndex":1,"EquipItemSlotIndex":[0,slots[1].slot_index,0,slots[0].slot_index],"EquipItemUid":["",slots[1].uid,"",slots[0].uid],"EquipItemRunePage":[0,1,0,2]}]);
    crate::db::query("INSERT INTO hero_presets(account_id,storage_key,data) VALUES($1,'batch',$2)")
        .bind(a).bind(preset.to_string()).execute(&s.db).await.unwrap();
    let before: Vec<(i64, i64)> = crate::db::query_as("SELECT item_index,count FROM items WHERE account_id=$1").bind(a).fetch_all(&s.db).await.unwrap();
    let ids = slots.iter().map(|e| e.slot_index.to_string()).collect::<Vec<_>>().join(",");
    let result = call(&s, &u, "break_equip", &format!("EquipItemSlotIndices=[{ids}]")).await;
    assert_eq!(result["Result"], "Success", "{result}");
    let remaining: i64 = crate::db::query_scalar("SELECT count(*) FROM equip_items WHERE account_id=$1 AND slot_index IN (SELECT sprk_json_ints($2))")
        .bind(a).bind(format!("[{ids}]")).fetch_one(&s.db).await.unwrap();
    assert_eq!(remaining, 0);
    let mut db = s.db.acquire().await.unwrap();
    let storage = get(&mut db, a, "equip_storage", 1).await.unwrap();
    assert_eq!((storage["EquipItemSlotIndex2"].as_i64(), storage["EquipItemSlotIndex4"].as_i64()), (Some(armor.slot_index as i64), Some(0)));
    let saved: String = crate::db::query_scalar("SELECT data FROM hero_presets WHERE account_id=$1 AND storage_key='batch'").bind(a).fetch_one(&mut *db).await.unwrap();
    let saved: Value = serde_json::from_str(&saved).unwrap();
    assert_eq!(saved[0]["EquipItemSlotIndex"], json!([0,0,0,0]));
    assert_eq!(saved[0]["EquipItemUid"], json!(["","","",""]));
    // Materials are summed per item and granted once; the reported new counts
    // match the inventory change.
    let after: Vec<(i64, i64)> = crate::db::query_as("SELECT item_index,count FROM items WHERE account_id=$1").bind(a).fetch_all(&mut *db).await.unwrap();
    let results = result["ItemResults"].as_array().unwrap();
    assert!(!results.is_empty(), "{result}");
    let mut indices = std::collections::BTreeSet::new();
    for r in results {
        let index = r["ItemIndex"].as_i64().unwrap();
        assert!(indices.insert(index), "one result per material: {result}");
        let old = before.iter().find(|(i, _)| *i == index).map(|v| v.1).unwrap_or(0);
        let new = after.iter().find(|(i, _)| *i == index).map(|v| v.1).unwrap();
        assert!(new > old, "{index}: {old} -> {new}");
    }
}
