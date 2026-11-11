//! Native promotion coupons. Redemption and mailbox delivery commit together.
use crate::{
    api::{community::mail::MailItemInfo, system::request::Request},
    error::Result,
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde::Deserialize;
use serde_json::{json, Value};
use std::collections::HashMap;

#[derive(Debug, Clone, Default, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct CouponPool {
    pub coupons: HashMap<String, Coupon>,
}
#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct Coupon {
    pub items: Vec<MailItemInfo>,
    #[serde(default)]
    pub countries: Vec<String>,
    pub expires_at: Option<i64>,
    pub mail_title: String,
    pub mail_content: String,
}
pub(crate) async fn migrate(db: &sqlx::SqlitePool) -> Result<()> {
    sqlx::query("CREATE TABLE IF NOT EXISTS coupon_redemptions(account_id INTEGER NOT NULL REFERENCES accounts(account_id),code TEXT NOT NULL,mail_id INTEGER NOT NULL,redeemed_at TEXT NOT NULL DEFAULT(datetime('now')),PRIMARY KEY(account_id,code))")
        .execute(db).await?;
    Ok(())
}
fn response(code: &str) -> Json<Value> {
    Json(json!({"BaseResult":"Success","Result":code}))
}
pub async fn use_coupon(State(s): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let r = Request::parse(&body)?;
    let account = r.account(&s)?;
    let code = r.text("CouponCode").trim().to_ascii_uppercase();
    let country = r.text("CountryCode").trim();
    if code.is_empty() {
        return Ok(response("MissingCouponCode"));
    }
    if country.is_empty() {
        return Ok(response("MissingCountryCode"));
    }
    let Some(coupon) = s.tables.coupons.coupons.get(&code) else {
        return Ok(response("CouponNotFound"));
    };
    if coupon
        .expires_at
        .is_some_and(|t| t <= chrono::Utc::now().timestamp())
    {
        return Ok(response("CouponExpired"));
    }
    if !coupon.countries.is_empty()
        && !coupon
            .countries
            .iter()
            .any(|c| c.eq_ignore_ascii_case(country))
    {
        return Ok(response("CountryCodeMismatch"));
    }
    // Validate configured attachments before writing a receipt. Never silently
    // accept a broken pool entry or deliver retired/unknown items.
    if coupon.items.is_empty()
        || coupon.items.iter().any(|i| {
            i.item_count <= 0
                || crate::models::item::disabled_reward(i.item_index)
                || !s
                    .tables
                    .items
                    .reward_item(i.item_index)
                    .is_some_and(|m| m.kind == "Item")
        })
    {
        return Ok(response("Fail"));
    }
    let mut tx = s.db.begin().await?;
    // Take the writer lock before checking the ledger, including on WAL pools.
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(account)
        .execute(&mut *tx)
        .await?;
    let received: bool = sqlx::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM coupon_redemptions WHERE account_id=? AND code=?)",
    )
    .bind(account)
    .bind(&code)
    .fetch_one(&mut *tx)
    .await?;
    if received {
        tx.rollback().await?;
        return Ok(response("AlreadyReceived"));
    }
    let mail = sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_items,is_received) VALUES(?,'System',?,?,?,0)")
        .bind(account).bind(&coupon.mail_title).bind(&coupon.mail_content)
        .bind(serde_json::to_string(&coupon.items).expect("serializable mail items"))
        .execute(&mut *tx).await?.last_insert_rowid();
    sqlx::query("INSERT INTO coupon_redemptions(account_id,code,mail_id) VALUES(?,?,?)")
        .bind(account)
        .bind(&code)
        .bind(mail)
        .execute(&mut *tx)
        .await?;
    tx.commit().await?;
    Ok(response("Success"))
}

#[cfg(test)]
mod tests;
