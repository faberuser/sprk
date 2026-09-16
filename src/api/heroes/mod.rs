//! Native hero ownership, appearance, and progression operations.
pub mod inn;
pub mod presets;

#[cfg(test)]
mod tests;

use crate::api::{
    inventory::item::{self, n, rule},
    system::request::Request,
    tutorial::{self, Rewards},
};
use crate::{
    error::{Result, ServerError},
    models::hero::HeroInfo,
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};
use std::collections::BTreeSet;

macro_rules! endpoints {($($name:ident),*)=>{$(pub async fn $name(State(state):State<AppState>,body:Bytes)->Result<Json<Value>> {handle(state,body,stringify!($name)).await})*};}
endpoints!(
    buy_hero_storage_slot,
    add_hero_storage_slot,
    set_hero_storage_slot,
    remove_hero_storage_slot,
    change_hero_storage_slot_name,
    use_costume_select_item,
    use_hero_growth_item,
    use_multiple_hero_select_item,
    buy_hero,
    buy_costume,
    set_costume,
    unset_costume,
    bookmark_hero,
    change_avatar_hero,
    set_hide_hero_unique_weapon,
    save_costume_storage_slot,
    set_costume_storage_slot,
    learn_hero_skill,
    upgrade_hero_skill,
    extend_hero_skill,
    get_awake_material_challenge,
    purify_hero,
    max_purify_hero,
    upgrade_hero_star,
    transcend_hero,
    learn_hero_transcend_skill,
    reset_hero_transcend_skill,
    buy_hero_transcend_skill_page,
    apply_hero_transcend_skill_page,
    learn_hero_transcend_skill_page,
    reset_hero_transcend_skill_page,
    hero_limit_break_level_up,
    hero_limit_break_exp_up
);

