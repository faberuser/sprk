use super::*;
use chrono::{Datelike, Duration, Utc};
use std::collections::{BTreeMap, BTreeSet};
use std::sync::OnceLock;

/// Immutable table lookups shared by evaluations within one account snapshot.
/// Player progress is always loaded afresh; nothing survives into the next request.
#[derive(Default)]
struct BattleLookup {
    types: BTreeMap<(i64, i64), i64>,
    tower_floors: BTreeMap<(i64, i64, i64), i64>,
}
impl BattleLookup {
    fn new(table: &crate::tables::BattleTable) -> Self {
        let mut out = Self::default();
        for row in table.rows("CampaignDungeon") {
            // Preserve BattleTable::find's first matching row semantics.
            if let (Some(chapter), Some(dungeon)) = (row["ChapterIndex"].as_i64(), row["DungeonIndex"].as_i64()) {
                out.types.entry((chapter, dungeon)).or_insert_with(|| n(row, "BattleType"));
            }
        }
        for row in table.rows("TowerFloor") {
            let (tower, chapter, dungeon, floor) = (n(row,"TowerIndex"), n(row,"ChapterIndex"), n(row,"DungeonIndex"), n(row,"Floor"));
            // Trial floors select one of DungeonIndices; their primary
            // DungeonIndex is zero and is not a playable stage.
            for dungeon in std::iter::once(dungeon).chain(
                row["DungeonIndices"].as_array().into_iter().flatten().filter_map(Value::as_i64)
            ).filter(|d| *d > 0) {
                out.tower_floors.entry((tower,chapter,dungeon))
                    .and_modify(|f| *f=(*f).max(floor)).or_insert(floor);
            }
        }
        out
    }
}

/// Mission-category membership and sub-quest final steps, indexed once per
/// snapshot. MissionClear rows previously rescanned every table per member.
#[derive(Default)]
struct MissionLookup {
    /// RewardQuestIndex -> (MainCategoryIndex, SubCategoryIndex); first row wins.
    categories: BTreeMap<i64, (i64, i64)>,
    /// Category -> SubQuest members as (QuestIndex, MainStep), in table order.
    members: BTreeMap<(i64, i64), Vec<(i64, i64)>>,
    /// QuestIndex -> highest Step defined for that sub-quest.
    final_steps: BTreeMap<i64, i64>,
}
impl MissionLookup {
    fn new(progression: &crate::tables::ProgressionTable) -> Self {
        let mut out = Self::default();
        for c in &progression.mission_categories {
            out.categories.entry(n(c, "RewardQuestIndex")).or_insert((n(c, "MainCategoryIndex"), n(c, "SubCategoryIndex")));
        }
        for v in progression.mission_visuals.iter().filter(|v| v["QuestType"] == "SubQuest") {
            out.members.entry((n(v, "MainCategoryIndex"), n(v, "SubCategoryIndex"))).or_default()
                .push((n(v, "QuestIndex"), n(v, "MainStep")));
        }
        for q in &progression.sub_quests {
            let step = out.final_steps.entry(n(q, "QuestIndex")).or_insert(i64::MIN);
            *step = (*step).max(n(q, "Step"));
        }
        out
    }
}

pub(super) fn daily_hot_time(row: &Value) -> bool {
    // These native daily missions share Limited with retired event milestones,
    // but have no event window. Only their three defined daily steps are active.
    n(row, "Type") == 8
        && row["DailyReset"] == true
        && (1..=3).contains(&n(row, "Step"))
        && row["Kind"] == "LoginOrConnected"
        && row["RewardKind"] == "Booster"
        && matches!(
            (n(row, "Index"), row["RewardCode"].as_str()),
            (54, Some("BOOSTER_EXP_TEST_MISSION"))
                | (55, Some("BOOSTER_GOLD_TEST_MISSION"))
        )
}

pub(super) fn period(row: &Value) -> String {
    let d = Utc::now().date_naive();
    if row["DailyReset"] == true || matches!(n(row, "Type"), 2 | 3) {
        return d.to_string();
    }
    if row["WeeklyReset"] == true || n(row, "Type") == 5 {
        return (d - Duration::days(d.weekday().num_days_from_monday() as i64)).to_string();
    }
    if let Some(days) = row["MonthlyResetDate"].as_array().filter(|v| !v.is_empty()) {
        for offset in 0..=62 {
            let day = d - Duration::days(offset);
            if days.iter().any(|v| v.as_u64() == Some(day.day() as u64)) {
                return day.to_string();
            }
        }
    }
    "all".into()
}
/// Row-independent Total* achievement values (see `View::inventory_totals`).
pub(super) struct InventoryTotals {
    pub equipment: i64,
    pub unique_weapon: i64,
    pub unique_treasure: i64,
    pub artifact: i64,
    pub item: i64,
    pub normal_item: i64,
}

/// family -> id -> [(period, step)]: `last()` runs per mission row.
type ClaimIndex = std::collections::HashMap<String, std::collections::HashMap<i64, Vec<(String, i64)>>>;
type Metric = (String, String, i64, i64, i64);
type Claim = (String, i64, i64, String, String);

/// The parts of a View. Each is loaded by its own query and reloaded only when
/// a revision part that `part::of` maps to it changed. Metrics and claims are
/// tracked finer: per metric kind and per (family, index).
pub(super) mod part {
    pub const HEROES: u64 = 1 << 0;
    pub const EQUIPMENT: u64 = 1 << 1;
    pub const NPC: u64 = 1 << 2;
    pub const GUILD_DAMAGE: u64 = 1 << 3;
    pub const CONQUEST_KILLS: u64 = 1 << 4;
    pub const SOULS: u64 = 1 << 5;
    pub const DUNGEONS: u64 = 1 << 6;
    pub const LOGIN: u64 = 1 << 7;
    pub const LEVEL: u64 = 1 << 8;
    pub const COSTUMES: u64 = 1 << 9;
    pub const FRIENDS: u64 = 1 << 10;
    pub const GUILD: u64 = 1 << 11;
    pub const TUTORIALS: u64 = 1 << 12;
    pub const ITEMS: u64 = 1 << 13;
    pub const ENTITLEMENTS: u64 = 1 << 14;
    pub const ARENA: u64 = 1 << 15;
    pub const BATTLE: u64 = 1 << 16;
    pub const HIST_EQUIP: u64 = 1 << 17;
    pub const FACTS: u64 = 1 << 18;
    pub const GUILD_ATT: u64 = 1 << 19;
    pub const GUILD_WINS: u64 = 1 << 20;
    pub const CONQUEST: u64 = 1 << 21;
    pub const GUILD_DECKS: u64 = 1 << 22;
    pub const PET: u64 = 1 << 23;
    pub const MONTHLY: u64 = 1 << 24;
    pub const LINKED: u64 = 1 << 25;
    pub const WORLD_RANK: u64 = 1 << 26;
    pub const CREATED: u64 = 1 << 27;
    pub const ATTENDANCE: u64 = 1 << 28;
    pub const MAIN_QUEST: u64 = 1 << 29;
    pub const WORLD_EVENTS: u64 = 1 << 30;
    /// Read every metric (or claim) row, not one kind (or one mission).
    pub const METRICS_ALL: u64 = 1 << 31;
    pub const CLAIMS_ALL: u64 = 1 << 32;
    /// Every loaded part (metrics and claims are always loaded in full then).
    pub const ALL: u64 = (1 << 31) - 1;
    /// Bit names, for the debug trace of why sections were re-evaluated.
    pub const NAMES: [&str; 33] = ["heroes", "equipment", "npc", "guild_damage", "conquest_kills", "souls", "dungeons",
        "login", "level", "costumes", "friends", "guild", "tutorials", "items", "entitlements", "arena", "battle",
        "hist_equip", "facts", "guild_att", "guild_wins", "conquest", "guild_decks", "pet", "monthly", "linked",
        "world_rank", "created", "attendance", "main_quest", "world_events", "metrics_all", "claims_all"];

