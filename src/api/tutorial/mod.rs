//! Client-driven tutorial completion with atomic, replayable rewards.
use crate::api::battle::campaign_handlers::CurrencyResultInfo3;
use crate::{
    error::{Result, ServerError},
    models::equip::EquipItemInfo,
    state::AppState,
    tables::{add_exp, parse_item_code, TutorialDefinition},
};
use axum::{
    extract::{Form, State},
    Json,
};
use serde::{Deserialize, Serialize};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};
use std::collections::BTreeSet;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialRequest {
    pub session_id: Option<String>,
    pub session_key: Option<String>,
    pub tutorial_index: Option<i32>,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialInfo {
    pub tutorial_index: i32,
    pub completed_time: Option<String>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialResponse {
    pub base_result: &'static str,
    pub result: &'static str,
}

fn account(state: &AppState, req: &TutorialRequest) -> Result<i64> {
    let key = req
        .session_key
        .as_ref()
        .or(req.session_id.as_ref())
        .ok_or(ServerError::SessionExpired)?;
    state
        .get_session(key)
        .map(|session| session.account_id)
        .ok_or(ServerError::SessionExpired)
}

fn definition<'a>(
    state: &'a AppState,
    req: &TutorialRequest,
) -> Result<(i32, &'a TutorialDefinition)> {
    let index = req
        .tutorial_index
        .ok_or_else(|| ServerError::InvalidRequest("TutorialIndex is required".into()))?;
    state
        .tables
        .tutorials
        .get(index)
        .map(|data| (index, data))
        .ok_or_else(|| ServerError::InvalidRequest(format!("Unknown tutorial {index}")))
}

pub async fn begin_tutorial(
    State(state): State<AppState>,
    Form(req): Form<TutorialRequest>,
) -> Result<Json<TutorialResponse>> {
    let account_id = account(&state, &req)?;
    let (index, _) = definition(&state, &req)?;
    // Beginning a sequence is not evidence that its scripted battle has been won.
    sqlx::query("INSERT INTO tutorial_progress (account_id, tutorial_index) VALUES (?, ?) ON CONFLICT(account_id, tutorial_index) DO NOTHING")
        .bind(account_id).bind(index).execute(&state.db).await?;
    Ok(Json(TutorialResponse {
        base_result: "Success",
        result: "Success",
    }))
}

#[derive(Default)]
pub(crate) struct Rewards {
    pub(crate) pets: Vec<Value>,
    pub(crate) currencies: Vec<Value>,
    pub(crate) items: Vec<Value>,
    pub(crate) equipment: Vec<EquipItemInfo>,
    pub(crate) heroes: BTreeSet<i32>,
    pub(crate) team_exp: Vec<Value>,
    pub(crate) team_exp_to_add: i64,
    pub(crate) dungeons: Vec<Value>,
}

pub(crate) async fn currency(
    db: &mut SqliteConnection,
    account_id: i64,
    kind: &str,
    amount: i64,
    rewards: &mut Rewards,
) -> Result<()> {
    if amount == 0 {
        return Ok(());
    }
    let column = match kind {
        "Gold" => "gold",
        "Gem" => "gem",
        "RaidPoint" => "raid_point",
        "EventDungeonPoint" => "event_dungeon_point",
        _ => {
            return Err(ServerError::Internal(format!(
                "Unsupported tutorial currency {kind}"
            )))
        }
    };
    let row = sqlx::query(&format!("UPDATE user_info SET {column} = {column} + ? WHERE account_id = ? RETURNING {column}, pay_gem"))
        .bind(amount).bind(account_id).fetch_one(&mut *db).await?;
    let value = row.get::<i64, _>(column);
    let info = if kind == "Gem" {
        CurrencyResultInfo3::gem(amount, value, row.get("pay_gem"))
    } else {
        CurrencyResultInfo3::new(kind, amount, value)
    };
    rewards.currencies.push(json!(info));
    Ok(())
}

pub(crate) async fn team_exp(
    db: &mut SqliteConnection,
    state: &AppState,
    account_id: i64,
    amount: i64,
    rewards: &mut Rewards,
) -> Result<()> {
    if amount == 0 {
        return Ok(());
    }
    let row = sqlx::query("SELECT team_level, team_exp FROM user_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_one(&mut *db)
        .await?;
    let old_level: i32 = row.get("team_level");
    let levels = &state.tables.tutorials.support.team_levels;
    let cap = levels
        .iter()
        .map(|row| row.level)
        .max()
        .ok_or_else(|| ServerError::Internal("Missing team levels".into()))?;
    let (level, exp) = add_exp(levels, old_level, row.get("team_exp"), amount, cap);
    sqlx::query("UPDATE user_info SET team_level = ?, team_exp = ? WHERE account_id = ?")
        .bind(level)
        .bind(exp)
        .bind(account_id)
        .execute(&mut *db)
        .await?;
    rewards.team_exp.push(
        json!({"AddValue": amount, "NewValue": exp, "OldLevel": old_level, "NewLevel": level}),
    );
    Ok(())
}

pub(crate) async fn equipment(
    db: &mut SqliteConnection,
    account_id: i64,
    mut item: EquipItemInfo,
    rewards: &mut Rewards,
) -> Result<()> {
    let result = sqlx::query("INSERT INTO equip_items (account_id, item_index, star, level, exp, option_index_1, option_step_1, option_index_2, option_step_2, option_index_3, option_step_3, option_index_4, option_step_4, rune_slot_count, rune_item_index_1, rune_item_index_2, rune_item_index_3, created_time, identified) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 1)")
        .bind(account_id).bind(item.item_index).bind(item.star).bind(item.level).bind(item.exp)
        .bind(item.option_index_1).bind(item.option_step_1).bind(item.option_index_2).bind(item.option_step_2)
        .bind(item.option_index_3).bind(item.option_step_3).bind(item.option_index_4).bind(item.option_step_4)
        .bind(item.rune_slot_count).bind(item.rune_item_index_1).bind(item.rune_item_index_2).bind(item.rune_item_index_3)
        .bind(&item.created_time).execute(&mut *db).await?;
    item.slot_index = result.last_insert_rowid() as i32;
    sqlx::query("UPDATE equip_items SET option_renew_count_1 = ?, is_renewed_option_1 = ?, option_renew_count_2 = ?, is_renewed_option_2 = ?, option_renew_count_3 = ?, is_renewed_option_3 = ?, option_renew_count_4 = ?, is_renewed_option_4 = ? WHERE slot_index = ? AND account_id = ?")
        .bind(item.option_renew_count_1).bind(item.is_renewed_option_1)
        .bind(item.option_renew_count_2).bind(item.is_renewed_option_2)
        .bind(item.option_renew_count_3).bind(item.is_renewed_option_3)
        .bind(item.option_renew_count_4).bind(item.is_renewed_option_4)
        .bind(item.slot_index).bind(account_id).execute(&mut *db).await?;
    item.uid = item.slot_index.to_string();
    item.identified = 1;
    rewards.equipment.push(item);
    Ok(())
}

pub(crate) async fn grant_item(
    db: &mut SqliteConnection,
    state: &AppState,
    account_id: i64,
    index: i32,
    count: i32,
    star: i32,
    custom: i32,
    rewards: &mut Rewards,
) -> Result<()> {
    let support = &state.tables.tutorials.support;
    let data = support
        .items
        .get(&index)
        .or_else(|| state.tables.items.reward_item(index))
        .ok_or_else(|| ServerError::Internal(format!("Missing item metadata {index}")))?;
    if data.kind == "Hero" {
        // hero_id is account-local in the existing schema; never overwrite an owned hero.
        let result = sqlx::query("INSERT INTO heroes (account_id, hero_id, hero_index, star, level, transcend) SELECT ?, COALESCE(MAX(hero_id), 0) + 1, ?, ?, ?, ? FROM heroes WHERE account_id = ? HAVING NOT EXISTS (SELECT 1 FROM heroes WHERE account_id = ? AND hero_index = ?)")
            .bind(account_id).bind(data.hero_index).bind(data.star).bind(data.level).bind(data.transcend)
            .bind(account_id).bind(account_id).bind(data.hero_index).execute(&mut *db).await?;
        if result.rows_affected() > 0 {
            rewards.heroes.insert(data.hero_index);
            let star_data = support
                .hero_stars
                .iter()
                .find(|s| s.star == data.star && s.transcended == data.transcend)
                .ok_or_else(|| ServerError::Internal("Missing hero star data".into()))?;
            rewards.team_exp_to_add += star_data.get_hero_team_exp;
        }
    } else if data.kind == "Equip" {
        for _ in 0..count {
            let mut item = if custom != 0 {
                support
                    .custom_equipment
                    .get(&custom)
                    .or_else(|| state.tables.inventory.custom_equipment.get(&custom))
                    .cloned()
                    .ok_or_else(|| {
                        ServerError::Internal(format!("Missing custom equipment {custom}"))
                    })?
            } else {
                EquipItemInfo::default()
            };
            item.item_index = index;
            item.star = star;
            item.created_time = state.server_time_str();
            equipment(db, account_id, item, rewards).await?;
        }
    } else {
        let row = sqlx::query("INSERT INTO items (account_id, item_index, count) VALUES (?, ?, ?) ON CONFLICT(account_id, item_index) DO UPDATE SET count = count + excluded.count RETURNING count, locked")
            .bind(account_id).bind(index).bind(count).fetch_one(&mut *db).await?;
        rewards.items.push(json!({"ItemIndex": index, "AddCount": count, "NewCount": row.get::<i32, _>("count"), "AddBoosterCount": 0, "AddNPCBoosterCount": 0, "AddBonusAssignedItemPercent": 0, "Locked": row.get::<i32,_>("locked"), "IsFirstClearReward": false}));
    }
    Ok(())
}

async fn reward_action(
    db: &mut SqliteConnection,
    state: &AppState,
    account_id: i64,
    action: &[String],
    rewards: &mut Rewards,
) -> Result<()> {
    let values = action[1..]
        .iter()
        .map(|s| {
            s.parse::<i32>()
                .map_err(|_| ServerError::Internal(format!("Invalid tutorial action {action:?}")))
        })
        .collect::<Result<Vec<_>>>()?;
    match (action[0].as_str(), values.as_slice()) {
        ("AddHeroExp", [amount, heroes @ ..]) if *amount >= 0 && !heroes.is_empty() => {
            for index in heroes {
                let row = sqlx::query("SELECT level, exp, star, transcend FROM heroes WHERE account_id = ? AND hero_index = ?")
                    .bind(account_id).bind(index).fetch_optional(&mut *db).await?;
                // Alternate tutorial routes can run without all four starter heroes.
                let Some(row) = row else {
                    continue;
                };
                let support = &state.tables.tutorials.support;
                let cap = support
                    .hero_stars
                    .iter()
                    .find(|s| {
                        s.star == row.get::<i32, _>("star")
                            && s.transcended == row.get::<i32, _>("transcend")
                    })
                    .ok_or_else(|| ServerError::Internal("Missing hero level cap".into()))?
                    .max_hero_level;
                let (level, exp) = add_exp(
                    &support.hero_levels,
                    row.get("level"),
                    row.get("exp"),
                    *amount as i64,
                    cap,
                );
                sqlx::query(
                    "UPDATE heroes SET level = ?, exp = ? WHERE account_id = ? AND hero_index = ?",
                )
                .bind(level)
                .bind(exp)
                .bind(account_id)
                .bind(index)
                .execute(&mut *db)
                .await?;
                rewards.heroes.insert(*index);
            }
        }
        ("SetHeroStar", [index, star]) => {
            sqlx::query(
                "UPDATE heroes SET star = MAX(star, ?) WHERE account_id = ? AND hero_index = ?",
            )
            .bind(star)
            .bind(account_id)
            .bind(index)
            .execute(&mut *db)
            .await?;
            rewards.heroes.insert(*index);
        }
        ("AddRaidPoint", [amount]) => {
            currency(db, account_id, "RaidPoint", *amount as i64, rewards).await?
        }
        ("AddCustomEquipItem", [index]) => {
            let mut item = state
                .tables
                .tutorials
                .support
                .custom_equipment
                .get(index)
                .cloned()
                .ok_or_else(|| {
                    ServerError::Internal(format!("Missing custom equipment {index}"))
                })?;
            item.created_time = state.server_time_str();
            equipment(db, account_id, item, rewards).await?;
        }
        _ => {
            return Err(ServerError::Internal(format!(
                "Unsupported tutorial action {action:?}"
            )))
        }
    }
    Ok(())
}

pub(crate) async fn hero_info(db: &mut SqliteConnection, account_id: i64, index: i32) -> Result<Value> {
    crate::api::heroes::info(db, account_id, index).await
}

async fn grant_rewards(
    db: &mut SqliteConnection,
    state: &AppState,
    account_id: i64,
    data: &TutorialDefinition,
    rewards: &mut Rewards,
) -> Result<()> {
    let mut gold = data.gold;
    let mut gems = data.gem;
    for index in &data.rewards {
        let reward = state
            .tables
            .get_reward(*index)
            .ok_or_else(|| ServerError::Internal(format!("Missing reward {index}")))?;
        gold += reward.roll_gold();
        gems += reward.roll_gem();
        for drop in reward.roll_items(&state.tables.reward_string_pool) {
            if drop.item_code == "EventDungeonPoint" {
                currency(
                    db,
                    account_id,
                    "EventDungeonPoint",
                    drop.count as i64,
                    rewards,
                )
                .await?;
                continue;
            }
            let (code, filter) = parse_item_code(&drop.item_code);
            let (item_index, count, star) = if let Some(index) = state.tables.get_item_index(&code)
            {
                use rand::Rng;
                let star =
                    rand::thread_rng().gen_range(drop.star_min..=drop.star_max.max(drop.star_min));
                (index, drop.count, star)
            } else if let Some((index, count, star, _)) =
                state.tables.roll_item_from_group_code(&code, &filter)
            {
                (index, count * drop.count, star)
            } else {
                return Err(ServerError::Internal(format!(
                    "Unresolved tutorial reward {}",
                    drop.item_code
                )));
            };
            grant_item(
                db,
                state,
                account_id,
                item_index,
                count,
                star,
                drop.custom_option_index,
                rewards,
            )
            .await?;
        }
    }
    currency(db, account_id, "Gold", gold, rewards).await?;
    currency(db, account_id, "Gem", gems, rewards).await?;
    for action in &data.actions {
        reward_action(db, state, account_id, action, rewards).await?;
    }
    team_exp(db, state, account_id, rewards.team_exp_to_add, rewards).await?;
    for &(chapter, dungeon) in &data.dungeons {
        let now = state.server_time_str();
        sqlx::query("INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) VALUES (?, ?, ?, 1, 3, 1, ?) ON CONFLICT(account_id, chapter_id, dungeon_id) DO UPDATE SET clear_count = MAX(clear_count, 1), best_star = MAX(best_star, 3), is_unlocked = 1, completed_time = COALESCE(completed_time, excluded.completed_time)")
            .bind(account_id).bind(chapter).bind(dungeon).bind(&now).execute(&mut *db).await?;
        let row = sqlx::query("SELECT clear_count, best_star, completed_time FROM campaign_progress WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?")
            .bind(account_id).bind(chapter).bind(dungeon).fetch_one(&mut *db).await?;
        let difficulty = state.tables.tutorials.dungeon_difficulty(chapter, dungeon);
        rewards.dungeons.push(json!({"ChapterIndex": chapter, "DungeonIndex": dungeon,
            "MaxStar": difficulty * 10 + row.get::<i32, _>("best_star"), "FirstRewardedDiff": 1 << difficulty, "ScenarioComplete": 1,
            "VisitedTime": row.get::<String, _>("completed_time"), "CompletedTime": row.get::<String, _>("completed_time"),
            "DailyCompletedCount": row.get::<i32, _>("clear_count"), "ResetCount": 0}));
        // Only unlock real successors. The client derives their availability from the cleared node.
        if state
            .tables
            .get_campaign_dungeon(chapter, dungeon + 1)
            .is_some()
        {
            sqlx::query("INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) VALUES (?, ?, ?, 1) ON CONFLICT(account_id, chapter_id, dungeon_id) DO UPDATE SET is_unlocked = 1")
                .bind(account_id).bind(chapter).bind(dungeon + 1).execute(&mut *db).await?;
        }
    }
    Ok(())
}

/// Preserve the reward receipt on retry, but return current totals so an old
/// request cannot undo later purchases, hero upgrades, or equipment changes in the client.
async fn refresh_receipt(
    db: &mut SqliteConnection,
    account_id: i64,
    response: &mut Value,
) -> Result<()> {
    let user = sqlx::query("SELECT * FROM user_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_one(&mut *db)
        .await?;
    if let Some(currencies) = response["CurrencyResults"].as_array_mut() {
        for result in currencies {
            let column = match result["CurrencyType"].as_str() {
                Some("Gold") => "gold",
                Some("Gem") => "gem",
                Some("RaidPoint") => "raid_point",
                Some("EventDungeonPoint") => "event_dungeon_point",
                _ => continue,
            };
            let current: i64 = user.get(column);
            result["NewValue"] = json!(current);
            if column == "gem" {
                *result = json!(CurrencyResultInfo3::gem(
                    result["AddValue"].as_i64().unwrap_or(0),
                    current,
                    user.get("pay_gem")
                ));
            }
        }
    }
    if let Some(items) = response["ItemResults"].as_array_mut() {
        for item in items {
            let count: Option<i64> = sqlx::query_scalar(
                "SELECT count FROM items WHERE account_id = ? AND item_index = ?",
            )
            .bind(account_id)
            .bind(item["ItemIndex"].as_i64())
            .fetch_optional(&mut *db)
            .await?;
            item["NewCount"] = json!(count.unwrap_or(0));
            let locked:Option<i32>=sqlx::query_scalar("SELECT locked FROM items WHERE account_id=? AND item_index=?").bind(account_id).bind(item["ItemIndex"].as_i64()).fetch_optional(&mut *db).await?;
            item["Locked"]=json!(locked.unwrap_or(0));
        }
    }
    if let Some(heroes) = response["HeroInfos"].as_array_mut() {
        for hero in heroes {
            *hero = hero_info(
                db,
                account_id,
                hero["HeroIndex"].as_i64().unwrap_or(0) as i32,
            )
            .await?;
        }
    }
    if let Some(equipment) = response["EquipItemResults"].as_array_mut() {
        let mut current = Vec::new();
        for item in equipment.iter() {
            if let Some(row) =
                sqlx::query("SELECT * FROM equip_items WHERE account_id = ? AND slot_index = ?")
                    .bind(account_id)
                    .bind(item["SlotIndex"].as_i64())
                    .fetch_optional(&mut *db)
                    .await?
            {
                current.push(json!(EquipItemInfo::from_row(&row)));
            }
        }
        *equipment = current;
    }
    if let Some(results) = response["TeamExpResultInfos"].as_array_mut() {
        for result in results {
            result["NewLevel"] = json!(user.get::<i32, _>("team_level"));
            result["NewValue"] = json!(user.get::<i64, _>("team_exp"));
        }
    }
    if let Some(dungeons) = response["DungeonInfos"].as_array_mut() {
        for dungeon in dungeons {
            let row = sqlx::query("SELECT clear_count, best_star, completed_time FROM campaign_progress WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?")
                .bind(account_id).bind(dungeon["ChapterIndex"].as_i64()).bind(dungeon["DungeonIndex"].as_i64()).fetch_one(&mut *db).await?;
            let difficulty = dungeon["MaxStar"].as_i64().unwrap_or(0) / 10;
            dungeon["MaxStar"] = json!(difficulty * 10 + row.get::<i64, _>("best_star"));
            dungeon["DailyCompletedCount"] = json!(row.get::<i32, _>("clear_count"));
            dungeon["CompletedTime"] = json!(row.get::<Option<String>, _>("completed_time"));
        }
    }
    Ok(())
}

pub async fn complete_tutorial(
    State(state): State<AppState>,
    Form(req): Form<TutorialRequest>,
) -> Result<Json<Value>> {
    let account_id = account(&state, &req)?;
    let (index, data) = definition(&state, &req)?;
    let mut tx = state.db.begin().await?;
    let now = state.server_time_str();
    // First statement acquires the SQLite write lock. A concurrent completion must wait.
    let claimed = sqlx::query("INSERT INTO tutorial_progress (account_id, tutorial_index, is_completed, completed_time) VALUES (?, ?, 1, ?) ON CONFLICT(account_id, tutorial_index) DO UPDATE SET is_completed = 1, completed_time = excluded.completed_time WHERE tutorial_progress.is_completed = 0")
        .bind(account_id).bind(index).bind(&now).execute(&mut *tx).await?.rows_affected() > 0;
    if !claimed {
        let row = sqlx::query("SELECT completed_time, completion_response FROM tutorial_progress WHERE account_id = ? AND tutorial_index = ?")
            .bind(account_id).bind(index).fetch_one(&mut *tx).await?;
        let response = match row.get::<Option<String>, _>("completion_response") {
            Some(response) => {
                let mut response = serde_json::from_str(&response)
                    .map_err(|e| ServerError::Internal(e.to_string()))?;
                refresh_receipt(&mut tx, account_id, &mut response).await?;
                response
            }
            None => {
                json!({"BaseResult": "Success", "Result": "Success", "Info": {"TutorialIndex": index, "CompletedTime": row.get::<Option<String>, _>("completed_time")}})
            }
        };
        tx.commit().await?;
        return Ok(Json(response));
    }
    let mut rewards = Rewards::default();
    grant_rewards(&mut tx, &state, account_id, data, &mut rewards).await?;
    let mut heroes = Vec::new();
    for hero in &rewards.heroes {
        heroes.push(hero_info(&mut tx, account_id, *hero).await?);
    }
    let response = json!({"BaseResult": "Success", "Result": "Success", "Info": {"TutorialIndex": index, "CompletedTime": now},
        "CurrencyResults": rewards.currencies, "ItemResults": rewards.items, "EquipItemResults": rewards.equipment,
        "HeroInfos": heroes, "TeamExpResultInfos": rewards.team_exp, "StaminaResultInfos": [], "DungeonInfos": rewards.dungeons});
    sqlx::query("UPDATE tutorial_progress SET completion_response = ? WHERE account_id = ? AND tutorial_index = ?")
        .bind(response.to_string()).bind(account_id).bind(index).execute(&mut *tx).await?;
    tx.commit().await?;
    Ok(Json(response))
}

pub async fn get_tutorial_progress(
    State(state): State<AppState>,
    Form(req): Form<TutorialRequest>,
) -> Result<Json<Value>> {
    let account_id = account(&state, &req)?;
    let rows = sqlx::query("SELECT tutorial_index, completed_time FROM tutorial_progress WHERE account_id = ? AND is_completed = 1 ORDER BY tutorial_index")
        .bind(account_id).fetch_all(&state.db).await?;
    let tutorials: Vec<_> = rows
        .iter()
        .map(|row| TutorialInfo {
            tutorial_index: row.get("tutorial_index"),
            completed_time: row.get("completed_time"),
        })
        .collect();
    Ok(Json(
        json!({"BaseResult": "Success", "Result": "Success", "Tutorials": tutorials}),
    ))
}

pub async fn skip_tutorial(
    State(state): State<AppState>,
    Form(req): Form<TutorialRequest>,
) -> Result<Json<TutorialResponse>> {
    let account_id = account(&state, &req)?;
    let mut tx = state.db.begin().await?;
    sqlx::query("INSERT INTO tutorial_settings (account_id, is_skipped) VALUES (?, 1) ON CONFLICT(account_id) DO UPDATE SET is_skipped = 1")
        .bind(account_id).execute(&mut *tx).await?;
    // Skipping suppresses the World Tree story battle, but the path to 1-18
    // still requires its completion flag. Grant no battle clears or rewards.
    sqlx::query("INSERT INTO tutorial_progress (account_id, tutorial_index, is_completed, completed_time) VALUES (?, 14000, 1, datetime('now')) ON CONFLICT(account_id, tutorial_index) DO UPDATE SET is_completed = 1, completed_time = excluded.completed_time WHERE tutorial_progress.is_completed = 0")
        .bind(account_id).execute(&mut *tx).await?;
    // A skipped introduction still needs the basic party to play campaign battles.
    let mut rewards = Rewards::default();
    for index in 1..=4 {
        grant_item(&mut tx, &state, account_id, index, 1, 0, 0, &mut rewards).await?;
    }
    team_exp(
        &mut tx,
        &state,
        account_id,
        rewards.team_exp_to_add,
        &mut rewards,
    )
    .await?;
    sqlx::query("INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) VALUES (?, 1, 1, 1) ON CONFLICT(account_id, chapter_id, dungeon_id) DO UPDATE SET is_unlocked = 1")
        .bind(account_id).execute(&mut *tx).await?;
    tx.commit().await?;
    Ok(Json(TutorialResponse {
        base_result: "Success",
        result: "Success",
    }))
}

pub async fn get_tutorial_reward(
    State(state): State<AppState>,
    Form(req): Form<TutorialRequest>,
) -> Result<Json<Value>> {
    account(&state, &req)?;
    let (index, data) = definition(&state, &req)?;
    let mut gold = data.gold;
    let mut gems = data.gem;
    for reward_index in &data.rewards {
        let reward = state
            .tables
            .get_reward(*reward_index)
            .ok_or_else(|| ServerError::Internal(format!("Missing reward {reward_index}")))?;
        if reward.gold_rate == 1000 {
            gold += reward.gold_min as i64;
        }
        if reward.gem_rate == 1000 {
            gems += reward.gem_min as i64;
        }
    }
    Ok(Json(
        json!({"BaseResult": "Success", "Result": "Success", "TutorialIndex": index, "RewardGold": gold, "RewardGem": gems, "RewardIndices": data.rewards}),
    ))
}

#[cfg(test)]
mod tests;
