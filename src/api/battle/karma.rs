use super::*;
use crate::models::item::ItemGrant;
use std::collections::BTreeMap;

#[derive(Clone, serde::Serialize, serde::Deserialize)]
#[serde(rename_all = "PascalCase")]
struct RewardPolicy {
    version: i64,
    fragments_per_sqrt_wave: i64,
    max_fragments_per_run: i64,
    bonus_every_cleared_waves: i64,
    max_bonus_bundles_per_run: i64,
}

impl RewardPolicy {
    fn parse(value: &Value) -> Result<Self> {
        let policy: Self =
            serde_json::from_value(value.clone()).map_err(|_| rule("InvalidValue"))?;
        if policy.version != 1
            || !(1..=1000).contains(&policy.fragments_per_sqrt_wave)
            || !(1..=1000).contains(&policy.max_fragments_per_run)
            || !(1..=1000).contains(&policy.bonus_every_cleared_waves)
            || !(0..=100).contains(&policy.max_bonus_bundles_per_run)
        {
            return Err(rule("InvalidValue"));
        }
        Ok(policy)
    }

    fn amounts(&self, cleared: i64) -> Result<(i64, i64)> {
        if cleared < 0 {
            return Err(rule("InvalidValue"));
        }
        // Integer square root avoids rounding at the perfect-square milestones.
        let scaled = (cleared as u64)
            .checked_mul(self.fragments_per_sqrt_wave as u64)
            .and_then(|v| v.checked_mul(self.fragments_per_sqrt_wave as u64))
            .ok_or_else(|| rule("InvalidValue"))?;
        Ok((
            (scaled.isqrt() as i64).min(self.max_fragments_per_run),
            (cleared / self.bonus_every_cleared_waves).min(self.max_bonus_bundles_per_run),
        ))
    }
}

async fn grant_scaled_bundle(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    index: i64,
    copies: i64,
    grant: &mut Rewards,
) -> Result<()> {
    if copies == 0 {
        return Ok(());
    }
    let data = s
        .tables
        .get_reward(i32::try_from(index).map_err(|_| rule("RewardDataNotFound"))?)
        .ok_or_else(|| rule("RewardDataNotFound"))?;
    let mut items = BTreeMap::<(i32, i32, i32), i32>::new();
    // Keep individual source rolls independent, but return one inventory update
    // per stack instead of dozens of duplicate reward icons.
    for _ in 0..copies {
        for drop in data.roll_items(&s.tables.reward_string_pool) {
            let id = s
                .tables
                .get_item_index(&drop.item_code)
                .ok_or_else(|| rule("ItemDataNotFound"))?;
            if !s
                .tables
                .items
                .reward_item(id)
                .is_some_and(|r| r.kind == "Item")
            {
                return Err(rule("InvalidItemType"));
            }
            use rand::Rng;
            let star =
                rand::thread_rng().gen_range(drop.star_min..=drop.star_max.max(drop.star_min));
            let count = items
                .entry((id, star, drop.custom_option_index))
                .or_default();
            *count = count
                .checked_add(drop.count)
                .ok_or_else(|| rule("InvalidItemCount"))?;
        }
    }
    for ((index, star, custom), count) in items {
        item::give(
            db,
            s,
            a,
            ItemGrant {
                index,
                count,
                star,
                custom,
            },
            grant,
        )
        .await?;
    }
    Ok(())
}

