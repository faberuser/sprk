//! Mission predicates over durable, account-scoped gameplay history. Keep
//! participation, victories, current ownership and lifetime discoveries distinct.
use super::*;
use std::collections::{BTreeMap, BTreeSet};

#[derive(Default)]
pub(super) struct History {
    pub arena: Vec<(i64, i64, Value)>,
    /// Runs deleted by retention: (kind, UTC day, matches, wins, best streak, best tier).
    pub arena_archive: Vec<(i64, String, i64, i64, i64, i64)>,
    pub battle: BTreeMap<String, Value>,
    pub equipment: BTreeSet<i64>,
    pub facts: Vec<(String, i64, i64)>,
    pub guild_attendance: i64,
    pub guild_wins: i64,
    /// Guild War attacks, won or lost.
    pub guild_attacks: i64,
    pub conquest_entries: i64,
    pub guild_decks: Vec<Value>,
    pub pet_hatches: i64,
    pub monthly_stories: i64,
    pub linked_account: bool,
    pub world_rank: Option<i64>,
    /// Best single-battle damage per World Boss index.
    pub world_boss_best: BTreeMap<i64, i64>,
}

/// Mission-relevant fields of a completed arena run. Triggers keep
/// `mission_summary` equal to this expression for completed runs, and a
/// partial covering index stores it, so snapshots never re-parse each run's
/// full battle record (several MB per active arena player).
#[cfg(test)]
pub(crate) const ARENA_MISSION_SUMMARY: &str = "jsonb_build_object('DuelType',COALESCE(sprk_json(data,'$.DuelType'),'0'::jsonb),'CompletedAt',sprk_json(data,'$.CompletedAt'),'End',jsonb_build_object('MatchResult',sprk_json(data,'$.End.MatchResult'),'BattleInfo',jsonb_build_object('SuccessiveWin',sprk_json(data,'$.End.BattleInfo.SuccessiveWin')),'BattleBanPickInfo',jsonb_build_object('SuccessiveWin',sprk_json(data,'$.End.BattleBanPickInfo.SuccessiveWin'))))::text";



pub(crate) fn arena_history_query() -> &'static str {
    "SELECT kind,started,mission_summary FROM arena_runs WHERE account=$1 AND status='complete' ORDER BY id"
}

impl History {
    #[cfg(test)]
    pub async fn load(db: &mut PgConnection, account: i64) -> Result<Self> {
        let mut out = Self::default();
        out.reload(db, account, super::state::part::ALL, None).await?;
        Ok(out)
    }

