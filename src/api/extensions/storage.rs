use super::*;

fn blank(index: i64) -> Value {
    let mut v = json!({"EquipStorageSlotIndex":index,"Name":"","LinkInfo":"[]"});
    for i in 1..=10 {
        v[format!("EquipItemSlotIndex{i}")] = json!(0);
    }
    v
}
pub(super) async fn initialize(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
) -> Result<()> {
    let count = state
        .tables
        .extensions
        .rows("StorageSlotExtend")
        .iter()
        .filter(|r| r["IsDefaultOpen"] == true)
        .map(|r| n(r, "SlotCount"))
        .max()
        .unwrap_or(0);
    for i in 1..=count {
        if get(db, account, "equip_storage", i).await?.is_null() {
            put(db, account, "equip_storage", i, &blank(i)).await?;
        }
    }
    Ok(())
}
pub(super) async fn preset(db: &mut SqliteConnection, account: i64, v: &Value) -> Result<Value> {
    let mut slots = vec![];
    let mut uids = vec![];
    for i in 1..=10 {
        let slot = n(v, &format!("EquipItemSlotIndex{i}"));
        slots.push(slot);
        let uid = if slot > 0 {
            equip(db, account, slot).await?.uid
        } else {
            String::new()
        };
        uids.push(uid);
    }
    Ok(
        json!({"EquipStorageSlotIndex":v["EquipStorageSlotIndex"],"Name":v["Name"],"EquipItemSlotIndex":slots,"EquipItemUid":uids}),
    )
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    initialize(db, state, account).await?;
    let all = list(db, account, "equip_storage").await?;
    let mut out = item::success();
    if action == "buy_equip_storage_slot" {
        let next = all
            .iter()
            .map(|r| n(r, "EquipStorageSlotIndex"))
            .max()
            .unwrap_or(0)
            + 1;
        let r = state
            .tables
            .extensions
            .rows("StorageSlotExtend")
            .iter()
            .filter(|r| n(r, "SlotCount") >= next)
            .min_by_key(|r| n(r, "SlotCount"))
            .ok_or_else(|| rule("MaxSlotCount"))?;
        out["CurrencyResult"] = hero::currency(db, account, "Gem", -n(r, "ExtendPrice")).await?;
        let v = blank(next);
        put(db, account, "equip_storage", next, &v).await?;
        out["NewEquipStorageSlotInfo"] = v.clone();
        out["EquipPresetInfo"] = preset(db, account, &v).await?;
        return Ok(out);
    }
    let index = req.number("EquipStorageSlotIndex", 0)?;
    let mut v = get(db, account, "equip_storage", index).await?;
    if v.is_null() {
        return Err(rule("SlotNotFound"));
    }
    match action {
        "change_equip_storage_slot_name" | "add_equip_storage_slot" => {
            let name = req.text("Name").trim();
            if name.chars().count() > 40 || name.chars().any(char::is_control) {
                return Err(rule("InvalidName"));
            }
            v["Name"] = json!(name);
            if action == "add_equip_storage_slot" {
                let hero_id =
                    i32::try_from(req.number("HeroIndex", 0)?).map_err(|_| rule("NoHero"))?;
                let h = hero::info(db, account, hero_id).await?;
                let parts: Vec<i64> = serde_json::from_str(req.text("HeroPartIndex"))
                    .map_err(|_| rule("InvalidEquipItem"))?;
                let slots = ids(req, "EquipItemSlotIndex")?;
                if slots.is_empty()
                    || parts.len() != slots.len()
                    || parts.iter().collect::<BTreeSet<_>>().len() != parts.len()
                {
                    return Err(rule("InvalidEquipItem"));
                }
                for i in 1..=10 {
                    v[format!("EquipItemSlotIndex{i}")] = json!(0);
                }
                for (part, slot) in parts.into_iter().zip(slots) {
                    if !(0..10).contains(&part) {
                        return Err(rule("InvalidEquipItem"));
                    }
                    let eq = equip(db, account, slot).await?;
                    compatible(state, &h, &eq, part + 1)?;
                    v[format!("EquipItemSlotIndex{}", part + 1)] = json!(slot);
                }
                let old = req.number("OldEquipStorageSlotIndex", 0)?;
                if old > 0 && old != index {
                    let mut previous = get(db, account, "equip_storage", old).await?;
                    if previous.is_null() {
                        return Err(rule("SlotNotFound"));
                    }
                    previous["LinkInfo"] = json!("[]");
                    put(db, account, "equip_storage", old, &previous).await?;
                    out["OldEquipStorageSlotInfo"] = previous;
                } else {
                    out["OldEquipStorageSlotInfo"] = Value::Null;
                }
                v["LinkInfo"] = json!(json!([hero_id]).to_string());
            }
            out["NewName"] = v["Name"].clone();
        }
        "reset_equip_storage_slot" => {
            let name = v["Name"].clone();
            v = blank(index);
            v["Name"] = name;
        }
        "remove_hero_equip_storage_slot" => {
            v["LinkInfo"] = json!("[]");
        }
        "set_equip_storage_slot" => {
            let hero_id = i32::try_from(req.number("HeroIndex", 0)?).map_err(|_| rule("NoHero"))?;
            let h = hero::info(db, account, hero_id).await?;
            let mut equipped = vec![];
            let mut removed = vec![];
            let mut old_removed = vec![];
            let mut old_hero = 0;
            for part in 1..=10 {
                let key = format!("EquipItemSlotIndex{part}");
                let slot = n(&v, &key);
                if slot > 0 {
                    let eq = equip(db, account, slot).await?;
                    compatible(state, &h, &eq, part)?;
                }
            }
            for part in 1..=10 {
                let key = format!("EquipItemSlotIndex{part}");
                let slot = n(&v, &key);
                let previous = n(&h, &key);
                if previous > 0 && previous != slot {
                    removed.push(previous);
                }
                if slot > 0 {
                    for p in 1..=10 {
                        let col = format!("equip_item_slot_index_{p}");
                        let owners:Vec<i32>=sqlx::query_scalar(&format!("SELECT hero_index FROM heroes WHERE account_id=? AND hero_index<>? AND {col}=?")).bind(account).bind(hero_id).bind(slot).fetch_all(&mut *db).await?;
                        if let Some(owner) = owners.first() {
                            old_hero = *owner;
                            old_removed.push(slot);
                        }
                        sqlx::query(&format!(
                            "UPDATE heroes SET {col}=0 WHERE account_id=? AND {col}=?"
                        ))
                        .bind(account)
                        .bind(slot)
                        .execute(&mut *db)
                        .await?;
                    }
                    equipped.push(slot);
                }
                sqlx::query(&format!("UPDATE heroes SET equip_item_slot_index_{part}=? WHERE account_id=? AND hero_index=?")).bind(slot).bind(account).bind(hero_id).execute(&mut *db).await?;
            }
            v["LinkInfo"] = json!(json!([hero_id]).to_string());
            out["NewHeroIndex"] = json!(hero_id);
            out["HeroIndex"] = json!(hero_id);
            out["OldHeroIndex"] = json!(old_hero);
            out["NewEquippedSlotIndex"] = json!(equipped);
            out["NewUnEquippedSlotIndex"] = json!(removed);
            out["UnEquippedSlotIndices"] = json!(removed);
            out["OldUnEquippedSlotIndex"] = json!(old_removed);
            out["HeroInfo"] = hero::info(db, account, hero_id).await?;
        }
        _ => return Err(rule("Fail")),
    }
    put(db, account, "equip_storage", index, &v).await?;
    out["EquipStorageSlotIndex"] = json!(index);
    out["EquipStorageSlotInfo"] = v.clone();
    out["EquipPresetInfo"] = preset(db, account, &v).await?;
    Ok(out)
}
fn compatible(state: &AppState, h: &Value, eq: &EquipItemInfo, part: i64) -> Result<()> {
    let data = meta(state, eq.item_index)?;
    if n(data, "PartType") != part || n(data, "ReqLevel") > n(h, "Level") || eq.identified == 0 {
        return Err(rule("InvalidEquipItem"));
    }
    if data["CreatureIndex"]
        .as_array()
        .is_some_and(|v| !v.is_empty() && !v.contains(&h["HeroIndex"]))
    {
        return Err(rule("NotCorrectHero"));
    }
    let hero = state
        .tables
        .hero_shop
        .heroes
        .get(&(n(h, "HeroIndex") as i32))
        .ok_or_else(|| rule("NoHero"))?;
    if let Some(tags) = data["TagType"].as_array() {
        if !tags.is_empty() && !tags.contains(&hero["TagType"]) {
            return Err(rule("NotCorrectHero"));
        }
    }
    Ok(())
}
