//! Native friend invitations; group membership is validated before arena pairing.
use super::*;

pub(crate) fn supported(protocol: &str) -> bool {
    matches!(protocol, "CanDuelRequest"|"CanDuelResponse"|"RequestDuel"|"AcceptDuel"|"CancelDuel"|"CompletePartySetting"|"BeginDuel"|"DuelPing"|"DuelPong")
}
pub(crate) async fn send(s: &AppState, a: i64, b: i64, protocol: &str, mut content: Value) -> Result<()> {
    if a==b || b<=0 || !s.chat.online(b) { return Err(rule("WrongMember")); }
    let mut tx=s.db.begin().await?;
    item::init(&mut tx,s,a).await?;
    let friends:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM friends WHERE status='accepted' AND ((account_id=? AND friend_account_id=?) OR (account_id=? AND friend_account_id=?)))")
        .bind(a).bind(b).bind(b).bind(a).fetch_one(&mut *tx).await?;
    if !friends { return Err(rule("WrongMember")); }
    let group=content["GroupId"].as_i64().or_else(||content["GroupId"].as_str().and_then(|v|v.parse().ok())).unwrap_or(0);
    match protocol {
        "CanDuelRequest"|"CanDuelResponse"=>{},
        "RequestDuel"=>{
            // The native friend menu uses the inviter's account ID as GroupId.
            if group!=a {return Err(rule("InvalidValue"));}
            let busy:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM friendly_duels WHERE status IN ('invited','accepted') AND expires>? AND (requester IN (?,?) OR opponent IN (?,?)))")
                .bind(now()).bind(a).bind(b).bind(a).bind(b).fetch_one(&mut *tx).await?;
            if busy {return Err(rule("WaitMore"));}
            sqlx::query("INSERT INTO friendly_duels(group_id,requester,opponent,status,expires) VALUES(?,?,?,'invited',?) ON CONFLICT(group_id) DO UPDATE SET requester=excluded.requester,opponent=excluded.opponent,status='invited',expires=excluded.expires")
                .bind(group).bind(a).bind(b).bind(now()+180).execute(&mut *tx).await?;
            let user=user(&mut tx,a).await?;
            content=json!({"GroupId":group,"Nick":user["Nick"],"TeamLevel":user["TeamLevel"],"AvatarIndex":user["AvatarHeroIndex"]});
        },
        _=>{
            // Native clients start pinging before acceptance. In that state the
            // receiver replies with GroupId=0, which DuelManager explicitly accepts.
            // Resolve only that pong through the authenticated, live invitation pair.
            let row=if protocol=="DuelPong" && group==0 {
                sqlx::query("SELECT requester,opponent,status FROM friendly_duels WHERE status IN ('invited','accepted') AND expires>? AND ((requester=? AND opponent=?) OR (requester=? AND opponent=?))")
                    .bind(now()).bind(a).bind(b).bind(b).bind(a).fetch_optional(&mut *tx).await?
            } else {
                sqlx::query("SELECT requester,opponent,status FROM friendly_duels WHERE group_id=? AND expires>?")
                    .bind(group).bind(now()).fetch_optional(&mut *tx).await?
            }.ok_or_else(||rule("WrongMember"))?;
            let requester:i64=row.get("requester");let opponent:i64=row.get("opponent");let status:String=row.get("status");
            if !((requester==a && opponent==b)||(requester==b && opponent==a)) {return Err(rule("WrongMember"));}
            if protocol=="AcceptDuel" {
                if a!=opponent || status!="invited" {return Err(rule("WrongMember"));}
                sqlx::query("UPDATE friendly_duels SET status='accepted',expires=? WHERE group_id=?")
                    .bind(now()+900).bind(group).execute(&mut *tx).await?;
            } else if protocol=="CancelDuel" {
                sqlx::query("UPDATE friendly_duels SET status='canceled' WHERE group_id=?").bind(group).execute(&mut *tx).await?;
            } else if matches!(protocol,"DuelPing"|"DuelPong") {
                if !matches!(status.as_str(),"invited"|"accepted") {return Err(rule("WrongMember"));}
            } else if status!="accepted" {return Err(rule("WrongMember"));}
        }
    }
    tx.commit().await?;
    s.chat.notify(b,a,protocol,content);
    Ok(())
}
pub(crate) async fn opponent(db:&mut SqliteConnection,a:i64,group:i64)->Result<i64> {
    sqlx::query_scalar("SELECT CASE WHEN requester=? THEN opponent ELSE requester END FROM friendly_duels WHERE group_id=? AND status='accepted' AND expires>? AND (requester=? OR opponent=?)")
        .bind(a).bind(group).bind(now()).bind(a).bind(a).fetch_optional(db).await?.ok_or_else(||rule("MatchingBlocked"))
}
