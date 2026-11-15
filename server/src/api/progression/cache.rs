use super::*;

pub(crate) struct Snapshot {
    /// (this account's revision, global revision).
    revision: (i64, i64),
    tables: usize,
    day: String,
    expires: i64,
    value: Value,
    /// The view and sections `value` was built from, for an incremental rebuild.
    built: Option<Built>,
}
impl std::fmt::Debug for Snapshot {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Snapshot").field("revision", &self.revision).field("day", &self.day)
            .field("expires", &self.expires).field("built", &self.built.is_some()).finish()
    }
}

/// What the recorded revision parts changed (see `state::part::of`); None when
/// one is unknown or none is recorded, so everything is rebuilt.
fn changes(parts: &[String]) -> Option<state::part::Invalidates> {
    if parts.is_empty() { return None; }
    let mut out = state::part::Invalidates::default();
    for name in parts {
        out.merge(state::part::of(name)?);
    }
    Some(out)
}

/// For a rebuild, in one statement: whether the reader can build the snapshot
/// (the account is initialized, as `item::initialized` checks, and today's
/// login is recorded), and with `since` (this account's and the global
/// revision) the progression parts changed after it: `table`, `table:kind`
/// or `progression_metrics:kind@day`, global ones prefixed `global:`, as
/// recorded by the triggers in migrations/0011_revision_parts.sql.
async fn rebuild_inputs(db: &mut PgConnection, state: &AppState, account: i64, since: Option<(i64, i64)>) -> Result<(bool, Option<Vec<String>>)> {
    let slots = state.tables.inventory.constant("CraftSlotCountMin", 2);
    let (ready, parts): (bool, Option<String>) = crate::db::query_as("SELECT EXISTS(SELECT 1 FROM inventory_settings WHERE account_id=$1) \
        AND (SELECT COUNT(*) FROM craft_slots WHERE account_id=$1 AND slot_index BETWEEN 1 AND $2)=$2 \
        AND EXISTS(SELECT 1 FROM progression_login WHERE account_id=$1 AND last_day=sprk_date('now')), \
        CASE WHEN $3 <> 0 THEN (SELECT COALESCE(json_agg(CASE WHEN account=0 THEN 'global:'||part ELSE part END),'[]')::text FROM response_revision_parts \
            WHERE scope='progression' AND ((account=$1 AND revision>$4) OR (account=0 AND revision>$5))) END")
        .bind(account).bind(slots).bind(since.is_some()).bind(since.unwrap_or_default().0).bind(since.unwrap_or_default().1)
        .fetch_one(db).await?;
    let parts = parts.map(|p| serde_json::from_str(&p)).transpose().map_err(|e| ServerError::Internal(e.to_string()))?;
    Ok((ready, parts))
}

