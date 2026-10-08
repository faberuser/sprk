//! Entry recovery shared by polling, login, battle costs and item refills.
use crate::{
    api::inventory::item::{n, rule},
    error::Result,
};
use chrono::{Datelike, Duration, NaiveDate, Utc};
use serde_json::{json, Value};

pub(crate) fn period(date: NaiveDate, days: i64) -> NaiveDate {
    if days == 7 {
        date - Duration::days(date.weekday().num_days_from_monday() as i64)
    } else {
        date
    }
}

pub(crate) fn reset_days(kind: i64) -> i64 {
    if matches!(kind, 21 | 29 | 30) {
        7
    } else {
        1
    }
}

pub(crate) fn at(
    info: &Value,
    def: &Value,
    overrides: &Value,
    initial: i64,
    now: chrono::DateTime<Utc>,
    cost: i64,
) -> Result<(Value, Value)> {
    let name = def["AttributeName"]
        .as_str()
        .ok_or_else(|| rule("InvalidStaminaType"))?;
    let cap = def["MaxCountValue"]
        .as_str()
        .and_then(|v| v.parse::<i64>().ok())
        .unwrap_or(initial)
        .max(0);
    let initial = overrides["InitialCount"].as_i64().unwrap_or(initial).max(0);
    let old = if info.is_null() {
        initial
    } else {
        n(info, "Count").max(0)
    };
    let mode = n(def, "UpdateType");
    let timestamp = now.timestamp();
    let mut anchor = n(info, "RechargeTime");
    let mut saved = if info.is_null() {
        json!({})
    } else {
        info.clone()
    };
    let (available, next, full_interval, batch) = if mode == 1 {
        let interval = n(overrides, "IntervalSeconds");
        if interval <= 0 {
            return Err(rule("InvalidStaminaType"));
        }
        if anchor <= 0 || anchor > timestamp {
            anchor = timestamp;
        }
        let added = ((timestamp - anchor) / interval).min((cap - old).max(0));
        let available = old + added;
        anchor = if available >= cap {
            timestamp
        } else {
            anchor + added * interval
        };
        (available, interval - (timestamp - anchor), interval, 1)
    } else if mode == 2 {
        let days = overrides["PeriodDays"].as_i64().unwrap_or(1);
        if !matches!(days, 1 | 7) {
            return Err(rule("InvalidStaminaType"));
        }
        let today = period(now.date_naive(), days);
        let last = info["Period"]
            .as_str()
            .or_else(|| info["Day"].as_str())
            .and_then(|v| NaiveDate::parse_from_str(v, "%Y-%m-%d").ok())
            .map(|v| period(v, days))
            .unwrap_or(today);
        let elapsed = (today - last).num_days().max(0) / days;
        let counts = def["ResetCount"]
            .as_array()
            .ok_or_else(|| rule("InvalidStaminaType"))?;
        let grant = overrides["GrantCount"]
            .as_i64()
            .unwrap_or_else(|| {
                let index = if counts.len() == 7 {
                    now.weekday().num_days_from_sunday() as usize
                } else {
                    0
                };
                counts.get(index).and_then(Value::as_i64).unwrap_or(0)
            })
            .max(0);
        let accumulate = overrides["Accumulate"]
            .as_bool()
            .unwrap_or(matches!(n(def, "MaxCountType"), 4 | 5));
        let available = if elapsed == 0 {
            old
        } else if accumulate {
            old.saturating_add(elapsed.saturating_mul(grant).min((cap - old).max(0)))
        } else {
            // Do not discard purchased overflow on a scheduled refill.
            grant + (old - cap).max(0)
        };
        let reset = last.max(today);
        anchor = reset.and_hms_opt(0, 0, 0).unwrap().and_utc().timestamp();
        saved["Period"] = json!(reset.to_string());
        saved["Day"] = json!(reset.to_string());
        (
            available,
            anchor + days * 86400 - timestamp,
            days * 86400,
            grant,
        )
    } else {
        // These balances are awarded by their content, not a clock.
        (old, 0, 0, 0)
    };
    if cost < 0 || cost > available {
        return Err(rule("NotEnoughDungeonKey"));
    }
    let value = available - cost;
    let next = if mode == 1 && value >= cap { 0 } else { next };
    let full = if value >= cap || batch == 0 {
        0
    } else {
        next + ((cap - value + batch - 1) / batch - 1) * full_interval
    };
    saved["Count"] = json!(value);
    saved["RechargeTime"] = json!(anchor);
    if mode != 2 {
        saved["Day"] = json!(now.date_naive().to_string());
    }
    let result = json!({"Type":name,"AddValue":available-old-cost,"NewValue":value,
        "StaminaRechargeTime":chrono::DateTime::from_timestamp(anchor.max(0),0).unwrap().format("%Y-%m-%d %H:%M:%S").to_string(),
        "NextRechargeRemainTime":next,"FullRechargeRemainTime":full,
        "RechargeCount":n(&saved,"RechargeCount"),"IsHide":false});
    Ok((saved, result))
}

