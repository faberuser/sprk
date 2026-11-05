use super::*;

pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let hero_id = i32::try_from(req.number("HeroIndex", 0)?).map_err(|_| rule("NoHero"))?;
    let h = hero::info(db, account, hero_id).await?;
    let mut details = hero::details(db, account, hero_id).await?;
    let mut appearance = hero::appearance(&h);
    let mut accessories = equipped_accessories(db, account, &h).await?;
    let mut out = item::success();
    if action == "reset_all_customizing_costumes" {
        for (key, v) in appearance.as_object_mut().unwrap() {
            if key != "HeroIndex" {
                *v = json!(0);
            }
        }
        appearance["SprkAccessories"] = json!([]);
        for (key, v) in appearance.as_object().unwrap() {
            if key != "HeroIndex" {
                details[key] = v.clone();
            }
        }
        hero::save_details(db, account, hero_id, &details).await?;
        out["HeroCostumeResult"] = appearance;
        return Ok(out);
    }
    let mut gold = 0;
    let mut gem = 0;
    let mut mileage = 0;
    let mut new_body = vec![];
    if action == "buy_customizing_costumes" {
        let id = i32::try_from(req.number("CostumeIndex", n(&h, "CostumeIndex"))?)
            .map_err(|_| rule("InvalidCostume"))?;
        if id > 0 {
            let c = state
                .tables
                .hero_shop
                .costumes
                .get(&id)
                .ok_or_else(|| rule("CostumeDataNotFound"))?;
            if n(c, "HeroIndex") != hero_id as i64 {
                return Err(rule("NotCorrectHero"));
            }
            if c["IsDefault"] != true && !owned_body(db, account, id as i64).await? {
                if c["Buyable"] != true || n(c, "ReqBuyMileage") != 0 {
                    return Err(rule("NotForSale"));
                }
                gold += n(c, "ReqBuyGold");
                gem += n(c, "ReqBuyGem");
                mileage += n(c, "Mileage");
                new_body.push(id as i64);
                new_body.extend(
                    c["BonusCostumeIndices"]
                        .as_array()
                        .into_iter()
                        .flatten()
                        .filter_map(Value::as_i64),
                );
                for b in &new_body {
                    if !state.tables.hero_shop.costumes.contains_key(&(*b as i32)) {
                        return Err(rule("CostumeDataNotFound"));
                    }
                    sqlx::query(
                        "INSERT OR IGNORE INTO costumes(account_id,costume_index) VALUES(?,?)",
                    )
                    .bind(account)
                    .bind(b)
                    .execute(&mut *db)
                    .await?;
                }
            }
            hero::check_costume(db, state, account, &h, id).await?;
        }
        appearance["CostumeIndex"] = json!(id);
        let hide = req.number("HideUniqueWeapon", n(&h, "HideUniqueWeapon"))?;
        if !(0..=1).contains(&hide) {
            return Err(rule("InvalidCostume"));
        }
        appearance["HideUniqueWeapon"] = json!(hide);
        for (table, kind, key) in [
            ("HairCostume", "hair", "HairCostumeIndex"),
            ("WeaponCostume", "weapon", "WeaponCostumeIndex"),
        ] {
            let id = req.number(key, n(&h, key))?;
            if id < 0 {
                return Err(rule("InvalidCostume"));
            }
            if id > 0 {
                let data = row(state, table, &[("Index", id)])?;
                if n(data, "HeroIndex") != hero_id as i64 || n(data, "Star") > n(&h, "Star") {
                    return Err(rule("NotCorrectHero"));
                }
                let mut owned = get(db, account, kind, id).await?;
                if owned.is_null() {
                    let needed: Vec<_> = data["NeedCostume"]
                        .as_array()
                        .into_iter()
                        .flatten()
                        .filter_map(Value::as_i64)
                        .collect();
                    let mut unlocked = data["IsDefault"] == true;
                    for body in needed {
                        unlocked |= owned_body(db, account, body).await?;
                    }
                    if !unlocked && (data["IsOpen"] != true || n(data, "ReqBuyGem") <= 0) {
                        return Err(rule("CostumeNotOwned"));
                    }
                    if !unlocked {
                        gem += n(data, "ReqBuyGem");
                        mileage += n(data, "Mileage");
                    }
                    owned = json!({key:id,"CreatedTime":state.server_time_str()});
                    put(db, account, kind, id, &owned).await?;
                }
                out[format!("{table}Info")] = owned;
            } else {
                out[format!("{table}Info")] = Value::Null;
            }
            appearance[key] = json!(id);
        }
        let unset = ids(req, "UnsetAccessoryCostumeIndices")?;
        accessories.retain(|v| !unset.contains(&n(v, "Index")));
        for id in &unset {
            for slot in 1..=6 {
                let key = format!("AccessoryCostumeIndex{slot}");
                if n(&appearance, &key) == *id {
                    appearance[key] = json!(0);
                }
            }
        }
        out["UnsetAccessoryCostumeIndices"] = json!(unset);
    }
    let positions = positions(req.text("AccessoryCostumePositionInfo"))?;
    let mut result = vec![];
    let mut parts = BTreeSet::new();
    for (id, position) in positions {
        let data = row(state, "AccessoryCostume", &[("Index", id)])?;
        let part = n(data, "PartType");
        if !(1..=6).contains(&part)
            || (!(1..=4).contains(&part) && !parts.insert(part))
            || data["UnableHero"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(hero_id)))
        {
            return Err(rule("InvalidCostume"));
        }
        let ownership_key = accessory_key(hero_id, id)?;
        let mut owned = get(db, account, "accessory", ownership_key).await?;
        if owned.is_null() {
            // All archived accessories are sold directly in this Dressing Room.
            // Keep the table price, compatibility and per-hero ownership intact.
            if action != "buy_customizing_costumes" {
                return Err(rule("CostumeNotOwned"));
            }
            gem += n(data, "ReqBuyGem");
            mileage += n(data, "Mileage");
            owned = json!({"HeroIndex":0,"AccessoryCostumeIndex":id,"CreatedTime":state.server_time_str()});
        }
        owned["HeroIndex"] = json!(hero_id);
        owned["PositionInfo"] = json!(position.to_string());
        put(db, account, "accessory", ownership_key, &owned).await?;
        result.push(owned);
        if action == "edit_accessory_costume_position"
            && !accessories.iter().any(|v| n(v, "Index") == id)
        {
            return Err(rule("InvalidCostume"));
        }
        equip_accessory(&mut accessories, id, part, &position.to_string());
        appearance[format!("AccessoryCostumeIndex{part}")] = json!(id);
    }
    // Native scalar fields retain the last accessory in each part for older consumers.
    for part in 1..=6 {
        appearance[format!("AccessoryCostumeIndex{part}")] = json!(accessories.iter()
            .rev().find(|v| n(v, "Part") == part).map_or(0, |v| n(v, "Index")));
    }
    appearance["SprkAccessories"] = json!(accessories);
    if action == "buy_customizing_costumes" {
        if req.number("BuyGold", 0)? != gold || req.number("BuyGem", 0)? != gem {
            return Err(rule("InvalidPrice"));
        }
        let mut currencies = vec![];
        for (kind, delta) in [("Gold", -gold), ("Gem", -gem), ("Mileage", mileage)] {
            if delta != 0 {
                currencies.push(hero::currency(db, account, kind, delta).await?);
            }
        }
        out["CurrencyResults"] = json!(currencies);
        out["CostumeInfos"] = json!(hero::costumes(db, account)
            .await?
            .into_iter()
            .filter(|v| new_body.contains(&n(v, "CostumeIndex")))
            .collect::<Vec<_>>());
        out["HeroCostumeResultInfo"] = appearance.clone();
    }
    out["PlayerAccessoryCostumeInfos"] = json!(result);
    for (key, v) in appearance.as_object().unwrap() {
        if key != "HeroIndex" {
            details[key] = v.clone();
        }
    }
    hero::save_details(db, account, hero_id, &details).await?;
    Ok(out)
}
pub(crate) fn equip_accessory(accessories: &mut Vec<Value>, id: i64, part: i64, position: &str) {
    accessories.retain(|v| n(v, "Index") != id && ((1..=4).contains(&part) || n(v, "Part") != part));
    accessories.push(json!({"Index":id,"Part":part,"PositionInfo":position}));
}

