//! Persistent battle entries, rewards, dungeon modes, rooms, and asynchronous runs.
use crate::api::{
    heroes as hero,
    inventory::item::{self, n, rule},
    system::request::Request,
    tutorial::{self, Rewards},
};
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{
    body::Bytes,
    extract::{OriginalUri, State},
    Json,
};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection, SqlitePool};
use std::collections::BTreeSet;
mod campaign;
pub mod campaign_handlers;
mod dispatch;
mod dungeons;
mod restrictions;
mod rooms;
mod seasons;
mod special;
#[cfg(test)]
mod tests;

pub(crate) async fn migrate(db: &SqlitePool) -> Result<()> {
    for query in [
        "CREATE TABLE IF NOT EXISTS battle_state(account INTEGER NOT NULL,kind TEXT NOT NULL,idx INTEGER NOT NULL,data TEXT NOT NULL,PRIMARY KEY(account,kind,idx))",
        "CREATE TABLE IF NOT EXISTS battle_runs(account INTEGER PRIMARY KEY,run_id TEXT NOT NULL,started INTEGER NOT NULL,completed INTEGER NOT NULL DEFAULT 0,entry TEXT NOT NULL,begin_response TEXT NOT NULL)",
        "CREATE TABLE IF NOT EXISTS battle_rooms(id INTEGER PRIMARY KEY AUTOINCREMENT,family TEXT NOT NULL,master INTEGER NOT NULL,data TEXT NOT NULL,updated INTEGER NOT NULL)",
        "CREATE TABLE IF NOT EXISTS battle_room_members(account INTEGER PRIMARY KEY,room INTEGER NOT NULL,joined INTEGER NOT NULL,updated INTEGER NOT NULL,FOREIGN KEY(room) REFERENCES battle_rooms(id) ON DELETE CASCADE)",
        "CREATE TABLE IF NOT EXISTS battle_scores(family TEXT NOT NULL,boss INTEGER NOT NULL,season INTEGER NOT NULL,account INTEGER NOT NULL,day TEXT NOT NULL,score INTEGER NOT NULL,battle_time INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(family,boss,season,account,day))",
        "CREATE TABLE IF NOT EXISTS battle_reward_claims(account INTEGER NOT NULL,kind TEXT NOT NULL,idx INTEGER NOT NULL,period TEXT NOT NULL,PRIMARY KEY(account,kind,idx,period))",
        "CREATE TABLE IF NOT EXISTS battle_currencies(account INTEGER NOT NULL,kind TEXT NOT NULL,value INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(account,kind))",
    ] { sqlx::query(query).execute(db).await?; }
    Ok(())
}
pub fn routes(tables: &crate::tables::BattleTable) -> axum::Router<AppState> {
    let mut r = axum::Router::new();
    for path in tables.contracts.keys() {
        if matches!(
            path.as_str(),
            "campaign/begin_campaign"
                | "campaign/end_campaign"
                | "campaign/visit_dungeon"
                | "campaign/complete_scenario_dungeon"
                | "campaign/reward_clear_chapter"
                | "match/get_season_info"
        ) {
            continue;
        }
        r = r.route(&format!("/{path}"), axum::routing::post(handle));
    }
    r
}
pub async fn handle(
    State(s): State<AppState>,
    OriginalUri(uri): OriginalUri,
    body: Bytes,
) -> Result<Json<Value>> {
    execute_request(&s, uri.path().trim_start_matches('/'), body)
        .await
        .map(Json)
}
pub(crate) async fn execute_request(s: &AppState, path: &str, body: Bytes) -> Result<Value> {
    let mut req = Request::parse(&body)?;
    let a = req.account(s)?;
    if let Some(fields) = s
        .tables
        .battle
        .contracts
        .get(path)
        .and_then(|c| c["Request"].as_object())
    {
        for (key, typ) in fields {
            if let Some(values) = typ.as_str().and_then(|t| s.tables.battle.enums.get(t)) {
                if let Some(raw) = req.0.get_mut(key) {
                    if let Some(value) = values.get(raw) {
                        *raw = value.to_string();
                    }
                }
            }
        }
    }
    // Awakening trials have their own transactional proof and reward state.
    if path == "campaign/begin_campaign" || path == "campaign/end_campaign" {
        let completed = if path.ends_with("end_campaign") {
            Some(boolean(&req, "Completed", false)?)
        } else {
            None
        };
        if let Some(trial) = hero::trial(
            s,
            a,
            int(&req, "ChapterIndex")? as i32,
            int(&req, "DungeonIndex")? as i32,
            completed,
        )
        .await?
        {
            let mut out = response(s, path);
            if completed.is_some() {
                out["HeroInfos"] = trial["HeroInfos"].clone();
                out["ItemResults"] = trial["ItemResults"].clone();
            }
            return Ok(out);
        }
    }
    let mut tx = s.db.begin().await?;
    item::init(&mut tx, s, a).await?;
    let action = path.rsplit('/').next().unwrap_or("");
    let result = match path.split('/').next().unwrap_or("") {
        "match" => seasons::match_season(s, &req),
        "dispatch" => dispatch::execute(&mut tx, s, a, &req, action).await,
        "raid" | "party_dungeon" if action.contains("room") => {
            rooms::execute(&mut tx, s, a, &req, path).await
        }
        "world_boss" | "event_world_boss" => seasons::execute(&mut tx, s, a, &req, path).await,
        "raid" if action.contains("rank") => seasons::execute(&mut tx, s, a, &req, path).await,
        "eclipse" | "ordeal_arena" => special::execute(&mut tx, s, a, &req, path).await,
        _ => match action {
            "begin_campaign" => campaign::begin(&mut tx, s, a, &req).await,
            "end_campaign" => campaign::end(&mut tx, s, a, &req).await,
            "visit_dungeon" | "complete_scenario_dungeon" => {
                campaign::visit(&mut tx, s, a, &req, action).await
            }
            "sweep_dungeon" => dispatch::sweep(&mut tx, s, a, &req).await,
            "get_selected_reward" => campaign::select_reward(&mut tx, s, a, &req).await,
            "get_multiplay_reward" | "apply_for_multiplay_reward" | "abandon_multiplay_reward" => {
                rooms::reward(&mut tx, s, a, &req, action).await
            }
            _ => dungeons::execute(&mut tx, s, a, &req, path).await,
        },
    };
    match result {
        Ok(v) => {
            tx.commit().await?;
            let mut out = response(s, path);
            merge(&mut out, v);
            Ok(out)
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            let c = s
                .tables
                .battle
                .contracts
                .get(path)
                .map(|v| &v["Results"])
                .unwrap_or(&Value::Null);
            let code = if c.as_array().is_some_and(|v| v.contains(&json!(code))) {
                code
            } else {
                "Fail".into()
            };
            Ok(json!({"BaseResult":"Success","Result":code}))
        }
        Err(e) => Err(e),
    }
}
fn response(s: &AppState, path: &str) -> Value {
    let mut out = item::success();
    if let Some(fields) = s
        .tables
        .battle
        .contracts
        .get(path)
        .and_then(|v| v["Response"].as_object())
    {
        for (key, typ) in fields {
            let t = typ.as_str().unwrap_or("");
            out[key] = if t.ends_with("[]") {
                json!([])
            } else if t == "bool" {
                json!(false)
            } else if matches!(t, "int" | "long" | "byte") {
                json!(0)
            } else {
                Value::Null
            };
        }
    }
    out
}
pub(crate) fn match_calendar(s: &AppState) -> Result<Value> {
    Ok(seasons::match_season(s, &Request::parse(b"ArenaType=5")?)?["SeasonData"].clone())
}
fn merge(out: &mut Value, other: Value) {
    if let Some(o) = other.as_object() {
        for (k, v) in o {
            out[k] = v.clone();
        }
    }
}
fn row<'a>(s: &'a AppState, table: &str, fields: &[(&str, i64)]) -> Result<&'a Value> {
    s.tables
        .battle
        .find(table, fields)
        .ok_or_else(|| rule("DungeonNotFound"))
}
fn int(r: &Request, key: &str) -> Result<i64> {
    let v = r.number(key, 0)?;
    if !(0..=i32::MAX as i64).contains(&v) {
        return Err(rule("InvalidRequest"));
    }
    Ok(v)
}
fn boolean(r: &Request, key: &str, default: bool) -> Result<bool> {
    match r.text(key) {
        "" => Ok(default),
        "true" | "True" | "1" => Ok(true),
        "false" | "False" | "0" => Ok(false),
        _ => Err(rule("InvalidRequest")),
    }
}
fn ids(r: &Request, key: &str, max: usize) -> Result<Vec<i64>> {
    if r.text(key).is_empty() {
        return Ok(vec![]);
    }
    let ids: Vec<i64> = read_json(r.text(key)).map_err(|_| rule("InvalidHero"))?;
    if ids.len() > max
        || ids.iter().any(|v| *v <= 0 || *v > i32::MAX as i64)
        || ids.iter().collect::<BTreeSet<_>>().len() != ids.len()
    {
        return Err(rule("DuplicatedHero"));
    }
    Ok(ids)
}
fn settings(s: &AppState, key: &str, default: i64) -> i64 {
    s.tables.battle.rules[key].as_i64().unwrap_or(default)
}
fn now() -> i64 {
    chrono::Utc::now().timestamp()
}
fn time(t: i64) -> String {
    chrono::DateTime::from_timestamp(t, 0)
        .unwrap()
        .format("%Y-%m-%d %H:%M:%S")
        .to_string()
}
fn day() -> String {
    chrono::Utc::now().format("%Y-%m-%d").to_string()
}
pub(crate) async fn get(db: &mut SqliteConnection, a: i64, kind: &str, idx: i64) -> Result<Value> {
    let data: Option<String> =
        sqlx::query_scalar("SELECT data FROM battle_state WHERE account=? AND kind=? AND idx=?")
            .bind(a)
            .bind(kind)
            .bind(idx)
            .fetch_optional(db)
            .await?;
    Ok(data
        .map(|v| read_json(&v))
        .transpose()?
        .unwrap_or(Value::Null))
}
pub(crate) async fn put(db: &mut SqliteConnection, a: i64, kind: &str, idx: i64, v: &Value) -> Result<()> {
    sqlx::query("INSERT INTO battle_state(account,kind,idx,data) VALUES(?,?,?,?) ON CONFLICT(account,kind,idx) DO UPDATE SET data=excluded.data").bind(a).bind(kind).bind(idx).bind(v.to_string()).execute(db).await?;
    Ok(())
}
async fn list(db: &mut SqliteConnection, a: i64, kind: &str) -> Result<Vec<Value>> {
    let rows: Vec<String> =
        sqlx::query_scalar("SELECT data FROM battle_state WHERE account=? AND kind=? ORDER BY idx")
            .bind(a)
            .bind(kind)
            .fetch_all(db)
            .await?;
    rows.iter()
        .map(|v| read_json(v).map_err(Into::into))
        .collect()
}
async fn owned(db: &mut SqliteConnection, a: i64, heroes: &[i64]) -> Result<()> {
    for h in heroes {
        hero::info(db, a, *h as i32).await?;
    }
    Ok(())
}
async fn claim(
    db: &mut SqliteConnection,
    a: i64,
    kind: &str,
    idx: i64,
    period: &str,
) -> Result<()> {
    let r = sqlx::query(
        "INSERT OR IGNORE INTO battle_reward_claims(account,kind,idx,period) VALUES(?,?,?,?)",
    )
    .bind(a)
    .bind(kind)
    .bind(idx)
    .bind(period)
    .execute(db)
    .await?;
    if r.rows_affected() != 1 {
        return Err(rule("AlreadyCompleted"));
    }
    Ok(())
}
async fn reward_index(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    index: i64,
    r: &mut Rewards,
) -> Result<()> {
    if index > 0 {
        item::reward(db, s, a, index as i32, r).await?;
    }
    Ok(())
}
async fn rewards(db: &mut SqliteConnection, s: &AppState, a: i64, r: Rewards) -> Result<Value> {
    let mut out = item::reward_response(db, s, a, r).await?;
    out["EquipItemInfos"] = out["EquipItemResults"].clone();
    out["HeroExpResults"] = json!([]);
    Ok(out)
}
pub(crate) async fn login(s: &AppState, a: i64) -> Result<Value> {
    let mut tx = s.db.begin().await?;
    item::init(&mut tx, s, a).await?;
    seasons::settle(&mut tx, s, a).await?;
    let towers = dungeons::tower_list(&mut tx, s, a).await?;
    let under = list(&mut tx, a, "under_prison").await?;
    let raid = list(&mut tx, a, "raid").await?;
    let god = list(&mut tx, a, "godking").await?;
    let mut out = json!({"Towers":towers,"UnderPrisonInfos":under,"RaidInfos":raid,"GodkingTrialDungeonInfos":god});
    let mut progress = vec![];
    for p in list(&mut tx, a, "dungeon").await? {
        progress.push(
            campaign::progress(&mut tx, s, a, n(&p, "ChapterIndex"), n(&p, "DungeonIndex")).await?,
        );
    }
    out["DungeonInfos"] = json!(progress);
    out["BattleKeyResults"] = dungeons::key_snapshot(&mut tx, s, a).await?;
    out["DispatchBattleInfos"] = json!(dispatch::snapshot(&mut tx, a).await?);
    out["TopClearDungeonInfos"] = json!(list(&mut tx, a, "top_clear").await?);
    out["HideoutDungeons"] = json!(list(&mut tx, a, "hideout").await?);
    out["ConquestDungeons"] = json!(list(&mut tx, a, "conquest").await?);
    if !s.tables.battle.rows("ShakmehBoss").is_empty() {
        out["ShakemehPassiveInfos"] = dungeons::shakmeh_passives(&mut tx, s, a).await?;
    }
    let currencies = sqlx::query("SELECT kind,value FROM battle_currencies WHERE account=?")
        .bind(a)
        .fetch_all(&mut *tx)
        .await?;
    out["PlayerCurrencyInfos"] = json!(currencies.iter().map(|r|json!({"CurrencyType":r.get::<String,_>("kind"),"Amount":r.get::<i64,_>("value"),"DailyAmount":0,"DailyResetTime":null,"NextResetTime":-1})).collect::<Vec<_>>());
    tx.commit().await?;
    Ok(out)
}

