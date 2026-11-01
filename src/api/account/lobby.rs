use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use chrono::{Datelike, Duration, NaiveDate, Utc};
use crate::{
    error::{Result, ServerError},
    models::{
        user::UserInfo,
        hero::HeroInfo,
        item::ItemInfo,
        hero_inn::PlayerHeroFriendlyInfo,
    },
    state::AppState,
};

/// First lobby request (called after initial login for new users)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct FirstLobbyRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub nick: Option<String>,
}

/// First lobby response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct FirstLobbyResponse {
    #[serde(flatten)]
    pub extensions: serde_json::Value,
    pub hero_preset_storages: Vec<serde_json::Value>,
    pub player_book_mark_hero_info: serde_json::Value,
    pub base_result: String,
    pub result: String,
}

/// Handle first lobby request (set initial nickname)
pub async fn first_lobby(
    State(state): State<AppState>,
    Form(req): Form<FirstLobbyRequest>,
) -> Result<Json<FirstLobbyResponse>> {
    // Client sends SessionKey, not SessionId
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    if let Some(nick) = req.nick {
        // Update nickname
        sqlx::query("UPDATE accounts SET nick = ? WHERE account_id = ?")
            .bind(&nick)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    let bookmarks = crate::api::heroes::bookmarks(&mut *state.db.acquire().await?,session.account_id).await?;
    let hero_preset_storages = crate::api::heroes::presets::list(&mut *state.db.acquire().await?,session.account_id).await?;
    let mut extensions=crate::api::extensions::first_lobby(&state,session.account_id).await?;
    let live=crate::api::live::snapshot(&state,session.account_id).await?;
    if let Some(fields)=live.as_object(){for(k,v)in fields{extensions[k]=v.clone();}}
    let supporting=crate::api::services::login(&state,session.account_id).await?;
    for(k,v)in supporting.as_object().unwrap(){extensions[k]=v.clone();}
    let punishment=crate::api::battle::punishment_lobby_snapshot(&state,session.account_id).await?;
    for(k,v)in punishment.as_object().unwrap(){extensions[k]=v.clone();}
    extensions["StaminaResults"]=crate::api::account::stamina::login(&state,session.account_id).await?;
    Ok(Json(FirstLobbyResponse {
        extensions,
        hero_preset_storages,
        player_book_mark_hero_info: bookmarks,
        base_result: "Success".to_string(),
        result: "Success".to_string(),
    }))
}

/// Chapter dungeon info for lobby responses
#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct ChapterDungeonInfo {
    pub chapter_index: i32,
    pub dungeon_index: i32,
    pub max_star: i16,
    pub first_rewarded_diff: i16,
    pub scenario_complete: i16,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub visited_time: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub completed_time: Option<String>,
    pub daily_completed_count: i32,
    pub reset_count: i32,
}

/// Enter lobby request (called when entering the main game lobby)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct EnterLobbyRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
}

/// Enter lobby response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct EnterLobbyResponse {
    #[serde(flatten)]
    pub battle: serde_json::Value,
    // The native lobby handler overwrites MiscInfo.LoginDailyCount with this field.
    pub login_daily_count: i64,
    pub attendance_datas: Vec<serde_json::Value>,
    pub attendance_infos: Vec<serde_json::Value>,
    pub achievement_infos: Vec<serde_json::Value>,
    pub opend_mission_categories: Vec<serde_json::Value>,
    pub reserved_sub_quest_infos: Vec<serde_json::Value>,
    pub reserved_play_record_infos: Vec<serde_json::Value>,
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub user_info: Option<UserInfo>,
    #[serde(skip_serializing_if = "Vec::is_empty")]
    pub heroes: Vec<HeroInfo>,
    #[serde(skip_serializing_if = "Vec::is_empty")]
    pub items: Vec<ItemInfo>,
    pub dungeon_infos: Vec<ChapterDungeonInfo>,
    pub server_time: String,
    #[serde(rename = "ServerUTCTime")]
    pub server_utc_time: String,
    pub server_local_time: String,
    #[serde(rename = "ServerUTCOffsetHour")]
    pub server_utc_offset_hour: i32,
    pub remain_day_second: i64,
    pub remain_week_second: i64,
    pub remain_month_second: i64,
    pub new_mail_count: i32,
    pub friend_request_count: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendly_info: Option<PlayerHeroFriendlyInfo>,
}

