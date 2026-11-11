//! Durable dispatch preparation, independent of player sessions.
use super::*;
use std::collections::HashSet;
use std::sync::{Arc, Mutex};
use tokio::sync::{oneshot, Semaphore};

pub(super) struct Pending {
    worker: String,
    reply: Option<oneshot::Sender<Value>>,
}

/// Simulations in flight on the shared battle worker at once.
const CONCURRENCY: usize = 4;
/// Fights simulated natively per dispatch. Longer dispatches draw their
/// remaining fights from these samples; shorter ones simulate every fight.
const SAMPLE_FIGHTS: usize = 20;

/// Continuous, fair scheduling. Each pending job runs its own task that
/// prepares one repeat per slot turn and immediately queues for the next.
/// Slots are FIFO, so a job that just finished waits behind every other job,
/// and a long fight only occupies its own slot instead of holding a whole pass.
#[derive(Clone)]
struct Scheduler {
    slots: Arc<Semaphore>,
    active: Arc<Mutex<HashSet<(i64, i64)>>>,
}

pub(crate) fn start(s: AppState) {
    let scheduler = Scheduler::new();
    crate::database::spawn_labelled("task:dispatch-scan", async move {
        loop {
            if let Err(error) = scheduler.scan(&s).await {
                tracing::warn!(%error,"Dispatch preparation will retry");
            }
            tokio::time::sleep(std::time::Duration::from_secs(3)).await;
        }
    });
}

impl Scheduler {
    fn new() -> Self {
        Self { slots: Arc::new(Semaphore::new(CONCURRENCY)), active: Arc::default() }
    }

    /// Start a task for each pending job that does not already have one.
    async fn scan(&self, s: &AppState) -> Result<()> {
        let jobs: Vec<(i64,i64)> = crate::db::query_as(sqlx::AssertSqlSafe(format!("SELECT account,idx FROM battle_state WHERE {} ORDER BY account,idx", crate::api::battle::PENDING_DISPATCH_SIMULATION)))
            .fetch_all(&s.db).await?;
        for job in jobs {
            if !self.active.lock().unwrap_or_else(|e| e.into_inner()).insert(job) {
                continue;
            }
            let (scheduler, s) = (self.clone(), s.clone());
            crate::database::spawn_labelled("task:dispatch-preparation", async move { scheduler.run(&s, job).await });
        }
        Ok(())
    }

    /// Keep preparing while results are saved. Any other outcome (no worker,
    /// finished, cancelled, error) ends the task; the next scan restarts it.
    async fn run(&self, s: &AppState, job: (i64, i64)) {
        struct Release<'a>(&'a Scheduler, (i64, i64));
        impl Drop for Release<'_> {
            fn drop(&mut self) {
                self.0.active.lock().unwrap_or_else(|e| e.into_inner()).remove(&self.1);
            }
        }
        let _release = Release(self, job);
        let (a, slot) = job;
        loop {
            let Ok(_turn) = self.slots.acquire().await else { return; };
            match prepare(s, a, slot).await {
                Ok(true) => {}
                Ok(false) => return,
                Err(error) => {
                    tracing::warn!(%error,account=a,slot,"Dispatch job will retry");
                    return;
                }
            }
        }
    }
}