fn state_key(group: i64) -> i64 {
    group * 10 + 1
}
// Karma is a score mode and intentionally has no victory-cost receipt. Its
// local battle still needs to release the party after withdrawal/relogin.
pub(super) fn is_entry(s: &AppState, entry: &Value) -> bool {
    s.tables
        .battle
        .find(
            "CampaignDungeon",
            &[
                ("ChapterIndex", n(entry, "ChapterIndex")),
                ("DungeonIndex", n(entry, "DungeonIndex")),
            ],
        )
        .is_some_and(|dungeon| n(dungeon, "BattleType") == 48)
}
fn group_data<'a>(s: &'a AppState, r: &Request) -> Result<&'a Value> {
    row(
        s,
        "PunishmentGroup",
        &[
            ("GroupIndex", int(r, "GroupIndex")?),
            ("PunishmentDungeonType", 1),
        ],
    )
}
pub(super) async fn opening(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    let group = group_data(s, r)?;
    let key = state_key(n(group, "GroupIndex"));
    let mut p = get(db, a, "punishment_open", key).await?;
    let mut out = item::success();
    if action != "get_punishment_raid_info" {
        let active:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_runs WHERE account=? AND completed=0 AND started>=?)")
            .bind(a).bind(now()-settings(s,"BattleExpirySeconds",14400)).fetch_one(&mut *db).await?;
        if active {
            return Err(rule("AlreadyOnBattleHero"));
        }
    }
    if action == "open_punishment_raid" {
        let raid = row(
            s,
            "Raid",
            &[("Index", n(group, "KarmaRaidIndex")), ("Level", 1)],
        )?;
        if raid["IsOpen"] != true || n(raid, "ChapterIndex") != n(group, "ChapterIndex") {
            return Err(rule("ContentsDisabled"));
        }
        if p["Day"] == day() && n(&p, "IsOpen") == 1 {
            return Err(rule("AlreadyOpened"));
        }
        out["StaminaResult"] = dungeons::charge(db, s, a, 1, n(group, "OpenStaminaCount")).await?;
        p = json!({"GroupIndex":n(group,"GroupIndex"),"DungeonType":1,"IsOpen":1,"OpenedTime":time(now()),"ExpireTime":time((now()/86400+1)*86400),"OpenLevel":1,"ClearLevel":0,"ClearCount":0,"Day":day()});
        put(db, a, "punishment_open", key, &p).await?;
    } else if action == "reset_punishment_raid" {
        if p.is_null() {
            return Err(rule("DungeonNotFound"));
        }
        p["IsOpen"] = json!(0);
        put(db, a, "punishment_open", key, &p).await?;
    }
    if out["StaminaResult"].is_null() {
        out["StaminaResult"] = dungeons::charge(db, s, a, 1, 0).await?;
    }
    out["OpenPunishmentRaidInfo"] = p;
    out["PunishmentRaidInfos"] = json!(list(db, a, "punishment_raid").await?);
    Ok(out)
}

#[cfg(test)]
mod reward_tests {
    use super::*;
    use crate::api::battle::tests::{account, call, setup};
    use std::sync::Arc;

    #[test]
    fn karma_policy_milestones_and_validation() {
        let value = json!({"Version":1,"FragmentsPerSqrtWave":10,"MaxFragmentsPerRun":100,
            "BonusEveryClearedWaves":10,"MaxBonusBundlesPerRun":5});
        let policy = RewardPolicy::parse(&value).unwrap();
        for (wave, fragments, bundles) in [
            (0, 0, 0),
            (1, 10, 0),
            (5, 22, 0),
            (10, 31, 1),
            (25, 50, 2),
            (50, 70, 5),
            (100, 100, 5),
            (1000, 100, 5),
        ] {
            assert_eq!(policy.amounts(wave).unwrap(), (fragments, bundles));
        }
        assert!(policy.amounts(-1).is_err());
        for (field, invalid) in [
            ("Version", 2),
            ("BonusEveryClearedWaves", 0),
            ("FragmentsPerSqrtWave", -1),
        ] {
            let mut bad = value.clone();
            bad[field] = json!(invalid);
            assert!(RewardPolicy::parse(&bad).is_err());
        }
    }

    async fn begin(s: &AppState, u: &Value, group: i64, chapter: i64, flask: i64) {
        sqlx::query("UPDATE heroes SET level=100")
            .execute(&s.db)
            .await
            .unwrap();
        sqlx::query("UPDATE user_info SET stamina=500000")
            .execute(&s.db)
            .await
            .unwrap();
        let opened = call(
            s,
            u,
            "punishment_raid/open_punishment_raid",
            &format!("GroupIndex={group}&DungeonType=1"),
        )
        .await;
        assert_eq!(opened["Result"], "Success", "{opened}");
        let begun = call(s,u,"contents/begin_content",&format!("GroupIndex={group}&DungeonType=Karma&ChapterIndex={chapter}&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=1&FlaskItemIndex={flask}")).await;
        assert_eq!(begun["Result"], "Success", "{begun}");
        sqlx::query("UPDATE battle_runs SET started=started-200")
            .execute(&s.db)
            .await
            .unwrap();
    }

