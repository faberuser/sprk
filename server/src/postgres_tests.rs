//! Tests of the PostgreSQL foundation: migrations, triggers, constraints, the
//! account lock, the retry wrapper and the read pool.
use super::*;
use sqlx::{AssertSqlSafe, Row};

struct TestDb {
    pool: PgPool,
}

impl TestDb {
    async fn new() -> Option<Self> {
        Some(Self { pool: crate::database::test_pool().await })
    }

    async fn drop(self) {
        self.pool.close().await;
    }
}

fn sqlstate(error: &sqlx::Error) -> Option<String> {
    error.as_database_error().and_then(|e| e.code()).map(|c| c.into_owned())
}

#[tokio::test]
async fn rerunning_migrations_is_a_no_op() {
    let Some(db) = TestDb::new().await else { return };
    migrate(&db.pool).await.unwrap();
    db.drop().await;
}

#[tokio::test]
async fn currency_constraints_reject_invalid_balances() {
    let Some(db) = TestDb::new().await else { return };
    let p = &db.pool;
    sqlx::query("INSERT INTO accounts(account_id,login_id) VALUES(1,'a')").execute(p).await.unwrap();
    sqlx::query("INSERT INTO user_info(account_id,gold) VALUES(1,10)").execute(p).await.unwrap();
    for (sql, code) in [
        ("UPDATE user_info SET gold=-1 WHERE account_id=1", "23514"),
        ("UPDATE user_info SET gem=9223372036854775807, pay_gem=1 WHERE account_id=1", "23514"),
        ("UPDATE user_info SET gold=gold+9223372036854775807 WHERE account_id=1", "22003"),
        ("INSERT INTO battle_currencies(account,kind,value) VALUES(1,'x',-1)", "23514"),
    ] {
        let error = sqlx::query(sql).execute(p).await.expect_err(sql);
        assert_eq!(sqlstate(&error).as_deref(), Some(code), "{sql}");
    }
    let gold: i64 = sqlx::query_scalar("SELECT gold FROM user_info WHERE account_id=1").fetch_one(p).await.unwrap();
    assert_eq!(gold, 10);
    db.drop().await;
}

