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
    pub conquest_entries: i64,
    pub guild_decks: Vec<Value>,
    pub pet_hatches: i64,
    pub monthly_stories: i64,
    pub linked_account: bool,
    pub world_rank: Option<i64>,
}

/// Mission-relevant fields of a completed arena run. Triggers keep
/// `mission_summary` equal to this expression for completed runs, and a
/// partial covering index stores it, so snapshots never re-parse each run's
/// full battle record (several MB per active arena player).
pub(crate) const ARENA_MISSION_SUMMARY: &str = "json_object('DuelType',COALESCE(json_extract(data,'$.DuelType'),0),'CompletedAt',json_extract(data,'$.CompletedAt'),'End',json_object('MatchResult',json_extract(data,'$.End.MatchResult'),'BattleInfo',json_object('SuccessiveWin',json_extract(data,'$.End.BattleInfo.SuccessiveWin')),'BattleBanPickInfo',json_object('SuccessiveWin',json_extract(data,'$.End.BattleBanPickInfo.SuccessiveWin'))))";
pub(crate) async fn migrate_arena_history(db: &sqlx::SqlitePool) -> Result<()> {
    let mut tx = db.begin().await?;
    let columns: Vec<String> = sqlx::query_scalar("SELECT name FROM pragma_table_info('arena_runs')").fetch_all(&mut *tx).await?;
    if !columns.iter().any(|c| c == "mission_summary") {
        sqlx::query("ALTER TABLE arena_runs ADD COLUMN mission_summary TEXT").execute(&mut *tx).await?;
    }
    let summary = ARENA_MISSION_SUMMARY.replace("data", "NEW.data");
    for (name, event) in [("insert", "INSERT"), ("update", "UPDATE OF status,data")] {
        // Replace old trigger definitions as well as backfilling old summaries.
        sqlx::query(&format!("DROP TRIGGER IF EXISTS arena_mission_summary_{name}")).execute(&mut *tx).await?;
        sqlx::query(&format!("CREATE TRIGGER IF NOT EXISTS arena_mission_summary_{name} AFTER {event} ON arena_runs WHEN NEW.status='complete' BEGIN UPDATE arena_runs SET mission_summary={summary} WHERE id=NEW.id; END"))
            .execute(&mut *tx).await?;
    }
    sqlx::query(&format!("UPDATE arena_runs SET mission_summary={ARENA_MISSION_SUMMARY} WHERE status='complete' AND (mission_summary IS NULL OR json_type(mission_summary,'$.DuelType') IS NULL)"))
        .execute(&mut *tx).await?;
    sqlx::query("CREATE INDEX IF NOT EXISTS arena_mission_history ON arena_runs(account,status,id,kind,started,mission_summary) WHERE status='complete'")
        .execute(&mut *tx).await?;
    tx.commit().await?;
    Ok(())
}
/// Finished arena runs kept per account and arena kind. Older runs are
/// deleted once their mission-relevant facts are folded into
/// `arena_history_archive` (per UTC day), so mission progress is unchanged.
pub(crate) const ARENA_RUNS_KEPT: i64 = 20;

/// Small keys that decide which older runs remain readable, maintained by
/// triggers so retention never re-parses large run payloads.
const ARENA_RETENTION_KEYS: &str = "season=json_extract(data,'$.Season'),nonduel=(json_extract(data,'$.DuelType') IS NOT 1),squad=(json_extract(data,'$.DuelType') IS NOT 1 AND COALESCE(json_array_length(data,'$.Heroes') BETWEEN 1 AND 4,0))";

/// Finished runs of `account` that may be deleted. Besides the newest
/// ARENA_RUNS_KEPT per kind, these readers' rows always survive:
/// the newest complete run per (kind, season) (ranking decks), the newest
/// complete non-friendly run with a 1-4 hero squad per (kind, season)
/// (squad lookups, profile inspection), and the newest non-friendly run per
/// kind (defense team).
fn arena_prunable(account: &str) -> String {
    format!("SELECT id FROM (SELECT id,status,squad,nonduel,\
        ROW_NUMBER() OVER (PARTITION BY kind ORDER BY id DESC) AS recent,\
        ROW_NUMBER() OVER (PARTITION BY kind,season,status='complete' ORDER BY id DESC) AS season_last,\
        ROW_NUMBER() OVER (PARTITION BY kind,season,status='complete' AND squad ORDER BY id DESC) AS squad_last,\
        ROW_NUMBER() OVER (PARTITION BY kind,nonduel ORDER BY id DESC) AS defense_last \
        FROM arena_runs WHERE account={account} AND status NOT IN ('waiting','battle')) \
        WHERE recent>{ARENA_RUNS_KEPT} \
        AND NOT (status='complete' AND season_last=1) \
        AND NOT (status='complete' AND squad AND squad_last=1) \
        AND NOT (nonduel AND defense_last=1)")
}

