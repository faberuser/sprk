use super::*;
use rand::{seq::SliceRandom, Rng};

pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if action == "restore_artifact" {
        let r = row(
            state,
            "ArtifactRestore",
            &[("Index", req.number("ArtifactRestoreIndex", 0)?)],
        )?;
        let consumed = item::consume(
            db,
            account,
            n(r, "MaterialItemIndex") as i32,
            n(r, "MaterialItemCount") as i32,
        )
        .await?;
        let mut rewards = Rewards::default();
        let (id, count, star, _) = state
            .tables
            .roll_item_from_group_code(r["ArtifactItemGroup"].as_str().unwrap_or(""), &[])
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        item::give(db, state, account, id, count, star, 0, &mut rewards).await?;
        if rewards.equipment.len() != 1 {
            return Err(rule("InvalidRewardData"));
        }
        super::super::progression::record(db, account, "ArtifactRestore", 0, 0, 1).await?;
        return Ok(
            json!({"BaseResult":"Success","Result":"Success","ItemResult":consumed,"EquipItem":rewards.equipment[0]}),
        );
    }
    if action == "break_equip" {
        return break_equipment(db, state, account, req).await;
    }
    let mut eq = equip(db, account, req.number("EquipItemSlotIndex", 0)?).await?;
    // Event forge levels must only advance through their event material rules.
    if n(item::data(state,eq.item_index)?,"Type")==52 {
        return Err(rule("InvalidMaterial"));
    }
    let data = meta(state, eq.item_index)?;
    let detail = row(
        state,
        "EquipItemDetail",
        &[("DetailIndex", n(data, "DetailIndex"))],
    )?;
    let mut out = item::success();
    match action {
        "awaken_equip" => {
            if eq.star as i64 >= n(detail, "MaxStar") || n(data, "EquipType") != 0 {
                return Err(rule("MaxStar"));
            }
            let materials = ids(req, "MaterialSlotIndices")?;
            let pairs = item_pairs(req, "MaterialItemInfors")?;
            if materials.is_empty() && pairs.is_empty() {
                return Err(rule("NotEnoughMaterial"));
            }
            let mut points = 0;
            for slot in &materials {
                if *slot == eq.slot_index as i64 {
                    return Err(rule("InvalidMaterial"));
                }
                let m = material(db, account, *slot).await?;
                if m.item_index != eq.item_index
                    || (m.level as i64) < n(detail, "ReqAwakeMaterialLevel")
                {
                    return Err(rule("InvalidMaterial"));
                }
                points += n(
                    row(state, "EquipAwakenPoint", &[("Star", m.star as i64)])?,
                    "AwakenPoint",
                );
            }
            let mut consumed = vec![];
            for (id, count) in pairs {
                let stone = row(state, "AwakenStone", &[("ItemIndex", id)])?;
                let valid_type = match n(stone, "Type") {
                    42 => matches!(n(detail, "EquipItemDetailType"), 11 | 13),
                    43 => n(detail, "EquipItemDetailType") == 14,
                    _ => false,
                };
                if !valid_type
                    || stone["ItemIndices"]
                        .as_array()
                        .is_some_and(|ids| !ids.is_empty() && !ids.contains(&json!(eq.item_index)))
                {
                    return Err(rule("InvalidMaterial"));
                }
                if stone["IsAllowNPC"] == false
                    && data["CreatureIndex"]
                        .as_array()
                        .into_iter()
                        .flatten()
                        .any(|v| {
                            v.as_i64().is_some_and(|i| {
                                state
                                    .tables
                                    .extensions
                                    .rows("NPCFriendlyPoint")
                                    .iter()
                                    .any(|npc| n(npc, "HeroIndex") == i)
                            })
                        })
                {
                    return Err(rule("InvalidMaterial"));
                }
                points += n(
                    row(
                        state,
                        "EquipAwakenPoint",
                        &[("Star", n(stone, "AwakenStar"))],
                    )?,
                    "AwakenPoint",
                ) * count;
                consumed.push(item::consume(db, account, id as i32, count as i32).await?);
            }
            let max = state
                .tables
                .extensions
                .rows("EquipAwakenRatio")
                .iter()
                .filter(|r| n(r, "Star") == eq.star as i64 + 1)
                .map(|r| n(r, "AwakenPoint"))
                .max()
                .ok_or_else(|| rule("MaxStar"))?;
            let ratio = row(
                state,
                "EquipAwakenRatio",
                &[
                    ("Star", eq.star as i64 + 1),
                    ("AwakenPoint", points.min(max)),
                ],
            )?;
            let price = row(
                state,
                "EquipAwakenPrice",
                &[
                    ("DetailIndex", n(data, "DetailIndex")),
                    ("Tier", n(data, "Tier")),
                    ("Star", eq.star as i64),
                ],
            )?;
            out["CurrencyResult"] =
                hero::currency(db, account, "Gold", -n(price, "ReqGold")).await?;
            if n(price, "ItemCount1") > 0 {
                consumed.push(
                    item::consume(
                        db,
                        account,
                        n(price, "ItemIndex1") as i32,
                        n(price, "ItemCount1") as i32,
                    )
                    .await?,
                );
            }
            let success = roll(
                n(ratio, "SuccessRatio") + eq.upgrade_star_fail_bonus as i64,
                100,
            );
            if success {
                eq.star += 1;
                eq.upgrade_star_fail_bonus = 0;
            } else {
                eq.upgrade_star_fail_bonus += n(ratio, "FailBonusRatio") as i32;
            }
            for slot in &materials {
                remove_equip(db, account, *slot as i32).await?;
            }
            out["Success"] = json!(success);
            out["RemovedEquipItemSlotIndices"] = json!(materials);
            out["RemovedItemResultInfos"] = json!(consumed);
            out["ItemResults"] = json!([]);
            if success {
                super::super::progression::record(db, account, "AwakenGear", 0, 0, 1).await?;
            }
        }
        "upgrade_equip" => {
            let target = req.number("UpgradeLevel", 0)?;
            if target <= eq.level as i64
                || target > n(detail, "MaxLevel")
                || detail["MaxLevelByHero"] == true
            {
                return Err(rule("MaxLevel"));
            }
            let materials = ids(req, "MaterialSlotIndices")?;
            let mut gained = eq.exp as i64;
            for slot in &materials {
                if *slot == eq.slot_index as i64 {
                    return Err(rule("InvalidMaterial"));
                }
                let m = material(db, account, *slot).await?;
                let md = meta(state, m.item_index)?;
                gained += n(
                    row(
                        state,
                        "EquipUpgradeExp",
                        &[
                            ("PartType", n(md, "PartType")),
                            ("Tier", n(md, "Tier")),
                            ("DetailIndex", n(md, "DetailIndex")),
                            ("Level", m.level as i64),
                        ],
                    )?,
                    "GiveExp",
                );
            }
            let mut cost = 0;
            for level in eq.level as i64..target {
                cost += n(
                    row(
                        state,
                        "EquipUpgradePrice",
                        &[
                            ("Tier", n(data, "Tier")),
                            ("DetailIndex", n(data, "DetailIndex")),
                            ("Level", level),
                        ],
                    )?,
                    "ReqGold",
                );
                let need = n(
                    row(
                        state,
                        "EquipUpgradeExp",
                        &[
                            ("PartType", n(data, "PartType")),
                            ("Tier", n(data, "Tier")),
                            ("DetailIndex", n(data, "DetailIndex")),
                            ("Level", level),
                        ],
                    )?,
                    "ReqExp",
                );
                if gained < need {
                    return Err(rule("NotEnoughMaterial"));
                }
                gained -= need;
            }
            out["CurrencyResult"] = hero::currency(db, account, "Gold", -cost).await?;
            eq.level = target as i32;
            eq.exp = i32::try_from(gained).map_err(|_| rule("InvalidMaterial"))?;
            for slot in &materials {
                remove_equip(db, account, *slot as i32).await?;
            }
            out["RemovedEquipItemSlotIndices"] = json!(materials);
            out["ItemResults"] = json!([]);
        }
        "upgrade_equip_tier" => {
            let pending:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM equipment_pending WHERE account_id=? AND slot_index=?)").bind(account).bind(eq.slot_index).fetch_one(&mut *db).await?;
            if pending {
                return Err(rule("UnconfirmedOption"));
            }
            if eq.star as i64 != n(detail, "MaxStar") {
                return Err(rule("NotEnoughStar"));
            }
            let r = row(
                state,
                "EquipUpgradeTier",
                &[("ItemIndex", eq.item_index as i64)],
            )?;
            let target = n(r, "ResultItemIndex") as i32;
            meta(state, target)?;
            let mut consumed = vec![];
            for i in 1..=5 {
                let count = n(r, &format!("MaterialItemCount{i}"));
                if count > 0 {
                    consumed.push(
                        item::consume(
                            db,
                            account,
                            n(r, &format!("MaterialItemIndex{i}")) as i32,
                            count as i32,
                        )
                        .await?,
                    );
                }
            }
            out["CurrencyResults"] = json!([hero::currency(
                db,
                account,
                currency_type(n(r, "CostType"))?,
                -n(r, "CostValue")
            )
            .await?]);
            out["ItemResults"] = json!(consumed);
            let before = n(data, "Tier");
            let after = n(meta(state, target)?, "Tier");
            let mut v = json!(eq);
            for i in 1..=5 {
                let key = format!("OptionIndex{i}");
                if let Some(r) = state.tables.extensions.find(
                    "EquipOptionChange",
                    &[
                        ("OptionIndex", n(&v, &key)),
                        ("BeforeTier", before),
                        ("AfterTier", after),
                    ],
                ) {
                    v[key] = r["ResultOptionIndex"].clone();
                }
            }
            eq = serde_json::from_value(v).map_err(|_| rule("InvalidOption"))?;
            eq.item_index = target;
            eq.star = 0;
            eq.upgrade_star_fail_bonus = 0;
            out["EquipHeroIndex"] = json!(0);
            out["NewEquipStorageSlotInfo"] = Value::Null;
            out["EquipPresetInfos"] = json!([]);
            super::super::progression::record(db, account, "UpgradeEquipTier", 0, 0, 1).await?;
        }
        "upgrade_equip_option" | "max_upgrade_equip_option" => {
            let price = row(
                state,
                "EquipOptionUpgrade",
                &[
                    ("Tier", n(data, "Tier")),
                    ("DetailIndex", n(data, "DetailIndex")),
                ],
            )?;
            if req.number("MaterialItemIndex", 0)? != n(price, "MaterialItemIndex") {
                return Err(rule("InvalidMaterial"));
            }
            let max = if action == "max_upgrade_equip_option" {
                1000
            } else {
                1
            };
            let mut v = json!(eq);
            let mut steps = 0;
            for _ in 0..max {
                let available: Vec<usize> = (1..=4)
                    .filter(|i| {
                        state
                            .tables
                            .inventory
                            .options
                            .get(&(n(&v, &format!("OptionIndex{i}")) as i32))
                            .is_some_and(|o| n(&v, &format!("OptionStep{i}")) < n(o, "Steps"))
                    })
                    .collect();
                if available.is_empty() {
                    break;
                }
                let slot = *available.choose(&mut rand::thread_rng()).unwrap();
                let key = format!("OptionStep{slot}");
                v[&key] = json!(n(&v, &key) + 1);
                steps += 1;
            }
            if steps == 0 {
                return Err(rule("MaxLevel"));
            }
            let count = n(price, "MaterialItemCount") * steps;
            if req.number("MaterialItemCount", 0)? < count {
                return Err(rule("NotEnoughMaterial"));
            }
            out["ItemResult"] = item::consume(
                db,
                account,
                n(price, "MaterialItemIndex") as i32,
                count as i32,
            )
            .await?;
            out["CurrencyResult"] =
                hero::currency(db, account, "Gold", -n(price, "ReqGold") * steps).await?;
            eq = serde_json::from_value(v).map_err(|_| rule("InvalidOption"))?;
            super::super::progression::record(db, account, "UpgradeEquipOption", 0, 0, steps)
                .await?;
        }
        "renew_equip_option" | "renew_equip_skill" => {
            return renew(db, state, account, req, eq, action).await
        }
        "renew_confirm_equip_option"
        | "renew_confirm_equip_skill"
        | "confirm_equip_enchant_option" => return confirm(db, account, req, eq, action).await,
        "enchant_equip" => return enchant(db, state, account, req, eq).await,
        _ => return Err(rule("Fail")),
    }
    save_equip(db, account, &eq).await?;
    out["ResultEquipItem"] = json!(eq);
    Ok(out)
}

