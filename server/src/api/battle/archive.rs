//! Monster Index (Illustrated Book) records. The client colors a monster and
//! counts it toward the collection gauge once the server lists it in a
//! Monster PlayerArchiveInfo (MainIndex 10, SubIndex = NShared.ArchiveType,
//! ArchiveValue = comma-separated creature indices). The server records the
//! dungeon's archive monsters when a battle ends and sends the new ones in
//! ReservedPlayerArchiveInfos, which the client applies after every response.
use super::{now, time};
use crate::api::system::request::Request;
use crate::error::Result;
use crate::state::AppState;
use crate::tables::ArchiveMonster;
use serde_json::{json, Value};
use sqlx::PgConnection;
use std::collections::{BTreeMap, BTreeSet};

/// Bosses whose HP pool outlasts one battle (Guild Raid, World Boss, Guild
/// Suppress): a single run never defeats them, so fighting one records it.
const FOUGHT_TYPES: [i64; 3] = [3, 4, 8];

fn flag(r: &Request, key: &str) -> bool {
    matches!(r.text(key), "true" | "True" | "1")
}

/// MonsterIndices/KillCounts: the creatures this battle killed. The native
/// client sends JSON arrays of numbers or numeric strings.
fn killed(r: &Request) -> BTreeSet<i64> {
    let parse = |key: &str| -> Vec<i64> {
        serde_json::from_str::<Vec<Value>>(r.text(key)).unwrap_or_default().iter()
            .map(|v| v.as_i64().or_else(|| v.as_str().and_then(|s| s.parse().ok())).unwrap_or(0))
            .collect()
    };
    let counts = parse("KillCounts");
    parse("MonsterIndices").into_iter().enumerate()
        .filter(|(i, _)| counts.get(*i).is_none_or(|count| *count > 0))
        .map(|(_, index)| index)
        .collect()
}

/// The archive monsters an ended battle records: every monster of a cleared
/// battle, otherwise the ones it killed and any shared-HP boss it fought.
/// Only monsters of the entered dungeon count, so a forged kill list adds
/// nothing the battle could not have contained.
pub(crate) fn defeated(s: &AppState, r: &Request) -> Result<Vec<ArchiveMonster>> {
    let (c, d) = (r.number("ChapterIndex", 0)?, r.number("DungeonIndex", 0)?);
    let difficulty = r.number("DungeonDifficulty", 1)?;
    let completed = flag(r, "Completed");
    let killed = if completed { BTreeSet::new() } else { killed(r) };
    Ok(s.tables.monster_archive.monsters(c, d, difficulty, flag(r, "ScenarioDungeon")).iter()
        .filter(|(kind, index)| completed || FOUGHT_TYPES.contains(kind) || killed.contains(index))
        .copied()
        .collect())
}

/// Store `monsters` and return the ones not recorded before.
pub(crate) async fn add(db: &mut PgConnection, a: i64, monsters: &[ArchiveMonster]) -> Result<Vec<ArchiveMonster>> {
    if monsters.is_empty() {
        return Ok(vec![]);
    }
    Ok(crate::db::query_as(
        "INSERT INTO monster_archive(account_id,archive_type,monster,recorded_time) \
         SELECT $1,(e->>0)::smallint,(e->>1)::bigint,$3 FROM jsonb_array_elements($2::jsonb) e \
         ON CONFLICT DO NOTHING RETURNING archive_type::bigint,monster")
        .bind(a).bind(json!(monsters)).bind(time(now()))
        .fetch_all(&mut *db).await?)
}

/// After a battle request succeeds: record what it defeated and attach the
/// new entries to the response.
pub(super) async fn record(db: &mut PgConnection, s: &AppState, a: i64, r: &Request, out: &mut Value) -> Result<()> {
    let added = add(db, a, &defeated(s, r)?).await?;
    if !added.is_empty() {
        out["ReservedPlayerArchiveInfos"] = json!(infos(s, &added));
    }
    Ok(())
}

/// One PlayerArchiveInfo per archive type.
pub(crate) fn infos(s: &AppState, monsters: &[ArchiveMonster]) -> Vec<Value> {
    let mut by_type: BTreeMap<i64, BTreeSet<i64>> = BTreeMap::new();
    for (kind, index) in monsters {
        by_type.entry(*kind).or_default().insert(*index);
    }
    let updated = time(now());
    by_type.into_iter().map(|(kind, indices)| json!({
        "ArchiveMainIndex": s.tables.monster_archive.main_index,
        "ArchiveSubIndex": kind,
        "ArchiveValue": indices.iter().map(i64::to_string).collect::<Vec<_>>().join(","),
        "UpdatedTime": updated,
    })).collect()
}

/// Login PlayerArchiveInfos: the recorded monsters plus those of every stage
/// the account cleared before the archive was recorded, from its dungeon
/// progress (`(chapter, dungeon, FirstRewardedDiff bits, ScenarioComplete)`).
pub(crate) async fn login_infos(
    db: &mut PgConnection,
    s: &AppState,
    a: i64,
    cleared: impl IntoIterator<Item = (i64, i64, i64, bool)>,
) -> Result<Vec<Value>> {
    let mut monsters: BTreeSet<ArchiveMonster> = crate::db::query_as(
        "SELECT archive_type::bigint,monster FROM monster_archive WHERE account_id=$1")
        .bind(a).fetch_all(&mut *db).await?.into_iter().collect();
    let table = &s.tables.monster_archive;
    for (c, d, difficulties, scenario) in cleared {
        for difficulty in (0..4).filter(|bit| difficulties & (1 << bit) != 0) {
            monsters.extend(table.monsters(c, d, difficulty, false));
        }
        if scenario {
            monsters.extend(table.monsters(c, d, 1, true));
        }
    }
    Ok(infos(s, &monsters.into_iter().collect::<Vec<_>>()))
}
