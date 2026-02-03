use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::{BaseResultType, GuildInfo},
    state::AppState,
};

/// Guild member info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct GuildMemberInfo {
    pub account_id: i64,
    pub nickname: String,
    pub level: i32,
    pub role: i32, // 0 = member, 1 = officer, 2 = master
    pub contribution: i64,
    pub last_login: String,
    pub is_online: bool,
}

/// Get guild info request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetGuildInfoRequest {
    pub session_id: Option<String>,
}

/// Get guild info response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetGuildInfoResponse {
    pub base_result: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub guild_info: Option<GuildInfo>,
    pub members: Vec<GuildMemberInfo>,
    pub is_in_guild: bool,
}

/// Handle get guild info request
pub async fn get_guild_info(
    State(state): State<AppState>,
    Form(req): Form<GetGuildInfoRequest>,
) -> Result<Json<GetGuildInfoResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Check if user is in a guild
    let membership = sqlx::query(
        "SELECT guild_id, role FROM guild_members WHERE account_id = ?"
    )
    .bind(session.account_id)
    .fetch_optional(&state.db)
    .await?;

    if let Some(member_row) = membership {
        let guild_id: i64 = member_row.get("guild_id");
        
        // Get guild info
        let guild_row = sqlx::query(
            "SELECT guild_id, name, level, exp, notice, max_members, created_at FROM guilds WHERE guild_id = ?"
        )
        .bind(guild_id)
        .fetch_optional(&state.db)
        .await?;

        let guild_info = guild_row.map(|row| GuildInfo {
            guild_id: row.get("guild_id"),
            name: row.get("name"),
            level: row.get("level"),
            exp: row.get("exp"),
            notice: row.get::<Option<String>, _>("notice").unwrap_or_default(),
            max_members: row.get("max_members"),
            member_count: 0, // Will be set below
            master_account_id: 0, // TODO: Query from members
            master_nick: String::new(),
        });

        // Get guild members
        let member_rows = sqlx::query(
            r#"
            SELECT gm.account_id, gm.role, gm.contribution, u.nickname, u.level
            FROM guild_members gm
            JOIN user_info u ON gm.account_id = u.account_id
            WHERE gm.guild_id = ?
            ORDER BY gm.role DESC, gm.contribution DESC
            "#
        )
        .bind(guild_id)
        .fetch_all(&state.db)
        .await?;

        let members: Vec<GuildMemberInfo> = member_rows.iter().map(|row| GuildMemberInfo {
            account_id: row.get("account_id"),
            nickname: row.get("nickname"),
            level: row.get("level"),
            role: row.get("role"),
            contribution: row.get("contribution"),
            last_login: String::new(),
            is_online: state.sessions.iter().any(|s| s.value().account_id == row.get::<i64, _>("account_id")),
        }).collect();

        let guild_info = guild_info.map(|mut g| {
            g.member_count = members.len() as i32;
            g
        });

        Ok(Json(GetGuildInfoResponse {
            base_result: BaseResultType::Success as i32,
            guild_info,
            members,
            is_in_guild: true,
        }))
    } else {
        Ok(Json(GetGuildInfoResponse {
            base_result: BaseResultType::Success as i32,
            guild_info: None,
            members: vec![],
            is_in_guild: false,
        }))
    }
}

/// Create guild request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct CreateGuildRequest {
    pub session_id: Option<String>,
    pub guild_name: Option<String>,
}

/// Create guild response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct CreateGuildResponse {
    pub base_result: i32,
    pub result: i32,
    pub guild_id: i64,
}

/// Handle create guild request
pub async fn create_guild(
    State(state): State<AppState>,
    Form(req): Form<CreateGuildRequest>,
) -> Result<Json<CreateGuildResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let guild_name = req.guild_name.ok_or_else(|| ServerError::InvalidRequest("Missing guild_name".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Check if already in a guild
    let existing_membership = sqlx::query("SELECT 1 FROM guild_members WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_optional(&state.db)
        .await?;

    if existing_membership.is_some() {
        return Ok(Json(CreateGuildResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Already in guild
            guild_id: 0,
        }));
    }

    // Check if guild name is taken
    let existing_guild = sqlx::query("SELECT 1 FROM guilds WHERE name = ?")
        .bind(&guild_name)
        .fetch_optional(&state.db)
        .await?;

    if existing_guild.is_some() {
        return Ok(Json(CreateGuildResponse {
            base_result: BaseResultType::Success as i32,
            result: 2, // Name taken
            guild_id: 0,
        }));
    }

    // Create guild
    let result = sqlx::query(
        "INSERT INTO guilds (name, master_account_id, level, exp, max_members) VALUES (?, ?, 1, 0, 30)"
    )
    .bind(&guild_name)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    let guild_id = result.last_insert_rowid();

    // Add creator as master
    sqlx::query(
        "INSERT INTO guild_members (guild_id, account_id, role, contribution) VALUES (?, ?, 2, 0)"
    )
    .bind(guild_id)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    Ok(Json(CreateGuildResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        guild_id,
    }))
}

