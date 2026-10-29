//! Mission predicates over durable, account-scoped gameplay history. Keep
//! participation, victories, current ownership and lifetime discoveries distinct.
use super::*;
use std::collections::{BTreeMap, BTreeSet};

#[derive(Default)]
pub(super) struct History {
    pub arena: Vec<(i64, i64, Value)>,
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

impl History {
    pub async fn load(db: &mut SqliteConnection, account: i64) -> Result<Self> {
        fn parse(raw: String) -> Result<Value> {
            serde_json::from_str(&raw)
                .map_err(|e| ServerError::Internal(format!("Invalid mission history: {e}")))
        }
        let mut out = Self::default();
        for (kind, at, raw) in sqlx::query_as::<_, (i64,i64,String)>(
            "SELECT kind,started,json_object('CompletedAt',json_extract(data,'$.CompletedAt'),'End',json_object('MatchResult',json_extract(data,'$.End.MatchResult'),'BattleInfo',json_object('SuccessiveWin',json_extract(data,'$.End.BattleInfo.SuccessiveWin')),'BattleBanPickInfo',json_object('SuccessiveWin',json_extract(data,'$.End.BattleBanPickInfo.SuccessiveWin')))) FROM arena_runs WHERE account=? AND status='complete' ORDER BY id")
            .bind(account).fetch_all(&mut *db).await? {
            let data = parse(raw)?;
            if data["End"]["MatchResult"]["Canceled"] != true && data["End"]["MatchResult"].is_object() {
                out.arena.push((kind, n(&data,"CompletedAt").max(at), data));
            }
        }
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

    fn arena_count(&self, kind: i64, period: &str, win: bool) -> i64 {
        self.arena
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

    fn arena_best(&self, kind: i64, field: &str) -> i64 {
        let key = if kind == 1 {
            "BattleBanPickInfo"
        } else {
            "BattleInfo"
        };
        self.arena
            .iter()
            .filter(|(k, _, _)| *k == kind)
            .map(|(_, _, d)| n(&d["End"][key], field))
            .max()
            .unwrap_or(0)
    }

    fn best_tier(&self, kind: i64) -> i64 {
        self.arena
            .iter()
            .filter(|(k, _, _)| *k == kind)
            .map(|(_, _, d)| n(&d["End"]["MatchResult"], "NewTierIndex"))
            .max()
            .unwrap_or(0)
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
            "ClearTower" => c == ["System", "CanGoDungeon", "2", "27"],
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
