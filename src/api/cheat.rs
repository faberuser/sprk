use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::BaseResultType,
    state::AppState,
};

/// Core hero indices from CreatureTable (index 1-102 + 111 Valance).
/// This is the actual playable hero roster; higher indices are variant skins,
/// NPCs, and special creatures that shouldn't appear in the hero index.
const ALL_HERO_INDICES: &[i32] = &[
    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
    21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
    41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60,
    61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
    81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
    101, 102, 111,
];

/// Cheat codes for development/testing
/// These endpoints allow GM/admin commands for testing purposes

/// GM add currency request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddCurrencyRequest {
    pub session_id: Option<String>,
    pub gold: Option<i64>,
    pub gem: Option<i32>,
    pub stamina: Option<i32>,
}

/// GM add currency response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddCurrencyResponse {
    pub base_result: i32,
    pub new_gold: i64,
    pub new_gem: i32,
    pub new_stamina: i32,
}

/// Handle GM add currency request
pub async fn gm_add_currency(
    State(state): State<AppState>,
    Form(req): Form<GmAddCurrencyRequest>,
) -> Result<Json<GmAddCurrencyResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let gold = req.gold.unwrap_or(0);
    let gem = req.gem.unwrap_or(0);
    let stamina = req.stamina.unwrap_or(0);

    // Add currencies
    sqlx::query(
        "UPDATE user_info SET gold = gold + ?, gem = gem + ?, stamina = stamina + ? WHERE account_id = ?"
    )
    .bind(gold)
    .bind(gem)
    .bind(stamina)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    // Get new values
    let user = sqlx::query("SELECT gold, gem, stamina FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_one(&state.db)
        .await?;

    Ok(Json(GmAddCurrencyResponse {
        base_result: BaseResultType::Success as i32,
        new_gold: user.get("gold"),
        new_gem: user.get("gem"),
        new_stamina: user.get("stamina"),
    }))
}

/// GM add hero request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddHeroRequest {
    pub session_id: Option<String>,
    pub hero_id: Option<i64>,
    pub level: Option<i32>,
    pub star: Option<i32>,
}

/// GM add hero response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddHeroResponse {
    pub base_result: i32,
    pub result: i32,
    pub unique_hero_id: i64,
}

/// Handle GM add hero request
pub async fn gm_add_hero(
    State(state): State<AppState>,
    Form(req): Form<GmAddHeroRequest>,
) -> Result<Json<GmAddHeroResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let hero_id = req.hero_id.ok_or_else(|| ServerError::InvalidRequest("Missing hero_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let level = req.level.unwrap_or(1);
    let star = req.star.unwrap_or(1);

    // Add hero - hero_index same as hero_id for individual adds
    let result = sqlx::query(
        "INSERT INTO heroes (account_id, hero_id, hero_index, star, level, skill_level_1, skill_level_2, skill_level_3, skill_level_4) VALUES (?, ?, ?, ?, ?, 1, 1, 1, 1)"
    )
    .bind(session.account_id)
    .bind(hero_id)
    .bind(hero_id)
    .bind(star)
    .bind(level)
    .execute(&state.db)
    .await?;

    let unique_hero_id = result.last_insert_rowid();

    Ok(Json(GmAddHeroResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        unique_hero_id,
    }))
}

/// GM set level request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmSetLevelRequest {
    pub session_id: Option<String>,
    pub level: Option<i32>,
}

/// GM set level response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmSetLevelResponse {
    pub base_result: i32,
    pub new_level: i32,
}

/// Handle GM set level request
pub async fn gm_set_level(
    State(state): State<AppState>,
    Form(req): Form<GmSetLevelRequest>,
) -> Result<Json<GmSetLevelResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let level = req.level.ok_or_else(|| ServerError::InvalidRequest("Missing level".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Set player level
    sqlx::query("UPDATE user_info SET level = ? WHERE account_id = ?")
        .bind(level)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(GmSetLevelResponse {
        base_result: BaseResultType::Success as i32,
        new_level: level,
    }))
}

/// GM unlock all request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmUnlockAllRequest {
    pub session_id: Option<String>,
}