/// Prepare one repeat. Returns whether a result was saved.
async fn prepare(s: &AppState, a: i64, slot: i64) -> Result<bool> {
    let (worker, sender) = {
        let guard = s.conquest.worker.read().await;
        let Some(w) = guard
            .as_ref()
            .filter(|w| w.dispatch_version >= 1 && !w.sender.is_closed())
        else {
            return Ok(false);
        };
        (w.id.clone(), w.sender.clone())
    };
    let mut tx = crate::database::begin(&s.db).await?;
    crate::db::query("SELECT pg_advisory_xact_lock($1)")
        .bind(a)
        .execute(&mut *tx)
        .await?;
    let mut job = get(&mut tx, a, "dispatch", slot).await?;
    if matches!(job["State"].as_str(), Some("Complete" | "Cancel")) {
        return Ok(false);
    }
    let index = job["SimulationResults"].as_array().map_or(0, Vec::len);
    if index >= n(&job, "RepeatCount") as usize {
        return Ok(false);
    }
    if job["SimulationSnapshot"].is_null() {
        job["SimulationSnapshot"] =
            super::super::dispatch::simulation_snapshot(&mut tx, s, a, &job).await?;
    }
    job["SimulationVersion"] = json!(2);
    put(&mut tx, a, "dispatch", slot, &job).await?;
    tx.commit().await?;
    let id = format!(
        "dispatch:{}:{index}",
        job["SimulationJobId"]
            .as_str()
            .ok_or_else(|| rule("InvalidValue"))?
    );
    let (reply, mut rx) = oneshot::channel();
    let s_worker = worker.clone();
    s.conquest.dispatch.lock().await.insert(id.clone(), Pending {
        worker,
        reply: Some(reply),
    });
    // Independent across jobs, stable when a repeat is retried after a restart.
    let seed = id.bytes().fold(2166136261_u32, |hash, byte| (hash ^ byte as u32).wrapping_mul(16777619)) & 0x7fffffff;
    let payload = json!({"RunId":id,"Dispatch":true,"BossHp":0,"BossIndex":0,"Seed":seed,
        "Simulation":job["SimulationSnapshot"]});
    let sent = sender.try_send(("WorkerPrepare".into(), payload)).is_ok();
    if sent {
        let _ = sender.try_send(("WorkerStart".into(), json!({"RunId":id})));
    }
    let answer = if sent {
        tokio::time::timeout(std::time::Duration::from_secs(900),async {
        loop {
            tokio::select! {
                result=&mut rx => return result.ok(),
                _=tokio::time::sleep(std::time::Duration::from_secs(3)) => {
                    let active=s.conquest.worker.read().await.as_ref().is_some_and(|w|w.id==s_worker && !w.sender.is_closed());
                    let mut db=s.db.acquire().await.ok()?;
                    let current=get(&mut db,a,"dispatch",slot).await.ok()?;
                    if !active || current["SimulationJobId"]!=job["SimulationJobId"] || matches!(current["State"].as_str(),Some("Complete"|"Cancel")) {return None;}
                }
            }
        }
    }).await.ok().flatten()
    } else {
        None
    };
    s.conquest.dispatch.lock().await.remove(&id);
    let _ = sender.try_send(("WorkerCancel".into(), json!({"RunId":id})));
    let Some(answer) = answer else { return Ok(false); };
    save(s, a, slot, &job, index, &answer).await?;
    Ok(true)
}

async fn save(
    s: &AppState,
    a: i64,
    slot: i64,
    original: &Value,
    index: usize,
    answer: &Value,
) -> Result<()> {
    let mut tx = crate::database::begin(&s.db).await?;
    crate::db::query("SELECT pg_advisory_xact_lock($1)")
        .bind(a)
        .execute(&mut *tx)
        .await?;
    let mut job = get(&mut tx, a, "dispatch", slot).await?;
    if job["SimulationJobId"] != original["SimulationJobId"]
        || matches!(job["State"].as_str(), Some("Complete" | "Cancel"))
        || job["SimulationResults"].as_array().map_or(0, Vec::len) != index
    {
        return Ok(());
    }
    let mut results = job["SimulationResults"]
        .as_array()
        .cloned()
        .unwrap_or_default();
    results.push(json!({"Outcome":if answer["Win"]==true {1}else{0},"TimeMs":number(answer,"TimeMs").clamp(1000,3600000)}));
    let repeats = n(&job, "RepeatCount").max(0) as usize;
    if results.len() >= SAMPLE_FIGHTS && results.len() < repeats {
        let seed = job["SimulationJobId"].as_str().unwrap_or_default().to_owned();
        job["SampledFights"] = json!(results.len());
        results = extrapolate(results, repeats, &seed);
    }
    let total: i64 = results.iter().map(|v| n(v, "TimeMs")).sum();
    let average = total / results.len() as i64;
    let estimate = total + average * (n(&job, "RepeatCount") - results.len() as i64);
    job["FinishTimestamp"] = json!(n(&job, "StartTimestamp") + (estimate + 999) / 1000);
    job["CompleteTime"] = json!(time(n(&job, "FinishTimestamp")));
    job["ProgressiveResults"] = json!(true);
    job["SimulationResults"] = json!(results);
    job["State"] = json!("Battle");
    put(&mut tx, a, "dispatch", slot, &job).await?;
    tx.commit().await?;
    Ok(())
}

