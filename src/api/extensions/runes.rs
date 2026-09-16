use super::*;

pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let hero_index = i32::try_from(req.number("HeroIndex", 0)?).map_err(|_| rule("NoHero"))?;
    hero::info(db, account, hero_index).await?;
    let mut details = hero::details(db, account, hero_index).await?;
    let mut pages = details["HeroRunePageInfos"]
        .as_array()
        .cloned()
        .unwrap_or_else(|| new_page(hero_index, 1, state));
    let page = req.number("RunePage", 1)?;
    let mut out = item::success();
    if action == "extend_rune_page" {
        let last = pages.iter().map(|r| n(r, "RunePage")).max().unwrap_or(1);
        if page != last + 1 || page > state.tables.hero_shop.constant("MaxRunePage", 6) {
            return Err(rule("InvalidRunePage"));
        }
        out["CurrencyResult"] = hero::currency(
            db,
            account,
            "Gem",
            -state.tables.hero_shop.constant("ExtendRunePageGem", 500),
        )
        .await?;
        pages.extend(new_page(hero_index, page, state));
    } else {
        if !pages.iter().any(|r| n(r, "RunePage") == page) {
            return Err(rule("InvalidRunePage"));
        }
        if action == "apply_hero_rune_page" {
            details["ApplyRunePage"] = json!(page);
        } else {
            let slot = req.number("SlotNum", 0)?;
            let current = pages
                .iter_mut()
                .find(|r| n(r, "RunePage") == page && n(r, "SlotNum") == slot)
                .ok_or_else(|| rule("InvalidRuneSlot"))?;
            let old = n(current, "ItemIndex");
            let old_equip = n(current, "EquipItemSlotIndex");
            let preserve = matches!(req.text("IsPreserve"), "True" | "true" | "1");
            let mut rewards = Rewards::default();
            if old > 0 {
                if preserve {
                    out["CurrencyResult"] = hero::currency(
                        db,
                        account,
                        "Gem",
                        -state.tables.hero_shop.constant(
                            if old_equip > 0 {
                                "PunishmentRunePreservePrice"
                            } else {
                                "RunePreservePrice"
                            },
                            250,
                        ),
                    )
                    .await?;
                    if old_equip > 0 {
                        // Equipment runes remain in their dedicated storage; clearing the page releases them.
                    } else {
                        item::give(db, state, account, old as i32, 1, 0, 0, &mut rewards).await?;
                    }
                } else if old_equip > 0 {
                    remove_equip(db, account, old_equip as i32).await?;
                }
            } else if action == "unequip_rune" {
                return Err(rule("RuneNotFound"));
            }
            let mut new_id = 0;
            let mut new_equip = 0;
            if action == "equip_rune" {
                new_id = req.number("ItemIndex", 0)?;
                new_equip = req.number("EquipItemSlotIndex", 0)?;
                let data = if new_equip > 0 {
                    let eq = punishment::available(db, account, new_equip).await?;
                    if eq.item_index as i64 != new_id {
                        return Err(rule("InvalidRune"));
                    }
                    let data = row(state, "PunishmentRune", &[("PunishmentRuneIndex", new_id)])?;
                    put(db, account, "rune_equipment", new_equip, &json!(eq)).await?;
                    sqlx::query("UPDATE equip_items SET inventory_type=3 WHERE account_id=? AND slot_index=?").bind(account).bind(new_equip).execute(&mut *db).await?;
                    out["RemoveEquipItemSlotIndex"] = json!(new_equip);
                    data
                } else {
                    let data = row(state, "RuneItem", &[("ItemIndex", new_id)])?;
                    let consumed = item::consume(db, account, new_id as i32, 1).await?;
                    out["ItemResults"] = json!([consumed]);
                    data
                };
                if !data["SlotTypes"]
                    .as_array()
                    .is_some_and(|s| s.contains(&json!(slot)))
                {
                    return Err(rule("InvalidRuneSlot"));
                }
                super::super::progression::record(db, account, "EquipRune", 0, 0, 1).await?;
            } else if action != "unequip_rune" {
                return Err(rule("Fail"));
            }
            current["ItemIndex"] = json!(new_id);
            current["EquipItemSlotIndex"] = json!(new_equip);
            out["HeroRunePageInfo"] = current.clone();
            let r = item::reward_response(db, state, account, rewards).await?;
            let mut results = out["ItemResults"].as_array().cloned().unwrap_or_default();
            results.extend(r["ItemResults"].as_array().into_iter().flatten().cloned());
            out["ItemResults"] = json!(results);
        }
    }
    details["HeroRunePageInfos"] = json!(pages);
    if details["ApplyRunePage"].is_null() {
        details["ApplyRunePage"] = json!(1);
    }
    hero::save_details(db, account, hero_index, &details).await?;
    out["HeroInfo"] = hero::info(db, account, hero_index).await?;
    out["HeroRunePageInfos"] = details["HeroRunePageInfos"].clone();
    Ok(out)
}
fn new_page(hero: i32, page: i64, state: &AppState) -> Vec<Value> {
    (1..=state.tables.hero_shop.constant("MaxRuneSlotNum",5)).map(|slot|json!({"HeroIndex":hero,"RunePage":page,"SlotNum":slot,"EquipItemSlotIndex":0,"ItemIndex":0})).collect()
}