pub(crate) async fn details(db: &mut SqliteConnection, account: i64, index: i32) -> Result<Value> {
    let s: Option<String> =
        sqlx::query_scalar("SELECT data FROM hero_details WHERE account_id=? AND hero_index=?")
            .bind(account)
            .bind(index)
            .fetch_optional(db)
            .await?;
    Ok(s.map(|s| serde_json::from_str(&s))
        .transpose()
        .map_err(|e| ServerError::Internal(e.to_string()))?
        .unwrap_or(json!({})))
}
pub(crate) async fn save_details(
    db: &mut SqliteConnection,
    account: i64,
    index: i32,
    data: &Value,
) -> Result<()> {
    sqlx::query("INSERT INTO hero_details(account_id,hero_index,data) VALUES (?,?,?) ON CONFLICT(account_id,hero_index) DO UPDATE SET data=excluded.data").bind(account).bind(index).bind(data.to_string()).execute(db).await?;
    Ok(())
}
pub(crate) async fn info(db: &mut SqliteConnection, account: i64, index: i32) -> Result<Value> {
    let row = sqlx::query("SELECT * FROM heroes WHERE account_id=? AND hero_index=?")
        .bind(account)
        .bind(index)
        .fetch_optional(&mut *db)
        .await?
        .ok_or_else(|| rule("NoHero"))?;
    let mut out = json!(HeroInfo::new(row.get("hero_id"), index, row.get("star")));
    for (wire, col) in [
        ("Level", "level"),
        ("Exp", "exp"),
        ("Transcended", "transcend"),
        ("Awakened", "awakened"),
        ("ClosenessPoint", "closeness"),
        ("UniqueWeaponId", "unique_weapon_id"),
    ] {
        out[wire] = json!(row.get::<i64, _>(col));
    }
    out["IsBookmarked"] = json!(row.get::<i64, _>("is_bookmarked") != 0);
    for i in 1..=4 {
        out[format!("SkillLevel{i}")] =
            json!(row.get::<i64, _>(format!("skill_level_{i}").as_str()));
    }
    for i in 1..=10 {
        out[format!("EquipItemSlotIndex{i}")] =
            json!(row.get::<i64, _>(format!("equip_item_slot_index_{i}").as_str()));
    }
    for (k, v) in details(&mut *db, account, index)
        .await?
        .as_object()
        .into_iter()
        .flatten()
    {
        out[k] = v.clone();
    }
    if out["TranscendSkillPage1"].is_null() {
        out["TranscendSkillPage1"] = json!("[]");
    }
    if out["ApplySkillPage"].is_null() {
        out["ApplySkillPage"] = json!(1);
    }
    Ok(out)
}
pub(crate) async fn snapshot(db: &mut SqliteConnection, account: i64) -> Result<Vec<HeroInfo>> {
    let indices: Vec<i32> =
        sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=? ORDER BY hero_id")
            .bind(account)
            .fetch_all(&mut *db)
            .await?;
    let mut result = vec![];
    for index in indices {
        result.push(
            serde_json::from_value(info(&mut *db, account, index).await?)
                .map_err(|e| ServerError::Internal(e.to_string()))?,
        );
    }
    Ok(result)
}
pub(crate) async fn costumes(db: &mut SqliteConnection, account: i64) -> Result<Vec<Value>> {
    Ok(sqlx::query("SELECT costume_index,created_time FROM costumes WHERE account_id=? ORDER BY costume_index").bind(account).fetch_all(db).await?.iter().map(|r|json!({"CostumeIndex":r.get::<i32,_>("costume_index"),"CreatedTime":r.get::<String,_>("created_time"),"ColorCostumeIndices":[]})).collect())
}
pub(crate) async fn presets(db: &mut SqliteConnection, account: i64) -> Result<Vec<Value>> {
    let rows: Vec<String> = sqlx::query_scalar(
        "SELECT data FROM costume_presets WHERE account_id=? ORDER BY hero_index,slot_index",
    )
    .bind(account)
    .fetch_all(db)
    .await?;
    rows.iter()
        .map(|s| serde_json::from_str(s).map_err(|e| ServerError::Internal(e.to_string())))
        .collect()
}
pub(crate) async fn currency(
    db: &mut SqliteConnection,
    account: i64,
    kind: &str,
    amount: i64,
) -> Result<Value> {
    if matches!(kind, "Gold" | "Gem") {
        return item::money(db, account, kind, amount).await;
    }
    if matches!(kind, "WorldBossPoint" | "ShakmehMiddleBossPoint" | "GuildPoint" | "GuildArenaPoint") {
        sqlx::query("INSERT OR IGNORE INTO battle_currencies(account,kind,value) VALUES(?,?,0)")
            .bind(account).bind(kind).execute(&mut *db).await?;
        let value: Option<i64> = sqlx::query_scalar("UPDATE battle_currencies SET value=value+? WHERE account=? AND kind=? AND value+? BETWEEN 0 AND 2147483647 RETURNING value")
            .bind(amount).bind(account).bind(kind).bind(amount).fetch_optional(db).await?;
        let value = value.ok_or_else(|| rule(&format!("NotEnough{kind}")))?;
        return Ok(json!({"CurrencyType":kind,"AddValue":amount,"NewValue":value,"AddDailyAccValue":0,"NewDailyAccValue":0}));
    }
    let col = match kind {
        "Mileage" => "mileage",
        "FriendshipPoint" => "friendship_point",
        "PvpCoin" => "pvp_coin",
        "RoyalPoint" => "royal_point",
        "RaidPoint" => "raid_point",
        _ => return Err(rule("InvalidCost")),
    };
    let value:Option<i64>=sqlx::query_scalar(&format!("UPDATE user_info SET {col}={col}+? WHERE account_id=? AND {col}+? BETWEEN 0 AND 2147483647 RETURNING {col}")).bind(amount).bind(account).bind(amount).fetch_optional(&mut *db).await?;
    let value = value.ok_or_else(|| rule(&format!("NotEnough{kind}")))?;
    let daily: i64 = if kind == "FriendshipPoint" {
        sqlx::query_scalar(
            "SELECT points FROM friend_daily WHERE account_id=? AND day=strftime('%Y-%m-%d','now')",
        )
        .bind(account)
        .fetch_optional(db)
        .await?
        .unwrap_or(0)
    } else {
        0
    };
    Ok(
        json!({"CurrencyType":kind,"AddValue":amount,"NewValue":value,"AddDailyAccValue":0,"NewDailyAccValue":daily}),
    )
}
async fn handle(state: AppState, body: Bytes, action: &str) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let mut tx = state.db.begin().await?;
    item::init(&mut tx, &state, account).await?;
    match execute(&mut tx, &state, account, &req, action).await {
        Ok(v) => {
            tx.commit().await?;
            Ok(Json(v))
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            Ok(Json(
                json!({"BaseResult":"Success","Result":state.tables.hero_shop.result(action,&code)}),
            ))
        }
        Err(e) => Err(e),
    }
}
pub(crate) fn appearance(hero: &Value) -> Value {
    let mut r = json!({});
    for k in [
        "HeroIndex",
        "CostumeIndex",
        "HideUniqueWeapon",
        "WeaponCostumeIndex",
        "HairCostumeIndex",
        "AccessoryCostumeIndex1",
        "AccessoryCostumeIndex2",
        "AccessoryCostumeIndex3",
        "AccessoryCostumeIndex4",
        "AccessoryCostumeIndex5",
        "AccessoryCostumeIndex6",
    ] {
        r[k] = json!(n(hero, k));
    }
    r
}
fn star<'a>(state: &'a AppState, s: i64, t: i64) -> Result<&'a Value> {
    state
        .tables
        .hero_shop
        .stars
        .iter()
        .find(|v| n(v, "Star") == s && n(v, "Transcended") == t)
        .ok_or_else(|| rule("CannotTranscendMore"))
}
fn next_star<'a>(state: &'a AppState, h: &Value) -> Result<&'a Value> {
    let s = n(h, "Star");
    let t = n(h, "Transcended");
    star(
        state,
        if s < 5 { s + 1 } else { s },
        if s == 5 { t + 1 } else { t },
    )
}
pub(crate) async fn check_costume(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    h: &Value,
    id: i32,
) -> Result<()> {
    if id == 0 {
        return Ok(());
    }
    let c = state
        .tables
        .hero_shop
        .costumes
        .get(&id)
        .ok_or_else(|| rule("CostumeDataNotFound"))?;
    if n(c, "HeroIndex") != n(h, "HeroIndex") {
        return Err(rule("NotCorrectHero"));
    }
    if c["IsDefault"] == true {
        if n(h, "Star") < n(c, "HeroStar") || n(h, "Transcended") < n(c, "HeroTranscended") {
            return Err(rule("CannotSetCostume"));
        }
    } else {
        let owned: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM costumes WHERE account_id=? AND costume_index=?)",
        )
        .bind(account)
        .bind(id)
        .fetch_one(db)
        .await?;
        if !owned {
            return Err(rule("CostumeNotOwned"));
        }
    }
    Ok(())
}
pub(crate) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let table = &state.tables.hero_shop;
    let mut out = item::success();
    if action == "bookmark_hero" {
        let indices: Vec<i32> =
            serde_json::from_str(req.text("HeroIndex")).map_err(|_| rule("InvalidHeroIndex"))?;
        if indices.len() > 103 || indices.iter().collect::<BTreeSet<_>>().len() != indices.len() {
            return Err(rule("InvalidHeroIndex"));
        }
        for i in &indices {
            info(db, account, *i as i32)
                .await
                .map_err(|_| rule("InvalidHeroIndex"))?;
        }
        sqlx::query("UPDATE heroes SET is_bookmarked=0 WHERE account_id=?")
            .bind(account)
            .execute(&mut *db)
            .await?;
        let len = indices.len();
        for (pos, i) in indices.into_iter().enumerate() {
            sqlx::query("UPDATE heroes SET is_bookmarked=? WHERE account_id=? AND hero_index=?")
                .bind((len - pos) as i32)
                .bind(account)
                .bind(i)
                .execute(&mut *db)
                .await?;
        }
        return Ok(out);
    }
    if action.ends_with("hero_storage_slot") || action == "change_hero_storage_slot_name" {
        return crate::api::heroes::presets::execute(db, state, account, req, action).await;
    }
    if action.starts_with("use_") {
        return collection_item(db, state, account, req, action).await;
    }
    let index = item::item_index(req, "HeroIndex")?;
    let creature = table
        .heroes
        .get(&index)
        .ok_or_else(|| rule("CreatureDataNotFound"))?;
    if action == "buy_hero" {
        if creature["Buyable"] != true {
            return Err(rule("InvalidItemData"));
        }
        let id = item::item_index(req, "ItemIndex")?;
        let meta = state
            .tables
            .items
            .reward_item(id)
            .ok_or_else(|| rule("InvalidItemIndex"))?;
        if meta.kind != "Hero"
            || meta.hero_index != index
            || meta.star as i64 != n(creature, "StartHeroStar")
            || meta.transcend != 0
            || meta.level as i64 != n(creature, "StartHeroLevel").max(1)
        {
            return Err(rule("InvalidItemIndex"));
        }
        let exists: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)",
        )
        .bind(account)
        .bind(index)
        .fetch_one(&mut *db)
        .await?;
        if exists {
            return Err(rule("HeroAlreadyExist"));
        }
        let price = table
            .prices
            .iter()
            .find(|v| {
                n(v, "Star") == n(creature, "StartHeroStar")
                    && n(v, "OpenStatus") == n(creature, "OpenStatus")
            })
            .ok_or_else(|| rule("InvalidItemData"))?;
        let gem = n(price, "BuyGem");
        if gem <= 0 || req.number("BuyGem", -1)? != gem {
            return Err(rule("InvalidPrice"));
        }
        let mut currencies = vec![currency(db, account, "Gem", -gem).await?];
        if n(price, "Mileage") > 0 {
            currencies.push(currency(db, account, "Mileage", n(price, "Mileage")).await?);
        }
        let mut rewards = Rewards::default();
        item::give(db, state, account, id, 1, 0, 0, &mut rewards).await?;
        tutorial::team_exp(db, state, account, rewards.team_exp_to_add, &mut rewards).await?;
        let mut extra = details(db, account, index).await?;
        extra["CreatedTime"] = json!(chrono::Utc::now().format("%Y-%m-%d %H:%M:%S").to_string());
        save_details(db, account, index, &extra).await?;
        out["CurrencyResults"] = json!(currencies);
        out["HeroResult"] = json!({"HeroInfo":info(db,account,index).await?,"TeamExpResult":rewards.team_exp.first(),"StaminaResult":null,"HeroFriendlyInfo":null});
        return Ok(out);
    }
    let mut h = info(db, account, index).await?;
    let mut extra = details(db, account, index).await?;
    match action {
        "buy_costume" => {
            let id = item::item_index(req, "CostumeIndex")?;
            let c = table
                .costumes
                .get(&id)
                .ok_or_else(|| rule("CostumeDataNotFound"))?;
            if n(c, "HeroIndex") != index as i64 {
                return Err(rule("NotCorrectHero"));
            }
            if c["Buyable"] != true {
                return Err(rule("NotForSale"));
            }
            let owned: bool = sqlx::query_scalar(
                "SELECT EXISTS(SELECT 1 FROM costumes WHERE account_id=? AND costume_index=?)",
            )
            .bind(account)
            .bind(id)
            .fetch_one(&mut *db)
            .await?;
            if owned {
                return Err(rule("AlreadyHaveCostume"));
            }
            let mut currencies = vec![];
            for kind in ["Gem", "Gold", "Mileage"] {
                let cost = n(c, &format!("ReqBuy{kind}"));
                if req.number(&format!("Buy{kind}"), 0)? != cost {
                    return Err(rule("InvalidPrice"));
                }
                if cost > 0 {
                    currencies.push(currency(db, account, kind, -cost).await?);
                }
            }
            if n(c, "Mileage") > 0 {
                currencies.push(currency(db, account, "Mileage", n(c, "Mileage")).await?);
            }
            let mut ids = vec![id];
            for bonus in c["BonusCostumeIndices"].as_array().into_iter().flatten() {
                let b = bonus.as_i64().unwrap_or(0) as i32;
                if !table.costumes.contains_key(&b) {
                    return Err(rule("CostumeDataNotFound"));
                }
                ids.push(b);
            }
            for id in &ids {
                sqlx::query(
                    "INSERT OR IGNORE INTO costumes(account_id,costume_index) VALUES (?,?)",
                )
                .bind(account)
                .bind(id)
                .execute(&mut *db)
                .await?;
            }
            extra["CostumeIndex"] = json!(id);
            h["CostumeIndex"] = json!(id);
            out["CurrencyResults"] = json!(currencies);
            out["HeroCostumeResult"] = appearance(&h);
            out["CostumeResults"] = json!(costumes(db, account)
                .await?
                .into_iter()
                .filter(|v| ids.contains(&(n(v, "CostumeIndex") as i32)))
                .collect::<Vec<_>>());
        }
        "set_costume" | "unset_costume" => {
            let id = if action == "unset_costume" {
                0
            } else {
                item::item_index(req, "CostumeIndex")?
            };
            check_costume(db, state, account, &h, id).await?;
            extra["CostumeIndex"] = json!(id);
            h["CostumeIndex"] = json!(id);
            out["HeroCostumeResult"] = appearance(&h);
        }
        "change_avatar_hero" => {
            let avatar = req.number("AvatarHeroIndex", index as i64 * 10 + n(&h, "Star"))?;
            let avatar_costume = if avatar >= 10000 {
                let id =
                    i32::try_from(avatar - 10000).map_err(|_| rule("InvalidAvatarHeroIndex"))?;
                check_costume(db, state, account, &h, id).await?;
                id
            } else {
                if avatar / 10 != index as i64 || avatar % 10 < 1 || avatar % 10 > n(&h, "Star") {
                    return Err(rule("InvalidAvatarHeroIndex"));
                }
                0
            };
            sqlx::query("UPDATE user_info SET avatar_hero_index=? WHERE account_id=?")
                .bind(avatar)
                .bind(account)
                .execute(&mut *db)
                .await?;
            let mut avatar = appearance(&h);
            avatar["CostumeIndex"] = json!(avatar_costume);
            avatar["AccountId"] = json!(account);
            out["PlayerAvatarHeroInfo"] = avatar;
        }
        "set_hide_hero_unique_weapon" => {
            let value = req.number("HideUniqueWeapon", 0)?;
            if !(0..=1).contains(&value)
                || req.number("WeaponCpstumeIndex", 0)? != n(&h, "WeaponCostumeIndex")
            {
                return Err(rule("InvalidValue"));
            }
            extra["HideUniqueWeapon"] = json!(value);
        }
        "save_costume_storage_slot" | "set_costume_storage_slot" => {
            let slot = req.number("CostumeStorageSlotIndex", 0)?;
            if !(1..=table.constant("CostumeSorageSlotMaxCount", 5)).contains(&slot) {
                return Err(rule("InvalidValue"));
            }
            let preset = if action == "save_costume_storage_slot" {
                let mut p = appearance(&h);
                p["CostumeStorageSlotIndex"] = json!(slot);
                p["Name"] = json!("");
                sqlx::query("INSERT INTO costume_presets(account_id,hero_index,slot_index,data) VALUES (?,?,?,?) ON CONFLICT(account_id,hero_index,slot_index) DO UPDATE SET data=excluded.data").bind(account).bind(index).bind(slot).bind(p.to_string()).execute(&mut *db).await?;
                p
            } else {
                let s:Option<String>=sqlx::query_scalar("SELECT data FROM costume_presets WHERE account_id=? AND hero_index=? AND slot_index=?").bind(account).bind(index).bind(slot).fetch_optional(&mut *db).await?;
                let p: Value = serde_json::from_str(&s.ok_or_else(|| rule("EmptySorageSlot"))?)
                    .map_err(|_| rule("InvalidValue"))?;
                check_costume(db, state, account, &h, n(&p, "CostumeIndex") as i32).await?;
                for key in ["CostumeIndex", "HideUniqueWeapon"] {
                    extra[key] = p[key].clone();
                }
                p
            };
            out["CostumeStorageSlotInfo"] = preset;
        }
        "learn_hero_skill" | "upgrade_hero_skill" | "extend_hero_skill" => {
            skills(
                db, state, account, req, action, creature, &h, &mut extra, &mut out,
            )
            .await?;
        }
        "get_awake_material_challenge"
        | "purify_hero"
        | "max_purify_hero"
        | "upgrade_hero_star"
        | "transcend_hero" => {
            progress(
                db, state, account, index, action, creature, &h, &mut extra, &mut out,
            )
            .await?;
        }
        "hero_limit_break_level_up" | "hero_limit_break_exp_up" => {
            limit_break(
                db, state, account, req, action, index, &h, &mut extra, &mut out,
            )
            .await?;
        }
        _ => {
            transcend_skills(
                db, state, account, req, action, index, creature, &h, &mut extra, &mut out,
            )
            .await?;
        }
    }
    save_details(db, account, index, &extra).await?;
    if action.contains("transcend_skill") {
        out["Hero"] = info(db, account, index).await?;
    }
    Ok(out)
}

