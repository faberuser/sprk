//! SPRK auto-battle skill rules (Hero Info → Skills: rank 1-3, Normal, Never use), stored per
//! account and hero so the native battle worker can apply them to AI-driven heroes in online
//! battles (raids, parties, arena). The client keeps its own copy and syncs it here: one hero
//! when a rule changes, and a full replacement once per session.
//!
//! Rows live in `extension_state` (kind `skill_rules`, idx = hero index, data
//! `{"HeroIndex":h,"Rules":r}`). `r` is the client's 5-slot, 3-bit encoding.
use crate::api::{inventory::item::{self, n, rule}, system::request::Request};
use crate::error::Result;
use serde_json::{json, Map, Value};
use sqlx::PgConnection;
use std::collections::{BTreeMap, BTreeSet};

const KIND: &str = "skill_rules";
const MAX_HEROES: usize = 400;

pub(crate) fn valid(rules: i64) -> bool {
    (1..=0x7fff).contains(&rules)
}

/// `{"heroIndex": rules}` from the client, accepting the doubly escaped form Unity sends.
/// `None` when the field is not a JSON object.
pub(crate) fn parse(raw: &str) -> Option<BTreeMap<i64, i64>> {
    let parsed = serde_json::from_str::<Value>(raw)
        .ok()
        .or_else(|| serde_json::from_str::<Value>(&urlencoding::decode(raw).ok()?).ok())?;
    let Value::Object(map) = parsed else { return None };
    let mut out = BTreeMap::new();
    for (hero, value) in map {
        let hero = hero.parse::<i64>().ok()?;
        let value = value.as_i64().or_else(|| value.as_str().and_then(|v| v.parse().ok()))?;
        if hero <= 0 || !(value == 0 || valid(value)) {
            return None;
        }
        out.insert(hero, value);
    }
    Some(out)
}

/// Every stored rule of an account, as the worker expects it: `{"heroIndex": rules}`.
pub(crate) async fn all(db: &mut PgConnection, account: i64) -> Result<Map<String, Value>> {
    let mut out = Map::new();
    for row in crate::api::extensions::list(db, account, KIND).await? {
        let (hero, rules) = (n(&row, "HeroIndex"), n(&row, "Rules"));
        if hero > 0 && valid(rules) {
            out.insert(hero.to_string(), json!(rules));
        }
    }
    Ok(out)
}

/// `hero/set_skill_rules`: `SkillRules` = `{"heroIndex": rules}` (0 clears a hero). With
/// `Replace=1` the map becomes the account's complete set. Only owned heroes are stored.
pub(crate) async fn set(db: &mut PgConnection, account: i64, req: &Request) -> Result<Value> {
    let wanted = parse(req.text("SkillRules")).ok_or_else(|| rule("InvalidHeroIndex"))?;
    if wanted.len() > MAX_HEROES {
        return Err(rule("InvalidHeroIndex"));
    }
    let replace = matches!(req.text("Replace"), "1" | "true" | "True");
    let owned: BTreeSet<i64> = crate::db::query_scalar::<i64>("SELECT hero_index FROM heroes WHERE account_id=$1")
        .bind(account)
        .fetch_all(&mut *db)
        .await?
        .into_iter()
        .collect();
    let current: BTreeMap<i64, i64> = all(db, account)
        .await?
        .into_iter()
        .filter_map(|(hero, rules)| Some((hero.parse().ok()?, rules.as_i64()?)))
        .collect();
    let mut target = if replace { BTreeMap::new() } else { current.clone() };
    for (hero, rules) in wanted {
        if !owned.contains(&hero) {
            continue;
        }
        if rules == 0 {
            target.remove(&hero);
        } else {
            target.insert(hero, rules);
        }
    }
    // Writes bump the account's response revisions, so only changed rows are touched.
    for hero in current.keys().filter(|hero| !target.contains_key(hero)) {
        crate::db::query("DELETE FROM extension_state WHERE account_id=$1 AND kind=$2 AND idx=$3")
            .bind(account)
            .bind(KIND)
            .bind(hero)
            .execute(&mut *db)
            .await?;
    }
    for (hero, rules) in &target {
        if current.get(hero) != Some(rules) {
            crate::api::extensions::put(db, account, KIND, *hero, &json!({"HeroIndex":hero,"Rules":rules})).await?;
        }
    }
    Ok(item::success())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn parse_accepts_plain_and_escaped_maps_and_rejects_bad_values() {
        assert_eq!(parse(r#"{"1":13,"7":0}"#), Some(BTreeMap::from([(1, 13), (7, 0)])));
        assert_eq!(parse("%7b%221%22%3a185%7d"), Some(BTreeMap::from([(1, 185)])));
        assert_eq!(parse(r#"{"1":"13"}"#), Some(BTreeMap::from([(1, 13)])));
        assert_eq!(parse("{}"), Some(BTreeMap::new()));
        assert_eq!(parse(r#"{"1":32768}"#), None);
        assert_eq!(parse(r#"{"1":-1}"#), None);
        assert_eq!(parse(r#"{"x":3}"#), None);
        assert_eq!(parse(r#"{"0":3}"#), None);
        assert_eq!(parse("[1]"), None);
        assert_eq!(parse("not json"), None);
    }
}
