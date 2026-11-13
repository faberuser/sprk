use super::*;
use crate::models::item::ItemGrant;
use rand::RngExt;

pub(super) fn unique_options(state: &AppState, data: &Value, eq: &mut EquipItemInfo) -> Result<()> {
    let mut pool: BTreeSet<i64> = data["UniqueOptionIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
        .collect();
    for group in data["UniqueOptionGroupIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
    {
        if let Some(ids) = state.tables.inventory.option_groups.get(&(group as i32)) {
            pool.extend(ids.iter().map(|i| *i as i64));
        }
    }
    let count = 1;
    eq.extra_option_index_2 = 0;
    eq.extra_option_step_2 = 0;
    let mut v = json!(eq);
    for i in 1..=count {
        let (id, step) = equipment::option(state, &pool.iter().copied().collect::<Vec<_>>())?;
        v[format!("ExtraOptionIndex{i}")] = json!(id);
        v[format!("ExtraOptionStep{i}")] = json!(step);
        if data["EnableDuplicationUniqueOption"] != true {
            pool.remove(&id);
        }
    }
    *eq = serde_json::from_value(v).map_err(|_| rule("InvalidOption"))?;
    Ok(())
}
pub(crate) fn select_unique(
    state: &AppState,
    eq: &mut EquipItemInfo,
    chosen: &[i32],
) -> Result<()> {
    let data = meta(state, eq.item_index)?;
    let count = 1;
    if n(data, "EquipType") != 1 || chosen.len() != count {
        return Err(rule("NoAvailableOption"));
    }
    let mut pool: BTreeSet<i64> = data["UniqueOptionIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
        .collect();
    for group in data["UniqueOptionGroupIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
    {
        if let Some(ids) = state.tables.inventory.option_groups.get(&(group as i32)) {
            pool.extend(ids.iter().map(|v| *v as i64));
        }
    }
    eq.extra_option_index_2 = 0;
    eq.extra_option_step_2 = 0;
    let mut v = json!(eq);
    for (slot, id) in chosen.iter().enumerate() {
        if !pool.contains(&(*id as i64)) {
            return Err(rule("NoAvailableOption"));
        }
        let option = state
            .tables
            .inventory
            .options
            .get(id)
            .ok_or_else(|| rule("NoAvailableOption"))?;
        v[format!("ExtraOptionIndex{}", slot + 1)] = json!(id);
        v[format!("ExtraOptionStep{}", slot + 1)] = json!(n(option, "Steps"));
        if data["EnableDuplicationUniqueOption"] != true {
            pool.remove(&(*id as i64));
        }
    }
    v["Identified"] = json!(1);
    *eq = serde_json::from_value(v).map_err(|_| rule("NoAvailableOption"))?;
    Ok(())
}
pub(super) async fn execute(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if action == "valance_craft" {
        return craft(db, state, account, req).await;
    }
    let mut out = item::success();
    if action == "valance_identified" {
        let slots = ids(req, "EquipItemSlotIndices")?;
        if slots.is_empty()
            || slots.len() as i64
                > state
                    .tables
                    .hero_shop
                    .constant("MaxValanceIdentifiedCount", 10)
        {
            return Err(rule("InvalidItemCount"));
        }
        let mut cost = 0;
        let mut results = vec![];
        let mut identified = vec![];
        for slot in slots {
            let mut eq = equip(db, account, slot).await?;
            let data = meta(state, eq.item_index)?;
            if eq.identified != 0 || n(data, "EquipType") != 1 {
                return Err(rule("AlreadyIdentified"));
            }
            cost += n(
                row(
                    state,
                    "ValanceIdentified",
                    &[("DetailIndex", n(data, "DetailIndex"))],
                )?,
                "ReqGold",
            );
            unique_options(state, data, &mut eq)?;
            eq.identified = 1;
            save_equip(db, account, &eq).await?;
            let mut v = json!(eq);
            v["EquipItemSlotIndex"] = json!(eq.slot_index);
            identified.push(v);
            results.push(eq);
        }
        out["CurrencyResult"] = hero::currency(db, account, "Gold", -cost).await?;
        out["EquipItemResults"] = json!(results);
        out["EquipItemIdentifiedInfos"] = json!(identified);
        super::super::progression::record(db,account,"IdentifiedEquip",0,0,results.len() as i64).await?;
        return Ok(out);
    }
    // The extracted tier-upgrade request has no target or material fields.
    if action == "valance_tier_upgrade" {
        return Err(rule("Fail"));
    }
    let mut eq = equip(db, account, req.number("EquipItemSlotIndex", 0)?).await?;
    let data = meta(state, eq.item_index)?;
    if n(data, "EquipType") != 1 || eq.identified == 0 {
        return Err(rule("InvalidEquipItem"));
    }
    match action {
        "valance_awaken" => {
            let max_star = n(row(state, "EquipItemDetail", &[("DetailIndex", n(data, "DetailIndex"))])?, "MaxStar");
            if eq.star < 0 || eq.star as i64 >= max_star {
                return Err(rule("CannotAwakenMore"));
            }
            // The patched client may request several deterministic TM stars at
            // once; native clients omit TargetStar and upgrade one star.
            let before = eq.star as i64;
            let target = req.number("TargetStar", before + 1)?;
            if target <= before || target > max_star {
                return Err(rule("CannotAwakenMore"));
            }
            let mut gold = 0;
            let mut materials: std::collections::BTreeMap<i64, i64> = Default::default();
            for star in before + 1..=target {
                let r = row(
                    state,
                    "ValanceAwaken",
                    &[
                        ("Tier", n(data, "Tier")),
                        ("DetailIndex", n(data, "DetailIndex")),
                        ("CreatureTagType", data["TagType"][0].as_i64().unwrap_or(0)),
                        // Native ValanceAwakenWindow uses AfterStar for both stones and gold.
                        ("Star", star),
                    ],
                )?;
                gold += n(r, "ReqGold");
                if n(r, "ItemCount1") > 0 {
                    *materials.entry(n(r, "ItemIndex1")).or_default() += n(r, "ItemCount1");
                }
            }
            out["CurrencyResult"] = hero::currency(db, account, "Gold", -gold).await?;
            let mut consumed = vec![];
            for (id, count) in materials {
                let count = i32::try_from(count).map_err(|_| rule("NotEnoughMaterial"))?;
                consumed.push(item::consume(db, account, id as i32, count).await?);
            }
            out["ItemResults"] = json!(consumed);
            eq.star = target as i32;
            eq.upgrade_star_fail_bonus = 0;
            out["Success"] = json!(true);
            for star in before + 1..=target {
                super::super::progression::record(db, account, "AwakenGearAttempt", eq.item_index as i64, 0, 1).await?;
                super::super::progression::record(db, account, "AwakenGear", eq.item_index as i64, star, 1).await?;
            }
        }
        "valance_enchant" => {
            let item_id = req.number("ConsumeItemIndex", 0)?;
            let r = row(
                state,
                "ValanceEnchant",
                &[
                    ("ItemIndex", item_id),
                    ("PartType", n(data, "PartType")),
                    ("SubType", n(data, "SubType")),
                ],
            )?;
            let pool = enchant_pool(state, r)?;
            let counts = enchant_counts(state, item_id)?;
            let targets = enchant_targets(req)?;
            if !targets.is_empty() {
                return enchant_until(db, state, account, req, eq, data, item_id, r, &pool, &counts, &targets).await;
            }
            let mut pending =
                json!({"ConsumeItemIndex":item_id,"EnchantEquipItemSlotIndex":eq.slot_index});
            for (i, (id, step)) in roll_enchant(state, &pool, &counts)?.into_iter().enumerate() {
                pending[format!("EnchantOptionIndex{}", i + 1)] = json!(id);
                pending[format!("EnchantOptionStep{}", i + 1)] = json!(step);
            }
            if eq.enchant_option_index_1 == 0 && eq.enchant_option_index_2 == 0 && eq.enchant_option_index_3 == 0 {
                // The native first-enchant flow has no confirmation UI. Only
                // replacing existing options creates a pending choice.
                let mut value = json!(eq);
                for i in 1..=3 {
                    for field in ["Index", "Step"] {
                        let key = format!("EnchantOption{field}{i}");
                        value[&key] = pending[&key].clone();
                    }
                }
                eq = serde_json::from_value(value).map_err(|_| rule("InvalidOption"))?;
            } else {
                equipment::pending(db, account, eq.slot_index, "valance_enchant", &pending).await?;
            }
            out["CurrencyResult"] = hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
            out["ItemResultInfo"] = item::consume(db, account, item_id as i32, 1).await?;
            out["NewValanceEnchantOptionInfo"] = pending;
            super::super::progression::record(db, account, "Enchant", eq.item_index as i64, item_id, 1).await?;
        }
        "confirm_valance_enchant" => {
            let pending:String=crate::db::query_scalar("SELECT data FROM equipment_pending WHERE account_id=$1 AND slot_index=$2 AND kind='valance_enchant'")
                .bind(account).bind(eq.slot_index).fetch_optional(&mut *db).await?.ok_or_else(||rule("UnconfirmedOption"))?;
            let p: Value = serde_json::from_str(&pending).map_err(|_| rule("InvalidOption"))?;
            if matches!(req.text("IsNew"), "true" | "True" | "1") {
                let mut v = json!(eq);
                for i in 1..=3 {
                    for field in ["Index", "Step"] {
                        let key = format!("EnchantOption{field}{i}");
                        v[&key] = p[&key].clone();
                    }
                }
                eq = serde_json::from_value(v).map_err(|_| rule("InvalidOption"))?;
            }
            crate::db::query("DELETE FROM equipment_pending WHERE account_id=$1 AND slot_index=$2 AND kind='valance_enchant'").bind(account).bind(eq.slot_index).execute(&mut *db).await?;
        }
        _ => return Err(rule("Fail")),
    }
    save_equip(db, account, &eq).await?;
    out["ResultEquipItem"] = json!(eq);
    Ok(out)
}
async fn craft(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let r = row(
        state,
        "ValanceCraft",
        &[
            ("SetType", craft_enum(req, "SetType", &["None", "Gargoria", "Siegfried", "Ascalon"])?),
            ("CreatureTagType", craft_enum(req, "CreatureTagType", &["None", "Warrior", "Archer", "Knight", "Wizard", "Priest", "Assassin", "Mechanic", "End"])?),
            ("EquipPartType", craft_enum(req, "PartType", &["None", "Weapon", "Armor", "Accessory", "SecondGear", "Artifact", "Orb", "Treasure"])?),
            ("EquipItemSubType", craft_enum(req, "SubPartType", &["SpecialWeapon", "Spear", "Sword", "Dagger", "Bow", "Cannon", "Staff", "HeavyArmor", "MediumArmor", "LightArmor", "Ring", "Earring", "Necklace", "Bracelet", "SpecialTreasure", "SpecialTreasure_1", "SpecialTreasure_2", "SpecialTreasure_3", "SpecialTreasure_4", "Treasure", "None"])?),
        ],
    )?;
    let chosen = ids(req, "SelectEquipOptionIndices")?;
    let mut allowed = BTreeSet::new();
    for group in r["EquipOptionGroupIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
    {
        if let Some(ids) = state.tables.inventory.option_groups.get(&(group as i32)) {
            allowed.extend(ids.iter().map(|v| *v as i64));
        }
    }
    if chosen.iter().any(|id| !allowed.contains(id)) {
        return Err(rule("InvalidOption"));
    }
    let mut out = item::success();
    let material_count = discounted_material(db, state, account, n(r, "MaterialItemCount"), 69).await?
        + if chosen.is_empty() { 0 } else {
            discounted_material(db, state, account, n(r, "AdditionalMaterialItemCount"), 70).await?
        };
    let sub_material_count = if chosen.is_empty() { 0 } else {
        discounted_material(db, state, account, n(r, "SubMaterialItemCount"), 71).await?
    };
    out["CurrencyResult"] = hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
    let mut removed = vec![
        item::consume(
            db,
            account,
            n(r, "MaterialItemIndex") as i32,
            material_count as i32,
        )
        .await?,
    ];
    if !chosen.is_empty() {
        removed.push(
            item::consume(
                db,
                account,
                n(r, "SubMaterialItemIndex") as i32,
                sub_material_count as i32,
            )
            .await?,
        );
    }
    let (id, count, star, _) = state
        .tables
        .roll_item_from_group_code(r["ResultItemGroupCode"].as_str().unwrap_or(""), &[])
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    if count != 1 {
        return Err(rule("InvalidRewardData"));
    }
    let mut rewards = Rewards::default();
    item::give(
        db,
        state,
        account,
        ItemGrant {
            index: id,
            count: 1,
            star,
            custom: 0,
        },
        &mut rewards,
    )
    .await?;
    let eq = rewards
        .equipment
        .get_mut(0)
        .ok_or_else(|| rule("InvalidRewardData"))?;
    if chosen.is_empty() {
        eq.identified = 0;
    } else {
        // The native selector sends an additional skill, not a normal stat.
        select_unique(state, eq, &chosen.iter().map(|v| *v as i32).collect::<Vec<_>>())?;
    }
    save_equip(db, account, eq).await?;
    out["EquipItemResult"] = json!(eq);
    out["ItemResults"] = json!(removed);
    out["IsAdvanced"] = json!(n(meta(state, id)?, "EquipGrade") == 1);
    super::super::progression::record(db,account,"CraftEquip",0,0,1).await?;
    Ok(out)
}

// ValanceCraftWindow multiplies owned-hero discounts independently for base,
// selected-option and sub-material costs, then rounds to 2 decimals and up.
async fn discounted_material(db: &mut PgConnection, state: &AppState, account: i64, count: i64, kind: i64) -> Result<i64> {
    let owned = crate::db::query("SELECT hero_index,star,transcend FROM heroes WHERE account_id=$1 ORDER BY hero_index")
        .bind(account).fetch_all(&mut *db).await?;
    let mut ratio = 1.0_f32;
    for h in owned {
        if let Some(bonus) = state.tables.hero_shop.hero_bonuses.iter().find(|v|
            n(v,"HeroIndex") == h.get::<i64,_>("hero_index") &&
            n(v,"HeroStar") == h.get::<i64,_>("star") &&
            n(v,"HeroTranscended") == h.get::<i64,_>("transcend")) {
            for i in 1..=5 {
                if n(bonus,&format!("BonusType{i}")) == kind {
                    let percent = bonus[format!("BonusValue{i}")][0].as_str()
                        .and_then(|s| s.parse::<i64>().ok()).unwrap_or(0);
                    ratio *= 1.0 - percent.clamp(0,100) as f32 * 0.01;
                }
            }
        }
    }
    Ok((((count as f32 * ratio) as f64 * 100.0).round_ties_even() / 100.0).ceil() as i64)
}

fn craft_enum(req: &Request, key: &str, names: &[&str]) -> Result<i64> {
    if let Some(index) = names.iter().position(|name| *name == req.text(key)) {
        Ok(index as i64)
    } else {
        req.number(key, 0)
    }
}

/// Option IDs a Technomagic enchant core can roll for this gear part.
fn enchant_pool(state: &AppState, r: &Value) -> Result<Vec<i64>> {
    Ok(state
        .tables
        .inventory
        .option_groups
        .get(&(n(r, "OptionGroupIndex") as i32))
        .ok_or_else(|| rule("InvalidOption"))?
        .iter()
        .map(|i| *i as i64)
        .collect())
}

/// (line count, weight) rows for one enchant core.
fn enchant_counts(state: &AppState, core: i64) -> Result<Vec<(i64, i64)>> {
    let counts: Vec<(i64, i64)> = state
        .tables
        .extensions
        .rows("ValanceEnchantOptionCount")
        .iter()
        .filter(|v| n(v, "ItemIndex") == core)
        .map(|v| (n(v, "OptionCount"), n(v, "Rate")))
        .filter(|(_, rate)| *rate > 0)
        .collect();
    if counts.is_empty() || counts.iter().any(|(count, _)| !(1..=3).contains(count)) {
        return Err(rule("ItemDataNotFound"));
    }
    Ok(counts)
}

/// One enchant roll: a weighted line count, then that many distinct options.
fn roll_enchant(state: &AppState, pool: &[i64], counts: &[(i64, i64)]) -> Result<[(i64, i64); 3]> {
    let total: i64 = counts.iter().map(|c| c.1).sum();
    let mut roll = rand::rng().random_range(0..total);
    let count = counts
        .iter()
        .find(|c| {
            roll -= c.1;
            roll < 0
        })
        .map(|c| c.0)
        .ok_or_else(|| rule("InvalidOption"))?;
    let mut candidates = pool.to_vec();
    let mut lines = [(0, 0); 3];
    for (i, line) in lines.iter_mut().enumerate() {
        if (i as i64) < count {
            *line = equipment::option(state, &candidates)?;
            candidates.retain(|v| *v != line.0);
        }
    }
    Ok(lines)
}

/// Most enchants one "enchant until" request may perform.
pub(super) const ENCHANT_UNTIL_MAX_TRIES: i64 = 5000;

/// SPRK "enchant until" targets: up to 3 distinct stat types, separated by `_`
/// (the client form-escapes commas twice, so `,` arrives as a literal `%2c`).
fn enchant_targets(req: &Request) -> Result<Vec<i64>> {
    let text = req.text("SprkTargetOptionTypes").replace("%2c", "_").replace("%2C", "_");
    if text.is_empty() {
        return Ok(vec![]);
    }
    let mut targets = vec![];
    for part in text.split(['_', ',']) {
        let value: i64 = part.trim().parse().map_err(|_| rule("InvalidOption"))?;
        if value <= 0 || targets.contains(&value) {
            return Err(rule("InvalidOption"));
        }
        targets.push(value);
    }
    if targets.len() > 3 {
        return Err(rule("InvalidOption"));
    }
    Ok(targets)
}

fn stat_type(state: &AppState, option: i64) -> i64 {
    state.tables.inventory.options.get(&(option as i32)).map(|o| n(o, "Type")).unwrap_or(0)
}

/// Exact chance that one enchant with this core rolls every target stat.
pub(super) fn enchant_chance(state: &AppState, pool: &[i64], counts: &[(i64, i64)], targets: &[i64]) -> f64 {
    // Same weights as `equipment::option`: options with a positive Ratio.
    let items: Vec<(i64, f64)> = pool
        .iter()
        .filter_map(|id| state.tables.inventory.options.get(&(*id as i32)))
        .filter(|o| n(o, "Ratio") > 0)
        .map(|o| (n(o, "Type"), n(o, "Ratio") as f64))
        .collect();
    fn draw(items: &[(i64, f64)], draws: i64, need: &[i64]) -> f64 {
        if need.is_empty() {
            return 1.0;
        }
        if draws < need.len() as i64 || items.is_empty() {
            return 0.0;
        }
        let total: f64 = items.iter().map(|i| i.1).sum();
        let mut chance = 0.0;
        for (k, item) in items.iter().enumerate() {
            let rest: Vec<_> = items[..k].iter().chain(&items[k + 1..]).copied().collect();
            let still: Vec<_> = need.iter().copied().filter(|t| *t != item.0).collect();
            chance += item.1 / total * draw(&rest, draws - 1, &still);
        }
        chance
    }
    let total: i64 = counts.iter().map(|c| c.1).sum();
    if total <= 0 {
        return 0.0;
    }
    counts.iter().map(|(count, rate)| *rate as f64 / total as f64 * draw(&items, *count, targets)).sum()
}

/// SPRK "enchant until": re-enchant until the rolled lines include every target
/// stat type (any order; other lines are free).
///
/// `SprkPreview=1` returns the per-try chance for this core and for every core
/// that fits the gear, without charging. Otherwise up to `SprkMaxTries` rolls run
/// in memory, each costing one core and its gold; the run stops at the first hit
/// or when the next try is unaffordable. Costs are charged once, and only the last
/// roll is kept: applied directly on a first enchant (as the native flow does),
/// otherwise left pending for the usual choice.
#[allow(clippy::too_many_arguments)]
async fn enchant_until(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    mut eq: EquipItemInfo,
    data: &Value,
    core: i64,
    r: &Value,
    pool: &[i64],
    counts: &[(i64, i64)],
    targets: &[i64],
) -> Result<Value> {
    let chance = enchant_chance(state, pool, counts, targets);
    let mut out = item::success();
    if matches!(req.text("SprkPreview"), "1" | "true" | "True") {
        let mut cores = vec![];
        for row in state.tables.extensions.rows("ValanceEnchant") {
            if n(row, "PartType") != n(data, "PartType") || n(row, "SubType") != n(data, "SubType") {
                continue;
            }
            let other = n(row, "ItemIndex");
            let (Ok(other_pool), Ok(other_counts)) = (enchant_pool(state, row), enchant_counts(state, other)) else {
                continue;
            };
            cores.push(json!({"ItemIndex":other,"ReqGold":n(row,"ReqGold"),
                "Chance":enchant_chance(state, &other_pool, &other_counts, targets)}));
        }
        out["SprkChance"] = json!(chance);
        out["SprkCores"] = json!(cores);
        return Ok(out);
    }
    if chance <= 0.0 {
        return Err(rule("FailGenerateValanceEnchantOption"));
    }
    let max_tries = req.number("SprkMaxTries", 0)?;
    if !(1..=ENCHANT_UNTIL_MAX_TRIES).contains(&max_tries) {
        return Err(rule("InvalidOption"));
    }
    let price = n(r, "ReqGold").max(0);
    let cores = equipment::spendable_item(db, account, core).await?;
    let gold = equipment::spendable_gold(db, account).await?;
    let (mut tries, mut last, mut hit) = (0i64, None, false);
    while tries < max_tries && tries < cores && price * (tries + 1) <= gold {
        tries += 1;
        let lines = roll_enchant(state, pool, counts)?;
        hit = targets.iter().all(|t| lines.iter().any(|(id, _)| *id != 0 && stat_type(state, *id) == *t));
        last = Some(lines);
        if hit {
            break;
        }
    }
    let Some(lines) = last else {
        return Err(rule(if cores == 0 { "ItemNotOwned" } else { "NotEnoughGold" }));
    };
    let mut pending = json!({"ConsumeItemIndex":core,"EnchantEquipItemSlotIndex":eq.slot_index});
    for (i, (id, step)) in lines.into_iter().enumerate() {
        pending[format!("EnchantOptionIndex{}", i + 1)] = json!(id);
        pending[format!("EnchantOptionStep{}", i + 1)] = json!(step);
    }
    if eq.enchant_option_index_1 == 0 && eq.enchant_option_index_2 == 0 && eq.enchant_option_index_3 == 0 {
        let mut value = json!(eq);
        for i in 1..=3 {
            for field in ["Index", "Step"] {
                let key = format!("EnchantOption{field}{i}");
                value[&key] = pending[&key].clone();
            }
        }
        eq = serde_json::from_value(value).map_err(|_| rule("InvalidOption"))?;
    } else {
        equipment::pending(db, account, eq.slot_index, "valance_enchant", &pending).await?;
    }
    out["CurrencyResult"] = hero::currency(db, account, "Gold", -(price * tries)).await?;
    out["ItemResultInfo"] = item::consume(db, account, core as i32, tries as i32).await?;
    out["NewValanceEnchantOptionInfo"] = pending;
    out["SprkTries"] = json!(tries);
    out["SprkHit"] = json!(hit);
    out["SprkGoldSpent"] = json!(price * tries);
    out["SprkCoresUsed"] = json!(tries);
    super::super::progression::record(db, account, "Enchant", eq.item_index as i64, core, tries).await?;
    save_equip(db, account, &eq).await?;
    out["ResultEquipItem"] = json!(eq);
    Ok(out)
}
