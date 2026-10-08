//! Repositorium uses a weekly battle-time gauge, not a wall-clock recharge.
use super::*;
use chrono::{Datelike, Duration, NaiveDate, Utc};

const CURRENCY: &str = "TreasureHousePoint";

fn week(date: NaiveDate) -> NaiveDate {
    date - Duration::days(date.weekday().num_days_from_monday() as i64)
}

fn choose(s: &AppState) -> Result<i64> {
    let choices = s.tables.battle.rows("TreasureHouseDungeon");
    let total = choices.iter().map(|v| n(v, "Ratio").max(0)).sum::<i64>();
    if total <= 0 {
        return Err(rule("DungeonNotFound"));
    }
    let mut roll = (rand::random::<u64>() % total as u64) as i64;
    choices
        .iter()
        .find(|v| {
            roll -= n(v, "Ratio").max(0);
            roll < 0
        })
        .map(|v| n(v, "Index"))
        .ok_or_else(|| rule("DungeonNotFound"))
}

pub(super) fn max(s: &AppState) -> Result<i64> {
    s.tables
        .battle
        .find("CurrencyType", &[("CurrencyType", 54)])
        .map(|v| n(v, "MaxValue"))
        .filter(|v| *v > 0)
        .ok_or_else(|| rule("DungeonNotFound"))
}

pub(super) async fn info(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    info_at(db, s, a, Utc::now().date_naive()).await
}

async fn info_at(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    date: NaiveDate,
) -> Result<Value> {
    let mut p = get(db, a, "treasure", 0).await?;
    let period = week(date);
    // Adopt the old daily record without discarding this week's monster or clear.
    let previous = p["Period"]
        .as_str()
        .or_else(|| p["Day"].as_str())
        .or_else(|| p["ResetTime"].as_str().and_then(|v| v.get(..10)))
        .and_then(|v| NaiveDate::parse_from_str(v, "%Y-%m-%d").ok())
        .map(week);
    let reset = !p.is_null() && previous.is_some_and(|v| v < period);
    if p.is_null() || reset {
        if reset {
            sqlx::query("UPDATE battle_currencies SET value=0 WHERE account=? AND kind=?")
                .bind(a)
                .bind(CURRENCY)
                .execute(&mut *db)
                .await?;
        }
        p = json!({"Index":choose(s)?,"ClearCount":0,"ClearedTime":null,
            "UpdatedTime":time(now()),"CreatedTime":time(now()),"BattleTimeRemainderMs":0});
    }
    let period = previous.filter(|v| *v > period).unwrap_or(period);
    p["Period"] = json!(period.to_string());
    p["ResetTime"] = json!(format!("{period} 00:00:00"));
    p["NextResetTime"] = json!(format!("{} 00:00:00", period + Duration::days(7)));
    put(db, a, "treasure", 0, &p).await?;
    Ok(p)
}

pub(super) async fn snapshot(db: &mut SqliteConnection, a: i64) -> Result<Value> {
    hero::currency(db, a, CURRENCY, 0).await
}

fn sync(out: &mut Value, result: Value) {
    let mut values = out["ReservedCurrencyResults"]
        .as_array()
        .cloned()
        .unwrap_or_default();
    values.push(result);
    out["ReservedCurrencyResults"] = json!(values);
}

pub(super) async fn validate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    d: &Value,
) -> Result<()> {
    let p = info(db, s, a).await?;
    let target = row(s, "TreasureHouseDungeon", &[("Index", n(&p, "Index"))])?;
    let def = row(s, "TreasureHouseInfo", &[("Index", n(target, "InfoIndex"))])?;
    if n(target, "DungeonIndex") != n(d, "DungeonIndex")
        || n(&p, "ClearCount") >= n(def, "EnterCount")
        || n(&snapshot(db, a).await?, "NewValue") < max(s)?
    {
        return Err(rule("TreasurehouseInitializeEnterPopup"));
    }
    Ok(())
}

