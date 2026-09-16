//! Table-priced shops with persistent stock and atomic purchase limits.
use crate::api::{
    heroes as hero,
    inventory::item::{self, n, rule},
    system::request::Request,
    tutorial::Rewards,
};
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use chrono::{Datelike, TimeZone, Utc};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};

macro_rules! endpoints {($($name:ident),*)=>{$(pub async fn $name(State(state):State<AppState>,body:Bytes)->Result<Json<Value>> {handle(state,body,stringify!($name)).await})*};}
endpoints!(
    get_shop_list,
    request_shop_list,
    get_all_shop_item_purchase_count,
    buy_shop_item
);
fn period(row: &Value, now: i64, revision: i64, rotating: bool) -> String {
    if rotating {
        return format!("stock:{revision}");
    }
    let d = Utc.timestamp_opt(now, 0).single().unwrap_or_else(Utc::now);
    if row["DailyReset"] == true {
        return d.format("day:%Y-%m-%d").to_string();
    }
    if row["WeeklyReset"] == true {
        let week = d.iso_week();
        return format!("week:{}:{}", week.year(), week.week());
    }
    if let Some(days) = row["MonthlyResetDate"].as_array().filter(|v| !v.is_empty()) {
        let today = d.date_naive();
        // The most recent configured reset date identifies the purchase period.
        for offset in 0..=62 {
            let date = today - chrono::Duration::days(offset);
            if days.iter().any(|v| v.as_u64() == Some(date.day() as u64)) {
                return date.format("month:%Y-%m-%d").to_string();
            }
        }
    }
    "all".into()
}
fn kind(cost: i64) -> Result<&'static str> {
    match cost {
        1 => Ok("Gold"),
        2 => Ok("Gem"),
        3 => Ok("PvpCoin"),
        4 => Ok("GuildPoint"),
        5 => Ok("RoyalPoint"),
        6 => Ok("Mileage"),
        7 => Ok("FriendshipPoint"),
        8 => Ok("RaidPoint"),
        17 => Ok("GuildArenaPoint"),
        _ => Err(rule("InvalidCost")),
    }
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
async fn purchased(
    db: &mut SqliteConnection,
    account: i64,
    shop: i32,
    index: i64,
    period: &str,
) -> Result<i64> {
    Ok(sqlx::query_scalar("SELECT purchased FROM shop_purchase_ledger WHERE account_id=? AND shop_index=? AND item_index=? AND period=?").bind(account).bind(shop).bind(index).bind(period).fetch_optional(db).await?.unwrap_or(0))
}
fn purchasable(state: &AppState, shop: &Value, row: &Value) -> bool {
    let cost = if n(row, "BuyCostTypeForItem") > 0 {
        n(row, "BuyCostTypeForItem")
    } else {
        n(shop, "BuyCostType")
    };
    kind(cost).is_ok()
        && n(row, "ItemIndex") > 0
        && n(row, "Condition") == 0
        && state
            .tables
            .items
            .reward_item(n(row, "ItemIndex") as i32)
            .is_some_and(|r| matches!(r.kind.as_str(), "Item" | "Equip"))
}
async fn stock(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    shop: &Value,
    refresh: bool,
) -> Result<(Vec<Value>, Value)> {
    let shop_id = n(shop, "Index") as i32;
    let now = state.server_time();
    let old = sqlx::query("SELECT * FROM shop_stock WHERE account_id=? AND shop_index=?")
        .bind(account)
        .bind(shop_id)
        .fetch_optional(&mut *db)
        .await?;
    let rotating = n(shop, "MaxStock") > 0;
    let expired = old.as_ref().is_some_and(|r| {
        r.get::<i64, _>("restock_time") > 0 && r.get::<i64, _>("restock_time") <= now
    });
    let needs = old.is_none() || refresh || expired;
    if needs {
        let rev = old
            .as_ref()
            .map(|r| r.get::<i64, _>("revision") + 1)
            .unwrap_or(1);
        let mut rows: Vec<Value> = state
            .tables
            .hero_shop
            .shop_items
            .iter()
            .filter(|v| n(v, "ShopIndex") == shop_id as i64 && purchasable(state, shop, v))
            .cloned()
            .collect();
        if rotating {
            if rows.is_empty() {
                rows = state
                    .tables
                    .hero_shop
                    .shop_items
                    .iter()
                    .filter(|v| n(v, "ShopIndex") == 0 && purchasable(state, shop, v))
                    .cloned()
                    .collect();
            }
            use rand::seq::SliceRandom;
            rows.shuffle(&mut rand::thread_rng());
            rows.truncate(n(shop, "MaxStock") as usize);
            for (i, r) in rows.iter_mut().enumerate() {
                r["Index"] = json!(i + 1);
                r["PurchasableCount"] = json!(1);
            }
        }
        rows.sort_by_key(|r| n(r, "Index"));
        let next = if n(shop, "RestockSecond") > 0 {
            now + n(shop, "RestockSecond")
        } else {
            0
        };
        let count = if refresh {
            old.as_ref()
                .map(|r| r.get::<i64, _>("restock_count") + 1)
                .unwrap_or(1)
        } else {
            0
        };
        sqlx::query("INSERT INTO shop_stock(account_id,shop_index,revision,restock_time,restock_count,stock) VALUES (?,?,?,?,?,?) ON CONFLICT(account_id,shop_index) DO UPDATE SET revision=excluded.revision,restock_time=excluded.restock_time,restock_count=excluded.restock_count,stock=excluded.stock").bind(account).bind(shop_id).bind(rev).bind(next).bind(count).bind(json!(rows).to_string()).execute(&mut *db).await?;
    }
    let row = sqlx::query("SELECT * FROM shop_stock WHERE account_id=? AND shop_index=?")
        .bind(account)
        .bind(shop_id)
        .fetch_one(&mut *db)
        .await?;
    let rows: Vec<Value> = serde_json::from_str(row.get::<String, _>("stock").as_str())
        .map_err(|_| rule("NoShopItemInfo"))?;
    let time = row.get::<i64, _>("restock_time");
    let info = json!({"ShopIndex":shop_id,"ShopItemListIndex":row.get::<i64,_>("revision"),"RestockTime":if time>0 {Utc.timestamp_opt(time,0).single().map(|d|d.format("%Y-%m-%d %H:%M:%S").to_string())}else{None},"RestockCount":row.get::<i64,_>("restock_count"),"RestockCountResetTime":null});
    Ok((rows, info))
}
async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let id = item::item_index(req, "ShopIndex")?;
    let table = &state.tables.hero_shop;
    let shop = table.shops.get(&id).ok_or_else(|| rule("NoShopData"))?;
    if n(shop, "EventOnly") != 0 {
        return Err(rule("ContentsDisabled"));
    }
    let guild_level = if n(shop, "BuyCostType") == 4 {
        Some(crate::api::community::validate_shop(db, state, account, id as i64).await?)
    } else { None };
    let mut out = item::success();
    if action == "buy_shop_item" && n(shop, "MaxStock") > 0 {
        let expiry: Option<i64> = sqlx::query_scalar(
            "SELECT restock_time FROM shop_stock WHERE account_id=? AND shop_index=?",
        )
        .bind(account)
        .bind(id)
        .fetch_optional(&mut *db)
        .await?;
        if expiry.is_none_or(|t| t > 0 && t <= state.server_time()) {
            return Err(rule("InvalidShopItemListIndex"));
        }
    }
    let refresh = action == "request_shop_list";
    if refresh {
        if n(shop, "MaxStock") == 0 || n(shop, "RestockSecond") <= 0 || n(shop, "RestockGem") <= 0 {
            return Err(rule("CannotRestock"));
        }
        out["CurrencyResult"] = hero::currency(db, account, "Gem", -n(shop, "RestockGem")).await?;
    }
    let (mut rows, restock) = stock(db, state, account, shop, refresh).await?;
    if let Some(level) = guild_level {
        if shop["UseGroupIndexForLevel"] == true { rows.retain(|r|n(r,"GroupIndex")<=level); }
    }
    let rotating = n(shop, "MaxStock") > 0;
    let revision = n(&restock, "ShopItemListIndex");
    let now = state.server_time();
    if action == "buy_shop_item" {
        let index = req.number(if rotating { "ListNo" } else { "ShopItemIndex" }, 0)?;
        let row = rows
            .iter()
            .find(|v| n(v, "Index") == index)
            .ok_or_else(|| rule("ShopItemDataNotFound"))?;
        let count = item::positive(req, "ShopItemPurchaseCount")? as i64;
        if rotating && count != 1 {
            return Err(rule("InvalidValue"));
        }
        let period = period(row, now, revision, rotating);
        let old = purchased(db, account, id, index, &period).await?;
        let max = n(row, "PurchasableCount");
        if max > 0 && old + count > max {
            return Err(rule("SoldOut"));
        }
        let item_id = n(row, "ItemIndex") as i32;
        let meta = table
            .items
            .get(&item_id)
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        let cost_type = if n(row, "BuyCostTypeForItem") > 0 {
            n(row, "BuyCostTypeForItem")
        } else {
            n(shop, "BuyCostType")
        };
        let kind = kind(cost_type)?;
        if kind == "GuildPoint" {
            crate::api::community::validate_shop(db,state,account,id as i64).await?;
        }
        let mut price = if n(row, "TargetPrice") > 0 {
            n(row, "TargetPrice")
        } else {
            n(meta, &format!("Buy{kind}")) * n(row, "Count")
        };
        if n(row, "TargetPrice") == 0
            && state
                .tables
                .items
                .reward_item(item_id)
                .is_some_and(|m| m.kind == "Equip")
        {
            if let Some(star) = table
                .equip_star_prices
                .iter()
                .find(|v| n(v, "Star") == n(row, "Star"))
            {
                price = price * n(star, "BuyPriceFactor") / 1000;
            }
        }
        if price <= 0 {
            return Err(rule("InvalidCost"));
        }
        if n(shop, "HeroIndex") > 0 {
            let owned = sqlx::query(
                "SELECT star,transcend FROM heroes WHERE account_id=? AND hero_index=?",
            )
            .bind(account)
            .bind(n(shop, "HeroIndex"))
            .fetch_optional(&mut *db)
            .await?;
            if let Some(owned) = owned {
                if let Some(bonus) = table.hero_bonuses.iter().find(|v| {
                    n(v, "HeroIndex") == n(shop, "HeroIndex")
                        && n(v, "HeroStar") == owned.get::<i64, _>("star")
                        && n(v, "HeroTranscended") == owned.get::<i64, _>("transcend")
                }) {
                    let mut ratio = 1.0_f64;
                    for i in 1..=5 {
                        if n(bonus, &format!("BonusType{i}")) != 19 {
                            continue;
                        }
                        if let Some(values) = bonus[format!("BonusValue{i}")]
                            .as_array()
                            .filter(|v| v.len() == 2)
                        {
                            let number = |v: &Value| {
                                v.as_str().and_then(|s| s.parse::<i64>().ok()).unwrap_or(0)
                            };
                            if number(&values[0]) == id as i64 {
                                ratio *= 1.0 - number(&values[1]).clamp(0, 100) as f64 / 100.0;
                            }
                        }
                    }
                    price = ((price as f64 * ratio * 100.0).round() / 100.0).ceil() as i64;
                }
            }
        }
        let cost = price
            .checked_mul(count)
            .ok_or_else(|| rule("InvalidCost"))?;
        let currency = hero::currency(db, account, kind, -cost).await?;
        if kind == "FriendshipPoint" {
            out["FriendshipPointResult"] = currency;
        } else if kind == "GuildPoint" {
            out["GuildPointResult"] = currency;
        } else if kind == "RoyalPoint" {
            out["RoyalPointResult"] = currency;
        } else {
            out["CurrencyResults"] = json!([currency]);
        }
        let amount = n(row, "Count")
            .checked_mul(count)
            .and_then(|v| i32::try_from(v).ok())
            .ok_or_else(|| rule("InvalidValue"))?;
        let mut rewards = Rewards::default();
        item::give(
            db,
            state,
            account,
            item_id,
            amount,
            n(row, "Star") as i32,
            0,
            &mut rewards,
        )
        .await?;
        if rewards.items.len() > 1 || !rewards.heroes.is_empty() {
            return Err(rule("InvalidItemType"));
        }
        out["ItemResult"] = json!(rewards.items.first());
        out["EquipItems"] = json!(rewards.equipment);
        sqlx::query("INSERT INTO shop_purchase_ledger(account_id,shop_index,item_index,period,purchased,purchased_time) VALUES (?,?,?,?,?,?) ON CONFLICT(account_id,shop_index,item_index,period) DO UPDATE SET purchased=excluded.purchased,purchased_time=excluded.purchased_time").bind(account).bind(id).bind(index).bind(period).bind(old+count).bind(now).execute(&mut *db).await?;
        out["ShopItem"] = json!({"ShopIndex":id,"ListNo":index,"Sold":if max>0&&old+count>=max {1}else{0},"Purchased":old+count,"PurchasedTime":now});
    } else {
        let mut result = vec![];
        for row in rows {
            let period = period(&row, now, revision, rotating);
            let count = purchased(db, account, id, n(&row, "Index"), &period).await?;
            let max = n(&row, "PurchasableCount");
            result.push(json!({"ShopIndex":id,"ListNo":n(&row,"Index"),"ItemCode":row["ItemCode"],"Count":row["Count"],"Star":row["Star"],"Sold":if max>0&&count>=max {1}else{0},"PurchasableCount":max,"Purchased":count,"PurchasedTime":0}));
        }
        out["ShopItems"] = json!(result);
        out["RestockTimeInfo"] = restock;
    }
    Ok(out)
}

// The original live-service paid catalog is absent from the extracted tables.
// These endpoints deliberately cannot manufacture a successful purchase.
pub async fn get_payshop_products(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<Value>> {
    Request::parse(&body)?.account(&state)?;
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","PayShopProductInfos":[],"PlayerProductPurchaseInfos":[]}),
    ))
}
pub async fn unavailable_product(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<Value>> {
    Request::parse(&body)?.account(&state)?;
    Ok(Json(json!({"BaseResult":"Success","Result":"Fail"})))
}
