//! Permanent Piece of Memory episodes, using the archived story and NPC gates.
use crate::{error::Result, state::AppState};
use crate::api::inventory::item::{n, rule};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};

#[cfg(test)]
mod tests;

pub(crate) struct Episode {
    pub chapter: i64,
    pub npc: i64,
    gate: (i64, i64),
    shop: &'static str,
    title: &'static str,
}

const EPISODES: &[Episode] = &[
    Episode { chapter: 30031, npc: 827, gate: (9, 15), shop: "HeroDungeon_Pancironne_Shop", title: "Pansirone" },
    Episode { chapter: 30035, npc: 831, gate: (50000, 1), shop: "2020_Xerah_Shop", title: "Xerah" },
    Episode { chapter: 30036, npc: 835, gate: (97, 2), shop: "", title: "Cycle of Fate" },
    Episode { chapter: 30037, npc: 837, gate: (97, 2), shop: "2020_Hilda_Shop", title: "Hilda" },
    Episode { chapter: 30039, npc: 838, gate: (10, 1), shop: "2020_RebelClause_Shop", title: "Rebel Clause" },
    Episode { chapter: 30040, npc: 839, gate: (10, 1), shop: "2020_RiheetRipine_Shop", title: "Riheet & Ripine" },
    Episode { chapter: 30041, npc: 80, gate: (8, 26), shop: "2021_Shakmeh_Shop", title: "Shakmeh" },
];

pub(crate) fn chapter(chapter: i64) -> Option<&'static Episode> {
    EPISODES.iter().find(|e| e.chapter == chapter)
}

pub(crate) fn npc(npc: i64) -> Option<&'static Episode> {
    EPISODES.iter().find(|e| e.npc == npc)
}

fn enabled(s: &AppState) -> bool {
    s.tables.battle.rules["EnablePieceOfMemoryEpisodes"] != false
}

/// EventExist controls the existing episode battle and Present menu buttons.
pub(crate) fn events(s: &AppState) -> Vec<Value> {
    if !enabled(s) { return vec![]; }
    let mut events = vec![];
    for e in EPISODES {
        // Native BaseJsonMarshaler.ParseEnum accepts enum names, not JSON numbers.
        for (id, bonus, condition) in [(20, "EventDungeon", e.chapter.to_string()), (18, "EventShop", e.shop.to_string())] {
            if condition.is_empty() { continue; }
            events.push(json!({"Id":e.chapter * 10 + id,
                "DayOfWeek":"", "BeginTime":"2020-01-01 00:00:00", "EndTime":"2099-01-01 00:00:00",
                "BeginHour":0,"BeginMinute":0,"EndHour":0,"EndMinute":0,
                "DungeonType":["EventDungeon"],"BonusType":bonus,"Condition":condition,
                "ItemCode":"","ItemIndices":[],"BonusPercentage":0,
                "Title":null,"Message":[],"LocalInfo":[{"Language":"English","Title":e.title,"Message":"Piece of Memory"}],
                "MoveTaskType":"None","MoveTaskValue":"","MinLevel":0,"BuffPosition":"None"}));
        }
    }
    events
}

async fn cleared(db: &mut SqliteConnection, account: i64, chapter: i64, dungeon: i64) -> Result<bool> {
    let native: Option<String> = sqlx::query_scalar("SELECT data FROM battle_state WHERE account=? AND kind='dungeon' AND idx=?")
        .bind(account).bind((chapter << 32) | dungeon).fetch_optional(&mut *db).await?;
    if let Some(native) = native {
        let info: Value = serde_json::from_str(&native).map_err(|_| rule("InvalidProgress"))?;
        // A testing reset keeps reward flags, so those flags alone are not a clear.
        return Ok(n(&info, "MaxStar") % 10 > 0 && info["CompletedTime"].as_str().is_some_and(|v| !v.is_empty()));
    }
    Ok(sqlx::query("SELECT clear_count,completed_time FROM campaign_progress WHERE account_id=? AND chapter_id=? AND dungeon_id=?")
        .bind(account).bind(chapter).bind(dungeon).fetch_optional(db).await?
        .is_some_and(|r| r.get::<i64,_>("clear_count") > 0 && r.get::<Option<String>,_>("completed_time").is_some()))
}

pub(crate) async fn require_battle(db: &mut SqliteConnection, s: &AppState, account: i64,
    chapter_id: i64, dungeon: i64, difficulty: i64) -> Result<()> {
    let Some(e) = chapter(chapter_id) else { return Ok(()); };
    if !enabled(s) { return Err(rule("NotOpenedDungeon")); }
    // All seven archived episode routes have Easy reward/preset definitions.
    if difficulty != 0 { return Err(rule("InvalidDiff")); }
    let data = s.tables.battle.find("EventDungeon", &[("ChapterIndex", chapter_id), ("DungeonIndex", dungeon)])
        .ok_or_else(|| rule("DungeonDataNotFound"))?;
    for condition in ["OpenType1", "OpenType2"] {
        match n(data, condition) {
            29 if !cleared(db, account, e.gate.0, e.gate.1).await? => return Err(rule("NotCompletedReqDungeon")),
            9 if dungeon > 1 && !cleared(db, account, chapter_id, dungeon - 1).await? => return Err(rule("CannotVisitDungeon")),
            _ => {}
        }
    }
    Ok(())
}

pub(crate) fn require_npc(s: &AppState, id: i64) -> Result<bool> {
    if npc(id).is_none() { return Ok(false); }
    if !enabled(s) { return Err(rule("NPCNotFound")); }
    Ok(true)
}