/// Legacy lobby wrapper for direct handler tests; HTTP uses enter_lobby_http.
#[cfg(test)]
pub async fn enter_lobby(
    State(state): State<AppState>,
    Form(req): Form<EnterLobbyRequest>,
) -> Result<Json<EnterLobbyResponse>> {
    enter_lobby_inner(state, req, false).await
}

/// The native lobby response does not define UserInfo/Heroes/Items. The patched
/// client opts out of those legacy snapshots; initial login remains complete.
pub async fn enter_lobby_http(
    State(state): State<AppState>,
    headers: axum::http::HeaderMap,
    Form(req): Form<EnterLobbyRequest>,
) -> Result<Json<serde_json::Value>> {
    let compact=headers.get("x-sprk-compact-lobby").is_some_and(|v|v=="1");
    let session=req.session_key.as_deref().or(req.session_id.as_deref()).unwrap_or("").to_owned();
    let mut response=enter_lobby_inner(state, req, compact).await?;
    if compact {
        conditional_catalog(&mut response.0.battle, &session,
            headers.get("x-sprk-shop-catalog").and_then(|v|v.to_str().ok()).unwrap_or(""));
    }
    let mut value=serde_json::to_value(response.0).map_err(|e|ServerError::Internal(e.to_string()))?;
    crate::api::progression::notifications::conditional_sections(&mut value, &session, &headers);
    Ok(Json(value))
}

fn conditional_catalog(fields: &mut serde_json::Value, session: &str, acknowledged: &str) {
    use sha2::{Digest, Sha256};
    let mut digest=Sha256::new();digest.update(session.as_bytes());
    digest.update(serde_json::to_vec(&fields["PayShopProductInfos"]).unwrap());
    let token=hex::encode(digest.finalize());
    if token==acknowledged {fields.as_object_mut().unwrap().remove("PayShopProductInfos");}
    fields["SprkShopCatalogToken"]=serde_json::json!(token);
}