#[cfg(test)]
mod tests {
    use super::*;
    fn weekly(info: &Value, stamp: &str, cost: i64) -> (Value, Value) {
        at(
            info,
            &json!({"AttributeName":"TechnoEnchantKey","UpdateType":2,"MaxCountType":5,
            "MaxCountValue":"20","ResetCount":[5]}),
            &json!({"PeriodDays":7,"GrantCount":5,"Accumulate":true}),
            5,
            stamp.parse().unwrap(),
            cost,
        )
        .unwrap()
    }
    #[test]
    fn arcdim_preserves_legacy_balance_and_recovers_only_on_monday() {
        let (saved, result) = weekly(
            &json!({"Day":"2026-10-06","Count":4}),
            "2026-10-06T12:00:00Z",
            0,
        );
        assert_eq!(result["NewValue"], 4);
        assert_eq!(result["NextRechargeRemainTime"], 5 * 86400 + 12 * 3600);
        assert_eq!(
            result["FullRechargeRemainTime"],
            5 * 86400 + 12 * 3600 + 3 * 7 * 86400
        );
        assert_eq!(weekly(&saved, "2026-10-07T12:00:00Z", 0).1["NewValue"], 4);
        let (saved, result) = weekly(&saved, "2026-10-12T00:00:00Z", 1);
        assert_eq!(result["NewValue"], 8);
        assert_eq!(result["AddValue"], 4);
        assert_eq!(weekly(&saved, "2026-10-12T00:00:00Z", 0).1["AddValue"], 0);
        assert_eq!(weekly(&saved, "2026-10-26T00:00:00Z", 0).1["NewValue"], 18);
    }
    #[test]
    fn scheduled_recovery_caps_and_preserves_purchased_overflow_and_future_anchors() {
        for (count, expected) in [(19, 20), (20, 20), (25, 25)] {
            let (saved, result) = weekly(
                &json!({"Count":count,"Day":"2026-09-01"}),
                "2026-10-06T00:00:00Z",
                0,
            );
            assert_eq!(result["NewValue"], expected);
            assert_eq!(result["FullRechargeRemainTime"], 0);
            assert!(n(&result, "NextRechargeRemainTime") > 0);
            assert_eq!(
                weekly(&saved, "2026-10-05T00:00:00Z", 0).1["NewValue"],
                expected
            );
        }
        let old = json!({"Day":"2026-10-19","Count":0});
        let (saved, _) = weekly(&old, "2026-10-06T00:00:00Z", 0);
        assert_eq!(weekly(&saved, "2026-10-19T00:00:00Z", 0).1["NewValue"], 0);
    }
    #[test]
    fn weekly_trials_refill_without_banking_unused_weeks() {
        for (kind, grant) in [("GodkingTrialKey", 2), ("PunishmentRaidKey", 1)] {
            let def = json!({"AttributeName":kind,"UpdateType":2,"MaxCountType":6,"MaxCountValue":grant.to_string(),"ResetCount":[1]});
            let policy = json!({"PeriodDays":7,"GrantCount":grant,"Accumulate":false});
            let (saved, result) = at(
                &json!({"Day":"2026-09-01","Count":0}),
                &def,
                &policy,
                grant,
                "2026-10-06T00:00:00Z".parse().unwrap(),
                grant,
            )
            .unwrap();
            assert_eq!(result["NewValue"], 0);
            assert_eq!(
                at(
                    &saved,
                    &def,
                    &policy,
                    grant,
                    "2026-10-07T00:00:00Z".parse().unwrap(),
                    0
                )
                .unwrap()
                .1["NewValue"],
                0
            );
            assert_eq!(
                at(
                    &saved,
                    &def,
                    &policy,
                    grant,
                    "2026-10-12T00:00:00Z".parse().unwrap(),
                    0
                )
                .unwrap()
                .1["NewValue"],
                grant
            );
        }
    }
    #[test]
    fn arena_regeneration_preserves_fractional_time_and_restarts_after_full() {
        let def = json!({"AttributeName":"Sword","UpdateType":1,"MaxCountValue":"10"});
        let policy = json!({"IntervalSeconds":1800});
        let stamp: chrono::DateTime<Utc> = "2026-10-06T00:00:00Z".parse().unwrap();
        let (saved, result) = at(
            &json!({"Count":0,"RechargeTime":stamp.timestamp()-4500}),
            &def,
            &policy,
            10,
            stamp,
            1,
        )
        .unwrap();
        assert_eq!(result["NewValue"], 1);
        assert_eq!(result["NextRechargeRemainTime"], 900);
        assert_eq!(result["FullRechargeRemainTime"], 900 + 8 * 1800);
        assert!(at(&saved, &def, &policy, 10, stamp, 2).is_err());
        let (saved, result) = at(&saved, &def, &policy, 10, stamp + Duration::days(2), 1).unwrap();
        assert_eq!(result["NewValue"], 9);
        assert_eq!(result["NextRechargeRemainTime"], 1800);
        assert_eq!(
            at(&saved, &def, &policy, 10, stamp + Duration::days(2), 0)
                .unwrap()
                .1["AddValue"],
            0
        );
    }
    #[test]
    fn content_awarded_keys_do_not_gain_clock_based_entries() {
        let def = json!({"AttributeName":"EclipseKey","UpdateType":0,"MaxCountValue":"5"});
        let info = json!({"Day":"2026-01-01","Count":1});
        let (_, result) = at(
            &info,
            &def,
            &Value::Null,
            5,
            "2026-10-06T00:00:00Z".parse().unwrap(),
            1,
        )
        .unwrap();
        assert_eq!(result["NewValue"], 0);
        assert_eq!(result["NextRechargeRemainTime"], 0);
    }
}
