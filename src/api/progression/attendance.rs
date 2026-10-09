use super::*;

pub(super) fn calendars(state: &AppState, view: &View) -> Vec<Value> {
    let now = state.server_time_str();
    state
        .tables
        .progression
        .calendars
        .iter()
        .filter(|c| {
            c["Enabled"] != false
                && n(c, "OpenLevel") <= view.level
                && c["StartDate"].as_str().is_none_or(|s| s <= now.as_str())
                && c["EndDate"].as_str().is_none_or(|s| s > now.as_str())
                && view.condition(&c["Conditions"])
        })
        .cloned()
        .collect()
}
pub(super) async fn infos(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    view: &View,
) -> Result<Vec<Value>> {
    let rows:Vec<(i64,i64,String,Option<String>)>=sqlx::query_as("SELECT idx,claims,last_day,completed_time FROM attendance_calendar_state WHERE account_id=?").bind(account).fetch_all(db).await?;
    let mut out = vec![];
    for c in calendars(state, view) {
        let id = n(&c, "Index");
        let max = c["Reward"].as_array().unwrap().len() as i64;
        let row = rows.iter().find(|r| r.0 == id);
        let count = row.map(|r| r.1).unwrap_or(0);
        let today = row.is_some_and(|r| r.2 == state.server_date());
        let finished = n(&c, "Repeatable") == 0 && count >= max;
        let last = if count == 0 {
            0
        } else if today || finished {
            (count - 1) % max + 1
        } else {
            count % max
        };
        out.push(json!({"AttendanceIndex":id,"CompletedDay":if today||finished {last}else{last+1},"LastRewardedDay":last,"CompletedTime":row.and_then(|r|r.3.clone()),"EndTime":c["EndDate"],"SuspendEndTime":null}));
    }
    Ok(out)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let view = View::load(db, account).await?;
    let mut out = item::success();
    if action == "attendance_info" {
        out["AttendanceInfos"] = json!(infos(db, state, account, &view).await?);
        out["AttendanceDatas"] = json!(calendars(state, &view));
        return Ok(out);
    }
    let mut rewards = Rewards::default();
    if action == "get_logindaily_reward" {
        let id = req.number("LoginDailyIndex", 0)?;
        let r = state
            .tables
            .progression
            .login_rewards
            .iter()
            .find(|r| n(r, "Index") == id)
            .ok_or_else(|| rule("AccumulateLoginRewardTableaNotFound"))?;
        if view.last("login", id, "all") > 0 {
            return Err(rule("AlreadyReceived"));
        }
        if view.login_days < n(r, "Day") {
            return Err(rule("IsNotCondition"));
        }
        claim(db, account, "login", id, 1, "all").await?;
        for i in 1..=3 {
            db_reward(db,state,account,&json!({"Type":r[format!("Type{i}")],"Value":r[format!("Value{i}")],"Count":r[format!("Count{i}")]}),&mut rewards).await?;
        }
        out["LoginDailyRewardResult"] = item::reward_response(db, state, account, rewards).await?;
        out["LoginDailyInfo"] = json!({"LoginDailyIndex":id,"CompletedCount":1,"CompletedTime":state.server_time_str()});
    } else {
        let ids = if action == "get_all_conditional_attendance_reward" {
            req.ids("AttendanceIndex")?
        } else {
            vec![req.number("AttendanceIndex", 0)?]
        };
        if ids.is_empty() {
            return Err(rule("Fail"));
        }
        let calendars = calendars(state, &view);
        for id in ids {
            let c = calendars
                .iter()
                .find(|c| n(c, "Index") == id)
                .ok_or_else(|| rule("Fail"))?;
            let count:Option<(i64,String)>=sqlx::query_as("SELECT claims,last_day FROM attendance_calendar_state WHERE account_id=? AND idx=?").bind(account).bind(id).fetch_optional(&mut *db).await?;
            let (count, day) = count.unwrap_or((0, String::new()));
            let days = c["Reward"].as_array().ok_or_else(|| rule("Fail"))?;
            if day == state.server_date() || (n(c, "Repeatable") == 0 && count >= days.len() as i64)
            {
                return Err(rule("AlreadyReceived"));
            }
            let reward = &days[count as usize % days.len()];
            let entries = reward["Reward"]
                .as_array()
                .ok_or_else(|| rule("InvalidRewardData"))?;
            if entries.is_empty() {
                return Err(rule("InvalidRewardData"));
            }
            // Direct delivery is used for local calendars; mail-only calendars need a delivery integration.
            if n(c, "SendMail") != 0 || entries.iter().any(|r| r["SendMail"] == true) {
                return Err(rule("InvalidRewardData"));
            }
            for r in entries {
                db_reward(db, state, account, r, &mut rewards).await?;
            }
            sqlx::query("INSERT INTO attendance_calendar_state(account_id,idx,claims,last_day,completed_time) VALUES(?,?,?,date('now'),datetime('now')) ON CONFLICT(account_id,idx) DO UPDATE SET claims=excluded.claims,last_day=excluded.last_day,completed_time=excluded.completed_time").bind(account).bind(id).bind(count+1).execute(&mut *db).await?;
        }
        out["AttendanceRewardResult"] = item::reward_response(db, state, account, rewards).await?;
        out["AttendanceInfos"] = json!(infos(db, state, account, &view).await?);
        out["AttendanceDatas"] = json!(calendars);
    }
    Ok(out)
}
