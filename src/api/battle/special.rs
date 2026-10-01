use super::*;
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
) -> Result<Value> {
    if path.starts_with("eclipse/") {
        eclipse(db, s, a, r, path.rsplit('/').next().unwrap()).await
    } else {
        ordeal(db, s, a, r, path.rsplit('/').next().unwrap()).await
    }
}
async fn eclipse(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    let mut decks = list(db, a, "eclipse_deck").await?;
    // Older emulator versions persisted JSON arrays inside this string. The
    // native client splits it on commas, so normalize saved decks on reads too.
    for deck in &mut decks {
        deck["HeroIndices"] = json!(eclipse_hero_string(&eclipse_heroes(&deck["HeroIndices"])?));
    }
    let run = get(db, a, "eclipse", 0).await?;
    if action == "get_eclipse_deck" {
        return Ok(json!({"DeckResults":decks}));
    }
    if action == "set_eclipse_deck" {
        if run["IsPlayEclipse"] == true {
            return Err(rule("Fail"));
        }
        let values: Vec<Value> = read_json(r.text("HeroInfos")).map_err(|_| rule("Fail"))?;
        if values.len() > s.tables.hero_shop.constant("EclipseMaxTeam", 10) as usize {
            return Err(rule("Fail"));
        }
        let mut used = BTreeSet::new();
        let mut deck_ids = BTreeSet::new();
        let mut new = vec![];
        for v in values {
            // JM_NShared_EclipseDeckResult encodes integer fields as strings.
            let index = v["DeckIndex"].as_i64()
                .or_else(|| v["DeckIndex"].as_str().and_then(|v| v.parse().ok()))
                .ok_or_else(|| rule("Fail"))?;
            if !(1..=10).contains(&index) || !deck_ids.insert(index) {
                return Err(rule("Fail"));
            }
            let hero_ids = eclipse_heroes(&v["HeroIndices"])?;
            let mut heroes = vec![];
            for id in &hero_ids {
                if *id <= 0 || *id > i32::MAX as i64 || !used.insert(*id) {
                    return Err(rule("HeroNotFound"));
                }
                heroes.push(cached_hero(db, a, *id as i32).await?);
            }
            new.push(json!({"DeckIndex":index,"HeroIndices":eclipse_hero_string(&hero_ids),"CachedHeroInfos":heroes,"UpdatedTime":time(now()),"ClearMaxWaveIndex":0}));
        }
        new.sort_by_key(|v| n(v, "DeckIndex"));
        // Both battle initialization and the deck UI address decks by index - 1.
        if new.iter().enumerate().any(|(i, v)| n(v, "DeckIndex") != i as i64 + 1) {
            return Err(rule("Fail"));
        }
        sqlx::query("DELETE FROM battle_state WHERE account=? AND kind='eclipse_deck'")
            .bind(a)
            .execute(&mut *db)
            .await?;
        for v in &new {
            put(db, a, "eclipse_deck", n(v, "DeckIndex"), v).await?;
        }
        decks = new;
        return Ok(json!({"DeckResults":decks}));
    }
    if action == "get_eclipse_info" {
        return super::eclipse::info(db, s, a, decks).await;
    }
    if action == "begin_eclipse" {
        if decks.is_empty() || decks.iter().all(|d| d["HeroIndices"] == "") {
            return Err(rule("HeroNotFound"));
        }
        // A real-time battle service is required by this request's response contract.
        // Decks and collection records remain available while that service is absent.
        return Err(rule("BattleServerNotFound"));
    }
    super::eclipse::execute(db, s, a, r, action).await
}
pub(super) fn eclipse_heroes(value: &Value) -> Result<Vec<i64>> {
    let text = value.as_str().ok_or_else(|| rule("Fail"))?.trim();
    let heroes = if text.starts_with('[') {
        // Retain compatibility with existing API clients and saved decks.
        let values: Vec<Value> = read_json(text).map_err(|_| rule("Fail"))?;
        values.iter().map(|v| v.as_i64()
            .or_else(|| v.as_str().and_then(|v| v.parse().ok()))
            .ok_or_else(|| rule("HeroNotFound"))).collect::<Result<Vec<_>>>()?
    } else if text.is_empty() {
        vec![]
    } else {
        text.split(',').map(|v| v.trim().parse::<i64>()
            .map_err(|_| rule("HeroNotFound"))).collect::<Result<Vec<_>>>()?
    };
    if heroes.len() > 4 || heroes.iter().any(|id| *id <= 0 || *id > i32::MAX as i64)
        || heroes.iter().collect::<BTreeSet<_>>().len() != heroes.len() {
        return Err(rule("HeroNotFound"));
    }
    Ok(heroes)
}
fn eclipse_hero_string(heroes: &[i64]) -> String {
    heroes.iter().map(i64::to_string).collect::<Vec<_>>().join(",")
}
pub(super) async fn cached_hero(db: &mut SqliteConnection, a: i64, id: i32) -> Result<Value> {
    let mut hero = hero::info(db, a, id).await?;
    for part in 1..=10 {
        let slot = n(&hero, &format!("EquipItemSlotIndex{part}"));
        let item = sqlx::query("SELECT * FROM equip_items WHERE account_id=? AND slot_index=?")
            .bind(a)
            .bind(slot)
            .fetch_optional(&mut *db)
            .await?;
        hero[format!("EquipItemInfo{part}")] = item
            .as_ref()
            .map(|r| json!(crate::models::equip::EquipItemInfo::from_row(r)))
            .unwrap_or(Value::Null);
    }
    hero["PunishmentRuneOptionInfos"] = json!([]);
    Ok(hero)
}
pub(super) async fn opponent(db: &mut SqliteConnection, a: i64, exclude: i64) -> Result<Value> {
    let selected:Option<i64>=sqlx::query_scalar("SELECT account_id FROM heroes WHERE account_id!=? AND account_id!=? GROUP BY account_id ORDER BY RANDOM() LIMIT 1").bind(a).bind(exclude).fetch_optional(&mut *db).await?;
    let account = selected.unwrap_or(a);
    let ids: Vec<i32> = sqlx::query_scalar(
        "SELECT hero_index FROM heroes WHERE account_id=? ORDER BY RANDOM() LIMIT 4",
    )
    .bind(account)
    .fetch_all(&mut *db)
    .await?;
    let mut heroes = serde_json::Map::new();
    for id in ids {
        heroes.insert(id.to_string(), cached_hero(db, account, id).await?);
    }
    let user=sqlx::query("SELECT a.nick,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u ON a.account_id=u.account_id WHERE a.account_id=?").bind(account).fetch_one(db).await?;
    Ok(
        json!({"UserInfo":{"AccountId":if account == a {-a} else {account},"Nick":user.get::<String,_>("nick"),"TeamLevel":user.get::<i64,_>("team_level"),"AvatarHeroIndex":user.get::<i64,_>("avatar_hero_index"),"MatchScore":0,"SeasonWin":0,"SeasonLose":0},"HeroInfos":heroes,"AiHeroInfos":{},"GroupHeroInfos":{},"DeckInfos":{}}),
    )
}
async fn nodes(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Vec<Value>> {
    let mut result = vec![];
    for v in s.tables.battle.rows("OrdealArenaNode") {
        let account = if n(v, "NodeType") == 2 {
            opponent(db, a, 0).await?
        } else {
            Value::Null
        };
        result.push(json!({"Index":n(v,"Index"),"Floor":n(v,"Floor"),"NodeType":(["Start","Event","Battle","End"].get(n(v,"NodeType") as usize).unwrap_or(&"Start")),"EventIndex":if n(v,"NodeType")==1{1}else{0},"Rank":0,"AccountInfo":account,"CountryCode":""}));
    }
    Ok(result)
}
fn buffs(s: &AppState) -> Vec<i64> {
    let mut pool = s
        .tables
        .battle
        .rows("OrdealArenaBuff")
        .iter()
        .map(|v| n(v, "Index"))
        .collect::<Vec<_>>();
    use rand::seq::SliceRandom;
    pool.shuffle(&mut rand::thread_rng());
    pool.truncate(3);
    pool
}
async fn ordeal(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    let mut info = get(db, a, "ordeal", 0).await?;
    let period = seasons::season(s).0;
    if info.is_null() || n(&info, "Period") != period {
        info = json!({"SelectedTier":0,"MatchScore":0,"OrdealNodeInfos":nodes(db,s,a).await?,"ClearNodeIndices":[1],"DeadHeroIndices":[],"RemainRefreshCount":s.tables.hero_shop.constant("OrdealArenaChangeOpponent",5),"SelectableBuffIndices":[],"SelectedBuffIndices":[],"Period":period,"Selected":false,"ActiveNode":0});
    }
    // The client keys BattlePlayerContainer by account ID; a mirror opponent
    // must not reuse the local player's ID or its creatures never spawn.
    // Repair saved maps too, without resetting their progress or hero snapshots.
    if let Some(nodes) = info["OrdealNodeInfos"].as_array_mut() {
        for node in nodes {
            if n(&node["AccountInfo"]["UserInfo"], "AccountId") == a {
                node["AccountInfo"]["UserInfo"]["AccountId"] = json!(-a);
            }
        }
    }
    let mut out = item::success();
    match action {
        "ordeal_info" => {}
        "select_tier" => {
            let tier = int(r, "Tier")?;
            let row = row(s, "OrdealArenaTier", &[("MatchTierType", tier)])?;
            if info["Selected"] == true {
                return Err(rule("NotAvailableNodeIndex"));
            }
            info["SelectedTier"] = json!(tier);
            info["MatchScore"] = json!(n(row, "StartRating"));
            info["Selected"] = json!(true);
        }
        "select_buff" => {
            let id = int(r, "BuffIndex")?;
            if !info["SelectableBuffIndices"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(id)))
            {
                return Err(rule("NotFoundSelectBuff"));
            }
            info["SelectedBuffIndices"]
                .as_array_mut()
                .unwrap()
                .push(json!(id));
            info["SelectableBuffIndices"] = json!([]);
        }
        "resurrection_hero" => {
            let id = int(r, "HeroIndex")?;
            let dead = info["DeadHeroIndices"].as_array_mut().unwrap();
            if !dead.contains(&json!(id)) {
                return Err(rule("NotAvailableHero"));
            }
            let c = hero::currency(
                db,
                a,
                "Gem",
                -s.tables
                    .hero_shop
                    .constant("OrdealArenaRetireCharacterResetGem", 30),
            )
            .await?;
            dead.retain(|v| *v != id);
            out["CurrencyResults"] = json!([c]);
        }
        "refresh_node" => {
            if info["Selected"] != true || n(&info, "ActiveNode") != 0 {
                return Err(rule("NotAvailableNodeIndex"));
            }
            let id = int(r, "NodeIndex")?;
            let node = row(s, "OrdealArenaNode", &[("Index", id)])?;
            if n(node, "NodeType") != 2
                || info["ClearNodeIndices"]
                    .as_array()
                    .unwrap()
                    .contains(&json!(id))
            {
                return Err(rule("NotAvailableNodeIndex"));
            }
            let old = info["OrdealNodeInfos"]
                .as_array()
                .unwrap()
                .iter()
                .find(|v| n(v, "Index") == id)
                .unwrap();
            let replacement =
                opponent(db, a, n(&old["AccountInfo"]["UserInfo"], "AccountId")).await?;
            if replacement == old["AccountInfo"] {
                return Err(rule("NotAvailableNodeIndex"));
            }
            if boolean(r, "UseGem", false)? {
                out["CurrencyResults"] = json!([hero::currency(
                    db,
                    a,
                    "Gem",
                    -s.tables
                        .hero_shop
                        .constant("OrdealArenaChangeOpponentGem", 100)
                )
                .await?]);
            } else {
                if n(&info, "RemainRefreshCount") <= 0 {
                    return Err(rule("NotAvailableNodeIndex"));
                }
                info["RemainRefreshCount"] = json!(n(&info, "RemainRefreshCount") - 1);
            }
            let node = info["OrdealNodeInfos"]
                .as_array_mut()
                .unwrap()
                .iter_mut()
                .find(|v| n(v, "Index") == id)
                .unwrap();
            node["EventIndex"] = json!(0);
            node["AccountInfo"] = replacement;
            out["OrdealNodeInfos"] = node.clone();
        }
        "begin_ordeal_arena" => {
            let id = int(r, "NodeIndex")?;
            let node = row(s, "OrdealArenaNode", &[("Index", id)])?;
            if n(node, "NodeType") != 2 {
                return Err(rule("NotAvailableNodeIndex"));
            }
            if info["Selected"] != true
                || n(&info, "ActiveNode") != 0
                || !info["SelectableBuffIndices"].as_array().unwrap().is_empty()
            {
                return Err(rule("NotAvailableNodeIndex"));
            }
            let cleared = info["ClearNodeIndices"].as_array().unwrap();
            let max = s
                .tables
                .battle
                .rows("OrdealArenaNode")
                .iter()
                .filter(|v| cleared.contains(&v["Index"]))
                .map(|v| n(v, "Floor"))
                .max()
                .unwrap_or(0);
            if n(node, "Floor") != max + 1 || cleared.contains(&json!(id)) {
                return Err(rule("NotAvailableNodeIndex"));
            }
            let heroes = ids(r, "HeroIndices", 4)?;
            if heroes.is_empty() {
                return Err(rule("InvalidHero"));
            }
            owned(db, a, &heroes).await?;
            dispatch::ensure_available(db, a, &heroes, None).await?;
            if heroes.iter().any(|id| {
                info["DeadHeroIndices"]
                    .as_array()
                    .unwrap()
                    .contains(&json!(id))
            }) {
                return Err(rule("NotAvailableHero"));
            }
            info["ActiveNode"] = json!(id);
            info["ActiveHeroes"] = json!(heroes);
            info["Started"] = json!(now());
        }
        "end_ordeal_arena" => {
            let id = int(r, "NodeIndex")?;
            let node = row(s, "OrdealArenaNode", &[("Index", id)])?;
            let battle = n(node, "NodeType") == 2;
            if info["Selected"] != true
                || (battle && n(&info, "ActiveNode") != id)
                || int(r, "ChapterIndex")? != n(node, "ChapterIndex")
                || int(r, "DungeonIndex")? != n(node, "DungeonIndex")
            {
                return Err(rule("NotAvailableNodeIndex"));
            }
            if !battle {
                let cleared = info["ClearNodeIndices"].as_array().unwrap();
                let max = s
                    .tables
                    .battle
                    .rows("OrdealArenaNode")
                    .iter()
                    .filter(|v| cleared.contains(&v["Index"]))
                    .map(|v| n(v, "Floor"))
                    .max()
                    .unwrap_or(0);
                if n(&info, "ActiveNode") != 0
                    || n(node, "Floor") != max + 1
                    || !info["SelectableBuffIndices"].as_array().unwrap().is_empty()
                {
                    return Err(rule("NotAvailableNodeIndex"));
                }
            }
            let heroes = ids(r, "HeroIndices", 4)?;
            if battle
                && (info["ActiveHeroes"] != json!(heroes)
                    || now() - n(&info, "Started") > settings(s, "BattleExpirySeconds", 14400))
            {
                return Err(rule("InvalidHero"));
            }
            let won = boolean(r, "Completed", false)?;
            if won {
                info["ClearNodeIndices"]
                    .as_array_mut()
                    .unwrap()
                    .push(json!(id));
                let points = if battle {
                    settings(s, "OrdealArenaWinPoint", 100)
                } else {
                    0
                };
                info["MatchScore"] = json!(n(&info, "MatchScore") + points);
                out["AddMatchScore"] = json!(points);
                out["ResultMatchScore"] = info["MatchScore"].clone();
                out["PointResult"] = json!(points);
                if battle {
                    // Native results apply this currency separately from arena
                    // rating. The base win award uses the existing local rule;
                    // HP/time/survival bonuses still need recovered formulas.
                    out["CurrencyResult"] =
                        hero::currency(db, a, "OrdealArenaPoint", points).await?;
                }
                info["SelectableBuffIndices"] = if n(node, "NodeType") == 3 {
                    json!([])
                } else {
                    json!(buffs(s))
                };
                let area = n(node, "RewardIndex");
                if area > 0 {
                    if let Some(reward) = s.tables.battle.find(
                        "OrdealArenaRewardArea",
                        &[("Index", area), ("TierType", n(&info, "SelectedTier"))],
                    ) {
                        let mut rewards = Rewards::default();
                        reward_index(db, s, a, n(reward, "RewardIndex"), &mut rewards).await?;
                        out["AreaRewardInfo"] = super::rewards(db, s, a, rewards).await?;
                    }
                }
                if n(node, "NodeType") == 3 {
                    if let Some(reward) = s
                        .tables
                        .battle
                        .rows("OrdealArenaRewardRating")
                        .iter()
                        .find(|v| {
                            n(v, "TierType") == n(&info, "SelectedTier")
                                && n(&info, "MatchScore")
                                    >= v["RatingArrange"][0].as_i64().unwrap_or(i64::MAX)
                                && n(&info, "MatchScore")
                                    <= v["RatingArrange"][1].as_i64().unwrap_or(-1)
                        })
                    {
                        let mut earned = Rewards::default();
                        reward_index(db, s, a, n(reward, "RewardIndex"), &mut earned).await?;
                        let result = super::rewards(db, s, a, earned).await?;
                        if out["AreaRewardInfo"].is_null() {
                            out["AreaRewardInfo"] = result;
                        } else {
                            dungeons::append_rewards(&mut out["AreaRewardInfo"], &result);
                        }
                    }
                }
            } else {
                let dead = info["DeadHeroIndices"].as_array_mut().unwrap();
                for h in heroes {
                    if !dead.contains(&json!(h)) {
                        dead.push(json!(h));
                    }
                }
            }
            info["ActiveNode"] = json!(0);
        }
        _ => return Err(rule("ContentsDisabled")),
    }
    put(db, a, "ordeal", 0, &info).await?;
    for key in [
        "SelectedTier",
        "MatchScore",
        "ClearNodeIndices",
        "DeadHeroIndices",
        "RemainRefreshCount",
        "SelectableBuffIndices",
        "SelectedBuffIndices",
    ] {
        out[key] = info[key].clone();
    }
    if action != "refresh_node" {
        // The native client treats a nonempty node list as a selected tier.
        // Publishing the generated map early skips select_tier entirely.
        out["OrdealNodeInfos"] = if info["Selected"] == true {
            info["OrdealNodeInfos"].clone()
        } else {
            json!([])
        };
    }
    Ok(out)
}