    fn ending(s: &AppState, group: i64, chapter: i64, waves: i64) -> String {
        let (_, kills) = gauge(s, group, waves, 0).unwrap();
        format!("GroupIndex={group}&DungeonType=Karma&ChapterIndex={chapter}&DungeonIndex=1&DungeonDifficulty=0&Completed=false&ClearWave={waves}&EndWave={}&EndKillCount=0&TotalKillCount={kills}", waves+1)
    }

    fn added(out: &Value, id: i64) -> i64 {
        out["ItemResults"]
            .as_array()
            .unwrap()
            .iter()
            .filter(|v| n(v, "ItemIndex") == id)
            .map(|v| n(v, "AddCount"))
            .sum()
    }

    #[tokio::test]
    async fn karma_progress_rewards_both_groups_and_no_repeat() {
        let (s, u) = setup().await;
        for (group, chapter, flask, fragment) in
            [(101001, 70101, 40236, 45121), (101002, 70201, 40246, 45130)]
        {
            for (waves, fragments, bundles) in [
                (0, 0, 0),
                (5, 22, 0),
                (10, 31, 1),
                (25, 50, 2),
                (50, 70, 5),
                (100, 100, 5),
            ] {
                begin(&s, &u, group, chapter, flask).await;
                let args = ending(&s, group, chapter, waves);
                let out = call(&s, &u, "contents/end_content", &args).await;
                assert_eq!(out["Result"], "Success", "{out}");
                assert_eq!(added(&out, fragment), fragments, "{out}");
                for (offset, count) in [(1, 10), (2, 4), (3, 2), (4, 1)] {
                    assert_eq!(added(&out, flask + offset), bundles * count, "{out}");
                }
                let inventory: Vec<(i64, i64)> = sqlx::query_as(
                    "SELECT item_index,count FROM items WHERE account_id=? ORDER BY item_index",
                )
                .bind(account(&u))
                .fetch_all(&s.db)
                .await
                .unwrap();
                assert_ne!(
                    call(&s, &u, "contents/end_content", &args).await["Result"],
                    "Success"
                );
                assert_eq!(
                    sqlx::query_as::<_, (i64, i64)>(
                        "SELECT item_index,count FROM items WHERE account_id=? ORDER BY item_index"
                    )
                    .bind(account(&u))
                    .fetch_all(&s.db)
                    .await
                    .unwrap(),
                    inventory
                );
            }
        }
    }