    /// What a revision part (`table` or `table:kind`, `global:` for account 0;
    /// `progression_metrics:kind@day` for one day of a metric) invalidates.
    /// Kinds a View never reads map to nothing; an unknown part returns None,
    /// so the caller rebuilds everything.
    pub(in super::super) fn of(name: &str) -> Option<Invalidates> {
        let mut out = Invalidates::default();
        if let Some(kind) = name.strip_prefix("progression_metrics:") {
            match kind.rsplit_once('@') {
                Some((kind, day)) => out.metric(kind, Some(day)),
                None => out.metric(kind, None),
            }
            return Some(out);
        }
        // Only the play records of a changed kind are reloaded.
        if let Some(kind) = name.strip_prefix("play_record_values:") {
            out.mask = FACTS;
            out.fact_kinds = Some([kind.to_owned()].into());
            return Some(out);
        }
        // Equipment inserted, none updated or deleted (0016): only the new rows.
        if name == "equip_items:+" {
            out.equipment_added = true;
            return Some(out);
        }
        // These parts read every kind of their table.
        let name = match name.split_once(':') {
            Some((table @ ("arena_runs" | "guild_battle_scores"), _)) => table,
            _ => name,
        };
        out.mask = match name {
            "progression_claims" => { out.claims = true; 0 }
            "heroes" | "hero_details" => HEROES,
            "equip_items" => EQUIPMENT | SOULS | HIST_EQUIP,
            "extension_state:npc" => NPC,
            "extension_state:soul" => SOULS,
            "extension_state:pet_misc" => PET,
            "extension_state:monthly_hero" => MONTHLY,
            "guild_battle_scores" => GUILD_DAMAGE,
            "community_claims:guild_conquest_kill" => CONQUEST_KILLS,
            "community_claims:guild_attend_total" => GUILD_ATT,
            "campaign_progress" | "battle_state:dungeon" => DUNGEONS,
            "progression_login" => LOGIN,
            "user_info" => LEVEL,
            "costumes" => COSTUMES,
            "friends" => FRIENDS,
            "guild_members" => GUILD,
            "tutorial_progress" => TUTORIALS,
            "items" => ITEMS,
            "progression_entitlements" => ENTITLEMENTS,
            "arena_runs" => ARENA,
            "battle_state:eclipse_record" | "battle_state:ordeal" => BATTLE,
            "play_record_values" => FACTS,
            "guild_arena_records" => GUILD_WINS,
            "community_state:guild_conquest_history" => CONQUEST,
            "guild_arena_decks" => GUILD_DECKS,
            "accounts" => LINKED | CREATED,
            "global:credentials" => LINKED,
            "battle_scores" | "global:battle_scores" => WORLD_RANK,
            "attendance_calendar_state" => ATTENDANCE,
            "progression_main_quest" => MAIN_QUEST,
            "progression_world_events" => WORLD_EVENTS,
            // Kinds of these tables that no part reads.
            n if n.starts_with("extension_state:") || n.starts_with("battle_state:")
                || n.starts_with("community_claims:") => 0,
            _ => return None,
        };
        Some(out)
    }

    /// Parts to reload, metric kinds to reload (only some days of a kind, or
    /// every day with None), and whether claims changed.
    #[derive(Default, Debug)]
    pub(in super::super) struct Invalidates {
        pub mask: u64,
        pub metrics: std::collections::BTreeMap<String, Option<std::collections::BTreeSet<String>>>,
        pub claims: bool,
        /// With FACTS: reload only these play record kinds (None: every kind).
        pub fact_kinds: Option<std::collections::BTreeSet<String>>,
        /// Equipment rows were inserted; without EQUIPMENT, none other changed.
        pub equipment_added: bool,
    }
    impl Invalidates {
        /// Reload `day` of metric `kind` (every day with None).
        pub(in super::super) fn metric(&mut self, kind: &str, day: Option<&str>) {
            let days = self.metrics.entry(kind.to_owned()).or_insert_with(|| Some(Default::default()));
            match (days, day) {
                (Some(days), Some(day)) => { days.insert(day.to_owned()); }
                (days, _) => *days = None,
            }
        }
        /// Everything `other` invalidates as well.
        pub(in super::super) fn merge(&mut self, other: Invalidates) {
            if other.mask & FACTS != 0 {
                self.fact_kinds = match (self.mask & FACTS != 0, self.fact_kinds.take(), other.fact_kinds) {
                    (false, _, kinds) => kinds,
                    (true, Some(mut mine), Some(theirs)) => { mine.extend(theirs); Some(mine) }
                    _ => None,
                };
            }
            self.equipment_added |= other.equipment_added;
            self.mask |= other.mask;
            self.claims |= other.claims;
            for (kind, days) in other.metrics {
                match days {
                    None => self.metric(&kind, None),
                    Some(days) => for day in days { self.metric(&kind, Some(&day)) },
                }
            }
        }
    }
}

/// What one evaluation read (or what a refresh changed): parts, metric kinds,
/// claimed missions, single heroes by index and campaign stages (a whole
/// chapter is `(chapter, WHOLE_CHAPTER)`).
#[derive(Default, Clone, Debug)]
pub(super) struct Deps {
    pub mask: u64,
    pub metrics: BTreeSet<String>,
    pub claims: BTreeSet<(String, i64)>,
    pub heroes: BTreeSet<i64>,
    pub stages: BTreeSet<(i64, i64)>,
}
const WHOLE_CHAPTER: i64 = i64::MIN;
impl Deps {
    /// Whether anything this evaluation read is in `changed`.
    pub(super) fn affected_by(&self, changed: &Deps) -> bool {
        self.mask & changed.mask != 0
            || self.metrics.iter().any(|k| changed.metrics.contains(k))
            || self.claims.iter().any(|k| changed.claims.contains(k))
            || self.heroes.iter().any(|k| changed.heroes.contains(k))
            || self.stages.iter().any(|k| changed.stages.contains(k))
    }
    /// What in `changed` this evaluation read (debug trace only).
    pub(super) fn causes(&self, changed: &Deps) -> Vec<String> {
        let mut out: Vec<String> = (0..part::NAMES.len()).filter(|bit| self.mask & changed.mask & (1 << bit) != 0)
            .map(|bit| part::NAMES[bit].to_owned()).collect();
        out.extend(self.metrics.iter().filter(|k| changed.metrics.contains(*k)).map(|k| format!("metric:{k}")));
        out.extend(self.claims.iter().filter(|k| changed.claims.contains(*k)).map(|k| format!("claim:{}", k.0)));
        if self.heroes.iter().any(|k| changed.heroes.contains(k)) { out.push("hero".into()); }
        if self.stages.iter().any(|k| changed.stages.contains(k)) { out.push("stage".into()); }
        out
    }
}

/// Hero fields that no mission reads and battles change constantly; the view
/// leaves them out so they never count as a hero change.
const UNREAD_HERO_FIELDS: &[&str] = &["FlaskExp", "FlaskItemIndex"];