/// Join guild request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct JoinGuildRequest {
    pub session_id: Option<String>,
    pub guild_id: Option<i64>,
}

/// Join guild response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct JoinGuildResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle join guild request
pub async fn join_guild(
    State(state): State<AppState>,
    Form(req): Form<JoinGuildRequest>,
) -> Result<Json<JoinGuildResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let guild_id = req.guild_id.ok_or_else(|| ServerError::InvalidRequest("Missing guild_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Check if already in a guild
    let existing_membership = sqlx::query("SELECT 1 FROM guild_members WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_optional(&state.db)
        .await?;

    if existing_membership.is_some() {
        return Ok(Json(JoinGuildResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Already in guild
        }));
    }

    // Check guild exists and has space
    let guild = sqlx::query(
        "SELECT max_members, (SELECT COUNT(*) FROM guild_members WHERE guild_id = g.guild_id) as member_count FROM guilds g WHERE guild_id = ?"
    )
    .bind(guild_id)
    .fetch_optional(&state.db)
    .await?;

    let guild = guild.ok_or_else(|| ServerError::NotFound("Guild not found".to_string()))?;
    let max_members: i32 = guild.get("max_members");
    let member_count: i32 = guild.get("member_count");

    if member_count >= max_members {
        return Ok(Json(JoinGuildResponse {
            base_result: BaseResultType::Success as i32,
            result: 2, // Guild full
        }));
    }

    // Join guild as member
    sqlx::query(
        "INSERT INTO guild_members (guild_id, account_id, role, contribution) VALUES (?, ?, 0, 0)"
    )
    .bind(guild_id)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    Ok(Json(JoinGuildResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// Leave guild request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct LeaveGuildRequest {
    pub session_id: Option<String>,
}

/// Leave guild response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct LeaveGuildResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle leave guild request
pub async fn leave_guild(
    State(state): State<AppState>,
    Form(req): Form<LeaveGuildRequest>,
) -> Result<Json<LeaveGuildResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Check if master - masters can't leave without transferring
    let membership = sqlx::query("SELECT guild_id, role FROM guild_members WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_optional(&state.db)
        .await?;

    let membership = membership.ok_or_else(|| ServerError::NotFound("Not in a guild".to_string()))?;
    let role: i32 = membership.get("role");

    if role == 2 {
        // Guild master - check if only member
        let guild_id: i64 = membership.get("guild_id");
        let member_count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM guild_members WHERE guild_id = ?")
            .bind(guild_id)
            .fetch_one(&state.db)
            .await?;

        if member_count > 1 {
            return Ok(Json(LeaveGuildResponse {
                base_result: BaseResultType::Success as i32,
                result: 1, // Master must transfer ownership first
            }));
        }

        // Last member, delete guild
        sqlx::query("DELETE FROM guilds WHERE guild_id = ?")
            .bind(guild_id)
            .execute(&state.db)
            .await?;
    }

    // Leave guild
    sqlx::query("DELETE FROM guild_members WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(LeaveGuildResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// Search guilds request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct SearchGuildsRequest {
    pub session_id: Option<String>,
    pub search_text: Option<String>,
}

/// Search guilds response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct SearchGuildsResponse {
    pub base_result: i32,
    pub guilds: Vec<GuildInfo>,
}

/// Handle search guilds request
pub async fn search_guilds(
    State(state): State<AppState>,
    Form(req): Form<SearchGuildsRequest>,
) -> Result<Json<SearchGuildsResponse>> {
    let _session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let search_text = req.search_text.unwrap_or_default();

    let guilds = if search_text.is_empty() {
        // Return top guilds
        sqlx::query(
            r#"
            SELECT g.guild_id, g.name, g.level, g.exp, g.notice, g.max_members,
                   (SELECT COUNT(*) FROM guild_members WHERE guild_id = g.guild_id) as member_count
            FROM guilds g
            ORDER BY g.level DESC, g.exp DESC
            LIMIT 20
            "#
        )
        .fetch_all(&state.db)
        .await?
    } else {
        // Search by name
        sqlx::query(
            r#"
            SELECT g.guild_id, g.name, g.level, g.exp, g.notice, g.max_members,
                   (SELECT COUNT(*) FROM guild_members WHERE guild_id = g.guild_id) as member_count
            FROM guilds g
            WHERE g.name LIKE ?
            ORDER BY g.level DESC
            LIMIT 20
            "#
        )
        .bind(format!("%{}%", search_text))
        .fetch_all(&state.db)
        .await?
    };

    let guilds: Vec<GuildInfo> = guilds.iter().map(|row| GuildInfo {
        guild_id: row.get("guild_id"),
        name: row.get("name"),
        level: row.get("level"),
        exp: row.get("exp"),
        notice: row.get::<Option<String>, _>("notice").unwrap_or_default(),
        max_members: row.get("max_members"),
        member_count: row.get("member_count"),
        master_account_id: 0, // TODO: Query master
        master_nick: String::new(),
    }).collect();

    Ok(Json(SearchGuildsResponse {
        base_result: BaseResultType::Success as i32,
        guilds,
    }))
}
