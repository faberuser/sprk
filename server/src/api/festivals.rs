//! Annual local festival calendar. The dates are SPRK policy (see
//! docs/proposed-event-calendar.md and `Festivals` in LiveRules.json); the
//! content each window opens comes from the archived client tables: hidden
//! world-map nodes, event dungeons, event shops, Ophelia's crafting and
//! roulettes, Lakrak's Forge, the World Tree, event bosses and weekend bonuses.
//!
//! The native client learns what is open only from Login's DropBonusEvents and
//! filters them by time, so `events` also announces upcoming windows.
use crate::api::extensions::{get, put};
use crate::api::inventory::item::{n, rule};
use crate::{error::Result, state::AppState};
use chrono::{Datelike, NaiveDate};
use serde_json::{json, Value};
use sqlx::PgConnection;

pub(crate) mod phase_step;
#[cfg(test)]
mod tests;

const DAY: i64 = 86_400;
/// Server-wide GM overrides live on account 0, like the World Tree total.
const OVERRIDE_KIND: &str = "festival_override";
/// Per account: the festival run each expiring code was last seen in.
const EXPIRY_KIND: &str = "festival_expiry";
/// Per account: participation-gift progress of each festival's current run.
const GIFT_KIND: &str = "festival_gift";

/// Native NShared.BattleType names; DropBonusEvent.DungeonType is parsed by name.
const BATTLE_TYPES: &[(i64, &str)] = &[
    (1, "Campaign"), (2, "CustomCampaign"), (3, "Trial"), (4, "Arena"), (5, "Tower"),
    (6, "Pit"), (7, "Labyrinth"), (8, "Event"), (9, "Party"), (10, "Boss"), (12, "Raid"),
    (13, "OrdealTower"), (14, "ChallengeTower"), (15, "WorldBoss"), (16, "GuildRaid"),
    (17, "Hideout"), (18, "EventDungeon"), (19, "EventWorldBoss"), (20, "ArenaBanPick"),
    (21, "RaidSingle"), (22, "Maze"), (23, "SubStory"), (24, "SubStoryBoss"),
    (25, "ChallengeRaidSingle"), (26, "GuildSuppress"), (27, "TreasureHouse"),
    (28, "SquareSideQuest"), (29, "HardRaid"), (30, "ChallengeRaid"), (31, "Conquest"),
    (32, "GuildArena"), (33, "FieldRaid"), (34, "GatewaytoTrial"), (35, "GodkingTrial"),
    (36, "Eclipse"), (37, "UnderPrison"), (38, "Shakmeh"), (39, "PurchaseDungeon"),
    (40, "GargoriaRaid"), (41, "SeigfriedRaid"), (42, "AscalonRaid"), (43, "OrdealArena"),
    (44, "LuckyArena"), (45, "ArcdimRaid"), (46, "ArcdimHardRaid"), (47, "PunishmentRaid"),
    (48, "KarmaRaid"),
];

/// One run of a festival, `[begin, end)` in UTC seconds.
#[derive(Clone, Copy, Debug)]
pub(crate) struct Occurrence<'a> {
    pub def: &'a Value,
    index: usize,
    pub begin: i64,
    pub end: i64,
}

impl Occurrence<'_> {
    /// Identity of this run: run-scoped progress and purchase limits key on it.
    pub fn season(&self) -> i64 {
        self.begin.div_euclid(DAY)
    }
    pub fn key(&self) -> &str {
        self.def["Key"].as_str().unwrap_or("")
    }
    fn contains(&self, t: i64) -> bool {
        self.begin <= t && t < self.end
    }
}

pub(crate) struct Calendar<'a> {
    config: &'a Value,
    defs: &'a [Value],
    pub now: i64,
    overrides: Value,
}

