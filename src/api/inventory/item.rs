//! Inventory mutations use client table rules and one database transaction per request.
use crate::api::{
    battle::campaign_handlers::CurrencyResultInfo3,
    system::request::Request,
    tutorial::{self, Rewards},
};
use crate::{
    error::{Result, ServerError},
    models::equip::EquipItemInfo,
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};

pub(crate) fn rule(code: &str) -> ServerError {
    ServerError::InvalidRequest(code.into())
}
pub(crate) fn n(v: &Value, k: &str) -> i64 {
    v[k].as_i64().unwrap_or(0)
}
pub(crate) fn success() -> Value {
    json!({"BaseResult":"Success","Result":"Success"})
}
pub(crate) fn positive(req: &Request, key: &str) -> Result<i32> {
    let value = req.number(key, 1)?;
    if !(1..=1000).contains(&value) {
        return Err(rule("InvalidItemCount"));
    }
    Ok(value as i32)
}
pub(crate) fn item_index(req: &Request, key: &str) -> Result<i32> {
    i32::try_from(req.number(key, 0)?).map_err(|_| rule("ItemDataNotFound"))
}
pub(crate) fn data<'a>(state: &'a AppState, index: i32) -> Result<&'a Value> {
    state
        .tables
        .inventory
        .items
        .get(&index)
        .ok_or_else(|| rule("ItemDataNotFound"))
}
pub(crate) async fn init(db: &mut SqliteConnection, state: &AppState, account: i64) -> Result<()> {
    sqlx::query("INSERT OR IGNORE INTO inventory_settings(account_id) VALUES (?)")
        .bind(account)
        .execute(&mut *db)
        .await?;
    for slot in 1..=state.tables.inventory.constant("CraftSlotCountMin", 2) {
        sqlx::query("INSERT OR IGNORE INTO craft_slots(account_id,slot_index) VALUES (?,?)")
            .bind(account)
            .bind(slot)
            .execute(&mut *db)
            .await?;
    }
    Ok(())
}
pub(crate) async fn consume(
    db: &mut SqliteConnection,
    account: i64,
    index: i32,
    count: i32,
) -> Result<Value> {
    if count <= 0 {
        return Err(rule("InvalidItemCount"));
    }
    let row=sqlx::query("UPDATE items SET count=count-? WHERE account_id=? AND item_index=? AND count>=? AND locked=0 RETURNING count,locked")
        .bind(count).bind(account).bind(index).bind(count).fetch_optional(&mut *db).await?;
    let Some(row) = row else {
        return Err(rule("ItemNotOwned"));
    };
    Ok(
        json!({"ItemIndex":index,"AddCount":-count,"NewCount":row.get::<i32,_>("count"),"Locked":row.get::<i32,_>("locked"),"AddBoosterCount":0,"AddNPCBoosterCount":0,"AddBonusAssignedItemPercent":0,"IsFirstClearReward":false}),
    )
}
pub(crate) async fn money(
    db: &mut SqliteConnection,
    account: i64,
    kind: &str,
    amount: i64,
) -> Result<Value> {
    if kind == "Gem" && amount < 0 {
        let row = sqlx::query("SELECT gem,pay_gem FROM user_info WHERE account_id=?")
            .bind(account)
            .fetch_one(&mut *db)
            .await?;
        let free: i64 = row.get("gem");
        let paid: i64 = row.get("pay_gem");
        if free + paid < -amount {
            return Err(rule("NotEnoughGem"));
        }
        let used_free = free.min(-amount);
        let new_free = free - used_free;
        let new_paid = paid + amount + used_free;
        sqlx::query("UPDATE user_info SET gem=?,pay_gem=? WHERE account_id=?")
            .bind(new_free)
            .bind(new_paid)
            .bind(account)
            .execute(&mut *db)
            .await?;
        return Ok(json!(CurrencyResultInfo3::gem(amount, new_free, new_paid)));
    }
    let column = match kind {
        "Gold" => "gold",
        "Gem" => "gem",
        _ => return Err(rule("Fail")),
    };
    let row=sqlx::query(&format!("UPDATE user_info SET {column}={column}+? WHERE account_id=? AND {column}+?>=0 AND {column}+?<=2147483647 RETURNING {column},pay_gem"))
        .bind(amount).bind(account).bind(amount).bind(amount).fetch_optional(&mut *db).await?.ok_or_else(||rule(if kind=="Gold" {"NotEnoughGold"} else {"NotEnoughGem"}))?;
    Ok(if kind == "Gem" {
        json!(CurrencyResultInfo3::gem(
            amount,
            row.get(column),
            row.get("pay_gem")
        ))
    } else {
        json!(CurrencyResultInfo3::new(kind, amount, row.get(column)))
    })
}
pub(crate) async fn capacity(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    inventory: i32,
    extra: i64,
) -> Result<()> {
    let column = if inventory == 1 {
        "chest_extend"
    } else {
        "inventory_extend"
    };
    let extend: i64 = sqlx::query_scalar(&format!(
        "SELECT {column} FROM inventory_settings WHERE account_id=?"
    ))
    .bind(account)
    .fetch_optional(&mut *db)
    .await?
    .unwrap_or(0);
    let size = state
        .tables
        .inventory
        .extensions
        .iter()
        .find(|v| {
            n(v, "InventoryType") == inventory as i64 && n(v, "EquipItemExtendCount") > extend
        })
        .or_else(|| {
            state
                .tables
                .inventory
                .extensions
                .iter()
                .filter(|v| n(v, "InventoryType") == inventory as i64)
                .last()
        })
        .map(|v| n(v, "EquipItemExtendSize"))
        .unwrap_or(8);
    let base = if inventory == 1 {
        state.tables.inventory.constant("EquipCHESTMaxCount", 40)
    } else {
        state.tables.inventory.constant("EquipItemMaxCount", 280)
    };
    let count: i64 = sqlx::query_scalar(
        "SELECT COUNT(*) FROM equip_items WHERE account_id=? AND inventory_type=?",
    )
    .bind(account)
    .bind(inventory)
    .fetch_one(db)
    .await?;
    if count + extra > base + extend * size {
        return Err(rule("EquipItemFull"));
    }
    Ok(())
}
pub(crate) async fn give(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    index: i32,
    count: i32,
    star: i32,
    custom: i32,
    rewards: &mut Rewards,
) -> Result<()> {
    if count <= 0 {
        return Err(rule("InvalidItemCount"));
    }
    let metadata = state
        .tables
        .items
        .reward_item(index)
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    if state.tables.live.find("Pet", &[("Index",index as i64)]).is_some() {
        if count > 100 {return Err(rule("InvalidItemCount"));}
        for _ in 0..count {
            let result=Box::pin(crate::api::live::add_pet(db,state,account,index as i64)).await?;
            if !result["PetResult"].is_null(){rewards.pets.push(result["PetResult"].clone());}
            rewards.items.extend(result["PetSoulResults"].as_array().into_iter().flatten().cloned());
        }
        return Ok(());
    }
    if metadata.kind == "Hero" {
        let owned: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)",
        )
        .bind(account)
        .bind(metadata.hero_index)
        .fetch_one(&mut *db)
        .await?;
        if owned {
            if metadata.duplicate_reward_index > 0 {
                Box::pin(reward(
                    db,
                    state,
                    account,
                    metadata.duplicate_reward_index,
                    rewards,
                ))
                .await?;
            }
            return Ok(());
        }
    }
    if metadata.kind == "Equip" {
        if count > 1000 {
            return Err(rule("EquipItemFull"));
        }
        capacity(db, state, account, 0, count as i64).await?;
    } else if metadata.kind != "Hero" {
        let cap = n(data(state, index)?, "ItemMaxCap");
        let old: i64 =
            sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=?")
                .bind(account)
                .bind(index)
                .fetch_optional(&mut *db)
                .await?
                .unwrap_or(0);
        if old + count as i64 > i32::MAX as i64 || (cap > 0 && old + count as i64 > cap) {
            return Err(rule("InvalidItemCount"));
        }
    }
    let equip_start = rewards.equipment.len();
    tutorial::grant_item(db, state, account, index, count, star, custom, rewards).await?;
    if metadata.kind == "Equip" && custom == 0 {
        for equip in &mut rewards.equipment[equip_start..] {
            make_options(state, index, equip, &[])?;
            save_options(db, account, equip).await?;
            if state.tables.extensions.find("EquipItem",&[("ItemIndex",index as i64)]).is_some_and(|v|n(v,"EquipType")==1){
                equip.identified=0;
                crate::api::extensions::save_equip(db,account,equip).await?;
            }
        }
    }
    sqlx::query("UPDATE items SET created_time=COALESCE(created_time,datetime('now')) WHERE account_id=? AND item_index=?").bind(account).bind(index).execute(&mut *db).await?;
    // Rewarding additional copies must preserve the item's lock in the client response.
    if let Some(result) = rewards.items.last_mut().filter(|r| r["ItemIndex"] == index) {
        let locked: i32 =
            sqlx::query_scalar("SELECT locked FROM items WHERE account_id=? AND item_index=?")
                .bind(account)
                .bind(index)
                .fetch_one(db)
                .await?;
        result["Locked"] = json!(locked);
    }
    Ok(())
}
pub(crate) async fn reward(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    index: i32,
    rewards: &mut Rewards,
) -> Result<()> {
    let row = state
        .tables
        .get_reward(index)
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    for (kind, rate, min, max) in [
        ("LuaPoint", row.lua_point_rate, row.lua_point_min, row.lua_point_max),
        ("ShopEventPoint", row.shop_event_point_rate, row.shop_event_point_min, row.shop_event_point_max),
        ("LimitedShopEventPoint", row.limited_shop_event_point_rate, row.limited_shop_event_point_value, row.limited_shop_event_point_value),
    ] {
        use rand::Rng;
        let amount = if max > 0 && rand::thread_rng().gen_range(0..1000) < rate {
            rand::thread_rng().gen_range(min.max(0)..=max.max(min).max(0)) as i64
        } else { 0 };
        if amount > 0 { rewards.currencies.push(crate::api::heroes::currency(db,account,kind,amount).await?); }
    }
    tutorial::currency(db, account, "Gold", row.roll_gold(), rewards).await?;
    tutorial::currency(db, account, "Gem", row.roll_gem(), rewards).await?;
    for drop in row.roll_items(&state.tables.reward_string_pool) {
        let (code, filter) = crate::tables::parse_item_code(&drop.item_code);
        if matches!(code.as_str(), "WorldBossPoint" | "ShakmehMiddleBossPoint" | "GuildPoint" | "GuildArenaPoint" | "PvpCoin" | "RaidPoint" | "EventDungeonPoint2" | "GloryPoint" | "EventGiftPoint" | "LuaPoint" | "GuildActivityPoint" | "GuildSuppressPoint" | "GuildWood" | "GuildStone" | "GuildMetal" | "RankingPoint" | "EventDungeonPoint3" | "EclipsePoint" | "ShopEventPoint" | "LimitedShopEventPoint" | "OrdealArenaPoint" | "TreasureHousePoint" | "EventOrvelPoint" | "ChallengeRaidPoint" | "CraftEventPoint" | "GrowWorldTreePoint") {
            let mut amount=drop.count as i64;
            if code=="ShakmehMiddleBossPoint" {
                let max=state.tables.battle.find("CurrencyType",&[("CurrencyType",48)]).map(|v|n(v,"MaxValue")).filter(|v|*v>0).ok_or_else(||rule("ItemDataNotFound"))?;
                let balance:i64=sqlx::query_scalar("SELECT COALESCE((SELECT value FROM battle_currencies WHERE account=? AND kind='ShakmehMiddleBossPoint'),0)").bind(account).fetch_one(&mut *db).await?;
                amount=amount.min((max-balance).max(0));
            }
            rewards.currencies.push(crate::api::heroes::currency(db,account,&code,amount).await?);
            continue;
        }
        if code == "EventDungeonPoint" || code == "RaidPoint" {
            tutorial::currency(db, account, &code, drop.count as i64, rewards).await?;
            continue;
        }
        let (index, count, star) = if let Some(index) = state.tables.get_item_index(&code) {
            use rand::Rng;
            (
                index,
                drop.count,
                rand::thread_rng().gen_range(drop.star_min..=drop.star_max.max(drop.star_min)),
            )
        } else if let Some((i, c, s, _)) = state.tables.roll_item_from_group_code(&code, &filter) {
            (
                i,
                c.checked_mul(drop.count)
                    .ok_or_else(|| rule("InvalidItemCount"))?,
                s,
            )
        } else {
            return Err(rule("ItemDataNotFound"));
        };
        give(
            db,
            state,
            account,
            index,
            count,
            star,
            drop.custom_option_index,
            rewards,
        )
        .await?;
    }
    Ok(())
}
pub(crate) async fn reward_response(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    mut r: Rewards,
) -> Result<Value> {
    tutorial::team_exp(db, state, account, r.team_exp_to_add, &mut r).await?;
    let mut heroes = vec![];
    for index in &r.heroes {
        heroes.push(tutorial::hero_info(db, account, *index).await?);
    }
    Ok(
        json!({"CurrencyResults":r.currencies,"ItemResults":r.items,"EquipItemResults":r.equipment,"HeroInfos":heroes,"PetInfos":r.pets,"ExpResultInfos":r.team_exp,"StaminaResultInfos":[],"ItemTimeDurationInfos":[],"EquipItemBoostInfos":[],"HeroFriendlyInfos":[],"PlayerAnyMiscs":[]}),
    )
}

