//! Name tags (SPRK): Discord-style badges, an optional small icon and a short
//! text in a coloured pill, shown after a player's name on every screen and
//! in chat. Only GMs manage them (`gm.py name-tag ...`, admin listener).
//!
//! Every query that shows a name selects `sprk_display_nick(account_id, nick)`
//! (migration 0018), which prefixes tagged players' nicks with an invisible
//! marker: `MARK_START <tag id> MARK_END`. The client (`SprkNameTag`) strips
//! the marker from labels and draws the badge, fetching each tag's look once
//! from `/sprk/name_tag/get`. Player input never carries markers
//! ([`strip_markers`] runs on every parsed request), so tags cannot be faked.
use axum::{body::Bytes, extract::State, Json};
use base64::{engine::general_purpose::STANDARD, Engine};
use serde_json::{json, Value};
use crate::{
    api::system::request::Request,
    db::RowExt,
    error::{Result, ServerError},
    state::AppState,
};

pub const MARK_START: char = '\u{E0A0}';
pub const MARK_END: char = '\u{E0A1}';

/// Label length: short like Discord's tags, so names keep their room.
pub const MAX_LABEL: usize = 8;
/// Stored icons are small: gm.py shrinks uploads to 64 px; larger ones are refused.
pub const MAX_ICON_SIDE: u32 = 128;
pub const MIN_ICON_SIDE: u32 = 8;
pub const MAX_ICON_BYTES: usize = 64 * 1024;
const MAX_LIST: usize = 100;

/// Removes name-tag markers from player-supplied text.
pub fn strip_markers(text: String) -> String {
    if text.contains([MARK_START, MARK_END]) {
        text.replace([MARK_START, MARK_END], "")
    } else {
        text
    }
}

fn tag_json(row: &sqlx::postgres::PgRow, with_icon: bool) -> Value {
    let icon: Option<Vec<u8>> = row.get("icon");
    let mut tag = json!({
        "TagId": row.get::<i64, _>("tag_id"),
        "Label": row.get::<String, _>("label"),
        "TextColor": row.get::<String, _>("text_color"),
        "BackgroundColor": row.get::<String, _>("background_color"),
        "Version": row.get::<String, _>("updated_at"),
    });
    if with_icon {
        tag["Icon"] = json!(icon.map(|png| STANDARD.encode(png)).unwrap_or_default());
    } else {
        tag["HasIcon"] = json!(icon.is_some());
    }
    tag
}

/// Client: the look of up to 100 `TagIds` (JSON array). Unknown (deleted)
/// tags are absent from `Tags`; the client then shows the plain name.
/// Every reply carries `"SprkNameTag":1` so an older server's generic
/// fallback (which answers unknown routes with Success) is never mistaken
/// for an answer.
pub async fn get(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    req.account(&state)?;
    let ids = req.ids_with_limit("TagIds", MAX_LIST)?;
    let rows = if ids.is_empty() { Vec::new() } else {
        crate::db::query("SELECT * FROM name_tags WHERE tag_id IN (SELECT jsonb_array_elements_text($1::jsonb)::bigint)")
            .bind(serde_json::to_string(&ids).unwrap_or_default()).fetch_all(&state.db).await?
    };
    let tags: serde_json::Map<String, Value> = rows.iter()
        .map(|row| (row.get::<i64, _>("tag_id").to_string(), tag_json(row, true))).collect();
    Ok(Json(json!({"BaseResult":"Success","SprkNameTag":1,"Result":"Success","Tags":tags})))
}

// ---------------------------------------------------------------------------
// GM (admin listener only; see api::system::cheat::admin_routes)
// ---------------------------------------------------------------------------

fn invalid(message: impl Into<String>) -> ServerError {
    ServerError::InvalidRequest(message.into())
}