thread_local! {
    static TRACKING: std::cell::RefCell<Option<Deps>> = const { std::cell::RefCell::new(None) };
}
/// Evaluate `f` and return what it read through View's accessors. Evaluation
/// is synchronous, so a thread-local tracker sees exactly this call.
pub(super) fn tracked<T>(f: impl FnOnce() -> T) -> (T, Deps) {
    let outer = TRACKING.with(|t| t.replace(Some(Deps::default())));
    let value = f();
    let deps = TRACKING.with(|t| t.replace(outer)).unwrap_or_default();
    TRACKING.with(|t| if let Some(o) = t.borrow_mut().as_mut() {
        o.mask |= deps.mask;
        o.metrics.extend(deps.metrics.iter().cloned());
        o.claims.extend(deps.claims.iter().cloned());
        o.heroes.extend(deps.heroes.iter().copied());
        o.stages.extend(deps.stages.iter().copied());
    });
    (value, deps)
}
fn mark(bits: u64) {
    TRACKING.with(|t| if let Some(d) = t.borrow_mut().as_mut() { d.mask |= bits; });
}
fn mark_metric(kind: &str) {
    TRACKING.with(|t| if let Some(d) = t.borrow_mut().as_mut() {
        if !d.metrics.contains(kind) { d.metrics.insert(kind.to_owned()); }
    });
}
fn mark_stage(chapter: i64, dungeon: i64) {
    TRACKING.with(|t| if let Some(d) = t.borrow_mut().as_mut() { d.stages.insert((chapter, dungeon)); });
}
fn mark_hero(index: i64) {
    TRACKING.with(|t| if let Some(d) = t.borrow_mut().as_mut() { d.heroes.insert(index); });
}
fn mark_claim(family: &str, id: i64) {
    TRACKING.with(|t| if let Some(d) = t.borrow_mut().as_mut() {
        let key = (family.to_owned(), id);
        if !d.claims.contains(&key) { d.claims.insert(key); }
    });
}

/// Loaded inputs, one field per part. Private to this module: evaluators read
/// them through View's accessors, which record what each evaluation used.
/// Folds the dungeon progress the client is shown at login (battle_state kind
/// `dungeon`, keyed `chapter<<32 | dungeon`) into the `campaign_progress` rows.
/// The client counts chapter stars as `MaxStar % 10` of that list, so the
/// server must count the same stars: a clear saved only there (older writes)
/// would otherwise show a reward as claimable that the server refuses.
/// Clear counts stay from `campaign_progress`, since `DailyCompletedCount`
/// resets every day; a clear known only to the shown list counts once.
pub(super) fn merge_shown_dungeons(dungeons: &mut Vec<(i64, i64, i64, i64)>, shown: &[(i64, String)]) {
    for (idx, data) in shown {
        let Ok(p) = serde_json::from_str::<Value>(data) else { continue };
        let max_star = p["MaxStar"].as_i64().unwrap_or(0);
        if max_star <= 0 {
            continue;
        }
        let (chapter, dungeon) = (idx >> 32, idx & 0xffff_ffff);
        match dungeons.iter_mut().find(|d| d.0 == chapter && d.1 == dungeon) {
            Some(d) => {
                d.2 = d.2.max(1);
                if max_star % 10 > d.3 % 10 {
                    d.3 = max_star;
                }
            }
            None => dungeons.push((chapter, dungeon, 1, max_star)),
        }
    }
    dungeons.sort_unstable();
}
#[derive(Default)]
struct Inputs {
    history: super::mission_history::History,
    heroes: Vec<Value>,
    equipped_slots: BTreeSet<i64>,
    equipment: Vec<Value>,
    soul_weapons: Vec<Value>,
    npc_friendly: Vec<Value>,
    dungeons: Vec<(i64, i64, i64, i64)>,
    /// By kind; each kind's rows sorted, so a reload of one kind matches a full load.
    metrics: std::collections::HashMap<String, Vec<Metric>>,
    claims: Vec<Claim>,
    login_days: i64,
    last_login_day: Option<String>,
    level: i64,
    costumes: i64,
    friends: i64,
    guild: bool,
    tutorials: BTreeSet<i64>,
    items: BTreeMap<i64, i64>,
    /// (product index, created time).
    entitlements: Vec<(i64, String)>,
    guild_damage: BTreeMap<String, i64>,
    conquest_kills: i64,
    created: String,
    /// (index, claims, last day, completed time).
    attendance: Vec<(i64, i64, String, Option<String>)>,
    main_quest: Option<(i64, i64)>,
    world_events: Vec<Value>,
}

pub(super) struct View {
    d: Inputs,
    // Memoised on first use. A refresh resets the data-derived ones and keeps
    // the table lookups (an incremental refresh uses the same tables). Tests
    // that edit a View in place reset these.
    pub(super) inventory_totals: OnceLock<InventoryTotals>,
    /// Per metric kind: its values summed by (battle type of the stage, day).
    typed_metrics: std::sync::Mutex<BTreeMap<String, std::sync::Arc<BTreeMap<(i64, String), i64>>>>,
    claim_index: OnceLock<ClaimIndex>,
    battle_lookup: OnceLock<BattleLookup>,
    equipment_lookup: OnceLock<BTreeMap<i64,(i64,i64,i64)>>,
    mission_lookup: OnceLock<MissionLookup>,
}
fn json_rows(rows: Vec<String>, what: &str) -> Result<Vec<Value>> {
    rows.iter().map(|data| serde_json::from_str(data)).collect::<std::result::Result<_, _>>()
        .map_err(|e| ServerError::Internal(format!("Invalid {what}: {e}")))
}
fn metric_map(rows: Vec<Metric>) -> std::collections::HashMap<String, Vec<Metric>> {
    let mut out = std::collections::HashMap::<String, Vec<Metric>>::new();
    for row in rows { out.entry(row.1.clone()).or_default().push(row); }
    for rows in out.values_mut() { rows.sort(); }
    out
}
/// (family, index) of every mission whose claims differ between `old` and `new`.
fn changed_claims(old: &[Claim], new: &[Claim]) -> BTreeSet<(String, i64)> {
    let group = |claims: &[Claim]| {
        let mut map = BTreeMap::<(String, i64), Vec<(i64, String, String)>>::new();
        for c in claims { map.entry((c.0.clone(), c.1)).or_default().push((c.2, c.3.clone(), c.4.clone())); }
        for v in map.values_mut() { v.sort(); }
        map
    };
    let (old, new) = (group(old), group(new));
    old.keys().chain(new.keys()).filter(|k| old.get(*k) != new.get(*k)).cloned().collect()
}

impl View {
    pub async fn load(db: &mut PgConnection, account: i64) -> Result<Self> {
        Ok(Self::refresh(db, account, None).await?.0)
    }