/// Fill a dispatch to `repeats` fights by drawing whole (outcome, time) pairs
/// from the natively simulated ones. Deterministic per job, so the result does
/// not depend on retries or restarts. Fights are revealed by the usual clock.
fn extrapolate(mut results: Vec<Value>, repeats: usize, seed: &str) -> Vec<Value> {
    let samples = results.len();
    if samples == 0 {
        return results;
    }
    // FNV-1a seed into splitmix64: small, dependency-free and stable.
    let mut state = seed.bytes().fold(0xcbf29ce484222325_u64, |h, b| (h ^ b as u64).wrapping_mul(0x100000001b3));
    while results.len() < repeats {
        state = state.wrapping_add(0x9e3779b97f4a7c15);
        let mut z = state;
        z = (z ^ (z >> 30)).wrapping_mul(0xbf58476d1ce4e5b9);
        z = (z ^ (z >> 27)).wrapping_mul(0x94d049bb133111eb);
        z ^= z >> 31;
        let pick = &results[(z % samples as u64) as usize];
        results.push(json!({"Outcome":n(pick,"Outcome"),"TimeMs":n(pick,"TimeMs")}));
    }
    results
}

pub(super) async fn packet(s: &AppState, worker: &str, name: &str, body: &Value) -> Result<()> {
    let mut pending = s.conquest.dispatch.lock().await;
    let Some(id) = body["RunId"].as_str() else { return Ok(()); };
    let Some(p) = pending.get_mut(id) else {
        return Ok(());
    };
    if p.worker != worker {
        return Err(rule("Unauthorized"));
    }
    if name == "WorkerResult" {
        if !body["Win"].is_boolean() || !(0..=3600000).contains(&number(body, "TimeMs")) {
            return Err(rule("InvalidValue"));
        }
        #[cfg(test)]
        if let Ok(path) = std::env::var("DISPATCH_NATIVE_REPORT") {
            std::fs::write(path, serde_json::to_vec_pretty(body).unwrap()).unwrap();
        }
        if let Some(reply) = p.reply.take() {
            let _ = reply.send(body.clone());
        }
    } else if name == "WorkerPacket" && body["Name"] == "WaveReadyNot" {
        s.conquest
            .send_worker("WorkerStart", json!({"RunId":id}))
            .await?;
        s.conquest
            .send_worker("WorkerResume", json!({"RunId":id}))
            .await?;
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::battle::tests::{account, call, setup};

    async fn job() -> (AppState, Value, Value) {
        let (s, u) = setup().await;
        put(
            &mut s.db.acquire().await.unwrap(),
            account(&u),
            "dungeon",
            super::super::super::campaign::key(1, 1),
            &json!({"FirstRewardedDiff":2,"MaxStar":13}),
        )
        .await
        .unwrap();
        let response = call(
            &s,
            &u,
            "dispatch/start_dispatch",
            "ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount=2&DeckIndex=1",
        )
        .await;
        assert_eq!(response["Result"], "Success", "{response}");
        (s, u, response["DispatchBattleInfo"].clone())
    }

    #[tokio::test]
    #[ignore = "Requires DISPATCH_NATIVE_EXECUTABLE pointing to the patched Unity worker"]
    async fn offline_dispatch_native_engine() {
        struct Native(std::process::Child);
        impl Drop for Native {
            fn drop(&mut self) {
                let _ = self.0.kill();
                let _ = self.0.wait();
            }
        }
        let (mut s, u, mut job) = job().await;
        // Reproduce a reported combat snapshot without importing accounts or
        // modifying the supplied database. Keep exports in ignored local paths.
        if let Ok(path) = std::env::var("DISPATCH_NATIVE_JOB") {
            job = serde_json::from_slice(&std::fs::read(path).unwrap()).unwrap();
            job["SlotIndex"] = json!(1);
            job["RepeatCount"] = json!(1);
            job["SimulationResults"] = json!([]);
            job["State"] = json!("Simulation");
            job["SimulationVersion"] = json!(2);
            put(&mut s.db.acquire().await.unwrap(), account(&u), "dispatch", 1, &job).await.unwrap();
        }
        let key = uuid::Uuid::new_v4().to_string();
        s.battle_service_key = Arc::new(Some(key.clone()));
        let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
        let port = listener.local_addr().unwrap().port();
        let server = tokio::spawn(serve(listener, s.clone()));
        let http = TcpListener::bind("127.0.0.1:0").await.unwrap();
        let http_port = http.local_addr().unwrap().port();
        let host = axum::Router::new().route(
            "/host.json",
            axum::routing::get(crate::api::system::query::get_host_info),
        );
        let http_server = tokio::spawn(async move {
            axum::serve(http, host).await.unwrap();
        });
        let executable =
            std::path::PathBuf::from(std::env::var("DISPATCH_NATIVE_EXECUTABLE").unwrap())
                .canonicalize()
                .unwrap();
        let mut command = std::process::Command::new(&executable);
        command
            .current_dir(executable.parent().unwrap())
            .args(["-batchmode", "-nographics", "-logFile"])
            .arg(
                std::env::current_dir()
                    .unwrap()
                    .join("target/dispatch-native.log"),
            )
            .env(
                "SPRK_HOST_URL",
                format!("http://127.0.0.1:{http_port}/host.json"),
            )
            .env("SPRK_CONQUEST_WORKER_KEY", key)
            .env("SPRK_CONQUEST_WORKER_HOST", "127.0.0.1")
            .env("SPRK_CONQUEST_WORKER_PORT", port.to_string());
        #[cfg(windows)]
        {
            use std::os::windows::process::CommandExt;
            command.creation_flags(0x08000000);
        }
        let _native = Native(command.spawn().unwrap());
        tokio::time::timeout(std::time::Duration::from_secs(90), async {
            while !s
                .conquest
                .worker
                .read()
                .await
                .as_ref()
                .is_some_and(|w| w.dispatch_version >= 1)
            {
                tokio::time::sleep(std::time::Duration::from_millis(100)).await;
            }
        })
        .await
        .expect("Dispatch worker did not authenticate");
        let simulation_started = std::time::Instant::now();
        tokio::time::timeout(
            std::time::Duration::from_secs(900),
            prepare(&s, account(&u), n(&job, "SlotIndex")),
        )
        .await
        .expect("Native dispatch timed out")
        .unwrap();
        let simulation_elapsed = simulation_started.elapsed();
        let saved = get(
            &mut s.db.acquire().await.unwrap(),
            account(&u),
            "dispatch",
            n(&job, "SlotIndex"),
        )
        .await
        .unwrap();
        assert_eq!(
            saved["SimulationResults"].as_array().unwrap().len(),
            1,
            "{saved}"
        );
        let simulated_ms = n(&saved["SimulationResults"][0], "TimeMs");
        println!("Native dispatch: {simulated_ms} ms of combat prepared in {} ms", simulation_elapsed.as_millis());
        if simulated_ms >= 10000 {
            assert!(simulation_elapsed.as_millis() < simulated_ms as u128 / 2,
                "Offline preparation must run ahead of wall-clock combat time");
        }
        if let Ok(path) = std::env::var("DISPATCH_NATIVE_OUTPUT") {
            std::fs::write(path, serde_json::to_vec_pretty(&saved["SimulationResults"]).unwrap()).unwrap();
        }
        server.abort();
        http_server.abort();
    }

    #[tokio::test]
    async fn offline_dispatch_jobs_prepare_concurrently_and_route_results_by_run() {
        let (s, u, job) = job().await;
        let a = account(&u);
        for slot in 1..=5 {
            let mut copy = job.clone();
            copy["SlotIndex"] = json!(slot);
            copy["SimulationJobId"] = json!(format!("concurrent-{slot}"));
            put(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot, &copy).await.unwrap();
        }
        let (tx, mut rx) = mpsc::channel(50);
        *s.conquest.worker.write().await = Some(Worker {
            id: "dispatch-test".into(), sender: tx,
            arena_version: 1, challenge_version: 1, dispatch_version: 1,
        });
        Scheduler::new().scan(&s).await.unwrap();
        let mut ids = Vec::new();
        tokio::time::timeout(std::time::Duration::from_secs(5), async {
            while ids.len() < 4 {
                let (name, body) = rx.recv().await.unwrap();
                if name == "WorkerPrepare" { ids.push(body["RunId"].as_str().unwrap().to_owned()); }
            }
        }).await.expect("One stalled dispatch must not block other jobs");
        assert_eq!(s.conquest.dispatch.lock().await.len(), 4);
        // Complete the last of the initial four first. The fifth job, already
        // waiting, gets the freed slot before that job's own next repeat.
        let completed = ids.pop().unwrap();
        packet(&s, "dispatch-test", "WorkerResult", &json!({"RunId":completed,"Win":false,"TimeMs":2000})).await.unwrap();
        let fifth = tokio::time::timeout(std::time::Duration::from_secs(5), async {
            loop {
                let (name, body) = rx.recv().await.unwrap();
                if name == "WorkerPrepare" { break body["RunId"].as_str().unwrap().to_owned(); }
            }
        }).await.expect("Freed preparation capacity must serve the next slot");
        assert_eq!(fifth, "dispatch:concurrent-5:0");
        for id in ids.into_iter().chain([fifth]) {
            assert!(packet(&s, "forged-worker", "WorkerResult", &json!({"RunId":id,"Win":true,"TimeMs":1000})).await.is_err());
            packet(&s, "dispatch-test", "WorkerResult", &json!({"RunId":id,"Win":true,"TimeMs":1000})).await.unwrap();
        }
        // Every job continues with its second repeat without waiting for a pass.
        let mut next = HashSet::new();
        tokio::time::timeout(std::time::Duration::from_secs(5), async {
            while next.len() < 4 {
                let (name, body) = rx.recv().await.unwrap();
                if name == "WorkerPrepare" { next.insert(body["RunId"].as_str().unwrap().to_owned()); }
            }
        }).await.expect("Saved jobs must queue their next repeat");
        assert!(next.iter().all(|id| id.ends_with(":1")), "{next:?}");
        for slot in 1..=5 {
            let saved = get(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot).await.unwrap();
            let lost = completed == format!("dispatch:concurrent-{slot}:0");
            assert_eq!(saved["SimulationResults"], json!([{"Outcome":if lost {0}else{1},"TimeMs":if lost {2000}else{1000}}]));
        }
    }

    #[tokio::test]
    async fn a_long_dispatch_fight_does_not_hold_back_other_jobs() {
        let (s, u, job) = job().await;
        let a = account(&u);
        for slot in 1..=2 {
            let mut copy = job.clone();
            copy["SlotIndex"] = json!(slot);
            copy["SimulationJobId"] = json!(format!("pace-{slot}"));
            put(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot, &copy).await.unwrap();
        }
        let (tx, mut rx) = mpsc::channel(50);
        *s.conquest.worker.write().await = Some(Worker {
            id: "dispatch-test".into(), sender: tx,
            arena_version: 1, challenge_version: 1, dispatch_version: 1,
        });
        async fn next(rx: &mut mpsc::Receiver<Packet>) -> String {
            loop {
                let (name, body) = rx.recv().await.unwrap();
                if name == "WorkerPrepare" { break body["RunId"].as_str().unwrap().to_owned(); }
            }
        }
        let scheduler = Scheduler::new();
        scheduler.scan(&s).await.unwrap();
        let mut prepared = HashSet::new();
        let wait = std::time::Duration::from_secs(5);
        for _ in 0..2 { prepared.insert(tokio::time::timeout(wait, next(&mut rx)).await.unwrap()); }
        assert_eq!(prepared, HashSet::from(["dispatch:pace-1:0".to_owned(), "dispatch:pace-2:0".to_owned()]));
        // Slot 1's fight is still computing. Slot 2 finishing must start its
        // next repeat at once rather than waiting for slot 1 or a new scan.
        packet(&s, "dispatch-test", "WorkerResult", &json!({"RunId":"dispatch:pace-2:0","Win":true,"TimeMs":1000})).await.unwrap();
        assert_eq!(tokio::time::timeout(wait, next(&mut rx)).await.expect("A short job waited on a long one"), "dispatch:pace-2:1");
        assert!(s.conquest.dispatch.lock().await.contains_key("dispatch:pace-1:0"));
        // A periodic scan while both jobs are in flight must not duplicate work.
        scheduler.scan(&s).await.unwrap();
        assert!(tokio::time::timeout(std::time::Duration::from_millis(300), next(&mut rx)).await.is_err());
        assert_eq!(s.conquest.dispatch.lock().await.len(), 2);
    }

    #[tokio::test]
    async fn long_dispatches_simulate_a_sample_and_draw_the_rest() {
        let (s, u, job) = job().await;
        let a = account(&u);
        let pending = |s: AppState| async move {
            crate::db::query_scalar::<i64>(sqlx::AssertSqlSafe(format!("SELECT COUNT(*) FROM battle_state WHERE {}", crate::api::battle::PENDING_DISPATCH_SIMULATION)))
                .fetch_one(&s.db).await.unwrap()
        };
        // slot 1: 50 fights (sampled); slot 2: exactly SAMPLE_FIGHTS (all simulated);
        // slot 3: already past the sample when this version deploys.
        let mut jobs = vec![];
        for (slot, repeats) in [(1, 50), (2, SAMPLE_FIGHTS as i64), (3, 50)] {
            let mut copy = job.clone();
            copy["SlotIndex"] = json!(slot);
            copy["RepeatCount"] = json!(repeats);
            copy["SimulationJobId"] = json!(format!("sample-{slot}"));
            put(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot, &copy).await.unwrap();
            jobs.push(copy);
        }
        let fight = |i: usize| json!({"Win": i % 4 != 0, "TimeMs": 1000 + i as i64 * 100});
        for i in 0..SAMPLE_FIGHTS {
            for slot in 1..=2 { save(&s, a, slot, &jobs[slot as usize - 1], i, &fight(i)).await.unwrap(); }
        }
        let saved = get(&mut s.db.acquire().await.unwrap(), a, "dispatch", 1).await.unwrap();
        let results = saved["SimulationResults"].as_array().unwrap();
        assert_eq!((results.len(), saved["SampledFights"].clone()), (50, json!(SAMPLE_FIGHTS)));
        let sample = &results[..SAMPLE_FIGHTS];
        for (i, r) in sample.iter().enumerate() {
            assert_eq!(*r, json!({"Outcome": if i % 4 != 0 {1} else {0}, "TimeMs": 1000 + i as i64 * 100}));
        }
        assert!(results[SAMPLE_FIGHTS..].iter().all(|r| sample.contains(r)), "drawn fights repeat sampled pairs");
        assert_eq!(results.to_vec(), extrapolate(sample.to_vec(), 50, "sample-1"), "deterministic per job");
        let total: i64 = results.iter().map(|r| n(r, "TimeMs")).sum();
        assert_eq!(n(&saved, "FinishTimestamp"), n(&saved, "StartTimestamp") + (total + 999) / 1000);
        // A dispatch no longer than the sample simulates every fight.
        let short = get(&mut s.db.acquire().await.unwrap(), a, "dispatch", 2).await.unwrap();
        assert_eq!(short["SimulationResults"].as_array().unwrap().len(), SAMPLE_FIGHTS);
        assert!(short["SampledFights"].is_null());
        // Slot 3 had 25 natively simulated fights before sampling existed.
        let mut legacy = jobs[2].clone();
        legacy["SimulationResults"] = json!((0..25).map(|_| json!({"Outcome":1,"TimeMs":2000})).collect::<Vec<_>>());
        put(&mut s.db.acquire().await.unwrap(), a, "dispatch", 3, &legacy).await.unwrap();
        assert_eq!(pending(s.clone()).await, 1, "only the in-flight legacy job still needs the worker");
        save(&s, a, 3, &legacy, 25, &json!({"Win":false,"TimeMs":3000})).await.unwrap();
        let filled = get(&mut s.db.acquire().await.unwrap(), a, "dispatch", 3).await.unwrap();
        assert_eq!((filled["SimulationResults"].as_array().unwrap().len(), filled["SampledFights"].clone()), (50, json!(26)));
        assert_eq!(pending(s.clone()).await, 0, "filled dispatches leave the preparation queue");
    }

    #[tokio::test]
    async fn offline_dispatch_worker_persists_restarts_and_rejects_client_results() {
        let (mut s, u, job) = job().await;
        let a = account(&u);
        let slot = n(&job, "SlotIndex");
        assert_eq!(job["SimulationVersion"], 2);
        assert!(job["SimulationSnapshot"]["AccountInfos"][0]["HeroInfos"]["1"].is_object());
        assert_ne!(call(&s,&u,"dispatch/complete_calculate_dispatch_result",&format!("SlotIndex={slot}&SimulationJobId={}&Progressive=true&Results=[1]&TimesMs=[1000]",job["SimulationJobId"].as_str().unwrap())).await["Result"],"Success");
        for outcome in [true, false] {
            if !outcome {
                let mut db = s.db.acquire().await.unwrap();
                let mut legacy = get(&mut db, a, "dispatch", slot).await.unwrap();
                legacy["SimulationVersion"] = json!(1);
                legacy.as_object_mut().unwrap().remove("SimulationSnapshot");
                put(&mut db, a, "dispatch", slot, &legacy).await.unwrap();
            }
            // New process state; only the database survives. No player session is used.
            s = AppState::new(s.db.clone(), s.tables.as_ref().clone());
            let (tx, mut rx) = mpsc::channel(20);
            *s.conquest.worker.write().await = Some(Worker {
                id: "dispatch-test".into(),
                sender: tx,
                arena_version: 1,
                challenge_version: 1,
                dispatch_version: 1,
            });
            let state = s.clone();
            let task = tokio::spawn(async move {
                assert!(prepare(&state, a, slot).await.unwrap(), "one result is saved");
            });
            let (name, payload) = rx.recv().await.unwrap();
            assert_eq!(name, "WorkerPrepare");
            assert_eq!(payload["Simulation"], job["SimulationSnapshot"]);
            assert_eq!(rx.recv().await.unwrap().0, "WorkerStart");
            let body = json!({"RunId":payload["RunId"],"Win":outcome,"TimeMs":1000});
            assert!(packet(&s, "forged-worker", "WorkerResult", &body)
                .await
                .is_err());
            packet(&s, "dispatch-test", "WorkerResult", &body)
                .await
                .unwrap();
            packet(&s, "dispatch-test", "WorkerResult", &body)
                .await
                .unwrap();
            task.await.unwrap();
        }
        let mut db = s.db.acquire().await.unwrap();
        let saved = get(&mut db, a, "dispatch", slot).await.unwrap();
        assert_eq!(
            saved["SimulationResults"],
            json!([{"Outcome":1,"TimeMs":1000},{"Outcome":0,"TimeMs":1000}])
        );
        assert_eq!(saved["StartTimestamp"], job["StartTimestamp"]);
        let mut canceled = saved.clone();
        canceled["State"] = json!("Cancel");
        put(&mut db, a, "dispatch", slot, &canceled).await.unwrap();
        drop(db);
        save(&s, a, slot, &job, 2, &json!({"Win":true,"TimeMs":1000}))
            .await
            .unwrap();
        assert_eq!(
            get(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot)
                .await
                .unwrap(),
            canceled
        );
    }
}