macro_rules! endpoint {($($name:ident),*)=>{$(pub async fn $name(State(state):State<AppState>,body:Bytes)->Result<Json<Value>>{handle(state,body,stringify!($name)).await})*};}
endpoint!(
    break_rune,
    use_equip_option_select_item,
    use_booster_item,
    use_potion_item,
    use_package_item,
    use_package_select_item,
    use_weapon_unique_select_item,
    use_hero_select_item,
    sell_item,
    break_item,
    set_lock_rune_item,
    sell_equip,
    set_lock_equip_item,
    extend_equip,
    extend_chest,
    set_chest,
    unset_chest
);
async fn handle(state: AppState, body: Bytes, action: &str) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let mut tx = state.db.begin().await?;
    init(&mut tx, &state, account).await?;
    match execute(&mut tx, &state, account, &req, action).await {
        Ok(value) => {
            if action.starts_with("use_") {
                crate::api::progression::record(&mut tx,account,"UseItem",req.number("ItemIndex",0)?,0,req.number("ItemCount",1)?).await?;
            }
            let kind=match action {"use_potion_item"=>"UsePotionItem","use_booster_item"=>"UseBoosterItem",_=>""};
            if !kind.is_empty() {crate::api::progression::record(&mut tx,account,kind,req.number("ItemIndex",0)?,0,req.number("ItemCount",1)?.max(1)).await?;}
            tx.commit().await?;
            Ok(Json(value))
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            Ok(Json(
                json!({"BaseResult":"Success","Result":native_result(action,&code)}),
            ))
        }
        Err(e) => Err(e),
    }
}
async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let mut out = success();
    if action == "break_rune" {
        return dismantle_runes(db, state, account, req).await;
    }
    if matches!(action, "sell_item" | "break_item") {
        return sell_or_break(db, state, account, req, action == "break_item").await;
    }
    if matches!(
        action,
        "sell_equip" | "set_lock_equip_item" | "set_chest" | "unset_chest"
    ) {
        return equipment_action(db, state, account, req, action).await;
    }
    if matches!(action, "extend_equip" | "extend_chest") {
        return expand(db, state, account, req, action == "extend_chest").await;
    }
    let index = item_index(
        req,
        if action == "set_lock_rune_item" {
            "RuneItemIndex"
        } else {
            "ItemIndex"
        },
    )?;
    let item = data(state, index)?;
    if action == "set_lock_rune_item" {
        if n(item, "Type") != 2 {
            return Err(rule("InvalidItemType"));
        }
        let locked = req.number("Locked", 0)?;
        if ![0, 1].contains(&locked) {
            return Err(rule("Fail"));
        }
        let changed = sqlx::query(
            "UPDATE items SET locked=? WHERE account_id=? AND item_index=? AND count>0",
        )
        .bind(locked)
        .bind(account)
        .bind(index)
        .execute(db)
        .await?
        .rows_affected();
        if changed == 0 {
            return Err(rule("ItemNotOwned"));
        }
        return Ok(out);
    }
    let count = positive(req, "ItemCount")?;
    match action {
        "use_booster_item" => {
            let booster = state
                .tables
                .inventory
                .boosters
                .get(&index)
                .ok_or_else(|| rule("BoosterDataNotFound"))?;
            if booster["IsOnetime"] == true {
                return Err(rule("ItemTypeMismatch"));
            }
            out["ItemResult"] = consume(db, account, index, count).await?;
            out["ItemTimeDurationInfo"] =
                activate_booster(db, state, account, index, count).await?;
        }
        "use_potion_item" => {
            let potion = state
                .tables
                .inventory
                .potions
                .get(&index)
                .ok_or_else(|| rule("PotionDataNotFound"))?;
            out["ItemResult"] = consume(db, account, index, count).await?;
            let amount = n(potion, "ActionValue")
                .checked_mul(count as i64)
                .ok_or_else(|| rule("InvalidItemCount"))?;
            if amount <= 0 {
                return Err(rule("InvalidActionValue"));
            }
            match n(potion, "ActionType") {
                1 | 2 => {
                    let kind = potion["ActionSubValue"].as_str().unwrap_or("");
                    let (column, wire) = match kind {
                        "" | "Stamina" | "Chicken" => ("stamina", "Chicken"),
                        "Sword" => ("sword", "Sword"),
                        "Sword2" => ("sword2", "Sword2"),
                        "GuildRaidTicket" => ("guild_raid_ticket", "GuildRaidTicket"),
                        "WorldBossTicket" => ("world_boss_ticket", "WorldBossTicket"),
                        _ => return Err(rule("InvalidStaminaType")),
                    };
                    let amount = if n(potion, "ActionType") == 2 {
                        // StaminaTable uses the team's level-based stamina maximum.
                        let level: i32 = sqlx::query_scalar(
                            "SELECT team_level FROM user_info WHERE account_id=?",
                        )
                        .bind(account)
                        .fetch_one(&mut *db)
                        .await?;
                        let max = state
                            .tables
                            .inventory
                            .team_levels
                            .iter()
                            .find(|r| n(r, "Level") == level as i64)
                            .map(|r| n(r, "MaxStamina"))
                            .unwrap_or(0);
                        if max <= 0 {
                            return Err(rule("InvalidActionValue"));
                        }
                        max * amount / 100
                    } else {
                        amount
                    };
                    let new:i64=sqlx::query_scalar(&format!("UPDATE user_info SET {column}={column}+? WHERE account_id=? AND {column}+?<=2147483647 RETURNING {column}"))
                        .bind(amount).bind(account).bind(amount).fetch_optional(&mut *db).await?.ok_or_else(||rule("InvalidActionValue"))?;
                    out["StaminaResult"] = json!({"Type":wire,"AddValue":amount,"NewValue":new,"NextRechargeRemainTime":0,"FullRechargeRemainTime":0,"RechargeCount":0,"IsHide":false});
                }
                4 => out["CurrencyResult"] = money(db, account, "Gem", amount).await?,
                3 => {
                    let hero = item_index(req, "HeroIndex")?;
                    let row=sqlx::query("SELECT level,exp,star,transcend FROM heroes WHERE account_id=? AND hero_index=?").bind(account).bind(hero).fetch_optional(&mut *db).await?.ok_or_else(||rule("HeroNotOwned"))?;
                    let cap = state
                        .tables
                        .tutorials
                        .support
                        .hero_stars
                        .iter()
                        .find(|s| {
                            s.star == row.get::<i32, _>("star")
                                && s.transcended == row.get::<i32, _>("transcend")
                        })
                        .ok_or_else(|| rule("CreatureDataNotFound"))?
                        .max_hero_level;
                    let old: i32 = row.get("level");
                    if old >= cap {
                        return Err(rule("MaxHeroLevel"));
                    }
                    let (level, exp) = crate::tables::add_exp(
                        &state.tables.tutorials.support.hero_levels,
                        old,
                        row.get("exp"),
                        amount,
                        cap,
                    );
                    sqlx::query(
                        "UPDATE heroes SET level=?,exp=? WHERE account_id=? AND hero_index=?",
                    )
                    .bind(level)
                    .bind(exp)
                    .bind(account)
                    .bind(hero)
                    .execute(&mut *db)
                    .await?;
                    out["HeroExpResult"] = json!({"HeroIndex":hero,"AddValue":amount,"NewValue":exp,"OldLevel":old,"NewLevel":level});
                }
                6 | 10 => {
                    let chest = n(potion, "ActionType") == 10;
                    let column = if chest {
                        "chest_extend"
                    } else {
                        "inventory_extend"
                    };
                    let old: i64 = sqlx::query_scalar(&format!(
                        "SELECT {column} FROM inventory_settings WHERE account_id=?"
                    ))
                    .bind(account)
                    .fetch_one(&mut *db)
                    .await?;
                    let max = state
                        .tables
                        .inventory
                        .extensions
                        .iter()
                        .filter(|v| n(v, "InventoryType") == if chest { 1 } else { 0 })
                        .map(|v| n(v, "EquipItemExtendCount"))
                        .max()
                        .unwrap_or(0);
                    if old + amount > max {
                        return Err(rule("InvalidActionValue"));
                    }
                    sqlx::query(&format!(
                        "UPDATE inventory_settings SET {column}={column}+? WHERE account_id=?"
                    ))
                    .bind(amount)
                    .bind(account)
                    .execute(&mut *db)
                    .await?;
                    out[if chest {
                        "NewChestExtend"
                    } else {
                        "NewInventoryExtend"
                    }] = json!(old + amount);
                }
                11=>{
                    let id=item_index(req,"HeroIndex")?;
                    let h=crate::api::heroes::info(db,account,id).await?;
                    let max=state.tables.hero_shop.constant("MaxExtraTranscendPoint",15);
                    let points=n(&h,"TranscendPoint").checked_add(amount).filter(|p|*p<=max).ok_or_else(||rule("MaxHeroTranscendPoint"))?;
                    if amount<=0{return Err(rule("InvalidHeroTranscendPoint"));}
                    let mut details=crate::api::heroes::details(db,account,id).await?;details["TranscendPoint"]=json!(points);crate::api::heroes::save_details(db,account,id,&details).await?;
                    out["HeroTranscendResult"]=json!({"HeroIndex":id,"AddPointValue":amount,"NewPointValue":points});
                }
                _ => return Err(rule("InvalidAction")),
            }
            let mut buffs = vec![];
            for booster in potion["BoosterCodes"].as_array().into_iter().flatten() {
                buffs.push(
                    activate_booster(
                        db,
                        state,
                        account,
                        booster.as_i64().unwrap_or(0) as i32,
                        count,
                    )
                    .await?,
                );
            }
            out["ItemTimeDurationInfos"] = json!(buffs);
        }
        "use_package_item" => {
            let package = state
                .tables
                .inventory
                .packages
                .get(&index)
                .ok_or_else(|| rule("PackageDataNotFound"))?;
            if n(package, "MaxOpenCount") > 0 && count as i64 > n(package, "MaxOpenCount") {
                return Err(rule("InvalidItemCount"));
            }
            out["ItemResult"] = consume(db, account, index, count).await?;
            let mut results = vec![];
            for _ in 0..count {
                use rand::Rng;
                let mut roll = rand::thread_rng().gen_range(0..1000);
                let mut selected = 0;
                for i in 1..=5 {
                    let rate = n(package, &format!("RewardRate{i}"));
                    if roll < rate {
                        selected = n(package, &format!("RewardIndex{i}"));
                        break;
                    }
                    roll -= rate;
                }
                if selected <= 0 {
                    return Err(rule("PackageDataNotFound"));
                }
                let mut rewards = Rewards::default();
                reward(db, state, account, selected as i32, &mut rewards).await?;
                results.push(reward_response(db, state, account, rewards).await?);
            }
            out["RewardResults"] = json!(results);
        }
        "use_package_select_item" => {
            let selector = state
                .tables
                .inventory
                .selectors
                .get(&index)
                .or_else(|| state.tables.inventory.equipment_selectors.get(&index))
                .ok_or_else(|| rule("SelectItemDataNotFound"))?;
            let selected = item_index(req, "SelectItemIndex")?;
            if !selector["ItemIndices"]
                .as_array()
                .is_some_and(|a| a.contains(&json!(selected)))
            {
                return Err(rule("ImpossibleSelectItem"));
            }
            if state
                .tables
                .items
                .reward_item(selected)
                .is_none_or(|m| m.kind != "Item")
            {
                return Err(rule("ImpossibleSelectItem"));
            }
            out["ItemResult"] = consume(db, account, index, count).await?;
            let mut r = Rewards::default();
            give(db, state, account, selected, count, 0, 0, &mut r).await?;
            out["RewardItemResult"] = r.items[0].clone();
        }
        "use_weapon_unique_select_item" | "use_equip_option_select_item" => {
            let selector = state
                .tables
                .inventory
                .equipment_selectors
                .get(&index)
                .ok_or_else(|| rule("WeaponUniqueSelectDataNotFound"))?;
            let selected = item_index(
                req,
                if action == "use_equip_option_select_item" {
                    "EquipOptionItemIndex"
                } else {
                    "WeaponUniqueItemIndex"
                },
            )?;
            if !selector["ItemIndices"]
                .as_array()
                .is_some_and(|a| a.contains(&json!(selected)))
            {
                return Err(rule("WeaponUniqueNotSelectable"));
            }
            if state
                .tables
                .items
                .reward_item(selected)
                .is_none_or(|m| m.kind != "Equip")
            {
                return Err(rule("WeaponUniqueNotSelectable"));
            }
            let options: Vec<i32> = if action == "use_equip_option_select_item" {
                serde_json::from_str(req.text("EquipOptionIndices"))
                    .map_err(|_| rule("NotCorrectOptionCount"))?
            } else {
                vec![]
            };
            if action == "use_equip_option_select_item" {
                let max = state
                    .tables
                    .inventory
                    .equipment
                    .get(&selected)
                    .map(|e| n(e, "OptionCount"))
                    .unwrap_or(0);
                if options.iter().filter(|i| **i > 0).count() as i64
                    != n(selector, "SelectOptionCount").min(max)
                {
                    return Err(rule("NotCorrectOptionCount"));
                }
                let extra: Vec<i32> = if req.text("EquipExtraOptionIndices").is_empty() {
                    vec![]
                } else {
                    serde_json::from_str(req.text("EquipExtraOptionIndices"))
                        .map_err(|_| rule("NoAvailableOption"))?
                };
                if extra.iter().any(|i| *i != 0) && selector["SelectUniqueOption"]!=true {
                    return Err(rule("NoAvailableOption"));
                }
            }
            out["ItemResult"] = consume(db, account, index, count).await?;
            let mut r = Rewards::default();
            give(
                db,
                state,
                account,
                selected,
                count,
                n(selector, "Star") as i32,
                0,
                &mut r,
            )
            .await?;
            for equip in &mut r.equipment {
                if selector["SelectUniqueOption"]==true {
                    let extra:Vec<i32>=serde_json::from_str(req.text("EquipExtraOptionIndices")).map_err(|_|rule("NoAvailableOption"))?;
                    crate::api::extensions::valance::select_unique(state,equip,&extra)?;
                    crate::api::extensions::save_equip(db,account,equip).await?;
                }
                if !options.is_empty() {
                    make_options(state, selected, equip, &options)?;
                    save_options(db, account, equip).await?;
                }
                equip.level = n(selector, "Level") as i32;
                sqlx::query("UPDATE equip_items SET level=? WHERE account_id=? AND slot_index=?")
                    .bind(equip.level)
                    .bind(account)
                    .bind(equip.slot_index)
                    .execute(&mut *db)
                    .await?;
            }
            out["EquipItemResults"] = json!(r.equipment);
        }
        "use_hero_select_item" => {
            let selector = state
                .tables
                .inventory
                .hero_selectors
                .get(&index)
                .ok_or_else(|| rule("HeroSelectDataNotFound"))?;
            let selected = item_index(req, "HeroIndex")?;
            let choices = selector["HeroIndices"]
                .as_array()
                .ok_or_else(|| rule("HeroSelectDataNotFound"))?;
            if req.text("GetOtherReward").eq_ignore_ascii_case("true")
                || req.text("GetOtherReward") == "1"
            {
                for id in choices {
                    let owned: bool = sqlx::query_scalar(
                        "SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)",
                    )
                    .bind(account)
                    .bind(id.as_i64().unwrap_or(0))
                    .fetch_one(&mut *db)
                    .await?;
                    if !owned {
                        return Err(rule("AllHeroNotOwned"));
                    }
                }
                let mut r = Rewards::default();
                reward(
                    db,
                    state,
                    account,
                    n(selector, "AllOwnedRewardIndex") as i32,
                    &mut r,
                )
                .await?;
                let mut items = vec![consume(db, account, index, 1).await?];
                items.extend(r.items);
                out["ItemResults"] = json!(items);
                out["CurrencyResults"] = json!(r.currencies);
                out["ItemUseResults"] = json!([]);
            } else {
                if !choices.contains(&json!(selected)) {
                    return Err(rule("HeroNotSelectable"));
                }
                let owned: bool = sqlx::query_scalar(
                    "SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)",
                )
                .bind(account)
                .bind(selected)
                .fetch_one(&mut *db)
                .await?;
                if owned {
                    return Err(rule("HeroNotSelectable"));
                }
                out["ItemResults"] = json!([consume(db, account, index, 1).await?]);
                let star = n(selector, "HeroStar");
                let level = n(selector, "HeroLevel").max(1);
                let transcend = n(selector, "HeroTranscend");
                sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,star,level,transcend) SELECT ?,COALESCE(MAX(hero_id),0)+1,?,?,?,? FROM heroes WHERE account_id=?")
                    .bind(account).bind(selected).bind(star).bind(level).bind(transcend).bind(account).execute(&mut *db).await?;
                let exp = state
                    .tables
                    .tutorials
                    .support
                    .hero_stars
                    .iter()
                    .find(|s| s.star == star as i32 && s.transcended == transcend as i32)
                    .ok_or_else(|| rule("CreatureStarDataNotFound"))?
                    .get_hero_team_exp;
                let mut r = Rewards::default();
                tutorial::team_exp(db, state, account, exp, &mut r).await?;
                out["HeroResult"] = json!({"HeroInfo":tutorial::hero_info(db,account,selected).await?,"TeamExpResult":r.team_exp.first()});
                out["ItemUseResults"] = json!([]);
                out["CurrencyResults"] = json!([]);
            }
        }
        _ => return Err(rule("Fail")),
    }
    Ok(out)
}
fn pairs(req: &Request, counts_key: &str) -> Result<Vec<(i32, i32)>> {
    let ids: Vec<i32> =
        serde_json::from_str(req.text("ItemIndices")).map_err(|_| rule("InvalidItemCount"))?;
    let counts: Vec<i32> =
        serde_json::from_str(req.text(counts_key)).map_err(|_| rule("InvalidItemCount"))?;
    if ids.is_empty() || ids.len() > 100 || ids.len() != counts.len() {
        return Err(rule("InvalidItemCount"));
    }
    let mut combined = std::collections::BTreeMap::<i32, i32>::new();
    for (id, count) in ids.into_iter().zip(counts) {
        if count <= 0 {
            return Err(rule("InvalidItemCount"));
        }
        let sum = combined.entry(id).or_default();
        *sum = sum
            .checked_add(count)
            .ok_or_else(|| rule("InvalidItemCount"))?;
    }
    Ok(combined.into_iter().collect())
}
async fn sell_or_break(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    breaking: bool,
) -> Result<Value> {
    let pairs = pairs(req, if breaking { "Counts" } else { "ItemCount" })?;
    let mut gold = 0i64;
    let mut consumed = vec![];
    let mut r = Rewards::default();
    for (id, count) in pairs {
        let item = data(state, id)?;
        let locked: Option<i32> = sqlx::query_scalar(
            "SELECT locked FROM items WHERE account_id=? AND item_index=? AND count>=?",
        )
        .bind(account)
        .bind(id)
        .bind(count)
        .fetch_optional(&mut *db)
        .await?;
        if locked.is_none() {
            return Err(rule(if breaking { "ItemNotOwned" } else { "NotOwned" }));
        }
        if locked != Some(0) {
            return Err(rule("LockedItemExists"));
        }
        if breaking {
            if count > 1000 || item["Breakable"] != true {
                return Err(rule("NotBreakableItemExists"));
            }
            let reward_index = *state
                .tables
                .inventory
                .break_rewards
                .get(&id)
                .ok_or_else(|| rule("ItemBreakDataNotFound"))?;
            consumed.push(consume(db, account, id, count).await?);
            for _ in 0..count {
                reward(db, state, account, reward_index, &mut r).await?;
            }
        } else {
            if item["NotForSale"] == true || n(item, "SellGold") <= 0 {
                return Err(rule("NotForSale"));
            }
            gold = gold
                .checked_add(n(item, "SellGold") * count as i64)
                .ok_or_else(|| rule("InvalidItemCount"))?;
            consumed.push(consume(db, account, id, count).await?);
        }
    }
    let mut out = success();
    if breaking {
        out["DecItemResults"] = json!(consumed);
        out["ItemResults"] = json!(r.items);
        out["CurrencyResults"] = json!(r.currencies);
    } else {
        out["ItemResults"] = json!(consumed);
        out["CurrencyResult"] = money(db, account, "Gold", gold).await?;
    }
    Ok(out)
}
async fn equipment_action(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let ids = if action == "set_lock_equip_item" {
        vec![req.number("EquipItemSlotIndex", 0)?]
    } else {
        req.ids(if action == "sell_equip" {
            "EquipItemSlotIndices"
        } else {
            "EquipItemSlotIndex"
        })?
    };
    if ids.is_empty() {
        return Err(rule("EquipNotOwned"));
    }
    let mut gold = 0;
    let mut changed = vec![];
    for id in ids {
        let row = sqlx::query("SELECT * FROM equip_items WHERE account_id=? AND slot_index=?")
            .bind(account)
            .bind(id)
            .fetch_optional(&mut *db)
            .await?
            .ok_or_else(|| rule("EquipNotOwned"))?;
        let in_use:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM equipment_pending WHERE account_id=? AND slot_index=?) OR EXISTS(SELECT 1 FROM extension_state WHERE account_id=? AND kind='soul' AND idx=?)").bind(account).bind(id).bind(account).bind(id).fetch_one(&mut *db).await?;
        if (row.get::<i32,_>("inventory_type")>1 && action!="set_lock_equip_item") || (action!="set_lock_equip_item" && in_use) {return Err(rule("Equipped"));}
        let preset:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM extension_state s,json_each(s.data) j WHERE s.account_id=? AND s.kind='equip_storage' AND j.key LIKE 'EquipItemSlotIndex%' AND j.value=?)").bind(account).bind(id).fetch_one(&mut *db).await?;
        if preset && action!="set_lock_equip_item" {return Err(rule("Equipped"));}
        if action == "set_lock_equip_item" {
            let locked = req.number("Locked", 0)?;
            if ![0, 1].contains(&locked) {
                return Err(rule("Fail"));
            }
            sqlx::query("UPDATE equip_items SET locked=? WHERE account_id=? AND slot_index=?")
                .bind(locked)
                .bind(account)
                .bind(id)
                .execute(&mut *db)
                .await?;
            continue;
        }
        let equipped:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND ? IN (equip_item_slot_index_1,equip_item_slot_index_2,equip_item_slot_index_3,equip_item_slot_index_4,equip_item_slot_index_5,equip_item_slot_index_6,equip_item_slot_index_7,equip_item_slot_index_8,equip_item_slot_index_9,equip_item_slot_index_10))").bind(account).bind(id).fetch_one(&mut *db).await?;
        if equipped {
            return Err(rule("Equipped"));
        }
        if action == "sell_equip" {
            if row.get::<i32, _>("locked") != 0 {
                return Err(rule("LockedEquipItemExists"));
            }
            let item = data(state, row.get("item_index"))?;
            if item["NotForSale"] == true || n(item, "SellGold") <= 0 {
                return Err(rule("NotForSale"));
            }
            gold += n(item, "SellGold");
            sqlx::query("DELETE FROM equip_items WHERE account_id=? AND slot_index=?")
                .bind(account)
                .bind(id)
                .execute(&mut *db)
                .await?;
        } else {
            let destination = if action == "set_chest" { 1 } else { 0 };
            if row.get::<i32, _>("inventory_type") != destination {
                capacity(db, state, account, destination, 1).await?;
            }
            sqlx::query(
                "UPDATE equip_items SET inventory_type=? WHERE account_id=? AND slot_index=?",
            )
            .bind(destination)
            .bind(account)
            .bind(id)
            .execute(&mut *db)
            .await?;
            changed.push(id);
        }
    }
    let mut out = success();
    if action == "sell_equip" {
        out["CurrencyResult"] = money(db, account, "Gold", gold).await?;
    } else {
        out[if action == "set_chest" {
            "EquippedSlotIndexInChest"
        } else {
            "UnequippedSlotIndexInChest"
        }] = json!(changed);
    }
    Ok(out)
}
async fn expand(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    chest: bool,
) -> Result<Value> {
    let column = if chest {
        "chest_extend"
    } else {
        "inventory_extend"
    };
    let count: i64 = sqlx::query_scalar(&format!(
        "SELECT {column} FROM inventory_settings WHERE account_id=?"
    ))
    .bind(account)
    .fetch_one(&mut *db)
    .await?;
    let tier = state
        .tables
        .inventory
        .extensions
        .iter()
        .filter(|r| {
            n(r, "InventoryType") == if chest { 1 } else { 0 }
                && n(r, "EquipItemExtendCount") > count
        })
        .min_by_key(|r| n(r, "EquipItemExtendCount"))
        .ok_or_else(|| rule("ReqGemError"))?;
    let price = n(tier, "EquipItemExtendPrice");
    if req.number("ReqGem", 0)? != price {
        return Err(rule("ReqGemError"));
    }
    let mut out = success();
    out["CurrencyResult"] = money(db, account, "Gem", -price).await?;
    sqlx::query(&format!(
        "UPDATE inventory_settings SET {column}={column}+1 WHERE account_id=?"
    ))
    .bind(account)
    .execute(db)
    .await?;
    out[if chest {
        "NewChestExtend"
    } else {
        "NewInventoryExtend"
    }] = json!(count + 1);
    Ok(out)
}

