//! Durable dispatch preparation, independent of player sessions.
use super::*;
use tokio::sync::oneshot;

pub(super) struct Pending {
    id: String,
    worker: String,
    reply: Option<oneshot::Sender<Value>>,
}

pub(crate) fn start(s: AppState) {
    tokio::spawn(async move {
        loop {
            if let Err(error) = batch(&s).await {
                tracing::warn!(%error,"Dispatch preparation will retry");
            }
            tokio::time::sleep(std::time::Duration::from_secs(3)).await;
        }
    });
}

async fn batch(s: &AppState) -> Result<()> {
    // One repeat per slot per pass bounds engine load and gives every job a turn.
    let jobs: Vec<(i64,i64)> = sqlx::query_as("SELECT account,idx FROM battle_state WHERE kind='dispatch' AND json_extract(data,'$.SimulationVersion') IN (1,2) AND json_extract(data,'$.State') NOT IN ('Complete','Cancel') AND COALESCE(json_array_length(json_extract(data,'$.SimulationResults')),0)<json_extract(data,'$.RepeatCount') ORDER BY account,idx")
        .fetch_all(&s.db).await?;
    for (a, slot) in jobs {
        if let Err(error) = prepare(s, a, slot).await {
            tracing::warn!(%error,account=a,slot,"Dispatch job will retry");
        }
    }
    Ok(())
}

async fn prepare(s: &AppState, a: i64, slot: i64) -> Result<()> {
    let (worker, sender) = {
        let guard = s.conquest.worker.read().await;
        let Some(w) = guard
            .as_ref()
            .filter(|w| w.dispatch_version >= 1 && !w.sender.is_closed())
        else {
            return Ok(());
        };
        (w.id.clone(), w.sender.clone())
    };
    let mut tx = s.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(a)
        .execute(&mut *tx)
        .await?;
    let mut job = get(&mut tx, a, "dispatch", slot).await?;
    if matches!(job["State"].as_str(), Some("Complete" | "Cancel")) {
        return Ok(());
    }
    let index = job["SimulationResults"].as_array().map_or(0, Vec::len);
    if index >= n(&job, "RepeatCount") as usize {
        return Ok(());
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
    *s.conquest.dispatch.lock().await = Some(Pending {
        id: id.clone(),
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
    *s.conquest.dispatch.lock().await = None;
    let _ = sender.try_send(("WorkerCancel".into(), json!({"RunId":id})));
    if let Some(answer) = answer {
        save(s, a, slot, &job, index, &answer).await?;
    }
    Ok(())
}

async fn save(
    s: &AppState,
    a: i64,
    slot: i64,
    original: &Value,
    index: usize,
    answer: &Value,
) -> Result<()> {
    let mut tx = s.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
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

pub(super) async fn packet(s: &AppState, worker: &str, name: &str, body: &Value) -> Result<()> {
    let mut pending = s.conquest.dispatch.lock().await;
    let Some(p) = pending.as_mut().filter(|p| body["RunId"] == p.id) else {
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
            .send_worker("WorkerStart", json!({"RunId":p.id}))
            .await?;
        s.conquest
            .send_worker("WorkerResume", json!({"RunId":p.id}))
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
        tokio::time::timeout(
            std::time::Duration::from_secs(900),
            prepare(&s, account(&u), n(&job, "SlotIndex")),
        )
        .await
        .expect("Native dispatch timed out")
        .unwrap();
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
        if let Ok(path) = std::env::var("DISPATCH_NATIVE_OUTPUT") {
            std::fs::write(path, serde_json::to_vec_pretty(&saved["SimulationResults"]).unwrap()).unwrap();
        }
        server.abort();
        http_server.abort();
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
                batch(&state).await.unwrap();
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