pub(crate) async fn reset(db: &mut SqliteConnection, a: i64, keep_heroes: bool) -> Result<()> {
    let membership: bool =
        sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_room_members WHERE account=?)")
            .bind(a)
            .fetch_one(&mut *db)
            .await?;
    if membership {
        rooms::remove_account(db, a).await?;
    }
    sqlx::query("DELETE FROM battle_runs WHERE account=?")
        .bind(a)
        .execute(&mut *db)
        .await?;
    // Keep earned leaderboard rewards/claims; resetting an account must not make them claimable twice.
    sqlx::query(
        "DELETE FROM battle_state WHERE account=? AND (?=0 OR kind NOT IN ('eclipse_deck'))",
    )
    .bind(a)
    .bind(if keep_heroes { 1 } else { 0 })
    .execute(db)
    .await?;
    Ok(())
}

fn read_json<T: serde::de::DeserializeOwned>(text: &str) -> Result<T> {
    serde_json::from_str(text)
        .map_err(|e| ServerError::Internal(format!("Invalid stored battle JSON: {e}")))
}
fn read_value<T: serde::de::DeserializeOwned>(value: Value) -> Result<T> {
    serde_json::from_value(value)
        .map_err(|e| ServerError::Internal(format!("Invalid stored battle value: {e}")))
}

pub(crate) use dungeons::charge as charge_key;

pub(crate) use campaign::end_authoritative;
