//! Victory participation unlocks daily mail through the end of that season.
use super::*;
use std::collections::BTreeMap;

pub(super) async fn due(db: &mut SqliteConnection, s: &AppState, account: i64, at: i64) -> Result<Vec<(String, i64)>> {
    let snapshots: Vec<(i64, String)> = sqlx::query_as("SELECT idx,data FROM community_state WHERE owner=? AND kind='arena_daily' AND idx<? ORDER BY idx")
        .bind(account).bind(at.div_euclid(86400)).fetch_all(&mut *db).await?;
    let participating: BTreeSet<i64> = sqlx::query_scalar("SELECT season FROM arena_scores WHERE account=? AND kind=0 AND wins+losses>0")
        .bind(account).fetch_all(&mut *db).await?.into_iter().collect();
    let claimed: BTreeSet<String> = sqlx::query_scalar("SELECT period FROM community_claims WHERE account=? AND kind='arena_daily_reward'")
        .bind(account).fetch_all(&mut *db).await?.into_iter().collect();
    let mut days = BTreeMap::new();
    for (day, raw) in snapshots {
        let snapshot: Value = parse(&raw)?;
        let rank = n(&snapshot, "Rank");
        let (season, start, end) = season_at(s, day * 86400);
        // Preserve earned legacy snapshots, but only extend a participating
        // Victory season. Never carry participation into the next season.
        let last = if participating.contains(&season) && day * 86400 >= start {
            (end - 1).div_euclid(86400).min(at.div_euclid(86400) - 1)
        } else { day };
        // Later played-day snapshots overwrite carried ranks. On skipped days
        // use the last recorded rank, as no historical leaderboard exists.
        for reward_day in day..=last {
            days.insert(time(reward_day * 86400)[..10].to_owned(), rank);
        }
    }
    Ok(days.into_iter().filter(|(day, _)| !claimed.contains(day)).collect())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{setup, login};

    #[tokio::test]
    async fn victory_daily_rewards_continue_after_losses_and_skipped_days_but_stop_at_season_end() {
        let s = setup().await;
        let u = login(&s, "victory-daily-gaps").await;
        let a = n(&u["UserInfo"], "AccountId");
        let monday = chrono::NaiveDate::from_ymd_opt(2026, 11, 2).unwrap().and_hms_opt(0,0,0).unwrap().and_utc().timestamp();
        let season = season_at(&s, monday).0;
        let mut db = s.db.acquire().await.unwrap();
        sqlx::query("INSERT INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,0,?,1000,0,1) ON CONFLICT(account,kind,season) DO UPDATE SET wins=0,losses=1")
            .bind(a).bind(season).execute(&mut *db).await.unwrap();
        for (offset, rank) in [(1, 4), (3, 2)] {
            put(&mut db, a, "arena_daily", monday / 86400 + offset, &json!({"Day":time(monday+offset*86400)[..10],"Rank":rank})).await.unwrap();
        }
        // A first match today grants nothing for previous days.
        assert!(due(&mut db,&s,a,monday+86400).await.unwrap().is_empty());
        assert_eq!(due(&mut db,&s,a,monday+4*86400).await.unwrap(), vec![
            ("2026-11-03".into(),4), ("2026-11-04".into(),4), ("2026-11-05".into(),2)]);
        // No new-season reward until a match in that season; old entitlements
        // remain deliverable even after a player misses the entire weekend.
        let rewards = due(&mut db,&s,a,monday+10*86400).await.unwrap();
        assert_eq!(rewards.len(),6);
        assert_eq!(rewards.last().unwrap(), &("2026-11-08".into(),2));
        for (day, rank) in rewards {
            rank_mail(&mut db,&s,a,rank,"arena_daily_reward",&day).await.unwrap();
        }
        assert!(due(&mut db,&s,a,monday+10*86400).await.unwrap().is_empty());
        let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=? AND title='Arena daily reward'").bind(a).fetch_one(&mut *db).await.unwrap();
        assert_eq!(count,6);
    }

    #[tokio::test]
    async fn victory_daily_rewards_do_not_unlock_from_honor_or_an_unplayed_score() {
        let s=setup().await;
        let u=login(&s,"victory-daily-unplayed").await;
        let a=n(&u["UserInfo"],"AccountId");
        let mut db=s.db.acquire().await.unwrap();
        sqlx::query("INSERT INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,1,?,1000,1,0)")
            .bind(a).bind(season(&s).0).execute(&mut *db).await.unwrap();
        sqlx::query("INSERT OR IGNORE INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,0,?,1000,0,0)")
            .bind(a).bind(season(&s).0).execute(&mut *db).await.unwrap();
        assert!(due(&mut db,&s,a,now()).await.unwrap().is_empty());
    }

    #[tokio::test]
    async fn victory_daily_rewards_deliver_skipped_days_on_mail_poll_once() {
        let s=setup().await;
        let u=login(&s,"victory-daily-mail-poll").await;
        let a=n(&u["UserInfo"],"AccountId");
        let (current,start,end)=season(&s);
        let length=end-start;
        let first=start-length;
        {
            let mut db=s.db.acquire().await.unwrap();
            sqlx::query("INSERT INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,0,?,1000,0,1)")
                .bind(a).bind(current-1).execute(&mut *db).await.unwrap();
            put(&mut db,a,"arena_daily",first/86400,&json!({"Day":time(first)[..10],"Rank":1})).await.unwrap();
        }
        let body=Bytes::from(format!("SessionKey={}&TopmostMailIndex=0",u["UserInfo"]["SessionKey"].as_str().unwrap()));
        for _ in 0..2 {
            let reply=crate::api::community::mail::check_new_mail(State(s.clone()),body.clone()).await.unwrap().0;
            assert_eq!(reply["NewMailArrived"],true);
        }
        let count: i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=? AND title='Arena daily reward'")
            .bind(a).fetch_one(&s.db).await.unwrap();
        assert_eq!(count,length/86400);
        // Logging in again must not duplicate mail generated by polling.
        login(&s,"victory-daily-mail-poll").await;
        let after: i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=? AND title='Arena daily reward'")
            .bind(a).fetch_one(&s.db).await.unwrap();
        assert_eq!(after,count);
    }
}
