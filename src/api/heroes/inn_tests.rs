use super::*;
use serde_json::Value;

fn request(user:&Value)->GiveRewardHeroInnRouletteRequest {
    GiveRewardHeroInnRouletteRequest {session_id:None,session_key:Some(user["UserInfo"]["SessionKey"].as_str().unwrap().into()),roulette_index:Some("1".into())}
}

#[tokio::test]
async fn inn_roulette_all_sections_credit_once_and_enforce_daily_quota() {
    let s=crate::api::community::tests::setup().await;
    let user=crate::api::community::tests::login(&s,"inn-roulette").await;
    let account=user["UserInfo"]["AccountId"].as_i64().unwrap();
    crate::db::query("UPDATE user_info SET gem=3000000000,pay_gem=4000,gold=5000000000 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
    for section in 1..=8 {
        crate::db::query("DELETE FROM hero_inn_roulette_spins WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
        let before:(i64,i64,i64)=crate::db::query_as("SELECT gold,gem,friendship_point FROM user_info WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap();
        let out=spin_roulette(s.clone(),request(&user),section).await.unwrap().0;
        assert_eq!(out.result,"Success");assert_eq!(out.section,section);assert_eq!(out.roulette_info.as_ref().unwrap().remain_count,0);
        let reward=get_default_roulette_rewards(1).into_iter().find(|r|r.section==section).unwrap();
        let after:(i64,i64,i64)=crate::db::query_as("SELECT gold,gem,friendship_point FROM user_info WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap();
        let value=reward.value1 as i64;
        assert_eq!(after,(before.0+if reward.type1=="Gold"{value}else{0},before.1+if reward.type1=="Gem"{value}else{0},before.2+if reward.type1=="FriendshipPoint"{value}else{0}));
        if reward.type1=="Gem" {assert_eq!(out.currency_results.unwrap()[0].new_value,after.1+4000);}
        let duplicate=spin_roulette(s.clone(),request(&user),section).await.unwrap().0;
        assert_eq!(duplicate.result,"NoMoreRoulette");
        assert_eq!(crate::db::query_scalar::<i64>("SELECT spin_count FROM hero_inn_roulette_spins WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap(),1);
        let unchanged:(i64,i64,i64)=crate::db::query_as("SELECT gold,gem,friendship_point FROM user_info WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap();
        assert_eq!(unchanged,after);
    }
}

#[tokio::test]
async fn inn_roulette_reward_failure_rolls_back_spin_and_mission_credit() {
    let s=crate::api::community::tests::setup().await;
    let user=crate::api::community::tests::login(&s,"inn-rollback").await;
    let account=user["UserInfo"]["AccountId"].as_i64().unwrap();
    crate::db::query("UPDATE user_info SET gold=$1 WHERE account_id=$2").bind(i64::MAX).bind(account).execute(&s.db).await.unwrap();
    assert!(spin_roulette(s.clone(),request(&user),1).await.is_err());
    assert_eq!(crate::db::query_scalar::<i64>("SELECT count(*) FROM hero_inn_roulette_spins WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap(),0);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT count(*) FROM progression_metrics WHERE account_id=$1 AND kind='RotateRulletDaily'").bind(account).fetch_one(&s.db).await.unwrap(),0);
    // Two concurrent attempts at the remaining free spin cannot award twice.
    crate::db::query("UPDATE user_info SET gold=0 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
    let (a,b)=tokio::join!(spin_roulette(s.clone(),request(&user),1),spin_roulette(s.clone(),request(&user),1));
    assert_eq!([a.unwrap().0.result,b.unwrap().0.result].iter().filter(|r|r.as_str()=="Success").count(),1);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT gold FROM user_info WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap(),50000);
}