    #[tokio::test]
    async fn karma_policy_snapshot_and_atomic_retry() {
        let (mut s, u) = setup().await;
        begin(&s, &u, 101001, 70101, 40236).await;
        let args = ending(&s, 101001, 70101, 10);
        let rules = &mut Arc::make_mut(&mut Arc::make_mut(&mut s.tables).battle).rules;
        rules["KarmaRewardPolicy"]["FragmentsPerSqrtWave"] = json!(1);
        rules["KarmaRewardPolicy"]["MaxFragmentsPerRun"] = json!(1);
        rules["KarmaRewardPolicy"]["MaxBonusBundlesPerRun"] = json!(0);
        Arc::make_mut(&mut Arc::make_mut(&mut s.tables).inventory)
            .items
            .get_mut(&45121)
            .unwrap()["ItemMaxCap"] = json!(1);
        assert_ne!(
            call(&s, &u, "contents/end_content", &args).await["Result"],
            "Success"
        );
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT completed FROM battle_runs WHERE account=?")
                .bind(account(&u))
                .fetch_one(&s.db)
                .await
                .unwrap(),
            0
        );
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM items WHERE account_id=? AND item_index IN (40241,45121,40237)").bind(account(&u)).fetch_one(&s.db).await.unwrap(),0);
        Arc::make_mut(&mut Arc::make_mut(&mut s.tables).inventory)
            .items
            .get_mut(&45121)
            .unwrap()["ItemMaxCap"] = json!(0);
        let out = call(&s, &u, "contents/end_content", &args).await;
        assert_eq!(out["Result"], "Success", "{out}");
        assert_eq!(added(&out, 45121), 31);
        assert_eq!(added(&out, 40237), 10);
        begin(&s, &u, 101001, 70101, 40236).await;
        let out = call(&s, &u, "contents/end_content", &args).await;
        assert_eq!(out["Result"], "Success", "{out}");
        assert_eq!(added(&out, 45121), 1);
        assert_eq!(added(&out, 40237), 0);
    }
}
pub(super) async fn validate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<()> {
    let group = group_data(s, r)?;
    let p = get(db, a, "punishment_open", state_key(n(group, "GroupIndex"))).await?;
    if int(r, "DungeonType")? != 1 || n(&p, "IsOpen") != 1 || p["Day"] != day() {
        return Err(rule("RaidNotStarted"));
    }
    if int(r, "ChapterIndex")? != n(group, "ChapterIndex")
        || int(r, "DungeonIndex")? != 1
        || campaign::difficulty(r)? != 0
        || int(r, "RaidIndex")? != n(group, "KarmaRaidIndex")
        || int(r, "RaidLevel")? != 1
        || !ids(r, "AffixIndices", 32)?.is_empty()
        || !ids(r, "AiHeroIndices", 32)?.is_empty()
        || !ids(r, "GroupHeroIndices", 32)?.is_empty()
    {
        return Err(rule("DungeonNotFound"));
    }
    let flask = row(s, "ShardFlaskItem", &[("Index", int(r, "FlaskItemIndex")?)])?;
    if n(flask, "GroupIndex") != n(group, "GroupIndex") || n(flask, "FlaskGauge") <= 0 {
        return Err(rule("InvalidItem"));
    }
    if flask["IsDefault"] != true {
        let count:i64=sqlx::query_scalar("SELECT COALESCE((SELECT count FROM items WHERE account_id=? AND item_index=? AND locked=0),0)")
            .bind(a).bind(n(flask,"Index")).fetch_one(&mut *db).await?;
        if count <= 0 {
            return Err(rule("ItemNotOwned"));
        }
    }
    Ok(())
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    if action == "end_content" {
        return end(db, s, a, r).await;
    }
    if action != "begin_content" {
        return Err(rule("ContentsDisabled"));
    }
    let group = group_data(s, r)?;
    let mut request = Request(r.0.clone());
    request
        .0
        .insert("RaidIndex".into(), n(group, "KarmaRaidIndex").to_string());
    request.0.insert("RaidLevel".into(), "1".into());
    if int(r, "FlaskItemIndex")? == 0 {
        let default = s
            .tables
            .battle
            .rows("ShardFlaskItem")
            .iter()
            .find(|v| n(v, "GroupIndex") == n(group, "GroupIndex") && v["IsDefault"] == true)
            .ok_or_else(|| rule("ContentsDisabled"))?;
        // The native selector normally sends this index; retain zero-input compatibility.
        request
            .0
            .insert("FlaskItemIndex".into(), n(default, "Index").to_string());
    }
    validate(db, s, a, &request).await?;
    // Retries cannot swap the selected flask while retaining the old entry snapshot.
    if let Some((entry,)) = sqlx::query_as::<_, (String,)>(
        "SELECT entry FROM battle_runs WHERE account=? AND completed=0 AND started>=?",
    )
    .bind(a)
    .bind(now() - settings(s, "BattleExpirySeconds", 14400))
    .fetch_optional(&mut *db)
    .await?
    {
        let saved: Value = read_json(&entry)?;
        if saved["Request"]["FlaskItemIndex"] != json!(request.text("FlaskItemIndex")) {
            return Err(rule("AlreadyOnBattleHero"));
        }
    }
    let out = campaign::begin(db, s, a, &request).await?;
    let raw: String = sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?")
        .bind(a)
        .fetch_one(&mut *db)
        .await?;
    let mut entry: Value = read_json(&raw)?;
    if entry["KarmaFlaskCount"].is_null() {
        let count:i64=sqlx::query_scalar("SELECT COALESCE((SELECT count FROM items WHERE account_id=? AND item_index=? AND locked=0),0)")
            .bind(a).bind(int(&request,"FlaskItemIndex")?).fetch_one(&mut *db).await?;
        entry["KarmaFlaskCount"] = json!(count);
        entry["KarmaRewardPolicy"] = json!(RewardPolicy::parse(
            &s.tables.battle.rules["KarmaRewardPolicy"]
        )?);
        sqlx::query("UPDATE battle_runs SET entry=? WHERE account=?")
            .bind(entry.to_string())
            .bind(a)
            .execute(&mut *db)
            .await?;
    }
    Ok(out)
}

