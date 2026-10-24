//! Native PlayRecordTable row IDs. Historical facts survive inventory/season resets;
//! ownership rows are intentionally computed from the current account snapshot.
use super::*;
use std::collections::BTreeMap;

pub(crate) async fn migrate(pool: &crate::database::DbPool) -> anyhow::Result<()> {
    let mut tx = pool.begin().await?;
    sqlx::query("CREATE TABLE IF NOT EXISTS play_record_values(account_id INTEGER NOT NULL,kind TEXT NOT NULL,idx INTEGER NOT NULL DEFAULT 0,value INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(account_id,kind,idx))").execute(&mut *tx).await?;
    // Recover only known history. Metrics retain acquisitions even after sale;
    // existing inventory proves a minimum for saves predating those metrics.
    for sql in [
        "INSERT OR IGNORE INTO progression_metrics(account_id,day,kind,arg,arg2,value) SELECT account_id,last_day,'LoginDaily',0,0,1 FROM progression_login WHERE days>0 AND last_day!=''",
        "INSERT OR IGNORE INTO play_record_values SELECT account_id,'equip_count',item_index,MAX(owned,acquired) FROM (SELECT account_id,item_index,COUNT(*) owned,COALESCE((SELECT SUM(value) FROM progression_metrics m WHERE m.account_id=e.account_id AND m.kind='AddEquip' AND m.arg=e.item_index),0) acquired FROM equip_items e GROUP BY account_id,item_index)",
        "INSERT OR IGNORE INTO play_record_values SELECT account_id,'equip_count',arg,SUM(value) FROM progression_metrics WHERE kind='AddEquip' GROUP BY account_id,arg",
        "INSERT OR IGNORE INTO play_record_values SELECT account_id,'equip_star',item_index,MAX(star) FROM equip_items GROUP BY account_id,item_index",
        "INSERT OR IGNORE INTO play_record_values SELECT s.account_id,'soul_count',0,COUNT(*) FROM extension_state s JOIN equip_items e ON e.account_id=s.account_id AND e.slot_index=s.idx WHERE s.kind='soul' GROUP BY s.account_id",
        "INSERT OR IGNORE INTO play_record_values SELECT account_id,'soul_grade',0,MAX(COALESCE(json_extract(data,'$.Grade'),0)) FROM extension_state WHERE kind='soul' GROUP BY account_id",
        "INSERT OR IGNORE INTO play_record_values SELECT account_id,'soul_reinforce',0,MAX(COALESCE(json_extract(data,'$.ReinforceLevel'),0)) FROM extension_state WHERE kind='soul' GROUP BY account_id",
        "INSERT OR IGNORE INTO play_record_values SELECT account,'arena_score',0,MAX(score) FROM arena_scores WHERE kind=0 AND wins>0 GROUP BY account",
        "INSERT OR IGNORE INTO play_record_values SELECT account,'arena_streak',0,MAX(COALESCE(json_extract(data,'$.SuccessiveWin'),0)) FROM arena_scores WHERE kind=0 GROUP BY account",
        "INSERT OR IGNORE INTO play_record_values SELECT account,'arena_wins',0,SUM(wins) FROM arena_scores WHERE kind=0 GROUP BY account",
        "INSERT OR IGNORE INTO play_record_values SELECT account,'arena_losses',0,SUM(losses) FROM arena_scores WHERE kind=0 GROUP BY account",
        "INSERT INTO play_record_values SELECT account,'arena_tier',0,MAX(COALESCE(json_extract(data,'$.End.MatchResult.NewTierIndex'),0)) FROM arena_runs WHERE kind=0 AND status='complete' AND COALESCE(json_extract(data,'$.End.MatchResult.Canceled'),0)=0 GROUP BY account ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=MAX(value,excluded.value)",
        "INSERT INTO play_record_values SELECT account,'arena_score',0,MAX(COALESCE(json_extract(data,'$.End.MatchResult.NewMatchScore'),0)) FROM arena_runs WHERE kind=0 AND status='complete' AND json_extract(data,'$.End.MatchResult.Win')=1 GROUP BY account ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=MAX(value,excluded.value)",
        "INSERT INTO play_record_values SELECT account,'arena_streak',0,MAX(COALESCE(json_extract(data,'$.End.BattleInfo.SuccessiveWin'),0)) FROM arena_runs WHERE kind=0 AND status='complete' GROUP BY account ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=MAX(value,excluded.value)",
        // One baseline row per stage prevents counting legacy clears twice on restart.
        "INSERT OR IGNORE INTO progression_metrics(account_id,day,kind,arg,arg2,value) SELECT account_id,'0001-01-01','ClearDungeon',chapter_id,dungeon_id,MAX(0,clear_count-COALESCE((SELECT SUM(value) FROM progression_metrics m WHERE m.account_id=p.account_id AND m.kind='ClearDungeon' AND m.arg=p.chapter_id AND m.arg2=p.dungeon_id),0)) FROM campaign_progress p",
    ] { sqlx::query(sql).execute(&mut *tx).await?; }
    for (name, table, event, condition, account, kind, idx, value, accumulate) in [
        (
            "equip_count",
            "equip_items",
            "INSERT",
            "1",
            "NEW.account_id",
            "equip_count",
            "NEW.item_index",
            "1",
            true,
        ),
        (
            "equip_star_new",
            "equip_items",
            "INSERT",
            "1",
            "NEW.account_id",
            "equip_star",
            "NEW.item_index",
            "NEW.star",
            false,
        ),
        (
            "equip_star",
            "equip_items",
            "UPDATE",
            "NEW.star>OLD.star",
            "NEW.account_id",
            "equip_star",
            "NEW.item_index",
            "NEW.star",
            false,
        ),
        (
            "soul_grade_new",
            "extension_state",
            "INSERT",
            "NEW.kind='soul'",
            "NEW.account_id",
            "soul_grade",
            "0",
            "COALESCE(json_extract(NEW.data,'$.Grade'),0)",
            false,
        ),
        (
            "soul_grade",
            "extension_state",
            "UPDATE",
            "NEW.kind='soul'",
            "NEW.account_id",
            "soul_grade",
            "0",
            "COALESCE(json_extract(NEW.data,'$.Grade'),0)",
            false,
        ),
        (
            "soul_reinforce_new",
            "extension_state",
            "INSERT",
            "NEW.kind='soul'",
            "NEW.account_id",
            "soul_reinforce",
            "0",
            "COALESCE(json_extract(NEW.data,'$.ReinforceLevel'),0)",
            false,
        ),
        (
            "soul_reinforce",
            "extension_state",
            "UPDATE",
            "NEW.kind='soul'",
            "NEW.account_id",
            "soul_reinforce",
            "0",
            "COALESCE(json_extract(NEW.data,'$.ReinforceLevel'),0)",
            false,
        ),
        (
            "arena_score",
            "arena_scores",
            "UPDATE",
            "NEW.kind=0 AND NEW.wins>OLD.wins",
            "NEW.account",
            "arena_score",
            "0",
            "NEW.score",
            false,
        ),
        (
            "arena_streak",
            "arena_scores",
            "UPDATE",
            "NEW.kind=0 AND NEW.wins>OLD.wins",
            "NEW.account",
            "arena_streak",
            "0",
            "COALESCE(json_extract(NEW.data,'$.SuccessiveWin'),0)",
            false,
        ),
        (
            "arena_wins",
            "arena_scores",
            "UPDATE",
            "NEW.kind=0 AND NEW.wins>OLD.wins",
            "NEW.account",
            "arena_wins",
            "0",
            "NEW.wins-OLD.wins",
            true,
        ),
        (
            "arena_losses",
            "arena_scores",
            "UPDATE",
            "NEW.kind=0 AND NEW.losses>OLD.losses",
            "NEW.account",
            "arena_losses",
            "0",
            "NEW.losses-OLD.losses",
            true,
        ),
    ] {
        let update = if accumulate {
            "value+excluded.value"
        } else {
            "MAX(value,excluded.value)"
        };
        sqlx::query(&format!("CREATE TRIGGER IF NOT EXISTS play_record_{name} AFTER {event} ON {table} WHEN {condition} BEGIN INSERT INTO play_record_values(account_id,kind,idx,value) VALUES({account},'{kind}',{idx},{value}) ON CONFLICT(account_id,kind,idx) DO UPDATE SET value={update}; END")).execute(&mut *tx).await?;
    }
    tx.commit().await?;
    Ok(())
}