fn date(t: i64) -> NaiveDate {
    chrono::DateTime::from_timestamp(t, 0).unwrap_or_default().date_naive()
}
fn stamp(d: NaiveDate) -> i64 {
    d.and_hms_opt(0, 0, 0).unwrap_or_default().and_utc().timestamp()
}
pub(crate) fn time(t: i64) -> String {
    chrono::DateTime::from_timestamp(t, 0).unwrap_or_default().format("%Y-%m-%d %H:%M:%S").to_string()
}
/// Midnight of a calendar day; a day past the month's end (February 29) is
/// clamped to the month's last day.
fn day(y: i32, m: u32, d: u32) -> Option<i64> {
    (1..=d).rev().find_map(|d| NaiveDate::from_ymd_opt(y, m, d)).map(stamp)
}
fn month_day(v: &Value) -> Option<(u32, u32)> {
    let (m, d) = v.as_str()?.split_once('-')?;
    Some((m.trim().parse().ok().filter(|m| (1..=12).contains(m))?, d.trim().parse().ok().filter(|d| (1..=31).contains(d))?))
}
fn weekday(v: &Value) -> Option<u32> {
    let name = v.as_str()?.to_ascii_lowercase();
    ["mon", "tue", "wed", "thu", "fri", "sat", "sun"].iter().position(|d| name.starts_with(d)).map(|i| i as u32)
}
fn ints(v: &Value) -> impl Iterator<Item = i64> + '_ {
    v.as_array().into_iter().flatten().filter_map(Value::as_i64)
}
fn strings(v: &Value) -> impl Iterator<Item = &str> + '_ {
    v.as_array().into_iter().flatten().filter_map(Value::as_str)
}

/// The configured windows of one festival overlapping `[from, to)`. End dates
/// are inclusive days; an annual end before its start runs into the next year.
pub(crate) fn windows(def: &Value, from: i64, to: i64) -> Vec<(i64, i64)> {
    let mut out = vec![];
    let (first, last) = (date(from).year() - 1, date(to).year() + 1);
    for w in def["Annual"].as_array().into_iter().flatten() {
        let (Some((m1, d1)), Some((m2, d2))) = (month_day(&w[0]), month_day(&w[1])) else { continue };
        for y in first..=last {
            let end_year = if (m2, d2) < (m1, d1) { y + 1 } else { y };
            if let (Some(b), Some(e)) = (day(y, m1, d1), day(end_year, m2, d2)) {
                out.push((b, e + DAY));
            }
        }
    }
    // Holidays that move each year (Lunar New Year): listed dates, widened by
    // `Before` and `After` days.
    let around = &def["Around"];
    for d in strings(&around["Dates"]) {
        if let Ok(d) = NaiveDate::parse_from_str(d, "%Y-%m-%d") {
            let center = stamp(d);
            out.push((center - n(around, "Before").max(0) * DAY, center + (n(around, "After").max(0) + 1) * DAY));
        }
    }
    let monthly = &def["Monthly"];
    if monthly.is_object() {
        let days: Vec<i64> = ints(&monthly["Days"]).collect();
        let months: Vec<i64> = ints(&monthly["Months"]).collect();
        if let [d1, d2] = days[..] {
            for y in first..=last {
                for m in 1..=12u32 {
                    if !months.is_empty() && !months.contains(&(m as i64)) { continue; }
                    if let (Some(b), Some(e)) = (day(y, m, d1.clamp(1, 31) as u32), day(y, m, d2.clamp(1, 31) as u32)) {
                        out.push((b, e + DAY));
                    }
                }
            }
        }
    }
    let weekly = &def["Weekly"];
    if let Some(start) = weekday(&weekly["From"]) {
        let length = n(weekly, "Days").clamp(1, 7) * DAY;
        let mut d = date(from - 7 * DAY);
        while d.weekday().num_days_from_monday() != start {
            d = d.succ_opt().unwrap_or(d);
        }
        let mut b = stamp(d);
        while b < to {
            out.push((b, b + length));
            b += 7 * DAY;
        }
    }
    out.retain(|(b, e)| *b < to && *e > from);
    out.sort();
    out.dedup();
    out
}