// Mirror PunishmentShardGaugeWindow.ExpCalculator: loop the extracted stages,
// multiply each next wave's gauge, then credit its fractional final kill count.
fn gauge(s: &AppState, group: i64, clear: i64, end_kills: i64) -> Result<(f32, i64)> {
    let def = row(
        s,
        "PunishmentGroup",
        &[("GroupIndex", group), ("PunishmentDungeonType", 1)],
    )?;
    let mut points = n(def, "InitialShardPoint").max(1) as f32;
    let mut total = 0f32;
    let mut kills = 0;
    let mut level = 1;
    let mut wave = 1;
    for index in 0..=clear {
        let stage = row(
            s,
            "KarmaDungeon",
            &[("PunishmentGroupIndex", group), ("StageLevel", level)],
        )?;
        let meta = row(
            s,
            "KarmaWave",
            &[
                ("ChapterIndex", n(stage, "ChapterIndex")),
                ("DungeonIndex", n(stage, "DungeonIndex")),
                ("Difficulty", 0),
                ("WaveIndex", wave),
            ],
        )?;
        let monsters = n(meta, "MonsterCount");
        if monsters <= 0 {
            return Err(rule("ContentsDisabled"));
        }
        if index == clear {
            if end_kills < 0 || end_kills > monsters {
                return Err(rule("ModulatedData"));
            }
            total += end_kills as f32 / monsters as f32 * points;
            kills += end_kills;
            break;
        }
        total += points;
        kills += monsters;
        let multiplier = s
            .tables
            .battle
            .rows("KarmaGaugeExponential")
            .iter()
            .filter(|v| n(v, "PunishmentGroupIndex") == group && n(v, "Wave") <= index + 2)
            .max_by_key(|v| n(v, "Wave"))
            .and_then(|v| v["ExponentialValue"].as_f64())
            .unwrap_or(0.) as f32;
        points *= multiplier;
        wave += 1;
        if s.tables
            .battle
            .find(
                "KarmaWave",
                &[
                    ("ChapterIndex", n(stage, "ChapterIndex")),
                    ("DungeonIndex", n(stage, "DungeonIndex")),
                    ("Difficulty", 0),
                    ("WaveIndex", wave),
                ],
            )
            .is_none()
        {
            level = n(stage, "NextLevel");
            wave = 1;
        }
    }
    if !total.is_finite() || total < 0. {
        return Err(rule("ModulatedData"));
    }
    Ok((total, kills))
}
pub(super) async fn end(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<Value> {
    let saved = sqlx::query("SELECT * FROM battle_runs WHERE account=?")
        .bind(a)
        .fetch_optional(&mut *db)
        .await?
        .ok_or_else(|| rule("UserCampaignInfoNotFound"))?;
    if saved.get::<i64, _>("completed") != 0 {
        return Err(rule("AlreadyCompleted"));
    }
    let elapsed = now() - saved.get::<i64, _>("started");
    if elapsed > settings(s, "BattleExpirySeconds", 14400) {
        return Err(rule("UserCampaignInfoNotFound"));
    }
    let entry: Value = read_json(&saved.get::<String, _>("entry"))?;
    let request = Request(read_value(entry["Request"].clone())?);
    if entry["KarmaFlaskCount"].is_null()
        || entry["ServiceRequired"] == true
        || entry["ServiceOwned"] == true
    {
        return Err(rule("NotCompletedBattle"));
    }
    for field in [
        "ChapterIndex",
        "DungeonIndex",
        "DungeonDifficulty",
        "GroupIndex",
        "DungeonType",
    ] {
        if int(r, field)? != int(&request, field)? {
            return Err(rule("DungeonNotFound"));
        }
    }
    let clear = int(r, "ClearWave")?;
    if clear < 0
        || clear > settings(s, "KarmaMaxClearWaves", 100)
        || int(r, "EndWave")? != clear + 1
        || clear > elapsed / settings(s, "KarmaMinSecondsPerWave", 1).max(1)
    {
        return Err(rule("ModulatedData"));
    }
    let group = group_data(s, &request)?;
    let (mut points, kills) = gauge(s, n(group, "GroupIndex"), clear, int(r, "EndKillCount")?)?;
    if int(r, "TotalKillCount")? != kills {
        return Err(rule("ModulatedData"));
    }
    let alive = ids(r, "AliveHeroIndices", 32)?;
    let party: Vec<i64> = read_value(entry["Heroes"].clone())?;
    if alive.iter().any(|v| !party.contains(v)) {
        return Err(rule("HeroNotFound"));
    }
    let flask = row(
        s,
        "ShardFlaskItem",
        &[("Index", int(&request, "FlaskItemIndex")?)],
    )?;
    let mut grant = Rewards::default();
    if flask["IsDefault"] != true {
        let count = ((points / n(flask, "FlaskGauge") as f32).floor() as i64)
            .min(n(&entry, "KarmaFlaskCount"));
        if count > 0 {
            grant
                .items
                .push(item::consume(db, a, n(flask, "Index") as i32, count as i32).await?);
            tutorial::grant_item(
                db,
                s,
                a,
                ItemGrant {
                    index: n(flask, "ShardItemIndex") as i32,
                    count: count as i32,
                    star: 0,
                    custom: 0,
                },
                &mut grant,
            )
            .await?;
            points -= count as f32 * n(flask, "FlaskGauge") as f32;
        }
    }
    let default = s
        .tables
        .battle
        .rows("ShardFlaskItem")
        .iter()
        .find(|v| n(v, "GroupIndex") == n(group, "GroupIndex") && v["IsDefault"] == true)
        .ok_or_else(|| rule("ContentsDisabled"))?;
    let count = (points / n(default, "FlaskGauge") as f32).floor() as i64;
    if count > settings(s, "KarmaMaxShardsPerRun", 100000) {
        return Err(rule("ModulatedData"));
    }
    if count > 0 {
        tutorial::grant_item(
            db,
            s,
            a,
            ItemGrant {
                index: n(default, "ShardItemIndex") as i32,
                count: count as i32,
                star: 0,
                custom: 0,
            },
            &mut grant,
        )
        .await?;
    }
    // Explicit local balance policy. An old active run without a snapshot uses
    // the configured policy; new runs retain the version selected at entry.
    let policy = RewardPolicy::parse(
        entry
            .get("KarmaRewardPolicy")
            .unwrap_or(&s.tables.battle.rules["KarmaRewardPolicy"]),
    )?;
    let (fragments, bonus_bundles) = policy.amounts(clear)?;
    grant_scaled_bundle(
        db,
        s,
        a,
        n(group, "KarmaFixRewardIndex"),
        fragments,
        &mut grant,
    )
    .await?;
    grant_scaled_bundle(
        db,
        s,
        a,
        n(group, "KarmaBonusRewardIndex"),
        bonus_bundles,
        &mut grant,
    )
    .await?;
    let mut p = get(db, a, "punishment_open", state_key(n(group, "GroupIndex"))).await?;
    p["IsOpen"] = json!(0);
    p["ClearLevel"] = json!(clear);
    p["ClearCount"] = json!(n(&p, "ClearCount") + 1);
    put(
        db,
        a,
        "punishment_open",
        state_key(n(group, "GroupIndex")),
        &p,
    )
    .await?;
    sqlx::query("UPDATE battle_runs SET completed=1 WHERE account=? AND completed=0")
        .bind(a)
        .execute(&mut *db)
        .await?;
    crate::api::progression::record(db,a,"KillMonster",0,0,kills).await?;
    let mut out = rewards(db, s, a, grant).await?;
    super::treasure::record_time(db, s, a, r, saved.get("started"), 48, &mut out).await?;
    out["OpenPunishmentRaidInfos"] = json!([p]);
    out["PunishmentRaidInfos"] = json!(list(db, a, "punishment_raid").await?);
    Ok(out)
}