pub(super) fn item_pairs(req: &Request, key: &str) -> Result<Vec<(i64, i64)>> {
    if req.text(key).is_empty() {
        return Ok(vec![]);
    }
    let values: Vec<i64> =
        serde_json::from_str(req.text(key)).map_err(|_| rule("InvalidMaterial"))?;
    if values.len() % 2 != 0 || values.len() > 200 {
        return Err(rule("InvalidMaterial"));
    }
    let mut seen = BTreeSet::new();
    values
        .chunks_exact(2)
        .map(|v| {
            if v[0] > 0 && (1..=100000).contains(&v[1]) && seen.insert(v[0]) {
                Ok((v[0], v[1]))
            } else {
                Err(rule("InvalidMaterial"))
            }
        })
        .collect()
}
async fn break_equipment(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let ids = ids(req, "EquipItemSlotIndices")?;
    if ids.is_empty() {
        return Err(rule("InvalidMaterial"));
    }
    let mut rewards = Rewards::default();
    for slot in ids {
        let eq = material(db, account, slot).await?;
        let data = meta(state, eq.item_index)?;
        let all = state.tables.extensions.rows("EquipItemBreak");
        let r = if n(data, "SetIndex") > 0 {
            all.iter().find(|r| {
                n(r, "Tier") == n(data, "Tier")
                    && n(r, "DetailIndex") == n(data, "DetailIndex")
                    && n(r, "SetIndex") == n(data, "SetIndex")
            })
        } else {
            all.iter()
                .find(|r| n(r, "ItemIndex") == eq.item_index as i64)
                .or_else(|| {
                    all.iter()
                        .filter(|r| {
                            n(r, "Tier") == n(data, "Tier")
                                && n(r, "DetailIndex") == n(data, "DetailIndex")
                                && n(r, "SetIndex") == 0
                                && n(r, "ItemIndex") == 0
                                && n(r, "Star") <= eq.star as i64
                        })
                        .max_by_key(|r| n(r, "Star"))
                })
        }
        .ok_or_else(|| rule("ItemDataNotFound"))?;
        for i in 1..=5 {
            let max = n(r, &format!("MaxItemCount{i}"));
            let mut count = if max > 0 {
                rand::thread_rng().gen_range(n(r, &format!("MinItemCount{i}"))..=max)
            } else {
                0
            };
            for bonus in 1..=2 {
                if roll(n(r, &format!("ItemRate{i}_{bonus}")), 1000) {
                    count += n(r, &format!("ItemCount{i}_{bonus}"));
                }
            }
            if count > 0 {
                item::give(
                    db,
                    state,
                    account,
                    n(r, &format!("ItemIndex{i}")) as i32,
                    count as i32,
                    0,
                    0,
                    &mut rewards,
                )
                .await?;
            }
        }
        remove_equip(db, account, eq.slot_index).await?;
    }
    let r = item::reward_response(db, state, account, rewards).await?;
    Ok(json!({"BaseResult":"Success","Result":"Success","ItemResults":r["ItemResults"]}))
}
pub(super) async fn pending(
    db: &mut SqliteConnection,
    account: i64,
    slot: i32,
    kind: &str,
    value: &Value,
) -> Result<()> {
    let exists: bool = sqlx::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM equipment_pending WHERE account_id=? AND kind=?)",
    )
    .bind(account)
    .bind(kind)
    .fetch_one(&mut *db)
    .await?;
    if exists {
        return Err(rule("UnconfirmedOption"));
    }
    sqlx::query("INSERT INTO equipment_pending(account_id,slot_index,kind,data) VALUES(?,?,?,?)")
        .bind(account)
        .bind(slot)
        .bind(kind)
        .bind(value.to_string())
        .execute(db)
        .await?;
    Ok(())
}
pub(super) fn option(state: &AppState, candidates: &[i64]) -> Result<(i64, i64)> {
    let mut pool = vec![];
    for id in candidates {
        if let Some(r) = state.tables.inventory.options.get(&(*id as i32)) {
            if n(r, "Ratio") > 0 {
                pool.push((*id, n(r, "Ratio"), n(r, "Steps")));
            }
        }
    }
    let total: i64 = pool.iter().map(|r| r.1).sum();
    if total <= 0 {
        return Err(rule("InvalidOption"));
    }
    let mut value = rand::thread_rng().gen_range(0..total);
    let chosen = pool
        .iter()
        .find(|r| {
            value -= r.1;
            value < 0
        })
        .unwrap();
    Ok((chosen.0, rand::thread_rng().gen_range(0..=chosen.2.max(0))))
}
async fn renew(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    eq: EquipItemInfo,
    action: &str,
) -> Result<Value> {
    let skill = action == "renew_equip_skill";
    let kind = if skill { "skill" } else { "option" };
    let slot = req.number(
        if skill {
            "SkillSlotIndex"
        } else {
            "OptionSlotIndex"
        },
        0,
    )?;
    if !(1..=if skill { 2 } else { 4 }).contains(&slot) {
        return Err(rule("InvalidOptionSlotIndex"));
    }
    let data = meta(state, eq.item_index)?;
    let detail = row(
        state,
        "EquipItemDetail",
        &[("DetailIndex", n(data, "DetailIndex"))],
    )?;
    let mut value = json!(eq);
    let key = format!(
        "{}Index{slot}",
        if skill { "ExtraOption" } else { "Option" }
    );
    if n(&value, &key) == 0 {
        return Err(rule("InvalidOption"));
    }
    let mut candidates = BTreeSet::new();
    for id in data[if skill {
        "UniqueOptionIndex"
    } else {
        "OptionIndex"
    }]
    .as_array()
    .into_iter()
    .flatten()
    .filter_map(Value::as_i64)
    {
        candidates.insert(id);
    }
    for group in data[if skill {
        "UniqueOptionGroupIndex"
    } else {
        "OptionGroupIndex"
    }]
    .as_array()
    .into_iter()
    .flatten()
    .filter_map(Value::as_i64)
    {
        if let Some(ids) = state.tables.inventory.option_groups.get(&(group as i32)) {
            candidates.extend(ids.iter().map(|i| *i as i64));
        }
    }
    if detail["EnableRenewSameOption"] != true {
        candidates.remove(&n(&value, &key));
    }
    if !skill && data["EnableDuplicationOption"] != true {
        let occupied: BTreeSet<i64> = (1..=4)
            .filter(|i| *i != slot)
            .filter_map(|i| {
                state
                    .tables
                    .inventory
                    .options
                    .get(&(n(&value, &format!("OptionIndex{i}")) as i32))
                    .map(|o| n(o, "Type"))
            })
            .collect();
        candidates.retain(|i| {
            state
                .tables
                .inventory
                .options
                .get(&(*i as i32))
                .is_some_and(|o| !occupied.contains(&n(o, "Type")))
        });
    }
    let (new, step) = option(state, &candidates.into_iter().collect::<Vec<_>>())?;
    pending(
        db,
        account,
        eq.slot_index,
        kind,
        &json!({"Slot":slot,"Index":new,"Step":step}),
    )
    .await?;
    let mut out = item::success();
    let mut consumed = vec![];
    if skill {
        consumed.push(
            item::consume(
                db,
                account,
                n(detail, "ExtraSkillRenewItemIndex") as i32,
                n(detail, "ExtraSkillRenewItemCount") as i32,
            )
            .await?,
        );
    } else {
        let used = (1..=4)
            .filter(|i| n(&value, &format!("IsRenewedOption{i}")) != 0)
            .count() as i64;
        if used >= n(detail, "RenewableOptionCount")
            && n(&value, &format!("IsRenewedOption{slot}")) == 0
        {
            return Err(rule("InvalidOption"));
        }
        let coupon = req.number("CouponItemIndex", 0)?;
        if coupon > 0 {
            if coupon != n(detail, "RenewTicketItemIndex") {
                return Err(rule("InvalidMaterial"));
            }
            consumed.push(item::consume(db, account, coupon as i32, 1).await?);
        } else {
            let prices = detail["RenewPrice"]
                .as_array()
                .filter(|p| !p.is_empty())
                .ok_or_else(|| rule("InvalidOption"))?;
            let count = n(&value, &format!("OptionRenewCount{slot}"));
            let price = prices[(count as usize).min(prices.len() - 1)]
                .as_i64()
                .ok_or_else(|| rule("InvalidOption"))?;
            out["CurrencyResult"] = hero::currency(
                db,
                account,
                currency_type(n(detail, "RenewCurrencyType"))?,
                -price,
            )
            .await?;
        }
        for i in 1..=3 {
            let count = n(detail, &format!("RenewItemCount{i}"));
            if count > 0 {
                consumed.push(
                    item::consume(
                        db,
                        account,
                        n(detail, &format!("RenewItemIndex{i}")) as i32,
                        count as i32,
                    )
                    .await?,
                );
            }
        }
    }
    let prefix = if skill { "ExtraOption" } else { "Option" };
    let countkey = format!("{prefix}RenewCount{slot}");
    value[&countkey] = json!(n(&value, &countkey) + 1);
    value[format!("IsRenewed{prefix}{slot}")] = json!(1);
    let eq: EquipItemInfo = serde_json::from_value(value).map_err(|_| rule("InvalidOption"))?;
    save_equip(db, account, &eq).await?;
    out["ResultEquipItem"] = json!(eq);
    out["ItemResults"] = json!(consumed);
    out[if skill {
        "NewSkillIndex"
    } else {
        "NewOptionIndex"
    }] = json!(new);
    out[if skill {
        "NewSkillStep"
    } else {
        "NewOptionStep"
    }] = json!(step);
    super::super::progression::record(db, account, "RenewEquipOption", 0, 0, 1).await?;
    Ok(out)
}
async fn confirm(
    db: &mut SqliteConnection,
    account: i64,
    req: &Request,
    eq: EquipItemInfo,
    action: &str,
) -> Result<Value> {
    let kind = if action == "confirm_equip_enchant_option" {
        "enchant"
    } else if action == "renew_confirm_equip_skill" {
        "skill"
    } else {
        "option"
    };
    let text: Option<String> = sqlx::query_scalar(
        "SELECT data FROM equipment_pending WHERE account_id=? AND slot_index=? AND kind=?",
    )
    .bind(account)
    .bind(eq.slot_index)
    .bind(kind)
    .fetch_optional(&mut *db)
    .await?;
    let pending: Value = serde_json::from_str(&text.ok_or_else(|| rule("UnconfirmedOption"))?)
        .map_err(|_| rule("InvalidOption"))?;
    let slot = n(&pending, "Slot");
    let accepted = matches!(req.text("IsNew"), "true" | "True" | "1");
    if kind != "enchant"
        && req.number(
            if kind == "skill" {
                "SkillSlotIndex"
            } else {
                "OptionSlotIndex"
            },
            0,
        )? != slot
    {
        return Err(rule("InvalidOption"));
    }
    let prefix = match kind {
        "enchant" => "EnchantOption",
        "skill" => "ExtraOption",
        _ => "Option",
    };
    let mut value = json!(eq);
    let expected = if accepted {
        n(&pending, "Index")
    } else {
        n(&value, &format!("{prefix}Index{slot}"))
    };
    if req.number(
        match kind {
            "enchant" => "ConfirmEnchantOptionIndex",
            "skill" => "ConfirmSkillIndex",
            _ => "ConfirmOptionIndex",
        },
        0,
    )? != expected
    {
        return Err(rule("InvalidOption"));
    }
    if accepted {
        value[format!("{prefix}Index{slot}")] = pending["Index"].clone();
        value[format!("{prefix}Step{slot}")] = pending["Step"].clone();
    }
    if kind == "enchant" {
        value["RenewEnchantOptionSlotIndex"] = json!(0);
    }
    let result: EquipItemInfo = serde_json::from_value(value).map_err(|_| rule("InvalidOption"))?;
    save_equip(db, account, &result).await?;
    sqlx::query("DELETE FROM equipment_pending WHERE account_id=? AND slot_index=? AND kind=?")
        .bind(account)
        .bind(eq.slot_index)
        .bind(kind)
        .execute(db)
        .await?;
    Ok(json!({"BaseResult":"Success","Result":"Success","ResultEquipItem":result}))
}
async fn enchant(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    mut eq: EquipItemInfo,
) -> Result<Value> {
    let id = req.number("EnchantItemIndex", 0)?;
    let r = row(state, "EquipEnchantOptionItem", &[("ItemIndex", id)])?;
    let data = meta(state, eq.item_index)?;
    let code = r["ItemCode"].as_str().unwrap_or("");
    let allowed = data["EnchantOptionItemCodes"]
        .as_array()
        .is_some_and(|a| a.contains(&json!(code)))
        || state
            .tables
            .extensions
            .rows("EquipEnchantableCondition")
            .iter()
            .any(|c| {
                n(c, "TierIndex") == n(data, "Tier")
                    && n(c, "DetailIndex") == n(data, "DetailIndex")
                    && c["EnchantOptionItemCodes"]
                        .as_array()
                        .is_some_and(|a| a.contains(&json!(code)))
            });
    if !allowed {
        return Err(rule("InvalidEnchantItem"));
    }
    let slot = (1..=3)
        .find(|i| {
            r[format!("EnchantOptionIndex{i}")]
                .as_array()
                .is_some_and(|a| !a.is_empty())
        })
        .ok_or_else(|| rule("InvalidEnchantItem"))?;
    let candidates: Vec<i64> = r[format!("EnchantOptionIndex{slot}")]
        .as_array()
        .unwrap()
        .iter()
        .filter_map(Value::as_i64)
        .collect();
    let (new, step) = option(state, &candidates)?;
    pending(
        db,
        account,
        eq.slot_index,
        "enchant",
        &json!({"Slot":slot,"Index":new,"Step":step}),
    )
    .await?;
    let consumed = item::consume(db, account, id as i32, n(r, "ReqCount") as i32).await?;
    let currency = hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
    eq.renew_enchant_option_slot_index = slot as u8;
    save_equip(db, account, &eq).await?;
    super::super::progression::record(db, account, "Enchant", 0, 0, 1).await?;
    Ok(
        json!({"BaseResult":"Success","Result":"Success","ResultEquipItem":eq,"CurrencyResult":currency,"ItemResult":consumed,"EquipHeroIndex":0,"NewEnchantOptionIndex":new,"NewEnchantOptionStep":step}),
    )
}