fn handle(req: &Request) -> Result<String> {
    let name = req.text("Name").trim().to_ascii_lowercase();
    if name.is_empty() || name.len() > 32 || !name.bytes().all(|b| b.is_ascii_lowercase() || b.is_ascii_digit() || b == b'-' || b == b'_') {
        return Err(invalid("Name must be 1-32 characters: a-z, 0-9, - or _"));
    }
    Ok(name)
}

/// Text in the pill. NGUI labels read `[...]` as markup, so brackets are refused.
fn label(text: &str) -> Result<String> {
    let text = text.trim();
    let length = text.chars().count();
    if length == 0 || length > MAX_LABEL
        || text.chars().any(|c| c.is_control() || matches!(c, '[' | ']' | MARK_START | MARK_END))
    {
        return Err(invalid(format!("Label must be 1-{MAX_LABEL} characters, without [ ] or control characters")));
    }
    Ok(text.to_string())
}

fn color(text: &str, field: &str) -> Result<String> {
    let text = text.trim().trim_start_matches('#');
    if text.len() != 6 || !text.bytes().all(|b| b.is_ascii_hexdigit()) {
        return Err(invalid(format!("{field} must be a RRGGBB hex colour")));
    }
    Ok(text.to_ascii_uppercase())
}

fn icon(base64: &str) -> Result<Vec<u8>> {
    let png = STANDARD.decode(base64.trim()).map_err(|_| invalid("Icon must be base64 PNG"))?;
    match crate::api::account::profile_image::png_size(&png) {
        Some((w, h)) if png.len() <= MAX_ICON_BYTES
            && (MIN_ICON_SIDE..=MAX_ICON_SIDE).contains(&w) && (MIN_ICON_SIDE..=MAX_ICON_SIDE).contains(&h) => Ok(png),
        _ => Err(invalid(format!(
            "Icon must be a PNG of {MIN_ICON_SIDE}-{MAX_ICON_SIDE} px per side and at most {} KiB (gm.py shrinks images when Pillow is installed)",
            MAX_ICON_BYTES / 1024))),
    }
}

async fn tag_by_name(state: &AppState, name: &str) -> Result<sqlx::postgres::PgRow> {
    crate::db::query("SELECT t.*,(SELECT count(*) FROM account_name_tags a WHERE a.tag_id=t.tag_id) AS members FROM name_tags t WHERE t.name=$1")
        .bind(name).fetch_optional(&state.db).await?
        .ok_or_else(|| ServerError::NotFound(format!("No name tag named {name}")))
}

fn gm_json(row: &sqlx::postgres::PgRow) -> Value {
    let mut tag = tag_json(row, false);
    tag["Name"] = json!(row.get::<String, _>("name"));
    tag["Members"] = json!(row.get::<i64, _>("members"));
    tag
}