    /// Load the history parts in `mask` (see `state::part`), keeping the
    /// others. Returns the parts whose data changed.
    /// `owned_equipment`: the account's equipment item indices when the caller
    /// just loaded them (HIST_EQUIP then needs no statement of its own).
    pub(super) async fn reload(&mut self, db: &mut PgConnection, account: i64, mask: u64, owned_equipment: Option<BTreeSet<i64>>) -> Result<u64> {
        use super::state::part::*;
        fn parse(raw: String) -> Result<Value> {
            serde_json::from_str(&raw)
                .map_err(|e| ServerError::Internal(format!("Invalid mission history: {e}")))
        }
        let before = std::mem::take(self);
        let out = self;
        let has = |bit: u64| mask & bit != 0;
        if has(ARENA) {
        out.arena.clear();
        for (kind, at, raw) in crate::db::query_as::<(i64,i64,String)>(arena_history_query())
            .bind(account).fetch_all(&mut *db).await? {
            let data = parse(raw)?;
            // All Arena missions (participation, wins, tiers and streaks) share
            // this history. Friendly duels never qualify as ranked play.
            if n(&data,"DuelType") != 1 && data["End"]["MatchResult"]["Canceled"] != true && data["End"]["MatchResult"].is_object() {
                out.arena.push((kind, n(&data,"CompletedAt").max(at), data));
            }
        }
        out.arena_archive = crate::db::query_as("SELECT kind,day,matches,wins,best_streak,best_tier FROM arena_history_archive WHERE account=$1")
            .bind(account).fetch_all(&mut *db).await?;
        }
        // Missions read only BestWave of eclipse_record and two fields of the
        // ordeal run, which reaches ~0.5 MB; the eclipse run is never read.
        // Deserializing just those fields skips building the rest per snapshot.
        #[derive(serde::Deserialize)]
        struct Ordeal {
            #[serde(rename = "SelectedTier", default)]
            selected_tier: Value,
            #[serde(rename = "ClearNodeIndices", default)]
            clear_node_indices: Value,
        }
        if has(BATTLE) {
        out.battle.clear();
        for (kind, raw) in crate::db::query_as::<(String,String)>(
            "SELECT kind,data FROM battle_state WHERE account=$1 AND kind IN ('eclipse_record','ordeal') AND idx=0")
            .bind(account).fetch_all(&mut *db).await? {
            // A non-object state (never written today) keeps the full parse.
            let data = match (kind == "ordeal").then(|| serde_json::from_str::<Ordeal>(&raw)) {
                Some(Ok(o)) => json!({"SelectedTier": o.selected_tier, "ClearNodeIndices": o.clear_node_indices}),
                _ => parse(raw)?,
            };
            out.battle.insert(kind, data);
        }
        }
        if has(HIST_EQUIP) {
        out.equipment = if let Some(owned) = owned_equipment { owned } else {
            crate::db::query_scalar("SELECT DISTINCT item_index FROM equip_items WHERE account_id=$1")
                .bind(account)
                .fetch_all(&mut *db)
                .await?
                .into_iter()
                .collect()
        };
        }
        if has(FACTS) {
        out.facts =
            crate::db::query_as("SELECT kind,idx,value FROM play_record_values WHERE account_id=$1")
                .bind(account)
                .fetch_all(&mut *db)
                .await?;
        }
        if has(GUILD_ATT) {
        out.guild_attendance = crate::db::query_scalar("SELECT COUNT(DISTINCT period) FROM community_claims WHERE account=$1 AND kind='guild_attend_total'")
            .bind(account).fetch_one(&mut *db).await?;
        }
        if has(GUILD_WINS) {
        (out.guild_wins, out.guild_attacks) = crate::db::query_as(
            "SELECT COUNT(*) FILTER (WHERE win=1),COUNT(*) FROM guild_arena_records WHERE account=$1",
        )
        .bind(account)
        .fetch_one(&mut *db)
        .await?;
        }
        if has(CONQUEST) {
        out.conquest_entries = crate::db::query_scalar("SELECT COUNT(*) FROM community_state WHERE kind='guild_conquest_history' AND sprk_json_int(data,'$.AccountId')=$1")
            .bind(account).fetch_one(&mut *db).await?;
        }
        if has(GUILD_DECKS) {
        out.guild_decks.clear();
        for raw in
            crate::db::query_scalar::<String>("SELECT data FROM guild_arena_decks WHERE account=$1")
                .bind(account)
                .fetch_all(&mut *db)
                .await?
        {
            out.guild_decks.push(parse(raw)?);
        }
        }
        if has(PET) {
        let pet = crate::api::extensions::get(db, account, "pet_misc", 6).await?;
        out.pet_hatches = n(&pet, "MiscValue");
        }
        if has(MONTHLY) {
        out.monthly_stories = crate::db::query_scalar("SELECT COUNT(*) FROM extension_state WHERE account_id=$1 AND kind='monthly_hero' AND sprk_json_text(data,'$.CompletedTime') IS NOT NULL")
            .bind(account).fetch_one(&mut *db).await?;
        }
        if has(LINKED) {
        out.linked_account = crate::db::query_scalar("SELECT EXISTS(SELECT 1 FROM credentials c JOIN accounts a ON a.login_id=c.login_id WHERE a.account_id=$1)")
            .bind(account).fetch_one(&mut *db).await?;
        }
        if has(WORLD_RANK) {
        out.world_rank = crate::db::query_scalar("SELECT MIN(place) FROM (SELECT account,ROW_NUMBER() OVER (PARTITION BY boss,season ORDER BY total DESC,account) place FROM (SELECT account,boss,season,MAX(score) total FROM battle_scores WHERE family='world_boss' GROUP BY account,boss,season HAVING MAX(score)>0)) WHERE account=$1")
            .bind(account).fetch_one(&mut *db).await?;
        out.world_boss_best = crate::db::query_as::<(i64, i64)>("SELECT boss,MAX(score) FROM battle_scores WHERE family='world_boss' AND account=$1 GROUP BY boss")
            .bind(account).fetch_all(&mut *db).await?.into_iter().collect();
        }
        let mut changed = 0;
        macro_rules! keep {
            ($bit:expr, $($field:ident),+) => {
                if has($bit) {
                    if $(out.$field != before.$field)||+ { changed |= $bit; }
                } else {
                    $(out.$field = before.$field;)+
                }
            };
        }
        keep!(ARENA, arena, arena_archive);
        keep!(BATTLE, battle);
        keep!(HIST_EQUIP, equipment);
        keep!(FACTS, facts);
        keep!(GUILD_ATT, guild_attendance);
        keep!(GUILD_WINS, guild_wins, guild_attacks);
        keep!(CONQUEST, conquest_entries);
        keep!(GUILD_DECKS, guild_decks);
        keep!(PET, pet_hatches);
        keep!(MONTHLY, monthly_stories);
        keep!(LINKED, linked_account);
        keep!(WORLD_RANK, world_rank, world_boss_best);
        Ok(changed)
    }