async fn enter_lobby_inner(state: AppState, req: EnterLobbyRequest, compact: bool) -> Result<Json<EnterLobbyResponse>> {
    // Client sends SessionKey, not SessionId
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;

    let withdrawal = crate::api::battle::abandon_local_run_on_lobby(&state, account_id, &session_id).await?;
    crate::api::tutorial::restore_clause_reward(&state, account_id).await?;

    let (mut user_info, heroes, items) = if compact {
        (None, Vec::new(), Vec::new())
    } else {
        // Legacy API consumers can still request the full account snapshot.
        let account_row = sqlx::query("SELECT nick FROM accounts WHERE account_id = ?")
            .bind(account_id)
            .fetch_optional(&state.db)
            .await?
            .ok_or_else(|| ServerError::NotFound("Account not found".to_string()))?;
    
        let nick: String = account_row.get("nick");

        // Fetch user info
        let user_info_row = sqlx::query("SELECT * FROM user_info WHERE account_id = ?")
            .bind(account_id)
            .fetch_optional(&state.db)
            .await?;

        let user_info = user_info_row.map(|row| UserInfo {
            session_key: session_id.clone(),
            account_id,
            nick,
            gold: row.get("gold"),
            gem: row.get("gem"),
            pay_gem: row.get("pay_gem"),
            pvp_coin: row.get("pvp_coin"),
            stamina: row.get("stamina"),
            stamina_recharge_time: None, // Handled separately if needed
            team_level: row.get("team_level"),
            team_exp: row.get("team_exp"),
            sword: row.get("sword"),
            sword_recharge_time: None, // Handled separately if needed
            sword2: row.get("sword2"),
            avatar_hero_index: row.get("avatar_hero_index"),
            royal_point: row.get("royal_point"),
            raid_point: row.get("raid_point"),
            mileage: row.get("mileage"),
            friendship_point: row.get("friendship_point"),
            guild_raid_ticket: row.get("guild_raid_ticket"),
            world_boss_ticket: row.get("world_boss_ticket"),
            ..Default::default()
        });

        // Fetch heroes
        let heroes = crate::api::heroes::snapshot(&mut *state.db.acquire().await?, account_id).await?;

        // Fetch items
        let item_rows = sqlx::query("SELECT * FROM items WHERE account_id = ? AND count > 0")
            .bind(account_id)
            .fetch_all(&state.db)
            .await?;

        let items: Vec<ItemInfo> = item_rows.iter().map(|row| ItemInfo {
            item_index: row.get("item_index"),
            count: row.get("count"),
            locked: row.get::<i32,_>("locked") as u8,
            created_time: row.get("created_time"),
            uid: row.get::<i32,_>("item_index").to_string(),
        }).collect();
        (user_info, heroes, items)
    };

    // Count unread mail
    let mail_count: i32 = sqlx::query_scalar(
        "SELECT COUNT(*) FROM mails WHERE account_id = ? AND is_received = 0"
    )
    .bind(account_id)
    .fetch_one(&state.db)
    .await?;

    // Count friend requests
    let friend_request_count: i32 = sqlx::query_scalar(
        "SELECT COUNT(*) FROM friends WHERE friend_account_id = ? AND status = 0"
    )
    .bind(account_id)
    .fetch_one(&state.db)
    .await?;

    // Fetch campaign progress for dungeon infos
    let campaign_rows = sqlx::query(
        "SELECT chapter_id, dungeon_id, clear_count, best_star, completed_time FROM campaign_progress WHERE account_id = ? ORDER BY chapter_id, dungeon_id"
    )
    .bind(account_id)
    .fetch_all(&state.db)
    .await
    .unwrap_or_default();

    // Build dungeon infos from database
    // 
    // MaxStar encoding: difficulty * 10 + stars
    //   - Chapter 1 has MinDifficulty = Normal (1), not Easy (0)!
    //   - So the DB stores best_star = 3, but we send MaxStar = 13 (Normal 3-star)
    //   - The client checks: (MaxStar / 10) >= MinDifficulty to verify completion
    //
    let mut dungeon_infos: Vec<ChapterDungeonInfo> = campaign_rows.iter().map(|row| {
        let chapter_id: i32 = row.get("chapter_id");
        let clear_count: i32 = row.get("clear_count");
        let best_star: i32 = row.get("best_star");
        let completed_time: Option<String> = row.get("completed_time");
        let is_completed = clear_count > 0 || best_star > 0 || completed_time.is_some();
        
        let difficulty = state.tables.tutorials.dungeon_difficulty(chapter_id, row.get("dungeon_id"));
        let max_star = if is_completed { (difficulty * 10 + best_star) as i16 } else { 0 };
        let first_rewarded_diff = if is_completed { (1 << difficulty) as i16 } else { 0 };
        
        ChapterDungeonInfo {
            chapter_index: chapter_id,
            dungeon_index: row.get("dungeon_id"),
            max_star,
            first_rewarded_diff,
            scenario_complete: if is_completed { 1 } else { 0 },
            visited_time: Some(state.server_time_str()),
            completed_time,
            daily_completed_count: clear_count,
            reset_count: 0,
        }
    }).collect();

    // Note: We don't pre-populate dungeon 1-1 here for new users.
    // The tutorial flow (tutorial 10000) will unlock 1-1 when appropriate.
    // For returning users who skip tutorial, user.rs handles pre-populating dungeons.

    // Fetch Hero Inn friendly info if exists
    let friendly_info = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?
        .map(|row| PlayerHeroFriendlyInfo {
            hero_index: row.get("hero_index"),
            selected_hero_index: row.get("selected_hero_index"),
            friendly_point: row.get("friendly_point"),
            last_greeting_time: row.get("last_greeting_time"),
            last_conversation_time: row.get("last_conversation_time"),
            last_gift_time: row.get("last_gift_time"),
            selected_time: row.get("selected_time"),
            selected_hero_indice: row.get("selected_hero_indices"),
            last_roulette_time: row.get("last_roulette_time"),
        });

    let progression = crate::api::progression::cache::get(&state, &session_id, session.account_id).await?;
    let mut battle = crate::api::battle::login(&state, account_id).await?;
    // EnterLobby does not reinitialize PlayerCurrencyInfos. Publish withdrawal
    // refunds through the common handler too, so native boss entry sees the
    // restored gauge immediately without requiring a fresh client login.
    if withdrawal["ReservedCurrencyResults"].is_array() {
        battle["ReservedCurrencyResults"] = withdrawal["ReservedCurrencyResults"].clone();
    }
    let community=crate::api::community::login(&state,account_id).await?;
    battle["GuildInfo"]=community["MyGuildInfo"].clone();
    battle["BattleInfo"]=community["BattleInfo"].clone();
    if let Some(keys)=battle["BattleKeyResults"].as_array_mut(){keys.push(community["SwordResult"].clone());keys.push(community["GuildRaidTicket"].clone());}
    if let Some(user)=&mut user_info{user.guild_point=community["GuildPoint"].as_i64().unwrap_or(0);}
    for info in battle["DungeonInfos"].as_array().into_iter().flatten() {
        let info: ChapterDungeonInfo = serde_json::from_value(info.clone()).map_err(|e|ServerError::Internal(e.to_string()))?;
        if let Some(old)=dungeon_infos.iter_mut().find(|d|d.chapter_index==info.chapter_index&&d.dungeon_index==info.dungeon_index){*old=info;}else{dungeon_infos.push(info);}
    }
    if let Some(user)=&mut user_info {
        let mut value=serde_json::to_value(&*user).map_err(|e|ServerError::Internal(e.to_string()))?;
        for key in battle["BattleKeyResults"].as_array().into_iter().flatten(){if let Some(kind)=key["Type"].as_str(){value[kind]=key["NewValue"].clone();}}
        // World Boss shops read this counter from UserInfo after lobby refresh.
        value["WorldBossPoint"] = battle["PlayerCurrencyInfos"].as_array().into_iter().flatten()
            .find(|c| c["CurrencyType"] == "WorldBossPoint").map(|c| c["Amount"].clone())
            .unwrap_or_else(|| serde_json::json!(0));
        *user=serde_json::from_value(value).map_err(|e|ServerError::Internal(e.to_string()))?;
    }
    battle.as_object_mut().unwrap().remove("DungeonInfos");
    battle["StaminaResults"]=crate::api::account::stamina::login(&state,account_id).await?;
    if let Some(user)=&mut user_info {
        let mut value=serde_json::to_value(&*user).map_err(|e|ServerError::Internal(e.to_string()))?;
        for key in battle["StaminaResults"].as_array().into_iter().flatten(){if let Some(kind)=key["Type"].as_str(){value[if kind=="Chicken"{"Stamina"}else{kind}]=key["NewValue"].clone();}}
        *user=serde_json::from_value(value).map_err(|e|ServerError::Internal(e.to_string()))?;
    }
    battle.as_object_mut().unwrap().remove("BattleKeyResults");
    let supporting=crate::api::services::login(&state,account_id).await?;
    for(k,v)in supporting.as_object().unwrap(){battle[k]=v.clone();}
    let shop=crate::api::live::shop_snapshot(&state,account_id).await?;
    for(k,v)in shop.as_object().unwrap(){battle[k]=v.clone();}
    let (remain_day_second, remain_week_second, remain_month_second) = reset_countdowns(Utc::now());
    Ok(Json(EnterLobbyResponse {
        battle,
        login_daily_count: progression["LoginDailyCount"].as_i64().unwrap_or(0),
        attendance_datas: progression["AttendanceDatas"].as_array().cloned().unwrap_or_default(),
        attendance_infos: progression["AttendanceInfos"].as_array().cloned().unwrap_or_default(),
        achievement_infos: progression["AchievementInfos"].as_array().cloned().unwrap_or_default(),
        opend_mission_categories: progression["OpendMissionCategories"].as_array().cloned().unwrap_or_default(),
        reserved_sub_quest_infos: crate::api::progression::notifications::subquest_updates(&state, &session_id, &progression["SubQuestInfos"]),
        reserved_play_record_infos: progression["PlayRecordInfos"].as_array().cloned().unwrap_or_default(),
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        user_info,
        heroes,
        items,
        dungeon_infos,
        server_time: state.server_time_str(),
        server_utc_time: state.server_utc_time_str(),
        server_local_time: state.server_time_str(),
        server_utc_offset_hour: 0,
        remain_day_second,
        remain_week_second,
        remain_month_second,
        new_mail_count: mail_count,
        friend_request_count,
        friendly_info,
    }))
}