fn array(req: &Request, key: &str) -> Result<Vec<i64>> {
    let v: Vec<i64> = serde_json::from_str(req.text(key)).map_err(|_| rule("InvalidValue"))?;
    if v.is_empty() || v.len() > 100 {
        return Err(rule("InvalidValue"));
    }
    Ok(v)
}
async fn skills(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
    c: &Value,
    h: &Value,
    extra: &mut Value,
    out: &mut Value,
) -> Result<()> {
    let table = &state.tables.hero_shop;
    let ids = if action == "upgrade_hero_skill" {
        array(req, "SkillIndices")?
    } else {
        vec![req.number("SkillIndex", 0)?]
    };
    let targets = if action == "upgrade_hero_skill" {
        array(req, "TargetLevels")?
    } else {
        vec![0]
    };
    if ids.len() != targets.len() || ids.iter().collect::<BTreeSet<_>>().len() != ids.len() {
        return Err(rule("SkillNotFound"));
    }
    let mut gold = 0;
    let mut book: Option<i32> = None;
    let mut count = 0;
    let mut changed = false;
    for (id, target) in ids.into_iter().zip(targets) {
        let slot = (1..=5)
            .find(|i| n(c, &format!("SkillIndex{i}")) == id && id > 0)
            .ok_or_else(|| rule("SkillNotFound"))?;
        let key = format!("SkillLevel{slot}");
        let level = n(h, &key);
        if action != "upgrade_hero_skill"
            && n(h, "Level") < table.constant(&format!("ReqHeroLevelSkillSlot{slot}"), 1)
        {
            return Err(rule("TeamLevelLimit"));
        }
        let mut required = 0;
        changed |= action != "upgrade_hero_skill" || target > level;
        match action {
            "learn_hero_skill" => {
                if level > 0 {
                    return Err(rule("SkillAlreadyLearned"));
                }
                required = table.constant("ReqItemCountSkill", 10);
                extra[&key] = json!(1);
            }
            "extend_hero_skill" => {
                if level <= 0 {
                    return Err(rule("SkillNotLearned"));
                }
                let key = format!("SkillExtend{slot}");
                let next = req.number("SkillExtend", 0)?;
                if next != n(h, &key) + 1 || next > table.constant("MaxSkillExtend", 3) {
                    return Err(rule("InvalidSkillExtend"));
                }
                required = table.constant(&format!("ReqItemCountSkillExtend{next}"), 0);
                extra[&key] = json!(next);
            }
            _ => {
                if level <= 0 || target < level || target > n(h, "Level").min(91) {
                    return Err(rule("CannotUpgradeMore"));
                }
                for lev in level..target {
                    let price = table
                        .skill_prices
                        .iter()
                        .find(|v| n(v, "SlotIndex") == slot && n(v, "Level") == lev)
                        .ok_or_else(|| rule("SkillPriceNotFound"))?;
                    gold += n(price, "ReqGold");
                    required += n(price, "ReqItemCount");
                }
                extra[&key] = json!(target);
            }
        }
        if required > 0 {
            let b = table
                .books
                .iter()
                .find(|v| n(v, "TagType") == n(c, "TagType") && n(v, "Grade") == slot)
                .ok_or_else(|| rule("BookItemDataNotFound"))?;
            let id = n(b, "ItemIndex") as i32;
            if book.is_some_and(|b| b != id) {
                return Err(rule("UpgradeBookDataNotFound"));
            }
            book = Some(id);
            count += required;
        }
        if slot <= 4 {
            sqlx::query(&format!(
                "UPDATE heroes SET skill_level_{slot}=? WHERE account_id=? AND hero_index=?"
            ))
            .bind(n(extra, &key).max(level))
            .bind(account)
            .bind(n(h, "HeroIndex"))
            .execute(&mut *db)
            .await?;
            extra.as_object_mut().unwrap().remove(&key);
        }
    }
    if !changed {
        return Err(rule("CannotUpgradeMore"));
    }
    if gold > 0 {
        out["CurrencyResult"] = currency(db, account, "Gold", -gold).await?;
    }
    if let Some(book) = book {
        out["ItemResult"] = item::consume(db, account, book, count as i32).await?;
    }
    Ok(())
}

