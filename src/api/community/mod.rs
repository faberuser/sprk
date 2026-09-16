//! Social, arena, and guild services. Mutations share the inventory write transaction.
use crate::api::{
    heroes as hero,
    inventory::item::{self, n, rule},
    system::request::Request,
    tutorial::Rewards,
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
mod arena;
pub mod chat;
pub mod friend;
mod guild;
pub mod mail;
mod progression;
#[cfg(test)]
mod social_tests;
#[cfg(test)]
mod tests;
mod warfare;
pub(crate) use warfare::{raid_enter, raid_finish, raid_validate};

pub(crate) async fn migrate(db: &SqlitePool) -> Result<()> {
    for q in [
        "CREATE TABLE IF NOT EXISTS community_state(owner INTEGER NOT NULL,kind TEXT NOT NULL,idx INTEGER NOT NULL,data TEXT NOT NULL,PRIMARY KEY(owner,kind,idx))",
        "CREATE TABLE IF NOT EXISTS community_claims(account INTEGER NOT NULL,kind TEXT NOT NULL,target INTEGER NOT NULL,period TEXT NOT NULL,PRIMARY KEY(account,kind,target,period))",
        "CREATE TABLE IF NOT EXISTS guild_requests(account INTEGER NOT NULL,guild_id INTEGER NOT NULL,created INTEGER NOT NULL,PRIMARY KEY(account,guild_id),FOREIGN KEY(guild_id) REFERENCES guilds(guild_id) ON DELETE CASCADE)",
        "CREATE TABLE IF NOT EXISTS arena_runs(id INTEGER PRIMARY KEY AUTOINCREMENT,account INTEGER NOT NULL,kind INTEGER NOT NULL,started INTEGER NOT NULL,status TEXT NOT NULL,data TEXT NOT NULL)",
        "CREATE UNIQUE INDEX IF NOT EXISTS arena_active_account ON arena_runs(account) WHERE status IN ('waiting','battle')",
        "CREATE TABLE IF NOT EXISTS arena_scores(account INTEGER NOT NULL,kind INTEGER NOT NULL,season INTEGER NOT NULL,score INTEGER NOT NULL,wins INTEGER NOT NULL DEFAULT 0,losses INTEGER NOT NULL DEFAULT 0,data TEXT NOT NULL DEFAULT '{}',PRIMARY KEY(account,kind,season))",
        "CREATE TABLE IF NOT EXISTS guild_battle_scores(guild_id INTEGER NOT NULL,account INTEGER NOT NULL,kind TEXT NOT NULL,season INTEGER NOT NULL,stage INTEGER NOT NULL,score INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(guild_id,account,kind,season,stage))",
        "CREATE TABLE IF NOT EXISTS guild_arena_decks(guild_id INTEGER NOT NULL,account INTEGER NOT NULL,season INTEGER NOT NULL,deck INTEGER NOT NULL,data TEXT NOT NULL,PRIMARY KEY(account,season,deck))",
        "CREATE TABLE IF NOT EXISTS guild_arena_records(id INTEGER PRIMARY KEY AUTOINCREMENT,guild_id INTEGER NOT NULL,enemy_guild INTEGER NOT NULL,account INTEGER NOT NULL,enemy INTEGER NOT NULL,season INTEGER NOT NULL,win INTEGER NOT NULL,data TEXT NOT NULL)",
    ] {sqlx::query(q).execute(db).await?;}
    Ok(())
}
pub fn routes(t: &crate::tables::BattleTable) -> axum::Router<AppState> {
    let mut router = axum::Router::new();
    for p in t.contracts.keys() {
        router = router.route(&format!("/{p}"), axum::routing::post(handle));
    }
    router
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
    let mut r = Request::parse(&body)?;
    let a = r.account(s)?;
    let path = match path {
        "guild/info" => "guild/get_guildbasicinfo",
        "guild/create" => "guild/create_guild",
        "guild/join" => "guild/request_join_guild",
        "guild/leave" => "guild/withdraw_guild",
        "guild/search" => "guild/get_guildlist",
        _ => path,
    };
    for (old, new) in [("GuildName", "name"), ("SearchText", "name")] {
        if !r.0.contains_key(new) {
            if let Some(v) = r.0.get(old).cloned() {
                r.0.insert(new.into(), v);
            }
        }
    }
    if let Some(fields) = s
        .tables
        .arena_guild
        .contracts
        .get(path)
        .and_then(|v| v["Request"].as_object())
    {
        for (key, typ) in fields {
            if let Some(e) = typ.as_str().and_then(|t| s.tables.arena_guild.enums.get(t)) {
                if let Some(raw) = r.0.get_mut(key) {
                    if let Some(v) = e.get(raw) {
                        *raw = v.to_string();
                    }
                }
            }
        }
    }
    let mut tx = s.db.begin().await?;
    item::init(&mut tx, s, a).await?;
    let action = path.rsplit('/').next().unwrap_or("");
    let result = match path.split('/').next().unwrap_or("") {
        "guild" => guild::execute(&mut tx, s, a, &r, action).await,
        "match" | "global_arena" => arena::execute(&mut tx, s, a, &r, action).await,
        _ => warfare::execute(&mut tx, s, a, &r, path).await,
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
            let allowed = s
                .tables
                .arena_guild
                .contracts
                .get(path)
                .and_then(|v| v["Results"].as_array())
                .is_some_and(|v| v.contains(&json!(code)));
            Ok(json!({"BaseResult":"Success","Result":if allowed{code}else{"Fail".into()}}))
        }
        Err(e) => Err(e),
    }
}
fn response(s: &AppState, path: &str) -> Value {
    let mut out = item::success();
    if let Some(fields) = s
        .tables
        .arena_guild
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
            } else if matches!(t, "byte" | "int" | "long") {
                json!(0)
            } else {
                Value::Null
            };
        }
    }
    out
}
fn merge(out: &mut Value, v: Value) {
    if let Some(m) = v.as_object() {
        for (k, v) in m {
            out[k] = v.clone();
        }
    }
}
fn int(r: &Request, k: &str) -> Result<i64> {
    let n = r.number(k, 0)?;
    if n < 0 || n > i32::MAX as i64 {
        return Err(rule("InvalidValue"));
    }
    Ok(n)
}
fn flag(r: &Request, k: &str) -> Result<bool> {
    match r.text(k) {
        "" | "false" | "False" | "0" => Ok(false),
        "true" | "True" | "1" => Ok(true),
        _ => Err(rule("InvalidValue")),
    }
}
fn ids(r: &Request, k: &str, max: usize) -> Result<Vec<i64>> {
    let ids: Vec<i64> = serde_json::from_str(if r.text(k).is_empty() {
        "[]"
    } else {
        r.text(k)
    })
    .map_err(|_| rule("InvalidHero"))?;
    if ids.len() > max
        || ids.iter().any(|v| *v <= 0 || *v > i32::MAX as i64)
        || ids.iter().collect::<BTreeSet<_>>().len() != ids.len()
    {
        return Err(rule("InvalidHero"));
    }
    Ok(ids)
}
fn now() -> i64 {
    chrono::Utc::now().timestamp()
}
fn time(v: i64) -> String {
    chrono::DateTime::from_timestamp(v, 0)
        .unwrap()
        .format("%Y-%m-%d %H:%M:%S")
        .to_string()
}
fn day() -> String {
    chrono::Utc::now().format("%Y-%m-%d").to_string()
}
fn settings(s: &AppState, k: &str, default: i64) -> i64 {
    s.tables.arena_guild.rules[k].as_i64().unwrap_or(default)
}
fn constant(s: &AppState, k: &str, default: i64) -> i64 {
    s.tables.hero_shop.constant(k, default)
}
fn row<'a>(s: &'a AppState, t: &str, f: &[(&str, i64)]) -> Result<&'a Value> {
    s.tables
        .arena_guild
        .find(t, f)
        .ok_or_else(|| rule("InvalidValue"))
}
fn parse<T: serde::de::DeserializeOwned>(v: &str) -> Result<T> {
    serde_json::from_str(v).map_err(|e| ServerError::Internal(e.to_string()))
}
async fn get(db: &mut SqliteConnection, a: i64, kind: &str, idx: i64) -> Result<Value> {
    let v: Option<String> =
        sqlx::query_scalar("SELECT data FROM community_state WHERE owner=? AND kind=? AND idx=?")
            .bind(a)
            .bind(kind)
            .bind(idx)
            .fetch_optional(db)
            .await?;
    v.map(|v| parse(&v))
        .transpose()
        .map(|v| v.unwrap_or(Value::Null))
}
async fn put(db: &mut SqliteConnection, a: i64, kind: &str, idx: i64, v: &Value) -> Result<()> {
    sqlx::query("INSERT INTO community_state(owner,kind,idx,data) VALUES(?,?,?,?) ON CONFLICT(owner,kind,idx) DO UPDATE SET data=excluded.data").bind(a).bind(kind).bind(idx).bind(v.to_string()).execute(db).await?;
    Ok(())
}
async fn claim(
    db: &mut SqliteConnection,
    a: i64,
    k: &str,
    target: i64,
    period: &str,
) -> Result<()> {
    if sqlx::query(
        "INSERT OR IGNORE INTO community_claims(account,kind,target,period) VALUES(?,?,?,?)",
    )
    .bind(a)
    .bind(k)
    .bind(target)
    .bind(period)
    .execute(db)
    .await?
    .rows_affected()
        != 1
    {
        return Err(rule("AlreadyCompleted"));
    }
    Ok(())
}
async fn reward(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    idx: i64,
    rewards: &mut Rewards,
) -> Result<()> {
    if idx > 0 {
        item::reward(db, s, a, idx as i32, rewards).await?;
    }
    Ok(())
}
async fn rewards(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    reward: Rewards,
) -> Result<Value> {
    item::reward_response(db, s, a, reward).await
}
async fn cached_hero(db: &mut SqliteConnection, a: i64, id: i64) -> Result<Value> {
    let mut h = hero::info(db, a, id as i32).await?;
    for part in 1..=10 {
        let slot = n(&h, &format!("EquipItemSlotIndex{part}"));
        let r = sqlx::query("SELECT * FROM equip_items WHERE account_id=? AND slot_index=?")
            .bind(a)
            .bind(slot)
            .fetch_optional(&mut *db)
            .await?;
        h[format!("EquipItemInfo{part}")] = r
            .as_ref()
            .map(|r| json!(crate::models::equip::EquipItemInfo::from_row(r)))
            .unwrap_or(Value::Null);
    }
    h["PunishmentRuneOptionInfos"] = json!([]);
    Ok(h)
}
async fn user(db: &mut SqliteConnection, a: i64) -> Result<Value> {
    let r=sqlx::query("SELECT a.nick,a.last_login,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u ON u.account_id=a.account_id WHERE a.account_id=?").bind(a).fetch_optional(db).await?.ok_or_else(||rule("GuildMemberNotFound"))?;
    Ok(
        json!({"AccountId":a,"Nick":r.get::<String,_>("nick"),"TeamLevel":r.get::<i64,_>("team_level"),"AvatarHeroIndex":r.get::<i64,_>("avatar_hero_index"),"LastLoginTime":r.get::<Option<String>,_>("last_login"),"MatchScore":0,"SeasonWin":0,"SeasonLose":0}),
    )
}
pub(crate) async fn validate_shop(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    shop: i64,
) -> Result<i64> {
    let (g, _) = guild::membership(db, a).await?;
    let info = guild::state(db, s, g).await?;
    let building = info["GuildBuildingInfos"]
        .as_array()
        .into_iter()
        .flatten()
        .find(|v| n(v, "BuildingIndex") == 3)
        .ok_or_else(|| rule("ContentsDisabled"))?;
    let level = n(building, "BuildingLevel");
    if level <= 0 {
        return Err(rule("ContentsDisabled"));
    }
    let data = row(s, "GuildBuilding", &[("Index", 3), ("Level", level)])?;
    if shop != n(data, "ShopIndex") {
        return Err(rule("ContentsDisabled"));
    }
    Ok(level)
}
pub(crate) async fn login(s: &AppState, a: i64) -> Result<Value> {
    let mut tx = s.db.begin().await?;
    item::init(&mut tx, s, a).await?;
    arena::settle(&mut tx, s, a).await?;
    warfare::settle(&mut tx, s, a).await?;
    let mut out = json!({"BattleInfo":arena::battle_info(&mut tx,s,a).await?,"SwordResult":arena::tickets(&mut tx,s,a,"Sword",0).await?,"GuildPoint":hero::currency(&mut tx,a,"GuildPoint",0).await?["NewValue"]});
    out["GuildRaidTicket"] = guild_ticket(&mut tx, s, a, 0).await?;
    let withdrawal = get(&mut tx, a, "withdraw", 0).await?;
    out["MiscInfo"] = json!({"GuildWithdrawCount":n(&withdrawal,"Count"),"GuildWithdrawTime":if n(&withdrawal,"At")>0{json!(time(n(&withdrawal,"At")))}else{Value::Null}});
    if let Ok((g, _)) = guild::membership(&mut tx, a).await {
        out["MyGuildInfo"] = guild::state(&mut tx, s, g).await?;
        out["GuildMemberInfo"] = guild::member(&mut tx, s, a).await?;
    }
    tx.commit().await?;
    Ok(out)
}
pub(crate) async fn guild_ticket(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    amount: i64,
) -> Result<Value> {
    let v = get(db, a, "guild_ticket", 0).await?;
    if v["Day"] != day() {
        sqlx::query(
            "UPDATE user_info SET guild_raid_ticket=MAX(guild_raid_ticket,?) WHERE account_id=?",
        )
        .bind(constant(s, "GuildRaidEnterCount", 3))
        .bind(a)
        .execute(&mut *db)
        .await?;
        put(db, a, "guild_ticket", 0, &json!({"Day":day()})).await?;
    }
    let value:Option<i64>=sqlx::query_scalar("UPDATE user_info SET guild_raid_ticket=guild_raid_ticket+? WHERE account_id=? AND guild_raid_ticket+? BETWEEN 0 AND 2147483647 RETURNING guild_raid_ticket").bind(amount).bind(a).bind(amount).fetch_optional(db).await?;
    let value = value.ok_or_else(|| rule("NotEnoughDungeonKey"))?;
    Ok(
        json!({"Type":"GuildRaidTicket","AddValue":amount,"NewValue":value,"StaminaRechargeTime":time(now()),"NextRechargeRemainTime":0,"FullRechargeRemainTime":0,"RechargeCount":0,"IsHide":false}),
    )
}
pub(crate) async fn ensure_available(
    db: &mut SqliteConnection,
    a: i64,
    heroes: &[i64],
) -> Result<()> {
    let rows:Vec<String>=sqlx::query_scalar("SELECT data FROM arena_runs WHERE account=? AND status IN ('waiting','battle') AND COALESCE(json_extract(data,'$.Expires'),started+900)>=?").bind(a).bind(now()).fetch_all(&mut *db).await?;
    for v in rows {
        let v: Value = parse(&v)?;
        if v["Heroes"]
            .as_array()
            .is_some_and(|v| heroes.iter().any(|id| v.contains(&json!(id))))
        {
            return Err(rule("AlreadyOnBattleHero"));
        }
    }
    let v = get(db, a, "guild_arena_active", 0).await?;
    if v["Active"] == true
        && n(&v, "Expires") >= now()
        && v["Heroes"]
            .as_array()
            .is_some_and(|v| heroes.iter().any(|id| v.contains(&json!(id))))
    {
        return Err(rule("AlreadyOnBattleHero"));
    }
    Ok(())
}