pub async fn get_inventory(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let account = Request::parse(&body)?.account(&state)?;
    let rows =
        sqlx::query("SELECT * FROM items WHERE account_id=? AND count>0 ORDER BY item_index")
            .bind(account)
            .fetch_all(&state.db)
            .await?;
    let items:Vec<_>=rows.iter().map(|r|json!({"ItemIndex":r.get::<i32,_>("item_index"),"Count":r.get::<i32,_>("count"),"Locked":r.get::<i32,_>("locked"),"CreatedTime":r.get::<Option<String>,_>("created_time"),"Uid":r.get::<i32,_>("item_index").to_string()})).collect();
    let equip: Vec<_> =
        sqlx::query("SELECT * FROM equip_items WHERE account_id=? ORDER BY slot_index")
            .bind(account)
            .fetch_all(&state.db)
            .await?
            .iter()
            .map(EquipItemInfo::from_row)
            .collect();
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","Items":items,"EquipItems":equip}),
    ))
}

pub(crate) async fn activate_booster(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    index: i32,
    count: i32,
) -> Result<Value> {
    let data = state
        .tables
        .inventory
        .boosters
        .get(&index)
        .ok_or_else(|| rule("BoosterDataNotFound"))?;
    if !matches!(n(data, "Type"), 1 | 3) || data["IsOnetime"] == true {
        return Err(rule("ItemTypeMismatch"));
    }
    let duration = n(data, "Duration")
        .checked_mul(count as i64)
        .ok_or_else(|| rule("Fail"))?;
    if duration <= 0 || duration > 31536000 {
        return Err(rule("Fail"));
    }
    let now = state.server_time_str();
    let row=sqlx::query("INSERT INTO item_boosters(account_id,item_index,start_time,end_time) VALUES (?,?,?,datetime(?,?)) ON CONFLICT(account_id,item_index) DO UPDATE SET start_time=CASE WHEN end_time>excluded.start_time THEN start_time ELSE excluded.start_time END,end_time=datetime(MAX(end_time,excluded.start_time),?) RETURNING start_time,end_time")
        .bind(account).bind(index).bind(&now).bind(&now).bind(format!("+{duration} seconds")).bind(format!("+{duration} seconds")).fetch_one(db).await?;
    Ok(
        json!({"ItemIndex":index,"StartTime":row.get::<String,_>("start_time"),"EndTime":row.get::<String,_>("end_time")}),
    )
}
pub(crate) async fn booster_login(state: &AppState, account: i64) -> Result<Vec<Value>> {
    let rows =
        sqlx::query("SELECT * FROM item_boosters WHERE account_id=? AND end_time>datetime('now')")
            .bind(account)
            .fetch_all(&state.db)
            .await?;
    Ok(rows.iter().map(|r|json!({"ItemIndex":r.get::<i32,_>("item_index"),"StartTime":r.get::<String,_>("start_time"),"EndTime":r.get::<String,_>("end_time")})).collect())
}
pub(crate) async fn campaign_boost(state: &AppState, account: i64) -> Result<(i32, i32)> {
    let rows = booster_login(state, account).await?;
    let mut gold = 0;
    let mut exp = 0;
    for row in rows {
        if let Some(data) = state
            .tables
            .inventory
            .boosters
            .get(&(n(&row, "ItemIndex") as i32))
        {
            if !data["BattleTypes"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(1)))
            {
                continue;
            }
            match n(data, "Type") {
                1 => exp = exp.max(n(data, "Value") as i32),
                3 => gold = gold.max(n(data, "Value") as i32),
                _ => {}
            }
        }
    }
    let (costume_gold,costume_exp)=crate::api::heroes::costume_boost(state,account).await?;
    Ok((gold+costume_gold, exp+costume_exp))
}