/// Archive then delete prunable runs of `account` (an SQL expression).
fn arena_prune_statements(account: &str) -> [String; 2] {
    let doomed = arena_prunable(account);
    [format!("INSERT INTO arena_history_archive(account,kind,day,matches,wins,best_streak,best_tier) \
        SELECT account,kind,date(MAX(COALESCE(json_extract(mission_summary,'$.CompletedAt'),0),started),'unixepoch') AS day,COUNT(*),\
        SUM(COALESCE(json_extract(mission_summary,'$.End.MatchResult.Win'),0)=1),\
        MAX(COALESCE(json_extract(mission_summary,CASE kind WHEN 1 THEN '$.End.BattleBanPickInfo.SuccessiveWin' ELSE '$.End.BattleInfo.SuccessiveWin' END),0)),\
        MAX(COALESCE(json_extract(mission_summary,'$.End.MatchResult.NewTierIndex'),0)) \
        FROM arena_runs WHERE id IN ({doomed}) AND status='complete' \n        AND COALESCE(json_extract(mission_summary,'$.DuelType'),0)!=1 AND json_type(mission_summary,'$.End.MatchResult')='object' AND COALESCE(json_extract(mission_summary,'$.End.MatchResult.Canceled'),0)!=1 \n        GROUP BY account,kind,day \
        ON CONFLICT(account,kind,day) DO UPDATE SET matches=matches+excluded.matches,wins=wins+excluded.wins,\
        best_streak=MAX(best_streak,excluded.best_streak),best_tier=MAX(best_tier,excluded.best_tier)"),
     format!("DELETE FROM arena_runs WHERE id IN ({doomed})")]
}

/// Bound arena history per player. Runs inside the statement that finishes a
/// run, so every completion path (local, online, expiry, cancel) is covered.
/// Requires `migrate_arena_history` first (mission summaries of old runs).
pub(crate) async fn migrate_arena_retention(db: &sqlx::SqlitePool) -> Result<()> {
    sqlx::query("CREATE TABLE IF NOT EXISTS arena_history_archive(account INTEGER NOT NULL,kind INTEGER NOT NULL,day TEXT NOT NULL,matches INTEGER NOT NULL,wins INTEGER NOT NULL,best_streak INTEGER NOT NULL,best_tier INTEGER NOT NULL,PRIMARY KEY(account,kind,day)) WITHOUT ROWID")
        .execute(db).await?;
    let columns: Vec<String> = sqlx::query_scalar("SELECT name FROM pragma_table_info('arena_runs')").fetch_all(db).await?;
    for column in ["season", "nonduel", "squad"] {
        if !columns.iter().any(|c| c == column) {
            sqlx::query(&format!("ALTER TABLE arena_runs ADD COLUMN {column} INTEGER")).execute(db).await?;
        }
    }
    let keys = ARENA_RETENTION_KEYS.replace("data", "NEW.data");
    for (name, event) in [("insert", "INSERT"), ("update", "UPDATE OF data")] {
        sqlx::query(&format!("CREATE TRIGGER IF NOT EXISTS arena_retention_keys_{name} AFTER {event} ON arena_runs BEGIN UPDATE arena_runs SET {keys} WHERE id=NEW.id; END"))
            .execute(db).await?;
    }
    sqlx::query(&format!("UPDATE arena_runs SET {ARENA_RETENTION_KEYS} WHERE nonduel IS NULL")).execute(db).await?;
    sqlx::query("CREATE INDEX IF NOT EXISTS arena_retention ON arena_runs(account,kind,id,status,season,nonduel,squad)")
        .execute(db).await?;
    let [archive, delete] = arena_prune_statements("NEW.account");
    let mut trigger_tx = db.begin().await?;
    for (name, event, condition) in [
        ("finish", "UPDATE OF status", "OLD.status IN ('waiting','battle') AND NEW.status NOT IN ('waiting','battle')"),
        ("insert", "INSERT", "NEW.status NOT IN ('waiting','battle')"),
    ] {
        sqlx::query(&format!("DROP TRIGGER IF EXISTS arena_retention_{name}")).execute(&mut *trigger_tx).await?;
        sqlx::query(&format!("CREATE TRIGGER IF NOT EXISTS arena_retention_{name} AFTER {event} ON arena_runs WHEN {condition} BEGIN {archive}; {delete}; END"))
            .execute(&mut *trigger_tx).await?;
    }
    trigger_tx.commit().await?;
    // Existing histories: one transaction per account over the limit.
    let accounts: Vec<i64> = sqlx::query_scalar(&format!(
        "SELECT DISTINCT account FROM (SELECT account FROM arena_runs WHERE status NOT IN ('waiting','battle') GROUP BY account,kind HAVING COUNT(*)>{ARENA_RUNS_KEPT})"))
        .fetch_all(db).await?;
    let mut pruned = 0u64;
    for account in accounts {
        let [archive, delete] = arena_prune_statements("?");
        let mut tx = db.begin().await?;
        sqlx::query(&archive).bind(account).execute(&mut *tx).await?;
        pruned += sqlx::query(&delete).bind(account).execute(&mut *tx).await?.rows_affected();
        tx.commit().await?;
    }
    if pruned > 0 {
        tracing::info!(pruned, "Archived and deleted old arena runs; run VACUUM while the server is stopped to return the space to disk");
    }
    Ok(())
}

