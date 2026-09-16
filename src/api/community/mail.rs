//! Persistent inboxes and atomic attachment claims using the native mail protocol.
use crate::api::{
    system::request::Request,
    tutorial::{self, Rewards},
};
use crate::{
    error::{Result, ServerError},
    models::equip::EquipItemInfo,
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde::{Deserialize, Serialize};
use serde_json::{json, Value};
use sqlx::{sqlite::SqliteRow, Row, SqliteConnection};

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct MailItemInfo {
    pub item_index: i32,
    pub item_count: i32,
}

fn response(result: &str) -> Value {
    json!({"BaseResult":"Success","Result":result})
}
fn attachments<T: serde::de::DeserializeOwned>(row: &SqliteRow, column: &str) -> Result<Vec<T>> {
    let data: Option<String> = row.get(column);
    match data.as_deref().filter(|s| !s.trim().is_empty()) {
        None | Some("null") => Ok(vec![]),
        Some(s) => serde_json::from_str(s)
            .map_err(|e| ServerError::Internal(format!("Invalid mail {column}: {e}"))),
    }
}

// Materialize a global announcement once per account. The delivery ledger survives claims.
async fn deliver_globals(state: &AppState, account: i64) -> Result<()> {
    let mut tx = state.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(account)
        .execute(&mut *tx)
        .await?;
    let rows = sqlx::query("SELECT global_id FROM global_mails g WHERE (expires_at IS NULL OR expires_at='' OR datetime(expires_at)>datetime('now')) AND NOT EXISTS(SELECT 1 FROM global_mail_deliveries d WHERE d.account_id=? AND d.global_id=g.global_id) ORDER BY global_id")
        .bind(account).fetch_all(&mut *tx).await?;
    let mut delivered = vec![];
    for row in rows {
        let global: i64 = row.get("global_id");
        let mail = sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_gold,reward_gem,reward_items,reward_stamina,reward_equipment,created_at,expires_at,opens_at,global_id) SELECT ?,sender,title,content,reward_gold,reward_gem,reward_items,reward_stamina,reward_equipment,created_at,expires_at,opens_at,global_id FROM global_mails WHERE global_id=?")
            .bind(account).bind(global).execute(&mut *tx).await?.last_insert_rowid();
        sqlx::query(
            "INSERT INTO global_mail_deliveries(account_id,global_id,mail_id) VALUES (?,?,?)",
        )
        .bind(account)
        .bind(global)
        .bind(mail)
        .execute(&mut *tx)
        .await?;
        delivered.push(mail);
    }
    tx.commit().await?;
    for mail in delivered {
        state.chat.notify(
            account,
            0,
            "NewMailArrived",
            json!({"MailIndex":mail,"IsGlobal":true}),
        );
    }
    Ok(())
}

fn mail_info(row: &SqliteRow) -> Result<Value> {
    let opens: Option<String> = row.get("opens_at");
    let remain = opens
        .as_deref()
        .and_then(|s| chrono::NaiveDateTime::parse_from_str(s, "%Y-%m-%d %H:%M:%S").ok())
        .map(|v| (v.and_utc() - chrono::Utc::now()).num_seconds().max(0))
        .unwrap_or(0);
    let equipment: Vec<EquipItemInfo> = attachments(row, "reward_equipment")?;
    let items: Vec<MailItemInfo> = attachments(row, "reward_items")?;
    let currencies: Vec<Value> = attachments(row, "reward_currencies")?;
    Ok(
        json!({"MailIndex":row.get::<i64,_>("mail_id"),"OpenIndex":0,"SenderId":0,"SenderName":row.get::<String,_>("sender"),
        "ReceiverId":row.get::<i64,_>("account_id"),"Title":row.get::<String,_>("title"),"Content":row.get::<Option<String>,_>("content").unwrap_or_default(),
        "SendGold":row.get::<i64,_>("reward_gold"),"SendGem":row.get::<i64,_>("reward_gem"),"SendChicken":row.get::<i64,_>("reward_stamina"),
        "ReadTime":row.get::<Option<String>,_>("read_time"),"ExpireTime":row.get::<Option<String>,_>("expires_at"),"OpenTime":opens,"OpenRemainTime":remain,
        "ItemInfos":items,"EquipItemInfos":equipment,"CurrencyInfos":currencies,"StaminaInfos":[]}),
    )
}

pub async fn check_new_mail(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    deliver_globals(&state, account).await?;
    let row=sqlx::query("SELECT COUNT(*) AS total, COALESCE(MAX(mail_id),0) AS latest FROM mails WHERE account_id=? AND is_received=0 AND (expires_at IS NULL OR expires_at='' OR datetime(expires_at)>datetime('now'))")
        .bind(account).fetch_one(&state.db).await?;
    let latest: i64 = row.get("latest");
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","NewMailArrived":latest>req.number("TopmostMailIndex",0)?,"TotalMailCount":row.get::<i64,_>("total"),"LastCheckedMailIndex":latest,"OpenedMailIndices":[]}),
    ))
}

