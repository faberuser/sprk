//! Native PlayRecordTable row IDs. Historical facts survive inventory/season resets;
//! ownership rows are intentionally computed from the current account snapshot.
use super::*;
use std::collections::BTreeMap;


pub(crate) async fn best(
    db: &mut PgConnection,
    account: i64,
    kind: &str,
    value: i64,
) -> Result<()> {
    crate::db::query("INSERT INTO play_record_values(account_id,kind,idx,value) VALUES($1,$2,0,$3) ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=GREATEST(play_record_values.value,excluded.value)")
        .bind(account).bind(kind).bind(value).execute(db).await?;
    Ok(())
}

/// Only an actual liberation counts; moving an existing weapon between slots does not.
pub(crate) async fn liberated(db: &mut PgConnection, account: i64) -> Result<()> {
    crate::db::query("INSERT INTO play_record_values(account_id,kind,idx,value) VALUES($1,'soul_count',0,1) ON CONFLICT(account_id,kind,idx) DO UPDATE SET value=play_record_values.value+1")
        .bind(account).execute(db).await?;
    Ok(())
}

pub(super) async fn snapshot(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    view: &View,
) -> Result<Vec<Value>> {
    let facts: Vec<(String, i64, i64)> =
        crate::db::query_as("SELECT kind,idx,value FROM play_record_values WHERE account_id=$1")
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
    let created: String = crate::db::query_scalar("SELECT created_at FROM accounts WHERE account_id=$1")
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
        crate::db::query_scalar::<String>("SELECT last_day FROM progression_login WHERE account_id=$1")
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
    let eclipse: Option<String> = crate::db::query_scalar(
        "SELECT data FROM battle_state WHERE account=$1 AND kind='eclipse_record' AND idx=0",
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
