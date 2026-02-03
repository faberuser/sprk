use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::{BaseResultType, RewardItemInfo},
    state::AppState,
};

/// Attendance reward info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct AttendanceRewardInfo {
    pub day: i32,
    pub reward_type: i32,
    pub reward_id: i64,
    pub reward_count: i32,
    pub is_received: bool,
}

/// Get attendance info request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetAttendanceInfoRequest {
    pub session_id: Option<String>,
}

/// Get attendance info response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetAttendanceInfoResponse {
    pub base_result: i32,
    pub current_day: i32,
    pub total_attendance: i32,
    pub last_attendance_date: String,
    pub rewards: Vec<AttendanceRewardInfo>,
    pub can_receive_today: bool,
}

/// Handle get attendance info request
pub async fn get_attendance_info(
    State(state): State<AppState>,
    Form(req): Form<GetAttendanceInfoRequest>,
) -> Result<Json<GetAttendanceInfoResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get attendance record
    let attendance = sqlx::query(
        "SELECT current_day, total_days, last_attendance FROM attendance WHERE account_id = ?"
    )
    .bind(session.account_id)
    .fetch_optional(&state.db)
    .await?;

    let (current_day, total_attendance, last_attendance_date, can_receive_today) = if let Some(row) = attendance {
        let current_day: i32 = row.get("current_day");
        let total_days: i32 = row.get("total_days");
        let last_attendance: String = row.get("last_attendance");
        
        // Check if can receive today
        let today = state.server_date();
        let can_receive = last_attendance != today;
        
        (current_day, total_days, last_attendance, can_receive)
    } else {
        // No record, create one
        let _today = state.server_date(); // Reserved for future use
        sqlx::query(
            "INSERT INTO attendance (account_id, current_day, total_days, last_attendance) VALUES (?, 0, 0, '')"
        )
        .bind(session.account_id)
        .execute(&state.db)
        .await?;
        
        (0, 0, String::new(), true)
    };

    // Generate reward schedule (sample rewards for 28 days)
    let rewards: Vec<AttendanceRewardInfo> = (1..=28).map(|day| {
        let (reward_type, reward_id, reward_count) = match day {
            7 | 14 | 21 | 28 => (1, 1000, 100), // Gems
            _ => (0, 0, day as i32 * 1000), // Gold
        };
        
        AttendanceRewardInfo {
            day,
            reward_type,
            reward_id,
            reward_count,
            is_received: day <= current_day,
        }
    }).collect();

    Ok(Json(GetAttendanceInfoResponse {
        base_result: BaseResultType::Success as i32,
        current_day,
        total_attendance,
        last_attendance_date,
        rewards,
        can_receive_today,
    }))
}

/// Receive attendance reward request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveAttendanceRewardRequest {
    pub session_id: Option<String>,
}

/// Receive attendance reward response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveAttendanceRewardResponse {
    pub base_result: i32,
    pub result: i32,
    pub day: i32,
    pub reward_gold: i64,
    pub reward_gem: i32,
    pub reward_items: Vec<RewardItemInfo>,
}

/// Handle receive attendance reward request
pub async fn receive_attendance_reward(
    State(state): State<AppState>,
    Form(req): Form<ReceiveAttendanceRewardRequest>,
) -> Result<Json<ReceiveAttendanceRewardResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let today = state.server_date();

    // Get current attendance
    let attendance = sqlx::query(
        "SELECT current_day, last_attendance FROM attendance WHERE account_id = ?"
    )
    .bind(session.account_id)
    .fetch_optional(&state.db)
    .await?;

    let (current_day, last_attendance) = if let Some(row) = attendance {
        (row.get::<i32, _>("current_day"), row.get::<String, _>("last_attendance"))
    } else {
        // Create attendance record
        sqlx::query(
            "INSERT INTO attendance (account_id, current_day, total_days, last_attendance) VALUES (?, 0, 0, '')"
        )
        .bind(session.account_id)
        .execute(&state.db)
        .await?;
        (0, String::new())
    };

    // Check if already received today
    if last_attendance == today {
        return Ok(Json(ReceiveAttendanceRewardResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Already received
            day: current_day,
            reward_gold: 0,
            reward_gem: 0,
            reward_items: vec![],
        }));
    }

    // Calculate next day (wrap around after 28)
    let next_day = if current_day >= 28 { 1 } else { current_day + 1 };

    // Calculate reward
    let (reward_gold, reward_gem) = match next_day {
        7 | 14 | 21 | 28 => (0, 100),
        _ => (next_day as i64 * 1000, 0),
    };

    // Update attendance
    sqlx::query(
        "UPDATE attendance SET current_day = ?, total_days = total_days + 1, last_attendance = ? WHERE account_id = ?"
    )
    .bind(next_day)
    .bind(&today)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    // Add rewards
    if reward_gold > 0 || reward_gem > 0 {
        sqlx::query("UPDATE user_info SET gold = gold + ?, gem = gem + ? WHERE account_id = ?")
            .bind(reward_gold)
            .bind(reward_gem)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    Ok(Json(ReceiveAttendanceRewardResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        day: next_day,
        reward_gold,
        reward_gem,
        reward_items: vec![],
    }))
}