async fn progress(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    index: i32,
    action: &str,
    c: &Value,
    h: &Value,
    extra: &mut Value,
    out: &mut Value,
) -> Result<()> {
    let table = &state.tables.hero_shop;
    let next = next_star(state, h)?;
    let awake = table
        .awake
        .iter()
        .find(|v| n(v, "HeroIndex") == index as i64)
        .ok_or_else(|| rule("AwakeDataNotFound"))?;
    let trans = n(h, "Star") == 5;
    let material = n(
        awake,
        &if trans {
            format!("TranscendMaterialItemIndex{}", n(h, "Transcended"))
        } else {
            format!("AwakeMaterialItemIndex{}", n(h, "Star"))
        },
    ) as i32;
    match action {
        "get_awake_material_challenge" => {
            if n(h, "EnterDungeon") != 0 {
                return Err(rule("AlreadyInDungeon"));
            }
            let challenges = table
                .challenges
                .iter()
                .find(|v| n(v, "TagType") == n(c, "TagType"))
                .ok_or_else(|| rule("AwakeChallengeDataError"))?;
            let mut items = vec![];
            for i in 1..=3 {
                let id = n(
                    challenges,
                    &if trans {
                        format!("TranscendItemIndex{i}")
                    } else {
                        format!("ItemIndex{i}")
                    },
                );
                items.push(
                    item::consume(db, account, id as i32, n(next, "HeroAwakeItemCount") as i32)
                        .await?,
                );
            }
            if trans && n(next, "HeroTranscendItemCount") > 0 {
                items.push(
                    item::consume(
                        db,
                        account,
                        n(next, "HeroTranscendItemIndex") as i32,
                        n(next, "HeroTranscendItemCount") as i32,
                    )
                    .await?,
                );
            }
            extra["EnterDungeon"] = json!(1);
            extra["Purified"] = json!(0);
            out["ItemResults"] = json!(items);
        }
        "purify_hero" | "max_purify_hero" => {
            if n(h, "EnterDungeon") != 2 {
                return Err(rule("NotEnoughAwakeItem"));
            }
            let old = n(h, "Purified");
            let max = n(next, "MaxPurified");
            if old >= max {
                return Err(rule("AlreadyMaxPurified"));
            }
            let mut value = old;
            let mut gold = 0;
            while value < max {
                let roll = rand::random::<u32>() % 100;
                let mut weight = 0;
                let mut amount = 0;
                for i in 1..=3 {
                    weight += n(next, &format!("Rate{i}"));
                    if (roll as i64) < weight {
                        amount = n(next, &format!("PurifyAmount{i}"));
                        break;
                    }
                }
                if amount <= 0 {
                    return Err(rule("AwakeDataNotFound"));
                }
                value = (value + amount).min(max);
                gold += n(next, "Gold");
                if action == "purify_hero" {
                    break;
                }
            }
            out["CurrencyResult"] = currency(db, account, "Gold", -gold).await?;
            let owned: i64 = sqlx::query_scalar(
                "SELECT COALESCE((SELECT count FROM items WHERE account_id=? AND item_index=?),0)",
            )
            .bind(account)
            .bind(material)
            .fetch_one(&mut *db)
            .await?;
            if owned < 1 {
                return Err(rule("NotEnoughAwakeItem"));
            }
            extra["Purified"] = json!(value);
            out["PurifyResult"] = json!({"HeroIndex":index,"AddValue":value-old,"NewValue":value});
        }
        _ => {
            if (action == "transcend_hero") != trans {
                return Err(rule("NotMaxHeroStar"));
            }
            if n(h, "Purified") < n(next, "MaxPurified") || n(h, "EnterDungeon") != 2 {
                return Err(rule("NotEnoughPurified"));
            }
            out["ItemResult"] = item::consume(db, account, material, 1).await?;
            sqlx::query("UPDATE heroes SET star=?,transcend=? WHERE account_id=? AND hero_index=?")
                .bind(n(next, "Star"))
                .bind(n(next, "Transcended"))
                .bind(account)
                .bind(index)
                .execute(&mut *db)
                .await?;
            extra["Purified"] = json!(0);
            extra["EnterDungeon"] = json!(0);
            let mut r = Rewards::default();
            tutorial::team_exp(db, state, account, n(next, "AwakeHeroTeamExp"), &mut r).await?;
            out["TeamExpResult"] = json!(r.team_exp.first());
            out[if trans {
                "NewTranscended"
            } else {
                "NewHeroStar"
            }] = json!(n(next, if trans { "Transcended" } else { "Star" }));
        }
    }
    Ok(())
}

