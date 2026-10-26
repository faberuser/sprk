use crate::models::item::ItemGrant;
use super::*;
use crate::api::account::user;
use crate::database;

async fn setup() -> (AppState, user::LoginResponse) {
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    let state = AppState::new(db, crate::tables::test_tables());
    let u = user::test_login(
        State(state.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    sqlx::query("UPDATE user_info SET gold=100000000,gem=100000")
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
async fn public_awakening_stones_obey_equipment_type() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let large_gold=7_782_963_002_i64;
    sqlx::query("UPDATE user_info SET gold=? WHERE account_id=?").bind(large_gold).bind(a).execute(&s.db).await.unwrap();
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
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT SUM(value) FROM progression_metrics WHERE account_id=? AND kind='IdentifiedEquip'")
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
    let pending_count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM equipment_pending WHERE account_id=? AND slot_index=?")
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
    sqlx::query("UPDATE user_info SET team_level=2")
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
    sqlx::query("UPDATE heroes SET level=100,star=5,transcend=5 WHERE hero_index=1")
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
    sqlx::query("UPDATE user_info SET team_level=10 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    let upgraded = json!({"OpenBuffLevel":2,"BuffReinforce":1,"UpdatedTime":"2026-01-01T00:00:00Z"});
    put(&mut *s.db.acquire().await.unwrap(),a,"team_buff",2,&upgraded).await.unwrap();
    let before: (i64,i64) = sqlx::query_as("SELECT gold,gem FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
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
    assert_eq!(sqlx::query_as::<_,(i64,i64)>("SELECT gold,gem FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),before);
    assert!(buffs::unlock_team_buffs(&mut *s.db.acquire().await.unwrap(),&s,a).await.unwrap().is_empty());
    // New accounts can bootstrap the same free records through first-lobby too.
    sqlx::query("DELETE FROM extension_state WHERE account_id=? AND kind='team_buff'").bind(a).execute(&s.db).await.unwrap();
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
    sqlx::query("INSERT INTO costumes(account_id,costume_index) VALUES(?,114)")
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
    assert_eq!(login.user_info.gem, 100000 - price as i32);
    assert!(login.player_accessory_costume_infos.iter().any(|v| v["HeroIndex"] == 1 && v["AccessoryCostumeIndex"] == 3100013));
    // Previously reward-only accessories are now purchasable at their table price.
    let body_pos = pos.to_string().replace("3100013", "3110025");
    let escaped = urlencoding::encode(&body_pos).into_owned();
    let wire = urlencoding::encode(&escaped);
    let bought = call(&s, &login, "hero/buy_customizing_costumes",
        &format!("HeroIndex=1&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0&AccessoryCostumePositionInfo={wire}&BuyGem=4000&BuyGold=0")).await;
    assert_eq!(bought["Result"], "Success", "{bought}");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.user_info.gem, 100000 - price as i32 - 4000);
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
    // A position-only edit must be reflected in the next login's lobby snapshot.
    let mut edited = pos.clone();
    edited["3100013"]["Scale"] = json!("1.5");
    edited["3100013"]["PositionX"] = json!("0.2");
    let encoded = urlencoding::encode(&edited.to_string()).into_owned();
    let edited_result = call(&s, &login, "hero/edit_accessory_costume_position",
        &format!("HeroIndex=1&AccessoryCostumePositionInfo={}", urlencoding::encode(&encoded))).await;
    assert_eq!(edited_result["Result"], "Success");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(serde_json::from_str::<Value>(login.player_avatar_hero_info[&position_key].as_str().unwrap()).unwrap(), edited["3100013"]);
    let unset = call(&s, &login, "hero/buy_customizing_costumes",
        "HeroIndex=1&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0&UnsetAccessoryCostumeIndices=3100013&BuyGold=0&BuyGem=0").await;
    assert_eq!(unset["Result"], "Success");
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.player_avatar_hero_info[&slot_key], 0);
    assert_eq!(login.player_avatar_hero_info[&position_key], "");
    assert!(login.player_accessory_costume_infos.iter().any(|v| v["AccessoryCostumeIndex"] == 3100013));
}

#[tokio::test]
async fn all_accessories_can_be_purchased_with_atomic_price_and_balance_checks() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    sqlx::query("UPDATE user_info SET gem=10000000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
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
    sqlx::query("UPDATE user_info SET gem=0,pay_gem=0 WHERE account_id=?")
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
        sqlx::query_scalar::<_, i64>("SELECT gem FROM user_info WHERE account_id=?")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap(),
        0
    );
}
#[tokio::test]
async fn equipment_presets_keep_paid_slots_and_protect_saved_items() {
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
            .is_err()
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
    sqlx::query("UPDATE heroes SET equip_item_slot_index_1=?,equip_item_slot_index_3=?,equip_item_slot_index_7=777 WHERE account_id=? AND hero_index=1")
        .bind(weapon.slot_index).bind(accessory.slot_index).bind(a).execute(&s.db).await.unwrap();
    sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,equip_item_slot_index_2) VALUES (?,2,2,?)")
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
    sqlx::query("UPDATE equip_items SET inventory_type=1 WHERE account_id=? AND slot_index=?")
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
    let before:i64=sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    let login=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    let info=login.npc_friendly_infos.iter().find(|r|r["HeroIndex"]==111).unwrap();
    assert_eq!(*info,json!({"HeroIndex":111,"Step":1,"FriendlyPoint":9000,"EndRewardCount":1}));
    let again=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(again.npc_friendly_infos,login.npc_friendly_infos);
    let after:i64=sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
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
    let gems:i64=sqlx::query_scalar("SELECT gem FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    // No valid outcome: preserve balances, counters and pending state.
    let rejected=call(&s,&u,"renew_equip_option",&args).await;
    assert_ne!(rejected["Result"],"Success");
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT gem FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),gems);
    assert_eq!(json!(equip(&mut s.db.acquire().await.unwrap(),a,eq.slot_index as i64).await.unwrap()),json!(eq));
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equipment_pending WHERE account_id=? AND slot_index=?").bind(a).bind(eq.slot_index).fetch_one(&s.db).await.unwrap(),0);
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
        let currencies: (i64, i64) = sqlx::query_as("SELECT gold,gem FROM user_info WHERE account_id=?")
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
                let remaining: i64 = sqlx::query_scalar("SELECT COALESCE(SUM(count),0) FROM items WHERE account_id=? AND item_index=?")
                    .bind(a).bind(id).fetch_one(&s.db).await.unwrap();
                assert_eq!(remaining, (2-slot) * if id == 2200 { 1 } else { 300 });
            }
        }
        assert_eq!(sqlx::query_as::<_, (i64, i64)>("SELECT gold,gem FROM user_info WHERE account_id=?")
            .bind(a).fetch_one(&s.db).await.unwrap(), currencies);
        // An unfunded retry must leave the item and pending choice unchanged.
        let rejected = call(&s, &u, "renew_equip_option",
            &format!("EquipItemSlotIndex={}&OptionSlotIndex=1", eq.slot_index)).await;
        assert_ne!(rejected["Result"], "Success");
        let saved = equip(&mut s.db.acquire().await.unwrap(), a, eq.slot_index as i64).await.unwrap();
        assert_eq!(json!(saved), current);
        let pending: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM equipment_pending WHERE account_id=?")
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
        assert_eq!(sqlx::query_scalar::<_, i64>("SELECT COALESCE(SUM(count),0) FROM items WHERE account_id=? AND item_index=?")
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
    sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,star) VALUES(?,111,111,5)")
        .bind(a).execute(&s.db).await.unwrap();
    give(&s, a, 990001, 3400).await;
    let r = call(&s, &u, "valance_craft",
        "SetType=Gargoria&CreatureTagType=Knight&PartType=Armor&SubPartType=None&SelectEquipOptionIndices=[]").await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["EquipItemResult"]["Identified"], 0);
    let remaining: i64 = sqlx::query_scalar("SELECT COALESCE(SUM(count),0) FROM items WHERE account_id=? AND item_index=990001")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(remaining, 0);
    assert_eq!(gold(&s).await, 91000000);
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
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT SUM(value) FROM progression_metrics WHERE account_id=? AND kind='CraftEquip'")
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
    let nick: String = sqlx::query_scalar("SELECT nick FROM accounts WHERE account_id=?")
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
    sqlx::query_scalar("SELECT gold FROM user_info")
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
        let saved:i64=sqlx::query_scalar("SELECT star FROM equip_items WHERE slot_index=?").bind(x).fetch_one(&s.db).await.unwrap();
        assert_eq!(saved,1);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items WHERE slot_index=?").bind(y).fetch_one(&s.db).await.unwrap(),0);
        assert_ne!(call(&s,&u,"awaken_equip",&args).await["Result"],"Success");
        assert_eq!(gold(&s).await,before-50000);
    }
    let target=give(&s,a,1001,1).await.equipment[0].slot_index;
    give(&s,a,5301,1).await;
    let result=call(&s,&u,"awaken_equip",&format!("EquipItemSlotIndex={target}&MaterialItemInfors=5301&MaterialItemInfors=1")).await;
    assert_eq!(result["Result"],"Success","{result}");
    assert_eq!(result["ResultEquipItem"]["Star"],1);
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
        sqlx::query_as::<_, (i32, i32)>("SELECT item_index,count FROM items WHERE account_id=? AND item_index IN (5100,5103) ORDER BY item_index")
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
        sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=100001")
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
    assert_eq!(ether["ItemResults"][0]["ItemIndex"], 5100);
    let protection: i64 =
        sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=5251")
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
    sqlx::query("UPDATE equip_items SET locked=1 WHERE account_id=? AND slot_index=?").bind(a).bind(second).execute(&s.db).await.unwrap();
    let before:i64=sqlx::query_scalar("SELECT COALESCE(SUM(count),0) FROM items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    let result=call(&s,&u,"break_equip",&format!("EquipItemSlotIndices={first}&EquipItemSlotIndices={second}")).await;
    assert_eq!(result["Result"],"LockedEquipItemExists","{result}");
    let result=call(&s,&u,"break_equip",&format!("EquipItemSlotIndices={first}&EquipItemSlotIndices={first}")).await;
    assert_ne!(result["Result"],"Success");
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items WHERE account_id=? AND slot_index IN (?,?)").bind(a).bind(first).bind(second).fetch_one(&s.db).await.unwrap(),2);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COALESCE(SUM(count),0) FROM items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),before);
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
        assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=?")
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
        assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=3100017")
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
    assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=40001").bind(a).fetch_one(&s.db).await.unwrap(), 0);
    assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=40005").bind(a).fetch_one(&s.db).await.unwrap(), 3);
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
    sqlx::query("UPDATE items SET locked=1 WHERE account_id=? AND item_index=40001").bind(a).execute(&s.db).await.unwrap();
    let mut tx = s.db.begin().await.unwrap();
    let (result, items) = consumables::fill_flask(&mut tx, &s, a, 1, 200).await.unwrap().unwrap();
    assert_eq!(result["FilledFlaskCount"], 1);
    assert_eq!(result["Info"]["ItemIndex"], 0);
    assert_eq!(items.len(), 1);
    tx.commit().await.unwrap();
    assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=40001").bind(a).fetch_one(&s.db).await.unwrap(), 1);
    assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=40002").bind(a).fetch_one(&s.db).await.unwrap(), 3);
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
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT gold FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(), 90000000);
    assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=990000").bind(a).fetch_one(&s.db).await.unwrap(), 163);
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
    sqlx::query("UPDATE items SET locked=1 WHERE account_id=? AND item_index=990000").bind(a).execute(&s.db).await.unwrap();
    assert_eq!(call(&s, &u, "upgrade_equip_tier", &args).await["Result"], "NotEnoughMaterialItem");
    sqlx::query("UPDATE items SET locked=0 WHERE account_id=? AND item_index=990000").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET gold=9999999 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_eq!(call(&s, &u, "upgrade_equip_tier", &args).await["Result"], "NotEnoughGold");
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT gold FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(), 9999999);
    assert_eq!(sqlx::query_scalar::<_, i32>("SELECT count FROM items WHERE account_id=? AND item_index=990000").bind(a).fetch_one(&s.db).await.unwrap(), 203);
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
        sqlx::query("UPDATE heroes SET equip_item_slot_index_2=? WHERE account_id=? AND hero_index=1")
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
