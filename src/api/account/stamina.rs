//! Native stamina APIs share the same balances used by battles and rewards.
use crate::{
    api::{
        battle, community,
        extensions::{get, put},
        heroes,
        inventory::item::{n, rule},
        system::request::Request,
    },
    error::{Result, ServerError},
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};
fn kind(s: &AppState, v: &str) -> Result<i64> {
    s.tables
        .services
        .enums
        .get("StaminaType")
        .and_then(|e| e.get(v))
        .copied()
        .or_else(|| v.parse().ok())
        .filter(|v| *v > 0 && *v <= 30)
        .ok_or_else(|| rule("InvalidStaminaType"))
}
fn name(s: &AppState, k: i64) -> Result<&str> {
    s.tables
        .services
        .enums
        .get("StaminaType")
        .and_then(|e| e.iter().find(|(_, v)| **v == k))
        .map(|(k, _)| k.as_str())
        .ok_or_else(|| rule("InvalidStaminaType"))
}
fn constant(s: &AppState, key: &str) -> Result<i64> {
    s.tables
        .services
        .rows("Constant")
        .iter()
        .find(|v| v["Key"] == key)
        .and_then(|v| v["Value"].as_str())
        .and_then(|v| v.parse().ok())
        .filter(|v| *v > 0)
        .ok_or_else(|| rule("Fail"))
}
async fn counter(db: &mut SqliteConnection, a: i64, k: i64) -> Result<Value> {
    let v = get(db, a, "stamina_recharges", k).await?;
    let day = chrono::Utc::now().date_naive().to_string();
    Ok(if v["Day"] == day {
        v
    } else {
        json!({"Day":day,"Count":0})
    })
}
pub(crate) async fn chicken(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let row = sqlx::query(
        "SELECT stamina,stamina_recharge_time,team_level FROM user_info WHERE account_id=?",
    )
    .bind(a)
    .fetch_one(&mut *db)
    .await?;
    let team = s
        .tables
        .services
        .find("TeamLevel", &[("Level", row.get("team_level"))])
        .ok_or_else(|| rule("Fail"))?;
    let ids: Vec<i32> = sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=?")
        .bind(a)
        .fetch_all(&mut *db)
        .await?;
    let cap = n(team, "MaxStamina")
        + ids
            .iter()
            .filter_map(|id| s.tables.hero_shop.heroes.get(id))
            .map(|v| n(v, "AddStamina"))
            .sum::<i64>();
    let interval = n(team, "RechargeStaminaSec").max(1);
    let now = s.server_time();
    let old = row.get::<i64, _>("stamina");
    let last = row.get::<i64, _>("stamina_recharge_time");
    let last = if last <= 0 || last > now { now } else { last };
    let added = if old < cap {
        ((now - last) / interval).min(cap - old)
    } else {
        0
    };
    let value = old + added;
    let anchor = if value >= cap {
        now
    } else {
        last + added * interval
    };
    sqlx::query("UPDATE user_info SET stamina=?,stamina_recharge_time=? WHERE account_id=?")
        .bind(value)
        .bind(anchor)
        .bind(a)
        .execute(db)
        .await?;
    let next = if value < cap {
        interval - (now - anchor)
    } else {
        0
    };
    Ok(
        json!({"Type":"Chicken","AddValue":added,"NewValue":value,"StaminaRechargeTime":chrono::DateTime::from_timestamp(anchor,0).unwrap().format("%Y-%m-%d %H:%M:%S").to_string(),"NextRechargeRemainTime":next,"FullRechargeRemainTime":if value<cap{next+(cap-value-1)*interval}else{0},"RechargeCount":0,"IsHide":false}),
    )
}
pub(crate) async fn snapshot(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    k: i64,
) -> Result<Value> {
    let mut v = match k {
        1 => chicken(db, s, a).await?,
        2 | 20 => community::tickets(db, s, a, name(s, k)?, 0).await?,
        10 => community::guild_ticket(db, s, a, 0).await?,
        _ => battle::charge_key(db, s, a, k, 0).await?,
    };
    v["RechargeCount"] = counter(db, a, k).await?["Count"].clone();
    Ok(v)
}
pub(crate) async fn login(s: &AppState, a: i64) -> Result<Value> {
    let mut tx = s.db.begin().await?;
    crate::api::inventory::item::init(&mut tx, s, a).await?;
    let mut out = vec![];
    for k in (1..=30).filter(|k| !matches!(k, 3 | 4 | 7)) {
        out.push(snapshot(&mut tx, s, a, k).await?);
    }
    tx.commit().await?;
    Ok(json!(out))
}
pub(crate) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
) -> Result<Value> {
    if path.ends_with("get_stamina_infos") {
        let raw = r.text("StaminaTypes");
        let values: Vec<Value> = serde_json::from_str(raw).unwrap_or_else(|_| vec![json!(raw)]);
        if values.is_empty() || values.len() > 30 {
            return Err(rule("InvalidStaminaType"));
        }
        let mut out = vec![];
        for v in values {
            let k = kind(
                s,
                &v.as_str()
                    .map(str::to_owned)
                    .unwrap_or_else(|| v.to_string()),
            )?;
            out.push(snapshot(db, s, a, k).await?);
        }
        return Ok(json!({"StaminaResults":out}));
    }
    let k = kind(s, r.text("StaminaType"))?;
    let mut out = snapshot(db, s, a, k).await?;
    if path.ends_with("get_stamina") {
        return Ok(json!({"StaminaResult":out}));
    }
    let mut count = counter(db, a, k).await?;
    let (amount, gold, gem) = if path.ends_with("buy_stamina") {
        if k != 1 {
            return Err(rule("InvalidStaminaType"));
        }
        (constant(s, "BuyStamina")?, 0, constant(s, "BuyStaminaGem")?)
    } else {
        let def = s
            .tables
            .services
            .find("Stamina", &[("StaminaType", k)])
            .ok_or_else(|| rule("InvalidStaminaType"))?;
        if n(def, "RechargeCostType") != 1 {
            return Err(rule("InvalidStaminaType"));
        }
        if def["IsResetLimit"] == true && n(&count, "Count") >= n(def, "ResetLimitCount") {
            return Err(rule("ResetLimitExceeded"));
        }
        let cost = |key: &str| -> Result<i64> {
            let Some(v) = def[key].as_array().filter(|v| !v.is_empty()) else {
                return Ok(0);
            };
            v[(n(&count, "Count") as usize).min(v.len() - 1)]
                .as_str()
                .and_then(|v| v.parse::<i64>().ok())
                .filter(|v| *v >= 0)
                .ok_or_else(|| rule("Fail"))
        };
        let amount = def["ResetCount"]
            .as_array()
            .and_then(|v| v.first())
            .and_then(Value::as_i64)
            .filter(|v| *v > 0)
            .ok_or_else(|| rule("InvalidStaminaType"))?;
        let max = def["MaxCountValue"]
            .as_str()
            .and_then(|v| v.parse::<i64>().ok())
            .unwrap_or(0);
        if (def["AllowOverflow"] == false || matches!(n(def, "MaxCountType"), 4 | 6))
            && n(&out, "NewValue") + amount > max
        {
            return Err(rule("MaxStamina"));
        }
        (
            amount,
            cost("RechargeGoldValue")?,
            cost("RechargeGemValue")?,
        )
    };
    if gold == 0 && gem == 0 {
        return Err(rule("InvalidStaminaType"));
    }
    if gold > 0 {
        heroes::currency(db, a, "Gold", -gold).await?;
    }
    let currency = heroes::currency(
        db,
        a,
        if gem > 0 { "Gem" } else { "Gold" },
        if gem > 0 { -gem } else { 0 },
    )
    .await?;
    let new = n(&out, "NewValue")
        .checked_add(amount)
        .filter(|v| *v <= i32::MAX as i64)
        .ok_or_else(|| rule("MaxStamina"))?;
    if k == 1 {
        sqlx::query("UPDATE user_info SET stamina=? WHERE account_id=?")
            .bind(new)
            .bind(a)
            .execute(&mut *db)
            .await?;
    } else if k == 2 {
        community::tickets(db, s, a, "Sword", amount).await?;
    } else {
        let mut v = battle::get(db, a, "key", k).await?;
        v["Count"] = json!(new);
        battle::put(db, a, "key", k, &v).await?;
    }
    count["Count"] = json!(n(&count, "Count") + 1);
    put(db, a, "stamina_recharges", k, &count).await?;
    out["NewValue"] = json!(new);
    out["AddValue"] = json!(amount);
    out["RechargeCount"] = count["Count"].clone();
    if k == 1 {
        let info = chicken(db, s, a).await?;
        out["NextRechargeRemainTime"] = info["NextRechargeRemainTime"].clone();
        out["FullRechargeRemainTime"] = info["FullRechargeRemainTime"].clone();
    }
    Ok(json!({"CurrencyResult":currency,"StaminaResult":out}))
}
async fn legacy(s: AppState, body: Bytes, path: &str) -> Result<Json<Value>> {
    let mut r = Request::parse(&body)?;
    r.0.insert("StaminaType".into(), "Chicken".into());
    let body = serde_urlencoded::to_string(&r.0).map_err(|_| rule("Fail"))?;
    crate::api::services::execute_request(
        &s,
        path,
        &axum::http::HeaderMap::new(),
        Bytes::from(body),
    )
    .await
    .map(Json)
}
pub async fn get_stamina_info(State(s): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    legacy(s, body, "user/get_stamina").await
}
pub async fn buy_stamina(State(s): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    legacy(s, body, "user/buy_stamina").await
}
pub async fn use_stamina(State(s): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let r = Request::parse(&body)?;
    let a = r.account(&s)?;
    let amount = r.number("Amount", 0)?;
    if !(1..=i32::MAX as i64).contains(&amount) {
        return Err(rule("InvalidCost"));
    }
    let mut tx = s.db.begin().await?;
    crate::api::inventory::item::init(&mut tx, &s, a).await?;
    chicken(&mut tx, &s, a).await?;
    let out = battle::charge_key(&mut tx, &s, a, 1, amount).await?;
    tx.commit().await?;
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","StaminaResult":out}),
    ))
}
pub async fn restore_stamina(State(s): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    Request::parse(&body)?.account(&s)?;
    Err(ServerError::InvalidRequest(
        "Stamina restoration requires a server reward".into(),
    ))
}