fn option_pool(state: &AppState, eq: &Value) -> Vec<i32> {
    let mut pool = std::collections::BTreeSet::new();
    for id in eq["OptionIndex"].as_array().into_iter().flatten() {
        if let Some(id) = id.as_i64() {
            pool.insert(id as i32);
        }
    }
    for group in eq["OptionGroupIndex"].as_array().into_iter().flatten() {
        if let Some(ids) = state
            .tables
            .inventory
            .option_groups
            .get(&(group.as_i64().unwrap_or(0) as i32))
        {
            pool.extend(ids);
        }
    }
    pool.into_iter()
        .filter(|i| {
            state
                .tables
                .inventory
                .options
                .get(i)
                .is_some_and(|v| n(v, "Ratio") > 0)
        })
        .collect()
}
pub(crate) fn make_options(
    state: &AppState,
    index: i32,
    item: &mut EquipItemInfo,
    chosen: &[i32],
) -> Result<()> {
    use rand::Rng;
    let eq = state
        .tables
        .inventory
        .equipment
        .get(&index)
        .ok_or_else(|| rule("EquipItemDataNotFound"))?;
    let count = n(eq, "OptionCount").clamp(0, 4) as usize;
    if chosen.len() > count {
        return Err(rule("NotCorrectOptionCount"));
    }
    let pool = option_pool(state, eq);
    let duplicates = eq["EnableDuplicationOption"] == true;
    let reserved: std::collections::BTreeSet<_> = chosen
        .iter()
        .filter(|i| **i > 0)
        .map(|i| {
            state
                .tables
                .inventory
                .options
                .get(i)
                .map(|v| n(v, "Type"))
                .ok_or_else(|| rule("NoAvailableOption"))
        })
        .collect::<Result<_>>()?;
    let mut values = vec![];
    let mut seen = std::collections::BTreeSet::new();
    for slot in 0..count {
        let chosen = chosen.get(slot).copied().unwrap_or(0);
        let available: Vec<_> = pool
            .iter()
            .copied()
            .filter(|i| {
                duplicates
                    || (!seen.contains(&n(&state.tables.inventory.options[i], "Type"))
                        && (chosen != 0
                            || !reserved.contains(&n(&state.tables.inventory.options[i], "Type"))))
            })
            .collect();
        let id = if chosen != 0 {
            if !available.contains(&chosen) {
                return Err(rule("NoAvailableOption"));
            }
            chosen
        } else {
            let total: i64 = available
                .iter()
                .map(|i| n(&state.tables.inventory.options[i], "Ratio"))
                .sum();
            if total <= 0 {
                return Err(rule("NoAvailableOption"));
            }
            let mut roll = rand::thread_rng().gen_range(0..total);
            *available
                .iter()
                .find(|i| {
                    roll -= n(&state.tables.inventory.options[i], "Ratio");
                    roll < 0
                })
                .unwrap()
        };
        let option = &state.tables.inventory.options[&id];
        seen.insert(n(option, "Type"));
        let steps = n(option, "Steps").max(0) as i32;
        values.push((
            id,
            if chosen != 0 {
                steps
            } else {
                rand::thread_rng().gen_range(0..=steps)
            },
        ));
    }
    values.resize(4, (0, 0));
    (item.option_index_1, item.option_step_1) = values[0];
    (item.option_index_2, item.option_step_2) = values[1];
    (item.option_index_3, item.option_step_3) = values[2];
    (item.option_index_4, item.option_step_4) = values[3];
    item.rune_slot_count = rand::thread_rng()
        .gen_range(n(eq, "MinRuneCount")..=n(eq, "MaxRuneCount").max(n(eq, "MinRuneCount")))
        as i32;
    Ok(())
}
async fn save_options(db: &mut SqliteConnection, account: i64, item: &EquipItemInfo) -> Result<()> {
    sqlx::query("UPDATE equip_items SET option_index_1=?,option_step_1=?,option_index_2=?,option_step_2=?,option_index_3=?,option_step_3=?,option_index_4=?,option_step_4=?,rune_slot_count=? WHERE account_id=? AND slot_index=?")
        .bind(item.option_index_1).bind(item.option_step_1).bind(item.option_index_2).bind(item.option_step_2).bind(item.option_index_3).bind(item.option_step_3).bind(item.option_index_4).bind(item.option_step_4).bind(item.rune_slot_count).bind(account).bind(item.slot_index).execute(db).await?;
    Ok(())
}
async fn dismantle_runes(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let batch = if matches!(req.text("ItemIndices"),""|"[]"|"null"){vec![]}else{pairs(req, "Counts")?};
    let mut consumed = vec![];
    let mut rewards = Rewards::default();
    if !matches!(req.text("EquipItemSlotIndices"),""|"[]"|"null") {
        crate::api::extensions::punishment::dismantle(db,state,account,req,&mut rewards).await?;
    }
    for (id, count) in batch {
        if count > 1000 {
            return Err(rule("InvalidItemCount"));
        }
        let metadata = data(state, id)?;
        if n(metadata, "Type") != 2 {
            return Err(rule("InvalidItemType"));
        }
        let locked: Option<i32> =
            sqlx::query_scalar("SELECT locked FROM items WHERE account_id=? AND item_index=?")
                .bind(account)
                .bind(id)
                .fetch_optional(&mut *db)
                .await?;
        if locked == Some(1) {
            return Err(rule("LockedItemExists"));
        }
        let drops = state
            .tables
            .inventory
            .rune_breaks
            .get(&(n(metadata, "Grade") as i32))
            .ok_or_else(|| rule("NotBreakableItemExists"))?;
        consumed.push(consume(db, account, id, count).await?);
        for drop in drops {
            use rand::Rng;
            let total: i64 = (0..count)
                .map(|_| rand::thread_rng().gen_range(n(drop, "Min")..=n(drop, "Max")))
                .sum();
            give(
                db,
                state,
                account,
                n(drop, "ItemIndex") as i32,
                i32::try_from(total).map_err(|_| rule("InvalidItemCount"))?,
                0,
                0,
                &mut rewards,
            )
            .await?;
        }
    }
    Ok(
        json!({"BaseResult":"Success","Result":"Success","DecItemResults":consumed,"ItemResults":rewards.items,"RemoveEquipItemSlotIndices":req.ids("EquipItemSlotIndices")?}),
    )
}