// Match the UTC daily, Monday weekly and calendar-month resets used by towers.
fn reset_countdowns(now: chrono::DateTime<Utc>) -> (i64, i64, i64) {
    let date = now.date_naive();
    let next_day = date + Duration::days(1);
    let next_week = date + Duration::days(7 - date.weekday().num_days_from_monday() as i64);
    let (year, month) = if date.month() == 12 { (date.year() + 1, 1) } else { (date.year(), date.month() + 1) };
    let next_month = NaiveDate::from_ymd_opt(year, month, 1).unwrap();
    let seconds = |d: NaiveDate| d.and_hms_opt(0, 0, 0).unwrap().and_utc().timestamp() - now.timestamp();
    (seconds(next_day), seconds(next_week), seconds(next_month))
}

#[cfg(test)]
mod reset_tests {
    use super::*;

    #[test]
    fn lobby_reset_countdowns_follow_calendar_boundaries() {
        let at = |s: &str| chrono::DateTime::parse_from_rfc3339(s).unwrap().with_timezone(&Utc);
        assert_eq!(reset_countdowns(at("2026-09-30T23:59:59Z")), (1, 345601, 1));
        assert_eq!(reset_countdowns(at("2026-12-31T23:59:59Z")).2, 1);
        assert_eq!(reset_countdowns(at("2024-02-28T00:00:00Z")).2, 172800);
        assert_eq!(reset_countdowns(at("2026-10-05T00:00:00Z")).1, 604800);
        assert_eq!(reset_countdowns(at("2026-10-04T23:59:59Z")).1, 1);
    }