pub(crate) fn arena_history_query() -> &'static str {
    "SELECT kind,started,mission_summary FROM arena_runs WHERE account=? AND status='complete' ORDER BY id"
}

impl History {
    pub async fn load(db: &mut SqliteConnection, account: i64) -> Result<Self> {
        fn parse(raw: String) -> Result<Value> {
            serde_json::from_str(&raw)
                .map_err(|e| ServerError::Internal(format!("Invalid mission history: {e}")))
        }
        let mut out = Self::default();
        for (kind, at, raw) in sqlx::query_as::<_, (i64,i64,String)>(arena_history_query())
            .bind(account).fetch_all(&mut *db).await? {
            let data = parse(raw)?;
            // All Arena missions (participation, wins, tiers and streaks) share
            // this history. Friendly duels never qualify as ranked play.
            if n(&data,"DuelType") != 1 && data["End"]["MatchResult"]["Canceled"] != true && data["End"]["MatchResult"].is_object() {
                out.arena.push((kind, n(&data,"CompletedAt").max(at), data));
            }
        }
        out.arena_archive = sqlx::query_as("SELECT kind,day,matches,wins,best_streak,best_tier FROM arena_history_archive WHERE account=?")
            .bind(account).fetch_all(&mut *db).await?;
        for (kind, raw) in sqlx::query_as::<_,(String,String)>(
            "SELECT kind,data FROM battle_state WHERE account=? AND kind IN ('eclipse_record','eclipse','ordeal') AND idx=0")
            .bind(account).fetch_all(&mut *db).await? { out.battle.insert(kind, parse(raw)?); }
        out.equipment =
            sqlx::query_scalar("SELECT DISTINCT item_index FROM equip_items WHERE account_id=?")
                .bind(account)
                .fetch_all(&mut *db)
                .await?
                .into_iter()
                .collect();
        out.facts =
            sqlx::query_as("SELECT kind,idx,value FROM play_record_values WHERE account_id=?")
                .bind(account)
                .fetch_all(&mut *db)
                .await?;
        out.guild_attendance = sqlx::query_scalar("SELECT COUNT(DISTINCT period) FROM community_claims WHERE account=? AND kind='guild_attend_total'")
            .bind(account).fetch_one(&mut *db).await?;
        out.guild_wins = sqlx::query_scalar(
            "SELECT COUNT(*) FROM guild_arena_records WHERE account=? AND win=1",
        )
        .bind(account)
        .fetch_one(&mut *db)
        .await?;
        out.conquest_entries = sqlx::query_scalar("SELECT COUNT(*) FROM community_state WHERE kind='guild_conquest_history' AND json_extract(data,'$.AccountId')=?")
            .bind(account).fetch_one(&mut *db).await?;
        for raw in
            sqlx::query_scalar::<_, String>("SELECT data FROM guild_arena_decks WHERE account=?")
                .bind(account)
                .fetch_all(&mut *db)
                .await?
        {
            out.guild_decks.push(parse(raw)?);
        }
        let pet = crate::api::extensions::get(db, account, "pet_misc", 6).await?;
        out.pet_hatches = n(&pet, "MiscValue");
        out.monthly_stories = sqlx::query_scalar("SELECT COUNT(*) FROM extension_state WHERE account_id=? AND kind='monthly_hero' AND json_extract(data,'$.CompletedTime') IS NOT NULL")
            .bind(account).fetch_one(&mut *db).await?;
        out.linked_account = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM credentials c JOIN accounts a ON a.login_id=c.login_id WHERE a.account_id=?)")
            .bind(account).fetch_one(&mut *db).await?;
        out.world_rank = sqlx::query_scalar("SELECT MIN(place) FROM (SELECT account,ROW_NUMBER() OVER (PARTITION BY boss,season ORDER BY total DESC,account) place FROM (SELECT account,boss,season,MAX(score) total FROM battle_scores WHERE family='world_boss' GROUP BY account,boss,season HAVING MAX(score)>0)) WHERE account=?")
            .bind(account).fetch_one(&mut *db).await?;
        Ok(out)
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
            _ => false,
        }
    }
    fn stage_count(&self, s: &AppState, metric: &str, period: &str, types: &[i64]) -> i64 {
        self.metrics
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
        self.metrics
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
                    self.history
                        .battle
                        .get("eclipse_record")
                        .unwrap_or(&Value::Null),
                    "BestWave",
                )),
                "MILESTONE_MISSION_GOAL_119" => self.metric("EndEclipse", "all", None, None).max(
                    (n(
                        self.history
                            .battle
                            .get("eclipse_record")
                            .unwrap_or(&Value::Null),
                        "BestWave",
                    ) > 0) as i64,
                ),
                _ => return None,
            },
            "EnterArena" => self.history.arena_count(0, "all", false),
            "EndArena" => match goal {
                "MAIN_MISSION_GOAL_29" | "MILESTONE_MISSION_GOAL_42" => {
                    self.history.arena_count(1, "all", false)
                }
                "MILESTONE_MISSION_GOAL_40" | "MILESTONE_MISSION_GOAL_43" => {
                    predicate(self.history.arena_best(
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
                            n(r, "Index") == self.history.best_tier(arena) && n(r, "Tier") >= tier
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
                self.metrics
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
                self.heroes
                    .iter()
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
            "GuildLoginDaily" => predicate(self.history.guild_attendance),
            "GuildArenaAttackSuccess" => self.history.guild_wins,
            "SetGuildArenaDeck" => {
                (!self.history.guild_decks.is_empty() || self.metric(kind, "all", None, None) > 0)
                    as i64
            }
            "SetGuildArenaDeckSkill" => {
                (self.history.guild_decks.iter().any(|d| {
                    d["SkillSlotIndices"]
                        .as_array()
                        .is_some_and(|a| !a.is_empty())
                }) || self.metric(kind, "all", None, None) > 0) as i64
            }
            "UsePetIncubator" => self
                .metric(kind, "all", None, None)
                .max(self.history.pet_hatches),
            "CraftEquip" | "IdentifiedEquip" => self.metric(kind, "all", None, None),
            "MonthlyHeroClear" => self.history.monthly_stories,
            "EndOrdealArena" => match goal {
                "MILESTONE_MISSION_GOAL_6311" => {
                    self.metric("WinOrdealArena", "all", None, None).max(
                        self.history
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
                        .metrics
                        .iter()
                        .any(|m| m.1 == "ClearOrdealArenaTier" && m.2 >= tier && m.4 > 0)
                        || self.history.battle.get("ordeal").is_some_and(|v| {
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
                    self.dungeons.iter().any(|d| {
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
        self.history
            .equipment
            .iter()
            .copied()
            .chain(
                self.history
                    .facts
                    .iter()
                    .filter(|(k, _, v)| k == "equip_count" && *v > 0)
                    .map(|(_, id, _)| *id),
            )
            .chain(
                self.metrics
                    .iter()
                    .filter(|m| m.1 == "AddEquip" && m.4 > 0)
                    .map(|m| m.2),
            )
            .collect()
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
            "JoinMatch" => self.history.arena_count(0, p, false),
            "MatchVictory" => self.history.arena_count(0, p, true),
            "JoinMatchBanPick" | "JoinMatchBanPickWeekly" => self.history.arena_count(1, p, false),
            "WinMatchBanPick" => self.history.arena_count(1, p, true),
            "MatchGrade" => self.history.best_tier(0),
            "GlobalMatchBanPickGrade" => self.history.best_tier(1),
            "MatchTierRank" | "GlobalMatchBanPickTierRank" => {
                let arena = if kind == "MatchTierRank" { 0 } else { 1 };
                return Some((
                    self.history.arena.iter().any(|(k, _, d)| {
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
                    self.metrics
                        .iter()
                        .filter(|m| m.1 == "ClearDungeon" && m.4 > 0)
                        .filter_map(|m| self.tower_floor(s, req, m.2, m.3))
                        .max()
                        .unwrap_or(0),
                    n(row, "ReqSub1Value").max(1),
                ))
            }
            "JoinGuildSuppress" => self.history.conquest_entries,
            "ExecEquipGacha" => {
                let typ = match row["ReqValue"].as_str()? {
                    "High" => 1,
                    "Normal" => 0,
                    _ => return None,
                };
                return Some((
                    self.metrics
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
            "TotalEquipment" | "TotalUniqueWeapon" | "TotalUniqueTreasure" | "TotalArtifact" => {
                let ids = self.discovered_equipment();
                s.tables
                    .extensions
                    .rows("EquipItem")
                    .iter()
                    .filter(|r| {
                        ids.contains(&n(r, "ItemIndex"))
                            && match kind {
                                "TotalUniqueWeapon" => {
                                    n(r, "PartType") == 1
                                        && matches!(n(r, "DetailIndex"), 90001 | 90003)
                                }
                                "TotalUniqueTreasure" => {
                                    n(r, "PartType") == 7 && n(r, "DetailIndex") == 90004
                                }
                                "TotalArtifact" => n(r, "PartType") == 5,
                                _ => {
                                    !matches!(n(r, "DetailIndex"), 90001 | 90003 | 90004)
                                        && n(r, "PartType") != 5
                                }
                            }
                    })
                    .map(|r| n(r, "ItemIndex"))
                    .collect::<BTreeSet<_>>()
                    .len() as i64
            }
            "TotalItem" | "TotalNormalItem" => {
                let mut ids: BTreeSet<_> = self
                    .items
                    .iter()
                    .filter(|(_, v)| **v > 0)
                    .map(|(id, _)| *id)
                    .chain(
                        self.metrics
                            .iter()
                            .filter(|m| m.1 == "AddItem" && m.4 > 0)
                            .map(|m| m.2),
                    )
                    .collect();
                if kind == "TotalItem" {
                    ids.extend(self.discovered_equipment());
                }
                ids.len() as i64
            }
            "TotalSoulWeapon" => self.soul_weapons.len() as i64,
            "TotalSoulWeaponUpgrade" => self
                .soul_weapons
                .iter()
                .map(|r| n(r, "Grade"))
                .max()
                .unwrap_or(0),
            "TotalSoulWeaponReinforce" => self
                .soul_weapons
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
                    .metrics
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
                    self.history
                        .world_rank
                        .is_some_and(|rank| rank <= req && rank > 0) as i64,
                    1,
                ))
            }
            "LinkAccount" => return Some((self.history.linked_account as i64, 1)),
            _ => return None,
        };
        Some((value.max(0), req.max(1)))
    }
}

/// Called only inside authenticated worker settlement, behind the existing
/// run completion/retry guard. Client-supplied kill counts are never accepted.
pub(crate) async fn worker_defeats(
    db: &mut SqliteConnection,
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
