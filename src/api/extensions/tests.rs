use super::*;
use crate::api::account::user;
use crate::database;
use crate::tables::GameTables;
use std::{path::Path, sync::OnceLock};

async fn setup() -> (AppState, user::LoginResponse) {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    let state = AppState::new(
        db,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    );
    let u = user::login(
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
    assert_eq!(r["ResultEquipItem"]["Star"], 1);
}
#[tokio::test]
async fn restore_soul_stone_persists_choices_and_rejects_replayed_claims() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    give(&s, a, 45080, 400).await;
    let r = call(&s, &u, "restore_soul_stone", "SoulStoneRestoreIndex=1").await;
    assert_eq!(r["Result"], "Success", "{r}");
    let ids = r["SelectSoulStoneIndices"].as_array().unwrap();
    assert_eq!(ids.len(), 3);
    let login = user::login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(login.misc_info.unwrap().extra["SoulStoneMileage"], 1);
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
}
#[tokio::test]
async fn valance_identification_and_enchantment_restore_pending_choices() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut eq = give(&s, a, 911301, 1).await.equipment.remove(0);
    eq.identified = 0;
    save_equip(&mut *s.db.acquire().await.unwrap(), a, &eq)
        .await
        .unwrap();
    let args = format!("EquipItemSlotIndices=[{}]", eq.slot_index);
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
    let enchant = s
        .tables
        .extensions
        .rows("ValanceEnchant")
        .iter()
        .find(|v| n(v, "PartType") == 3 && n(v, "SubType") == 10)
        .unwrap();
    let enchant_id = n(enchant, "ItemIndex");
    give(&s, a, enchant_id as i32, 1).await;
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
    let expected = r["NewValanceEnchantOptionInfo"]["EnchantOptionIndex1"].clone();
    let login = user::login(
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
async fn punishment_rune_crafting_equipping_preservation_and_break_are_atomic() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    for (id, count) in [(45005, 10000), (45123, 1), (45128, 150), (100005, 11)] {
        give(&s, a, id, count).await;
    }
    let ingredients = vec![100005; 10];
    let r = call(
        &s,
        &u,
        "craft_punishment_rune",
        &format!(
            "PunishmentRuneIndex=100262&IngredientRuneIndices={}",
            json!(ingredients)
        ),
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    let slot = n(&r["EquipItemResult"], "SlotIndex");
    assert!(r["EquipItemResult"]["PunishmentRuneOption"].is_object());
    let args =
        format!("HeroIndex=1&RunePage=1&SlotNum=1&ItemIndex=100262&EquipItemSlotIndex={slot}");
    assert_eq!(call(&s, &u, "equip_rune", &args).await["Result"], "Success");
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
    let result = item::break_rune(State(s.clone()), body).await.unwrap().0;
    assert_eq!(result["Result"], "Success", "{result}");
    assert!(
        punishment::available(&mut *s.db.acquire().await.unwrap(), a, slot)
            .await
            .is_err()
    );
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
    assert!(get(&mut *s.db.acquire().await.unwrap(), a, "hair", 220114)
        .await
        .unwrap()
        .is_null());
    let r = call(&s, &u, "buy_customizing_costumes", args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["HairCostumeInfo"]["HairCostumeIndex"], 220114);
    let login = user::login(
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
    assert!(get(&mut *s.db.acquire().await.unwrap(), a, "accessory", key).await.unwrap().is_null());
    let bought = call(&s, &u, "hero/buy_customizing_costumes", &format!("{args}&BuyGem={price}")).await;
    assert_eq!(bought["Result"], "Success", "{bought}");
    let again = call(&s, &u, "hero/buy_customizing_costumes", &format!("{args}&BuyGem=0")).await;
    assert_eq!(again["Result"], "Success", "{again}");
    let login = user::login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.user_info.gem, 100000 - price as i32);
    assert!(login.player_accessory_costume_infos.iter().any(|v| v["HeroIndex"] == 1 && v["AccessoryCostumeIndex"] == 3100013));
    // The body accessory from the reported failure uses the same wire format.
    let body_pos = pos.to_string().replace("3100013", "3110025");
    let escaped = urlencoding::encode(&body_pos).into_owned();
    let wire = urlencoding::encode(&escaped);
    let bought = call(&s, &login, "hero/buy_customizing_costumes",
        &format!("HeroIndex=1&CostumeIndex=0&HairCostumeIndex=0&WeaponCostumeIndex=0&HideUniqueWeapon=0&AccessoryCostumePositionInfo={wire}&BuyGem=10000&BuyGold=0")).await;
    assert_eq!(bought["Result"], "Success", "{bought}");
    assert_eq!(bought["HeroCostumeResultInfo"]["AccessoryCostumeIndex4"], 3110025);
    let login = user::login(State(s.clone()), Bytes::from_static(b"LoginId=extensions-test")).await.unwrap().0;
    assert_eq!(login.user_info.gem, 100000 - price as i32 - 10000);
    assert!(login.player_accessory_costume_infos.iter().any(|v| v["HeroIndex"] == 1 && v["AccessoryCostumeIndex"] == 3110025));
}
#[tokio::test]
async fn equipment_presets_keep_paid_slots_and_protect_saved_items() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let eq = give(&s, a, 1001, 1).await.equipment.remove(0);
    let args = format!(
        "EquipStorageSlotIndex=1&HeroIndex=1&HeroPartIndex=[0]&EquipItemSlotIndex=[{}]&Name=Weapon",
        eq.slot_index
    );
    let r = call(&s, &u, "add_equip_storage_slot", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert!(
        material(&mut *s.db.acquire().await.unwrap(), a, eq.slot_index as i64)
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
    assert_eq!(r["HeroInfo"]["EquipItemSlotIndex1"], eq.slot_index);
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
    let pending = misc(&mut *s.db.acquire().await.unwrap(), a).await.unwrap();
    assert_eq!(pending["RenewOptionResultIndex"], option);
    let r = call(&s, &u, "renew_confirm_equip_option", &args).await;
    assert_eq!(r["ResultEquipItem"]["OptionIndex1"], option, "{r}");
    assert_ne!(
        call(&s, &u, "renew_confirm_equip_option", &args).await["Result"],
        "Success"
    );
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
    save_equip(&mut *s.db.acquire().await.unwrap(), a, &source)
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
    item::give(&mut tx, s, account, id, count, 0, 0, &mut rewards)
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
    hero::recruit_at(&mut *s.db.acquire().await.unwrap(), &s, a, 2, 1, 1, 0)
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
    let login = user::login(
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
    assert!(
        material(&mut *s.db.acquire().await.unwrap(), a, slot as i64)
            .await
            .is_err()
    );
    let login = user::login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=extensions-test"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(login.soul_weapon_infos.len(), 1);
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
