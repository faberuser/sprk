use super::*;

fn class_name(value: &str) -> Option<i64> {
    [
        "None", "Warrior", "Archer", "Knight", "Wizard", "Priest", "Assassin", "Mechanic",
    ]
    .iter()
    .position(|v| v.eq_ignore_ascii_case(value))
    .map(|v| v as i64)
}
fn attribute(value: &str) -> Option<i64> {
    ["None", "Physical", "Magical"]
        .iter()
        .position(|v| v.eq_ignore_ascii_case(value))
        .map(|v| v as i64)
}

/// Apply the same class requirements and bans used by the client party screen.
/// Sweeps have no party: eligibility is established by their previous clear.
pub(super) fn party(s: &AppState, r: &Request, rule_index: i64) -> Result<()> {
    if rule_index == 0 || r.0.contains_key("SweepCount") {
        return Ok(());
    }
    let definition = row(s, "BanRule", &[("Index", rule_index)])?;
    let main = ids(r, "HeroIndices", 32)?;
    let mut all = main.clone();
    all.extend(ids(r, "GroupHeroIndices", 32)?);
    let heroes = all
        .iter()
        .map(|id| row(s, "BattleHero", &[("Index", *id)]))
        .collect::<Result<Vec<_>>>()?;
    let mut main_classes = main
        .iter()
        .map(|id| row(s, "BattleHero", &[("Index", *id)]).map(|v| n(v, "TagType")))
        .collect::<Result<Vec<_>>>()?;
    for required in definition["SelectClassType"]
        .as_array()
        .into_iter()
        .flatten()
    {
        let index = main_classes
            .iter()
            .position(|v| Some(*v) == required.as_i64())
            .ok_or_else(|| rule("NotMatchHeroIndices"))?;
        main_classes.remove(index);
    }
    if let Some(allowed) = definition["AcceptedClassType"]
        .as_array()
        .filter(|v| !v.is_empty())
    {
        if heroes.iter().any(|h| !allowed.contains(&h["TagType"])) {
            return Err(rule("NotAvailableHero"));
        }
    }
    for index in 1..=3 {
        let kind = n(definition, &format!("BanType{index}"));
        let values = definition[format!("BanValue{index}")]
            .as_str()
            .unwrap_or("")
            .split(',')
            .map(str::trim)
            .collect::<Vec<_>>();
        match kind {
            0 => {}
            1 => {
                let class = class_name(values[0]).ok_or_else(|| rule("ItemDataNotFound"))?;
                if heroes.iter().any(|h| n(h, "TagType") == class) {
                    return Err(rule("NotAvailableHero"));
                }
            }
            2 => {
                let attr = attribute(values[0]).ok_or_else(|| rule("ItemDataNotFound"))?;
                if heroes.iter().any(|h| {
                    h["AttrType"]
                        .as_array()
                        .is_some_and(|v| v.contains(&json!(attr)))
                }) {
                    return Err(rule("NotAvailableHero"));
                }
            }
            3 => {
                if heroes
                    .iter()
                    .any(|h| values.contains(&h["CodeName"].as_str().unwrap_or("")))
                {
                    return Err(rule("NotAvailableHero"));
                }
            }
            4 | 5 => {
                let limit = values[0]
                    .parse::<usize>()
                    .map_err(|_| rule("ItemDataNotFound"))?;
                if values.len() < 2 {
                    return Err(rule("ItemDataNotFound"));
                }
                let allowed = if values[1].eq_ignore_ascii_case("All") {
                    (1..=7).collect::<Vec<_>>()
                } else {
                    values[1..]
                        .iter()
                        .map(|v| {
                            if kind == 4 {
                                class_name(v)
                            } else {
                                attribute(v)
                            }
                        })
                        .collect::<Option<Vec<_>>>()
                        .ok_or_else(|| rule("ItemDataNotFound"))?
                };
                for value in allowed {
                    let count = heroes
                        .iter()
                        .filter(|h| {
                            if kind == 4 {
                                n(h, "TagType") == value
                            } else {
                                h["AttrType"]
                                    .as_array()
                                    .is_some_and(|v| v.contains(&json!(value)))
                            }
                        })
                        .count();
                    if count > limit {
                        return Err(rule("NotMatchHeroIndices"));
                    }
                }
            }
            _ => return Err(rule("ContentsDisabled")),
        }
    }
    Ok(())
}

pub(super) fn creatures(end: &Request, party: &[i64]) -> Result<Vec<Value>> {
    let values: Vec<Value> = read_json(end.text("CreatureInfoString"))
        .map_err(|_| rule("ParsingTowerCreatureInfoError"))?;
    if values.len() > 512 {
        return Err(rule("ParsingTowerCreatureInfoError"));
    }
    let mut keys = BTreeSet::new();
    for value in &values {
        let integer = |key: &str| {
            value[key]
                .as_i64()
                .filter(|v| *v >= 0)
                .ok_or_else(|| rule("ParsingTowerCreatureInfoError"))
        };
        let id = integer("Index")?;
        let team = integer("TeamId")?;
        if id == 0
            || !matches!(team, 0 | 1)
            || (team == 0 && !party.contains(&id))
            || integer("Hp")? > integer("MaxHp")?
            || integer("Mp")? > integer("MaxMp")?
        {
            return Err(rule("ParsingTowerCreatureInfoError"));
        }
        let key = value["Key"]
            .as_str()
            .ok_or_else(|| rule("ParsingTowerCreatureInfoError"))?;
        let pieces = key
            .split('_')
            .map(str::parse::<i64>)
            .collect::<std::result::Result<Vec<_>, _>>()
            .map_err(|_| rule("ParsingTowerCreatureInfoError"))?;
        if pieces.len() != 3
            || pieces[0] != id
            || pieces[2] != team
            || pieces[1] < 0
            || !keys.insert(key)
        {
            return Err(rule("ParsingTowerCreatureInfoError"));
        }
    }
    if party.iter().any(|id| {
        !values
            .iter()
            .any(|v| n(v, "TeamId") == 0 && n(v, "Index") == *id)
    }) {
        return Err(rule("ParsingTowerCreatureInfoError"));
    }
    Ok(values)
}