    /// Load the parts `changes` names into `previous` (everything without one),
    /// keeping its other parts. Also returns what changed, as evaluation deps:
    /// a reloaded part whose data is unchanged is not a change.
    pub(super) async fn refresh(db: &mut PgConnection, account: i64, previous: Option<(View, &part::Invalidates)>)
        -> Result<(Self, Deps)> {
        use part::*;
        let (mut d, mask, kinds, claims, fact_kinds, equipment_added, lookups) = match previous {
            Some((view, changes)) => (view.d, changes.mask, Some(&changes.metrics), changes.claims,
                changes.fact_kinds.as_ref(), changes.equipment_added,
                (view.battle_lookup, view.equipment_lookup, view.mission_lookup)),
            None => (Inputs::default(), ALL, None, true, None, false, Default::default()),
        };
        let mut changed = Deps::default();
        // A reloaded part is a change only when its data differs.
        macro_rules! update {
            ($field:ident, $bit:expr, $value:expr) => {{
                let value = $value;
                if d.$field != value { d.$field = value; changed.mask |= $bit; }
            }};
        }
        let has = |bit: u64| mask & bit != 0;
        if has(HEROES) {
            // Flask progress changes on every battle a capped hero fights and
            // no mission reads it: keeping it would re-evaluate every section
            // that reads heroes after each battle.
            let heroes: Vec<Value> = hero::snapshot(db, account).await?.into_iter().map(|h| {
                let mut h = serde_json::to_value(h).unwrap();
                if let Some(h) = h.as_object_mut() { for key in UNREAD_HERO_FIELDS { h.remove(*key); } }
                h
            }).collect();
            if heroes != d.heroes {
                let by_index = |heroes: &[Value]| {
                    let mut map = BTreeMap::<i64, Vec<Value>>::new();
                    for h in heroes { map.entry(n(h, "HeroIndex")).or_default().push(h.clone()); }
                    map
                };
                let (old, new) = (by_index(&d.heroes), by_index(&heroes));
                changed.heroes = old.keys().chain(new.keys()).filter(|k| old.get(*k) != new.get(*k)).copied().collect();
                changed.mask |= HEROES;
                d.equipped_slots = heroes.iter().flat_map(|h|(1..=10).map(move |part|n(h,&format!("EquipItemSlotIndex{part}"))))
                    .filter(|slot|*slot>0).collect();
                d.heroes = heroes;
            }
        }
        let equipment_row = |(slot, item, star, inventory): (i64, i64, i64, i64)|
            json!({"SlotIndex":slot,"ItemIndex":item,"Star":star,"InventoryType":inventory});
        // Pieces inserted since the view, when no other piece changed (see
        // migrations/0016): their slots are above every slot it holds.
        let mut added = Vec::new();
        if has(EQUIPMENT) {
            // Plain columns: building JSON per row in the database and parsing
            // it here cost more than the rows (thousands per account). In slot
            // order, as inserted pieces are appended.
            let mut rows = crate::db::query_as::<(i64, i64, i64, i64)>("SELECT slot_index::bigint,item_index::bigint,star::bigint,inventory_type::bigint FROM equip_items WHERE account_id=$1")
                .bind(account).fetch_all(&mut *db).await?;
            rows.sort_unstable();
            update!(equipment, EQUIPMENT, rows.into_iter().map(equipment_row).collect::<Vec<_>>());
        } else if equipment_added {
            let last = d.equipment.last().map(|e| n(e, "SlotIndex")).unwrap_or(0);
            added = crate::db::query_as::<(i64, i64, i64, i64)>("SELECT slot_index::bigint,item_index::bigint,star::bigint,inventory_type::bigint FROM equip_items WHERE account_id=$1 AND slot_index>$2 ORDER BY slot_index")
                .bind(account).bind(last).fetch_all(&mut *db).await?;
            if !added.is_empty() {
                d.equipment.extend(added.iter().copied().map(equipment_row));
                changed.mask |= EQUIPMENT;
            }
        }
        if has(NPC) { update!(npc_friendly, NPC, crate::api::extensions::list(db, account, "npc").await?); }
        if has(GUILD_DAMAGE) {
            update!(guild_damage, GUILD_DAMAGE, crate::db::query_as::<(String, i64)>("SELECT kind,SUM(score)::bigint FROM guild_battle_scores WHERE account=$1 GROUP BY kind")
                .bind(account).fetch_all(&mut *db).await?.into_iter().collect());
        }
        if has(CONQUEST_KILLS) {
            update!(conquest_kills, CONQUEST_KILLS, crate::db::query_scalar("SELECT COUNT(*) FROM community_claims WHERE account=$1 AND kind='guild_conquest_kill'")
                .bind(account).fetch_one(&mut *db).await?);
        }
        if has(SOULS) {
            // Count owned weapons, including stored/unequipped ones. The join
            // excludes orphaned soul records without treating them as ownership.
            update!(soul_weapons, SOULS, json_rows(crate::db::query_scalar::<String>("SELECT s.data FROM extension_state s JOIN equip_items e ON e.account_id=s.account_id AND e.slot_index=s.idx WHERE s.account_id=$1 AND s.kind='soul'")
                .bind(account).fetch_all(&mut *db).await?, "saved Soul Weapon")?);
        }
        if has(DUNGEONS) {
            let mut dungeons: Vec<(i64, i64, i64, i64)> = crate::db::query_as("SELECT chapter_id,dungeon_id,clear_count,best_star FROM campaign_progress WHERE account_id=$1 ORDER BY chapter_id,dungeon_id").bind(account).fetch_all(&mut *db).await?;
            let shown: Vec<(i64, String)> = crate::db::query_as("SELECT idx,data FROM battle_state WHERE account=$1 AND kind='dungeon'").bind(account).fetch_all(&mut *db).await?;
            merge_shown_dungeons(&mut dungeons, &shown);
            if dungeons != d.dungeons {
                let by_stage = |rows: &[(i64, i64, i64, i64)]| {
                    let mut map = BTreeMap::<(i64, i64), Vec<(i64, i64, i64, i64)>>::new();
                    for r in rows { map.entry((r.0, r.1)).or_default().push(*r); }
                    map
                };
                let (old, new) = (by_stage(&d.dungeons), by_stage(&dungeons));
                for stage in old.keys().chain(new.keys()).filter(|k| old.get(*k) != new.get(*k)) {
                    changed.stages.insert(*stage);
                    changed.stages.insert((stage.0, WHOLE_CHAPTER));
                }
                changed.mask |= DUNGEONS;
                d.dungeons = dungeons;
            }
        }
        match kinds {
            None => {
                d.metrics = metric_map(crate::db::query_as("SELECT day,kind,arg,arg2,value FROM progression_metrics WHERE account_id=$1").bind(account).fetch_all(&mut *db).await?);
                changed.mask |= METRICS_ALL;
            }
            Some(kinds) if !kinds.is_empty() => {
                let whole: Vec<&String> = kinds.iter().filter(|(_, days)| days.is_none()).map(|(kind, _)| kind).collect();
                let days: Vec<(&String, &String)> = kinds.iter()
                    .flat_map(|(kind, days)| days.iter().flatten().map(move |day| (kind, day))).collect();
                let mut rows = Vec::<Metric>::new();
                if !whole.is_empty() {
                    let list = serde_json::to_string(&whole).map_err(|e| ServerError::Internal(e.to_string()))?;
                    rows.extend(crate::db::query_as::<Metric>("SELECT day,kind,arg,arg2,value FROM progression_metrics WHERE account_id=$1 AND kind = ANY(ARRAY(SELECT jsonb_array_elements_text($2::jsonb)))")
                        .bind(account).bind(list).fetch_all(&mut *db).await?);
                }
                if !days.is_empty() {
                    // Only the changed days of these kinds: one primary-key
                    // range per (day, kind) of the lists, keeping the pairs asked.
                    let day_list: BTreeSet<&String> = days.iter().map(|(_, day)| *day).collect();
                    let kind_list: BTreeSet<&String> = days.iter().map(|(kind, _)| *kind).collect();
                    let json = |v: &BTreeSet<&String>| serde_json::to_string(v).map_err(|e| ServerError::Internal(e.to_string()));
                    rows.extend(crate::db::query_as::<Metric>("SELECT day,kind,arg,arg2,value FROM progression_metrics WHERE account_id=$1 \
                        AND day = ANY(ARRAY(SELECT jsonb_array_elements_text($2::jsonb))) AND kind = ANY(ARRAY(SELECT jsonb_array_elements_text($3::jsonb)))")
                        .bind(account).bind(json(&day_list)?).bind(json(&kind_list)?).fetch_all(&mut *db).await?
                        .into_iter().filter(|m| days.contains(&(&m.1, &m.0))));
                }
                let mut rows = metric_map(rows);
                for (kind, days) in kinds {
                    let new = match days {
                        None => rows.remove(kind).unwrap_or_default(),
                        // Keep the kind's other days; replace the reloaded ones.
                        Some(days) => {
                            let mut merged: Vec<Metric> = d.metrics.get(kind).into_iter().flatten()
                                .filter(|m| !days.contains(&m.0)).cloned().collect();
                            merged.extend(rows.remove(kind).unwrap_or_default());
                            merged.sort();
                            merged
                        }
                    };
                    if d.metrics.get(kind).map(Vec::as_slice).unwrap_or(&[]) != new.as_slice() {
                        changed.metrics.insert(kind.clone());
                        changed.mask |= METRICS_ALL;
                    }
                    if new.is_empty() { d.metrics.remove(kind); } else { d.metrics.insert(kind.clone(), new); }
                }
            }
            Some(_) => {}
        }
        if claims {
            let new: Vec<Claim> = crate::db::query_as("SELECT family,idx,step,period,claimed_time FROM progression_claims WHERE account_id=$1").bind(account).fetch_all(&mut *db).await?;
            changed.claims = changed_claims(&d.claims, &new);
            if !changed.claims.is_empty() { changed.mask |= CLAIMS_ALL; }
            d.claims = new;
        }
        if has(LOGIN) {
            let login: Option<(i64, String)> = crate::db::query_as("SELECT days,last_day FROM progression_login WHERE account_id=$1").bind(account).fetch_optional(&mut *db).await?;
            update!(login_days, LOGIN, login.as_ref().map(|l| l.0).unwrap_or(0));
            update!(last_login_day, LOGIN, login.map(|l| l.1));
        }
        if has(LEVEL) { update!(level, LEVEL, crate::db::query_scalar("SELECT team_level FROM user_info WHERE account_id=$1").bind(account).fetch_one(&mut *db).await?); }
        if has(COSTUMES) { update!(costumes, COSTUMES, crate::db::query_scalar("SELECT COUNT(*) FROM costumes WHERE account_id=$1").bind(account).fetch_one(&mut *db).await?); }
        if has(FRIENDS) { update!(friends, FRIENDS, crate::db::query_scalar("SELECT COUNT(*) FROM friends WHERE (account_id=$1 OR friend_account_id=$2) AND status='accepted'").bind(account).bind(account).fetch_one(&mut *db).await?); }
        if has(GUILD) { update!(guild, GUILD, crate::db::query_scalar("SELECT EXISTS(SELECT 1 FROM guild_members WHERE account_id=$1)").bind(account).fetch_one(&mut *db).await?); }
        if has(TUTORIALS) { update!(tutorials, TUTORIALS, crate::db::query_scalar::<i64>("SELECT tutorial_index FROM tutorial_progress WHERE account_id=$1 AND completed_time IS NOT NULL").bind(account).fetch_all(&mut *db).await?.into_iter().collect()); }
        if has(ITEMS) { update!(items, ITEMS, crate::db::query_as::<(i64,i64)>("SELECT item_index,count FROM items WHERE account_id=$1").bind(account).fetch_all(&mut *db).await?.into_iter().collect()); }
        if has(ENTITLEMENTS) { update!(entitlements, ENTITLEMENTS, crate::db::query_as("SELECT product_index,created_time FROM progression_entitlements WHERE account_id=$1").bind(account).fetch_all(&mut *db).await?); }
        if has(CREATED) { update!(created, CREATED, crate::db::query_scalar("SELECT created_at FROM accounts WHERE account_id=$1").bind(account).fetch_one(&mut *db).await?); }
        if has(ATTENDANCE) { update!(attendance, ATTENDANCE, crate::db::query_as("SELECT idx,claims,last_day,completed_time FROM attendance_calendar_state WHERE account_id=$1").bind(account).fetch_all(&mut *db).await?); }
        if has(MAIN_QUEST) { update!(main_quest, MAIN_QUEST, crate::db::query_as("SELECT step,progress FROM progression_main_quest WHERE account_id=$1").bind(account).fetch_optional(&mut *db).await?); }
        if has(WORLD_EVENTS) {
            // Unreadable rows are skipped, as the snapshot always did.
            update!(world_events, WORLD_EVENTS, crate::db::query_scalar::<String>("SELECT data FROM progression_world_events WHERE account_id=$1").bind(account).fetch_all(&mut *db).await?
                .iter().filter_map(|s| serde_json::from_str::<Value>(s).ok()).collect());
        }
        // Equipment history is the owned equipment's item indices: reuse the
        // equipment rows when they were reloaded too.
        let owned = (has(EQUIPMENT) && has(HIST_EQUIP)).then(|| d.equipment.iter().map(|e| n(e, "ItemIndex")).collect());
        changed.mask |= d.history.reload(db, account, mask, owned, fact_kinds).await?;
        // New pieces only add item indices to the equipment history.
        let before = d.history.equipment.len();
        d.history.equipment.extend(added.iter().map(|row| row.1));
        if d.history.equipment.len() != before { changed.mask |= HIST_EQUIP; }
        let (battle_lookup, equipment_lookup, mission_lookup) = lookups;
        Ok((Self {
            d,
            inventory_totals: OnceLock::new(),
            typed_metrics: Default::default(),
            claim_index: OnceLock::new(),
            battle_lookup,
            equipment_lookup,
            mission_lookup,
        }, changed))
    }