impl<'a> Calendar<'a> {
    pub fn at(s: &'a AppState, now: i64, overrides: Value) -> Self {
        let config = &s.tables.live.rules["Festivals"];
        let defs = config["Events"].as_array().map(Vec::as_slice).unwrap_or(&[]);
        Self { config, defs, now, overrides }
    }

    pub async fn load(db: &mut PgConnection, s: &'a AppState) -> Result<Self> {
        let overrides = get(db, 0, OVERRIDE_KIND, 0).await?;
        Ok(Self::at(s, chrono::Utc::now().timestamp(), overrides))
    }

    fn enabled(&self) -> bool {
        self.config.is_object() && self.config["Enabled"] != false
    }

    /// Festival runs overlapping `[from, to)`, after GM overrides that have
    /// not yet expired: a closed override hides runs it overlaps, and an open
    /// override adds one run of its own.
    pub fn occurrences(&self, from: i64, to: i64) -> Vec<Occurrence<'a>> {
        let mut out = vec![];
        if !self.enabled() { return out; }
        for (index, def) in self.defs.iter().enumerate() {
            if def["Enabled"] == false { continue; }
            let o = &self.overrides[def["Key"].as_str().unwrap_or("")];
            let (since, until) = (n(o, "Since"), n(o, "Until"));
            let mut runs = windows(def, from, to);
            if until > self.now {
                match o["Mode"].as_str() {
                    Some("Closed") => runs.retain(|(b, e)| *e <= since || *b >= until),
                    Some("Open") if since < to && until > from => runs.push((since, until)),
                    _ => {}
                }
            }
            runs.sort();
            out.extend(runs.into_iter().map(|(begin, end)| Occurrence { def, index, begin, end }));
        }
        out
    }

    pub fn active(&self) -> Vec<Occurrence<'a>> {
        self.occurrences(self.now, self.now + 1).into_iter().filter(|o| o.contains(self.now)).collect()
    }

    fn defs_with(&self, field: &str, pred: impl Fn(&Value) -> bool) -> bool {
        self.defs.iter().any(|d| pred(&d[field]))
    }

    /// Whether the calendar controls this event chapter at all.
    pub fn manages_chapter(&self, chapter: i64) -> bool {
        self.enabled() && self.defs_with("Chapters", |v| ints(v).any(|c| c == chapter))
    }
    pub fn chapter_open(&self, chapter: i64) -> bool {
        self.chapter_season(chapter).is_some()
    }
    /// The open run of an event chapter; run-scoped progress keys on it.
    pub fn chapter_season(&self, chapter: i64) -> Option<i64> {
        self.active().iter().find(|o| ints(&o.def["Chapters"]).any(|c| c == chapter)).map(Occurrence::season)
    }
    fn shop_indices(v: &Value) -> impl Iterator<Item = i64> + '_ {
        v.as_object().into_iter().flatten().filter_map(|(_, i)| i.as_i64()).filter(|i| *i > 0)
    }
    /// The open run of an EventOnly shop.
    pub fn shop(&self, index: i64) -> Option<Occurrence<'a>> {
        self.active().into_iter().find(|o| Self::shop_indices(&o.def["Shops"]).any(|i| i == index))
    }
    /// Crafting groups of all open runs, with the run that scopes craft counts.
    pub fn craft(&self) -> Option<(Vec<String>, i64)> {
        let runs: Vec<_> = self.active().into_iter().filter(|o| o.def["Craft"].is_array()).collect();
        let season = runs.iter().map(Occurrence::season).min()?;
        Some((runs.iter().flat_map(|o| strings(&o.def["Craft"]).map(str::to_owned)).collect(), season))
    }
    pub fn roulettes(&self) -> Vec<i64> {
        self.active().iter().flat_map(|o| ints(&o.def["Roulettes"]).collect::<Vec<_>>()).collect()
    }
    pub fn forge(&self) -> bool {
        self.active().iter().any(|o| o.def["Forge"] == true)
    }
    /// Orvel Castle Gold Coins each completed daily mission pays while the forge runs.
    pub fn forge_daily_mission_coins(&self) -> i64 {
        self.active().iter().filter(|o| o.def["Forge"] == true)
            .map(|o| n(&o.def, "DailyMissionCoins").max(0)).max().unwrap_or(0)
    }
    /// The forge's remnant share (numerator, denominator) for a failure or a break.
    pub fn forge_remnant_share(&self, broke: bool) -> (i64, i64) {
        let key = if broke { "BreakRemnantShare" } else { "FailRemnantShare" };
        self.active().iter().filter(|o| o.def["Forge"] == true)
            .find_map(|o| {
                let v = o.def[key].as_array()?;
                let (a, b) = (v.first()?.as_i64()?, v.get(1)?.as_i64()?);
                (a > 0 && b > 0).then_some((a, b))
            })
            .unwrap_or((0, 1))
    }
    /// The open World Tree run; its community total and personal steps are scoped to it.
    pub fn world_tree(&self) -> Option<i64> {
        self.active().iter().find(|o| o.def["WorldTree"] == true).map(Occurrence::season)
    }
    /// Open event bosses. `RotateBosses` runs one retained definition per year.
    pub fn bosses(&self) -> Vec<i64> {
        let mut out = vec![];
        for o in self.active() {
            let list: Vec<i64> = ints(&o.def["Bosses"]).collect();
            if list.is_empty() { continue; }
            if o.def["RotateBosses"] == true {
                out.push(list[date(o.begin).year().rem_euclid(list.len() as i32) as usize]);
            } else {
                out.extend(list);
            }
        }
        out
    }
    /// Summed percentage of an open bonus (`BonusExp`, `BonusGold`, `BonusItem`)
    /// for a battle type.
    pub fn bonus(&self, kind: &str, battle: i64) -> i64 {
        self.active().iter()
            .filter(|o| ints(&o.def["BattleTypes"]).any(|b| b == battle))
            .map(|o| n(&o.def["Bonuses"], kind).max(0)).sum()
    }

    fn event(id: i64, o: &Occurrence, bonus: &str, condition: String, dungeons: Value, percent: i64) -> Value {
        let title = o.def["Title"].as_str().unwrap_or(o.key());
        json!({"Id":id,"DayOfWeek":"","BeginTime":time(o.begin),"EndTime":time(o.end),
            "BeginHour":0,"BeginMinute":0,"EndHour":0,"EndMinute":0,
            "DungeonType":dungeons,"BonusType":bonus,"Condition":condition,
            "ItemCode":"","ItemIndices":[],"BonusPercentage":percent,
            "Title":null,"Message":[],"LocalInfo":[{"Language":"English","Title":title,"Message":title}],
            "MoveTaskType":"None","MoveTaskValue":"","MinLevel":0,"BuffPosition":"None"})
    }

    /// Native DropBonusEvents for open and upcoming runs.
    pub fn events(&self) -> Vec<Value> {
        let ahead = n(self.config, "LookaheadDays").clamp(1, 366);
        let mut out = vec![];
        for o in self.occurrences(self.now, self.now + ahead * DAY) {
            // Stable while a run lasts and unique per festival, run and entry.
            let base = 600_000_000 + o.index as i64 * 2_000_000 + o.season().rem_euclid(20_000) * 100;
            let mut entries: Vec<(&str, String, Value, i64)> = vec![];
            for node in ints(&o.def["Nodes"]) {
                entries.push(("EventNode", format!("99,{node}"), json!([]), 0));
            }
            for chapter in ints(&o.def["Chapters"]) {
                entries.push(("EventDungeon", chapter.to_string(), json!(["EventDungeon"]), 0));
            }
            for code in o.def["Shops"].as_object().into_iter().flatten().map(|(k, _)| k) {
                entries.push(("EventShop", code.clone(), json!(["EventDungeon"]), 0));
            }
            if o.def["Craft"].is_array() {
                entries.push(("EventCraft", json!({"ShowGroups":o.def["Craft"]}).to_string(), json!([]), 0));
            }
            if o.def["Roulettes"].is_array() {
                entries.push(("EventRoulette", json!({"RouletteIndices":o.def["Roulettes"]}).to_string(), json!([]), 0));
            }
            if o.def["WorldTree"] == true {
                entries.push(("GrowWorldTree", String::new(), json!([]), 0));
            }
            let battles: Vec<&str> = ints(&o.def["BattleTypes"])
                .filter_map(|b| BATTLE_TYPES.iter().find(|(id, _)| *id == b).map(|(_, name)| *name)).collect();
            for (kind, percent) in o.def["Bonuses"].as_object().into_iter().flatten() {
                let percent = percent.as_i64().unwrap_or(0);
                if percent > 0 {
                    entries.push((kind.as_str(), String::new(), json!(battles), percent));
                }
            }
            for (i, (bonus, condition, dungeons, percent)) in entries.into_iter().enumerate().take(100) {
                out.push(Self::event(base + i as i64, &o, bonus, condition, dungeons, percent));
            }
        }
        out
    }

    /// Native EventCalendar entries around the current month, and a version
    /// that changes whenever they do.
    pub fn calendar(&self) -> (String, Vec<Value>) {
        let today = date(self.now);
        let from = day(today.year(), today.month(), 1).unwrap_or(self.now) - 31 * DAY;
        let mut out = vec![];
        for o in self.occurrences(from, from + 220 * DAY) {
            if o.def["Calendar"] == false { continue; }
            let (b, e) = (date(o.begin), date(o.end - 1));
            let title = o.def["Title"].as_str().unwrap_or(o.key());
            out.push(json!({"Id":1_000_000 + o.index as i64 * 100_000 + o.season().rem_euclid(100_000),
                "BonusType":0,"Regular":0,"Color":o.def["Color"].as_str().unwrap_or("FFFFFF"),
                "BeginYear":b.year(),"BeginMonth":b.month(),"BeginDay":b.day(),
                "EndYear":e.year(),"EndMonth":e.month(),"EndDay":e.day(),"DayOfWeek":"","Show":1,
                "LocalInfo":{"Language":"en","Title":title,
                    "SimpleDescription":o.def["Description"].as_str().unwrap_or(title),
                    "Description":o.def["Description"].as_str().unwrap_or(title),
                    "Nametag":"Event","NoticeUrl":""}}));
        }
        use std::hash::{Hash, Hasher};
        let mut hash = std::collections::hash_map::DefaultHasher::new();
        Value::Array(out.clone()).to_string().hash(&mut hash);
        (format!("local-{:016x}", hash.finish()), out)
    }

    /// Codes whose balance expires when the festivals that use them close.
    fn expiring(&self) -> Vec<(String, bool)> {
        let mut out: Vec<(String, bool)> = vec![];
        for def in self.defs {
            for (field, item) in [("ExpireCurrencies", false), ("ExpireItems", true)] {
                for code in strings(&def[field]) {
                    if !out.iter().any(|(c, i)| c == code && *i == item) {
                        out.push((code.to_owned(), item));
                    }
                }
            }
        }
        out
    }

    /// Start of the merged run of festivals using `code` that contains now,
    /// or 0 outside them. Adjacent and overlapping runs form one run.
    fn run(&self, code: &str, item: bool) -> i64 {
        let field = if item { "ExpireItems" } else { "ExpireCurrencies" };
        let mut runs: Vec<(i64, i64)> = self.occurrences(self.now - 400 * DAY, self.now + 400 * DAY).into_iter()
            .filter(|o| strings(&o.def[field]).any(|c| c == code)).map(|o| (o.begin, o.end)).collect();
        runs.sort();
        let mut merged: Vec<(i64, i64)> = vec![];
        for (b, e) in runs {
            match merged.last_mut() {
                Some(last) if b <= last.1 => last.1 = last.1.max(e),
                _ => merged.push((b, e)),
            }
        }
        merged.into_iter().find(|(b, e)| *b <= self.now && self.now < *e).map_or(0, |(b, _)| b)
    }

    /// GM status: each festival, whether it is open, and its current or next run.
    pub fn status(&self) -> Vec<Value> {
        let runs = self.occurrences(self.now - 370 * DAY, self.now + 400 * DAY);
        self.defs.iter().enumerate().map(|(index, def)| {
            let current = runs.iter().find(|o| o.index == index && o.contains(self.now));
            let next = runs.iter().find(|o| o.index == index && o.begin > self.now);
            let key = def["Key"].as_str().unwrap_or("");
            json!({"Key":key,"Title":def["Title"],"Open":current.is_some(),
                "Begin":current.map(|o|time(o.begin)),"End":current.map(|o|time(o.end)),
                "NextBegin":next.map(|o|time(o.begin)),"NextEnd":next.map(|o|time(o.end)),
                "Override":self.overrides.get(key).filter(|o|n(o,"Until")>self.now)})
        }).collect()
    }
}

