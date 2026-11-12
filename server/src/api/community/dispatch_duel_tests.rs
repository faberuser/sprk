use super::*;
use super::tests::{setup,login,call};

async fn players()->(AppState,Vec<Value>) {
    let s=setup().await;
    let users=vec![login(&s,"dispatch-duel-a").await,login(&s,"dispatch-duel-b").await,login(&s,"dispatch-duel-outsider").await];
    for u in &users {
        for hero in 1..=16 {
            crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,level,star) VALUES($1,$2,$3,100,5) ON CONFLICT DO NOTHING")
                .bind(account(u)).bind(hero).bind(hero).execute(&s.db).await.unwrap();
        }
    }
    (s,users)
}
fn account(u:&Value)->i64 {n(&u["UserInfo"],"AccountId")}
fn entry(kind:i64,group:i64,heroes:&[i64])->String {
    format!("ArenaType={kind}&DuelType={}&GroupId={group}&HeroIndices={}&LeaderHeroIndex={}",if group>0{1}else{0},json!(heroes),heroes[0])
}
async fn invite(s:&AppState,users:&[Value],group:i64) {
    crate::db::query("INSERT INTO friendly_duels VALUES($1,$2,$3,'accepted',$4)")
        .bind(group).bind(account(&users[0])).bind(account(&users[1])).bind(now()+900).execute(&s.db).await.unwrap();
}
async fn dispatch(s:&AppState,a:i64,state:&str) {
    crate::db::query("INSERT INTO battle_state(account,kind,idx,data) VALUES($1,'dispatch',1,$2) ON CONFLICT(account,kind,idx) DO UPDATE SET data=excluded.data")
        .bind(a).bind(json!({"SlotIndex":1,"State":state,"HeroIndices":"[1]"}).to_string()).execute(&s.db).await.unwrap();
}
async fn active(s:&AppState)->i64 {
    crate::db::query_scalar("SELECT count(*) FROM arena_runs WHERE status IN ('waiting','battle')").fetch_one(&s.db).await.unwrap()
}

#[tokio::test]
async fn dispatch_duel_rejected_registration_releases_both_players_in_either_order_and_league() {
    for kind in [0,1] {for invalid_first in [false,true] {
        let (s,users)=players().await;
        let group=account(&users[0])+if kind==1{duel::HONOR_DUEL_GROUP_OFFSET}else{0};
        invite(&s,&users,group).await;
        dispatch(&s,account(&users[0]),"Battle").await;
        let heroes=if kind==1{(1..=16).collect::<Vec<_>>()}else{vec![1]};
        let args=entry(kind,group,&heroes);
        if !invalid_first {assert_eq!(call(&s,&users[1],"match/register_match",&args).await["Result"],"Success");}
        assert_eq!(call(&s,&users[0],"match/register_match",&args).await["Result"],"Fail");
        if invalid_first {
            assert_eq!(call(&s,&users[1],"match/register_match",&args).await["Result"],"MatchingBlocked");
        } else {
            assert_eq!(call(&s,&users[1],"match/wait_match","PlayOfflineMatch=false").await["Result"],"Canceled");
        }
        assert_eq!(active(&s).await,0);
        assert_eq!(crate::db::query_scalar::<String>("SELECT status FROM friendly_duels WHERE group_id=$1").bind(group).fetch_one(&s.db).await.unwrap(),"canceled");
        // Both accounts can immediately use a free hero in another league.
        for u in &users[..2] {
            assert_eq!(call(&s,u,"match/register_match",&entry(0,0,&[2])).await["Result"],"Success");
            assert_eq!(call(&s,u,"match/cancel_match","").await["Result"],"Success");
        }
        dispatch(&s,account(&users[0]),"Cancel").await;
        crate::db::query("UPDATE friendly_duels SET status='accepted' WHERE group_id=$1").bind(group).execute(&s.db).await.unwrap();
        for u in &users[..2] {assert_eq!(call(&s,u,"match/register_match",&args).await["Result"],"Success");}
    }}
}

#[tokio::test]
async fn dispatch_duel_forged_and_duplicate_failed_requests_preserve_valid_registrations() {
    let (s,users)=players().await;let group=account(&users[0]);invite(&s,&users,group).await;
    let args=entry(0,group,&[1]);
    for u in &users[..2] {assert_eq!(call(&s,u,"match/register_match",&args).await["Result"],"Success");}
    assert_eq!(call(&s,&users[2],"match/register_match",&args).await["Result"],"MatchingBlocked");
    dispatch(&s,account(&users[0]),"Battle").await;
    assert_eq!(call(&s,&users[0],"match/register_match",&args).await["Result"],"Fail");
    assert_eq!(active(&s).await,2);
    assert_eq!(crate::db::query_scalar::<String>("SELECT status FROM friendly_duels WHERE group_id=$1").bind(group).fetch_one(&s.db).await.unwrap(),"accepted");
}

#[tokio::test]
async fn dispatch_duel_busy_registration_cancels_only_the_pending_duel() {
    let (s,users)=players().await;let group=account(&users[0]);
    assert_eq!(call(&s,&users[0],"match/register_match",&entry(0,0,&[2])).await["Result"],"Success");
    invite(&s,&users,group).await;
    let args=entry(0,group,&[1]);
    assert_eq!(call(&s,&users[1],"match/register_match",&args).await["Result"],"Success");
    assert_eq!(call(&s,&users[0],"match/register_match",&args).await["Result"],"WaitMore");
    assert_eq!(call(&s,&users[1],"match/wait_match","").await["Result"],"Canceled");
    assert_eq!(call(&s,&users[0],"match/wait_match","PlayOfflineMatch=false").await["Result"],"WaitMore");
    assert_eq!(active(&s).await,1);
}

#[tokio::test]
async fn dispatch_duel_native_cancel_releases_waiting_peer_but_preserves_other_queue() {
    let (s,users)=players().await;let group=account(&users[0]);invite(&s,&users,group).await;
    assert_eq!(call(&s,&users[0],"match/register_match",&entry(0,group,&[1])).await["Result"],"Success");
    assert_eq!(call(&s,&users[2],"match/register_match",&entry(0,0,&[1])).await["Result"],"Success");
    duel::cancel_waiting(&mut s.db.acquire().await.unwrap(),account(&users[0]),account(&users[1]),group).await.unwrap();
    assert_eq!(call(&s,&users[0],"match/wait_match","").await["Result"],"Canceled");
    assert_eq!(active(&s).await,1);
}

#[tokio::test]
async fn dispatch_duel_only_terminal_dispatches_release_heroes() {
    let (s,users)=players().await;
    for state in ["Prepare","Simulation","Battle","ReadyToComplete","Complete","Cancel"] {
        dispatch(&s,account(&users[0]),state).await;
        let r=call(&s,&users[0],"match/register_match",&entry(0,0,&[1])).await;
        if matches!(state,"Complete"|"Cancel") {
            assert_eq!(r["Result"],"Success","{state}");call(&s,&users[0],"match/cancel_match","").await;
        } else {assert_eq!(r["Result"],"Fail","{state}");}
    }
}