    pub(super) fn arena_count(&self, kind: i64, period: &str, win: bool) -> i64 {
        let archived: i64 = self.arena_archive.iter()
            .filter(|(k, day, ..)| *k == kind && (period == "all" || day.as_str() >= period))
            .map(|(_, _, matches, wins, ..)| if win { *wins } else { *matches })
            .sum();
        archived + self.arena
            .iter()
            .filter(|(k, at, data)| {
                *k == kind
                    && (period == "all"
                        || chrono::DateTime::from_timestamp(*at, 0)
                            .is_some_and(|d| d.date_naive().to_string().as_str() >= period))
                    && (!win || data["End"]["MatchResult"]["Win"] == true)
            })
            .count() as i64
    }

    pub(super) fn arena_best(&self, kind: i64, field: &str) -> i64 {
        let key = if kind == 1 {
            "BattleBanPickInfo"
        } else {
            "BattleInfo"
        };
        let archived = if field == "SuccessiveWin" {
            self.arena_archive.iter().filter(|(k, ..)| *k == kind).map(|a| a.4).max().unwrap_or(0)
        } else {
            0
        };
        self.arena
            .iter()
            .filter(|(k, _, _)| *k == kind)
            .map(|(_, _, d)| n(&d["End"][key], field))
            .max()
            .unwrap_or(0)
            .max(archived)
    }

    pub(super) fn best_tier(&self, kind: i64) -> i64 {
        self.arena
            .iter()
            .filter(|(k, _, _)| *k == kind)
            .map(|(_, _, d)| n(&d["End"]["MatchResult"], "NewTierIndex"))
            .max()
            .unwrap_or(0)
            .max(self.arena_archive.iter().filter(|(k, ..)| *k == kind).map(|a| a.5).max().unwrap_or(0))
    }
}

