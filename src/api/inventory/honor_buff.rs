//! Usable Honor ranking tokens grant the described shared seasonal gold bonus.
use super::item::{n, rule};
use crate::{error::Result, state::AppState};
use serde_json::{json, Value};
use sqlx::SqliteConnection;

pub(crate) fn definition(s: &AppState, index: i32) -> Option<i64> {
    let token = s.tables.arena_guild.rows("HonorSeasonBooster").iter()
        .find(|r| n(r, "ItemIndex") == i64::from(index))?;
    let rank = n(token, "Rank");
    let tier = s.tables.arena_guild.rows("HonorTierReward").iter()
        .find(|r| n(r, "MinRank") == rank && n(r, "MaxRank") == rank)?;
    let buff = tier["ServerBuff"].as_array()?;
    if buff.first()?.as_str()? != "BonusGold" { return None; }
    buff.get(1)?.as_str()?.parse::<i64>().ok().filter(|v| *v > 0)
}

pub(crate) async fn activate(db: &mut SqliteConnection, s: &AppState, percent: i64, count: i32) -> Result<Value> {
    let (period, _, end) = crate::api::community::arena::season(s);
    let added = percent.checked_mul(i64::from(count)).ok_or_else(|| rule("InvalidItemCount"))?;
    let total: i64 = sqlx::query_scalar("INSERT INTO community_state(owner,kind,idx,data) VALUES(0,'honor_item_buff',?,json_object('GoldPercent',?,'Expires',?)) ON CONFLICT(owner,kind,idx) DO UPDATE SET data=json_object('GoldPercent',CAST(json_extract(data,'$.GoldPercent') AS INTEGER)+?,'Expires',?) RETURNING CAST(json_extract(data,'$.GoldPercent') AS INTEGER)")
        .bind(period).bind(added).bind(end).bind(added).bind(end).fetch_one(db).await?;
    let expires = chrono::DateTime::from_timestamp(end, 0).ok_or_else(|| rule("Fail"))?.format("%Y-%m-%d %H:%M:%S UTC").to_string();
    Ok(json!({"AddedGoldPercent":added,"GoldPercent":total,"Expires":end,"Message":format!("Server gold gains increased by {added}%. Active Honor token bonus: {total}%. Applies to all players until the next Honor rewards calculation ({expires}).")}))
}

pub(crate) async fn bonus(db: &mut SqliteConnection, s: &AppState) -> Result<i64> {
    let period = crate::api::community::arena::season(s).0;
    let percent: Option<i64> = sqlx::query_scalar("SELECT CAST(json_extract(data,'$.GoldPercent') AS INTEGER) FROM community_state WHERE owner=0 AND kind='honor_item_buff' AND idx=? AND CAST(json_extract(data,'$.Expires') AS INTEGER)>?")
        .bind(period).bind(chrono::Utc::now().timestamp()).fetch_optional(db).await?;
    Ok(percent.unwrap_or(0))
}


#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{setup, login};
    use axum::{body::Bytes, extract::State};
    async fn use_token(s: &AppState, user: &Value, index: i32, count: i32) -> Value {
        super::super::item::use_booster_item(State(s.clone()), Bytes::from(format!("SessionKey={}&ItemIndex={index}&ItemCount={count}", user["UserInfo"]["SessionKey"].as_str().unwrap()))).await.unwrap().0
    }
    #[tokio::test]
    async fn honor_booster_tokens_stack_serverwide_and_expire_at_reward_reset() {
        let s = setup().await;
        let a = login(&s, "honor-buff-owner").await;
        let b = login(&s, "honor-buff-other").await;
        let account = a["UserInfo"]["AccountId"].as_i64().unwrap();
        let other = b["UserInfo"]["AccountId"].as_i64().unwrap();
        for index in [110033, 110034, 110035] {
            sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,?,2)").bind(account).bind(index).execute(&s.db).await.unwrap();
        }
        let before = crate::api::battle::bonuses::calculate(&mut s.db.acquire().await.unwrap(), &s, other, 1).await.unwrap();
        assert_eq!(definition(&s, 110034), Some(3));
        assert_eq!(definition(&s, 110014), None);
        let first = use_token(&s, &a, 110034, 1).await;
        assert_eq!(first["Result"], "Success", "{first}");
        assert_eq!(first["ItemResult"]["NewCount"], 1);
        assert_eq!(first["HonorServerBuff"]["GoldPercent"], 3);
        assert_eq!(first["HonorServerBuff"]["Expires"], crate::api::community::arena::season(&s).2);
        assert!(first["ItemTimeDurationInfo"].is_null());
        assert_eq!(crate::api::battle::bonuses::calculate(&mut s.db.acquire().await.unwrap(), &s, other, 1).await.unwrap(), (before.0 + 3, before.1));
        assert_eq!(use_token(&s, &a, 110033, 1).await["HonorServerBuff"]["GoldPercent"], 8);
        assert_eq!(use_token(&s, &a, 110035, 2).await["HonorServerBuff"]["GoldPercent"], 12);
        assert_eq!(bonus(&mut s.db.acquire().await.unwrap(), &s).await.unwrap(), 12);
        let logged = login(&s, "honor-buff-other").await;
        assert_eq!(logged["UserInfo"]["AccountId"], other);
        assert_eq!(bonus(&mut s.db.acquire().await.unwrap(), &s).await.unwrap(), 12);
        sqlx::query("UPDATE community_state SET data=json_set(data,'$.Expires',0) WHERE kind='honor_item_buff'").execute(&s.db).await.unwrap();
        assert_eq!(bonus(&mut s.db.acquire().await.unwrap(), &s).await.unwrap(), 0);
        sqlx::query("UPDATE community_state SET idx=idx-1,data=json_set(data,'$.Expires',9223372036854775807) WHERE kind='honor_item_buff'").execute(&s.db).await.unwrap();
        assert_eq!(bonus(&mut s.db.acquire().await.unwrap(), &s).await.unwrap(), 0);
    }
    #[tokio::test]
    async fn honor_booster_failure_does_not_consume_or_activate_and_limits_count() {
        let s = setup().await;
        let a = login(&s, "honor-buff-failure").await;
        let account = a["UserInfo"]["AccountId"].as_i64().unwrap();
        sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,110034,1)").bind(account).execute(&s.db).await.unwrap();
        for (index, count) in [(110034,2),(110034,0),(110034,1001),(110033,1),(110037,1)] {
            assert_ne!(use_token(&s, &a, index, count).await["Result"], "Success");
            assert_eq!(bonus(&mut s.db.acquire().await.unwrap(), &s).await.unwrap(), 0);
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT count FROM items WHERE account_id=? AND item_index=110034").bind(account).fetch_one(&s.db).await.unwrap(), 1);
        }
        assert_eq!(use_token(&s, &a, 110034, 1).await["Result"], "Success");
        assert_ne!(use_token(&s, &a, 110034, 1).await["Result"], "Success");
        assert_eq!(bonus(&mut s.db.acquire().await.unwrap(), &s).await.unwrap(), 3);
    }
}
