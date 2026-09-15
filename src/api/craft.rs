//! Crafting costs and outcomes come from the installed client's tables.
use super::{
    item::{self, n, rule},
    social_request::Request,
    tutorial::Rewards,
};
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};

macro_rules! endpoint {($($name:ident),*)=>{$(pub async fn $name(State(state):State<AppState>,body:Bytes)->Result<Json<Value>>{handle(state,body,stringify!($name)).await})*};}
endpoint!(
    craft_item,
    add_craft_slot,
    take_craft_item,
    cancel_craft_item,
    instant_craft_item
);

pub(crate) async fn login_data(state: &AppState, account: i64) -> Result<Vec<Value>> {
    let mut tx = state.db.begin().await?;
    item::init(&mut tx, state, account).await?;
    let rows = sqlx::query("SELECT * FROM craft_slots WHERE account_id=? ORDER BY slot_index")
        .bind(account)
        .fetch_all(&mut *tx)
        .await?;
    tx.commit().await?;
    Ok(rows.iter().map(|r|json!({"SlotIndex":r.get::<i32,_>("slot_index"),"CraftIndex":r.get::<i32,_>("craft_index"),"ItemIndex":r.get::<i32,_>("item_index"),"ItemCount":r.get::<i32,_>("item_count"),"CompleteTime":chrono::DateTime::from_timestamp(r.get("complete_time"),0).map(|t|t.format("%Y-%m-%d %H:%M:%S").to_string())})).collect())
}
async fn slot_result(db: &mut SqliteConnection, account: i64, slot: i64) -> Result<Value> {
    let r = sqlx::query("SELECT * FROM craft_slots WHERE account_id=? AND slot_index=?")
        .bind(account)
        .bind(slot)
        .fetch_one(db)
        .await?;
    Ok(
        json!({"SlotIndex":slot,"CraftIndex":r.get::<i32,_>("craft_index"),"ItemIndex":r.get::<i32,_>("item_index"),"ItemCount":r.get::<i32,_>("item_count"),"RemainTime":(r.get::<i64,_>("complete_time")-chrono::Utc::now().timestamp()).max(0)}),
    )
}
async fn clear(db: &mut SqliteConnection, account: i64, slot: i64) -> Result<()> {
    sqlx::query("UPDATE craft_slots SET craft_index=0,item_index=0,item_count=0,complete_time=0,paid_gold=0,materials='[]' WHERE account_id=? AND slot_index=?").bind(account).bind(slot).execute(db).await?;
    Ok(())
}
async fn handle(state: AppState, body: Bytes, action: &str) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let mut tx = state.db.begin().await?;
    item::init(&mut tx, &state, account).await?;
    match execute(&mut tx, &state, account, &req, action).await {
        Ok(value) => {
            tx.commit().await?;
            Ok(Json(value))
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            Ok(Json(
                json!({"BaseResult":"Success","Result":item::native_result(action,&code)}),
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
    let slot = req.number("SlotIndex", 0)?;
    if !(1..=state.tables.inventory.constant("CraftSlotCountMax", 8)).contains(&slot) {
        return Err(rule("WrongSlotIndex"));
    }
    let mut out = item::success();
    let existing = sqlx::query("SELECT * FROM craft_slots WHERE account_id=? AND slot_index=?")
        .bind(account)
        .bind(slot)
        .fetch_optional(&mut *db)
        .await?;
    if action == "add_craft_slot" {
        if existing.is_some() {
            return Err(rule("NotSelectableSlotIndex"));
        }
        let previous: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM craft_slots WHERE account_id=? AND slot_index=?)",
        )
        .bind(account)
        .bind(slot - 1)
        .fetch_one(&mut *db)
        .await?;
        if !previous {
            return Err(rule("NotSelectableSlotIndex"));
        }
        let price = state
            .tables
            .inventory
            .constants
            .get(&format!("CraftSlotIndexOpen{slot}"))
            .ok_or_else(|| rule("CraftSlotPriceDataNotFound"))?;
        out["CurrencyResult"] = item::money(db, account, "Gold", -*price).await?;
        sqlx::query("INSERT INTO craft_slots(account_id,slot_index) VALUES (?,?)")
            .bind(account)
            .bind(slot)
            .execute(&mut *db)
            .await?;
    } else {
        let row = existing.ok_or_else(|| rule("NotSelectableSlotIndex"))?;
        let working = row.get::<i32, _>("craft_index") != 0;
        if action == "craft_item" {
            if working {
                return Err(rule("NotSelectableSlotIndex"));
            }
            let craft_index = item::item_index(req, "CraftIndex")?;
            let craft = state
                .tables
                .inventory
                .crafts
                .get(&craft_index)
                .ok_or_else(|| rule("CraftDataNotFound"))?;
            if craft["IsOpen"] != true {
                return Err(rule("CraftDataNotOpened"));
            }
            let index = item::item_index(req, "ItemIndex")?;
            if index as i64 != n(craft, "ItemIndex") {
                return Err(rule("ItemDataNotFound"));
            }
            let count = item::positive(req, "ItemCount")?;
            if count > 1 && craft["Countable"] != true {
                return Err(rule("InvalidItemCount"));
            }
            let output = n(craft, "ResultItemCount")
                .checked_mul(count as i64)
                .and_then(|v| i32::try_from(v).ok())
                .ok_or_else(|| rule("InvalidItemCount"))?;
            if output <= 0 {
                return Err(rule("InvalidItemCount"));
            }
            let materials = craft["Materials"]
                .as_array()
                .ok_or_else(|| rule("CraftDataNotFound"))?;
            let mut consumed = vec![];
            let mut refunds = vec![];
            for material in materials {
                let index = n(material, "ItemIndex") as i32;
                let required = n(material, "Count")
                    .checked_mul(count as i64)
                    .and_then(|v| i32::try_from(v).ok())
                    .ok_or_else(|| rule("InvalidItemCount"))?;
                consumed.push(item::consume(db, account, index, required).await.map_err(
                    |e| match e {
                        ServerError::InvalidRequest(_) => rule("NotEnoughMaterial"),
                        other => other,
                    },
                )?);
                refunds.push(json!({"ItemIndex":index,"Count":required}));
            }
            let gold = n(craft, "ReqGold")
                .checked_mul(count as i64)
                .ok_or_else(|| rule("NotEnoughGold"))?;
            out["CurrencyResult"] = item::money(db, account, "Gold", -gold).await?;
            let duration = n(craft, "ReqTime")
                .checked_mul(count as i64)
                .ok_or_else(|| rule("InvalidItemCount"))?;
            if duration == 0 {
                let mut r = Rewards::default();
                item::give(db, state, account, index, output, 0, 0, &mut r).await?;
                consumed.extend(r.items);
                out["EquipItemResults"] = json!(r.equipment);
            } else {
                sqlx::query("UPDATE craft_slots SET craft_index=?,item_index=?,item_count=?,complete_time=?,paid_gold=?,materials=? WHERE account_id=? AND slot_index=?")
                    .bind(craft_index).bind(index).bind(output).bind(state.server_time()+duration).bind(gold).bind(json!(refunds).to_string()).bind(account).bind(slot).execute(&mut *db).await?;
                out["EquipItemResults"] = json!([]);
            }
            out["ItemResults"] = json!(consumed);
        } else {
            if !working {
                return Err(rule("NotCraftSlotIndex"));
            }
            let remaining = row.get::<i64, _>("complete_time") - state.server_time();
            match action {
                "take_craft_item" => {
                    if remaining > 0 {
                        return Err(rule("NotYetCrafted"));
                    }
                    let mut r = Rewards::default();
                    item::give(
                        db,
                        state,
                        account,
                        row.get("item_index"),
                        row.get("item_count"),
                        0,
                        0,
                        &mut r,
                    )
                    .await?;
                    out["ItemResult"] = r.items.first().cloned().unwrap_or(Value::Null);
                    out["EquipItemResults"] = json!(r.equipment);
                    clear(db, account, slot).await?;
                }
                "cancel_craft_item" => {
                    if remaining <= 0 {
                        return Err(rule("AlreadyCompletedCraftItem"));
                    }
                    // Refund the recorded payment, so later table changes cannot alter it.
                    let materials: Vec<Value> =
                        serde_json::from_str(&row.get::<String, _>("materials"))
                            .map_err(|_| rule("CraftDataNotFound"))?;
                    let mut r = Rewards::default();
                    for material in materials {
                        item::give(
                            db,
                            state,
                            account,
                            n(&material, "ItemIndex") as i32,
                            n(&material, "Count") as i32,
                            0,
                            0,
                            &mut r,
                        )
                        .await?;
                    }
                    out["ItemResults"] = json!(r.items);
                    out["CurrencyResult"] =
                        item::money(db, account, "Gold", row.get("paid_gold")).await?;
                    clear(db, account, slot).await?;
                }
                "instant_craft_item" => {
                    if remaining <= 0 {
                        return Err(rule("AlreadyCompletedCraftItem"));
                    }
                    // Client RemainTime is advisory. The server clock determines the price.
                    let price = if remaining <= 600 {
                        0
                    } else {
                        let tier = state
                            .tables
                            .inventory
                            .craft_instant_prices
                            .iter()
                            .find(|r| {
                                n(r, "MinRemainTime") <= remaining
                                    && n(r, "MaxRemainTime") >= remaining
                            })
                            .ok_or_else(|| rule("WrongRemainTime"))?;
                        let seconds = n(tier, "ReqSecondPerGem");
                        let extra = remaining - n(tier, "MinRemainTime");
                        n(tier, "BaseReqGem")
                            + if seconds > 0 {
                                (extra + seconds - 1) / seconds
                            } else {
                                0
                            }
                    };
                    out["CurrencyResult"] = item::money(db, account, "Gem", -price).await?;
                    sqlx::query("UPDATE craft_slots SET complete_time=? WHERE account_id=? AND slot_index=?").bind(state.server_time()).bind(account).bind(slot).execute(&mut *db).await?;
                }
                _ => return Err(rule("Fail")),
            }
        }
    }
    out["CraftSlotResult"] = slot_result(db, account, slot).await?;
    Ok(out)
}