impl View {
    pub(super) fn history_proves_access(&self, row: &Value) -> bool {
        let c: Vec<_> = row["OpenCondition"]
            .as_array()
            .into_iter()
            .flatten()
            .filter_map(Value::as_str)
            .map(str::trim)
            .collect();
        match row["Kind"].as_str().unwrap_or("") {
            "ClearTechnoMagicKingdomRaid" => c == ["System", "CanGoDungeon", "10", "10"],
            // Tower of Challenge floors prove CanGoDungeon access; Ordeal Tower
            // (TowerIndex 11, normal achievement 511) floors prove the Hall of
            // Heroes entry condition, the same menu ClearOrdealTowerWeekly uses.
            "ClearTower" => {
                c == ["System", "CanGoDungeon", "2", "27"]
                    || (row["ReqValue"] == "11"
                        && c == ["Table", "NPCTable", "HallofHero", "EntryMenuCondition3"])
            }
            "ClearOrdealTowerWeekly" => c == ["Table", "NPCTable", "HallofHero", "EntryMenuCondition3"],
            "WorldBossMaxRank" => c == ["System", "MenuOpenCheckManager", "WorldBoss"],
            "JoinMatchBanPickWeekly" => c == ["System", "MatchManagement", "CheckBanPickCondition"],
            "EnterDungeon" if row["ReqValue"] == "GodkingTrial" => {
                c == ["Table", "NPCTable", "SoulWeapon", "EntryMenuCondition2"]
            }
            // Spending Eclipse keys proves the Eclipse menu (SoulWeapon entry 3).
            "ConsumeStamina" if row["ReqValue"] == "EclipseKey" => {
                c == ["Table", "NPCTable", "SoulWeapon", "EntryMenuCondition3"]
            }
            _ => false,
        }
    }
    fn stage_count(&self, s: &AppState, metric: &str, period: &str, types: &[i64]) -> i64 {
        self.metrics_of(metric)
            .iter()
            .filter(|(day, kind, c, d, _)| {
                kind == metric
                    && (period == "all" || day.as_str() >= period)
                    && self
                        .battle_type(s, *c, *d)
                        .is_some_and(|t| types.contains(&t))
            })
            .map(|m| m.4)
            .sum()
    }

    fn dragon_count(&self, s: &AppState, metric: &str, period: &str, hard: bool) -> i64 {
        let stages: BTreeSet<_> = s
            .tables
            .battle
            .rows("Raid")
            .iter()
            .filter(|r| {
                if hard {
                    matches!(n(r, "Type"), 2 | 3)
                } else {
                    (0..=3).contains(&n(r, "Type"))
                }
            })
            .map(|r| (n(r, "ChapterIndex"), n(r, "DungeonIndex")))
            .collect();
        self.metrics_of(metric)
            .iter()
            .filter(|(day, k, c, d, _)| {
                k == metric
                    && (period == "all" || day.as_str() >= period)
                    && stages.contains(&(*c, *d))
            })
            .map(|m| m.4)
            .sum()
    }