/// Expire festival currencies and items whose run has ended since this account
/// last saw it. An account seen for the first time keeps its balance, so
/// currency collected before scheduling began stays until its next run ends.
pub(crate) async fn expire(db: &mut PgConnection, s: &AppState, account: i64, cal: &Calendar<'_>) -> Result<()> {
    let mut marker = get(db, account, EXPIRY_KIND, 0).await?;
    if !marker.is_object() { marker = json!({}); }
    let before = marker.clone();
    for (code, item) in cal.expiring() {
        let key = if item { format!("item:{code}") } else { code.clone() };
        let current = cal.run(&code, item);
        if marker[&key].as_i64().is_some_and(|previous| previous > 0 && previous != current) {
            if item {
                if let Some(index) = s.tables.get_item_index(&code) {
                    crate::db::query("UPDATE items SET count=0 WHERE account_id=$1 AND item_index=$2 AND count>0")
                        .bind(account).bind(index as i64).execute(&mut *db).await?;
                }
            } else if code == "EventDungeonPoint" {
                crate::db::query("UPDATE user_info SET event_dungeon_point=0 WHERE account_id=$1 AND event_dungeon_point>0")
                    .bind(account).execute(&mut *db).await?;
            } else {
                crate::db::query("UPDATE battle_currencies SET value=0 WHERE account=$1 AND kind=$2 AND value>0")
                    .bind(account).bind(&code).execute(&mut *db).await?;
            }
            tracing::info!(account, code, "Festival balance expired");
        }
        marker[&key] = json!(current);
    }
    if marker != before {
        put(db, account, EXPIRY_KIND, 0, &marker).await?;
    }
    Ok(())
}

