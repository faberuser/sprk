//! Non-cash offers, summons, events and pets. Each request holds one inventory transaction.
use crate::api::{
    extensions::{get, list, put},
    heroes as hero,
    inventory::item::{self, n, rule},
    system::request::Request,
    tutorial::Rewards,
};
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{
    body::Bytes,
    extract::{OriginalUri, State},
    Json,
};
use serde_json::{json, Value};
use sqlx::SqliteConnection;
mod events;
mod offers;
mod pets;
pub(crate) use pets::add as add_pet;
mod summons;
#[cfg(test)]
mod tests;

pub fn routes(t: &crate::tables::BattleTable) -> axum::Router<AppState> {
    let mut r = axum::Router::new();
    for p in t.contracts.keys() {
        if matches!(
            p.as_str(),
            "shop/buy_hero"
                | "shop/buy_shop_item"
                | "shop/get_shop_list"
                | "shop/request_shop_list"
                | "shop/get_all_shop_item_purchase_count"
                | "shop/sell_item"
        ) {
            continue;
        }
        r = r.route(&format!("/{p}"), axum::routing::post(handle));
    }
    r
}
async fn handle(
    State(s): State<AppState>,
    OriginalUri(uri): OriginalUri,
    body: Bytes,
) -> Result<Json<Value>> {
    execute_request(&s, uri.path().trim_start_matches('/'), body)
        .await
        .map(Json)
}
pub(crate) async fn execute_request(s: &AppState, path: &str, body: Bytes) -> Result<Value> {
    let mut r = Request::parse(&body)?;
    let a = r.account(s)?;
    let contract = s
        .tables
        .live
        .contracts
        .get(path)
        .ok_or_else(|| rule("Fail"))?;
    if let Some(fields) = contract["Request"].as_object() {
        for (k, t) in fields {
            if let Some(e) = t.as_str().and_then(|t| s.tables.live.enums.get(t)) {
                if let Some(v) = r.0.get_mut(k) {
                    if let Some(i) = e.get(v) {
                        *v = i.to_string();
                    }
                }
            }
        }
    }
    let mut tx = s.db.begin().await?;
    item::init(&mut tx, s, a).await?;
    let action = path.rsplit('/').next().unwrap_or("");
    let result = match path.split('/').next().unwrap_or("") {
        "pet" if matches!(action, "exec_pet_gacha" | "pet_gacha_roof_reward") => {
            summons::execute(&mut tx, s, a, &r, action).await
        }
        "pet" => pets::execute(&mut tx, s, a, &r, action).await,
        "equip_gacha" => summons::execute(&mut tx, s, a, &r, action).await,
        "shop" => offers::execute(&mut tx, s, a, &r, action).await,
        _ => events::execute(&mut tx, s, a, &r, action).await,
    };
    match result {
        Ok(v) => {
            tx.commit().await?;
            let mut out = item::success();
            if let Some(fields) = contract["Response"].as_object() {
                for (k, t) in fields {
                    let t = t.as_str().unwrap_or("");
                    out[k] = if t.ends_with("[]") {
                        json!([])
                    } else if t == "bool" {
                        json!(false)
                    } else if matches!(t, "int" | "long" | "byte" | "sbyte") {
                        json!(0)
                    } else {
                        Value::Null
                    };
                }
            }
            merge(&mut out, v);
            Ok(out)
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            let allowed = contract["Results"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(code)));
            Ok(json!({"BaseResult":"Success","Result":if allowed{code}else{"Fail".into()}}))
        }
        Err(e) => Err(e),
    }
}
fn merge(out: &mut Value, v: Value) {
    if let Some(m) = v.as_object() {
        for (k, v) in m {
            out[k] = v.clone();
        }
    }
}
fn now() -> i64 {
    chrono::Utc::now().timestamp()
}
fn time(t: i64) -> String {
    chrono::DateTime::from_timestamp(t, 0)
        .unwrap_or_default()
        .format("%Y-%m-%d %H:%M:%S")
        .to_string()
}
fn timestamp(v: &Value) -> i64 {
    v.as_str()
        .and_then(|s| chrono::NaiveDateTime::parse_from_str(s, "%Y-%m-%d %H:%M:%S").ok())
        .map(|v| v.and_utc().timestamp())
        .unwrap_or(0)
}
fn day() -> i64 {
    now() / 86400
}
fn flag(r: &Request, k: &str) -> Result<bool> {
    match r.text(k) {
        "" | "0" | "false" | "False" => Ok(false),
        "1" | "true" | "True" => Ok(true),
        _ => Err(rule("InvalidValue")),
    }
}
fn int(r: &Request, k: &str) -> Result<i64> {
    let v = r.number(k, 0)?;
    if !(0..=i32::MAX as i64).contains(&v) {
        return Err(rule("InvalidValue"));
    }
    Ok(v)
}
fn ids(r: &Request, k: &str, max: usize, unique: bool) -> Result<Vec<i64>> {
    let v: Vec<i64> = serde_json::from_str(if r.text(k).is_empty() {
        "[]"
    } else {
        r.text(k)
    })
    .map_err(|_| rule("InvalidValue"))?;
    if v.len() > max
        || v.iter().any(|v| *v <= 0 || *v > i32::MAX as i64)
        || unique && v.iter().collect::<std::collections::BTreeSet<_>>().len() != v.len()
    {
        return Err(rule("InvalidValue"));
    }
    Ok(v)
}
fn row<'a>(s: &'a AppState, t: &str, f: &[(&str, i64)]) -> Result<&'a Value> {
    s.tables
        .live
        .find(t, f)
        .ok_or_else(|| rule("ItemDataNotFound"))
}
fn setting(s: &AppState, k: &str, d: i64) -> i64 {
    s.tables.live.rules[k].as_i64().unwrap_or(d)
}
fn active(v: &Value) -> bool {
    v["Enabled"] != false
        && (n(v, "Begin") == 0 || n(v, "Begin") <= now())
        && (n(v, "End") == 0 || now() < n(v, "End"))
}
fn event(s: &AppState) -> Result<&Value> {
    let v = &s.tables.live.rules["Events"];
    if v.is_null() || !active(v) {
        return Err(rule("ContentsDisabled"));
    }
    Ok(v)
}
fn currency_name(s: &AppState, id: i64) -> Result<&str> {
    s.tables
        .live
        .enums
        .get("CurrencyType")
        .and_then(|e| e.iter().find(|(_, v)| **v == id).map(|(k, _)| k.as_str()))
        .ok_or_else(|| rule("InvalidCost"))
}
async fn charge(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    kind: i64,
    cost: i64,
) -> Result<Value> {
    if cost < 0 {
        return Err(rule("InvalidCost"));
    }
    if kind == 0 && cost == 0 {
        return Ok(Value::Null);
    }
    hero::currency(db, a, currency_name(s, kind)?, -cost).await
}
async fn consume_code(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    code: &str,
    count: i64,
) -> Result<Value> {
    let id = s
        .tables
        .get_item_index(code)
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    item::consume(
        db,
        a,
        id,
        i32::try_from(count).map_err(|_| rule("InvalidItemCount"))?,
    )
    .await
}
async fn group(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    code: &str,
    r: &mut Rewards,
) -> Result<()> {
    let (i, c, star, custom) = s
        .tables
        .roll_item_from_group_code(code, &[])
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    item::give(db, s, a, i, c, star, custom, r).await
}
async fn reward(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    index: i64,
    r: &mut Rewards,
) -> Result<()> {
    if index > 0 {
        item::reward(db, s, a, index as i32, r).await?;
    }
    Ok(())
}
pub(crate) async fn snapshot(s: &AppState, a: i64) -> Result<Value> {
    let mut tx = s.db.begin().await?;
    item::init(&mut tx, s, a).await?;
    let mut out = summons::snapshot(&mut tx, s, a).await?;
    merge(&mut out, pets::snapshot(&mut tx, s, a).await?);
    out["EventCraftItemInfos"] = json!(list(&mut tx, a, "event_craft")
        .await?
        .into_iter()
        .filter(|v| n(v, "Season") == n(&s.tables.live.rules["Events"], "Season"))
        .collect::<Vec<_>>());
    out["PlayerProductPurchaseInfos"] = json!(offers::purchases(&mut tx, s, a).await?);
    tx.commit().await?;
    Ok(out)
}

pub(crate) async fn validate_purchase_dungeon(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    c: i64,
    d: i64,
) -> Result<()> {
    let data = row(
        s,
        "PurchaseDungeon",
        &[("ChapterIndex", c), ("DungeonIndex", d)],
    )?;
    let id = s
        .tables
        .get_item_index(data["BoosterItemCode"].as_str().unwrap_or(""))
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    let active:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM item_boosters WHERE account_id=? AND item_index=? AND end_time>datetime('now'))").bind(a).bind(id).fetch_one(db).await?;
    if !active {
        return Err(rule("NotOpenedDungeon"));
    }
    Ok(())
}