    pub(super) fn history_quest(&self, s: &AppState, row: &Value) -> Option<i64> {
        let kind = row["Kind"].as_str()?;
        let goal = row["Goal"].as_str().unwrap_or("");
        let variable = |key: &str| {
            row[key]
                .as_array()
                .and_then(|a| a.first())
                .and_then(Value::as_str)
                .unwrap_or("")
        };
        let required = variable("Variable3").parse::<i64>().ok().filter(|v| *v > 0);
        let predicate = |value: i64| required.is_some_and(|v| value >= v) as i64;
        let value = match kind {
            "EndEclipse" => match goal {
                "MILESTONE_MISSION_GOAL_10" => predicate(n(
                    self.hist(super::state::part::BATTLE)
                        .battle
                        .get("eclipse_record")
                        .unwrap_or(&Value::Null),
                    "BestWave",
                )),
                "MILESTONE_MISSION_GOAL_119" => self.metric("EndEclipse", "all", None, None).max(
                    (n(
                        self.hist(super::state::part::BATTLE)
                            .battle
                            .get("eclipse_record")
                            .unwrap_or(&Value::Null),
                        "BestWave",
                    ) > 0) as i64,
                ),
                _ => return None,
            },
            "EnterArena" => self.hist(super::state::part::ARENA).arena_count(0, "all", false),
            "EndArena" => match goal {
                "MAIN_MISSION_GOAL_29" | "MILESTONE_MISSION_GOAL_42" => {
                    self.hist(super::state::part::ARENA).arena_count(1, "all", false)
                }
                "MILESTONE_MISSION_GOAL_40" | "MILESTONE_MISSION_GOAL_43" => {
                    predicate(self.hist(super::state::part::ARENA).arena_best(
                        if goal == "MILESTONE_MISSION_GOAL_40" {
                            0
                        } else {
                            1
                        },
                        "SuccessiveWin",
                    ))
                }
                "MILESTONE_MISSION_GOAL_38" | "MILESTONE_MISSION_GOAL_41" => {
                    let arena = if goal == "MILESTONE_MISSION_GOAL_38" {
                        0
                    } else {
                        1
                    };
                    let tier = match variable("Variable3") {
                        "PVP_TIER_GOLD" => 2,
                        "PVP_TIER_PLATINUM" => 3,
                        "PVP_TIER_DIAMOND" => 4,
                        "PVP_TIER_MASTER" => 5,
                        "PVP_TIER_CHALLENGE" => 7,
                        _ => return None,
                    };
                    s.tables
                        .arena_guild
                        .rows(if arena == 1 {
                            "GlobalBanPickTier"
                        } else {
                            "MatchTier"
                        })
                        .iter()
                        .any(|r| {
                            n(r, "Index") == self.hist(super::state::part::ARENA).best_tier(arena) && n(r, "Tier") >= tier
                        }) as i64
                }
                _ => return None,
            },
            "EnterRaid" => match goal {
                "SPECIAL_MISSION_GOAL_5000940" => {
                    self.dragon_count(s, "EnterDungeon", "all", false)
                }
                "MAIN_MISSION_GOAL_59" => self.dragon_count(s, "EnterDungeon", "all", true),
                "SUPERPASS_MISSION_GOAL_GARGORIA_01" => {
                    self.stage_count(s, "EnterDungeon", "all", &[40])
                }
                _ => return None,
            },
            "ExecEquipGacha" => {
                let typ = match goal {
                    "MAIN_MISSION_GOAL_23" => 1,
                    "MAIN_MISSION_GOAL_68" => 4,
                    _ => return None,
                };
                self.metrics_of(kind)
                    .iter()
                    .filter(|m| {
                        m.1 == kind
                            && s.tables
                                .live
                                .find("NewEquipGacha", &[("Index", m.2)])
                                .is_some_and(|g| n(g, "Type") == typ)
                    })
                    .map(|m| m.4)
                    .sum()
            }
            "TransExtendCount" => {
                let args: Vec<i64> = row["StateValue"]
                    .as_array()?
                    .iter()
                    .map(|v| v.as_str().and_then(|v| v.parse().ok()).unwrap_or(-1))
                    .collect();
                if args.len() != 2 || args[1] <= 0 {
                    return Some(0);
                }
                self.heroes_with_index(args[0])
                    .find(|h| n(h, "HeroIndex") == args[0])
                    .is_some_and(|h| {
                        let page = format!("TranscendSkillPage{}", n(h, "ApplySkillPage").max(1));
                        h[page]
                            .as_str()
                            .and_then(|v| serde_json::from_str::<Vec<i64>>(v).ok())
                            .is_some_and(|skills| {
                                skills
                                    .into_iter()
                                    .filter(|v| *v > 0)
                                    .collect::<BTreeSet<_>>()
                                    .len() as i64
                                    >= args[1]
                            })
                    }) as i64
            }
            "GuildLoginDaily" => predicate(self.hist(super::state::part::GUILD_ATT).guild_attendance),
            "GuildArenaAttackSuccess" => self.hist(super::state::part::GUILD_WINS).guild_wins,
            "SetGuildArenaDeck" => {
                (!self.hist(super::state::part::GUILD_DECKS).guild_decks.is_empty() || self.metric(kind, "all", None, None) > 0)
                    as i64
            }
            "SetGuildArenaDeckSkill" => {
                (self.hist(super::state::part::GUILD_DECKS).guild_decks.iter().any(|d| {
                    d["SkillSlotIndices"]
                        .as_array()
                        .is_some_and(|a| !a.is_empty())
                }) || self.metric(kind, "all", None, None) > 0) as i64
            }
            "UsePetIncubator" => self
                .metric(kind, "all", None, None)
                .max(self.hist(super::state::part::PET).pet_hatches),
            "CraftEquip" | "IdentifiedEquip" => self.metric(kind, "all", None, None),
            "MonthlyHeroClear" => self.hist(super::state::part::MONTHLY).monthly_stories,
            "EndOrdealArena" => match goal {
                "MILESTONE_MISSION_GOAL_6311" => {
                    self.metric("WinOrdealArena", "all", None, None).max(
                        self.hist(super::state::part::BATTLE)
                            .battle
                            .get("ordeal")
                            .and_then(|v| v["ClearNodeIndices"].as_array())
                            .map_or(0, |nodes| {
                                s.tables
                                    .battle
                                    .rows("OrdealArenaNode")
                                    .iter()
                                    .filter(|r| {
                                        n(r, "NodeType") == 2 && nodes.contains(&r["Index"])
                                    })
                                    .count() as i64
                            }),
                    )
                }
                "MILESTONE_MISSION_GOAL_6301" => {
                    let tier = match variable("Variable3") {
                        "PVP_TIER_GOLD" => 2,
                        "PVP_TIER_PLATINUM" => 3,
                        "PVP_TIER_DIAMOND" => 4,
                        "PVP_TIER_MASTER" => 5,
                        "PVP_TIER_CHALLENGE" => 7,
                        _ => return None,
                    };
                    (self
                        .metrics_of("ClearOrdealArenaTier")
                        .iter()
                        .any(|m| m.1 == "ClearOrdealArenaTier" && m.2 >= tier && m.4 > 0)
                        || self.hist(super::state::part::BATTLE).battle.get("ordeal").is_some_and(|v| {
                            n(v, "SelectedTier") >= tier
                                && s.tables.battle.rows("OrdealArenaNode").iter().any(|r| {
                                    n(r, "NodeType") == 3
                                        && v["ClearNodeIndices"]
                                            .as_array()
                                            .is_some_and(|nodes| nodes.contains(&r["Index"]))
                                })
                        })) as i64
                }
                _ => return None,
            },
            "SubStoryCostumeUnlock" => s
                .tables
                .battle
                .rows("SubStoryUnlock")
                .iter()
                .filter(|r| {
                    self.stage(n(r, "ChapterIndex"), n(r, "DungeonIndex")).any(|d| {
                        d.0 == n(r, "ChapterIndex") && d.1 == n(r, "DungeonIndex") && d.2 > 0
                    })
                })
                .map(|r| n(r, "CreatureIndex"))
                .collect::<BTreeSet<_>>()
                .len() as i64,
            _ => return None,
        };
        Some(value.max(0))
    }

