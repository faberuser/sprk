use super::*;
use chrono::{Datelike, Duration, Utc};
use std::collections::{BTreeMap, BTreeSet};

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
pub(super) struct View {
    pub heroes: Vec<Value>,
    pub dungeons: Vec<(i64, i64, i64, i64)>,
    pub metrics: Vec<(String, String, i64, i64, i64)>,
    pub claims: Vec<(String, i64, i64, String, String)>,
    pub login_days: i64,
    pub level: i64,
    pub costumes: i64,
    pub friends: i64,
    pub guild: bool,
    pub tutorials: BTreeSet<i64>,
    pub items: BTreeMap<i64, i64>,
    pub entitlements: BTreeSet<i64>,
}
impl View {
    pub async fn load(db: &mut SqliteConnection, account: i64) -> Result<Self> {
        let heroes = hero::snapshot(db, account)
            .await?
            .into_iter()
            .map(|h| serde_json::to_value(h).unwrap())
            .collect();
        Ok(Self {
            heroes,
            dungeons:sqlx::query_as("SELECT chapter_id,dungeon_id,clear_count,best_star FROM campaign_progress WHERE account_id=?").bind(account).fetch_all(&mut *db).await?,
            metrics:sqlx::query_as("SELECT day,kind,arg,arg2,value FROM progression_metrics WHERE account_id=?").bind(account).fetch_all(&mut *db).await?,
            claims:sqlx::query_as("SELECT family,idx,step,period,claimed_time FROM progression_claims WHERE account_id=?").bind(account).fetch_all(&mut *db).await?,
            login_days:sqlx::query_scalar("SELECT days FROM progression_login WHERE account_id=?").bind(account).fetch_optional(&mut *db).await?.unwrap_or(0),
            level:sqlx::query_scalar("SELECT team_level FROM user_info WHERE account_id=?").bind(account).fetch_one(&mut *db).await?,
            costumes:sqlx::query_scalar("SELECT COUNT(*) FROM costumes WHERE account_id=?").bind(account).fetch_one(&mut *db).await?,
            friends:sqlx::query_scalar("SELECT COUNT(*) FROM friends WHERE (account_id=? OR friend_account_id=?) AND status='accepted'").bind(account).bind(account).fetch_one(&mut *db).await?,
            guild:sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM guild_members WHERE account_id=?)").bind(account).fetch_one(&mut *db).await?,
            tutorials:sqlx::query_scalar::<_,i64>("SELECT tutorial_index FROM tutorial_progress WHERE account_id=? AND completed_time IS NOT NULL").bind(account).fetch_all(&mut *db).await?.into_iter().collect(),
            items:sqlx::query_as::<_,(i64,i64)>("SELECT item_index,count FROM items WHERE account_id=?").bind(account).fetch_all(&mut *db).await?.into_iter().collect(),
            entitlements:sqlx::query_scalar::<_,i64>("SELECT product_index FROM progression_entitlements WHERE account_id=?").bind(account).fetch_all(&mut *db).await?.into_iter().collect(),
        })
    }
    pub fn last(&self, family: &str, id: i64, p: &str) -> i64 {
        self.claims
            .iter()
            .filter(|c| c.0 == family && c.1 == id && c.3 == p)
            .map(|c| c.2)
            .max()
            .unwrap_or(0)
    }
    pub fn metric(&self, kind: &str, p: &str, a: Option<i64>, b: Option<i64>) -> i64 {
        self.metrics
            .iter()
            .filter(|v| {
                (p == "all" || v.0.as_str() >= p)
                    && v.1 == kind
                    && a.is_none_or(|a| a == v.2)
                    && b.is_none_or(|b| b == v.3)
            })
            .map(|v| v.4)
            .sum()
    }
    pub fn stars(&self, state: &AppState, chapter: i64) -> i64 {
        self.dungeons
            .iter()
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
            "CheckTeamLevel" | "TeamLevel" => self.level >= num(1),
            "LoginDailyCount" => self.login_days >= num(1),
            "SubQuestComplete" => self.last("subquest", num(1), "all") > 0,
            // Missing event/paid-service conditions never grant eligibility implicitly.
            _ => false,
        }
    }
    pub fn quest(&self, state: &AppState, row: &Value) -> i64 {
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
            |key: &str, min: i64| self.heroes.iter().filter(|h| n(h, key) >= min).count() as i64;
        match row["Kind"].as_str().unwrap_or("") {
            "DungeonCleared" => {
                self.dungeons
                    .iter()
                    .any(|d| d.0 == arg(0) && d.1 == arg(1) && d.2 > 0) as i64
            }
            "CompleteTutorial" => self.tutorials.contains(&arg(0)) as i64,
            "ChapterStarCount" => self.stars(state, arg(0)),
            "CheckTeamLevel" => (self.level >= arg(0)) as i64,
            "HeroCountOfLevel" => count_hero("Level", arg(0)),
            "HeroCountOfStar" => count_hero("Star", arg(0)),
            "HeroCountOfTranscended" => count_hero("Transcended", arg(0)),
            "HeroCountOfIndex" => self
                .heroes
                .iter()
                .filter(|h| n(h, "HeroIndex") == arg(0))
                .count() as i64,
            "SkillExtendCount" => self
                .heroes
                .iter()
                .map(|h| {
                    (1..=5)
                        .filter(|i| n(h, &format!("SkillExtend{i}")) >= arg(0))
                        .count() as i64
                })
                .sum(),
            "CostumeCount" => self.costumes,
            "LoginDaily" => self.login_days,
            "JoinGuild" => self.guild as i64,
            "FriendRegistration" => self.friends,
            "WearEquip" => self
                .heroes
                .iter()
                .map(|h| {
                    (1..=10)
                        .filter(|i| n(h, &format!("EquipItemSlotIndex{i}")) > 0)
                        .count() as i64
                })
                .sum(),
            "MissionClear" | "StepMissionClear" => {
                let Some(category) = state
                    .tables
                    .progression
                    .mission_categories
                    .iter()
                    .find(|c| n(c, "RewardQuestIndex") == n(row, "QuestIndex"))
                else {
                    return 0;
                };
                let members: Vec<&Value> = state
                    .tables
                    .progression
                    .mission_visuals
                    .iter()
                    .filter(|v| {
                        n(v, "MainCategoryIndex") == n(category, "MainCategoryIndex")
                            && n(v, "SubCategoryIndex") == n(category, "SubCategoryIndex")
                            && n(v, "QuestType") == 1
                    })
                    .collect();
                let done = |v: &Value| {
                    self.last("subquest", n(v, "QuestIndex"), "all")
                        >= state
                            .tables
                            .progression
                            .sub_quests
                            .iter()
                            .filter(|q| n(q, "QuestIndex") == n(v, "QuestIndex"))
                            .map(|q| n(q, "Step"))
                            .max()
                            .unwrap_or(i64::MAX)
                };
                if row["Kind"] == "MissionClear" {
                    members.iter().filter(|v| done(v)).count() as i64
                } else {
                    let steps: BTreeSet<i64> = members.iter().map(|v| n(v, "MainStep")).collect();
                    steps
                        .iter()
                        .filter(|s| {
                            members
                                .iter()
                                .filter(|v| n(v, "MainStep") == **s)
                                .all(|v| done(v))
                        })
                        .count() as i64
                }
            }
            "AmountOfConsumeStamina" => self.metric("ConsumeStamina", "all", None, None),
            "ConsumeStamina" => self.metric("StaminaUses", "all", None, None),
            "ClearDungeon" => {
                self.metric("ClearDungeon", "all", a.first().copied(), a.get(1).copied())
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
            _ => 0,
        }
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
            "TotalHero" => self.heroes.len() as i64,
            "TotalHeroStar" => self.heroes.iter().map(|h| n(h, "Star")).sum(),
            "HeroIndex" => self
                .heroes
                .iter()
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
                self.items.get(&req).copied().unwrap_or(0)
            }
            "AddFriend" => self.friends,
            "GuildJoin" => self.guild as i64,
            "ClearDungeon" => self.metric("ClearDungeon", &p, None, None),
            "BuyShopItem" => self.metric(
                "BuyShopItem",
                &p,
                if sub(19) > 0 { Some(sub(19)) } else { None },
                None,
            ),
            "ConsumeGold" | "ConsumeGem" => self.metric(kind, &p, None, None),
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
                .claims
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
            // Definitions for unimplemented battle/event/archive services remain incomplete.
            _ => 0,
        };
        (value.max(0), target)
    }
    pub fn achievement_infos(&self, state: &AppState) -> Vec<Value> {
        let mut grouped = BTreeMap::<i64, Vec<&Value>>::new();
        for row in &state.tables.progression.achievements {
            grouped.entry(n(row, "Index")).or_default().push(row);
        }
        let mut infos = vec![];
        for (id, rows) in grouped {
            let row = rows[0];
            let p = period(row);
            let last = self.last("achievement", id, &p);
            let current = rows
                .iter()
                .copied()
                .find(|v| n(v, "Step") == last + 1)
                .unwrap_or(row);
            let (value, _) = self.achievement(state, current);
            infos.push(json!({"AchievementIndex":id,"LastStep":last,"Achievement":value,"ClearTeamLevel":self.level,"UpdatedTime":format!("{} 00:00:00",Utc::now().date_naive())}));
        }
        infos
    }
    pub fn subquest_infos(&self, state: &AppState) -> Vec<Value> {
        let mut grouped = BTreeMap::<i64, Vec<&Value>>::new();
        for row in &state.tables.progression.sub_quests {
            grouped.entry(n(row, "QuestIndex")).or_default().push(row);
        }
        let mut infos = vec![];
        for (id, rows) in grouped {
            let last = self.last("subquest", id, "all");
            let row = rows
                .iter()
                .copied()
                .find(|v| n(v, "Step") == last + 1)
                .unwrap_or(rows[0]);
            infos.push(json!({"SubQuestIndex":id,"LastStep":last,"Progress":self.quest(state,row).min(n(row,"ReqProgress").max(1))}));
        }
        infos
    }
}