pub(crate) async fn equipped_accessories(db: &mut SqliteConnection, account: i64, h: &Value) -> Result<Vec<Value>> {
    if let Some(values) = h["SprkAccessories"].as_array() {
        return Ok(values.clone());
    }
    // Migrate existing single-slot appearances lazily without changing ownership.
    let owned = super::list(db, account, "accessory").await?;
    let mut result = Vec::new();
    for part in 1..=6 {
        let id = n(h, &format!("AccessoryCostumeIndex{part}"));
        if id > 0 {
            let position = owned.iter().find(|v| n(v,"HeroIndex") == n(h,"HeroIndex") && n(v,"AccessoryCostumeIndex") == id)
                .and_then(|v| v["PositionInfo"].as_str()).unwrap_or("");
            equip_accessory(&mut result, id, part, position);
        }
    }
    Ok(result)
}

const IDENTITY_TRANSFORM: [(&str, f64); 7] = [("PositionX", 0.0), ("PositionY", 0.0), ("PositionZ", 0.0),
    ("RotationX", 0.0), ("RotationY", 0.0), ("RotationZ", 0.0), ("Scale", 1.0)];

// A stored native transform string as a PositionInfo object. Missing or
// unusable values become the identity transform, never a zero scale.
fn transform(raw: &str) -> Value {
    let parsed = position_json(raw).ok().filter(Value::is_object);
    let mut out = serde_json::Map::new();
    for (field, default) in IDENTITY_TRANSFORM {
        let value = parsed.as_ref().map(|v| &v[field])
            .and_then(|v| v.as_f64().or_else(|| v.as_str().and_then(|s| s.parse::<f64>().ok())))
            .filter(|v| v.is_finite() && (field != "Scale" || *v > 0.0))
            .unwrap_or(default);
        // Native PositionInfo JSON encodes floats as strings.
        out.insert(field.into(), json!(value.to_string()));
    }
    Value::Object(out)
}

