//! Archived Honor rewards. Weekly snapshots remain immutable after Monday UTC;
//! seasonal payouts read the closed Honor score partition, never Victory scores.
use super::*;

const WEEK: i64 = 7 * 86400;
const MONDAY: i64 = 4 * 86400;

pub(super) fn week(at: i64) -> i64 { (at - MONDAY).div_euclid(WEEK) }

pub(super) async fn record(db: &mut SqliteConnection, account: i64, info: &Value, at: i64) -> Result<()> {
    put(db, account, "honor_week", week(at), &json!({
        "Score":info["MatchScore"], "Wins":info["SeasonWin"], "Losses":info["SeasonLose"], "At":at
    })).await
}

fn reward(s: &AppState, rank: i64) -> Result<&Value> {
    s.tables.arena_guild.rows("HonorTierReward").iter()
        .find(|r| rank >= n(r,"MinRank") && rank <= n(r,"MaxRank"))
        .ok_or_else(|| rule("ItemDataNotFound"))
}

async fn send(db: &mut SqliteConnection, s: &AppState, account: i64, period: i64, rank: i64, weekly: bool) -> Result<()> {
    let def = reward(s, rank)?;
    let bundle = n(def, if weekly {"WeeklyBoxReward"} else {"ShowRewardIndex"});
    let mut items = s.tables.arena_guild.find("HonorRewardBundle", &[("Index",bundle)])
        .and_then(|r| r["Items"].as_array()).cloned().ok_or_else(|| rule("ItemDataNotFound"))?;
    if !weekly {
        if let Some(booster) = s.tables.arena_guild.find("HonorSeasonBooster", &[("Rank",rank)]) {
            items.push(json!({"ItemIndex":booster["ItemIndex"],"ItemCount":1}));
        }
    }
    // Validate before recording the claim. Missing catalog data must roll back
    // and retry, never silently consume a player's entitlement.
    for item in &items {
        if n(item,"ItemCount")<=0 || !s.tables.inventory.items.contains_key(&(n(item,"ItemIndex") as i32)) {
            return Err(rule("ItemDataNotFound"));
        }
    }
    let kind = if weekly {"honor_weekly_reward"} else {"honor_season_reward"};
    let inserted = sqlx::query("INSERT OR IGNORE INTO community_claims(account,kind,target,period) VALUES(?,?,0,?)")
        .bind(account).bind(kind).bind(period.to_string()).execute(&mut *db).await?.rows_affected();
    if inserted == 0 { return Ok(()); }
    let currencies = json!([{"CurrencyType":"RankingPoint","Amount":n(def,if weekly {"WeeklyReward"} else {"SeasonReward"})}]);
    let title = if weekly {"League of Honor weekly reward"} else {"League of Honor season reward"};
    let content = format!("Period {period}, final rank {rank}, tier {}",n(def,"TierIndex"));
    sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_items,reward_currencies,expires_at) VALUES(?,'League of Honor',?,?,?,?,?)")
        .bind(account).bind(title).bind(content).bind(json!(items).to_string()).bind(currencies.to_string())
        .bind(time(now()+settings(s,"HonorRewardMailDays",7).clamp(1,365)*86400)).execute(db).await?;
    Ok(())
}

