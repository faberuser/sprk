use super::*;

// PunishmentRaidContext uses the native contents contract and OfflinePlay.
// Adapt it to the shared transactional lifecycle without changing client combat.
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    if int(r, "DungeonType")? == 1 {
        return karma::execute(db, s, a, r, action).await;
    }
    if !matches!(action, "begin_content" | "end_content") || int(r, "DungeonType")? != 2 {
        return Err(rule("ContentsDisabled"));
    }
    let group = int(r, "GroupIndex")?;
    let stage = campaign::dungeon(s, r)?;
    if n(stage, "BattleType") != 47 {
        return Err(rule("DungeonNotFound"));
    }
    let opened = get(db, a, "punishment_open", group).await?;
    if n(&opened, "GroupIndex") != group || n(&opened, "DungeonType") != 2 {
        return Err(rule("RaidNotStarted"));
    }
    let level = if action == "begin_content" {
        int(r, "Level")?
    } else {
        n(&opened, "OpenLevel")
    };
    let raid = s
        .tables
        .battle
        .rows("Raid")
        .iter()
        .find(|v| {
            n(v, "ChapterIndex") == n(stage, "ChapterIndex")
                && n(v, "DungeonIndex") == n(stage, "DungeonIndex")
                && n(v, "Level") == level
        })
        .ok_or_else(|| rule("DungeonNotFound"))?;
    let def = row(
        s,
        "PunishmentRaid",
        &[("RaidIndex", n(raid, "Index")), ("RaidLevel", level)],
    )?;
    if n(def, "GroupIndex") != group {
        return Err(rule("DungeonNotFound"));
    }
    let mut request = Request(r.0.clone());
    request
        .0
        .insert("RaidIndex".into(), n(raid, "Index").to_string());
    request.0.insert("RaidLevel".into(), level.to_string());
    let mut out = if action == "begin_content" {
        // Native Squard includes both owned teams in HeroIndices. For this
        // contract AiHeroIndices identifies the sub team; it is not an AI roster.
        let heroes = ids(r, "HeroIndices", 32)?;
        let sub = ids(r, "AiHeroIndices", 32)?;
        if sub.iter().collect::<BTreeSet<_>>().len() != sub.len()
            || sub.iter().any(|id| !heroes.contains(id))
            || sub.len() as i64 > n(raid, "SubPartyCount")
            || (heroes.len() - sub.len()) as i64 > n(raid, "MainPartyCount")
            || !ids(r, "GroupHeroIndices", 32)?.is_empty()
        {
            return Err(rule("NotMatchHeroIndices"));
        }
        if int(r, "FlaskItemIndex")? != 0
            || int(r, "FlaskItemCount")? != 0
        {
            return Err(rule("ContentsDisabled"));
        }
        // Revalidate modifiers even when the shared lifecycle returns an entry retry.
        dungeons::validate(db, s, a, &request, stage).await?;
        campaign::begin(db, s, a, &request).await?
    } else {
        campaign::end(db, s, a, &request).await?
    };
    if action == "end_content" {
        // The client Set method replaces its complete cross-group clear cache.
        out["PunishmentRaidInfos"] = json!(list(db, a, "punishment_raid").await?);
        out["OpenPunishmentRaidInfos"] = json!([get(db, a, "punishment_open", group).await?]);
    }
    Ok(out)
}