/// Participation gifts (`Gifts`): on each day of a run an account logs in it
/// is mailed the `Daily` gift, and after `Days` such days the `Final` one.
pub(crate) async fn gifts(db: &mut PgConnection, s: &AppState, account: i64, cal: &Calendar<'_>) -> Result<()> {
    let mut state = get(db, account, GIFT_KIND, 0).await?;
    if !state.is_object() { state = json!({}); }
    let before = state.clone();
    let today = cal.now.div_euclid(DAY);
    for o in cal.active() {
        let gifts = &o.def["Gifts"];
        if !gifts.is_object() { continue; }
        let key = o.key().to_owned();
        let mut v = state[&key].clone();
        if n(&v, "Season") != o.season() {
            v = json!({"Season":o.season(),"Day":0,"Days":0,"Final":false});
        }
        if n(&v, "Day") == today { continue; }
        v["Day"] = json!(today);
        v["Days"] = json!(n(&v, "Days") + 1);
        // Gifts stay claimable for a week after the festival closes.
        let expires = time(o.end + 7 * DAY);
        mail_gift(db, s, account, &gifts["Daily"], &expires).await?;
        if v["Final"] != true && n(&v, "Days") >= n(gifts, "Days").max(1) && gifts["Final"].is_object() {
            mail_gift(db, s, account, &gifts["Final"], &expires).await?;
            v["Final"] = json!(true);
        }
        state[&key] = v;
    }
    if state != before {
        put(db, account, GIFT_KIND, 0, &state).await?;
    }
    Ok(())
}

