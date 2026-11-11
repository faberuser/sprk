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
    let mut tx = crate::database::begin(&s.db).await?;
    // Take the writer lock before checking the ledger, including on WAL pools.
    crate::db::query("SELECT pg_advisory_xact_lock($1)")
        .bind(account)
        .execute(&mut *tx)
        .await?;
    let received: bool = crate::db::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM coupon_redemptions WHERE account_id=$1 AND code=$2)",
    )
    .bind(account)
    .bind(&code)
    .fetch_one(&mut *tx)
    .await?;
    if received {
        tx.rollback().await?;
        return Ok(response("AlreadyReceived"));
    }
    let mail = crate::db::query_scalar::<i64>("INSERT INTO mails(account_id,sender,title,content,reward_items,is_received) VALUES($1,'System',$2,$3,$4,0) RETURNING mail_id")
        .bind(account).bind(&coupon.mail_title).bind(&coupon.mail_content)
        .bind(serde_json::to_string(&coupon.items).expect("serializable mail items"))
        .fetch_one(&mut *tx).await?;
    crate::db::query("INSERT INTO coupon_redemptions(account_id,code,mail_id) VALUES($1,$2,$3)")
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