    // Tracking accessors: every read of loaded data goes through one of these.
    pub(super) fn heroes(&self) -> &Vec<Value> { mark(part::HEROES); &self.d.heroes }
    /// The heroes with one HeroIndex: depends on those heroes only.
    pub(super) fn heroes_with_index(&self, index: i64) -> impl Iterator<Item = &Value> {
        mark_hero(index);
        self.d.heroes.iter().filter(move |h| n(h, "HeroIndex") == index)
    }
    pub(super) fn equipped_slots(&self) -> &BTreeSet<i64> { mark(part::HEROES); &self.d.equipped_slots }
    pub(super) fn equipment(&self) -> &Vec<Value> { mark(part::EQUIPMENT); &self.d.equipment }
    pub(super) fn soul_weapons(&self) -> &Vec<Value> { mark(part::SOULS); &self.d.soul_weapons }
    pub(super) fn npc_friendly(&self) -> &Vec<Value> { mark(part::NPC); &self.d.npc_friendly }
    pub(super) fn dungeons(&self) -> &Vec<(i64, i64, i64, i64)> { mark(part::DUNGEONS); &self.d.dungeons }
    /// Progress rows of one campaign stage: depends on that stage only.
    pub(super) fn stage(&self, chapter: i64, dungeon: i64) -> impl Iterator<Item = &(i64, i64, i64, i64)> {
        mark_stage(chapter, dungeon);
        self.d.dungeons.iter().filter(move |d| d.0 == chapter && d.1 == dungeon)
    }
    /// Progress rows of one chapter: depends on that chapter only.
    pub(super) fn chapter_stages(&self, chapter: i64) -> impl Iterator<Item = &(i64, i64, i64, i64)> {
        mark_stage(chapter, WHOLE_CHAPTER);
        self.d.dungeons.iter().filter(move |d| d.0 == chapter)
    }
    pub(super) fn login_days(&self) -> i64 { mark(part::LOGIN); self.d.login_days }
    pub(super) fn last_login_day(&self) -> Option<&str> { mark(part::LOGIN); self.d.last_login_day.as_deref() }
    pub(super) fn level(&self) -> i64 { mark(part::LEVEL); self.d.level }
    pub(super) fn costumes(&self) -> i64 { mark(part::COSTUMES); self.d.costumes }
    pub(super) fn friends(&self) -> i64 { mark(part::FRIENDS); self.d.friends }
    pub(super) fn guild(&self) -> bool { mark(part::GUILD); self.d.guild }
    pub(super) fn tutorials(&self) -> &BTreeSet<i64> { mark(part::TUTORIALS); &self.d.tutorials }
    pub(super) fn items(&self) -> &BTreeMap<i64, i64> { mark(part::ITEMS); &self.d.items }
    pub(super) fn entitlements(&self) -> &Vec<(i64, String)> { mark(part::ENTITLEMENTS); &self.d.entitlements }
    pub(super) fn entitled(&self, product: i64) -> bool { self.entitlements().iter().any(|e| e.0 == product) }
    pub(super) fn guild_damage(&self) -> &BTreeMap<String, i64> { mark(part::GUILD_DAMAGE); &self.d.guild_damage }
    pub(super) fn conquest_kills(&self) -> i64 { mark(part::CONQUEST_KILLS); self.d.conquest_kills }
    pub(super) fn created(&self) -> &str { mark(part::CREATED); &self.d.created }
    pub(super) fn attendance(&self) -> &Vec<(i64, i64, String, Option<String>)> { mark(part::ATTENDANCE); &self.d.attendance }
    pub(super) fn main_quest(&self) -> Option<(i64, i64)> { mark(part::MAIN_QUEST); self.d.main_quest }
    pub(super) fn world_events(&self) -> &Vec<Value> { mark(part::WORLD_EVENTS); &self.d.world_events }
    /// History, marking the parts the caller reads from it.
    pub(super) fn hist(&self, parts: u64) -> &super::mission_history::History { mark(parts); &self.d.history }
    /// Every claim row (for scans across missions; `last` tracks one mission).
    pub(super) fn claims_all(&self) -> &Vec<Claim> { mark(part::CLAIMS_ALL); &self.d.claims }
    /// The rows of one metric kind.
    pub(super) fn metrics_of(&self, kind: &str) -> &[Metric] {
        mark_metric(kind);
        self.d.metrics.get(kind).map(Vec::as_slice).unwrap_or(&[])
    }
    #[cfg(test)]
    pub(super) fn heroes_mut(&mut self) -> &mut Vec<Value> { &mut self.d.heroes }
    #[cfg(test)]
    pub(super) fn history_mut(&mut self) -> &mut super::mission_history::History { &mut self.d.history }
    #[cfg(test)]
    pub(super) fn sizes(&self) -> (usize, usize, usize) {
        (self.d.heroes.len(), self.d.metrics.values().map(Vec::len).sum(), self.d.claims.len())
    }