pub async fn get_mail_list(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    list(state, body, false, false).await
}
pub async fn get_mail_list_by_page(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<Value>> {
    list(state, body, true, false).await
}
pub async fn get_global_mail_list(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<Value>> {
    list(state, body, false, true).await
}
async fn list(state: AppState, body: Bytes, paged: bool, global: bool) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    deliver_globals(&state, account).await?;
    let max = if global {
        1000
    } else {
        req.number("MaxCount", 50)?.clamp(1, 100)
    };
    let cursor = if paged || global {
        0
    } else {
        req.number("LastMailIndex", 0)?.max(0)
    };
    let offset = if paged {
        req.number("PageNo", 0)?.clamp(0, 100000) * max
    } else {
        0
    };
    let filter="account_id=? AND is_received=0 AND (expires_at IS NULL OR expires_at='' OR datetime(expires_at)>datetime('now')) AND ((?=1 AND global_id IS NOT NULL) OR (?=0 AND global_id IS NULL))";
    let rows=sqlx::query(&format!("SELECT * FROM mails WHERE {filter} AND (?=0 OR mail_id<?) ORDER BY mail_id DESC LIMIT ? OFFSET ?"))
        .bind(account).bind(global).bind(global).bind(cursor).bind(cursor).bind(max).bind(offset).fetch_all(&state.db).await?;
    let total: i64 = sqlx::query_scalar(&format!("SELECT COUNT(*) FROM mails WHERE {filter}"))
        .bind(account)
        .bind(global)
        .bind(global)
        .fetch_one(&state.db)
        .await?;
    let infos = rows.iter().map(mail_info).collect::<Result<Vec<_>>>()?;
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","MailInfos":infos,"TotalCount":total,"OpenedMailIndices":[]}),
    ))
}

async fn claim(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    row: &SqliteRow,
) -> Result<Value> {
    let gold: i64 = row.get("reward_gold");
    let gem: i64 = row.get("reward_gem");
    let stamina: i64 = row.get("reward_stamina");
    if gold < 0 || gem < 0 || stamina < 0 {
        return Err(ServerError::Internal("Negative mail attachment".into()));
    }
    let mut rewards = Rewards::default();
    for currency in attachments::<Value>(row, "reward_currencies")? {
        let kind = currency["CurrencyType"].as_str().ok_or_else(|| ServerError::Internal("Invalid mail currency".into()))?;
        let amount = currency["Amount"].as_i64().filter(|n| *n > 0).ok_or_else(|| ServerError::Internal("Invalid mail currency amount".into()))?;
        rewards.currencies.push(crate::api::heroes::currency(db, account, kind, amount).await?);
    }
    tutorial::currency(db, account, "Gold", gold, &mut rewards).await?;
    tutorial::currency(db, account, "Gem", gem, &mut rewards).await?;
    for item in attachments::<MailItemInfo>(row, "reward_items")? {
        if item.item_count <= 0 {
            return Err(ServerError::Internal("Invalid mail item count".into()));
        }
        let metadata = state
            .tables
            .items
            .reward_item(item.item_index)
            .ok_or_else(|| ServerError::Internal("Unknown mail item".into()))?;
        if (metadata.kind == "Equip" && item.item_count > 1000)
            || (metadata.kind == "Hero" && item.item_count != 1)
        {
            return Err(ServerError::Internal(
                "Invalid mail attachment quantity".into(),
            ));
        }
        let owned: bool = if metadata.kind == "Hero" {
            sqlx::query_scalar(
                "SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)",
            )
            .bind(account)
            .bind(metadata.hero_index)
            .fetch_one(&mut *db)
            .await?
        } else {
            false
        };
        if owned && metadata.duplicate_reward_index > 0 {
            let reward = state
                .tables
                .get_reward(metadata.duplicate_reward_index)
                .ok_or_else(|| ServerError::Internal("Missing duplicate hero reward".into()))?;
            tutorial::currency(db, account, "Gold", reward.roll_gold(), &mut rewards).await?;
            tutorial::currency(db, account, "Gem", reward.roll_gem(), &mut rewards).await?;
            for drop in reward.roll_items(&state.tables.reward_string_pool) {
                let (code, _) = crate::tables::parse_item_code(&drop.item_code);
                let index = state.tables.get_item_index(&code).ok_or_else(|| {
                    ServerError::Internal("Unresolved duplicate hero reward".into())
                })?;
                crate::api::inventory::item::give(
                    db,
                    state,
                    account,
                    index,
                    drop.count,
                    drop.star_min,
                    drop.custom_option_index,
                    &mut rewards,
                )
                .await?;
            }
        } else if !owned {
            crate::api::inventory::item::give(
                db,
                state,
                account,
                item.item_index,
                item.item_count,
                0,
                0,
                &mut rewards,
            )
            .await?;
        }
    }
    let equipment = attachments::<EquipItemInfo>(row, "reward_equipment")?;
    if equipment.len() > 1000 {
        return Err(ServerError::Internal(
            "Too many mail equipment attachments".into(),
        ));
    }
    if !equipment.is_empty() {crate::api::inventory::item::capacity(db,state,account,0,equipment.len() as i64).await?;}
    for mut item in equipment {
        if !state
            .tables
            .items
            .reward_item(item.item_index)
            .is_some_and(|m| m.kind == "Equip")
        {
            return Err(ServerError::Internal("Invalid mail equipment".into()));
        }
        item.created_time = state.server_time_str();
        tutorial::equipment(db, account, item, &mut rewards).await?;
    }
    // The native response has a single HeroAddResult. Reject malformed multi-hero mail atomically.
    if rewards.heroes.len() > 1 {
        return Err(ServerError::Internal(
            "A mail may grant only one hero".into(),
        ));
    }
    tutorial::team_exp(db, state, account, rewards.team_exp_to_add, &mut rewards).await?;
    let mut result = json!({"CurrencyResults":rewards.currencies,"ItemResults":rewards.items,"EquipItemResults":rewards.equipment,"StaminaResults":[]});
    if stamina > 0 {
        let row=sqlx::query("UPDATE user_info SET stamina=stamina+? WHERE account_id=? RETURNING stamina,stamina_recharge_time").bind(stamina).bind(account).fetch_one(&mut *db).await?;
        result["StaminaResults"] = json!([{"Type":"Chicken","AddValue":stamina,"NewValue":row.get::<i64,_>("stamina"),"StaminaRechargeTime":chrono::DateTime::from_timestamp(row.get::<i64,_>("stamina_recharge_time"),0).map(|v|v.format("%Y-%m-%d %H:%M:%S").to_string()),"NextRechargeRemainTime":0,"FullRechargeRemainTime":0,"RechargeCount":0,"IsHide":false}]);
    }
    if let Some(index) = rewards.heroes.first() {
        result["HeroAddResult"] = json!({"HeroInfo":tutorial::hero_info(db,account,*index).await?,"TeamExpResult":rewards.team_exp.first()});
    }
    sqlx::query("UPDATE mails SET is_received=1,is_read=1,read_time=datetime('now'),received_time=datetime('now'),receipt=? WHERE mail_id=? AND account_id=?")
        .bind(result.to_string()).bind(row.get::<i64,_>("mail_id")).bind(account).execute(db).await?;
    Ok(result)
}