pub(crate) async fn best(
    db: &mut SqliteConnection,
    account: i64,
    kind: &str,
    value: i64,
) -> Result<()> {
    sqlx::query("INSERT INTO play_record_values(account_id,kind,idx,value) VALUES(?,?,0,?) ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=MAX(value,excluded.value)")
        .bind(account).bind(kind).bind(value).execute(db).await?;
    Ok(())
}

/// Only an actual liberation counts; moving an existing weapon between slots does not.
pub(crate) async fn liberated(db: &mut SqliteConnection, account: i64) -> Result<()> {
    sqlx::query("INSERT INTO play_record_values(account_id,kind,idx,value) VALUES(?,'soul_count',0,1) ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=value+1")
        .bind(account).execute(db).await?;
    Ok(())
}

pub(super) async fn snapshot(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    view: &View,
) -> Result<Vec<Value>> {
    let facts: Vec<(String, i64, i64)> =
        sqlx::query_as("SELECT kind,idx,value FROM play_record_values WHERE account_id=?")
            .bind(account)
            .fetch_all(&mut *db)
            .await?;
    let fact = |kind: &str| {
        facts
            .iter()
            .filter(|v| v.0 == kind)
            .map(|v| v.2)
            .max()
            .unwrap_or(0)
    };
    let created: String = sqlx::query_scalar("SELECT created_at FROM accounts WHERE account_id=?")
        .bind(account)
        .fetch_one(&mut *db)
        .await?;
    let mut records = BTreeMap::<i64, String>::new();
    records.insert(1, created);
    records.insert(2, view.login_days.to_string());
    // Dates, not elapsed 24-hour periods; repeated logins do not advance streaks.
    let mut dates: BTreeSet<String> = view
        .metrics
        .iter()
        .filter(|m| m.1 == "LoginDaily" && m.4 > 0)
        .map(|m| m.0.clone())
        .collect();
    if let Some(day) =
        sqlx::query_scalar::<_, String>("SELECT last_day FROM progression_login WHERE account_id=?")
            .bind(account)
            .fetch_optional(&mut *db)
            .await?
    {
        if !day.is_empty() {
            dates.insert(day);
        }
    }
    let (mut streak, mut longest, mut previous) = (0, 0, None::<chrono::NaiveDate>);
    for date in dates
        .iter()
        .filter_map(|d| chrono::NaiveDate::parse_from_str(d, "%Y-%m-%d").ok())
    {
        streak = if previous.is_some_and(|p| date.signed_duration_since(p).num_days() == 1) {
            streak + 1
        } else {
            1
        };
        longest = longest.max(streak);
        previous = Some(date);
    }
    records.insert(3, streak.to_string());
    records.insert(4, longest.to_string());
    let chapter = view
        .dungeons
        .iter()
        .filter(|d| {
            d.2 > 0
                && state
                    .tables
                    .battle
                    .find("CampaignChapter", &[("Index", d.0)])
                    .is_some_and(|c| c["MainField"] == true)
        })
        .map(|d| d.0)
        .max();
    if let Some(c) = chapter {
        records.insert(6, c.to_string());
    }
    records.insert(7, view.costumes.to_string());
    for (id, gachas) in [(11, [3, 4]), (12, [1, 2])] {
        records.insert(
            id,
            view.metrics
                .iter()
                .filter(|m| m.1 == "ExecEquipGacha" && gachas.contains(&m.2))
                .map(|m| m.4)
                .sum::<i64>()
                .to_string(),
        );
    }
    for (id, key, minimum) in [
        (101, "Star", 1),
        (102, "Level", 80),
        (103, "Transcended", 1),
        (104, "Transcended", 2),
        (105, "Transcended", 3),
        (106, "Transcended", 4),
        (107, "Transcended", 5),
        (109, "Level", 90),
        (110, "Level", 100),
    ] {
        records.insert(
            id,
            view.heroes
                .iter()
                .filter(|h| n(h, key) >= minimum)
                .count()
                .to_string(),
        );
    }
    records.insert(
        108,
        view.heroes
            .iter()
            .filter(|h| {
                (1..=4).all(|i| {
                    n(h, &format!("SkillExtend{i}"))
                        >= state.tables.hero_shop.constant("MaxSkillExtend", 3)
                })
            })
            .count()
            .to_string(),
    );
    let equipment: BTreeMap<_, _> = state
        .tables
        .extensions
        .rows("EquipItem")
        .iter()
        .map(|m| (n(m, "ItemIndex"), m))
        .collect();
    for (count_id, star_id, part, details) in [
        (201, 203, 1, vec![90001, 90003]),
        (204, 206, 5, vec![]),
        (207, 209, 7, vec![90004]),
    ] {
        let matches = |idx| {
            equipment.get(&idx).is_some_and(|m| {
                n(m, "PartType") == part
                    && (details.is_empty() || details.contains(&n(m, "DetailIndex")))
            })
        };
        records.insert(
            count_id,
            facts
                .iter()
                .filter(|f| f.0 == "equip_count" && matches(f.1))
                .map(|f| f.2)
                .sum::<i64>()
                .to_string(),
        );
        records.insert(
            star_id,
            facts
                .iter()
                .filter(|f| f.0 == "equip_star" && matches(f.1))
                .map(|f| f.2)
                .max()
                .unwrap_or(0)
                .to_string(),
        );
    }
    for (id, kind) in [
        (301, "soul_count"),
        (302, "soul_grade"),
        (303, "soul_reinforce"),
        (401, "arena_score"),
        (402, "arena_tier"),
        (403, "arena_streak"),
        (404, "arena_wins"),
        (405, "arena_losses"),
    ] {
        records.insert(id, fact(kind).to_string());
    }
    let clears: Vec<_> = view
        .metrics
        .iter()
        .filter(|m| m.1 == "ClearDungeon" && m.4 > 0)
        .collect();
    for (id, raids) in [
        (606, vec![1, 101, 11, 21]),
        (608, vec![3, 103, 13, 23]),
        (610, vec![2, 102, 12, 22]),
        (612, vec![4, 104, 14, 24]),
        (
            614,
            (50001..=50007)
                .chain(60001..=60007)
                .chain(70001..=70016)
                .collect(),
        ),
    ] {
        let stages: BTreeSet<_> = state
            .tables
            .battle
            .rows("Raid")
            .iter()
            .filter(|r| raids.contains(&n(r, "Index")))
            .map(|r| (n(r, "ChapterIndex"), n(r, "DungeonIndex")))
            .collect();
        let count: i64 = clears
            .iter()
            .filter(|m| stages.contains(&(m.2, m.3)))
            .map(|m| m.4)
            .sum();
        records.insert(id, count.to_string());
    }
    let battle_types: BTreeMap<_, _> = state
        .tables
        .battle
        .rows("CampaignDungeon")
        .iter()
        .map(|d| {
            (
                (n(d, "ChapterIndex"), n(d, "DungeonIndex")),
                n(d, "BattleType"),
            )
        })
        .collect();
    for (id, tower) in [(619, 21), (623, 11)] {
        let floors: Vec<_> = state
            .tables
            .battle
            .rows("TowerFloor")
            .iter()
            .filter(|f| n(f, "TowerIndex") == tower)
            .collect();
        let matches = |f: &&Value, m: &&(String, String, i64, i64, i64)| {
            n(f, "ChapterIndex") == m.2
                && (n(f, "DungeonIndex") == m.3
                    || f["DungeonIndices"]
                        .as_array()
                        .is_some_and(|ids| ids.contains(&json!(m.3))))
        };
        let battle_type = if tower == 21 { 14 } else { 13 };
        records.insert(
            id,
            clears
                .iter()
                .filter(|m| battle_types.get(&(m.2, m.3)) == Some(&battle_type))
                .map(|m| m.4)
                .sum::<i64>()
                .to_string(),
        );
        if tower == 21 {
            records.insert(
                620,
                floors
                    .iter()
                    .filter(|f| clears.iter().any(|m| matches(f, m)))
                    .map(|f| n(f, "Floor"))
                    .max()
                    .unwrap_or(0)
                    .to_string(),
            );
        }
    }
    records.insert(
        627,
        clears
            .iter()
            .filter(|m| battle_types.get(&(m.2, m.3)) == Some(&37))
            .map(|m| m.4)
            .sum::<i64>()
            .to_string(),
    );
    let eclipse: Option<String> = sqlx::query_scalar(
        "SELECT data FROM battle_state WHERE account=? AND kind='eclipse_record' AND idx=0",
    )
    .bind(account)
    .fetch_optional(&mut *db)
    .await?;
    let eclipse = eclipse
        .map(|s| serde_json::from_str::<Value>(&s))
        .transpose()
        .map_err(|e| ServerError::Internal(e.to_string()))?;
    records.insert(
        626,
        eclipse
            .as_ref()
            .map(|v| n(v, "BestWave"))
            .unwrap_or(0)
            .to_string(),
    );
    Ok(records.into_iter().map(|(index,value)|json!({"RecordIndex":index,"RecordValue":value,"UpdatedTime":state.server_time_str()})).collect())
}
