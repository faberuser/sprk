//! Persistent native attendance, achievements, and quest rewards.
use crate::api::{
    heroes as hero,
    inventory::item::{self, n, rule},
    system::request::Request,
    tutorial::Rewards,
};
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::SqliteConnection;
use std::collections::BTreeSet;
mod attendance;
pub mod notifications;
pub(crate) mod schema;
mod state;
#[cfg(test)]
mod tests;
use state::{period, View};

macro_rules! endpoints {($($name:ident),*)=>{$(pub async fn $name(State(state):State<AppState>,body:Bytes)->Result<Json<Value>>{handle(state,body,stringify!($name)).await})*};}
endpoints!(
    get_achievements,
    check_achievements,
    reward_achievement,
    get_attendance_reward,
    get_all_conditional_attendance_reward,
    get_logindaily_reward,
    get_clear_mission_reward,
    get_newbie_mission_reward,
    progress_client_main_quest,
    complete_sub_quest,
    reward_clear_chapter,
    receive_completed_reward,
    attendance_info
);
pub(super) async fn claim(
    db: &mut SqliteConnection,
    account: i64,
    family: &str,
    id: i64,
    step: i64,
    p: &str,
) -> Result<()> {
    let changed=sqlx::query("INSERT OR IGNORE INTO progression_claims(account_id,family,idx,step,period) VALUES(?,?,?,?,?)").bind(account).bind(family).bind(id).bind(step).bind(p).execute(db).await?.rows_affected();
    if changed != 1 {
        return Err(rule("AlreadyReceived"));
    }
    Ok(())
}
pub(crate) async fn record(
    db: &mut SqliteConnection,
    account: i64,
    kind: &str,
    arg: i64,
    arg2: i64,
    value: i64,
) -> Result<()> {
    if value <= 0 {
        return Ok(());
    }
    sqlx::query("INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES(?,date('now'),?,?,?,?) ON CONFLICT(account_id,day,kind,arg,arg2) DO UPDATE SET value=value+excluded.value").bind(account).bind(kind).bind(arg).bind(arg2).bind(value).execute(db).await?;
    Ok(())
}
pub(crate) async fn login(state: &AppState, account: i64) -> Result<Value> {
    let mut tx = state.db.begin().await?;
    item::init(&mut tx, state, account).await?;
    touch_day(&mut tx, account).await?;
    let out = snapshot(&mut tx, state, account).await?;
    tx.commit().await?;
    Ok(out)
}
async fn touch_day(db: &mut SqliteConnection, account: i64) -> Result<()> {
    let changed=sqlx::query("INSERT INTO progression_login(account_id,days,last_day) VALUES(?,1,date('now')) ON CONFLICT(account_id) DO UPDATE SET days=days+1,last_day=excluded.last_day WHERE last_day!=excluded.last_day").bind(account).execute(&mut *db).await?.rows_affected();
    if changed > 0 {
        record(db, account, "LoginDaily", 0, 0, 1).await?;
    }
    Ok(())
}
pub(crate) async fn snapshot(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
) -> Result<Value> {
    let view = View::load(db, account).await?;
    let mut chapter = std::collections::BTreeMap::<i64, i64>::new();
    for c in view.claims.iter().filter(|c| c.0 == "chapter") {
        *chapter.entry(c.1).or_default() |= 1 << c.2;
    }
    let mut out = json!({"AchievementInfos":view.achievement_infos(state),"SubQuestInfos":view.subquest_infos(state),"MainQuestInfo":{"Step":0,"Progress":0},"LoginDailyCount":view.login_days,
        "LoginDailyInfos":view.claims.iter().filter(|c|c.0=="login").map(|c|json!({"LoginDailyIndex":c.1,"CompletedCount":1,"CompletedTime":c.4})).collect::<Vec<_>>(),
        "NewbieMissionInfos":view.claims.iter().filter(|c|c.0=="newbie").map(|c|json!({"MissionIndex":c.1,"UpdateTime":c.4})).collect::<Vec<_>>(),
        "ChapterRewardInfos":chapter.into_iter().map(|(id,mask)|json!({"ChapterIndex":id,"LastRewardDiff":mask})).collect::<Vec<_>>(),
        "ClearMissionInfos":state.tables.progression.clear_missions.iter().map(|r|json!({"MissionIndex":n(r,"Index"),"LastStep":view.last("clear",n(r,"Index"),"all"),"Progress":view.quest(state,r),"RewardedTime":view.claims.iter().find(|c|c.0=="clear"&&c.1==n(r,"Index")).map(|c|c.4.clone())})).collect::<Vec<_>>()});
    out["OpendMissionCategories"] = json!(state.tables.progression.mission_categories.iter().filter(|r| {
        n(r, "OpenConditionType") == 0 ||
        (r["OpenConditionName"] == "ClearQuestIndex" && r["OpenConditionArgs"][0] == "SubQuest"
         && r["OpenConditionArgs"][1].as_str().and_then(|s| s.parse::<i64>().ok())
             .is_some_and(|id| view.last("subquest", id, "all") > 0))
    }).map(|r| json!({"MainCategoryIndex":r["MainCategoryIndex"],"SubCategoryIndex":r["SubCategoryIndex"]})).collect::<Vec<_>>());
    let main: Option<(i64, i64)> =
        sqlx::query_as("SELECT step,progress FROM progression_main_quest WHERE account_id=?")
            .bind(account)
            .fetch_optional(&mut *db)
            .await?;
    if let Some((step, progress)) = main {
        out["MainQuestInfo"] = json!({"Step":step,"Progress":progress});
    }
    out["AttendanceDatas"] = json!(attendance::calendars(state, &view));
    let products: Vec<(i64, String)> = sqlx::query_as(
        "SELECT product_index,created_time FROM progression_entitlements WHERE account_id=?",
    )
    .bind(account)
    .fetch_all(&mut *db)
    .await?;
    out["PlayerProductPurchaseInfos"] = json!(products.iter().map(|(id,time)| json!({"ProductIndex":id,"PurchasedCount":1,"PurchasedTime":time,"StartTime":time})).collect::<Vec<_>>());
    out["AttendanceInfos"] = json!(attendance::infos(db, state, account, &view).await?);
    let events: Vec<String> =
        sqlx::query_scalar("SELECT data FROM progression_world_events WHERE account_id=?")
            .bind(account)
            .fetch_all(db)
            .await?;
    out["WorldMapEventInfos"] = json!(events
        .iter()
        .filter_map(|s| serde_json::from_str::<Value>(s).ok())
        .collect::<Vec<_>>());
    Ok(out)
}
pub(super) async fn handle(state: AppState, body: Bytes, action: &str) -> Result<Json<Value>> {
    let req = Request::parse_with_arrays(&body, &["AchievementIndices", "Steps", "SubQuestIndices"])?;
    let account = req.account(&state)?;
    let mut tx = state.db.begin().await?;
    item::init(&mut tx, &state, account).await?;
    touch_day(&mut tx, account).await?;
    match execute(&mut tx, &state, account, &req, action).await {
        Ok(mut out) => {
            let snapshot = snapshot(&mut tx, &state, account).await?;
            for (wire, field) in [
                ("AchievementInfos", "AchievementInfos"),
                ("ReservedSubQuestInfos", "SubQuestInfos"),
                ("ReservedMainQuestInfo", "MainQuestInfo"),
                ("ReservedClearMissionInfos", "ClearMissionInfos"),
            ] {
                out[wire] = snapshot[field].clone();
            }
            tx.commit().await?;
            Ok(Json(out))
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            Ok(Json(
                json!({"BaseResult":"Success","Result":state.tables.hero_shop.result(action,&code)}),
            ))
        }
        Err(e) => Err(e),
    }
}
pub(super) async fn db_reward(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    r: &Value,
    rewards: &mut Rewards,
) -> Result<()> {
    let kind = r["Type"].as_str().unwrap_or("");
    let id = n(r, "Value");
    let count = n(r, "Count");
    if kind == "None" || r["Type"] == 0 {
        return Ok(());
    }
    if kind == "Item" || r["Type"] == 1 {
        item::give(
            db,
            state,
            account,
            i32::try_from(id).map_err(|_| rule("InvalidRewardData"))?,
            i32::try_from(count).map_err(|_| rule("InvalidRewardData"))?,
            n(r, "Star") as i32,
            0,
            rewards,
        )
        .await?;
    } else {
        let kind = match (kind, n(r, "Type")) {
            ("Gold", _) | (_, 2) => "Gold",
            ("Gem", _) | (_, 3) => "Gem",
            ("PvpCoin", _) | (_, 4) => "PvpCoin",
            ("Mileage", _) | (_, 6) => "Mileage",
            ("RaidPoint", _) | (_, 12) => "RaidPoint",
            ("FriendshipPoint", _) | (_, 14) => "FriendshipPoint",
            _ => return Err(rule("InvalidRewardData")),
        };
        if id <= 0 {
            return Err(rule("InvalidRewardData"));
        }
        rewards
            .currencies
            .push(hero::currency(db, account, kind, id).await?);
    }
    Ok(())
}
async fn achievement_reward(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    row: &Value,
    rewards: &mut Rewards,
    booster: &mut Option<Value>,
) -> Result<Vec<Value>> {
    let level: i64 = sqlx::query_scalar("SELECT team_level FROM user_info WHERE account_id=?")
        .bind(account)
        .fetch_one(&mut *db)
        .await?;
    let mut stamina = vec![];
    for prefix in ["Reward", "Reward2"] {
        let amount = n(row, &format!("{prefix}Value"));
        let kind = row[format!("{prefix}Kind")].as_str().unwrap_or("");
        if kind == "None" {
            continue;
        }
        if amount <= 0 && !matches!(kind, "GoldTeamLevel" | "StaminaTeamLevel") {
            return Err(rule("InvalidRewardData"));
        }
        match kind {
            "Booster" => {
                let id = state
                    .tables
                    .get_item_index(row[format!("{prefix}Code")].as_str().unwrap_or(""))
                    .ok_or_else(|| rule("InvalidBooster"))?;
                if booster
                    .as_ref()
                    .is_some_and(|b| n(b, "ItemIndex") != id as i64)
                {
                    return Err(rule("InvalidBooster"));
                }
                *booster = Some(
                    item::activate_booster(
                        db,
                        state,
                        account,
                        id,
                        i32::try_from(amount).map_err(|_| rule("InvalidBooster"))?,
                    )
                    .await?,
                );
            }
            "TeamExp" => rewards.team_exp_to_add += amount,
            "Item" => {
                let code = row[format!("{prefix}Code")].as_str().unwrap_or("");
                let id = state
                    .tables
                    .get_item_index(code)
                    .ok_or_else(|| rule("InvalidRewardData"))?;
                item::give(
                    db,
                    state,
                    account,
                    id,
                    i32::try_from(amount).map_err(|_| rule("InvalidRewardData"))?,
                    0,
                    0,
                    rewards,
                )
                .await?;
            }
            "GoldTeamLevel" => {
                let data = state
                    .tables
                    .inventory
                    .team_levels
                    .iter()
                    .find(|r| n(r, "Level") == level)
                    .ok_or_else(|| rule("InvalidRewardData"))?;
                rewards
                    .currencies
                    .push(hero::currency(db, account, "Gold", n(data, "DailyGold")).await?);
            }
            "Stamina" | "StaminaTeamLevel" => {
                let count = if kind == "StaminaTeamLevel" {
                    let data = state
                        .tables
                        .inventory
                        .team_levels
                        .iter()
                        .find(|r| n(r, "Level") == level)
                        .ok_or_else(|| rule("InvalidRewardData"))?;
                    n(data, "DailyBonusStamina")
                } else {
                    amount
                };
                let value:Option<i64>=sqlx::query_scalar("UPDATE user_info SET stamina=stamina+? WHERE account_id=? AND stamina+?<=2147483647 RETURNING stamina").bind(count).bind(account).bind(count).fetch_optional(&mut *db).await?;
                stamina.push(json!({"Type":"Chicken","AddValue":count,"NewValue":value.ok_or_else(||rule("InvalidRewardData"))?,"StaminaRechargeTime":null,"NextRechargeRemainTime":0,"FullRechargeRemainTime":0,"RechargeCount":0,"IsHide":false}));
            }
            "Gold" | "Gem" | "PvpCoin" | "FriendshipPoint" | "RaidPoint" => rewards
                .currencies
                .push(hero::currency(db, account, kind, amount).await?),
            _ => return Err(rule("InvalidRewardData")),
        }
    }
    Ok(stamina)
}
fn arrays(req: &Request, key: &str) -> Result<Vec<i64>> {
    let raw: Vec<Value> = serde_json::from_str(req.text(key)).map_err(|_| rule("InvalidStep"))?;
    let v: Vec<i64> = raw.iter().map(|v| v.as_i64()
        .or_else(|| v.as_str().and_then(|s| s.parse().ok()))
        .ok_or_else(|| rule("InvalidStep"))).collect::<Result<_>>()?;
    if v.is_empty() || v.len() > 100 {
        return Err(rule("InvalidStep"));
    }
    Ok(v)
}
async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if action.contains("attendance") || action == "get_logindaily_reward" {
        return attendance::execute(db, state, account, req, action).await;
    }
    let table = &state.tables.progression;
    let view = View::load(db, account).await?;
    let mut out = item::success();
    match action {
        "get_achievements" | "check_achievements" => {}
        "reward_achievement" => {
            let ids = arrays(req, "AchievementIndices")?;
            let steps = arrays(req, "Steps")?;
            if ids.len() != steps.len() || ids.iter().collect::<BTreeSet<_>>().len() != ids.len() {
                return Err(rule("InvalidStep"));
            }
            let mut rewards = Rewards::default();
            let mut stamina = vec![];
            let mut booster = None;
            for (id, last_step) in ids.into_iter().zip(steps) {
                // Native GetStepArray sends AchievementInfo.LastStep, not the
                // next reward step. Match it against saved state before advancing.
                let step = last_step.checked_add(1).filter(|_| last_step >= 0)
                    .ok_or_else(|| rule("InvalidStep"))?;
                let row = table
                    .achievements
                    .iter()
                    .find(|r| n(r, "Index") == id && n(r, "Step") == step)
                    .ok_or_else(|| rule("InvalidAchievementIndex"))?;
                let p = period(row);
                if view.last("achievement", id, &p) != last_step {
                    return Err(rule("InvalidStep"));
                }
                if matches!(n(row, "Type"), 3 | 7 | 8 | 9) || !view.condition(&row["OpenCondition"])
                {
                    return Err(rule("RewardTimeEnded"));
                }
                let (value, target) = view.achievement(state, row);
                if value < target {
                    return Err(rule("AchievementNotCleared"));
                }
                claim(db, account, "achievement", id, step, &p).await?;
                stamina.extend(
                    achievement_reward(db, state, account, row, &mut rewards, &mut booster).await?,
                );
            }
            if !rewards.heroes.is_empty() {
                return Err(rule("InvalidRewardData"));
            }
            let r = item::reward_response(db, state, account, rewards).await?;
            out["ItemTimeDurationInfo"] = json!(booster);
            out["CurrencyResults"] = r["CurrencyResults"].clone();
            out["itemResults"] = r["ItemResults"].clone();
            out["equipItemInfos"] = r["EquipItemResults"].clone();
            out["staminaResults"] = json!(stamina);
            out["expResult"] = r["ExpResultInfos"].get(0).cloned().unwrap_or(Value::Null);
            out["FriendshipPointResults"] = json!(r["CurrencyResults"]
                .as_array()
                .unwrap()
                .iter()
                .filter(|c| c["CurrencyType"] == "FriendshipPoint")
                .collect::<Vec<_>>());
        }
        "complete_sub_quest" => {
            let ids = arrays(req, "SubQuestIndices")?;
            if ids.iter().any(|id| *id <= 0) || ids.iter().collect::<BTreeSet<_>>().len() != ids.len() {
                return Err(rule("Fail"));
            }
            let mut infos = vec![];
            let mut results = vec![];
            for id in ids {
                let step = view.last("subquest", id, "all") + 1;
                let r = table
                    .sub_quests
                    .iter()
                    .find(|r| n(r, "QuestIndex") == id && n(r, "Step") == step)
                    .ok_or_else(|| rule("Fail"))?;
                if r["ClientOnly"] == true || view.quest(state, r) < n(r, "ReqProgress").max(1) {
                    return Err(rule("Fail"));
                }
                claim(db, account, "subquest", id, step, "all").await?;
                let mut rewards = Rewards::default();
                if n(r, "RewardIndex") > 0 {
                    item::reward(db, state, account, n(r, "RewardIndex") as i32, &mut rewards)
                        .await?;
                }
                results.push(item::reward_response(db, state, account, rewards).await?);
                infos.push(json!({"SubQuestIndex":id,"LastStep":step,"Progress":0}));
            }
            out["SucceededSubQuestInfos"] = json!(infos);
            out["RewardResults"] = json!(results);
        }
        "progress_client_main_quest" => {
            let current: Option<(i64, i64)> = sqlx::query_as(
                "SELECT step,progress FROM progression_main_quest WHERE account_id=?",
            )
            .bind(account)
            .fetch_optional(&mut *db)
            .await?;
            let (step, old) = current.unwrap_or((1, 0));
            let r = table
                .main_quests
                .iter()
                .find(|r| n(r, "Step") == step && n(r, "QuestIndex") > 0)
                .ok_or_else(|| rule("QuestDataNotFound"))?;
            if r["ClientOnly"] != true {
                return Err(rule("NotClientOnly"));
            }
            let progress = (old + 1).min(n(r, "ReqProgress").max(1));
            sqlx::query("INSERT INTO progression_main_quest(account_id,step,progress) VALUES(?,?,?) ON CONFLICT(account_id) DO UPDATE SET step=excluded.step,progress=excluded.progress").bind(account).bind(step).bind(progress).execute(&mut *db).await?;
            out["Progress"] = json!(progress);
        }
        "get_clear_mission_reward" | "get_newbie_mission_reward" => {
            let id = req.number("MissionIndex", 0)?;
            let clear = action == "get_clear_mission_reward";
            let family = if clear { "clear" } else { "newbie" };
            let row = if clear {
                &table.clear_missions
            } else {
                &table.newbie_missions
            }
            .iter()
            .find(|r| n(r, "Index") == id)
            .ok_or_else(|| rule("InvalidClearMissionIndex"))?;
            if view.last(family, id, "all") > 0 {
                return Err(rule("InvalidClearMissionStep"));
            }
            let entitled = if clear {
                table.clear_products.iter().any(|p| {
                    n(p, "ProductGroupIndex") == n(row, "ProductGroupIndex")
                        && (n(p, "PurchaseType") != 2
                            || view.entitlements.contains(&n(p, "PayShopProductIndex")))
                        && view.condition(&p["MissionOpenCondition"])
                })
            } else {
                let c = row["Condition"].as_array();
                if c.is_some_and(|c| {
                    c.len() == 3 && c[0] == "NewbieBuyItemBonusCount" && c[2] == "OneDollarShop"
                }) {
                    let needed = c.unwrap()[1]
                        .as_str()
                        .and_then(|s| s.parse::<usize>().ok())
                        .unwrap_or(usize::MAX);
                    table
                        .newbie_products
                        .iter()
                        .filter(|p| {
                            n(p, "PurchaseType") == 2
                                && n(p, "RewardType") == 6
                                && n(p, "ProductGroupIndex") == n(row, "ProductGroupIndex")
                                && view.entitlements.contains(&n(p, "PayShopProductIndex"))
                        })
                        .count()
                        >= needed
                } else {
                    view.condition(&row["Condition"])
                }
            };
            if !entitled {
                return Err(rule("CheckConditionNotPassed"));
            }
            if clear && view.quest(state, row) < n(row, "ReqProgress").max(1) {
                return Err(rule("ClearMissionNotCleared"));
            }
            claim(db, account, family, id, 1, "all").await?;
            let mut rewards = Rewards::default();
            item::reward(
                db,
                state,
                account,
                n(row, "RewardIndex") as i32,
                &mut rewards,
            )
            .await?;
            let r = item::reward_response(db, state, account, rewards).await?;
            for key in ["CurrencyResults", "ItemResults", "EquipItemResults"] {
                out[key] = r[key].clone();
            }
            out[if clear {
                "ClearMissionResult"
            } else {
                "NewbieMissionResult"
            }] = if clear {
                json!({"MissionIndex":id,"LastStep":1,"Progress":view.quest(state,row),"RewardedTime":state.server_time_str()})
            } else {
                json!({"MissionIndex":id,"UpdateTime":state.server_time_str()})
            };
        }
        "reward_clear_chapter" => {
            let id = req.number("ChapterIndex", 0)?;
            let step = req.number("Step", -1)?;
            let row = table
                .chapter_rewards
                .iter()
                .find(|r| n(r, "ChapterIndex") == id && n(r, "StepIndex") == step)
                .ok_or_else(|| rule("ChapterClearRewardDataNotFound"))?;
            if !(0..=7).contains(&step)
                || view
                    .claims
                    .iter()
                    .any(|c| c.0 == "chapter" && c.1 == id && c.2 == step)
            {
                return Err(rule("WrongStep"));
            }
            if view.stars(state, id) < n(row, "RewardReqStar") {
                return Err(rule("NotEnoughStar"));
            }
            claim(db, account, "chapter", id, step, "all").await?;
            let mut rewards = Rewards::default();
            let value = if n(row, "RewardType") == 1 {
                state
                    .tables
                    .get_item_index(row["RewardItemCode"].as_str().unwrap_or(""))
                    .ok_or_else(|| rule("InvalidRewardData"))? as i64
            } else {
                n(row, "RewardCount")
            };
            db_reward(
                db,
                state,
                account,
                &json!({"Type":row["RewardType"],"Value":value,"Count":row["RewardCount"]}),
                &mut rewards,
            )
            .await?;
            let r = item::reward_response(db, state, account, rewards).await?;
            if r["HeroInfos"].as_array().unwrap().len() > 1
                || r["ItemResults"].as_array().unwrap().len() > 1
                || r["EquipItemResults"].as_array().unwrap().len() > 1
            {
                return Err(rule("InvalidRewardData"));
            }
            out["CurrencyResults"] = r["CurrencyResults"].clone();
            out["ItemResult"] = r["ItemResults"].get(0).cloned().unwrap_or(Value::Null);
            out["EquipItemResult"] = r["EquipItemResults"].get(0).cloned().unwrap_or(Value::Null);
            out["HeroAddResult"] = r["HeroInfos"]
                .get(0)
                .map(|h| json!({"HeroInfo":h,"TeamExpResult":r["ExpResultInfos"].get(0)}))
                .unwrap_or(Value::Null);
            let mask = view
                .claims
                .iter()
                .filter(|c| c.0 == "chapter" && c.1 == id)
                .fold(1 << step, |m, c| m | (1 << c.2));
            out["RewardInfo"] = json!({"ChapterIndex":id,"LastRewardDiff":mask});
        }
        "receive_completed_reward" => return world_reward(db, state, account, req).await,
        _ => return Err(rule("Fail")),
    }
    Ok(out)
}

