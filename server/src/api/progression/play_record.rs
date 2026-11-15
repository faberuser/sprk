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

/// The table lookups the records need, built once per loaded tables (a
/// snapshot is rebuilt after most gameplay writes).
struct Lookups {
    /// EquipItem (PartType, DetailIndex) by ItemIndex (the last row wins).
    equipment: std::collections::HashMap<i64, (i64, i64)>,
    /// CampaignDungeon BattleType by (ChapterIndex, DungeonIndex) (the last row wins).
    battle_types: std::collections::HashMap<(i64, i64), i64>,
    /// Raid rows: (Index, (ChapterIndex, DungeonIndex)).
    raids: Vec<(i64, (i64, i64))>,
    /// TowerFloor rows by TowerIndex: (Floor, ChapterIndex, DungeonIndex, DungeonIndices).
    floors: std::collections::HashMap<i64, Vec<(i64, i64, i64, Vec<i64>)>>,
}
fn lookups(state: &AppState) -> std::sync::Arc<Lookups> {
    use std::sync::Arc;
    type Cached = (Arc<crate::tables::BattleTable>, Arc<crate::tables::ExtensionTable>, Arc<Lookups>);
    static CACHE: std::sync::Mutex<Option<Cached>> = std::sync::Mutex::new(None);
    let mut cache = CACHE.lock().unwrap_or_else(|e| e.into_inner());
    if let Some((battle, extensions, lookups)) = cache.as_ref() {
        if Arc::ptr_eq(battle, &state.tables.battle) && Arc::ptr_eq(extensions, &state.tables.extensions) {
            return lookups.clone();
        }
    }
    let battle = &state.tables.battle;
    let mut floors = std::collections::HashMap::<i64, Vec<_>>::new();
    for f in battle.rows("TowerFloor") {
        let indices = f["DungeonIndices"].as_array().into_iter().flatten().filter_map(Value::as_i64).collect();
        floors.entry(n(f, "TowerIndex")).or_default().push((n(f, "Floor"), n(f, "ChapterIndex"), n(f, "DungeonIndex"), indices));
    }
    let lookups = Arc::new(Lookups {
        equipment: state.tables.extensions.rows("EquipItem").iter().map(|m| (n(m, "ItemIndex"), (n(m, "PartType"), n(m, "DetailIndex")))).collect(),
        battle_types: battle.rows("CampaignDungeon").iter().map(|d| ((n(d, "ChapterIndex"), n(d, "DungeonIndex")), n(d, "BattleType"))).collect(),
        raids: battle.rows("Raid").iter().map(|r| (n(r, "Index"), (n(r, "ChapterIndex"), n(r, "DungeonIndex")))).collect(),
        floors,
    });
    *cache = Some((state.tables.battle.clone(), state.tables.extensions.clone(), lookups.clone()));
    lookups
}

/// Every native play record, from the loaded view only.
pub(super) fn snapshot(state: &AppState, view: &View) -> Vec<Value> {
    let tables = lookups(state);
    let facts = &view.hist(super::state::part::FACTS).facts;
    let fact = |kind: &str| {
        facts
            .iter()
            .filter(|v| v.0 == kind)
            .map(|v| v.2)
            .max()
            .unwrap_or(0)
    };
    let mut records = BTreeMap::<i64, String>::new();
    records.insert(1, view.created().to_owned());
    records.insert(2, view.login_days().to_string());
    // Dates, not elapsed 24-hour periods; repeated logins do not advance streaks.
    let mut dates: BTreeSet<String> = view
        .metrics_of("LoginDaily")
        .iter()
        .filter(|m| m.1 == "LoginDaily" && m.4 > 0)
        .map(|m| m.0.clone())
        .collect();
    if let Some(day) = view.last_login_day() {
        if !day.is_empty() {
            dates.insert(day.to_owned());
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
        .dungeons()
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
    records.insert(7, view.costumes().to_string());
    for (id, gachas) in [(11, [3, 4]), (12, [1, 2])] {
        records.insert(
            id,
            view.metrics_of("ExecEquipGacha")
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
            view.heroes()
                .iter()
                .filter(|h| n(h, key) >= minimum)
                .count()
                .to_string(),
        );
    }
    records.insert(
        108,
        view.heroes()
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
    for (count_id, star_id, part, details) in [
        (201, 203, 1, vec![90001, 90003]),
        (204, 206, 5, vec![]),
        (207, 209, 7, vec![90004]),
    ] {
        let matches = |idx| {
            tables.equipment.get(&idx).is_some_and(|&(part_type, detail)| {
                part_type == part && (details.is_empty() || details.contains(&detail))
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
        .metrics_of("ClearDungeon")
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
        let stages: BTreeSet<_> = tables
            .raids
            .iter()
            .filter(|r| raids.contains(&r.0))
            .map(|r| r.1)
            .collect();
        let count: i64 = clears
            .iter()
            .filter(|m| stages.contains(&(m.2, m.3)))
            .map(|m| m.4)
            .sum();
        records.insert(id, count.to_string());
    }
    let battle_types = &tables.battle_types;
    // Each cleared stage once, for matching tower floors.
    let cleared: BTreeSet<(i64, i64)> = clears.iter().map(|m| (m.2, m.3)).collect();
    for (id, tower) in [(619, 21), (623, 11)] {
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
            // A floor is cleared by its own stage or any of its DungeonIndices.
            records.insert(
                620,
                tables.floors.get(&tower).into_iter().flatten()
                    .filter(|(_, chapter, dungeon, dungeons)| cleared.contains(&(*chapter, *dungeon))
                        || dungeons.iter().any(|d| cleared.contains(&(*chapter, *d))))
                    .map(|(floor, ..)| *floor)
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
    records.insert(
        626,
        view.hist(super::state::part::BATTLE).battle.get("eclipse_record")
            .map(|v| n(v, "BestWave"))
            .unwrap_or(0)
            .to_string(),
    );
    records.into_iter().map(|(index,value)|json!({"RecordIndex":index,"RecordValue":value,"UpdatedTime":state.server_time_str()})).collect()
}
