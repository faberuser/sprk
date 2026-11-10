//! Native friend invitations; group membership is validated before arena pairing.
use super::*;

/// League of Honor friendly duels use the inviter's account ID plus this
/// offset as GroupId; Victory duels keep the native account ID. Every native
/// duel message and the arena registration carry GroupId, so both clients and
/// the server agree on the league without a new protocol field.
pub(crate) const HONOR_DUEL_GROUP_OFFSET: i64 = 1 << 40;

/// The arena kind (0 Victory, 1 Honor) of a friendly duel GroupId.
pub(crate) fn league(group: i64) -> i64 {
    i64::from(group > HONOR_DUEL_GROUP_OFFSET)
}

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
            // The native friend menu uses the inviter's account ID as GroupId;
            // Honor duels add HONOR_DUEL_GROUP_OFFSET to it.
            if group!=a && group!=a+HONOR_DUEL_GROUP_OFFSET {return Err(rule("InvalidValue"));}
            let busy:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM friendly_duels WHERE status IN ('invited','accepted') AND expires>? AND (requester IN (?,?) OR opponent IN (?,?)))")
                .bind(now()).bind(a).bind(b).bind(a).bind(b).fetch_one(&mut *tx).await?;
            if busy {return Err(rule("WaitMore"));}
            sqlx::query("INSERT INTO friendly_duels(group_id,requester,opponent,status,expires) VALUES(?,?,?,'invited',?) ON CONFLICT(group_id) DO UPDATE SET requester=excluded.requester,opponent=excluded.opponent,status='invited',expires=excluded.expires")
                .bind(group).bind(a).bind(b).bind(now()+180).execute(&mut *tx).await?;
            let user=user(&mut tx,a).await?;
            // The client names the league in the invitation from GroupId.
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
            };
            // Accepting an invitation its sender canceled (or that expired): the
            // native client has already entered the party step. Answer with the
            // inviter's CancelDuel so it shows "canceled" and returns, instead
            // of waiting for a partner who will never start.
            let withdrawn=|row:&Option<sqlx::sqlite::SqliteRow>| match row {
                None=>true,
                Some(row)=>row.get::<i64,_>("requester")==b && row.get::<i64,_>("opponent")==a && row.get::<String,_>("status")!="invited",
            };
            if protocol=="AcceptDuel" && (group==b || group==b+HONOR_DUEL_GROUP_OFFSET) && withdrawn(&row) {
                drop(tx);
                s.chat.notify(a,b,"CancelDuel",json!({"GroupId":group,"IsReject":false}));
                return Err(rule("WrongMember"));
            }
            let row=row.ok_or_else(||rule("WrongMember"))?;
            let requester:i64=row.get("requester");let opponent:i64=row.get("opponent");let status:String=row.get("status");
            if !((requester==a && opponent==b)||(requester==b && opponent==a)) {return Err(rule("WrongMember"));}
            if protocol=="AcceptDuel" {
                if a!=opponent || status!="invited" {return Err(rule("WrongMember"));}
                sqlx::query("UPDATE friendly_duels SET status='accepted',expires=? WHERE group_id=?")
                    .bind(now()+900).bind(group).execute(&mut *tx).await?;
            } else if protocol=="CancelDuel" {
                cancel_waiting(&mut tx,a,b,group).await?;
            } else if matches!(protocol,"DuelPing"|"DuelPong") {
                if !matches!(status.as_str(),"invited"|"accepted") {return Err(rule("WrongMember"));}
            } else if status!="accepted" {return Err(rule("WrongMember"));}
        }
    }
    tx.commit().await?;
    tracing::debug!(protocol, from=a, to=b, group, "Duel message relayed");
    s.chat.notify(b,a,protocol,content);
    Ok(())
}
pub(crate) async fn opponent(db:&mut SqliteConnection,a:i64,group:i64)->Result<i64> {
    sqlx::query_scalar("SELECT CASE WHEN requester=? THEN opponent ELSE requester END FROM friendly_duels WHERE group_id=? AND status='accepted' AND expires>? AND (requester=? OR opponent=?)")
        .bind(a).bind(group).bind(now()).bind(a).bind(a).fetch_optional(db).await?.ok_or_else(||rule("MatchingBlocked"))
}

// Party-ready messages precede HTTP registration in the native client. If one
// registration fails, its partner may already be waiting in the arena queue.
// Persist cancellation independently of the rejected registration transaction.
pub(super) async fn registration_failed(s:&AppState,a:i64,r:&Request)->Result<()> {
    if r.number("DuelType",0).ok()!=Some(1) {return Ok(());}
    let Some(group)=r.number("GroupId",0).ok().filter(|v|*v>0) else {return Ok(());};
    let mut tx=s.db.begin().await?;
    let partner:Option<i64>=sqlx::query_scalar("SELECT CASE WHEN requester=? THEN opponent ELSE requester END FROM friendly_duels WHERE group_id=? AND status='accepted' AND (requester=? OR opponent=?)")
        .bind(a).bind(group).bind(a).bind(a).fetch_optional(&mut *tx).await?;
    let Some(b)=partner else {return Ok(());};
    // A bad duplicate request must not cancel an already valid registration or
    // a match in progress. Nor may a forged group cancel another player's duel.
    let registered:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM arena_runs WHERE account=? AND status IN ('waiting','battle') AND json_extract(data,'$.DuelType')=1 AND json_extract(data,'$.GroupId')=?)")
        .bind(a).bind(group).fetch_one(&mut *tx).await?;
    if registered {return Ok(());}
    cancel_waiting(&mut tx,a,b,group).await?;
    tx.commit().await?;
    for (to,from) in [(a,b),(b,a)] {
        s.chat.notify(to,from,"CancelDuel",json!({"GroupId":group,"IsReject":false}));
    }
    tracing::info!(account=a,opponent=b,group,"Canceled pending duel after rejected registration");
    Ok(())
}

pub(super) async fn cancel_waiting(db:&mut SqliteConnection,a:i64,b:i64,group:i64)->Result<()> {
    sqlx::query("UPDATE friendly_duels SET status='canceled' WHERE group_id=? AND ((requester=? AND opponent=?) OR (requester=? AND opponent=?))")
        .bind(group).bind(a).bind(b).bind(b).bind(a).execute(&mut *db).await?;
    sqlx::query("UPDATE arena_runs SET status='canceled',data=json_set(data,'$.QueueCancelReason','DuelCanceled') WHERE account IN (?,?) AND status='waiting' AND json_extract(data,'$.DuelType')=1 AND json_extract(data,'$.GroupId')=?")
        .bind(a).bind(b).bind(group).execute(db).await?;
    Ok(())
}
