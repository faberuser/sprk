//! Native Guild Conquest solo battles, persistent guild HP and score sessions.
use super::*;

const RAID: i64 = 90001;
const EPOCH: i64 = 1767628800; // Monday 2026-01-05 16:00 UTC, archived weekly offset.
const WEEK: i64 = 7 * 86400;

#[derive(Clone, Copy)]
struct Session { index: i64, start: i64, apply_end: i64, battle_start: i64, end: i64 }
impl Session {
    fn by_index(index: i64) -> Self {
        Self::at(EPOCH + (index - 10000) * WEEK)
    }
    fn at(stamp: i64) -> Self {
        let number = (stamp - EPOCH).div_euclid(WEEK).max(0);
        let start = EPOCH + number * WEEK;
        Self { index: 10000 + number, start, apply_end: start + 47*3600,
               battle_start: start + 47*3600 + 1800, end: start + WEEK - 3600 }
    }
    fn state(self, stamp: i64) -> &'static str {
        if stamp < self.apply_end { "Apply" } else if stamp < self.battle_start { "ApplyEnded" }
        else if stamp < self.end { "Battle" } else { "Ended" }
    }
    fn info(self, stamp: i64) -> Value {
        json!({"SessionIndex":self.index,"State":self.state(stamp),"ApplyEndTime":time(self.apply_end-1),
            "BattleEndTime":time(self.end-1),"SessionStartedTime":time(self.start),"BattleStartedTime":time(self.battle_start)})
    }
}
// Calendar definitions belong to the server, alongside registration and battle
// timing. The client receives ordinary native DTOs, without generating table rows.
fn session_definitions(s: &AppState, session: Session, requested: i64) -> Result<Value> {
    let template = row(s, "GuildSuppressSession", &[("RaidIndex", RAID)])?;
    let mut indices = BTreeSet::from([session.index, requested]);
    if session.index > 10000 { indices.insert(session.index - 1); }
    Ok(json!(indices.into_iter().map(|index| {
        let period = Session::by_index(index);
        let mut definition = template.clone();
        definition["SessionIndex"] = json!(index);
        definition["SeasonIndex"] = json!(index);
        definition["ViewRankSeason"] = json!(index);
        definition["ViewSession"] = json!(1);
        definition["PortalViewSession"] = json!(1);
        definition["GlobalApplyStart"] = json!(time(period.start));
        definition["GlobalApplyEnd"] = json!(time(period.apply_end - 1));
        definition["GlobalBattleStart"] = json!(time(period.battle_start));
        definition["GlobalBattleEnd"] = json!(time(period.end - 1));
        definition
    }).collect::<Vec<_>>()))
}
fn stamp(s: &AppState) -> i64 {
    #[cfg(test)]
    if let Some(stamp) = s.tables.arena_guild.rules["GuildConquestTestTime"].as_i64() { return stamp; }
    let _ = s;
    now()
}
fn current(s: &AppState) -> Session { Session::at(stamp(s)) }
#[cfg(test)]
pub(super) fn calendar_for_test(epoch:i64)->Vec<(i64,&'static str)> {
    [0,47*3600,47*3600+1800,WEEK-3600,WEEK].iter().map(|delta| {
        let stamp=epoch+delta;let session=Session::at(stamp);(session.index,session.state(stamp))
    }).collect()
}
pub(super) fn session_index(s: &AppState) -> i64 { current(s).index }
fn boss(s: &AppState, level: i64) -> Result<&Value> {
    row(s,"GuildSuppressBoss", &[("Level",level)])
}
fn definition(s: &AppState, level: i64) -> Result<&Value> {
    let rotation = row(s,"GuildSuppressSession", &[("RaidIndex",RAID)])?;
    let index = rotation["SuppressDungeonIndex"].get((level-1) as usize)
        .and_then(Value::as_i64).ok_or_else(|| rule("WrongDungeonIndex"))?;
    row(s,"GuildSuppressDungeon", &[("Index", index)])
}
async fn registration(db: &mut SqliteConnection, s: &AppState, g: i64) -> Result<Value> {
    let session=current(s);
    let mut value=get(db,g,"guild_conquest",session.index).await?;
    if value.is_null() { value=json!({"Applied":false,"AppliedTime":null,"GroupIndex":0,"SessionIndex":session.index}); }
    if !value["Applied"].as_bool().unwrap_or(false) && n(&guild::state(db,s,g).await?,"AutoApplySuppress")>0
        && matches!(session.state(stamp(s)),"Apply"|"Battle") {
        apply(db,g,session,&mut value).await?;
    }
    Ok(value)
}
async fn apply(db: &mut SqliteConnection, g: i64, session: Session, value: &mut Value) -> Result<()> {
    // Four guilds per local group; the archived group reward table has four places.
    let registered:i64=sqlx::query_scalar("SELECT COUNT(*) FROM community_state WHERE kind='guild_conquest' AND idx=? AND json_extract(data,'$.Applied')=1")
        .bind(session.index).fetch_one(&mut *db).await?;
    value["Applied"]=json!(true);value["AppliedTime"]=json!(time(now()));
    value["GroupIndex"]=json!(registered/4+1);
    let members:Vec<i64>=sqlx::query_scalar("SELECT account_id FROM guild_members WHERE guild_id=? ORDER BY account_id")
        .bind(g).fetch_all(&mut *db).await?;
    value["Members"]=json!(members);
    put(db,g,"guild_conquest",session.index,value).await
}
async fn plays(db: &mut SqliteConnection, s: &AppState, g: i64, session: i64) -> Result<Vec<Value>> {
    let mut result=vec![];
    for level in 1..=3 {
        let key=session*10+level;
        let mut play=get(db,g,"guild_conquest_play",key).await?;
        if play.is_null() {
            play=json!({"SessionIndex":session,"SeasonIndex":session,"ServerGroup":"local","GuildId":g,
                "RaidIndex":RAID,"Level":level,"MonsterHp0":n(boss(s,level)?,"MaxHp"),"TotalDamage":0,"KilledByMe":false});
            // Read-only views need not create a registration or start a battle.
        }
        result.push(play);
    }
    Ok(result)
}
fn info(g: i64, registration: &Value, plays: &[Value]) -> Value {
    let play=&plays[0];
    json!({"SessionIndex":play["SessionIndex"],"GuidId":g,"GroupIndex":registration["GroupIndex"],
        "AppliedTime":registration["AppliedTime"],"RaidIndex":RAID,"Level":1,
        "MonsterHp0":play["MonsterHp0"],"TotalDamage":plays.iter().map(|v|n(v,"TotalDamage")).sum::<i64>(),"ServerGroup":"local"})
}
pub(super) async fn rankers(db: &mut SqliteConnection, s: &AppState, session: i64, group: Option<i64>) -> Result<Vec<Value>> {
    let rows=sqlx::query("SELECT c.owner,c.data,COALESCE(SUM(b.score),0) AS score FROM community_state c JOIN guilds g ON g.guild_id=c.owner LEFT JOIN guild_battle_scores b ON b.guild_id=c.owner AND b.kind='suppress' AND b.season=c.idx WHERE c.kind='guild_conquest' AND c.idx=? AND json_extract(c.data,'$.Applied')=1 GROUP BY c.owner ORDER BY score DESC,c.owner")
        .bind(session).fetch_all(&mut *db).await?;
    let mut result=vec![];
    for row in rows {
        let registration:Value=parse(&row.get::<String,_>("data"))?;
        if group.is_some_and(|v|v!=n(&registration,"GroupIndex")) {continue;}
        let g=row.get::<i64,_>("owner");let guild=guild::state(db,s,g).await?;
        result.push(json!({"Rank":result.len(),"GuildId":g,"GuildName":guild["Name"],"GuildLogo":guild["Logo"],
            "Score":row.get::<i64,_>("score"),"CountryCode":guild["CountryCode"],"ServerGroup":"local","SeasonIndex":session}));
    }
    Ok(result)
}
async fn member_scores(db: &mut SqliteConnection, s: &AppState, g: i64, session: i64) -> Result<Vec<Value>> {
    let rows=sqlx::query("SELECT m.account_id,COALESCE(SUM(b.score),0) AS score FROM guild_members m LEFT JOIN guild_battle_scores b ON b.account=m.account_id AND b.guild_id=m.guild_id AND b.kind='suppress' AND b.season=? WHERE m.guild_id=? GROUP BY m.account_id ORDER BY score DESC,m.account_id")
        .bind(session).bind(g).fetch_all(&mut *db).await?;
    let mut result=vec![];
    for row in rows {
        let a=row.get::<i64,_>("account_id");let u=user(db,a).await?;
        let _=s;
        result.push(json!({"GuildId":g,"AccountId":a,"ServerGroup":"local","NickName":u["NickName"],
            "AvatarHeroIndex":u["AvatarHeroIndex"],"Score":row.get::<i64,_>("score")}));
    }
    Ok(result)
}
pub(super) async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    let previous=sqlx::query("SELECT owner,idx,data FROM community_state c WHERE kind='guild_conquest' AND idx<? AND EXISTS(SELECT 1 FROM guild_battle_scores b WHERE b.guild_id=c.owner AND b.account=? AND b.kind='suppress' AND b.season=c.idx AND b.score>0) AND NOT EXISTS(SELECT 1 FROM community_claims x WHERE x.account=? AND x.kind='guild_conquest_session_reward' AND x.period=CAST(c.idx AS TEXT)) ORDER BY idx LIMIT 100")
        .bind(current(s).index).bind(a).bind(a).fetch_all(&mut *db).await?;
    for registration in previous {
        let g=registration.get::<i64,_>("owner");let period=registration.get::<i64,_>("idx");
        let data:Value=parse(&registration.get::<String,_>("data"))?;
        let ranks=rankers(db,s,period,Some(n(&data,"GroupIndex"))).await?;
        let Some(mine)=ranks.iter().find(|v|n(v,"GuildId")==g) else {continue;};
        let rank=n(mine,"Rank")+1;
        let Some(reward)=s.tables.arena_guild.rows("GuildSuppressReward").iter()
            .find(|v|rank>=n(v,"MinRank")&&rank<=n(v,"MaxRank")) else {continue;};
        let inserted=sqlx::query("INSERT OR IGNORE INTO community_claims(account,kind,target,period) VALUES(?,'guild_conquest_session_reward',0,?)")
            .bind(a).bind(period.to_string()).execute(&mut *db).await?.rows_affected();
        if inserted>0 {progression::mail_reward(db,s,a,n(reward,"RewardIndex"),"Guild Conquest session reward").await?;}
    }
    Ok(())
}
pub(crate) async fn tickets(db: &mut SqliteConnection, s: &AppState, a: i64, cost: i64) -> Result<Value> {
    // Custom (server-issued) stamina. Reset to the table's two keys each UTC day;
    // it does not accumulate offline or accept ruby recharges.
    if cost<0 {return Err(rule("InvalidCost"));}
    let today=time(stamp(s))[..10].to_string();
    let mut value=get(db,a,"guild_conquest_keys",0).await?;
    let grant=s.tables.services.find("Stamina",&[("StaminaType",15)])
        .and_then(|v|v["ResetCount"][0].as_i64()).unwrap_or(2);
    let old=n(&value,"Count");
    if value.is_null() || value["Day"]!=today {value=json!({"Day":today,"Count":grant.max(old)});}
    if n(&value,"Count")<cost {return Err(rule("NotEnoughDungeonKey"));}
    let count=n(&value,"Count")-cost;value["Count"]=json!(count);
    put(db,a,"guild_conquest_keys",0,&value).await?;
    Ok(json!({"Type":"GuildSuppressKey","NewValue":count,"AddValue":count-old,"StaminaRechargeCount":0,
        "StaminaRechargeTime":time((stamp(s).div_euclid(86400)+1)*86400),"MaxValue":grant,"RechargeValue":0,
        "NextRechargeRemainTime":(stamp(s).div_euclid(86400)+1)*86400-stamp(s),
        "FullRechargeRemainTime":if count>=grant {0}else{(stamp(s).div_euclid(86400)+1)*86400-stamp(s)},
        "RechargeCount":0,"IsHide":false}))
}
pub(crate) async fn combat_buffs(db:&mut SqliteConnection,s:&AppState,a:i64)->Result<Value> {
    let (g,_)=guild::membership(db,a).await?;let guild=guild::state(db,s,g).await?;
    let skills=guild["SkillInfos"].as_array().into_iter().flatten().filter(|v|n(v,"EffectSkillIndex")>0)
        .map(|v|json!({"GuildSkillIndex":v["SkillIndex"],"GuildSkillLevel":v["GuildSkillLevel"],"SkillIndex":v["EffectSkillIndex"]})).collect::<Vec<_>>();
    let saved=crate::api::extensions::list(db,a,"class_buff").await?;
    let classes=saved.iter().filter_map(|v|s.tables.extensions.find("ClassBuff",&[("TagType",n(v,"TagType")),("ClassBuffIndex",n(v,"ClassBuffIndex")),("ClassBuffLevel",n(v,"ClassBuffLevel"))]))
        .map(|v|json!({"CreatureType":v["TagType"],"ClassBuffIndex":v["ClassBuffIndex"],"SkillIndex":v["SkillIndex"]})).collect::<Vec<_>>();
    let avatar=crate::api::extensions::get(db,a,"pet_misc",1).await?;
    let id=avatar["MiscValue"].as_str().and_then(|v|v.parse::<i64>().ok()).unwrap_or(0);
    let owned=crate::api::extensions::get(db,a,"pet",id).await?;
    let mut stats=vec![];let mut conditions=vec![];
    if !owned.is_null() {
        if let Some(pet)=s.tables.live.find("Pet",&[("Index",id)]) {
            for option in pet["PetOptionIndices"].as_array().into_iter().flatten().filter_map(Value::as_i64) {
                if let Some(def)=s.tables.live.find("PetOption",&[("Index",option)]) {
                    let value=def[format!("OptionValue{}",n(&owned,"Star"))].clone();
                    if n(def,"ConditionType")==0 {stats.push(json!({"OptionType":def["OptionType"],"OptionValue":value}));}
                    else {conditions.push(json!({"Index":option,"OptionType":def["OptionType"],"OptionValue":value}));}
                }
            }
        }
    }
    Ok(json!({"GuildSkills":skills,"AccountBuffs":[],"ClassBuffDataBases":classes,"ExtraStatDatas":stats,"PetStatDataBases":conditions}))
}
pub(super) async fn execute(db: &mut SqliteConnection, s: &AppState, a: i64, r: &Request, action: &str) -> Result<Value> {
    let (g,role)=guild::membership(db,a).await?;guild::contents_available(db,a).await?;
    settle(db,s,a).await?;
    let session=current(s);let mut registration=registration(db,s,g).await?;
    let requested=r.number("SessionIndex",session.index)?;
    if requested!=0 && (requested<10000 || requested>session.index) {return Err(rule("InvalidSessionIndex"));}
    let requested=if requested==0 {session.index} else {requested};
    let mut result=item::success();
    result["SPRKConquestSessionDefinitions"] = session_definitions(s, session, requested)?;
    match action {
        "apply_guild_suppress" => {
            guild::admin(role)?;
            if requested!=session.index {return Err(rule("InvalidSessionIndex"));}
            if registration["Applied"]==true {return Err(rule("AlreadyApplied"));}
            if session.state(stamp(s))!="Apply" {return Err(rule("NotAppliable"));}
            apply(db,g,session,&mut registration).await?;
        }
        "auto_apply_guild_suppress" => {
            guild::admin(role)?;let mut guild=guild::state(db,s,g).await?;
            guild["AutoApplySuppress"]=json!(i64::from(flag(r,"IsApply")?));
            guild::save(db,g,&guild).await?;result["AutoApplySuppress"]=guild["AutoApplySuppress"].clone();
        }
        "get_guild_suppress_session_info" => {
            let plays=plays(db,s,g,session.index).await?;
            result["GuildSuppressSessionInfo"]=session.info(stamp(s));
            result["GuildSuppressSeasonInfo"]=json!({"SeasonIndex":session.index});
            result["GuildSuppressBanInfo"]=json!({"SeasonIndex":session.index,"SessionIndex":session.index});
            result["GuildSuppressInfo"]=info(g,&registration,&plays);
            result["GuildSuppressPlayInfos"]=json!(plays);result["GuildSuppressApplied"]=registration["Applied"].clone();
            result["AutoApplySuppress"]=guild::state(db,s,g).await?["AutoApplySuppress"].clone();
            result["StaminaResult"]=tickets(db,s,a,0).await?;
        }
        "get_guild_suppress_member_score_list" => {result["GuildSuppressMemberScores"]=json!(member_scores(db,s,g,requested).await?);}
        "get_guild_suppress_group_ranker_list" | "get_guild_suppress_status_board" => {
            let group=n(&registration,"GroupIndex");
            if r.number("GroupIndex",group)?!=group {return Err(rule("NotFoundGuildSuppressGroupIndex"));}
            let ranks=if group>0 {rankers(db,s,requested,Some(group)).await?} else {vec![]};
            if action=="get_guild_suppress_status_board" {
                let mut boards=vec![];
                for rank in &ranks {
                    let guild_id=n(rank,"GuildId");let guild=guild::state(db,s,guild_id).await?;
                    let histories=sqlx::query_scalar::<_,String>("SELECT data FROM community_state WHERE owner=? AND kind='guild_conquest_history' AND json_extract(data,'$.SessionIndex')=? ORDER BY idx DESC LIMIT 100")
                        .bind(guild_id).bind(session.index).fetch_all(&mut *db).await?;
                    let histories=histories.iter().map(|v|parse::<Value>(v)).collect::<Result<Vec<_>>>()?;
                    boards.push(json!({"GuildInfo":guild,"GuildSuppressPlayInfos":plays(db,s,guild_id,session.index).await?,
                        "GuildSuppressMemberScores":member_scores(db,s,guild_id,session.index).await?,"GuildSuppressMemberPlayHistorys":histories}));
                }
                result["GuildSuppressStatusBoardInfos"]=json!(boards);result["GuildSuppressSessionInfo"]=session.info(stamp(s));
            }
            result["GuildSuppressGroupRankerInfos"]=json!(ranks);
        }
        _=>return Err(rule("ContentsDisabled")),
    }
    Ok(result)
}
pub(crate) async fn validate(db: &mut SqliteConnection, s: &AppState, a: i64, r: &Request) -> Result<i64> {
    guild::contents_available(db,a).await?;let (g,_)=guild::membership(db,a).await?;
    let session=current(s);
    if session.state(stamp(s))!="Battle" {return Err(rule("NotOpenedDungeon"));}
    let registered=registration(db,s,g).await?;
    if registered["Applied"]!=true {return Err(rule("NotOpenedDungeon"));}
    let level=r.number("RaidLevel",0)?;
    if r.number("RaidIndex",0)?!=RAID || r.number("ChapterIndex",0)?!=9900
        || !(1..=3).contains(&level) && !(101..=103).contains(&level) || r.number("DungeonIndex",0)?!=level {
        return Err(rule("NotOpenedDungeon"));
    }
    let level=if level>100 {level-100} else {level};
    let play=plays(db,s,g,session.index).await?.remove((level-1) as usize);
    if n(&play,"MonsterHp0")<0 {return Err(rule("AlreadyCompleted"));}
    let heroes=ids(r,"HeroIndices",6)?;let ai=ids(r,"AiHeroIndices",3)?;
    if !ai.is_empty() && (ai.iter().any(|v|!heroes.contains(v)) || heroes.len()-ai.len()>3) {return Err(rule("NotMatchHeroIndices"));}
    Ok(n(definition(s,level)?,"BanIndex"))
}
pub(crate) async fn enter(db: &mut SqliteConnection, s: &AppState, a: i64, r: &Request, entry: &mut Value, out: &mut Value) -> Result<()> {
    let (g,_)=guild::membership(db,a).await?;let session=current(s);let requested=r.number("RaidLevel",0)?;let level=if requested>100 {requested-100} else {requested};
    let plays=plays(db,s,g,session.index).await?;let play=&plays[(level-1) as usize];
    let mut guild_info=info(g,&registration(db,s,g).await?,&plays);
    guild_info["Level"]=json!(requested);guild_info["MonsterHp0"]=play["MonsterHp0"].clone();
    out["GuildSuppressInfo"]=guild_info;
    out["GuildSuppressPlayInfo"]=play.clone();
    out["GuildSuppressScoreInfo"]=json!({"GuildId":g,"AccountId":a,"Rank":0,"TotalDamage":0});
    entry["ConquestGuildId"]=json!(g);entry["ConquestSession"]=json!(session.index);
    entry["ConquestLevel"]=json!(level);entry["ConquestBossHp"]=play["MonsterHp0"].clone();
    entry["ConquestBossIndex"]=boss(s,level)?["CreatureIndex"].clone();
    Ok(())
}
pub(crate) async fn finish(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &Value,
    out: &mut Value,
) -> Result<()> {
    let g = n(entry, "ConquestGuildId");
    let session = n(entry, "ConquestSession");
    let level = n(entry, "ConquestLevel");
    if guild::membership(db, a).await?.0 != g || current(s).index != session {
        return Err(rule("NotOpenedDungeon"));
    }
    let mut plays = plays(db, s, g, session).await?;
    let mut play = plays[(level - 1) as usize].clone();
    let damage = damage(r, entry)?;
    let credited = if level == 3 {
        damage
    } else {
        damage.min(n(&play, "MonsterHp0").max(0))
    };
    let total = n(&play, "TotalDamage")
        .checked_add(credited)
        .ok_or_else(|| rule("InvalidValue"))?;
    play["TotalDamage"] = json!(total);
    let killed = level != 3 && n(&play, "MonsterHp0") > 0 && credited == n(&play, "MonsterHp0");
    if level != 3 {
        play["MonsterHp0"] = json!(if killed || n(&play, "MonsterHp0") < 0 {
            -1
        } else {
            n(&play, "MonsterHp0") - credited
        });
    }
    play["KilledByMe"] = json!(false);
    put(db, g, "guild_conquest_play", session * 10 + level, &play).await?;
    sqlx::query("INSERT INTO guild_battle_scores(guild_id,account,kind,season,stage,score) VALUES(?,?,'suppress',?,?,?) ON CONFLICT(guild_id,account,kind,season,stage) DO UPDATE SET score=score+excluded.score")
        .bind(g).bind(a).bind(session).bind(level).bind(credited).execute(&mut *db).await?;
    let user=user(db,a).await?;
    let history=json!({"GuildId":g,"ServerGroup":"local","AccountId":a,"NickName":user["NickName"],
        "BattleTime":time(now()),"Score":credited,"RaidIndex":RAID,"Level":level,"SessionIndex":session});
    let key:i64=sqlx::query_scalar("SELECT COALESCE(MAX(idx),0)+1 FROM community_state WHERE owner=? AND kind='guild_conquest_history'")
        .bind(g).fetch_one(&mut *db).await?;
    put(db,g,"guild_conquest_history",key,&history).await?;
    if killed {
        let members:Vec<i64>=sqlx::query_scalar("SELECT account FROM guild_battle_scores WHERE guild_id=? AND kind='suppress' AND season=? AND stage=? AND score>0")
            .bind(g).bind(session).bind(level).fetch_all(&mut *db).await?;
        for member in members {
            let inserted=sqlx::query("INSERT OR IGNORE INTO community_claims(account,kind,target,period) VALUES(?,'guild_conquest_kill',?,?)")
                .bind(member).bind(level).bind(session.to_string()).execute(&mut *db).await?.rows_affected();
            if inserted>0 {progression::mail_reward(db,s,member,n(definition(s,level)?,"KillRewardIndex"),"Guild Conquest boss defeated").await?;}
        }
    }
    plays[(level-1) as usize]=play.clone();
    let registered=registration(db,s,g).await?;
    out["GuildSuppressInfo"]=info(g,&registered,&plays);
    play["KilledByMe"]=json!(killed);out["GuildSuppressPlayInfo"]=play;
    let rank=rankers(db,s,session,Some(n(&registered,"GroupIndex"))).await?.iter()
        .find(|v|n(v,"GuildId")==g).map(|v|n(v,"Rank")).unwrap_or(0);
    out["GuildSuppressScoreInfo"]=json!({"GuildId":g,"AccountId":a,"Rank":rank,"TotalDamage":credited});
    out["StaminaResult"]=tickets(db,s,a,0).await?;
    Ok(())
}
fn damage(r: &Request, entry: &Value) -> Result<i64> {
    let raw=r.text("CreatureInfoString");
    if raw.is_empty() {return Err(rule("InvalidValue"));}
    let creatures:Vec<Value>=serde_json::from_str(raw).or_else(|_| {
        let decoded=urlencoding::decode(raw).map_err(<serde_json::Error as serde::de::Error>::custom)?;
        serde_json::from_str(&decoded)
    }).map_err(|_|rule("InvalidValue"))?;
    if creatures.len()>64 {return Err(rule("InvalidValue"));}
    let integer=|v:&Value,k:&str|v[k].as_i64().or_else(||v[k].as_str().and_then(|s|s.parse().ok())).unwrap_or(-1);
    let heroes=entry["Heroes"].as_array().ok_or_else(||rule("InvalidValue"))?;
    let mut boss_hp=None;let mut boss_damage=None;let mut seen=BTreeSet::new();
    for creature in creatures {
        let index=integer(&creature,"Index");let team=integer(&creature,"TeamId");
        if team==0 {
            if !heroes.contains(&json!(index)) || !seen.insert(index) {return Err(rule("HeroNotFound"));}
            let damage=integer(&creature,"GivedDamage");
            if damage<0 {return Err(rule("InvalidValue"));}
        } else if team==1 && index==n(entry,"ConquestBossIndex") {
            let hp=integer(&creature,"Hp");
            if boss_hp.is_some() || hp<0 || hp>n(entry,"ConquestBossHp") {return Err(rule("InvalidValue"));}
            boss_hp=Some(hp);
            let damage=integer(&creature,"GivedDamage");
            if n(entry,"ConquestLevel")==3 && damage<0 {return Err(rule("InvalidValue"));}
            boss_damage=Some(damage);
        }
    }
    if n(entry,"ConquestLevel")==3 {
        if seen.is_empty() || boss_hp.is_none() {return Err(rule("InvalidValue"));}
        boss_damage.ok_or_else(||rule("InvalidValue"))
    } else {boss_hp.map(|hp|n(entry,"ConquestBossHp")-hp).ok_or_else(||rule("InvalidValue"))}
}