pub async fn receive_mail(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    receive(state, body, false).await
}
pub async fn receive_all_mail(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    receive(state, body, true).await
}
async fn receive(state: AppState, body: Bytes, all: bool) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    deliver_globals(&state, account).await?;
    let id = req.number("MailIndex", 0)?;
    let max = if all {
        req.number("MaxCount", 50)?.clamp(1, 100)
    } else {
        1
    };
    let mut tx = state.db.begin().await?;
    // Acquire the write lock before selecting attachments: concurrent claims cannot both grant.
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(account)
        .execute(&mut *tx)
        .await?;
    let rows=sqlx::query("SELECT * FROM mails WHERE account_id=? AND is_received=0 AND (?=1 OR mail_id=?) AND (expires_at IS NULL OR expires_at='' OR datetime(expires_at)>datetime('now')) AND (opens_at IS NULL OR opens_at='' OR datetime(opens_at)<=datetime('now')) ORDER BY mail_id DESC LIMIT ?")
        .bind(account).bind(all).bind(id).bind(max).fetch_all(&mut *tx).await?;
    if !all && rows.is_empty() {
        return Ok(Json(response("Fail")));
    }
    let mut results = vec![];
    for row in rows {
        match claim(&mut tx, &state, account, &row).await {
            Ok(result) => results.push(result),
            Err(error) => {
                tracing::warn!(%error, "Mail claim rolled back");
                return Ok(Json(response("Fail")));
            }
        }
    }
    tx.commit().await?;
    Ok(Json(if all {
        json!({"BaseResult":"Success","Result":"Success","ReceivedMailCount":results.len(),"MailReceiveResults":results})
    } else {
        json!({"BaseResult":"Success","Result":"Success","MailReceiveResult":results[0]})
    }))
}

// ============================================================
// Helper: Send System Mail (for internal use)
// ============================================================

#[allow(dead_code)]
pub async fn send_system_mail(
    state: &AppState,
    receiver_id: i64,
    title: &str,
    content: &str,
    gold: i64,
    gem: i64,
    items: Option<Vec<MailItemInfo>>,
) -> Result<i64> {
    let items_json = items.map(|i| serde_json::to_string(&i).unwrap_or_default());

    let result = sqlx::query(
        r#"INSERT INTO mails (account_id, sender, title, content, 
           reward_gold, reward_gem, reward_items, is_received)
           VALUES (?, 'System', ?, ?, ?, ?, ?, 0)"#,
    )
    .bind(receiver_id)
    .bind(title)
    .bind(content)
    .bind(gold)
    .bind(gem)
    .bind(items_json)
    .execute(&state.db)
    .await?;

    let id = result.last_insert_rowid();
    state.chat.notify(
        receiver_id,
        0,
        "NewMailArrived",
        json!({"MailIndex":id,"IsGlobal":false}),
    );
    Ok(id)
}