async fn transcend_skills(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
    index: i32,
    c: &Value,
    h: &Value,
    extra: &mut Value,
    out: &mut Value,
) -> Result<()> {
    let t = n(h, "Transcended");
    if t < 1 {
        return Err(rule("NotHeroTanscended"));
    }
    let table = &state.tables.hero_shop;
    let page = req.number("PageIndex", n(h, "ApplySkillPage").max(1))?;
    if !(1..=table.constant("MaxTranscendHeroSkillPage", 6)).contains(&page) {
        return Err(rule("PageNotFound"));
    }
    let key = format!("TranscendSkillPage{page}");
    let text = h[&key].as_str().unwrap_or("");
    if action == "buy_hero_transcend_skill_page" {
        if page == 1 || !text.is_empty() {
            return Err(rule("AlreadyHavePage"));
        }
        let cost = table.constant(&format!("AddTranscendSkillPageGem{page}"), 0);
        if cost <= 0 || req.number("BuyGem", -1)? != cost {
            return Err(rule("InvalidPrice"));
        }
        out["CurrencyResult"] = currency(db, account, "Gem", -cost).await?;
        extra[&key] = json!("[]");
        return Ok(());
    }
    if text.is_empty() && page != 1 {
        return Err(rule("NotOpenPage"));
    }
    if action == "apply_hero_transcend_skill_page" {
        extra["ApplySkillPage"] = json!(page);
        return Ok(());
    }
    let old: Vec<i64> = serde_json::from_str(if text.is_empty() { "[]" } else { text })
        .map_err(|_| rule("InvalidAction"))?;
    if action.starts_with("reset_") {
        if old.is_empty() {
            return Err(rule("SkillNotLearned"));
        }
        out["CurrencyResult"] = currency(
            db,
            account,
            "Gold",
            -table.constant("ResetTranscendSkillGold", 500000),
        )
        .await?;
        extra[&key] = json!("[]");
        return Ok(());
    }
    let choices = if action == "learn_hero_transcend_skill" {
        let code = req.number("TranscendSkill", 0)?;
        let mut v = old.clone();
        let tier = code / 10;
        let pos = code % 10;
        if c[format!("TranscendSkill{tier}")]
            .get(pos as usize)
            .and_then(Value::as_i64)
            != Some(req.number("SkillIndex", 0)?)
        {
            return Err(rule("SkillNotFound"));
        }
        v.push(code);
        v
    } else {
        let raw = array(req, "TranscendSkills")?;
        if raw.len() % 2 != 0 {
            return Err(rule("InvalidAction"));
        }
        let mut v = old.clone();
        let mut seen = BTreeSet::new();
        for pair in raw.chunks_exact(2) {
            if pair[1] != 1 || !seen.insert(pair[0]) {
                return Err(rule("InvalidAction"));
            }
            if !v.contains(&pair[0]) {
                v.push(pair[0]);
            }
        }
        v
    };
    if choices.iter().collect::<BTreeSet<_>>().len() != choices.len() {
        return Err(rule("SkillAlreadyLearned"));
    }
    // Learning only adds skills. Removing an existing choice uses the paid reset endpoint.
    if old.iter().any(|v| !choices.contains(v)) {
        return Err(rule("InvalidAction"));
    }
    let mut spent = 0;
    let mut groups = BTreeSet::new();
    for code in &choices {
        let tier = code / 10;
        let pos = code % 10;
        if tier < 1 || tier > t {
            return Err(rule("NotEnoughTranscend"));
        }
        let skills = c[format!("TranscendSkill{tier}")]
            .as_array()
            .ok_or_else(|| rule("SkillNotFound"))?;
        if pos < 0 || pos as usize >= skills.len() {
            return Err(rule("SkillNotFound"));
        }
        if tier == 3 && !groups.insert(pos / 2) {
            return Err(rule("SkillTypeAlreadyLearned"));
        }
        spent += c[format!("ReqSkillPoint{tier}")][pos as usize]
            .as_i64()
            .ok_or_else(|| rule("SkillNotFound"))?;
    }
    let total: i64 = c["GetTranscendSkillPoint"]
        .as_array()
        .into_iter()
        .flatten()
        .take(t as usize)
        .map(|v| v.as_i64().unwrap_or(0))
        .sum();
    let limit_points: i64 = table
        .limit_breaks
        .iter()
        .filter(|v| {
            n(v, "HeroIndex") == index as i64 && n(v, "LimitBreakLevel") <= n(h, "LimitBreakLevel")
        })
        .map(|v| n(v, "GetTranscendSkillPoint"))
        .sum();
    if spent > total + limit_points + n(h, "TranscendPoint") {
        return Err(rule("NotEnoughSkillPoint"));
    }
    extra[&key] = json!(json!(choices).to_string());
    Ok(())
}
async fn limit_break(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
    index: i32,
    h: &Value,
    extra: &mut Value,
    out: &mut Value,
) -> Result<()> {
    if n(h, "Transcended") != 5 {
        return Err(rule("InvalidValue"));
    }
    let table = &state.tables.hero_shop;
    let current = n(h, "LimitBreakLevel");
    if action == "hero_limit_break_level_up" {
        if n(h, "Level") < 100 + current {
            return Err(rule("InvalidValue"));
        }
        let row = table
            .limit_breaks
            .iter()
            .find(|v| n(v, "HeroIndex") == index as i64 && n(v, "LimitBreakLevel") == current + 1)
            .ok_or_else(|| rule("InvalidValue"))?;
        let mut results = vec![];
        for i in 1..=3 {
            let count = n(row, &format!("MaterialItemCount{i}"));
            if count > 0 {
                results.push(
                    item::consume(
                        db,
                        account,
                        n(row, &format!("MaterialItemIndex{i}")) as i32,
                        count as i32,
                    )
                    .await?,
                );
            }
        }
        extra["LimitBreakLevel"] = json!(current + 1);
        out["NewHeroLimitBreakLevel"] = json!(current + 1);
        out["ItemResults"] = json!(results);
    } else {
        if current <= 0 || n(h, "Level") >= 100 + current {
            return Err(rule("InvalidValue"));
        }
        let ids = array(req, "ExpItemIndices")?;
        let counts = array(req, "ExpItemCounts")?;
        if ids.len() != counts.len() {
            return Err(rule("InvalidValue"));
        }
        let mut exp = n(h, "LimitBreakExp");
        let mut results = vec![];
        for (id, count) in ids.into_iter().zip(counts) {
            if !(1..=1000).contains(&count) {
                return Err(rule("InvalidValue"));
            }
            let item = table
                .limit_exp_items
                .iter()
                .find(|v| n(v, "ItemIndex") == id)
                .ok_or_else(|| rule("InvalidValue"))?;
            exp = exp
                .checked_add(n(item, "ExpAmount") * count)
                .ok_or_else(|| rule("InvalidValue"))?;
            results.push(item::consume(db, account, id as i32, count as i32).await?);
        }
        let mut level = n(h, "Level");
        while level < 100 + current {
            let r = table
                .limit_breaks
                .iter()
                .find(|v| {
                    n(v, "HeroIndex") == index as i64 && n(v, "LimitBreakLevel") == level - 99
                })
                .ok_or_else(|| rule("InvalidValue"))?;
            let needed = n(r, "ReqLocalExp");
            if needed <= 0 || exp < needed {
                break;
            }
            exp -= needed;
            level += 1;
        }
        if level >= 100 + current {
            exp = 0;
        }
        sqlx::query("UPDATE heroes SET level=? WHERE account_id=? AND hero_index=?")
            .bind(level)
            .bind(account)
            .bind(index)
            .execute(&mut *db)
            .await?;
        extra["LimitBreakExp"] = json!(exp);
        out["NewHeroLevel"] = json!(level);
        out["NewHeroLimitBreakExp"] = json!(exp);
        out["ItemResults"] = json!(results);
    }
    Ok(())
}