/// `Action`: list | create | update | delete | assign | unassign | members.
pub async fn gm(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let present = |key: &str| req.0.contains_key(key);
    match req.text("Action") {
        "list" => {
            let rows = crate::db::query("SELECT t.*,(SELECT count(*) FROM account_name_tags a WHERE a.tag_id=t.tag_id) AS members FROM name_tags t ORDER BY t.name")
                .fetch_all(&state.db).await?;
            Ok(Json(json!({"Tags": rows.iter().map(gm_json).collect::<Vec<_>>()})))
        }
        "create" => {
            let name = handle(&req)?;
            let text = label(req.text("Label"))?;
            let text_color = if present("TextColor") { color(req.text("TextColor"), "TextColor")? } else { "FFFFFF".into() };
            let background = if present("BackgroundColor") { color(req.text("BackgroundColor"), "BackgroundColor")? } else { "2B2D31".into() };
            let png = if present("Icon") && !req.text("Icon").is_empty() { Some(icon(req.text("Icon"))?) } else { None };
            let created: Option<i64> = crate::db::query_scalar("INSERT INTO name_tags(name,label,text_color,background_color,icon) VALUES($1,$2,$3,$4,$5) ON CONFLICT(name) DO NOTHING RETURNING tag_id")
                .bind(&name).bind(&text).bind(&text_color).bind(&background).bind(png).fetch_optional(&state.db).await?;
            if created.is_none() {
                return Err(invalid(format!("A name tag named {name} already exists")));
            }
            Ok(Json(json!({"Tag": gm_json(&tag_by_name(&state, &name).await?)})))
        }
        "update" => {
            let name = handle(&req)?;
            let old = tag_by_name(&state, &name).await?;
            let text = if present("Label") { label(req.text("Label"))? } else { old.get("label") };
            let text_color = if present("TextColor") { color(req.text("TextColor"), "TextColor")? } else { old.get("text_color") };
            let background = if present("BackgroundColor") { color(req.text("BackgroundColor"), "BackgroundColor")? } else { old.get("background_color") };
            let png: Option<Vec<u8>> = if req.text("RemoveIcon") == "true" { None }
                else if present("Icon") { Some(icon(req.text("Icon"))?) } else { old.get("icon") };
            crate::db::query("UPDATE name_tags SET label=$2,text_color=$3,background_color=$4,icon=$5,updated_at=sprk_now() WHERE name=$1")
                .bind(&name).bind(&text).bind(&text_color).bind(&background).bind(png).execute(&state.db).await?;
            Ok(Json(json!({"Tag": gm_json(&tag_by_name(&state, &name).await?)})))
        }
        "delete" => {
            let name = handle(&req)?;
            let row = tag_by_name(&state, &name).await?;
            // Assignments go with it (ON DELETE CASCADE).
            crate::db::query("DELETE FROM name_tags WHERE name=$1").bind(&name).execute(&state.db).await?;
            Ok(Json(json!({"Deleted": name, "Unassigned": row.get::<i64, _>("members")})))
        }
        "assign" => {
            let name = handle(&req)?;
            let account = req.number("AccountId", 0)?;
            let tag: i64 = tag_by_name(&state, &name).await?.get("tag_id");
            let exists: bool = crate::db::query_scalar("SELECT EXISTS(SELECT 1 FROM accounts WHERE account_id=$1)")
                .bind(account).fetch_one(&state.db).await?;
            if !exists {
                return Err(ServerError::NotFound("Account not found".into()));
            }
            // One tag per player: assigning replaces the previous one.
            crate::db::query("INSERT INTO account_name_tags(account_id,tag_id) VALUES($1,$2) ON CONFLICT(account_id) DO UPDATE SET tag_id=EXCLUDED.tag_id,assigned_at=sprk_now()")
                .bind(account).bind(tag).execute(&state.db).await?;
            Ok(Json(json!({"AccountId": account, "Tag": name})))
        }
        "unassign" => {
            let account = req.number("AccountId", 0)?;
            let removed: Option<String> = crate::db::query_scalar("DELETE FROM account_name_tags a USING name_tags t WHERE a.account_id=$1 AND t.tag_id=a.tag_id RETURNING t.name")
                .bind(account).fetch_optional(&state.db).await?;
            Ok(Json(json!({"AccountId": account, "Removed": removed})))
        }
        "members" => {
            let name = handle(&req)?;
            let tag: i64 = tag_by_name(&state, &name).await?.get("tag_id");
            let rows = crate::db::query("SELECT a.account_id,a.nick,x.assigned_at FROM account_name_tags x JOIN accounts a USING(account_id) WHERE x.tag_id=$1 ORDER BY a.account_id")
                .bind(tag).fetch_all(&state.db).await?;
            Ok(Json(json!({"Tag": name, "Members": rows.iter().map(|r| json!({
                "AccountId": r.get::<i64, _>("account_id"),
                "Nickname": r.get::<Option<String>, _>("nick"),
                "AssignedAt": r.get::<String, _>("assigned_at"),
            })).collect::<Vec<_>>()})))
        }
        _ => Err(invalid("Action must be list, create, update, delete, assign, unassign or members")),
    }
}

#[cfg(test)]
mod tests;
