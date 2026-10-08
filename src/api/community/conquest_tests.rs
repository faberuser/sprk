use super::*;
use super::tests::{setup, login, call, create};
fn clock(s: &mut AppState, offset: i64) {
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).arena_guild).rules["GuildConquestTestTime"]=json!(1767628800+offset);
}
async fn battle(s:&AppState,u:&Value,path:&str,args:&str)->Value {
    crate::api::battle::execute_request(s,path,Bytes::from(format!("SessionKey={}&{args}",u["UserInfo"]["SessionKey"].as_str().unwrap())))
        .await.unwrap()
}
fn begin(level:i64)->String { format!("ChapterIndex=9900&DungeonIndex={level}&DungeonDifficulty=0&RaidIndex=90001&RaidLevel={level}&HeroIndices=[1]") }
fn ending(level:i64,hp:i64,damage:i64,win:bool)->String {
    let boss=if level==101 {11301} else if level==102 {8803} else {8801};
    let creatures=json!([{"Index":1,"TeamId":0,"Hp":"1","GivedDamage":damage.to_string(),"Key":"1_0_0"},
        {"Index":boss,"TeamId":1,"Hp":hp.to_string(),"GivedDamage":damage.to_string(),"Key":format!("{boss}_0_1")}]);
    format!("ChapterIndex=9900&DungeonIndex={level}&DungeonDifficulty=0&Completed={win}&Star={}&AliveHeroIndices=[1]&CreatureInfoString={}",
        if win {3}else{0},urlencoding::encode(&creatures.to_string()))
}
async fn prepared()->(AppState,Value,i64) {
    let mut s=setup().await;clock(&mut s,3600);
    let u=login(&s,"conquest-gameplay").await;let g=create(&s,&u,"ConquestTest",1).await;
    assert_eq!(call(&s,&u,"guild_suppress/apply_guild_suppress","").await["Result"],"Success");
    clock(&mut s,50*3600);(s,u,g)
}
#[tokio::test]
async fn conquest_defeat_records_damage_consumes_keys_and_retries_are_idempotent() {
    let (s,u,_)=prepared().await;
    let start=battle(&s,&u,"campaign/begin_campaign",&begin(101)).await;
    assert_eq!(start["Result"],"Success","{start}");
    assert_eq!(start["StaminaResult"]["NewValue"],0);
    let hp=n(&start["GuildSuppressInfo"],"MonsterHp0");
    assert_eq!(start["GuildSuppressInfo"]["Level"],101);
    assert_eq!(battle(&s,&u,"campaign/begin_campaign",&begin(101)).await,start);
    let end=battle(&s,&u,"campaign/end_campaign",&ending(101,hp-12345,12345,false)).await;
    assert_eq!(end["Result"],"Success","{end}");
    assert_eq!(end["GuildSuppressScoreInfo"]["TotalDamage"],12345);
    assert_eq!(end["GuildSuppressPlayInfo"]["MonsterHp0"],hp-12345);
    assert_eq!(end["StaminaResult"]["NewValue"],0);
    assert_eq!(battle(&s,&u,"campaign/end_campaign",&ending(101,hp-12345,12345,false)).await,end);
    let board=call(&s,&u,"guild_suppress/get_guild_suppress_status_board","").await;
    assert_eq!(board["GuildSuppressGroupRankerInfos"][0]["Score"],12345);
    assert_eq!(board["GuildSuppressStatusBoardInfos"][0]["GuildSuppressMemberPlayHistorys"].as_array().unwrap().len(),1);
    assert_ne!(battle(&s,&u,"campaign/begin_campaign",&begin(101)).await["Result"],"Success");
}
#[tokio::test]
async fn conquest_boss_kill_mails_once_and_final_boss_uses_native_boss_damage() {
    let (mut s,u,_)=prepared().await;
    let start=battle(&s,&u,"campaign/begin_campaign",&begin(101)).await;
    assert_eq!(start["Result"],"Success","{start}");
    let hp=n(&start["GuildSuppressInfo"],"MonsterHp0");
    let kill=battle(&s,&u,"campaign/end_campaign",&ending(101,0,hp,true)).await;
    assert_eq!(kill["Result"],"Success","{kill}");assert_eq!(kill["GuildSuppressPlayInfo"]["MonsterHp0"],-1);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE title='Guild Conquest boss defeated'").fetch_one(&s.db).await.unwrap(),1);
    assert_eq!(battle(&s,&u,"campaign/end_campaign",&ending(101,0,hp,true)).await,kill);
    clock(&mut s,74*3600);
    assert_ne!(battle(&s,&u,"campaign/begin_campaign",&begin(101)).await["Result"],"Success");
    let start=battle(&s,&u,"campaign/begin_campaign",&begin(103)).await;assert_eq!(start["Result"],"Success","{start}");
    let hp=n(&start["GuildSuppressInfo"],"MonsterHp0");
    let end=battle(&s,&u,"campaign/end_campaign",&ending(103,hp,654321,false)).await;
    assert_eq!(end["Result"],"Success","{end}");assert_eq!(end["GuildSuppressScoreInfo"]["TotalDamage"],654321);
    assert_eq!(end["GuildSuppressPlayInfo"]["MonsterHp0"],hp);
    clock(&mut s,7*86400+3600);
    let refreshed=call(&s,&u,"guild_suppress/get_guild_suppress_session_info","").await;
    assert_eq!(refreshed["GuildSuppressSessionInfo"]["SessionIndex"],10001);assert_eq!(refreshed["GuildSuppressPlayInfos"][0]["TotalDamage"],0);
    call(&s,&u,"guild_suppress/get_guild_suppress_session_info","").await;
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE title='Guild Conquest session reward'").fetch_one(&s.db).await.unwrap(),1);
}
#[tokio::test]
async fn conquest_invalid_requests_do_not_spend_or_score() {
    let (s,u,g)=prepared().await;
    let wrong=begin(101).replace("RaidIndex=90001","RaidIndex=80001");
    assert_ne!(battle(&s,&u,"campaign/begin_campaign",&wrong).await["Result"],"Success");
    assert_eq!(call(&s,&u,"guild_suppress/get_guild_suppress_session_info","").await["StaminaResult"]["NewValue"],2);
    let start=battle(&s,&u,"campaign/begin_campaign",&begin(101)).await;let hp=n(&start["GuildSuppressInfo"],"MonsterHp0");
    assert_ne!(battle(&s,&u,"campaign/end_campaign",&ending(101,hp+1,10,false)).await["Result"],"Success");
    assert_eq!(call(&s,&u,"guild_suppress/get_guild_suppress_status_board","").await["GuildSuppressGroupRankerInfos"][0]["Score"],0);
    sqlx::query("DELETE FROM guild_members WHERE guild_id=?").bind(g).execute(&s.db).await.unwrap();
    assert_ne!(battle(&s,&u,"campaign/end_campaign",&ending(101,hp-1,1,false)).await["Result"],"Success");
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM guild_battle_scores WHERE kind='suppress'").fetch_one(&s.db).await.unwrap(),0);
}
#[test]
fn conquest_calendar_matches_archived_weekly_boundaries() {
    assert_eq!(super::conquest::calendar_for_test(1767628800),vec![(10000,"Apply"),(10000,"ApplyEnded"),(10000,"Battle"),(10000,"Ended"),(10001,"Apply")]);
}