/// Trial battles have no normal campaign rewards. The challenge grants one purification essence.
pub(crate) async fn trial(
    state: &AppState,
    account: i64,
    chapter: i32,
    dungeon: i32,
    end: Option<bool>,
) -> Result<Option<Value>> {
    let Some(awake) = state
        .tables
        .hero_shop
        .awake
        .iter()
        .find(|v| n(v, "AwakeChapter") == chapter as i64)
    else {
        return Ok(None);
    };
    let index = n(awake, "HeroIndex") as i32;
    let mut tx = state.db.begin().await?;
    item::init(&mut tx, state, account).await?;
    let h = info(&mut tx, account, index).await?;
    let mut extra = details(&mut tx, account, index).await?;
    let suffix = if n(&h, "Star") == 5 {
        format!("TranscendDungeon{}", n(&h, "Transcended"))
    } else {
        format!("AwakeDungeon{}", n(&h, "Star"))
    };
    if n(awake, &suffix) != dungeon as i64 || n(&h, "EnterDungeon") != 1 {
        return Err(rule("InvalidDungeon"));
    }
    let mut result = item::success();
    if let Some(won) = end {
        if extra["TrialStarted"] != true {
            return Err(rule("InvalidDungeon"));
        }
        extra["TrialStarted"] = json!(false);
        if won {
            let suffix = if n(&h, "Star") == 5 {
                format!("TranscendMaterialItemIndex{}", n(&h, "Transcended"))
            } else {
                format!("AwakeMaterialItemIndex{}", n(&h, "Star"))
            };
            let mut rewards = Rewards::default();
            item::give(
                &mut tx,
                state,
                account,
                n(awake, &suffix) as i32,
                1,
                0,
                0,
                &mut rewards,
            )
            .await?;
            extra["EnterDungeon"] = json!(2);
            result["ItemResults"] = json!(rewards.items);
        }
    } else {
        extra["TrialStarted"] = json!(true);
    }
    save_details(&mut tx, account, index, &extra).await?;
    result["HeroInfos"] = json!([info(&mut tx, account, index).await?]);
    tx.commit().await?;
    Ok(Some(result))
}

