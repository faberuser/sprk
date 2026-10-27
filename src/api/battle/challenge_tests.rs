use super::*;
use crate::api::battle::tests::{setup,call,account};

fn pin(s: &mut AppState, id: i64) {
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle)
        .rules["ChallengeRaidRotation"]=json!([id]);
}
const ENTER: &str = "ChapterIndex=961&DungeonIndex=1&DungeonDifficulty=0&RaidIndex=70002&RaidLevel=1&HeroIndices=[1]";
const END: &str = "ChapterIndex=961&DungeonIndex=1&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=[1]";

#[tokio::test]
async fn challenge_native_entry_best_score_time_rewards_and_replay() {
    let (mut s,u)=setup().await;pin(&mut s,70002);let a=account(&u);
    sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
    let events=events(&s);assert_eq!(events.len(),6);
    assert_eq!(events[2]["BonusType"],"ChallengeRaid");assert_eq!(events[2]["Condition"],"70002");
    let rank=call(&s,&u,"raid/get_challenge_raid_rank_info","RaidIndex=70002").await;
    assert_eq!(rank["RankInfo"]["Rank"],-1);
    // Native affixes charge advantages against penalties, and reject foreign IDs.
    for invalid in ["&AffixIndices=[1]","&AffixIndices=[999999]","&AffixIndices=[1,1]"] {
        assert_ne!(call(&s,&u,"campaign/begin_campaign",&format!("{ENTER}{invalid}")).await["Result"],"Success");
    }
    for (attempt, seconds) in [60,90,40].into_iter().enumerate() {
        let begin=call(&s,&u,"campaign/begin_campaign",ENTER).await;
        assert_eq!(begin["Result"],"Success","{begin}");
        sqlx::query("UPDATE battle_runs SET started=? WHERE account=?").bind(now()-seconds).bind(a).execute(&s.db).await.unwrap();
        let end=call(&s,&u,"campaign/end_campaign",&format!("{END}&PureBattleTime={seconds}&TotalDamage=999999999999")).await;
        assert_eq!(end["Result"],"Success","{end}");
        assert_eq!(end["ChallengeRaidRankerInfo"]["Score"],3000);
        assert_eq!(end["ChallengeRaidRankerInfo"]["BattleTime"],seconds.min(60)*1000);
        assert_eq!(end["ChallengeRaidRankerInfo"]["CurrentBattleTime"],seconds*1000);
        let points=end["CurrencyResults"].as_array().unwrap().iter()
            .find(|v|v["CurrencyType"]=="ChallengeRaidPoint").expect("A clear must award spendable shop points");
        assert_eq!(points["AddValue"],3000);
        assert_eq!(points["NewValue"],(attempt+1)*3000);
        assert_ne!(call(&s,&u,"campaign/end_campaign",&format!("{END}&PureBattleTime={seconds}")).await["Result"],"Success");
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT value FROM battle_currencies WHERE account=? AND kind='ChallengeRaidPoint'")
            .bind(a).fetch_one(&s.db).await.unwrap(),(attempt as i64+1)*3000);
    }
    let season=seasons::season(&s).0;
    assert_eq!(call(&s,&u,"raid/get_challenge_raid_ranker_list","RaidIndex=70002&PageNo=1").await["RankerInfos"],json!([]));
    assert_ne!(call(&s,&u,"raid/get_challenge_raid_rank_info","RaidIndex=70004").await["Result"],"Success");
    // A native defeat sends zero PureBattleTime but retains elapsed milliseconds.
    assert_eq!(call(&s,&u,"campaign/begin_campaign",ENTER).await["Result"],"Success");
    let loss=call(&s,&u,"campaign/end_campaign","ChapterIndex=961&DungeonIndex=1&DungeonDifficulty=0&Completed=false&Star=0&PureBattleTime=0&ElapsedBattleTime=1234").await;
    assert_eq!(loss["Result"],"Success");
    assert_eq!(loss["ChallengeRaidRankerInfo"]["CurrentScore"],0);
    assert_eq!(loss["ChallengeRaidRankerInfo"]["CurrentBattleTime"],1234);
    assert_eq!(loss["ChallengeRaidRankerInfo"]["Score"],3000);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT value FROM battle_currencies WHERE account=? AND kind='ChallengeRaidPoint'")
        .bind(a).fetch_one(&s.db).await.unwrap(),9000);
    let login=crate::api::account::user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0;
    assert!(login.player_currency_infos.iter().any(|v|v["CurrencyType"]=="ChallengeRaidPoint" && n(v,"Amount")==9000));
    // A reconnect settles the previous season once, including its real reward bundle.
    sqlx::query("UPDATE challenge_results SET season=?").bind(season-1).execute(&s.db).await.unwrap();
    let before: i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails").fetch_one(&s.db).await.unwrap();
    for _ in 0..2 { super::super::login(&s,a).await.unwrap(); }
    let after: i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails").fetch_one(&s.db).await.unwrap();
    assert_eq!(after,before+1);
}

#[tokio::test]
async fn challenge_season_rollover_and_unselected_raid_cannot_award() {
    let (mut s,u)=setup().await;pin(&mut s,70002);
    sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
    assert_ne!(call(&s,&u,"campaign/begin_campaign",&ENTER.replace("&RaidIndex=70002&RaidLevel=1","")).await["Result"],"Success");
    assert_eq!(call(&s,&u,"campaign/begin_campaign",ENTER).await["Result"],"Success");
    sqlx::query("UPDATE battle_runs SET started=started-60,entry=json_set(entry,'$.ChallengeSeason',0)").execute(&s.db).await.unwrap();
    assert_ne!(call(&s,&u,"campaign/end_campaign",&format!("{END}&PureBattleTime=60")).await["Result"],"Success");
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM challenge_results").fetch_one(&s.db).await.unwrap(),0);
}

#[tokio::test]
async fn challenge_all_rotation_stages_have_archived_affixes_and_rewards() {
    let (s,_)=setup().await;
    for id in 70001..=70016 {
        let definition=row(&s,"ChallengeRaid",&[("Index",id)]).unwrap();
        if id%2==0 {assert!(s.tables.battle.rows("ChallengeRaidReward").iter().any(|v|n(v,"ChallengeRaidIndex")==id));}
        for raid in s.tables.battle.rows("Raid").iter().filter(|v|n(v,"Index")==n(definition,"RaidIndex")) {
            // Build-1558 RaidTable and its localized guides agree on this split.
            if id%2==0 {
                assert_eq!(n(raid,"Type"),9);
                assert_eq!(n(raid,"PlayerCount"),1);
                assert_eq!(n(raid,"MainPartyCount"),4);
                assert_eq!(n(raid,"SubPartyCount"),4);
            } else {
                assert_eq!(n(raid,"Type"),7);
                assert_eq!(n(raid,"PlayerCount"),3);
                assert_eq!(n(raid,"DeckCountPerPlayer"),4); // SPRK override; archive uses three.
            }
            assert_eq!(n(raid,"DeckCount"),8);
            let d=row(&s,"CampaignDungeon",&[("ChapterIndex",n(raid,"ChapterIndex")),("DungeonIndex",n(raid,"DungeonIndex"))]).unwrap();
            assert!(n(d,"ClearPoint_Easy")>0 && n(d,"AffixMultiple_Easy")>0);
            assert!(s.tables.battle.rows("ChallengeAffix").iter().any(|v|n(v,"GroupIndex")==n(d,"AffixGroupIndex_Easy")));
        }
    }
}
