//! K.O. step rewards of phase-step event dungeons (PhaseStepRewardTable; in the
//! archive only Thanksgiving Defense, chapter 30047).
//!
//! Each battle's kills (EndCampaign `KillCounts`) reach a Daily level; every
//! level pays out once per day with that battle's loot, as the client's
//! "received the Level {2} rewards for defending against {1} thieves" popup
//! describes. Kills also add up to a run total whose General milestones the
//! player claims in the K.O. rewards window. Progress is scoped to the
//! festival run, like the event's currency and shop limits.
use super::Calendar;
use crate::api::extensions::{get, put};
use crate::api::inventory::item::{self, n, rule};
use crate::api::system::request::Request;
use crate::api::tutorial::Rewards;
use crate::{error::Result, state::AppState};
use serde_json::{json, Value};
use sqlx::PgConnection;

const KIND: &str = "phase_step";
/// Kills one battle may report. The archived dungeon's top Daily level needs
/// 200; this bounds what a forged result can add to the run total.
const DEFAULT_MAX_KILLS: i64 = 500;
const DAY: i64 = 86_400;

fn steps<'a>(s: &'a AppState, chapter: i64, dungeon: i64, kind: &'a str) -> impl Iterator<Item = &'a Value> + 'a {
    s.tables.battle.rows("PhaseStepReward").iter()
        .filter(move |r| n(r, "ChapterIndex") == chapter && n(r, "DungeonIndex") == dungeon && r["Type"] == kind)
}

pub(crate) fn exists(s: &AppState, chapter: i64, dungeon: i64) -> bool {
    steps(s, chapter, dungeon, "Daily").next().is_some() || steps(s, chapter, dungeon, "General").next().is_some()
}

fn subkey(chapter: i64, dungeon: i64) -> i64 {
    chapter * 1000 + dungeon
}

/// Stored progress, reset for a new festival run and the Daily level for a new day.
async fn load(db: &mut PgConnection, account: i64, chapter: i64, dungeon: i64, season: i64, now: i64) -> Result<Value> {
    let mut v = get(db, account, KIND, subkey(chapter, dungeon)).await?;
    if !v.is_object() || n(&v, "Season") != season {
        v = json!({"Season":season,"Day":0,"DailyStep":0,"TotalCount":0,"TotalStep":0});
    }
    if n(&v, "Day") != now.div_euclid(DAY) {
        v["Day"] = json!(now.div_euclid(DAY));
        v["DailyStep"] = json!(0);
    }
    Ok(v)
}

/// Run that scopes the progress: the open run, else the stored one, so the
/// window still shows the last run's count while the festival is closed.
async fn season(db: &mut PgConnection, account: i64, chapter: i64, dungeon: i64, cal: &Calendar<'_>) -> Result<(i64, bool)> {
    if let Some(open) = cal.chapter_season(chapter) {
        return Ok((open, true));
    }
    if cal.manages_chapter(chapter) {
        let stored = n(&get(db, account, KIND, subkey(chapter, dungeon)).await?, "Season");
        return Ok((stored, false));
    }
    Ok((0, true))
}

/// Total kills an ended battle reports (`KillCounts`, numbers or numeric strings).
pub(crate) fn kills(s: &AppState, r: &Request) -> i64 {
    let max = s.tables.live.rules["Festivals"]["PhaseStepMaxKills"].as_i64().unwrap_or(DEFAULT_MAX_KILLS);
    serde_json::from_str::<Vec<Value>>(r.text("KillCounts")).unwrap_or_default().iter()
        .map(|v| v.as_i64().or_else(|| v.as_str().and_then(|s| s.parse().ok())).unwrap_or(0).max(0))
        .fold(0i64, i64::saturating_add)
        .min(max)
}

/// EndCampaign of a phase-step dungeon: count the kills, pay the Daily levels
/// reached for the first time today, and add them to the battle's results.
pub(crate) async fn finish(
    db: &mut PgConnection, s: &AppState, account: i64, chapter: i64, dungeon: i64, kills: i64, out: &mut Value,
) -> Result<()> {
    if !exists(s, chapter, dungeon) { return Ok(()); }
    let cal = Calendar::load(db, s).await?;
    // A battle that outlasts its run still counts toward that run.
    let (season, _) = season(db, account, chapter, dungeon, &cal).await?;
    let mut v = load(db, account, chapter, dungeon, season, cal.now).await?;
    v["TotalCount"] = json!(n(&v, "TotalCount").saturating_add(kills));
    let old = n(&v, "DailyStep");
    let mut rewards = Rewards { mail_overflow: true, ..Rewards::default() };
    let mut level = old;
    for row in steps(s, chapter, dungeon, "Daily") {
        let step = n(row, "Step");
        if step > old && kills >= n(row, "ReqValue") {
            item::reward(db, s, account, n(row, "RewardIndex") as i32, &mut rewards).await?;
            level = level.max(step);
        }
    }
    v["DailyStep"] = json!(level);
    put(db, account, KIND, subkey(chapter, dungeon), &v).await?;
    if level > old {
        let granted = item::reward_response(db, s, account, rewards).await?;
        for (from, to) in [("CurrencyResults", "CurrencyResults"), ("ItemResults", "ItemResults"),
            ("EquipItemResults", "EquipItemInfos"), ("HeroInfos", "HeroInfos"), ("PetInfos", "PetInfos")] {
            let added = granted[from].as_array().cloned().unwrap_or_default();
            if added.is_empty() { continue; }
            if !out[to].is_array() { out[to] = json!([]); }
            out[to].as_array_mut().into_iter().for_each(|list| list.extend(added.iter().cloned()));
        }
    }
    Ok(())
}

/// phase_step/get_phase_step_info and phase_step/recv_reward_phase_step.
pub(crate) async fn execute(db: &mut PgConnection, s: &AppState, account: i64, r: &Request, action: &str) -> Result<Value> {
    let (chapter, dungeon) = (r.number("ChapterIndex", 0)?, r.number("DungeonIndex", 0)?);
    if chapter <= 0 || dungeon <= 0 { return Err(rule("RequiredValueEmpty")); }
    if !exists(s, chapter, dungeon) { return Err(rule("PhaseStepDataNotFound")); }
    let cal = Calendar::load(db, s).await?;
    let (season, open) = season(db, account, chapter, dungeon, &cal).await?;
    let mut v = load(db, account, chapter, dungeon, season, cal.now).await?;
    if action == "get_phase_step_info" {
        return Ok(json!({"DailyAchievementStep":n(&v,"DailyStep"),"TotalAchievementStep":n(&v,"TotalStep"),
            "TotalCount":n(&v,"TotalCount")}));
    }
    // Daily levels are paid with the battle; only milestones are claimed.
    if r.text("Type") != "General" { return Err(rule("InvalidStep")); }
    if !open { return Err(rule("EventDungeonClosed")); }
    let (old, total) = (n(&v, "TotalStep"), n(&v, "TotalCount"));
    let mut rewards = Rewards { mail_overflow: true, ..Rewards::default() };
    let mut step = old;
    for row in steps(s, chapter, dungeon, "General") {
        if n(row, "Step") > old && total >= n(row, "ReqValue") {
            item::reward(db, s, account, n(row, "RewardIndex") as i32, &mut rewards).await?;
            step = step.max(n(row, "Step"));
        }
    }
    if step == old { return Err(rule("InvalidStep")); }
    v["TotalStep"] = json!(step);
    put(db, account, KIND, subkey(chapter, dungeon), &v).await?;
    let mut out = item::reward_response(db, s, account, rewards).await?;
    out["AchievementStep"] = json!(step);
    Ok(out)
}