async fn mail_gift(db: &mut PgConnection, s: &AppState, account: i64, gift: &Value, expires: &str) -> Result<()> {
    let Some(code) = gift["Item"].as_str() else { return Ok(()) };
    let Some(index) = s.tables.get_item_index(code) else {
        tracing::warn!(code, "Festival gift item is unknown");
        return Ok(());
    };
    let items = vec![crate::api::community::mail::MailItemInfo { item_index: index, item_count: n(gift, "Count").max(1) as i32 }];
    crate::db::query("INSERT INTO mails(account_id,sender,title,content,reward_items,expires_at) VALUES($1,'System',$2,$3,$4,$5)")
        .bind(account).bind(gift["Title"].as_str().unwrap_or("Festival gift")).bind(gift["Content"].as_str().unwrap_or(""))
        .bind(serde_json::to_string(&items).unwrap_or_default()).bind(expires)
        .execute(&mut *db).await?;
    Ok(())
}

/// Battle entry for calendar-controlled event chapters.
pub(crate) async fn require_chapter(db: &mut PgConnection, s: &AppState, account: i64, chapter: i64) -> Result<()> {
    let cal = Calendar::load(db, s).await?;
    if !cal.manages_chapter(chapter) { return Ok(()); }
    if !cal.chapter_open(chapter) { return Err(rule("NotOpenedDungeon")); }
    expire(db, s, account, &cal).await
}