// Hero snapshots shown to other clients or replayed by the native battle
// worker. The worker re-encodes absent native transforms as `{}`, which
// clients parse as scale 0 and render invisibly, so send the complete
// accessory list and explicit transforms for every occupied native slot.
pub(crate) async fn complete_snapshot_appearance(db: &mut SqliteConnection, account: i64, h: &mut Value) -> Result<()> {
    let accessories = equipped_accessories(db, account, h).await?;
    for part in 1..=6 {
        let id = n(h, &format!("AccessoryCostumeIndex{part}"));
        if id <= 0 {
            continue;
        }
        let raw = accessories.iter().find(|v| n(v, "Index") == id)
            .and_then(|v| v["PositionInfo"].as_str()).unwrap_or("");
        h[format!("AccessoryPositionInfo{part}")] = transform(raw);
    }
    h["SprkAccessories"] = json!(accessories);
    Ok(())
}

// The native accessory key contains both the hero and accessory IDs.
pub(super) fn accessory_key(hero_id: i32, accessory: i64) -> Result<i64> {
    if hero_id <= 0 || !(1..=i32::MAX as i64).contains(&accessory) {
        return Err(rule("InvalidCostume"));
    }
    Ok(((hero_id as i64) << 32) | accessory)
}

async fn owned_body(db: &mut SqliteConnection, account: i64, id: i64) -> Result<bool> {
    Ok(sqlx::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM costumes WHERE account_id=? AND costume_index=?)",
    )
    .bind(account)
    .bind(id)
    .fetch_one(db)
    .await?)
}
pub(super) fn position_json(raw: &str) -> Result<Value> {
    if raw.len() > 16384 {
        return Err(rule("InvalidCostume"));
    }
    // RequestInternal calls WWW.EscapeURL before WWWForm encodes the body.
    // Form parsing removes the outer layer; native position JSON still has one.
    // Parse plain JSON first so already-decoded values are never decoded twice.
    match serde_json::from_str(raw) {
        Ok(value) => Ok(value),
        Err(_) => {
            let decoded = urlencoding::decode(raw).map_err(|_| rule("InvalidCostume"))?;
            serde_json::from_str(&decoded).map_err(|_| rule("InvalidCostume"))
        }
    }
}