pub(crate) async fn avatar_info(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
) -> Result<Value> {
    let avatar: i64 =
        sqlx::query_scalar("SELECT avatar_hero_index FROM user_info WHERE account_id=?")
            .bind(account)
            .fetch_one(&mut *db)
            .await?;
    // Modern avatar IDs encode hero/star, or 10000 + costume index.
    let index = if avatar >= 10000 {
        state
            .tables
            .hero_shop
            .costumes
            .get(&((avatar - 10000) as i32))
            .map(|c| n(c, "HeroIndex") as i32)
            .unwrap_or(1)
    } else if avatar >= 10 {
        (avatar / 10) as i32
    } else {
        avatar.max(1) as i32
    };
    let h = info(db, account, index).await?;
    let mut out = appearance(&h);
    out["AccountId"] = json!(account);
    if avatar >= 10000 {
        out["CostumeIndex"] = json!(avatar - 10000);
    }
    Ok(out)
}
pub(crate) async fn bookmarks(db: &mut SqliteConnection, account: i64) -> Result<Value> {
    let ids:Vec<i32>=sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=? AND is_bookmarked>0 ORDER BY is_bookmarked DESC,hero_id").bind(account).fetch_all(db).await?;
    Ok(
        json!({"AccountId":account,"BookMarkHeroIndices":ids.iter().map(|i|i.to_string()).collect::<Vec<_>>().join(",")}),
    )
}

