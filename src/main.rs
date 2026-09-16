mod api;
mod crypto;
mod database;
mod error;
mod models;
mod state;
mod tables;

use axum::{
    Router,
    middleware,
    routing::{get, post},
};
use std::net::SocketAddr;
use std::path::Path;
use tower_http::{
    cors::{Any, CorsLayer},
    trace::TraceLayer,
};
use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt};

use crate::state::AppState;
use crate::tables::GameTables;

#[tokio::main]
async fn main() -> anyhow::Result<()> {
    // Initialize tracing
    tracing_subscriber::registry()
        .with(tracing_subscriber::EnvFilter::new(
            std::env::var("RUST_LOG").unwrap_or_else(|_| "info,tower_http=debug".into()),
        ))
        .with(tracing_subscriber::fmt::layer())
        .init();

    tracing::info!("Starting sprk Private Server...");

    // Initialize database
    let db = database::init_database().await?;
    
    // Load game tables
    let tables_path = std::env::var("GAME_TABLES_PATH")
        .unwrap_or_else(|_| "tables".to_string());
    let tables = match GameTables::load(Path::new(&tables_path)) {
        Ok(t) => {
            tracing::info!("Loaded game tables from: {}", tables_path);
            t
        }
        Err(e) => {
            tracing::warn!("Failed to load game tables from {}: {}. Using fallback values.", tables_path, e);
            GameTables::empty()
        }
    };
    
    // Create application state
    let state = AppState::new(db, tables);
    let chat_bind = std::env::var("CHAT_BIND").unwrap_or_else(|_| "127.0.0.1".into());
    let chat_listener = tokio::net::TcpListener::bind((chat_bind.as_str(), state.chat.port)).await?;
    tracing::info!("Message server listening on {}", chat_listener.local_addr()?);
    let chat_state = state.clone();
    tokio::spawn(async move {
        if let Err(error) = api::chat::serve(chat_listener, chat_state).await { tracing::error!(%error, "Message server stopped"); }
    });


    // Configure CORS (permissive for game client)
    let cors = CorsLayer::new()
        .allow_origin(Any)
        .allow_methods(Any)
        .allow_headers(Any);

    // Build router with all API routes
    let app = Router::new()
        .merge(api::extensions::routes())
        // Health check
        .route("/health", get(health_check))
        // Initial host query (client fetches this first to get server info)
        // Original path from client config
        .route("/Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json", get(api::query::get_host_info))
        // Alternative shorter paths for patched clients
        .route("/Masang_Tokyo/Live/:version/:hostfile", get(api::query::get_host_info))
        .route("/host.json", get(api::query::get_host_info))
        // Auth endpoints (Masang login server API)
        .route("/api/auth/login/guest", post(api::auth::guest_login))
        .route("/api/auth/token/verify", post(api::auth::verify_token))
        .route("/api/auth/refresh-token", post(api::auth::refresh_token))
        // User authentication endpoints
        .route("/user/login", post(api::user::login))
        .route("/user/logout", post(api::user::logout))
        .route("/user/get_certificate", post(api::user::get_certificate))
        // Ping endpoints
        .route("/ping", post(api::ping::ping))
        .route("/ping/idle", post(api::ping::ping_idle))
        // Server query
        .route("/query/session", post(api::query::query_session))
        .route("/query/nick", post(api::query::query_nick))
        // First lobby entry
        .route("/first/lobby", post(api::lobby::first_lobby))
        .route("/user/first_lobby", post(api::lobby::first_lobby))
        // Enter lobby
        .route("/enter/lobby", post(api::lobby::enter_lobby))
        .route("/lobby/enter_lobby", post(api::lobby::enter_lobby))
        // Hero management
        .route("/hero/buy", post(api::hero::buy_hero))
        .route("/hero/bookmark", post(api::hero::bookmark_hero))
        .route("/avatar/change", post(api::hero::change_avatar_hero))
        // Hero Inn - Hero recruitment through friendship
        .route("/hero/buy_costume", post(api::hero::buy_costume))
        .route("/hero/set_costume", post(api::hero::set_costume))
        .route("/hero/unset_costume", post(api::hero::unset_costume))
        .route("/hero/bookmark_hero", post(api::hero::bookmark_hero))
        .route("/hero/set_hide_hero_unique_weapon", post(api::hero::set_hide_hero_unique_weapon))
        .route("/hero/save_costume_storage_slot", post(api::hero::save_costume_storage_slot))
        .route("/hero/set_costume_storage_slot", post(api::hero::set_costume_storage_slot))
        .route("/hero/learn_hero_skill", post(api::hero::learn_hero_skill))
        .route("/hero/upgrade_hero_skill", post(api::hero::upgrade_hero_skill))
        .route("/hero/extend_hero_skill", post(api::hero::extend_hero_skill))
        .route("/hero/purify_hero", post(api::hero::purify_hero))
        .route("/hero/max_purify_hero", post(api::hero::max_purify_hero))
        .route("/hero/upgrade_hero_star", post(api::hero::upgrade_hero_star))
        .route("/hero/transcend_hero", post(api::hero::transcend_hero))
        .route("/hero/learn_hero_transcend_skill", post(api::hero::learn_hero_transcend_skill))
        .route("/hero/reset_hero_transcend_skill", post(api::hero::reset_hero_transcend_skill))
        .route("/hero/buy_hero_transcend_skill_page", post(api::hero::buy_hero_transcend_skill_page))
        .route("/hero/apply_hero_transcend_skill_page", post(api::hero::apply_hero_transcend_skill_page))
        .route("/hero/learn_hero_transcend_skill_page", post(api::hero::learn_hero_transcend_skill_page))
        .route("/hero/reset_hero_transcend_skill_page", post(api::hero::reset_hero_transcend_skill_page))
        .route("/hero/hero_limit_break_level_up", post(api::hero::hero_limit_break_level_up))
        .route("/hero/hero_limit_break_exp_up", post(api::hero::hero_limit_break_exp_up))
        .route("/hero_storage_slot/buy_hero_storage_slot", post(api::hero::buy_hero_storage_slot))
        .route("/hero_storage_slot/add_hero_storage_slot", post(api::hero::add_hero_storage_slot))
        .route("/hero_storage_slot/set_hero_storage_slot", post(api::hero::set_hero_storage_slot))
        .route("/hero_storage_slot/remove_hero_storage_slot", post(api::hero::remove_hero_storage_slot))
        .route("/hero_storage_slot/change_hero_storage_slot_name", post(api::hero::change_hero_storage_slot_name))
        .route("/shop/buy_hero", post(api::hero::buy_hero))
        .route("/user/change_avatar_hero", post(api::hero::change_avatar_hero))
        .route("/trial_dungeon/get_awake_material_challenge", post(api::hero::get_awake_material_challenge))
        .route("/hero/request_new_friendly_hero", post(api::hero_inn::request_new_friendly_hero))
        .route("/hero/do_hero_friendly", post(api::hero_inn::do_hero_friendly))
        .route("/hero/change_recruit_hero", post(api::hero_inn::change_recruit_hero))
        .route("/hero/recruit_hero", post(api::hero_inn::recruit_hero))
        .route("/hero/hero_inn_reset_time", post(api::hero_inn::hero_inn_reset_time))
        .route("/hero/give_reward_max_closeness_hero", post(api::hero_inn::give_reward_max_closeness_hero))
        // Hero Inn Roulette - Mini game
        .route("/hero/request_hero_inn_roulette", post(api::hero_inn::request_hero_inn_roulette))
        .route("/hero/give_reward_hero_inn_roulette", post(api::hero_inn::give_reward_hero_inn_roulette))
        // Equipment
        .route("/equip/set_equip", post(api::equip::set_equip))
        .route("/equip/set_equip_swap", post(api::equip::set_equip))
        .route("/equip/unset_equip", post(api::equip::unset_equip))
        // Shop
        .route("/shop/get_shop_list", post(api::shop::get_shop_list))
        .route("/shop/request_shop_list", post(api::shop::request_shop_list))
        .route("/shop/get_all_shop_item_purchase_count", post(api::shop::get_all_shop_item_purchase_count))
        .route("/shop/get_payshop_products", post(api::shop::get_payshop_products))
        .route("/shop/buy_payshop_product", post(api::shop::unavailable_product))
        .route("/shop/check_payshop_product", post(api::shop::unavailable_product))
        .route("/shop/list", post(api::shop::get_shop_list))
        .route("/shop/buy", post(api::shop::buy_shop_item))
        .route("/shop/buy_shop_item", post(api::shop::buy_shop_item))
        // Item
        .route("/item/break_rune", post(api::item::break_rune))
        .route("/item/use_equip_option_select_item", post(api::item::use_equip_option_select_item))
        .route("/item/use_booster_item", post(api::item::use_booster_item))
        .route("/item/use_potion_item", post(api::item::use_potion_item))
        .route("/item/use_package_item", post(api::item::use_package_item))
        .route("/item/use_package_select_item", post(api::item::use_package_select_item))
        .route("/item/use_weapon_unique_select_item", post(api::item::use_weapon_unique_select_item))
        .route("/item/use_hero_select_item", post(api::item::use_hero_select_item))
        .route("/item/break_item", post(api::item::break_item))
        .route("/item/set_lock_rune_item", post(api::item::set_lock_rune_item))
        .route("/item/craft_item", post(api::craft::craft_item))
        .route("/item/add_craft_slot", post(api::craft::add_craft_slot))
        .route("/item/cancel_craft_item", post(api::craft::cancel_craft_item))
        .route("/item/instant_craft_item", post(api::craft::instant_craft_item))
        .route("/item/take_craft_item", post(api::craft::take_craft_item))
        .route("/equip/sell_equip", post(api::item::sell_equip))
        .route("/equip/set_lock_equip_item", post(api::item::set_lock_equip_item))
        .route("/equip/extend_equip", post(api::item::extend_equip))
        .route("/equip/extend_chest", post(api::item::extend_chest))
        .route("/equip/set_chest", post(api::item::set_chest))
        .route("/equip/unset_chest", post(api::item::unset_chest))
        .route("/shop/sell_item", post(api::item::sell_item))
        .route("/inventory/get_inventory", post(api::item::get_inventory))
        .route("/item/use_flask_item", post(api::extensions::handle))
        .route("/item/cancel_flask_item", post(api::extensions::handle))
        .route("/item/use_recipe_item", post(api::extensions::handle))
        .route("/item/use_costume_select_item", post(api::hero::use_costume_select_item))
        .route("/item/use_accessory_select_item", post(api::extensions::handle))
        .route("/item/use_pet_select_item", post(api::extensions::handle))
        .route("/item/use_multiple_hero_select_item", post(api::hero::use_multiple_hero_select_item))
        .route("/item/use_hero_growth_item", post(api::hero::use_hero_growth_item))
        .route("/item/use_nick_change_item", post(api::extensions::handle))
        .route("/item/use_guild_name_change_item", post(api::extensions::handle))
        .route("/item/event_craft_item", post(api::item::unsupported_item))
        .route("/item/reward_event_roulette", post(api::item::unsupported_item))
        .route("/item/awaken_transition", post(api::extensions::handle))
        .route("/item/soul_weapon_transition_ticket", post(api::extensions::handle))
        .route("/item/soul_weapon_ability_ticket", post(api::extensions::handle))
        // Mail
        .route("/mail/check_new_mail", post(api::mail::check_new_mail))
        .route("/mail/get_mail_list", post(api::mail::get_mail_list))
        .route("/mail/get_mail_list_by_page", post(api::mail::get_mail_list_by_page))
        .route("/mail/get_global_mail_list", post(api::mail::get_global_mail_list))
        .route("/mail/receive_mail", post(api::mail::receive_mail))
        .route("/mail/receive_all_mail", post(api::mail::receive_all_mail))
        // Keep old routes for backwards compatibility
        .route("/mail/list", post(api::mail::get_mail_list))
        .route("/mail/receive", post(api::mail::receive_mail))
        .route("/mail/receive/all", post(api::mail::receive_all_mail))
        // Friend - new client endpoints
        .route("/friend/search_friend", post(api::friend::search_friend))
        .route("/friend/request_friend", post(api::friend::request_friend))
        .route("/friend/reject_friend", post(api::friend::reject_friend))
        .route("/friend/send_friendship_point", post(api::friend::send_friendship_point))
        .route("/friend/recv_friendship_point", post(api::friend::recv_friendship_point))
        // Friend - legacy endpoints
        .route("/friend/send_recv_friendship_point", post(api::friend::send_recv_friendship_point))
        .route("/friend/list", post(api::friend::get_friend_list))
        .route("/friend/accept", post(api::friend::accept_friend))
        .route("/friend/remove", post(api::friend::remove_friend))
        // Chat
        .route("/chat/info", post(api::chat::get_chat_info))
        .route("/chat/world", post(api::chat::send_world_chat))
        .route("/chat/whisper", post(api::chat::send_whisper))
        .route("/chat/guild", post(api::chat::send_guild_chat))
        .route("/chat/recent", post(api::chat::get_recent_chats))
        // Campaign
        .route("/campaign/info", post(api::campaign::get_campaign_info))
        .route("/campaign/start", post(api::campaign::start_campaign_battle))
        .route("/campaign/begin_campaign", post(api::campaign::begin_campaign))
        .route("/campaign/submit", post(api::campaign::submit_campaign_battle))
        .route("/campaign/end_campaign", post(api::campaign::end_campaign))
        .route("/campaign/visit_dungeon", post(api::campaign::visit_dungeon))
        .route("/campaign/complete_scenario_dungeon", post(api::campaign::complete_scenario_dungeon))
        // Guild
        .route("/guild/info", post(api::guild::get_guild_info))
        .route("/guild/create", post(api::guild::create_guild))
        .route("/guild/join", post(api::guild::join_guild))
        .route("/guild/leave", post(api::guild::leave_guild))
        .route("/guild/search", post(api::guild::search_guilds))
        .route("/attendance/get_attendance_reward", post(api::progression::get_attendance_reward))
        .route("/attendance/get_all_conditional_attendance_reward", post(api::progression::get_all_conditional_attendance_reward))
        .route("/logindaily/get_logindaily_reward", post(api::progression::get_logindaily_reward))
        .route("/achievement/get_achievements", post(api::progression::get_achievements))
        .route("/achievement/check_achievements", post(api::progression::check_achievements))
        .route("/achievement/reward_achievement", post(api::progression::reward_achievement))
        .route("/quest/complete_sub_quest", post(api::progression::complete_sub_quest))
        .route("/quest/progress_client_main_quest", post(api::progression::progress_client_main_quest))
        .route("/clear_mission/get_clear_mission_reward", post(api::progression::get_clear_mission_reward))
        .route("/newbie_mission/get_newbie_mission_reward", post(api::progression::get_newbie_mission_reward))
        .route("/campaign/reward_clear_chapter", post(api::progression::reward_clear_chapter))
        .route("/world_map/receive_completed_reward", post(api::progression::receive_completed_reward))
        // Attendance/Daily rewards
        .route("/attendance/info", post(api::progression::attendance_info))
        .route("/attendance/receive", post(api::progression::get_attendance_reward))
        // Achievements
        .route("/achievement/list", post(api::progression::get_achievements))
        .route("/achievement/receive", post(api::progression::reward_achievement))
        .route("/achievement/update", post(api::progression::check_achievements))
        // Stamina
        .route("/stamina/info", post(api::stamina::get_stamina_info))
        .route("/stamina/buy", post(api::stamina::buy_stamina))
        .route("/stamina/use", post(api::stamina::use_stamina))
        .route("/stamina/restore", post(api::stamina::restore_stamina))
        .route("/user/get_stamina_infos", post(api::stamina::get_stamina_infos))
        // Tutorial - endpoints matching client's expected paths
        .route("/tutorial/begin_tutorial", post(api::tutorial::begin_tutorial))
        .route("/tutorial/complete_tutorial", post(api::tutorial::complete_tutorial))
        .route("/tutorial/progress", post(api::tutorial::get_tutorial_progress))
        .route("/tutorial/skip", post(api::tutorial::skip_tutorial))
        .route("/tutorial/reward", post(api::tutorial::get_tutorial_reward))
        // Cheat/Admin (for development)
        .route("/cheat/currency", post(api::cheat::gm_add_currency))
        .route("/cheat/hero", post(api::cheat::gm_add_hero))
        .route("/cheat/level", post(api::cheat::gm_set_level))
        .route("/cheat/unlock", post(api::cheat::gm_unlock_all))
        .route("/cheat/reset", post(api::cheat::gm_reset_account))
        .route("/cheat/allheroes", post(api::cheat::gm_add_all_heroes))
        .route("/cheat/uwut", post(api::cheat::gm_add_all_uwut))
        // CDN endpoints for patch/asset downloads
        .route("/cdn/LastBuildVersion.txt", get(api::cdn::get_last_build_version))
        .route("/cdn/:version/patch.json", get(api::cdn::get_patch_json))
        // Catch-all for unimplemented endpoints
        .fallback(api::fallback::handle_fallback)
        // Middleware
        .layer(middleware::from_fn_with_state(state.clone(), api::progression::notifications::notify))
        .layer(cors)
        .layer(TraceLayer::new_for_http())
        .layer(middleware::from_fn_with_state(state.clone(), api::middleware::decrypt_request))
        .with_state(state);

    // Bind to address - use PORT env var or default to 8080
    // For production, set PORT=80 (requires admin/root privileges)
    let port: u16 = std::env::var("PORT")
        .ok()
        .and_then(|p| p.parse().ok())
        .unwrap_or(8080);
    let addr = SocketAddr::from(([0, 0, 0, 0], port));
    tracing::info!("Server listening on http://{}", addr);
    
    let listener = tokio::net::TcpListener::bind(addr).await?;
    axum::serve(listener, app).await?;

    Ok(())
}

async fn health_check() -> &'static str {
    "sprk Private Server is running!"
}