// The lock is per session, never held through an endpoint or a network operation.
// The revision and data are read in the same transaction. Never publish a
// cache entry before commit, or label old data with a newer concurrent revision.
pub(crate) async fn get(state: &AppState, session_key: &str, account: i64) -> Result<Value> {
    let Some(session) = state.get_session(session_key).filter(|s| s.account_id == account) else {
        return login(state, account).await;
    };
    let mut cache = session.progression_cache.lock().await;
    // One read transaction: the revision and any rebuilt data come from the same
    // committed snapshot, so a concurrent commit can only make the entry stale.
    let mut tx = state.reads.begin().await?;
    let revision = crate::api::system::revisions::progression(&mut tx, account).await?;
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
    // A miss rebuilds on the reader: the snapshot only reads, and building it
    // under the single writer queued every other request behind it (~300 ms
    // per mutation in production). Only the initializers below need the writer.
    let since = cache.as_ref().filter(|c| c.tables == tables && c.day == day && now < c.expires && c.built.is_some()).map(|c| c.revision);
    let (ready, parts) = rebuild_inputs(&mut tx, state, account, since).await?;
    if ready {
        // Same tables and day, within the entry's validity: reload only the
        // changed parts and re-evaluate only the sections that read them. The
        // entry keeps its expiry, so reused sections are never older than a
        // full rebuild would allow.
        // The entry is consumed: its sections' values move into the new
        // snapshot. On an error the cache is left empty (a full rebuild).
        let incremental = match (cache.take().filter(|c| Some(c.revision) == since && c.built.is_some()), parts) {
            (Some(entry), Some(parts)) => changes(&parts).map(|c| (c, entry)),
            _ => None,
        };
        let (value, built, expires) = match incremental {
            Some((changes, Snapshot { expires, value: previous, built: Some(Built { view, sections }), .. })) => {
                let started = std::time::Instant::now();
                let (view, changed) = View::refresh(&mut tx, account, Some((view, &changes))).await?;
                let refreshed = started.elapsed();
                let before = super::EVALUATED.with(|c| c.get());
                let (value, built) = build(state, view, Some((sections, previous, &changed)));
                tracing::debug!(target: "sprk_progression", account, refresh_ms = refreshed.as_secs_f64() * 1000.0,
                    build_ms = (started.elapsed() - refreshed).as_secs_f64() * 1000.0, parts = changes.mask,
                    changed = changed.mask, metrics = changed.metrics.len(), heroes = changed.heroes.len(),
                    stages = changed.stages.len(), evaluated = super::EVALUATED.with(|c| c.get()) - before,
                    "Incremental progression snapshot");
                #[cfg(test)]
                verify_incremental(&mut tx, state, account, &value).await;
                (value, built, expires)
            }
            _ => {
                let view = View::load(&mut tx, account).await?;
                let (value, built) = build(state, view, None);
                (value, built, expiry(state, now))
            }
        };
        tx.commit().await?;
        *cache = Some(Snapshot { revision, tables, day, expires, value: value.clone(), built: Some(built) });
        return Ok(value);
    }
    // End the read transaction before taking the account lock in login().
    // Its initializers intentionally acquire that lock before reading progress.
    tx.commit().await?;
    let mut tx = crate::database::begin(&state.db).await?;
    item::init(&mut tx, state, account).await?;
    touch_day(&mut tx, account).await?;
    let view = View::load(&mut tx, account).await?;
    let (value, built) = build(state, view, None);
    let revision = crate::api::system::revisions::progression(&mut tx, account).await?;
    tx.commit().await?;
    let expires = expiry(state, now);
    *cache = Some(Snapshot { revision, tables, day, expires, value: value.clone(), built: Some(built) });
    Ok(value)
}