pub(super) async fn clear(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    out: &mut Value,
) -> Result<()> {
    let mut p = info(db, s, a).await?;
    p["ClearCount"] = json!(n(&p, "ClearCount") + 1);
    p["ClearedTime"] = json!(time(now()));
    p["BattleTimeRemainderMs"] = json!(0);
    put(db, a, "treasure", 0, &p).await?;
    let balance = n(&snapshot(db, a).await?, "NewValue");
    sync(out, hero::currency(db, a, CURRENCY, -balance).await?);
    Ok(())
}

pub(super) async fn reroll(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let mut p = info(db, s, a).await?;
    p["Index"] = json!(choose(s)?);
    p["UpdatedTime"] = json!(time(now()));
    put(db, a, "treasure", 0, &p).await?;
    Ok(p)
}

pub(super) async fn record_time(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    started: i64,
    battle_type: i64,
    out: &mut Value,
) -> Result<()> {
    let mut p = info(db, s, a).await?;
    // Native ElapsedBattleTime is actual combat milliseconds; PureBattleTime is
    // simulation seconds. Prefer combat time so pauses/loading do not fill the gauge.
    let reported = if r.0.contains_key("ElapsedBattleTime") {
        r.number("ElapsedBattleTime", 0)?
    } else {
        r.number("PureBattleTime", 0)?
            .checked_mul(1000)
            .ok_or_else(|| rule("ModulatedData"))?
    };
    if reported < 0 {
        return Err(rule("ModulatedData"));
    }
    let period = NaiveDate::parse_from_str(p["Period"].as_str().unwrap_or(""), "%Y-%m-%d")
        .map_err(|_| rule("Fail"))?
        .and_hms_opt(0, 0, 0)
        .unwrap()
        .and_utc()
        .timestamp();
    let limit = (now() - started.max(period)).max(0).saturating_mul(1000);
    let ms = reported.min(limit);
    let before = snapshot(db, a).await?;
    let cap = max(s)?;
    if battle_type != 27 && n(&p, "ClearCount") == 0 && n(&before, "NewValue") < cap {
        let total = ms + n(&p, "BattleTimeRemainderMs").clamp(0, 999);
        let added = (total / 1000).min(cap - n(&before, "NewValue"));
        let result = hero::currency(db, a, CURRENCY, added).await?;
        p["BattleTimeRemainderMs"] = json!(if n(&result, "NewValue") >= cap {
            0
        } else {
            total % 1000
        });
        put(db, a, "treasure", 0, &p).await?;
        sync(out, result);
    } else {
        sync(out, before);
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn weekly_reset_preserves_daily_progress_and_migrates_old_records() {
        let (s, u) = super::super::tests::setup().await;
        let a = super::super::tests::account(&u);
        let mut db = s.db.acquire().await.unwrap();
        put(
            &mut db,
            a,
            "treasure",
            0,
            &json!({"Index":1001010,"ClearCount":1,
            "Day":"2026-10-06","CreatedTime":"2026-10-06 00:00:00"}),
        )
        .await
        .unwrap();
        hero::currency(&mut db, a, CURRENCY, 150).await.unwrap();
        let sunday = info_at(
            &mut db,
            &s,
            a,
            NaiveDate::from_ymd_opt(2026, 10, 11).unwrap(),
        )
        .await
        .unwrap();
        assert_eq!(sunday["Index"], 1001010);
        assert_eq!(sunday["ClearCount"], 1);
        assert_eq!(sunday["NextResetTime"], "2026-10-12 00:00:00");
        assert_eq!(snapshot(&mut db, a).await.unwrap()["NewValue"], 150);
        let monday = info_at(
            &mut db,
            &s,
            a,
            NaiveDate::from_ymd_opt(2026, 10, 12).unwrap(),
        )
        .await
        .unwrap();
        assert_eq!(monday["ClearCount"], 0);
        assert_eq!(monday["NextResetTime"], "2026-10-19 00:00:00");
        assert_eq!(snapshot(&mut db, a).await.unwrap()["NewValue"], 0);
    }
}
