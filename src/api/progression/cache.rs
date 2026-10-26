use super::*;

#[derive(Debug)]
pub(crate) struct Snapshot {
    revision: i64,
    tables: usize,
    day: String,
    expires: i64,
    value: Value,
}

// The lock is per session, never held through an endpoint or a network operation.
// The revision and data are read in the same SQLite transaction. Never publish a
// cache entry before commit, or label old data with a newer concurrent revision.
pub(crate) async fn get(state: &AppState, session_key: &str, account: i64) -> Result<Value> {
    let Some(session) = state.get_session(session_key).filter(|s| s.account_id == account) else {
        return login(state, account).await;
    };
    let mut cache = session.progression_cache.lock().await;
    let mut tx = state.db.begin().await?;
    let revision = crate::api::system::revisions::get(&mut tx, "progression", 0).await?;
    let now = state.server_time();
    let day = state.server_date();
    let tables = std::sync::Arc::as_ptr(&state.tables) as usize;
    if let Some(entry) = cache.as_ref().filter(|c| c.revision == revision && c.tables == tables && c.day == day && now < c.expires) {
        let mut value = entry.value.clone();
        // UpdatedTime is the observation time, not an invalidation dependency.
        for record in value["PlayRecordInfos"].as_array_mut().into_iter().flatten() {
            record["UpdatedTime"] = json!(state.server_time_str());
        }
        tx.commit().await?;
        return Ok(value);
    }
    // End the read transaction before taking SQLite's write lock in login().
    // Its initializers intentionally acquire that lock before reading progress.
    tx.commit().await?;
    let mut tx = state.db.begin().await?;
    item::init(&mut tx, state, account).await?;
    touch_day(&mut tx, account).await?;
    let value = snapshot(&mut tx, state, account).await?;
    let revision = crate::api::system::revisions::get(&mut tx, "progression", 0).await?;
    tx.commit().await?;
    let expires = expiry(state, now);
    *cache = Some(Snapshot { revision, tables, day, expires, value: value.clone() });
    Ok(value)
}

fn expiry(state: &AppState, now: i64) -> i64 {
    let mut expires = now + 60;
    for calendar in &state.tables.progression.calendars {
        for key in ["StartDate", "EndDate"] {
            if let Some(at) = calendar[key].as_str().and_then(|s| chrono::NaiveDateTime::parse_from_str(s, "%Y-%m-%d %H:%M:%S").ok()).map(|d| d.and_utc().timestamp()) {
                if at > now { expires = expires.min(at); }
            }
        }
    }
    expires
}

#[cfg(test)]
mod tests {
    use super::*;
    #[tokio::test]
    async fn cached_state_invalidates_for_commits_rollbacks_other_accounts_and_resets() {
        let db = sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
        crate::database::create_tables(&db).await.unwrap();
        let state = AppState::new(db, crate::tables::test_tables());
        let user = crate::api::account::user::test_login(State(state.clone()), axum::body::Bytes::from_static(b"LoginId=cache-test")).await.unwrap().0;
        let (account, key) = (user.user_info.account_id, user.user_info.session_key);
        let first = get(&state, &key, account).await.unwrap();
        let session = state.get_session(&key).unwrap();
        let revision = session.progression_cache.lock().await.as_ref().unwrap().revision;
        // No-op writes must not invalidate or create writer work on cache hits.
        sqlx::query("UPDATE user_info SET team_level=team_level WHERE account_id=?").bind(account).execute(&state.db).await.unwrap();
        let before: i64 = sqlx::query_scalar("SELECT total_changes()").fetch_one(&state.db).await.unwrap();
        let repeated = get(&state, &key, account).await.unwrap();
        assert_eq!(first["AchievementInfos"], repeated["AchievementInfos"]);
        assert_eq!(sqlx::query_scalar::<_, i64>("SELECT total_changes()").fetch_one(&state.db).await.unwrap(), before);
        assert_eq!(session.progression_cache.lock().await.as_ref().unwrap().revision, revision);
        let start=std::time::Instant::now();
        for _ in 0..5 { login(&state, account).await.unwrap(); }
        let uncached=start.elapsed();
        let start=std::time::Instant::now();
        for _ in 0..5 { get(&state, &key, account).await.unwrap(); }
        println!("Progression debug fixture, 5 requests: full snapshot={:.2}ms, cached={:.2}ms",uncached.as_secs_f64()*1000.0,start.elapsed().as_secs_f64()*1000.0);
        let mut tx = state.db.begin().await.unwrap();
        sqlx::query("UPDATE user_info SET team_level=10 WHERE account_id=?").bind(account).execute(&mut *tx).await.unwrap();
        tx.rollback().await.unwrap();
        get(&state, &key, account).await.unwrap();
        assert_eq!(session.progression_cache.lock().await.as_ref().unwrap().revision, revision);
        sqlx::query("UPDATE user_info SET team_level=10 WHERE account_id=?").bind(account).execute(&state.db).await.unwrap();
        let changed = get(&state, &key, account).await.unwrap();
        assert_ne!(first["AchievementInfos"], changed["AchievementInfos"]);
        let changed_revision = session.progression_cache.lock().await.as_ref().unwrap().revision;
        assert!(changed_revision > revision);
        // Cross-account ranking/friendship dependencies cannot reuse old data.
        sqlx::query("INSERT INTO accounts(login_id) VALUES('other-cache-account')").execute(&state.db).await.unwrap();
        get(&state, &key, account).await.unwrap();
        assert!(session.progression_cache.lock().await.as_ref().unwrap().revision > changed_revision);
        // Expiry/date checks are independent of database writes.
        session.progression_cache.lock().await.as_mut().unwrap().day = "2000-01-01".into();
        get(&state, &key, account).await.unwrap();
        assert_eq!(session.progression_cache.lock().await.as_ref().unwrap().day, state.server_date());
        session.progression_cache.lock().await.as_mut().unwrap().expires = 0;
        get(&state, &key, account).await.unwrap();
        assert!(session.progression_cache.lock().await.as_ref().unwrap().expires > state.server_time());
        let mut boundary = state.clone();
        let at = state.server_time() + 2;
        std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut boundary.tables).progression).calendars.push(json!({"StartDate":chrono::DateTime::from_timestamp(at, 0).unwrap().format("%Y-%m-%d %H:%M:%S").to_string()}));
        assert_eq!(expiry(&boundary, state.server_time()), at);
    }
}
