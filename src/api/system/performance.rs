//! Read paths that cannot advance missions or change hero-scaled equipment.
//! Deliberately explicit: a `get_*` endpoint can claim rewards or settle battles.
pub(crate) fn observation(path: &str) -> bool {
    matches!(
        path,
        "/ping"
            | "/ping/idle"
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
    )
}

/// These operations change presentation/preferences but cannot advance missions
/// or alter the hero level/equipment relationship. Keep unknown mutations covered.
pub(crate) fn cosmetic(path: &str) -> bool {
    matches!(
        path,
        "/hero/bookmark"
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

/// Measure application processing without buffering downloads or WebSockets.
/// Paths exclude query strings; request bodies and credentials are never logged.
pub(crate) async fn measure(
    request: axum::extract::Request,
    next: axum::middleware::Next,
) -> axum::response::Response {
    use axum::body::HttpBody;
    let path = request.uri().path().to_owned();
    let start = std::time::Instant::now();
    let mut response = next.run(request).await;
    let ms = start.elapsed().as_secs_f64() * 1000.0;
    response
        .headers_mut()
        .insert("server-timing", format!("app;dur={ms:.3}").parse().unwrap());
    tracing::debug!(target: "sprk_http", path, status=response.status().as_u16(),
        app_ms=ms, response_bytes=?response.body().size_hint().exact(), "HTTP request");
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
        ] {
            assert!(observation(path));
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
}