    fn discovered_equipment(&self) -> BTreeSet<i64> {
        self.hist(super::state::part::HIST_EQUIP)
            .equipment
            .iter()
            .copied()
            .chain(
                self.hist(super::state::part::FACTS)
                    .facts
                    .iter()
                    .filter(|(k, _, v)| k == "equip_count" && *v > 0)
                    .map(|(_, id, _)| *id),
            )
            .chain(
                self.metrics_of("AddEquip")
                    .iter()
                    .filter(|m| m.1 == "AddEquip" && m.4 > 0)
                    .map(|m| m.2),
            )
            .collect()
    }

    /// Distinct discovered equipment and item indices per Total* achievement kind.
    fn inventory_totals(&self, s: &AppState) -> &super::state::InventoryTotals {
        // Memoised: record its inputs on every call, not only the first.
        self.hist(super::state::part::HIST_EQUIP | super::state::part::FACTS);
        self.items();
        self.metrics_of("AddEquip");
        self.metrics_of("AddItem");
        self.inventory_totals.get_or_init(|| {
            let ids = self.discovered_equipment();
            let mut sets: [BTreeSet<i64>; 4] = Default::default();
            // Only the discovered items, through the view's table index.
            for &index in &ids {
                let Some((part, detail, _)) = self.equipment_metadata(s, index) else { continue; };
                if !matches!(detail, 90001 | 90003 | 90004) && part != 5 { sets[0].insert(index); }
                if part == 1 && matches!(detail, 90001 | 90003) { sets[1].insert(index); }
                if part == 7 && detail == 90004 { sets[2].insert(index); }
                if part == 5 { sets[3].insert(index); }
            }
            let normal: BTreeSet<i64> = self
                .items()
                .iter()
                .filter(|(_, v)| **v > 0)
                .map(|(id, _)| *id)
                .chain(
                    self.metrics_of("AddItem")
                        .iter()
                        .filter(|m| m.1 == "AddItem" && m.4 > 0)
                        .map(|m| m.2),
                )
                .collect();
            super::state::InventoryTotals {
                equipment: sets[0].len() as i64,
                unique_weapon: sets[1].len() as i64,
                unique_treasure: sets[2].len() as i64,
                artifact: sets[3].len() as i64,
                item: normal.union(&ids).count() as i64,
                normal_item: normal.len() as i64,
            }
        })
    }