/// Archived event chapters that predate reward-table drops define their
/// festival points per dungeon: a per-mille chance and an amount per difficulty.
pub(crate) async fn dungeon_points(
    db: &mut PgConnection, s: &AppState, account: i64, chapter: i64, dungeon: i64, difficulty: i64,
    count: i64, rewards: &mut crate::api::tutorial::Rewards,
) -> Result<()> {
    let cal = Calendar::at(s, 0, Value::Null);
    if !cal.manages_chapter(chapter) { return Ok(()); }
    let Some(row) = s.tables.battle.find("EventDungeon", &[("ChapterIndex", chapter), ("DungeonIndex", dungeon)]) else {
        return Ok(());
    };
    let diff = ["Easy", "Normal", "Hard", "Hell"][difficulty.clamp(0, 3) as usize];
    for (kind, field) in [("EventDungeonPoint", "EventDungeonPoint"), ("EventDungeonPoint2", "EventDungeonPoint2")] {
        let (rate, amount) = (n(row, &format!("{field}Rate_{diff}")), n(row, &format!("{field}Amount_{diff}")));
        if rate <= 0 || amount <= 0 { continue; }
        let mut total = 0;
        for _ in 0..count.max(0) {
            use rand::RngExt;
            if rand::rng().random_range(0..1000) < rate { total += amount; }
        }
        crate::api::tutorial::currency(db, account, kind, total, rewards).await?;
    }
    Ok(())
}

/// GM: force a festival open or closed for `hours`, or return it to its dates.
pub(crate) async fn set_override(db: &mut PgConnection, s: &AppState, key: &str, mode: &str, hours: i64) -> Result<Vec<Value>> {
    let cal = Calendar::load(db, s).await?;
    if !cal.defs.iter().any(|d| d["Key"] == key) {
        return Err(crate::error::ServerError::InvalidRequest(format!("Unknown festival {key}")));
    }
    let mut overrides = cal.overrides.clone();
    if !overrides.is_object() { overrides = json!({}); }
    let now = cal.now;
    match mode {
        "Open" | "Closed" => {
            overrides[key] = json!({"Mode":mode,"Since":now,"Until":now + hours.clamp(1, 24 * 366) * 3600});
        }
        "Auto" => { overrides.as_object_mut().map(|o| o.remove(key)); }
        _ => return Err(crate::error::ServerError::InvalidRequest("Mode must be Open, Closed or Auto".into())),
    }
    overrides.as_object_mut().map(|o| o.retain(|_, v| n(v, "Until") > now));
    put(db, 0, OVERRIDE_KIND, 0, &overrides).await?;
    Ok(Calendar::at(s, now, overrides).status())
}