async fn world_reward(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let chapter = req.number("ChapterIndex", 0)?;
    let dungeons = arrays(req, "DungeonIndices")?;
    let events = arrays(req, "EventIndices")?;
    if !state
        .tables
        .progression
        .chapters
        .iter()
        .any(|r| n(r, "Index") == chapter || n(r, "ChapterIndex") == chapter)
    {
        return Err(rule("ChapterNotFound"));
    }
    if dungeons.len() != events.len() {
        return Err(rule("WorldMapEventNotFound"));
    }
    let mut seen = BTreeSet::new();
    let mut removed = vec![];
    let mut rewards = Rewards::default();
    for (dungeon, event) in dungeons.into_iter().zip(events) {
        if !seen.insert((dungeon, event)) {
            return Err(rule("WorldMapEventNotFound"));
        }
        if state
            .tables
            .get_campaign_dungeon(chapter as i32, dungeon as i32)
            .is_none()
        {
            return Err(rule("DungeonNotFound"));
        }
        let data:Option<String>=sqlx::query_scalar("SELECT data FROM progression_world_events WHERE account_id=? AND chapter=? AND dungeon=? AND event=?").bind(account).bind(chapter).bind(dungeon).bind(event).fetch_optional(&mut *db).await?;
        let data: Value = serde_json::from_str(&data.ok_or_else(|| rule("WorldMapEventNotFound"))?)
            .map_err(|_| rule("WorldMapEventNotFound"))?;
        if data["Completed"] != true {
            return Err(rule("NotCompleted"));
        }
        if n(&data, "Duration") != 0
            && data["NoExpire"] != true
            && data["NoExpireOnComplete"] != true
        {
            let time = chrono::NaiveDateTime::parse_from_str(
                data["OccuredTime"].as_str().unwrap_or(""),
                "%Y-%m-%d %H:%M:%S",
            )
            .map_err(|_| rule("WorldMapEventNotFound"))?
            .and_utc()
            .timestamp();
            if time + n(&data, "Duration") <= state.server_time() {
                return Err(rule("WorldMapEventNotFound"));
            }
        }
        for kind in ["Gold", "Gem"] {
            let amount = n(&data, &format!("Reward{kind}"));
            if amount > 0 {
                rewards
                    .currencies
                    .push(hero::currency(db, account, kind, amount).await?);
            }
        }
        if let Some(encoded) = data["RewardItems"].as_str().filter(|s| !s.is_empty()) {
            let entries: Vec<Value> =
                serde_json::from_str(encoded).map_err(|_| rule("InvalidRewardData"))?;
            for r in entries {
                let id = state
                    .tables
                    .get_item_index(r["Code"].as_str().unwrap_or(""))
                    .ok_or_else(|| rule("InvalidRewardData"))?;
                db_reward(
                    db,
                    state,
                    account,
                    &json!({"Type":"Item","Value":id,"Count":r["Count"],"Star":r["Star"]}),
                    &mut rewards,
                )
                .await?;
            }
        }
        // Persisted normalized reward definitions are server-owned, never supplied by a claim request.
        for r in data["Rewards"].as_array().into_iter().flatten() {
            db_reward(db, state, account, r, &mut rewards).await?;
        }
        sqlx::query("DELETE FROM progression_world_events WHERE account_id=? AND chapter=? AND dungeon=? AND event=?").bind(account).bind(chapter).bind(dungeon).bind(event).execute(&mut *db).await?;
        removed.push(data);
    }
    let r = item::reward_response(db, state, account, rewards).await?;
    if !r["HeroInfos"].as_array().unwrap().is_empty() {
        return Err(rule("InvalidRewardData"));
    }
    Ok(
        json!({"BaseResult":"Success","Result":"Success","CurrencyResults":r["CurrencyResults"],"ItemResults":r["ItemResults"],"EquipItemInfos":r["EquipItemResults"],"ExpResult":r["ExpResultInfos"].get(0),"WorldMapEventInfos":[],"RemovedWorldMapEventInfos":removed,"WorldMapEventTimeInfos":[]}),
    )
}