pub(crate) async fn settle(db: &mut SqliteConnection, s: &AppState, account: i64) -> Result<()> {
    let pending: bool = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM community_state r WHERE owner=? AND kind='honor_week' AND idx<? AND NOT EXISTS(SELECT 1 FROM community_claims c WHERE c.account=r.owner AND c.kind='honor_weekly_reward' AND c.period=CAST(r.idx AS TEXT))) OR EXISTS(SELECT 1 FROM arena_scores r WHERE account=? AND kind=1 AND season<? AND wins+losses>0 AND NOT EXISTS(SELECT 1 FROM community_claims c WHERE c.account=r.account AND c.kind='honor_season_reward' AND c.period=CAST(r.season AS TEXT)))")
        .bind(account).bind(week(now())).bind(account).bind(season(s).0).fetch_one(&mut *db).await?;
    if !pending {return Ok(());}
    // ROW_NUMBER freezes deterministic ties across all eligible participants;
    // there is no leaderboard pagination limit on reward settlement.
    let weeks: Vec<(i64,i64)> = sqlx::query_as(
        "WITH ranks AS (SELECT idx,owner,ROW_NUMBER() OVER(PARTITION BY idx ORDER BY CAST(json_extract(data,'$.Score') AS INTEGER) DESC,CAST(json_extract(data,'$.Wins') AS INTEGER) DESC,owner) AS rank FROM community_state WHERE kind='honor_week' AND idx<?) SELECT idx,rank FROM ranks r WHERE owner=? AND NOT EXISTS(SELECT 1 FROM community_claims c WHERE c.account=r.owner AND c.kind='honor_weekly_reward' AND c.period=CAST(r.idx AS TEXT)) ORDER BY idx LIMIT 100")
        .bind(week(now())).bind(account).fetch_all(&mut *db).await?;
    for (period,rank) in weeks { send(db,s,account,period,rank,true).await?; }
    let seasons: Vec<(i64,i64)> = sqlx::query_as(
        "WITH ranks AS (SELECT season,account,ROW_NUMBER() OVER(PARTITION BY season ORDER BY score DESC,wins DESC,account) AS rank FROM arena_scores WHERE kind=1 AND season<? AND wins+losses>0) SELECT season,rank FROM ranks r WHERE account=? AND NOT EXISTS(SELECT 1 FROM community_claims c WHERE c.account=r.account AND c.kind='honor_season_reward' AND c.period=CAST(r.season AS TEXT)) ORDER BY season LIMIT 100")
        .bind(season(s).0).bind(account).fetch_all(&mut *db).await?;
    for (period,rank) in seasons { send(db,s,account,period,rank,false).await?; }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{setup,login,call};

    fn form(user: &Value, args: &str) -> Bytes {
        Bytes::from(format!("SessionKey={}&{args}",user["UserInfo"]["SessionKey"].as_str().unwrap()))
    }

    #[test]
    fn honor_weekly_boundary_is_monday_utc() {
        assert_eq!(week(MONDAY-1),-1);
        assert_eq!(week(MONDAY),0);
        assert_eq!(week(MONDAY+WEEK-1),0);
        assert_eq!(week(MONDAY+WEEK),1);
    }

    #[tokio::test]
    async fn honor_historical_rewards_cover_every_tier_and_chests_can_be_claimed_and_opened() {
        let mut s=setup().await;let u=login(&s,"honor-archive-catalog").await;let a=n(&u["UserInfo"],"AccountId");
        assert_eq!(s.tables.arena_guild.rows("HonorTierReward").len(),43);
        let mut tx=s.db.begin().await.unwrap();
        for (i,tier) in s.tables.arena_guild.rows("HonorTierReward").iter().enumerate() {
            let rank=n(tier,"MinRank");assert_eq!(reward(&s,rank).unwrap(),tier);
            assert_eq!(reward(&s,n(tier,"MaxRank")).unwrap(),tier);
            send(&mut tx,&s,a,i as i64,rank,true).await.unwrap();
            send(&mut tx,&s,a,i as i64,rank,false).await.unwrap();
            send(&mut tx,&s,a,i as i64,rank,true).await.unwrap();
        }
        tx.commit().await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE account_id=? AND sender='League of Honor'").bind(a).fetch_one(&s.db).await.unwrap(),86);
        let (mail,items,currency):(i64,String,String)=sqlx::query_as("SELECT mail_id,reward_items,reward_currencies FROM mails WHERE account_id=? AND title='League of Honor weekly reward' ORDER BY mail_id LIMIT 1").bind(a).fetch_one(&s.db).await.unwrap();
        assert_eq!(parse::<Value>(&items).unwrap()[0]["ItemIndex"],91256);
        assert_eq!(parse::<Value>(&items).unwrap()[0]["ItemCount"],7);
        assert_eq!(parse::<Value>(&currency).unwrap(),json!([{"CurrencyType":"RankingPoint","Amount":3}]));
        let claimed=crate::api::community::mail::receive_mail(State(s.clone()),form(&u,&format!("MailIndex={mail}"))).await.unwrap().0;
        assert_eq!(claimed["Result"],"Success","weekly mail: {claimed}");
        // Force the formerly broken currency branch without relying on RNG.
        let original=s.tables.clone();let mut tables=s.tables.as_ref().clone();
        let group=std::sync::Arc::make_mut(&mut tables.item_groups).entries.get_mut("PAKAGE_HONOR_DAILY_BOX_GROUP_1").unwrap();
        group.sub_data_list.retain(|v|v.item_code=="Gold");group.total_rate=group.sub_data_list[0].rate;
        s.tables=std::sync::Arc::new(tables);
        let opened=crate::api::inventory::item::use_package_item(State(s.clone()),form(&u,"ItemIndex=91256&ItemCount=7")).await.unwrap().0;
        assert_eq!(opened["Result"],"Success","weekly chest: {opened}");
        assert_eq!(opened["RewardResults"].as_array().unwrap().len(),7);
        assert!(opened["RewardResults"].as_array().unwrap().iter().all(|r|r["CurrencyResults"].as_array().unwrap().iter().any(|c|c["CurrencyType"]=="Gold" && c["AddValue"]==250000)));
        s.tables=original;
        let mut boxes=std::collections::BTreeSet::new();
        for tier in s.tables.arena_guild.rows("HonorTierReward") {
            let b=s.tables.arena_guild.find("HonorRewardBundle",&[("Index",n(tier,"WeeklyBoxReward"))]).unwrap();
            boxes.insert(n(&b["Items"][0],"ItemIndex"));
        }
        for index in boxes {
            sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,?,20) ON CONFLICT(account_id,item_index) DO UPDATE SET count=count+20")
                .bind(a).bind(index).execute(&s.db).await.unwrap();
            let opened=crate::api::inventory::item::use_package_item(State(s.clone()),form(&u,&format!("ItemIndex={index}&ItemCount=20"))).await.unwrap().0;
            assert_eq!(opened["Result"],"Success","archived chest {index}: {opened}");
        }
        let repeated=crate::api::community::mail::receive_mail(State(s.clone()),form(&u,&format!("MailIndex={mail}"))).await.unwrap().0;
        assert_ne!(repeated["Result"],"Success");
        let (season_mail,items,currency):(i64,String,String)=sqlx::query_as("SELECT mail_id,reward_items,reward_currencies FROM mails WHERE account_id=? AND title='League of Honor season reward' ORDER BY mail_id DESC LIMIT 1").bind(a).fetch_one(&s.db).await.unwrap();
        let items=parse::<Value>(&items).unwrap();
        assert!(items.as_array().unwrap().iter().any(|r|r["ItemIndex"]==4064 && r["ItemCount"]==1));
        assert!(items.as_array().unwrap().iter().any(|r|r["ItemIndex"]==110033 && r["ItemCount"]==1));
        assert_eq!(parse::<Value>(&currency).unwrap()[0]["Amount"],15000);
        let claimed=crate::api::community::mail::receive_mail(State(s.clone()),form(&u,&format!("MailIndex={season_mail}"))).await.unwrap().0;
        assert_eq!(claimed["Result"],"Success","{claimed}");
        let logged=login(&s,"honor-archive-catalog").await;
        assert!(logged["PlayerCurrencyInfos"].as_array().unwrap().iter().any(|v|v["CurrencyType"]=="RankingPoint" && v["Amount"]==15003));
    }

    #[tokio::test]
    async fn honor_historical_rewards_use_closed_participating_scores_and_settle_once_on_login_and_poll() {
        let s=setup().await;
        let mut users=vec![];for name in ["honor-period-a","honor-period-b","honor-period-idle"] {users.push(login(&s,name).await);}
        let old=season(&s).0-1;let monday=MONDAY+week(now())*WEEK;
        let mut tx=s.db.begin().await.unwrap();
        for (i,u) in users.iter().enumerate() {
            let a=n(&u["UserInfo"],"AccountId");
            sqlx::query("INSERT INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,1,?,?,?,0)")
                .bind(a).bind(old).bind(if i==2 {999999}else{2000}).bind(if i==2 {0}else{2}).execute(&mut *tx).await.unwrap();
            sqlx::query("INSERT INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,0,?,999999,100,0)")
                .bind(a).bind(old).execute(&mut *tx).await.unwrap();
            if i<2 {
                record(&mut tx,a,&json!({"MatchScore":2000,"SeasonWin":2,"SeasonLose":0}),monday-1).await.unwrap();
                record(&mut tx,a,&json!({"MatchScore":999999,"SeasonWin":99,"SeasonLose":0}),now()).await.unwrap();
            }
        }
        // Neither an open week nor an open season grants a reward.
        for u in &users {settle(&mut tx,&s,n(&u["UserInfo"],"AccountId")).await.unwrap();}
        tx.commit().await.unwrap();
        for (i,u) in users.iter().enumerate() {
            let a=n(&u["UserInfo"],"AccountId");
            let claims:i64=sqlx::query_scalar("SELECT COUNT(*) FROM community_claims WHERE account=? AND kind LIKE 'honor_%_reward'").bind(a).fetch_one(&s.db).await.unwrap();
            assert_eq!(claims,if i==2 {0}else{2});
            if i<2 {
                let contents:Vec<String>=sqlx::query_scalar("SELECT content FROM mails WHERE account_id=? AND sender='League of Honor'").bind(a).fetch_all(&s.db).await.unwrap();
                assert!(contents.iter().all(|v|v.contains(&format!("final rank {},",i+1))),"{contents:?}");
                // Login and an Arena poll both exercise the production hooks.
                let logged=login(&s,if i==0 {"honor-period-a"}else{"honor-period-b"}).await;
                assert_eq!(call(&s,&logged,"match/get_match_rank","ArenaType=BanPick").await["Result"],"Success");
                assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE account_id=? AND sender='League of Honor'").bind(a).fetch_one(&s.db).await.unwrap(),2);
            }
        }
        // A player who stays logged in also receives a newly due entitlement
        // through native mailbox polling, without entering Arena again.
        let a=n(&users[0]["UserInfo"],"AccountId");
        sqlx::query("INSERT INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,1,?,2000,2,0)")
            .bind(a).bind(old-1).execute(&s.db).await.unwrap();
        let notice=crate::api::community::mail::check_new_mail(State(s.clone()),form(&users[0],"TopmostMailIndex=0")).await.unwrap().0;
        assert_eq!(notice["NewMailArrived"],true);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE account_id=? AND sender='League of Honor'").bind(a).fetch_one(&s.db).await.unwrap(),3);
    }

    #[tokio::test]
    async fn honor_historical_missing_bundle_does_not_consume_entitlement() {
        let (mut s,u)={let s=setup().await;let u=login(&s,"honor-retry").await;(s,u)};
        let a=n(&u["UserInfo"],"AccountId");
        let original=s.tables.clone();let mut tables=s.tables.as_ref().clone();
        std::sync::Arc::make_mut(&mut tables.arena_guild).tables.remove("HonorRewardBundle");
        s.tables=std::sync::Arc::new(tables);
        let mut tx=s.db.begin().await.unwrap();
        assert!(send(&mut tx,&s,a,1,1,false).await.is_err());
        tx.commit().await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM community_claims WHERE account=? AND kind='honor_season_reward'").bind(a).fetch_one(&s.db).await.unwrap(),0);
        s.tables=original;
        let mut tx=s.db.begin().await.unwrap();send(&mut tx,&s,a,1,1,false).await.unwrap();tx.commit().await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM community_claims WHERE account=? AND kind='honor_season_reward'").bind(a).fetch_one(&s.db).await.unwrap(),1);
    }
}