pub(super) fn positions(raw: &str) -> Result<Vec<(i64, Value)>> {
    if raw.is_empty() {
        return Ok(vec![]);
    }
    let value = position_json(raw)?;
    // BaseJsonMarshaler encodes integer-keyed dictionaries as JSON objects.
    let object = value.as_object().ok_or_else(|| rule("InvalidCostume"))?;
    if object.len() > 6 {
        return Err(rule("InvalidCostume"));
    }
    let mut out = vec![];
    for (key, v) in object {
        let id: i64 = key.parse().map_err(|_| rule("InvalidCostume"))?;
        if id <= 0 {
            return Err(rule("InvalidCostume"));
        }
        for field in [
            "PositionX",
            "PositionY",
            "PositionZ",
            "RotationX",
            "RotationY",
            "RotationZ",
            "Scale",
        ] {
            // The native JSON marshaler emits floats with ToString().
            let number = v[field]
                .as_f64()
                .or_else(|| v[field].as_str().and_then(|s| s.parse::<f64>().ok()))
                .ok_or_else(|| rule("InvalidCostume"))?;
            if !number.is_finite() || (field == "Scale" && number <= 0.0) {
                return Err(rule("InvalidCostume"));
            }
        }
        out.push((id, v.clone()));
    }
    Ok(out)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn accessory_positions_accept_native_strings_and_validate_values() {
        for text in [false, true] {
            let mut position = json!({"PositionX":0.25,"PositionY":-1,"PositionZ":0,
                "RotationX":0,"RotationY":90,"RotationZ":0,"Scale":1});
            if text {
                for value in position.as_object_mut().unwrap().values_mut() {
                    *value = json!(value.to_string());
                }
            }
            let raw = json!({"3100013":position}).to_string();
            assert!(positions(&raw).is_ok());
            let escaped = urlencoding::encode(&raw);
            assert!(positions(&escaped).is_ok());
            // More than the single remaining native escape layer is invalid.
            assert!(positions(&urlencoding::encode(&escaped)).is_err());
            for invalid in [json!("NaN"), json!("inf"), json!("garbage"), json!(null), json!(true)] {
                let mut bad = position.clone();
                bad["PositionX"] = invalid;
                assert!(positions(&json!({"3100013":bad}).to_string()).is_err());
            }
            for invalid in [json!(0), json!(-1), json!("0"), json!("-1")] {
                let mut bad = position.clone();
                bad["Scale"] = invalid;
                assert!(positions(&json!({"3100013":bad}).to_string()).is_err());
            }
        }
    }

    #[test]
    fn snapshot_transforms_never_have_zero_scale() {
        // `{}` is what a native re-encode of an absent transform looks like.
        for raw in ["", "{}", "garbage", r#"{"Scale":"0"}"#, r#"{"Scale":-1}"#] {
            assert_eq!(transform(raw)["Scale"], "1", "{raw}");
            assert_eq!(transform(raw)["PositionX"], "0", "{raw}");
        }
        let custom = transform(r#"{"PositionX":"0.25","PositionY":-1,"PositionZ":"0","RotationX":"0","RotationY":"90","RotationZ":"0","Scale":"1.5"}"#);
        assert_eq!(custom, json!({"PositionX":"0.25","PositionY":"-1","PositionZ":"0",
            "RotationX":"0","RotationY":"90","RotationZ":"0","Scale":"1.5"}));
    }
}
