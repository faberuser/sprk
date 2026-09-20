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
    let mut out = item::success();
    if action == "reset_all_customizing_costumes" {
        for (key, v) in appearance.as_object_mut().unwrap() {
            if key != "HeroIndex" {
                *v = json!(0);
            }
        }
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
            || !parts.insert(part)
            || data["UnableHero"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(hero_id)))
        {
            return Err(rule("InvalidCostume"));
        }
        let ownership_key = accessory_key(hero_id, id)?;
        let mut owned = get(db, account, "accessory", ownership_key).await?;
        if owned.is_null() {
            if action != "buy_customizing_costumes"
                || data["IsOpen"] != true
                || data["IsBuy"] != true
            {
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
        appearance[format!("AccessoryCostumeIndex{part}")] = json!(id);
    }
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
pub(super) fn positions(raw: &str) -> Result<Vec<(i64, Value)>> {
    if raw.is_empty() {
        return Ok(vec![]);
    }
    if raw.len() > 16384 {
        return Err(rule("InvalidCostume"));
    }
    // RequestInternal calls WWW.EscapeURL before WWWForm encodes the body.
    // Form parsing removes the outer layer; native position JSON still has one.
    // Parse plain JSON first so already-decoded values are never decoded twice.
    let value: Value = match serde_json::from_str(raw) {
        Ok(value) => value,
        Err(_) => {
            let decoded = urlencoding::decode(raw).map_err(|_| rule("InvalidCostume"))?;
            serde_json::from_str(&decoded).map_err(|_| rule("InvalidCostume"))?
        }
    };
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
}