/// Tests: every incremental rebuild must equal a full one from the same
/// snapshot (play record UpdatedTime is the observation time).
#[cfg(test)]
async fn verify_incremental(tx: &mut PgConnection, state: &AppState, account: i64, value: &Value) {
    if std::env::var("SPRK_PROGRESSION_VERIFY").is_ok_and(|v| v == "0") { return; }
    let strip = |mut v: Value| {
        for record in v["PlayRecordInfos"].as_array_mut().into_iter().flatten() { record.as_object_mut().unwrap().remove("UpdatedTime"); }
        v
    };
    // The full rebuild is not part of the incremental one's work.
    let evaluated = super::EVALUATED.with(|c| c.get());
    let full = strip(snapshot(tx, state, account).await.unwrap());
    super::EVALUATED.with(|c| c.set(evaluated));
    let value = strip(value.clone());
    for (key, expected) in full.as_object().unwrap() {
        assert_eq!(&value[key], expected, "incremental progression snapshot differs in {key}");
    }
    assert_eq!(value, full);
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

    /// A trigger recording a part the cache cannot map makes every rebuild a
    /// full one. Cover every watched table and every part the script records.
    #[test]
    fn every_recorded_revision_part_maps_to_view_parts() {
        let migration = include_str!("../../../migrations/0009_response_revisions.sql");
        let kinds = ["arena_runs", "battle_state", "community_claims", "extension_state", "guild_battle_scores", "play_record_values", "progression_metrics"];
        let mut tables = 0;
        for line in migration.lines().filter(|l| l.trim_start().starts_with("('progression', '")) {
            let table = line.split('\'').nth(3).unwrap();
            let name = match table {
                "community_state" => "community_state:guild_conquest_history".to_owned(),
                "credentials" => "global:credentials".to_owned(),
                t if kinds.contains(&t) => format!("{t}:any"),
                t => t.to_owned(),
            };
            assert!(state::part::of(&name).is_some(), "{name} is not mapped");
            tables += 1;
        }
        assert!(tables >= 30, "{tables}");
        let fixture = include_str!("../../../tests/fixtures/trigger_rows.txt").replace("\r\n", "\n");
        let section = fixture.split("## response_revision_parts\n").nth(1).unwrap().split("\n## ").next().unwrap();
        for line in section.lines() {
            let fields: Vec<&str> = line.split('|').collect();
            let part = fields[2].trim_start_matches("Some(\"").trim_end_matches("\")");
            let name = if fields[1] == "Some(0)" { format!("global:{part}") } else { part.to_owned() };
            assert!(state::part::of(&name).is_some(), "{name} is not mapped");
        }
        assert!(state::part::of("unknown_table").is_none());
    }

    /// Each kind of write leaves the incremental snapshot equal to a full one
    /// (checked inside `get` in tests), and re-evaluates only the sections
    /// that read what changed.
    #[tokio::test]
    async fn incremental_snapshots_match_full_rebuilds_and_reuse_unaffected_sections() {
        let db = crate::database::test_pool().await;
        let state = AppState::new(db, crate::tables::test_tables());
        let user = crate::api::account::user::test_login(State(state.clone()), axum::body::Bytes::from_static(b"LoginId=incremental-test")).await.unwrap().0;
        let (account, key) = (user.user_info.account_id, user.user_info.session_key);
        get(&state, &key, account).await.unwrap();
        let total = state::mission_groups(&state).achievements.len() + state::mission_groups(&state).sub_quests.len() + 1;
        let evaluated = || super::super::EVALUATED.with(|c| c.replace(0));
        let other: i64 = crate::db::query_scalar("INSERT INTO accounts(login_id) VALUES('incremental-other') RETURNING account_id").fetch_one(&state.db).await.unwrap();
        for (label, sql) in [
            ("metric", "INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES($1,sprk_date('now'),'ClearDungeon',1,1,3)"),
            ("unread metric", "INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES($1,sprk_date('now'),'SprkUnreadMetric',0,0,1)"),
            // Other days of a kind: reloaded by day, folded into the whole kind.
            ("older metric day", "INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES($1,'2020-01-02','ClearDungeon',1,1,2)"),
            ("metric today again", "INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES($1,sprk_date('now'),'ClearDungeon',1,1,3) ON CONFLICT(account_id,day,kind,arg,arg2) DO UPDATE SET value=progression_metrics.value+1"),
            ("deleted metric day", "DELETE FROM progression_metrics WHERE account_id=$1 AND day='2020-01-02'"),
            // Battles change flask progress, which no mission reads.
            ("unread hero flask", "INSERT INTO hero_details(account_id,hero_index,data) SELECT account_id,hero_index,'{\"FlaskExp\":5}' FROM heroes WHERE account_id=$1 \
                ON CONFLICT(account_id,hero_index) DO UPDATE SET data=(hero_details.data::jsonb || jsonb_build_object('FlaskExp',7))::text"),
            ("claim", "INSERT INTO progression_claims(account_id,family,idx,step,period,claimed_time) VALUES($1,'achievement',1,1,'all',sprk_datetime('now'))"),
            ("subquest claim", "INSERT INTO progression_claims(account_id,family,idx,step,period,claimed_time) VALUES($1,'subquest',1,1,'all',sprk_datetime('now'))"),
            ("item", "INSERT INTO items(account_id,item_index,count) VALUES($1,1,5) ON CONFLICT(account_id,item_index) DO UPDATE SET count=items.count+5"),
            ("level", "UPDATE user_info SET team_level=team_level+1 WHERE account_id=$1"),
            ("hero", "UPDATE heroes SET level=level+1 WHERE account_id=$1"),
            ("dungeon", "INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES($1,1,1,1,3) ON CONFLICT(account_id,chapter_id,dungeon_id) DO UPDATE SET clear_count=campaign_progress.clear_count+1"),
            ("unread extension kind", "INSERT INTO extension_state(account_id,kind,idx,data) VALUES($1,'stamina_recharges',1,'{\"Count\":1}') ON CONFLICT(account_id,kind,idx) DO UPDATE SET data='{\"Count\":2}'"),
            ("pet", "INSERT INTO extension_state(account_id,kind,idx,data) VALUES($1,'pet_misc',6,'{\"MiscValue\":2}') ON CONFLICT(account_id,kind,idx) DO UPDATE SET data='{\"MiscValue\":3}'"),
            ("play record", "INSERT INTO play_record_values(account_id,kind,idx,value) VALUES($1,'arena_wins',0,4) ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=play_record_values.value+1"),
            ("eclipse", "INSERT INTO battle_state(account,kind,idx,data) VALUES($1,'eclipse_record',0,'{\"BestWave\":7}') ON CONFLICT(account,kind,idx) DO UPDATE SET data='{\"BestWave\":8}'"),
            ("login", "UPDATE progression_login SET days=days+1 WHERE account_id=$1"),
            ("entitlement", "INSERT INTO progression_entitlements(account_id,product_index,created_time) VALUES($1,1,sprk_datetime('now')) ON CONFLICT DO NOTHING"),
        ] {
            crate::db::query(sql).bind(account).execute(&state.db).await.unwrap_or_else(|e| panic!("{label}: {e}"));
            evaluated();
            get(&state, &key, account).await.unwrap_or_else(|e| panic!("{label}: {e}"));
            let count = evaluated();
            println!("{label}: {count} of {total} sections re-evaluated");
            // Every achievement reports ClearTeamLevel; team level changes rarely.
            let limit = if label == "level" { total } else { total / 4 };
            assert!(count < limit, "{label}: {count} of {total} sections re-evaluated");
            if label.starts_with("unread") {
                assert_eq!(count, 0, "{label} must not re-evaluate any section");
            }
        }
        // A friendship written by another player and a global world boss score.
        crate::db::query("INSERT INTO friends(account_id,friend_account_id,status) VALUES($1,$2,'accepted')").bind(other).bind(account).execute(&state.db).await.unwrap();
        get(&state, &key, account).await.unwrap();
        crate::db::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',1,1,$1,sprk_date('now'),10)").bind(other).execute(&state.db).await.unwrap();
        get(&state, &key, account).await.unwrap();
    }
    /// A metric part names its day, so a snapshot reloads only that day; the
    /// first change of a kind on another day folds its earlier days into the
    /// whole-kind part with their latest revision, bounding the parts kept.
    #[tokio::test]
    async fn metric_parts_name_their_day_and_fold_earlier_days() {
        let db = crate::database::test_pool().await;
        let a: i64 = crate::db::query_scalar("INSERT INTO accounts(login_id) VALUES('metric-parts') RETURNING account_id").fetch_one(&db).await.unwrap();
        let parts = || async {
            crate::db::query_as::<(String, i64)>("SELECT part,revision FROM response_revision_parts WHERE scope='progression' AND account=$1 AND left(part,20)='progression_metrics:' ORDER BY part")
                .bind(a).fetch_all(&db).await.unwrap()
        };
        let metric = |day: &'static str, kind: &'static str| crate::db::query("INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES($1,$2,$3,0,0,1) \
            ON CONFLICT(account_id,day,kind,arg,arg2) DO UPDATE SET value=progression_metrics.value+1").bind(a).bind(day).bind(kind).execute(&db);
        metric("2020-01-01", "ClearDungeon").await.unwrap();
        metric("2020-01-01", "ClearDungeon").await.unwrap();
        let first = parts().await;
        assert_eq!(first.iter().map(|p| p.0.as_str()).collect::<Vec<_>>(), ["progression_metrics:ClearDungeon@2020-01-01"]);
        metric("2020-01-02", "ClearDungeon").await.unwrap();
        metric("2020-01-01", "AddItem").await.unwrap();
        let folded = parts().await;
        assert_eq!(folded.iter().map(|p| p.0.as_str()).collect::<Vec<_>>(),
            ["progression_metrics:AddItem@2020-01-01", "progression_metrics:ClearDungeon", "progression_metrics:ClearDungeon@2020-01-02"]);
        assert_eq!(folded[1].1, first[0].1, "the whole-kind part keeps the folded day's latest revision");
        assert!(folded[2].1 > folded[1].1);
        // A day part reloads that day; a whole-kind part every day of the kind.
        let days = |name: &str| state::part::of(name).unwrap().metrics.into_iter().collect::<Vec<_>>();
        assert_eq!(days("progression_metrics:ClearDungeon@2020-01-02"), [("ClearDungeon".to_owned(), Some(["2020-01-02".to_owned()].into()))]);
        assert_eq!(days("progression_metrics:ClearDungeon"), [("ClearDungeon".to_owned(), None)]);
        let mut both = state::part::of("progression_metrics:ClearDungeon@2020-01-02").unwrap();
        both.merge(state::part::of("progression_metrics:ClearDungeon").unwrap());
        assert_eq!(both.metrics["ClearDungeon"], None, "a whole-kind change covers every day");
    }
    #[tokio::test]
    async fn cached_state_invalidates_for_commits_rollbacks_other_accounts_and_resets() {
        let db = crate::database::test_pool().await;
        let state = AppState::new(db, crate::tables::test_tables());
        let user = crate::api::account::user::test_login(State(state.clone()), axum::body::Bytes::from_static(b"LoginId=cache-test")).await.unwrap().0;
        let (account, key) = (user.user_info.account_id, user.user_info.session_key);
        let first = get(&state, &key, account).await.unwrap();
        let session = state.get_session(&key).unwrap();
        let revision = session.progression_cache.lock().await.as_ref().unwrap().revision;
        // No-op writes must not invalidate or create writer work on cache hits.
        crate::db::query("UPDATE user_info SET team_level=team_level WHERE account_id=$1").bind(account).execute(&state.db).await.unwrap();
        let before: i64 = crate::database::row_changes(&state.db).await;
        let repeated = get(&state, &key, account).await.unwrap();
        assert_eq!(first["AchievementInfos"], repeated["AchievementInfos"]);
        assert_eq!(crate::database::row_changes(&state.db).await, before);
        assert_eq!(session.progression_cache.lock().await.as_ref().unwrap().revision, revision);
        let start=std::time::Instant::now();
        for _ in 0..5 { login(&state, account).await.unwrap(); }
        let uncached=start.elapsed();
        let start=std::time::Instant::now();
        for _ in 0..5 { get(&state, &key, account).await.unwrap(); }
        println!("Progression debug fixture, 5 requests: full snapshot={:.2}ms, cached={:.2}ms",uncached.as_secs_f64()*1000.0,start.elapsed().as_secs_f64()*1000.0);
        let mut tx = crate::database::begin(&state.db).await.unwrap();
        crate::db::query("UPDATE user_info SET team_level=10 WHERE account_id=$1").bind(account).execute(&mut *tx).await.unwrap();
        tx.rollback().await.unwrap();
        get(&state, &key, account).await.unwrap();
        assert_eq!(session.progression_cache.lock().await.as_ref().unwrap().revision, revision);
        crate::db::query("UPDATE user_info SET team_level=10 WHERE account_id=$1").bind(account).execute(&state.db).await.unwrap();
        let changed = get(&state, &key, account).await.unwrap();
        assert_ne!(first["AchievementInfos"], changed["AchievementInfos"]);
        let changed_revision = session.progression_cache.lock().await.as_ref().unwrap().revision;
        assert!(changed_revision > revision);
        // Another player's own progress must not invalidate this player's cache.
        let other: i64 = crate::db::query_scalar("INSERT INTO accounts(login_id) VALUES('other-cache-account') RETURNING account_id").fetch_one(&state.db).await.unwrap();
        crate::db::query("INSERT INTO items(account_id,item_index,count) VALUES($1,1,5)").bind(other).execute(&state.db).await.unwrap();
        crate::db::query("INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES($1,sprk_date('now'),'AddEquip',1,0,1)").bind(other).execute(&state.db).await.unwrap();
        crate::db::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('raid',1,1,$1,sprk_date('now'),10)").bind(other).execute(&state.db).await.unwrap();
        get(&state, &key, account).await.unwrap();
        assert_eq!(session.progression_cache.lock().await.as_ref().unwrap().revision, changed_revision);
        // Cross-account ranking/friendship dependencies cannot reuse old data.
        crate::db::query("INSERT INTO friends(account_id,friend_account_id,status) VALUES($1,$2,'accepted')").bind(other).bind(account).execute(&state.db).await.unwrap();
        get(&state, &key, account).await.unwrap();
        let befriended = session.progression_cache.lock().await.as_ref().unwrap().revision;
        assert!(befriended.0 > changed_revision.0 && befriended.1 == changed_revision.1);
        crate::db::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',1,1,$1,sprk_date('now'),10)").bind(other).execute(&state.db).await.unwrap();
        get(&state, &key, account).await.unwrap();
        let ranked = session.progression_cache.lock().await.as_ref().unwrap().revision;
        assert!(ranked.0 == befriended.0 && ranked.1 > befriended.1);
        // A friendship row moving to a different player bumps both old and new owners.
        let third: i64 = crate::db::query_scalar("INSERT INTO accounts(login_id) VALUES('third-cache-account') RETURNING account_id").fetch_one(&state.db).await.unwrap();
        crate::db::query("UPDATE friends SET friend_account_id=$1 WHERE account_id=$2").bind(third).bind(other).execute(&state.db).await.unwrap();
        get(&state, &key, account).await.unwrap();
        assert!(session.progression_cache.lock().await.as_ref().unwrap().revision.0 > ranked.0);
        assert_eq!(crate::api::system::revisions::get(&mut state.db.acquire().await.unwrap(), "progression", third).await.unwrap(), 2);
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