/// GM unlock all response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmUnlockAllResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle GM unlock all request - unlocks all chapters, skips tutorial
pub async fn gm_unlock_all(
    State(state): State<AppState>,
    Form(req): Form<GmUnlockAllRequest>,
) -> Result<Json<GmUnlockAllResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let completed_time = state.server_time_str();

    // Skip tutorial - mark all key tutorials as completed
    let key_tutorials = [
        1, 1000, 10000, 10010, 10110, 10202, 10220, 10230, 10300, 10310,
        10400, 10600, 7400,
    ];
    for &tutorial_idx in &key_tutorials {
        sqlx::query(
            "INSERT OR REPLACE INTO tutorial_progress (account_id, tutorial_index, is_completed, completed_time) VALUES (?, ?, 1, ?)"
        )
        .bind(session.account_id)
        .bind(tutorial_idx)
        .bind(&completed_time)
        .execute(&state.db)
        .await?;
    }

    // Unlock all main story chapters (1-11) with all their dungeons cleared
    // This satisfies the prerequisite checks (ReqChapterIndex/ReqDungeonIndex)
    // Chapter dungeon counts vary; we mark key dungeons as cleared per chapter
    let chapter_dungeons: &[(i32, &[i32])] = &[
        (1, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23]),
        (2, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22]),
        (3, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24]),
        (4, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23]),
        (5, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24]),
        (6, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 24]),
        (7, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]),
        (8, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26]),
        (9, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23]),
        (10, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41]),
        (11, &[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]),
        // GodKingTemple prerequisite: IsOpenGodKingTemple() checks
        // ConstantData "GodKingTempleReqChapter"(default 65), "GodKingTempleReqDungeon"(default 11)
        // Since those keys don't exist in ConstantTable.jit, the fallback (ch65, d11) is used.
        (65, &[11_i32]),
    ];

    for &(chapter_id, dungeons) in chapter_dungeons {
        for &dungeon_id in dungeons {
            sqlx::query(
                "INSERT OR REPLACE INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) VALUES (?, ?, ?, 1, 3, 1, ?)"
            )
            .bind(session.account_id)
            .bind(chapter_id)
            .bind(dungeon_id)
            .bind(&completed_time)
            .execute(&state.db)
            .await?;
        }
    }

    // Set team level high enough to access all content
    sqlx::query(
        "UPDATE user_info SET team_level = MAX(team_level, 90) WHERE account_id = ?"
    )
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    Ok(Json(GmUnlockAllResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// GM reset account request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmResetAccountRequest {
    pub session_id: Option<String>,
    pub keep_heroes: Option<bool>,
}

/// GM reset account response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmResetAccountResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle GM reset account request
pub async fn gm_reset_account(
    State(state): State<AppState>,
    Form(req): Form<GmResetAccountRequest>,
) -> Result<Json<GmResetAccountResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let keep_heroes = req.keep_heroes.unwrap_or(false);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Reset user info to defaults
    sqlx::query(
        "UPDATE user_info SET level = 1, exp = 0, gold = 10000, gem = 100, stamina = 100 WHERE account_id = ?"
    )
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    // Clear progress
    sqlx::query("DELETE FROM campaign_progress WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    sqlx::query("DELETE FROM tutorial_progress WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    sqlx::query("DELETE FROM attendance WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    sqlx::query("DELETE FROM achievements WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    if !keep_heroes {
        sqlx::query("DELETE FROM heroes WHERE account_id = ?")
            .bind(session.account_id)
            .execute(&state.db)
            .await?;

        sqlx::query("DELETE FROM equip_items WHERE account_id = ?")
            .bind(session.account_id)
            .execute(&state.db)
            .await?;

        sqlx::query("DELETE FROM items WHERE account_id = ?")
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    Ok(Json(GmResetAccountResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// GM add all heroes request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddAllHeroesRequest {
    pub session_id: Option<String>,
    pub level: Option<i32>,
    pub star: Option<i32>,
}

/// GM add all heroes response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddAllHeroesResponse {
    pub base_result: i32,
    pub heroes_added: i32,
}

/// Handle GM add all heroes request
pub async fn gm_add_all_heroes(
    State(state): State<AppState>,
    Form(req): Form<GmAddAllHeroesRequest>,
) -> Result<Json<GmAddAllHeroesResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let level = req.level.unwrap_or(90);
    let star = req.star.unwrap_or(5);

    // Add all real heroes from CreatureTable (TagType 1-7, base indices only)
    let mut heroes_added = 0;
    for &hero_index in ALL_HERO_INDICES {
        let existing = sqlx::query("SELECT 1 FROM heroes WHERE account_id = ? AND hero_index = ?")
            .bind(session.account_id)
            .bind(hero_index)
            .fetch_optional(&state.db)
            .await?;

        if existing.is_none() {
            sqlx::query(
                "INSERT INTO heroes (account_id, hero_id, hero_index, star, level, skill_level_1, skill_level_2, skill_level_3, skill_level_4) VALUES (?, ?, ?, ?, ?, 1, 1, 1, 1)"
            )
            .bind(session.account_id)
            .bind(hero_index as i64)  // hero_id = hero_index for simplicity
            .bind(hero_index)
            .bind(star)
            .bind(level)
            .execute(&state.db)
            .await?;
            heroes_added += 1;
        }
    }

    Ok(Json(GmAddAllHeroesResponse {
        base_result: BaseResultType::Success as i32,
        heroes_added,
    }))
}

/// Heroes that have no UW/UT items in the game data (incomplete heroes 103-110)
const NO_UWUT_HEROES: &[i32] = &[103, 104, 105, 106, 107, 108, 109, 110];

/// GM add all UW/UT request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddAllUwUtRequest {
    pub session_id: Option<String>,
}

/// GM add all UW/UT response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddAllUwUtResponse {
    pub base_result: i32,
    pub items_added: i32,
}

/// Handle GM add all UW/UT request.
///
/// Item index formula (verified against ItemTable + hero_tabledata.py localization pattern):
///   UW  → item_index = 1000 + hero_index          (equip slot 1)
///   UT1 → item_index = 103000 + hero_index         (equip slot 7)
///   UT2 → item_index = 102000 + hero_index         (equip slot 8)
///   UT3 → item_index = 101000 + hero_index         (equip slot 9)
///   UT4 → item_index = 104000 + hero_index         (equip slot 10)
///
/// Heroes 103-110 have no UW/UT items in the game tables and are skipped.
/// Only equips items into empty slots (does not overwrite existing equips).
pub async fn gm_add_all_uwut(
    State(state): State<AppState>,
    Form(req): Form<GmAddAllUwUtRequest>,
) -> Result<Json<GmAddAllUwUtResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;

    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let created_time = state.server_time_str();
    let mut items_added = 0;

    // (hero_equip_slot_column, item_index_base)
    // Slot 1 = Weapon (UW), Slots 7-10 = Treasure (UT1-UT4)
    let slot_map: &[(&str, i32)] = &[
        ("equip_item_slot_index_1",  1000),   // UW
        ("equip_item_slot_index_7",  103000), // UT1 (SubType 15)
        ("equip_item_slot_index_8",  102000), // UT2 (SubType 16)
        ("equip_item_slot_index_9",  101000), // UT3 (SubType 17)
        ("equip_item_slot_index_10", 104000), // UT4 (SubType 18)
    ];

    for &hero_index in ALL_HERO_INDICES {
        if NO_UWUT_HEROES.contains(&hero_index) {
            continue;
        }

        // Fetch the hero row with all relevant slot columns
        let hero_row = sqlx::query(
            "SELECT unique_hero_id,
                    equip_item_slot_index_1,
                    equip_item_slot_index_7,
                    equip_item_slot_index_8,
                    equip_item_slot_index_9,
                    equip_item_slot_index_10
             FROM heroes WHERE account_id = ? AND hero_index = ?"
        )
        .bind(session.account_id)
        .bind(hero_index)
        .fetch_optional(&state.db)
        .await?;

        let hero_row = match hero_row {
            Some(r) => r,
            None => continue, // Hero not owned by this account
        };

        let unique_hero_id: i64 = hero_row.get("unique_hero_id");

        for &(col, base) in slot_map {
            let current: i32 = hero_row.get(col);
            if current != 0 {
                continue; // Slot already has an item
            }

            let item_index = base + hero_index;

            // Insert the equip item instance
            let result = sqlx::query(
                "INSERT INTO equip_items (account_id, item_index, star, created_time, identified) \
                 VALUES (?, ?, 5, ?, 1)"
            )
            .bind(session.account_id)
            .bind(item_index)
            .bind(&created_time)
            .execute(&state.db)
            .await?;

            let slot_index = result.last_insert_rowid() as i32;

            // Link the equip item to the hero's slot
            sqlx::query(&format!(
                "UPDATE heroes SET {} = ? WHERE unique_hero_id = ?", col
            ))
            .bind(slot_index)
            .bind(unique_hero_id)
            .execute(&state.db)
            .await?;

            items_added += 1;
        }
    }

    Ok(Json(GmAddAllUwUtResponse {
        base_result: BaseResultType::Success as i32,
        items_added,
    }))
}