    /// `kind`'s values by (battle type of their stage, day), for counts over
    /// battle types: about 200 missions count ClearDungeon by type, and each
    /// rescanned every row (850 for an active player) after every battle.
    /// Rows of stages without a battle type are left out. Reads the whole kind.
    pub(super) fn typed_metric(&self, state: &AppState, kind: &str) -> std::sync::Arc<BTreeMap<(i64, String), i64>> {
        let rows = self.metrics_of(kind);
        let mut memo = self.typed_metrics.lock().unwrap_or_else(|e| e.into_inner());
        memo.entry(kind.to_owned()).or_insert_with(|| {
            let mut sums = BTreeMap::<(i64, String), i64>::new();
            for (day, k, chapter, dungeon, value) in rows {
                if k != kind { continue; }
                let Some(battle) = self.battle_type(state, *chapter, *dungeon) else { continue; };
                let sum = sums.entry((battle, day.clone())).or_default();
                *sum = sum.wrapping_add(*value);
            }
            std::sync::Arc::new(sums)
        }).clone()
    }
    pub(super) fn battle_type(&self, state: &AppState, chapter: i64, dungeon: i64) -> Option<i64> {
        self.battle_lookup.get_or_init(|| BattleLookup::new(&state.tables.battle)).types.get(&(chapter,dungeon)).copied()
    }
    pub(super) fn equipment_metadata(&self, state: &AppState, item: i64) -> Option<(i64,i64,i64)> {
        // A snapshot evaluates many equipment missions. Index immutable data
        // once rather than scanning the full table for every inventory item.
        self.equipment_lookup.get_or_init(|| {
            let mut lookup=BTreeMap::new();
            for r in state.tables.extensions.rows("EquipItem") {
                lookup.entry(n(r,"ItemIndex")).or_insert((n(r,"PartType"),n(r,"DetailIndex"),n(r,"EquipType")));
            }
            lookup
        })
            .get(&item).copied()
    }
    pub(super) fn tower_floor(&self, state: &AppState, tower: i64, chapter: i64, dungeon: i64) -> Option<i64> {
        self.battle_lookup.get_or_init(|| BattleLookup::new(&state.tables.battle)).tower_floors.get(&(tower,chapter,dungeon)).copied()
    }
    fn is_tower_floor(&self, state: &AppState, tower: i64, chapter: i64, dungeon: i64) -> bool {
        self.tower_floor(state,tower,chapter,dungeon).is_some()
    }
    pub fn last(&self, family: &str, id: i64, p: &str) -> i64 {
        mark_claim(family, id);
        let index = self.claim_index.get_or_init(|| {
            let mut index = ClaimIndex::new();
            for c in &self.d.claims {
                index.entry(c.0.clone()).or_default().entry(c.1).or_default().push((c.3.clone(), c.2));
            }
            index
        });
        index.get(family).and_then(|ids| ids.get(&id)).into_iter().flatten()
            .filter(|(period, _)| period == p)
            .map(|(_, step)| *step)
            .max()
            .unwrap_or(0)
    }
    pub fn category_subquest_complete(&self, state: &AppState, id: i64) -> bool {
        self.last("subquest", id, "all") > 0
            || state.tables.progression.sub_quests.iter().any(|row| {
                n(row, "QuestIndex") == id
                    && n(row, "Step") == 1
                    && row["AutoComplete"] == true
                    && row["ClientOnly"] != true
                    && row["Kind"] == "DungeonCleared"
                    && self.quest(state, row) >= n(row, "ReqProgress").max(1)
            })
    }
    pub fn metric(&self, kind: &str, p: &str, a: Option<i64>, b: Option<i64>) -> i64 {
        self.metrics_of(kind).iter()
            .filter(|v| {
                (p == "all" || v.0.as_str() >= p)
                    && a.is_none_or(|a| a == v.2)
                    && b.is_none_or(|b| b == v.3)
            })
            .map(|v| v.4)
            .sum()
    }
    pub fn stars(&self, state: &AppState, chapter: i64) -> i64 {
        self.chapter_stages(chapter)
            .filter(|d| {
                d.0 == chapter
                    && state
                        .tables
                        .get_campaign_dungeon(d.0 as i32, d.1 as i32)
                        .is_some()
            })
            .map(|d| (d.3 % 10).clamp(0, 3))
            .sum()
    }
    pub fn condition(&self, v: &Value) -> bool {
        let values: Vec<&str> = v
            .as_array()
            .into_iter()
            .flatten()
            .filter_map(Value::as_str)
            .map(str::trim)
            .collect();
        if values.is_empty() {
            return true;
        }
        let num = |i: usize| {
            values
                .get(i)
                .and_then(|v| v.parse::<i64>().ok())
                .unwrap_or(i64::MAX)
        };
        match values[0] {
            "CheckTeamLevel" | "TeamLevel" => self.level() >= num(1),
            "LoginDailyCount" => self.login_days() >= num(1),
            "SubQuestComplete" => self.last("subquest", num(1), "all") > 0,
            // Missing event/paid-service conditions never grant eligibility implicitly.
            _ => false,
        }
    }
    pub fn quest(&self, state: &AppState, row: &Value) -> i64 {
        self.quest_value(state,row).unwrap_or(0)
    }
    // Keep unsupported rules distinct from supported rules with zero progress.
    // Claim/UI behavior stays fail-closed; the audit can now identify missing rules.
    pub(super) fn quest_value(&self, state: &AppState, row: &Value) -> Option<i64> {
        if let Some(value) = self.history_quest(state, row) { return Some(value); }
        // The archived public table omits server-only action filters. A null
        // StateValue must never turn a specialized mission into an any-action
        // counter. Resolve known goals explicitly; unknown scopes stay pending.
        if !row["StateValue"].as_array().is_some_and(|v| !v.is_empty())
            && matches!(row["Kind"].as_str(), Some("ClearDungeon" | "WearEquip" | "GiveFriendlyPoint" | "GiveMultipleGiftItem" | "BuyShopItem" | "AddItem" | "CraftItem" | "EnterDungeon" | "UseItem" | "ConsumeStamina" | "AwakenGear" | "UpgradeEquipOption" | "UpgradeEquipTier" | "RenewEquipOption" | "Enchant" | "EquipRune" | "ArtifactRestore" | "SoulRestore" | "SoulWeaponRenewOption")) {
            return self.scoped_quest(state, row);
        }
        let a: Vec<i64> = row["StateValue"]
            .as_array()
            .into_iter()
            .flatten()
            .map(|v| {
                v.as_i64()
                    .or_else(|| v.as_str().and_then(|s| s.parse().ok()))
                    .unwrap_or(-1)
            })
            .collect();
        let arg = |i: usize| a.get(i).copied().unwrap_or(0);
        let count_hero =
            |key: &str, min: i64| self.heroes().iter().filter(|h| n(h, key) >= min).count() as i64;
        Some(match row["Kind"].as_str().unwrap_or("") {
            "HeroDungeonPresentLevel" => {
                // StateValue is [claimed Present reward level, NPC index].
                // Step points to the next reward, while a claimed final reward
                // stays on its last step and increments EndRewardCount instead.
                let (level, npc) = (arg(0), arg(1));
                let rewards = state.tables.extensions.rows("NPCFriendlyPoint");
                let final_level = rewards.iter().filter(|r| n(r, "HeroIndex") == npc)
                    .map(|r| n(r, "Step")).max();
                (level > 0 && rewards.iter()
                    .any(|r| n(r, "HeroIndex") == npc && n(r, "Step") == level)
                    && self.npc_friendly().iter().any(|info| n(info, "HeroIndex") == npc
                        && (n(info, "Step") > level ||
                            (Some(level) == final_level && n(info, "Step") == level && n(info, "EndRewardCount") > 0)))) as i64
            }
            "DungeonCleared" => {
                self.stage(arg(0), arg(1))
                    .any(|d| d.0 == arg(0) && d.1 == arg(1) && d.2 > 0) as i64
            }
            "CompleteTutorial" => self.tutorials().contains(&arg(0)) as i64,
            "ChapterStarCount" => self.stars(state, arg(0)),
            "CheckTeamLevel" => (self.level() >= arg(0)) as i64,
            "HeroCountOfLevel" => count_hero("Level", arg(0)),
            "HeroCountOfStar" => count_hero("Star", arg(0)),
            "HeroCountOfTranscended" => count_hero("Transcended", arg(0)),
            "HeroCountOfIndex" => self
                .heroes_with_index(arg(0))
                .filter(|h| n(h, "HeroIndex") == arg(0))
                .count() as i64,
            "SkillExtendCount" => self
                .heroes()
                .iter()
                .map(|h| {
                    (1..=5)
                        .filter(|i| n(h, &format!("SkillExtend{i}")) >= arg(0))
                        .count() as i64
                })
                .sum(),
            "CostumeCount" => self.costumes(),
            // StateValue is [minimum Ether reinforcement, minimum awakening].
            // These are ownership predicates, so existing saves qualify too.
            "SoulWeaponCount" => self.soul_weapons().iter()
                .filter(|s| n(s, "ReinforceLevel") >= arg(0) && n(s, "Grade") >= arg(1))
                .count() as i64,
            "LoginDaily" => {
                // These special missions encode Day N in Variable3 while
                // ReqProgress is always one: it is a predicate, not a counter.
                if row["Goal"] == "SPECIAL_MISSION_GOAL_DESC_5001020" {
                    let day=row["Variable3"].as_array()?.first()?.as_str()?.parse::<i64>().ok()?;
                    if day<=0 {return None;}
                    (self.login_days()>=day) as i64
                } else {return None;}
            }
            "JoinGuild" => self.guild() as i64,
            "ClearGuildSuppress" => self.conquest_kills(),
            "FriendRegistration" => self.friends(),
            "WearEquip" => self
                .heroes()
                .iter()
                .map(|h| {
                    (1..=10)
                        .filter(|i| n(h, &format!("EquipItemSlotIndex{i}")) > 0)
                        .count() as i64
                })
                .sum(),
            "MissionClear" | "StepMissionClear" => {
                let lookup = self.mission_lookup.get_or_init(|| MissionLookup::new(&state.tables.progression));
                let Some(category) = lookup.categories.get(&n(row, "QuestIndex")) else {
                    return None;
                };
                let members = lookup.members.get(category).map(Vec::as_slice).unwrap_or(&[]);
                // A sub-quest without table rows can never be complete.
                let done = |quest: i64| {
                    self.last("subquest", quest, "all")
                        >= lookup.final_steps.get(&quest).copied().unwrap_or(i64::MAX)
                };
                if row["Kind"] == "MissionClear" {
                    members.iter().filter(|(quest, _)| done(*quest)).count() as i64
                } else {
                    let steps: BTreeSet<i64> = members.iter().map(|(_, step)| *step).collect();
                    steps
                        .iter()
                        .filter(|s| members.iter().filter(|(_, step)| step == *s).all(|(quest, _)| done(*quest)))
                        .count() as i64
                }
            }
            "AmountOfConsumeStamina" => self.metric("ConsumeStamina", "all", None, None),
            "ConsumeStamina" => self.metric("StaminaUses", "all", None, None),
            "ClearDungeon" => {
                self.metric("ClearDungeon", "all", a.first().copied(), a.get(1).copied())
            }
            "EnterWorldBoss" => {
                // Validated campaign entry already records these dungeon keys.
                // Read that history so participation before this evaluator was
                // added also counts, without awarding a win or requiring a kill.
                let locations: BTreeSet<_> = state.tables.battle.rows("WorldBoss").iter()
                    .map(|boss| (n(boss,"ChapterIndex"),n(boss,"DungeonIndex"))).collect();
                self.metrics_of("EnterDungeon").iter()
                    .filter(|(_,kind,chapter,dungeon,_)| kind=="EnterDungeon" && locations.contains(&(*chapter,*dungeon)))
                    .map(|(_,_,_,_,count)| *count).sum()
            }
            "BuyShopItem"
            | "ResetShop"
            | "UseItem"
            | "AddItem"
            | "DoHeroFriendly"
            | "ReceiveMail"
            | "CraftItem"
            | "SendFriendshipPoint"
            | "FriendRequest"
            | "EnterDungeon"
            | "UsePotionItem"
            | "UseBoosterItem"
            | "RotateRulletDaily" => self.metric(
                row["Kind"].as_str().unwrap(),
                "all",
                a.first().copied(),
                a.get(1).copied(),
            ),
            "AwakenGear"|"UpgradeEquipOption"|"UpgradeEquipTier"|"RenewEquipOption"|"Enchant"|"EquipRune"|"ArtifactRestore"|"SoulRestore"|"SoulWeaponRenewOption"|"GiveFriendlyPoint"|"GiveMultipleGiftItem" => self.metric(row["Kind"].as_str().unwrap(),"all",a.first().copied(),a.get(1).copied()),
            _ => return None,
        })
    }
    pub fn achievement(&self, state: &AppState, row: &Value) -> (i64, i64) {
        let req = row["ReqValue"]
            .as_str()
            .unwrap_or("")
            .parse::<i64>()
            .unwrap_or(0);
        let p = period(row);
        let kind = row["Kind"].as_str().unwrap_or("");
        // The native client compares Achievement against the last non-None subrequirement.
        let mut target = if n(row, "ReqSub2Type") != 0 {
            n(row, "ReqSub2Value")
        } else if n(row, "ReqSub1Type") != 0 {
            n(row, "ReqSub1Value")
        } else {
            req
        }
        .max(1);
        let sub = |t: i64| {
            if n(row, "ReqSub1Type") == t {
                n(row, "ReqSub1Value")
            } else if n(row, "ReqSub2Type") == t {
                n(row, "ReqSub2Value")
            } else {
                0
            }
        };
        let value = match kind {
            "LoginOrConnected" => self.metric("LoginDaily", &p, None, None),
            "TotalHero" => self.heroes().len() as i64,
            "TotalHeroStar" => self.heroes().iter().map(|h| n(h, "Star")).sum(),
            "HeroIndex" => self
                .heroes_with_index(req)
                .find(|h| n(h, "HeroIndex") == req)
                .map(|h| match n(row, "ReqSub1Type") {
                    1 => n(h, "Level"),
                    2 => n(h, "Star"),
                    15 => n(h, "Transcended"),
                    4 => {
                        (n(h, &format!("SkillExtend{}", n(row, "ReqSub1Value")))
                            >= state.tables.hero_shop.constant("MaxSkillExtend", 3))
                            as i64
                    }
                    5 => (1..=4).all(|s| {
                        n(h, &format!("SkillExtend{s}"))
                            >= state.tables.hero_shop.constant("MaxSkillExtend", 3)
                    }) as i64,
                    0 => 1,
                    _ => 0,
                })
                .unwrap_or(0),
            "GetItemIndex" => {
                target = sub(9).max(1);
                self.items().get(&req).copied().unwrap_or(0)
            }
            "AddFriend" => self.friends(),
            "GuildJoin" => self.guild() as i64,
            "ClearDungeon" => self.metric("ClearDungeon", &p, None, None),
            "ClearUnderPrison" => self.metrics_of("ClearDungeon").iter().filter(|(day,kind,chapter,dungeon,_)|
                (p=="all" || day>=&p) && kind=="ClearDungeon" && self.battle_type(state,*chapter,*dungeon)==Some(37))
                .map(|v|v.4).sum(),
            "ClearChallengeTowerDaily" | "ClearChallengeTowerWeekly" | "ClearOrdealTowerWeekly" => {
                let tower=if kind=="ClearOrdealTowerWeekly" {11} else {21};
                self.metrics_of("ClearDungeon").iter().filter(|(day,kind,chapter,dungeon,_)|
                    (p=="all" || day>=&p) && kind=="ClearDungeon" && self.is_tower_floor(state,tower,*chapter,*dungeon))
                    .map(|v|v.4).sum()
            }
            "ClearWorldBossDaily" | "ClearWorldBossWeekly" | "JoinWorldBoss" =>
                self.metric("CompleteWorldBoss",&p,None,None),
            "WinGuildSuppress" => self.metric("WinGuildSuppress",&p,None,None),
            "BuyShopItem" => self.metric(
                "BuyShopItem",
                &p,
                if sub(19) > 0 { Some(sub(19)) } else { None },
                None,
            ),
            "ConsumeGold" | "ConsumeGem" => self.metric(kind, &p, None, None),
            "ConsumeStamina" if row["ReqValue"] == "EclipseKey" => {
                target = sub(9).max(1);
                self.metric("ConsumeEclipseKey", &p, None, None)
            }
            "ConsumeStamina" => {
                if row["ReqValue"] == "Chicken" || row["ReqValue"] == "Stamina" || req > 0 {
                    if sub(9) > 0 {
                        target = sub(9);
                    }
                    self.metric("ConsumeStamina", &p, None, None)
                } else {
                    0
                }
            }
            "SendFriendshipPoint" => self.metric("SendFriendshipPoint", &p, None, None),
            "CompleteDailyQuest" | "CompleteWeeklyQuest" => self
                .claims_all()
                .iter()
                .filter(|c| {
                    c.0 == "achievement"
                        && c.3 != "all"
                        && c.3 >= p
                        && state.tables.progression.achievements.iter().any(|v| {
                            n(v, "Index") == c.1
                                && n(v, "Type") == if kind == "CompleteDailyQuest" { 2 } else { 5 }
                        })
                })
                .count() as i64,
            "RotateRulletDaily" | "RotateRulletWeekly" => {
                self.metric("RotateRulletDaily", &p, None, None)
            }
            _ => {
                if let Some((value, required)) = self.history_achievement(state, row, &p) {
                    target = required;
                    value
                } else { 0 }
            },
        };
        (value.max(0), target)
    }
    #[cfg(test)]
    pub fn achievement_infos(&self, state: &AppState) -> Vec<Value> {
        mission_groups(state).achievements.iter()
            .map(|(id, rows)| self.achievement_info(state, *id, &rows_of(&state.tables.progression.achievements, rows))).collect()
    }
    /// One achievement's info, from its table rows (all steps, table order).
    pub(super) fn achievement_info(&self, state: &AppState, id: i64, rows: &[&Value]) -> Value {
        let row = rows[0];
        let p = period(row);
        let last = self.last("achievement", id, &p);
        let current = rows
            .iter()
            .copied()
            .find(|v| n(v, "Step") == last + 1)
            .unwrap_or(row);
        let (value, _) = self.achievement(state, current);
        json!({"AchievementIndex":id,"LastStep":last,"Achievement":value,"ClearTeamLevel":self.level(),"UpdatedTime":format!("{} 00:00:00",Utc::now().date_naive())})
    }
    #[cfg(test)]
    pub fn subquest_infos(&self, state: &AppState) -> Vec<Value> {
        mission_groups(state).sub_quests.iter()
            .map(|(id, rows)| self.subquest_info(state, *id, &rows_of(&state.tables.progression.sub_quests, rows))).collect()
    }
    pub(super) fn subquest_info(&self, state: &AppState, id: i64, rows: &[&Value]) -> Value {
        let last = self.last("subquest", id, "all");
        let row = rows
            .iter()
            .copied()
            .find(|v| n(v, "Step") == last + 1)
            .unwrap_or(rows[0]);
        json!({"SubQuestIndex":id,"LastStep":last,"Progress":self.quest(state,row).min(n(row,"ReqProgress").max(1))})
    }
}