    #[tokio::test]
    async fn compact_lobby_keeps_native_state_and_skips_legacy_inventory() {
        let db=sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
        crate::database::create_tables(&db).await.unwrap();
        let state=AppState::new(db,crate::tables::test_tables());
        let login=crate::api::account::user::test_login(State(state.clone()),axum::body::Bytes::from_static(b"LoginId=compact-lobby")).await.unwrap().0;
        let req=||EnterLobbyRequest{session_id:None,session_key:Some(login.user_info.session_key.clone())};
        let legacy=serde_json::to_value(enter_lobby_inner(state.clone(),req(),false).await.unwrap().0).unwrap();
        let compact=serde_json::to_value(enter_lobby_inner(state.clone(),req(),true).await.unwrap().0).unwrap();
        for key in ["Heroes","Items","UserInfo"] {assert!(compact.get(key).is_none(),"{key}");}
        assert!(!legacy["Heroes"].as_array().unwrap().is_empty());
        for key in ["DungeonInfos","LoginDailyCount","AttendanceDatas","AchievementInfos","OpendMissionCategories","PayShopProductInfos"] {
            assert_eq!(compact[key],legacy[key],"Native field changed: {key}");
        }
        println!("Lobby fixture bytes: legacy={}, compact={}",serde_json::to_vec(&legacy).unwrap().len(),serde_json::to_vec(&compact).unwrap().len());
        let mut sizes=compact.as_object().unwrap().iter().map(|(k,v)|(serde_json::to_vec(v).unwrap().len(),k)).collect::<Vec<_>>();
        sizes.sort_by(|a,b|b.0.cmp(&a.0));
        println!("Largest lobby fields: {:?}",&sizes[..sizes.len().min(8)]);
        let mut headers=axum::http::HeaderMap::new();headers.insert("x-sprk-compact-lobby","1".parse().unwrap());
        let first=serde_json::to_value(enter_lobby_http(State(state.clone()),headers.clone(),Form(req())).await.unwrap().0).unwrap();
        headers.insert("x-sprk-shop-catalog",first["SprkShopCatalogToken"].as_str().unwrap().parse().unwrap());
        let repeated=serde_json::to_value(enter_lobby_http(State(state.clone()),headers.clone(),Form(req())).await.unwrap().0).unwrap();
        assert!(repeated.get("PayShopProductInfos").is_none());
        assert_eq!(first["SprkShopCatalogToken"],repeated["SprkShopCatalogToken"]);
        assert_eq!(first["PlayerProductPurchaseInfos"],repeated["PlayerProductPurchaseInfos"]);
        println!("Acknowledged lobby fixture bytes: {}",serde_json::to_vec(&repeated).unwrap().len());
        use std::io::Write;
        for (name,value) in [("first",first),("acknowledged",repeated)] {
            let mut encoder=flate2::write::GzEncoder::new(Vec::new(),flate2::Compression::default());
            encoder.write_all(&serde_json::to_vec(&value).unwrap()).unwrap();
            println!("Gzip {name} lobby fixture bytes: {}",encoder.finish().unwrap().len());
        }
        headers.insert("x-sprk-sections","1".parse().unwrap());
        let full=enter_lobby_http(State(state.clone()),headers.clone(),Form(req())).await.unwrap().0;
        for (token,header) in [("SprkAchievementToken","x-sprk-achievements"),("SprkPlayRecordToken","x-sprk-play-records")] {
            headers.insert(header,full[token].as_str().unwrap().parse().unwrap());
        }
        let reduced=enter_lobby_http(State(state),headers,Form(req())).await.unwrap().0;
        assert!(reduced.get("AchievementInfos").is_none());
        assert!(reduced.get("ReservedPlayRecordInfos").is_none());
        assert_eq!(reduced["AttendanceDatas"],full["AttendanceDatas"]);
        assert_eq!(reduced["DungeonInfos"],full["DungeonInfos"]);
        for (name,value) in [("sections-first",full),("sections-acknowledged",reduced)] {
            let bytes=serde_json::to_vec(&value).unwrap();
            let mut encoder=flate2::write::GzEncoder::new(Vec::new(),flate2::Compression::default());
            encoder.write_all(&bytes).unwrap();
            println!("Lobby {name}: JSON={} gzip={}",bytes.len(),encoder.finish().unwrap().len());
        }
    }

    #[test]
    fn catalog_acknowledgements_resend_changed_empty_lost_and_new_session_snapshots() {
        let catalog=serde_json::json!({"PayShopProductInfos":[{"Index":1,"Price":5}]});
        let mut first=catalog.clone();conditional_catalog(&mut first,"one","");
        let token=first["SprkShopCatalogToken"].as_str().unwrap();
        let mut repeated=catalog.clone();conditional_catalog(&mut repeated,"one",token);
        assert!(repeated.get("PayShopProductInfos").is_none());
        for (mut value,session,ack) in [(catalog.clone(),"two",token),(catalog.clone(),"one",""),
            (serde_json::json!({"PayShopProductInfos":[]}),"one",token),
            (serde_json::json!({"PayShopProductInfos":[{"Index":1,"Price":6}]}),"one",token)] {
            conditional_catalog(&mut value,session,ack);assert!(value["PayShopProductInfos"].is_array());
        }
    }
}