pub(crate) async fn recruit_at(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    index: i32,
    s: i64,
    level: i64,
    t: i64,
) -> Result<Value> {
    let c = state
        .tables
        .hero_shop
        .heroes
        .get(&index)
        .ok_or_else(|| rule("HeroDataNotFound"))?;
    let s = if s > 0 { s } else { n(c, "StartHeroStar") };
    let level = level.max(1);
    star(state, s, t)?;
    let result=sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,star,level,transcend) SELECT ?,COALESCE(MAX(hero_id),0)+1,?,?,?,? FROM heroes WHERE account_id=? HAVING NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)").bind(account).bind(index).bind(s).bind(level).bind(t).bind(account).bind(account).bind(index).execute(&mut *db).await?;
    if result.rows_affected() != 1 {
        return Err(rule("HeroAlreadyExist"));
    }
    let mut r = Rewards::default();
    tutorial::team_exp(
        db,
        state,
        account,
        n(star(state, s, t)?, "GetHeroTeamExp"),
        &mut r,
    )
    .await?;
    Ok(json!({"HeroInfo":info(db,account,index).await?,"TeamExpResult":r.team_exp.first()}))
}
async fn collection_item(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let id = item::item_index(req, "ItemIndex")?;
    let table = &state.tables.hero_shop;
    let mut out = item::success();
    let mut rewards = Rewards::default();
    let mut heroes = vec![];
    let consumed = item::consume(db, account, id, 1).await?;
    if action == "use_costume_select_item" {
        let selector = table
            .costume_selectors
            .iter()
            .find(|v| n(v, "ItemIndex") == id as i64)
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        if n(selector, "CostumeSelectItemType") != 0 {
            return Err(rule("ItemTypeMismatch"));
        }
        let choices: Vec<i32> = table
            .costumes
            .iter()
            .filter(|(_, c)| {
                selector["CostumeIndices"]
                    .as_array()
                    .is_some_and(|v| v.contains(&c["CostumeIndex"]))
                    || selector["CostumeCategories"].as_array().is_some_and(|v| {
                        table
                            .costume_groups
                            .get(c["Group"].as_str().unwrap_or(""))
                            .is_some_and(|g| v.contains(&json!(g)))
                    })
            })
            .map(|(id, _)| *id)
            .collect();
        if choices.is_empty() {
            return Err(rule("CostumeDataNotFound"));
        }
        let owned = costumes(db, account).await?;
        if req.text("GetOtherReward").eq_ignore_ascii_case("true") {
            if choices
                .iter()
                .any(|i| !owned.iter().any(|v| n(v, "CostumeIndex") == *i as i64))
            {
                return Err(rule("AllCostumeNotOwned"));
            }
            item::reward(
                db,
                state,
                account,
                n(selector, "AllOwnedRewardIndex") as i32,
                &mut rewards,
            )
            .await?;
        } else {
            let selected = item::item_index(req, "CostumeIndex")?;
            if !choices.contains(&selected) {
                return Err(rule("CostumeDataNotFound"));
            }
            if owned
                .iter()
                .any(|v| n(v, "CostumeIndex") == selected as i64)
            {
                return Err(rule("AlreadyHaveCostume"));
            }
            sqlx::query("INSERT INTO costumes(account_id,costume_index) VALUES (?,?)")
                .bind(account)
                .bind(selected)
                .execute(&mut *db)
                .await?;
            out["CostumeResults"] = json!(costumes(db, account)
                .await?
                .into_iter()
                .filter(|v| n(v, "CostumeIndex") == selected as i64)
                .collect::<Vec<_>>());
        }
        out["ItemResult"] = consumed;
    } else if action == "use_hero_growth_item" {
        let row = table
            .growth_items
            .iter()
            .find(|v| n(v, "ItemIndex") == id as i64)
            .ok_or_else(|| rule("HeroGrowthDataNotFound"))?;
        let selected = item::item_index(req, "HeroIndex")?;
        let choices = row["HeroIndices"]
            .as_array()
            .ok_or_else(|| rule("HeroGrowthDataNotFound"))?;
        if req.text("GetOtherReward").eq_ignore_ascii_case("true") {
            for i in choices {
                let h = info(db, account, i.as_i64().unwrap_or(0) as i32).await?;
                if n(&h, "Star") < n(row, "HeroStar")
                    || n(&h, "Level") < n(row, "HeroLevel")
                    || n(&h, "Transcended") < n(row, "HeroTranscend")
                {
                    return Err(rule("AllHeroNotOwned"));
                }
            }
            item::reward(
                db,
                state,
                account,
                n(row, "AllGrowthRewardIndex") as i32,
                &mut rewards,
            )
            .await?;
        } else {
            if !choices.contains(&json!(selected)) {
                return Err(rule("HeroNotSelectable"));
            }
            let h = info(db, account, selected).await?;
            let s = n(&h, "Star").max(n(row, "HeroStar"));
            let t = n(&h, "Transcended").max(n(row, "HeroTranscend"));
            let level = n(&h, "Level").max(n(row, "HeroLevel"));
            if s == n(&h, "Star") && t == n(&h, "Transcended") && level == n(&h, "Level") {
                return Err(rule("HeroNotSelectable"));
            }
            let mut exp = 0;
            for v in &table.stars {
                let stage = (n(v, "Star"), n(v, "Transcended"));
                if stage > (n(&h, "Star"), n(&h, "Transcended")) && stage <= (s, t) {
                    exp += n(v, "AwakeHeroTeamExp");
                }
            }
            sqlx::query("UPDATE heroes SET star=?,transcend=?,level=?,exp=CASE WHEN level<? THEN 0 ELSE exp END WHERE account_id=? AND hero_index=?").bind(s).bind(t).bind(level).bind(level).bind(account).bind(selected).execute(&mut *db).await?;
            let mut d = details(db, account, selected).await?;
            d["EnterDungeon"] = json!(0);
            d["Purified"] = json!(0);
            save_details(db, account, selected, &d).await?;
            tutorial::team_exp(db, state, account, exp, &mut rewards).await?;
            out["HeroResult"] = info(db, account, selected).await?;
            out["TeamExpResult"] = json!(rewards.team_exp.first());
        }
        rewards.items.push(consumed);
    } else {
        let row = table
            .multi_hero_items
            .iter()
            .find(|v| n(v, "ItemIndex") == id as i64)
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        let mut selected_all = BTreeSet::new();
        for (side, no) in [("Left", 1), ("Right", 2)] {
            let choices = row[format!("HeroIndices{no}")]
                .as_array()
                .ok_or_else(|| rule("ItemDataNotFound"))?;
            if req
                .text(&format!("{side}AllOwnedReward"))
                .eq_ignore_ascii_case("true")
            {
                for i in choices {
                    info(db, account, i.as_i64().unwrap_or(0) as i32).await?;
                }
                item::reward(
                    db,
                    state,
                    account,
                    n(row, &format!("AllOwnedRewardIndex{no}")) as i32,
                    &mut rewards,
                )
                .await?;
            } else {
                let ids = array(req, &format!("{side}HeroIndices"))?;
                if ids.len() != 1
                    || !choices.contains(&json!(ids[0]))
                    || !selected_all.insert(ids[0])
                {
                    return Err(rule("HeroNotSelectable"));
                }
                heroes.push(
                    recruit_at(
                        db,
                        state,
                        account,
                        ids[0] as i32,
                        n(row, "HeroStar"),
                        n(row, "HeroLevel"),
                        n(row, "HeroTranscend"),
                    )
                    .await?,
                );
            }
        }
        rewards.items.push(consumed);
        out["HeroResults"] = json!(heroes);
    }
    out["CurrencyResults"] = json!(rewards.currencies);
    out["ItemResults"] = json!(rewards.items);
    out["EquipItemResults"] = json!(rewards.equipment);
    out["ItemUseResults"] = json!([]);
    Ok(out)
}
pub(crate) async fn costume_boost(state: &AppState, account: i64) -> Result<(i32, i32)> {
    let ids: Vec<i32> = sqlx::query_scalar("SELECT costume_index FROM costumes WHERE account_id=?")
        .bind(account)
        .fetch_all(&state.db)
        .await?;
    let (mut gold, mut exp) = (0, 0);
    for id in ids {
        if let Some(c) = state.tables.hero_shop.costumes.get(&id) {
            for i in 1..=3 {
                if n(c, &format!("AbilityType{i}")) != 1 {
                    continue;
                }
                let values = &c[format!("AbilityValue{i}")];
                let amount = values[1]
                    .as_str()
                    .and_then(|s| s.parse::<i32>().ok())
                    .unwrap_or(0);
                match values[0].as_str().unwrap_or("") {
                    "BonusGold" => gold += amount,
                    "BonusExp" => exp += amount,
                    _ => {}
                }
            }
        }
    }
    Ok((gold, exp))
}