    pub(super) fn history_achievement(
        &self,
        s: &AppState,
        row: &Value,
        p: &str,
    ) -> Option<(i64, i64)> {
        let kind = row["Kind"].as_str()?;
        let req = row["ReqValue"]
            .as_str()
            .and_then(|v| v.parse::<i64>().ok())
            .unwrap_or(0);
        let value = match kind {
            "ClearRaidDaily" | "ClearRaidWeekly" => self.dragon_count(s, "ClearDungeon", p, false),
            "ClearHardRaid" => {
                return Some((
                    self.dragon_count(s, "ClearDungeon", p, true),
                    n(row, "ReqSub1Value").max(1),
                ))
            }
            "ClearChallengeRaidWeekly" => self.stage_count(s, "ClearDungeon", p, &[25, 30]),
            "ClearTechnoMagicKingdomRaid" => self.stage_count(s, "ClearDungeon", p, &[40, 41, 42]),
            "ClearTreasureHouse" => self.stage_count(s, "ClearDungeon", p, &[27]),
            "EnterDungeon" if row["ReqValue"] == "GodkingTrial" => {
                return Some((
                    self.stage_count(s, "EnterDungeon", p, &[35]),
                    n(row, "ReqSub1Value").max(1),
                ))
            }
            "JoinMatch" => self.hist(super::state::part::ARENA).arena_count(0, p, false),
            "MatchVictory" => self.hist(super::state::part::ARENA).arena_count(0, p, true),
            "JoinMatchBanPick" | "JoinMatchBanPickWeekly" => self.hist(super::state::part::ARENA).arena_count(1, p, false),
            "WinMatchBanPick" => self.hist(super::state::part::ARENA).arena_count(1, p, true),
            "MatchGrade" => self.hist(super::state::part::ARENA).best_tier(0),
            "GlobalMatchBanPickGrade" => self.hist(super::state::part::ARENA).best_tier(1),
            "MatchTierRank" | "GlobalMatchBanPickTierRank" => {
                let arena = if kind == "MatchTierRank" { 0 } else { 1 };
                return Some((
                    self.hist(super::state::part::ARENA).arena.iter().any(|(k, _, d)| {
                        *k == arena
                            && n(&d["End"]["MatchResult"], "NewTierIndex") >= req
                            && d["End"]["MatchResult"]["NewTierRank"]
                                .as_i64()
                                .is_some_and(|r| r >= 0 && r < n(row, "ReqSub1Value").max(1))
                    }) as i64,
                    1,
                ));
            }
            "ClearTower" => {
                return Some((
                    self.metrics_of("ClearDungeon")
                        .iter()
                        .filter(|m| m.1 == "ClearDungeon" && m.4 > 0)
                        .filter_map(|m| self.tower_floor(s, req, m.2, m.3))
                        .max()
                        .unwrap_or(0),
                    n(row, "ReqSub1Value").max(1),
                ))
            }
            "JoinGuildSuppress" => self.hist(super::state::part::CONQUEST).conquest_entries,
            "ExecEquipGacha" => {
                let typ = match row["ReqValue"].as_str()? {
                    "High" => 1,
                    "Normal" => 0,
                    _ => return None,
                };
                return Some((
                    self.metrics_of(kind)
                        .iter()
                        .filter(|m| {
                            m.1 == kind
                                && (p == "all" || m.0.as_str() >= p)
                                && s.tables
                                    .live
                                    .find("NewEquipGacha", &[("Index", m.2)])
                                    .is_some_and(|g| n(g, "Type") == typ)
                        })
                        .map(|m| m.4)
                        .sum(),
                    n(row, "ReqSub1Value").max(1),
                ));
            }
            // Row-independent: computed once per snapshot (one pass over the
            // EquipItem table) instead of once per achievement row.
            "TotalEquipment" => self.inventory_totals(s).equipment,
            "TotalUniqueWeapon" => self.inventory_totals(s).unique_weapon,
            "TotalUniqueTreasure" => self.inventory_totals(s).unique_treasure,
            "TotalArtifact" => self.inventory_totals(s).artifact,
            "TotalItem" => self.inventory_totals(s).item,
            "TotalNormalItem" => self.inventory_totals(s).normal_item,
            "TotalSoulWeapon" => self.soul_weapons().len() as i64,
            "TotalSoulWeaponUpgrade" => self
                .soul_weapons()
                .iter()
                .map(|r| n(r, "Grade"))
                .max()
                .unwrap_or(0),
            "TotalSoulWeaponReinforce" => self
                .soul_weapons()
                .iter()
                .map(|r| n(r, "ReinforceLevel"))
                .max()
                .unwrap_or(0),
            "BattleRewardEquip" => self.metric(kind, p, None, None),
            "KillMonster" => {
                self.metric(kind, p, None, None)
                    .max(self.metric("DefeatMonster", p, None, None))
            }
            "TotalMonster" | "TotalNormalMonster" | "TotalBossMonster" => {
                let known: BTreeSet<_> = self
                    .metrics_of("DefeatMonster")
                    .iter()
                    .filter(|m| m.1 == "DefeatMonster" && m.4 > 0)
                    .map(|m| m.2)
                    .collect();
                s.tables
                    .battle
                    .rows("MonsterArchive")
                    .iter()
                    .filter(|r| {
                        known.contains(&n(r, "Index"))
                            && match kind {
                                "TotalNormalMonster" => n(r, "ArchiveType") == 7,
                                "TotalBossMonster" => n(r, "ArchiveType") != 7,
                                _ => true,
                            }
                    })
                    .count() as i64
            }
            "WorldBossMaxRank" => {
                return Some((
                    self.hist(super::state::part::WORLD_RANK)
                        .world_rank
                        .is_some_and(|rank| rank <= req && rank > 0) as i64,
                    1,
                ))
            }
            "LinkAccount" => return Some((self.hist(super::state::part::LINKED).linked_account as i64, 1)),
            _ => return None,
        };
        Some((value.max(0), req.max(1)))
    }
}

/// Called only inside authenticated worker settlement, behind the existing
/// run completion/retry guard. Client-supplied kill counts are never accepted.
pub(crate) async fn worker_defeats(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    body: &Value,
) -> Result<()> {
    let Some(counts) = body["DefeatedCreatures"].as_object() else {
        return Ok(());
    };
    if counts.len() > 4096 {
        return Err(rule("InvalidValue"));
    }
    let mut total = 0;
    for (key, value) in counts {
        let id = key
            .parse::<i64>()
            .ok()
            .filter(|id| *id > 0)
            .ok_or_else(|| rule("InvalidValue"))?;
        let count = value
            .as_i64()
            .filter(|v| *v > 0 && *v <= 100000)
            .ok_or_else(|| rule("InvalidValue"))?;
        if state
            .tables
            .battle
            .find("MonsterArchive", &[("Index", id)])
            .is_none()
        {
            continue;
        }
        record(db, account, "DefeatMonster", id, 0, count).await?;
        total += count;
    }
    record(db, account, "KillMonster", 0, 0, total).await?;
    Ok(())
}
