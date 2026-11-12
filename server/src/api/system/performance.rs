//! Read paths that cannot advance missions or change hero-scaled equipment.
//! Deliberately explicit: a `get_*` endpoint can claim rewards or settle battles.
pub(crate) fn observation(path: &str) -> bool {
    matches!(
        path,
        "/ping"
            | "/ping/idle"
            // Heartbeat, nickname format check and push toggles change no state.
            | "/user/ping"
            | "/user/query_nick"
            | "/user/set_push_notification"
            | "/dispatch/get_dispatch_list"
            | "/mail/check_new_mail"
            | "/mail/get_mail_list"
            | "/mail/get_mail_list_by_page"
            | "/mail/get_global_mail_list"
            | "/mail/list"
            | "/friend/list"
            | "/friend/get_friend_list"
            | "/friend/search_friend"
            | "/match/get_season_info"
            | "/global_arena/get_season_info"
            // Stock initialization/expiry cannot change missions, hero levels
            // or equipment ownership. Paid request_shop_list is not exempt.
            | "/shop/get_shop_list"
            | "/shop/get_all_shop_item_purchase_count"
            | "/shop/list"
            | "/chat/info"
            | "/chat/recent"
            | "/query/session"
            // Guild display reads only normalize daily/default state in place
            // (idempotent), never progress missions or spend resources.
            | "/guild/get_guildbasicinfo"
            | "/guild/get_all_guild_member_info"
    )
}

/// These operations change presentation/preferences but cannot advance missions
/// or alter the hero level/equipment relationship. Keep unknown mutations covered.
pub(crate) fn cosmetic(path: &str) -> bool {
    matches!(
        path,
        "/mission/sync_seen_categories"
            | "/hero/bookmark"
            | "/hero/bookmark_hero"
            | "/hero/set_hide_hero_unique_weapon"
            | "/hero/set_costume"
            | "/hero/unset_costume"
            | "/hero/save_costume_storage_slot"
            | "/hero/set_costume_storage_slot"
            | "/hero_storage_slot/change_hero_storage_slot_name"
            | "/item/set_lock_rune_item"
            | "/equip/set_lock_equip_item"
            | "/avatar/change"
            | "/user/change_avatar_hero"
    )
}

const SLOW_REQUEST_MS: f64 = 1000.0;

/// Measure application processing without buffering downloads or WebSockets.
/// Paths exclude query strings; request bodies and credentials are never logged.
pub(crate) async fn measure(
    request: axum::extract::Request,
    next: axum::middleware::Next,
) -> axum::response::Response {
    use axum::body::HttpBody;
    let path = request.uri().path().to_owned();
    let start = std::time::Instant::now();
    // Lets the database pool attribute long connection holds to this request.
    let mut response = crate::database::REQUEST_PATH
        .scope(std::sync::Arc::from(path.as_str()), next.run(request))
        .await;
    let ms = start.elapsed().as_secs_f64() * 1000.0;
    response
        .headers_mut()
        .insert("server-timing", format!("app;dur={ms:.3}").parse().unwrap());
    if ms >= SLOW_REQUEST_MS && !path.starts_with("/ws/") {
        // Includes waits for the player's or a guild's lock; pair with
        // "Waited long for a lock" warnings for the same path.
        tracing::warn!(target: "sprk_http", path, status=response.status().as_u16(),
            app_ms=ms, "Slow HTTP request");
    } else {
        tracing::debug!(target: "sprk_http", path, status=response.status().as_u16(),
            app_ms=ms, response_bytes=?response.body().size_hint().exact(), "HTTP request");
    }
    response
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn read_paths_and_cosmetics_never_exempt_claims_or_unknown_mutations() {
        for path in [
            "/ping",
            "/dispatch/get_dispatch_list",
            "/friend/list",
            "/mail/check_new_mail",
            "/guild/get_guildbasicinfo",
            "/guild/get_all_guild_member_info",
        ] {
            assert!(observation(path));
        }
        for path in ["/world_boss/get_world_boss_info", "/treasure_house/get_treasure_house_info", "/guild/get_guild_attendance"] {
            assert!(!observation(path), "{path} settles or grants state and must stay serialized");
        }
        for path in ["/hero/bookmark_hero", "/equip/set_lock_equip_item"] {
            assert!(cosmetic(path));
            assert!(!observation(path));
        }
        for path in [
            "/campaign/get_selected_reward",
            "/raid/get_multiplay_reward",
            "/match/wait_match",
            "/match/get_match_result",
            "/hero/buy_costume",
            "/hero/new_future_mutation",
        ] {
            assert!(!observation(path) && !cosmetic(path), "{path}");
        }
    }

    /// The client runs these requests concurrently with each other. Each must
    /// be a server read path so it carries no ordered progression snapshot.
    #[test]
    fn client_parallel_reads_are_server_observations() {
        let source = include_str!("../../../../client/tools/DllPatcher/Resources/NetworkPerformance.cs.txt");
        let start = source.find("parallelReads=new HashSet<string>").expect("client parallel read list");
        let block = &source[start..start + source[start..].find("};").unwrap()];
        let paths: Vec<&str> = block.split('"').skip(1).step_by(2).collect();
        assert!(paths.len() >= 10, "{paths:?}");
        for path in paths {
            assert!(observation(&format!("/{path}")), "client parallelizes non-observation path {path}");
        }
    }
}