// Only return result names present in the native endpoint enum.
pub(crate) fn native_result(action: &str, code: &str) -> String {
    let code = match (action, code) {
        ("set_chest", "Equipped") => "AlreadyOtherHeroEquipped",
        ("set_chest", "EquipItemFull") => "ChestFull",
        ("unset_chest", "EquipItemFull") => "InventoryFull",
        ("unset_chest", "EquipNotOwned") => "EquipItemNotFound",
        _ => code,
    };
    let allowed: &[&str] = match action {
        "break_rune" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "InvalidItemCount",
            "Equipped",
            "EquipNotOwned",
            "NotBreakableItemExists",
            "LockedItemExists",
            "LockedEquipItemExists",
            "ItemDataNotFound",
            "InvalidItemType",
        ],
        "use_equip_option_select_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "ItemDataNotFound",
            "ItemTypeMismatch",
            "WeaponUniqueSelectDataNotFound",
            "WeaponUniqueNotSelectable",
            "EquipItemDataNotFound",
            "EquipItemFull",
            "NoAvailableOption",
            "NotCorrectOptionCount",
            "ItemCountError",
        ],
        "use_booster_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "ItemDataNotFound",
            "ItemTypeMismatch",
            "SameBoosterApplied",
            "BoosterDataNotFound",
        ],
        "use_potion_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "ItemDataNotFound",
            "ItemTypeMismatch",
            "PotionDataNotFound",
            "CreatureDataNotFound",
            "InvalidAction",
            "InvalidItemCount",
            "InvalidHeroIndex",
            "InvalidHeroExp",
            "MaxHeroTranscendPoint",
            "InvalidHeroTranscendPoint",
            "HeroNotOwned",
            "MaxHeroLevel",
            "InvalidActionValue",
            "InvalidStaminaType",
            "ItemExpired",
            "InvalidChapterIndex",
            "PlayerDungeonNotFound",
            "NotFoundChapterData",
            "IsNotOpenDungeon",
            "NotAvailableRecharge",
            "NotHeroTanscended",
        ],
        "use_package_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "InvalidItemCount",
            "ItemDataNotFound",
            "ItemTypeMismatch",
            "PackageDataNotFound",
            "EquipItemFull",
        ],
        "use_package_select_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "InvalidItemCount",
            "ItemDataNotFound",
            "ItemTypeMismatch",
            "SelectItemDataNotFound",
            "ImpossibleSelectItem",
            "EquipItemFull",
        ],
        "use_weapon_unique_select_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "ItemDataNotFound",
            "WeaponUniqueSelectDataNotFound",
            "WeaponUniqueNotSelectable",
            "ItemTypeMismatch",
            "EquipItemDataNotFound",
            "EquipItemFull",
            "ItemCountError",
            "WeaponUniqueSelectDataValueError",
        ],
        "use_hero_select_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "ItemDataNotFound",
            "ItemTypeMismatch",
            "HeroSelectDataNotFound",
            "HeroNotSelectable",
            "HeroFragmentDataError",
            "HeroDataNotFound",
            "AwakeChallengeDataError",
            "AwakeItemDataError",
            "CreatureStarDataNotFound",
            "AllHeroNotOwned",
        ],
        "sell_item" => &[
            "Fail",
            "Success",
            "NoItemData",
            "NoSellCount",
            "NotOwned",
            "InvalidItemCount",
            "LockedItemExists",
            "NotForSale",
        ],
        "break_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "InvalidItemCount",
            "Equipped",
            "NotBreakableItemExists",
            "LockedItemExists",
            "ItemDataNotFound",
            "ItemBreakDataNotFound",
            "InvalidItemType",
        ],
        "set_lock_rune_item" => &[
            "Fail",
            "Success",
            "ItemNotOwned",
            "ItemDataNotFound",
            "InvalidItemType",
            "AlreadyLocked",
            "AlreadyUnLocked",
        ],
        "sell_equip" => &[
            "Fail",
            "Success",
            "EquipNotOwned",
            "EquipDataNotFound",
            "Equipped",
            "NotForSale",
            "LockedEquipItemExists",
        ],
        "set_lock_equip_item" => &[
            "Fail",
            "Success",
            "EquipNotOwned",
            "ItemDataNotFound",
            "EquipItemDataNotFound",
            "AlreadyLocked",
            "AlreadyUnLocked",
        ],
        "set_chest" => &[
            "Fail",
            "Success",
            "EquipNotOwned",
            "AlreadyOtherHeroEquipped",
            "ItemDataNotFound",
            "EquipItemDataNotFound",
            "LockedPartIndex",
            "EquipPartError",
            "UnequipOldFailed",
            "EquipSlotDataNotFound",
            "NotMatchSubType",
            "ChestFull",
        ],
        "unset_chest" => &[
            "Fail",
            "Success",
            "EquipItemNotFound",
            "RemoveFailed",
            "InventoryFull",
        ],
        "extend_equip" => &["Fail", "Success", "ReqGemError", "NotEnoughGem"],
        "extend_chest" => &["Fail", "Success", "ReqGemError", "NotEnoughGem", "MaxChest"],
        "craft_item" => &[
            "Fail",
            "Success",
            "WrongSlotIndex",
            "NotSelectableSlotIndex",
            "ItemNotOwned",
            "ItemDataNotFound",
            "InvalidItemCount",
            "CraftDataNotFound",
            "NotEnoughMaterial",
            "NotEnoughGold",
            "CraftDataNotOpened",
        ],
        "add_craft_slot" => &[
            "Fail",
            "Success",
            "WrongSlotIndex",
            "NotSelectableSlotIndex",
            "CraftSlotPriceDataNotFound",
            "NotEnoughGold",
        ],
        "take_craft_item" => &[
            "Fail",
            "Success",
            "WrongSlotIndex",
            "NotSelectableSlotIndex",
            "NotCraftSlotIndex",
            "ItemDataNotFound",
            "CraftDataNotFound",
            "NotYetCrafted",
        ],
        "cancel_craft_item" => &[
            "Fail",
            "Success",
            "WrongSlotIndex",
            "NotSelectableSlotIndex",
            "NotCraftSlotIndex",
            "AlreadyCompletedCraftItem",
            "ItemDataNotFound",
            "CraftDataNotFound",
            "NotEnoughMaterial",
            "NotEnoughGold",
        ],
        "instant_craft_item" => &[
            "Fail",
            "Success",
            "WrongSlotIndex",
            "NotSelectableSlotIndex",
            "NotCraftSlotIndex",
            "AlreadyCompletedCraftItem",
            "ItemDataNotFound",
            "CraftDataNotFound",
            "NotEnoughGem",
            "WrongRemainTime",
        ],
        _ => &["Fail"],
    };
    if allowed.contains(&code) {
        code.into()
    } else {
        "Fail".into()
    }
}