/// Writes that exercise every gameplay trigger. The derived rows they leave
/// behind are recorded in tests/fixtures/trigger_rows.txt; they matched the
/// original SQLite triggers when the triggers were ported. To change them on
/// purpose, run this test with SPRK_RECORD_TRIGGER_ROWS=1 and review the diff.
fn trigger_script() -> Vec<String> {
    let mut script: Vec<String> = [
        "INSERT INTO accounts(account_id,login_id,nick) VALUES(1,'a','A'),(2,'b','B')",
        "INSERT INTO user_info(account_id,gold,gem,pay_gem,stamina) VALUES(1,100,50,10,200),(2,0,0,0,200)",
        "UPDATE user_info SET gold=40 WHERE account_id=1",
        "UPDATE user_info SET gold=90,stamina=190,gem=45 WHERE account_id=1",
        "UPDATE user_info SET pay_gem=5,team_level=2 WHERE account_id=1",
        "INSERT INTO items(account_id,item_index,count) VALUES(1,500,3)",
        "UPDATE items SET count=5 WHERE account_id=1 AND item_index=500",
        "UPDATE items SET count=1 WHERE account_id=1 AND item_index=500",
        "INSERT INTO equip_items(account_id,item_index,star) VALUES(1,1001,3),(1,1001,2),(2,1002,1)",
        "UPDATE equip_items SET star=5 WHERE account_id=1 AND star=2",
        "UPDATE equip_items SET locked=1 WHERE account_id=2",
        "INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count) VALUES(1,1,2,1),(1,1,3,0)",
        "UPDATE campaign_progress SET clear_count=4 WHERE account_id=1",
        r#"INSERT INTO extension_state(account_id,kind,idx,data) VALUES(1,'soul',0,'{"Grade":3,"ReinforceLevel":7}'),(1,'other',0,'{"Grade":9}')"#,
        r#"UPDATE extension_state SET data='{"Grade":2,"ReinforceLevel":9}' WHERE account_id=1 AND kind='soul'"#,
        "INSERT INTO arena_scores(account,kind,season,score,wins,losses,data) VALUES(1,0,1,100,0,0,'{}'),(1,1,1,100,0,0,'{}')",
        r#"UPDATE arena_scores SET wins=2,score=150,data='{"SuccessiveWin":2}' WHERE account=1"#,
        "UPDATE arena_scores SET losses=1 WHERE account=1",
        "INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',3,1,1,'2026-11-10',10),('raid',3,1,1,'2026-11-10',10),('world_boss',4,1,2,'2026-11-10',0)",
        "UPDATE battle_scores SET score=20 WHERE account=1",
        "UPDATE battle_scores SET score=15 WHERE account=1 AND family='world_boss'",
        "INSERT INTO friends(account_id,friend_account_id,status) VALUES(1,2,'pending'),(2,1,'accepted')",
        "INSERT INTO friend_points(account_id,friend_id) VALUES(1,2)",
        "UPDATE friend_points SET last_send_time='2026-11-10 10:00:00' WHERE account_id=1",
        "UPDATE friend_points SET last_send_time='2026-11-10 10:00:00' WHERE account_id=1",
        "UPDATE friend_points SET last_send_time='2026-11-11 10:00:00' WHERE account_id=1",
        "INSERT INTO hero_friendly_info(account_id,hero_index) VALUES(1,7)",
        "UPDATE hero_friendly_info SET last_greeting_time='2026-11-10 10:00:00',last_gift_time='2026-11-10 10:00:00' WHERE account_id=1",
        "UPDATE hero_friendly_info SET last_conversation_time='2026-11-10 11:00:00' WHERE account_id=1",
        "INSERT INTO hero_friendly_state(account_id,hero_index) VALUES(1,7)",
        "UPDATE hero_friendly_state SET friendly_point=5 WHERE account_id=1",
        "UPDATE hero_friendly_state SET friendly_point=2 WHERE account_id=1",
        "INSERT INTO hero_inn_roulette_spins(account_id,spin_date,spin_count) VALUES(1,'2026-11-10',1)",
        "UPDATE hero_inn_roulette_spins SET spin_count=3 WHERE account_id=1",
        "INSERT INTO mails(account_id,title) VALUES(1,'hi')",
        "INSERT INTO mails(account_id,title,global_id) VALUES(1,'g',9)",
        "UPDATE mails SET is_received=1 WHERE account_id=1",
        "UPDATE mails SET is_read=1 WHERE account_id=1",
        "INSERT INTO shop_purchase_ledger(account_id,shop_index,item_index,period,purchased) VALUES(1,4,5,'all',1)",
        "UPDATE shop_purchase_ledger SET purchased=3 WHERE account_id=1",
        "INSERT INTO shop_stock(account_id,shop_index) VALUES(1,4)",
        "UPDATE shop_stock SET restock_count=1 WHERE account_id=1",
        r#"INSERT INTO community_state(owner,kind,idx,data) VALUES(5,'guild_conquest_history',1,'{"AccountId":2}'),(5,'guild_other',1,'{"AccountId":1}')"#,
        "INSERT INTO credentials(username,login_id,password_hash,created_at) VALUES('u','a','h',0)",
    ].into_iter().map(String::from).collect();

    // Arena: enough finished runs per kind to trigger retention, with duels,
    // squads, canceled matches, missing seasons and a run finished by update.
    // 31 runs per kind; retention keeps the 20 most recent plus exceptions.
    for i in 0..62i64 {
        let kind = i % 2;
        let duel = if i % 7 == 3 { 1 } else { 0 };
        let heroes = if i % 5 == 0 { 5 } else { 1 + i % 4 };
        let heroes = (0..heroes).map(|h| h.to_string()).collect::<Vec<_>>().join(",");
        let season = if i % 11 == 10 { String::new() } else { format!(r#""Season":{},"#, 1 + i / 12) };
        let status = if i % 9 == 4 { "canceled" } else { "complete" };
        let data = format!(
            r#"{{{season}"DuelType":{duel},"Heroes":[{heroes}],"OnlineMatchUid":{i},"CompletedAt":{},"End":{{"MatchResult":{{"Win":{},"Canceled":{},"NewTierIndex":{}}},"BattleInfo":{{"SuccessiveWin":{}}},"BattleBanPickInfo":{{"SuccessiveWin":{}}}}}}}"#,
            1_762_000_000 + i * 40_000, i % 3 != 0, i % 13 == 6, i % 6, i % 4, i % 5);
        script.push(format!("INSERT INTO arena_runs(account,kind,started,status,data) VALUES(1,{kind},{},'{status}','{data}')", 1_762_000_000 + i * 40_000));
    }
    script.push(r#"INSERT INTO arena_runs(account,kind,started,status,data) VALUES(1,0,1763400000,'waiting','{"Season":3,"Heroes":[1]}')"#.into());
    script.push("UPDATE arena_runs SET last_poll=5 WHERE status='waiting'".into());
    script.push(r#"UPDATE arena_runs SET status='complete',data='{"Season":3,"Heroes":[1],"CompletedAt":1763400100,"End":{"MatchResult":{"Win":true,"NewTierIndex":2}}}' WHERE status='waiting'"#.into());
    for run in [1, 20, 40] {
        script.push(format!(r#"INSERT INTO arena_snapshots(run,data) VALUES({run},'{{}}')"#));
    }
    script
}

/// Derived rows checked against the recording.
const DERIVED: &[(&str, &str)] = &[
    ("progression_metrics", "SELECT account_id,day,kind,arg,arg2,value FROM progression_metrics ORDER BY 1,2,3,4,5"),
    ("play_record_values", "SELECT account_id,kind,idx,value FROM play_record_values ORDER BY 1,2,3"),
    ("mail_notifications", "SELECT mail_id,account_id,is_global FROM mail_notifications ORDER BY 1"),
    ("arena_history_archive", "SELECT account,kind,day,matches,wins,best_streak,best_tier FROM arena_history_archive ORDER BY 1,2,3"),
    ("arena_runs", "SELECT id,kind,status,season,nonduel,squad FROM arena_runs ORDER BY id"),
    ("arena_snapshots", "SELECT run FROM arena_snapshots ORDER BY run"),
    ("response_revisions keys", "SELECT scope,account FROM response_revisions ORDER BY 1,2"),
];

fn row_strings(rows: Vec<sqlx::postgres::PgRow>) -> Vec<String> {
    rows.iter().map(|row| (0..row.len()).map(|i| {
        row.try_get::<Option<i64>, _>(i).map(|v| format!("{v:?}"))
            .or_else(|_| row.try_get::<Option<String>, _>(i).map(|v| format!("{v:?}")))
            .unwrap_or_else(|e| panic!("column {i}: {e}"))
    }).collect::<Vec<_>>().join("|")).collect()
}

/// Every DERIVED query's rows plus the parsed arena mission summaries, as text.
async fn derived_rows(pool: &PgPool) -> String {
    let mut out = String::new();
    for (name, query) in DERIVED {
        let rows = row_strings(sqlx::query(AssertSqlSafe(*query)).fetch_all(pool).await.unwrap());
        assert!(!rows.is_empty(), "{name}: script produced no rows");
        out += &format!("## {name}\n{}\n", rows.join("\n"));
    }
    out += "## arena_runs.mission_summary\n";
    let summaries: Vec<(i64, Option<String>)> =
        sqlx::query_as("SELECT id, mission_summary FROM arena_runs ORDER BY id").fetch_all(pool).await.unwrap();
    for (id, summary) in summaries {
        let summary = summary.map(|s| serde_json::from_str::<serde_json::Value>(&s).unwrap().to_string());
        out += &format!("{id}|{summary:?}\n");
    }
    // Metrics are dated by the current UTC day.
    let today: String = sqlx::query_scalar("SELECT sprk_utc_day()").fetch_one(pool).await.unwrap();
    out.replace(&today, "TODAY")
}

#[tokio::test]
async fn triggers_derive_the_recorded_rows() {
    let Some(db) = TestDb::new().await else { return };
    for statement in trigger_script() {
        sqlx::query(AssertSqlSafe(statement.clone())).execute(&db.pool).await
            .unwrap_or_else(|e| panic!("{statement}: {e}"));
    }
    let actual = derived_rows(&db.pool).await;
    if std::env::var_os("SPRK_RECORD_TRIGGER_ROWS").is_some() {
        std::fs::write(concat!(env!("CARGO_MANIFEST_DIR"), "/tests/fixtures/trigger_rows.txt"), &actual).unwrap();
    }
    assert_eq!(actual, include_str!("../tests/fixtures/trigger_rows.txt").replace("\r\n", "\n"));

    // Every watched change bumps its scope's revision; others do not.
    let revision = |scope: &'static str, account: i64| {
        let pool = db.pool.clone();
        async move {
            sqlx::query_scalar::<_, i64>("SELECT revision FROM response_revisions WHERE scope=$1 AND account=$2")
                .bind(scope).bind(account).fetch_one(&pool).await.unwrap()
        }
    };
    let before = revision("progression", 1).await;
    sqlx::query("UPDATE arena_runs SET last_poll=9 WHERE account=1").execute(&db.pool).await.unwrap();
    sqlx::query("UPDATE user_info SET sword=1 WHERE account_id=1").execute(&db.pool).await.unwrap();
    assert_eq!(revision("progression", 1).await, before, "unwatched columns must not bump");
    sqlx::query("UPDATE user_info SET team_level=3 WHERE account_id=1").execute(&db.pool).await.unwrap();
    assert_eq!(revision("progression", 1).await, before + 1);
    let equipment = revision("equipment", 2).await;
    sqlx::query("UPDATE equip_items SET account_id=1 WHERE account_id=2").execute(&db.pool).await.unwrap();
    assert_eq!(revision("equipment", 2).await, equipment + 1, "moving a row bumps its old owner");

    db.drop().await;
}

#[tokio::test]
async fn account_lock_serializes_one_account_only() {
    let Some(db) = TestDb::new().await else { return };
    sqlx::query("CREATE TABLE counter(id bigint PRIMARY KEY, value bigint NOT NULL)").execute(&db.pool).await.unwrap();
    sqlx::query("INSERT INTO counter VALUES (1, 0)").execute(&db.pool).await.unwrap();
    // Read-then-write races at READ COMMITTED unless the account lock queues them.
    let tasks: Vec<_> = (0..20).map(|_| {
        let pool = db.pool.clone();
        tokio::spawn(async move {
            transaction(&pool, Isolation::ReadCommitted, |tx| Box::pin(async move {
                lock_account(&mut *tx, 1).await?;
                let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&mut *tx).await?;
                tokio::time::sleep(Duration::from_millis(2)).await;
                sqlx::query("UPDATE counter SET value=$1 WHERE id=1").bind(value + 1).execute(&mut *tx).await?;
                Ok::<_, sqlx::Error>(())
            })).await
        })
    }).collect();
    for task in tasks { task.await.unwrap().unwrap(); }
    let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&db.pool).await.unwrap();
    assert_eq!(value, 20, "locked read-then-write transactions must not lose updates");

    // While account 1 is held, account 2 (and a pair locked in either order)
    // stays available and account 1 does not.
    let mut holder = db.pool.begin().await.unwrap();
    lock_account(&mut *holder, 1).await.unwrap();
    let try_lock = "SELECT pg_try_advisory_xact_lock($1)";
    let mut probe = db.pool.begin().await.unwrap();
    assert!(!sqlx::query_scalar::<_, bool>(try_lock).bind(1i64).fetch_one(&mut *probe).await.unwrap());
    assert!(sqlx::query_scalar::<_, bool>(try_lock).bind(2i64).fetch_one(&mut *probe).await.unwrap());
    probe.rollback().await.unwrap();
    holder.commit().await.unwrap();
    let mut pair = db.pool.begin().await.unwrap();
    lock_accounts(&mut *pair, &[3, 2, 3]).await.unwrap();
    pair.commit().await.unwrap();
    db.drop().await;
}

#[tokio::test]
async fn transaction_retries_serialization_failures() {
    let Some(db) = TestDb::new().await else { return };
    sqlx::query("CREATE TABLE counter(id bigint PRIMARY KEY, value bigint NOT NULL)").execute(&db.pool).await.unwrap();
    sqlx::query("INSERT INTO counter VALUES (1, 0)").execute(&db.pool).await.unwrap();
    let attempts = std::sync::Arc::new(std::sync::atomic::AtomicU32::new(0));
    let barrier = std::sync::Arc::new(tokio::sync::Barrier::new(2));
    let tasks: Vec<_> = (0..2).map(|_| {
        let (pool, attempts, barrier) = (db.pool.clone(), attempts.clone(), barrier.clone());
        tokio::spawn(async move {
            let mut first = true;
            transaction(&pool, Isolation::Serializable, move |tx| {
                let barrier = barrier.clone();
                let wait = std::mem::replace(&mut first, false);
                attempts.fetch_add(1, std::sync::atomic::Ordering::SeqCst);
                Box::pin(async move {
                    let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&mut *tx).await?;
                    // Both first attempts read before either writes.
                    if wait { barrier.wait().await; }
                    sqlx::query("UPDATE counter SET value=$1 WHERE id=1").bind(value + 1).execute(&mut *tx).await?;
                    Ok::<_, crate::error::ServerError>(())
                })
            }).await
        })
    }).collect();
    for task in tasks { task.await.unwrap().unwrap(); }
    let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&db.pool).await.unwrap();
    assert_eq!(value, 2, "the conflicting transaction must rerun, not lose its update");
    assert_eq!(attempts.load(std::sync::atomic::Ordering::SeqCst), 3);

    // Other errors are returned at once.
    let runs = std::sync::atomic::AtomicU32::new(0);
    let error = transaction(&db.pool, Isolation::ReadCommitted, |tx| {
        runs.fetch_add(1, std::sync::atomic::Ordering::SeqCst);
        Box::pin(async move { sqlx::query("SELECT 1/0").execute(&mut *tx).await.map(|_| ()) })
    }).await.unwrap_err();
    assert_eq!((sqlstate(&error).as_deref(), runs.into_inner()), (Some("22012"), 1));
    db.drop().await;
}

#[tokio::test]
async fn read_pool_rejects_writes() {
    let Some(db) = TestDb::new().await else { return };
    let reads = crate::database::test_read_pool(&db.pool).await;
    let error = sqlx::query("INSERT INTO accounts(login_id) VALUES('x')").execute(&reads).await.unwrap_err();
    assert_eq!(sqlstate(&error).as_deref(), Some("25006"));
    let zone: String = sqlx::query_scalar("SHOW timezone").fetch_one(&reads).await.unwrap();
    assert_eq!(zone, "UTC");
    reads.close().await;
    db.drop().await;
}

#[tokio::test]
async fn request_transactions_serialize_per_acting_account() {
    let Some(db) = TestDb::new().await else { return };
    let pool = db.pool.clone();
    // A transaction of account 1's request is open...
    let held = crate::database::ACTING_ACCOUNT.scope(1, crate::database::begin(&pool)).await.unwrap();
    // ...so account 1's next transaction waits, while account 2's and
    // background work (no acting account) proceed.
    let same = tokio::spawn(crate::database::ACTING_ACCOUNT.scope(1, {
        let pool = pool.clone();
        async move { crate::database::begin(&pool).await.map(|tx| drop(tx)) }
    }));
    let other = crate::database::ACTING_ACCOUNT.scope(2, crate::database::begin(&pool));
    tokio::time::timeout(Duration::from_secs(2), other).await.expect("another player must not wait").unwrap();
    tokio::time::timeout(Duration::from_secs(2), crate::database::begin(&pool)).await.expect("background work must not wait").unwrap();
    tokio::time::sleep(Duration::from_millis(200)).await;
    assert!(!same.is_finished(), "the same player's transaction must wait");
    held.commit().await.unwrap();
    tokio::time::timeout(Duration::from_secs(2), same).await.expect("released at commit").unwrap().unwrap();
    db.drop().await;
}

#[tokio::test]
async fn acting_account_middleware_prevents_lost_updates_within_one_player() {
    use axum::extract::State;
    use tower::ServiceExt;
    let Some(db) = TestDb::new().await else { return };
    let state = crate::state::AppState::new(db.pool.clone(), crate::tables::test_tables());
    let login = crate::api::account::user::test_login(State(state.clone()), axum::body::Bytes::from_static(b"LoginId=acting-account")).await.unwrap().0;
    sqlx::query("CREATE TABLE counter(id bigint PRIMARY KEY, value bigint NOT NULL)").execute(&db.pool).await.unwrap();
    sqlx::query("INSERT INTO counter VALUES (1, 0)").execute(&db.pool).await.unwrap();
    // A handler with a read-then-write race, as many game handlers have.
    async fn bump(State(state): State<crate::state::AppState>) -> &'static str {
        let mut tx = crate::database::begin(&state.db).await.unwrap();
        let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&mut *tx).await.unwrap();
        tokio::time::sleep(Duration::from_millis(2)).await;
        sqlx::query("UPDATE counter SET value=$1 WHERE id=1").bind(value + 1).execute(&mut *tx).await.unwrap();
        tx.commit().await.unwrap();
        "ok"
    }
    let app = axum::Router::new().route("/bump", axum::routing::post(bump))
        .layer(axum::middleware::from_fn_with_state(state.clone(), crate::api::system::middleware::acting_account))
        .with_state(state.clone());
    let body = format!("SessionKey={}", login.user_info.session_key);
    let requests = (0..20).map(|_| {
        let (app, body) = (app.clone(), body.clone());
        tokio::spawn(async move {
            app.oneshot(axum::http::Request::post("/bump").header("content-type", "application/x-www-form-urlencoded")
                .body(axum::body::Body::from(body)).unwrap()).await.unwrap().status()
        })
    }).collect::<Vec<_>>();
    for request in requests { assert!(request.await.unwrap().is_success()); }
    let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&db.pool).await.unwrap();
    assert_eq!(value, 20, "one player's parallel requests must not lose updates");
    db.drop().await;
}