/// Achievement and sub-quest rows grouped per mission, once per loaded tables.
pub(super) fn mission_groups(state: &AppState) -> &crate::tables::MissionGroups {
    let tables = &state.tables.progression;
    tables.mission_groups.get_or_init(|| {
        let group = |rows: &[Value], key: &str| {
            let mut grouped = BTreeMap::<i64, Vec<usize>>::new();
            for (i, row) in rows.iter().enumerate() { grouped.entry(n(row, key)).or_default().push(i); }
            grouped.into_iter().collect()
        };
        crate::tables::MissionGroups {
            achievements: group(&tables.achievements, "Index"),
            sub_quests: group(&tables.sub_quests, "QuestIndex"),
        }
    })
}
/// One mission's rows (indices from `mission_groups`).
pub(super) fn rows_of<'a>(table: &'a [Value], indices: &[usize]) -> Vec<&'a Value> {
    indices.iter().map(|&i| &table[i]).collect()
}

#[cfg(test)]
mod lookup_tests {
    use super::*;

    #[test]
    fn battle_lookup_preserves_first_match_and_tower_variants() {
        let mut table = crate::tables::BattleTable::default();
        table.tables.insert("CampaignDungeon".into(), vec![
            json!({"ChapterIndex":20001,"DungeonIndex":1,"BattleType":37}),
            json!({"ChapterIndex":20001,"DungeonIndex":1,"BattleType":99}),
            json!({"ChapterIndex":20001,"DungeonIndex":2,"BattleType":14}),
        ]);
        table.tables.insert("TowerFloor".into(), vec![
            json!({"TowerIndex":21,"ChapterIndex":20001,"DungeonIndex":2,"DungeonIndices":[3,4],"Floor":5}),
            json!({"TowerIndex":21,"ChapterIndex":20001,"DungeonIndex":2,"Floor":7}),
            json!({"TowerIndex":11,"ChapterIndex":20001,"DungeonIndex":4,"Floor":8}),
            json!({"TowerIndex":11,"ChapterIndex":20001,"DungeonIndex":0,"DungeonIndices":[5],"Floor":25}),
        ]);
        let lookup = BattleLookup::new(&table);
        for chapter in [20001,20002] {
            for dungeon in 0..=5 {
                let linear = table.find("CampaignDungeon", &[("ChapterIndex",chapter),("DungeonIndex",dungeon)]).map(|r|n(r,"BattleType"));
                assert_eq!(lookup.types.get(&(chapter,dungeon)).copied(), linear);
                for tower in [11,21,99] {
                    let expected = match (chapter,tower,dungeon) {
                        (20001,21,2) => Some(7),
                        (20001,21,3|4) => Some(5),
                        (20001,11,4) => Some(8),
                        (20001,11,5) => Some(25),
                        _ => None,
                    };
                    assert_eq!(lookup.tower_floors.get(&(tower,chapter,dungeon)).copied(),expected);
                }
            }
        }
    }
}
