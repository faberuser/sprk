use super::*;
use crate::models::item::ItemGrant;
use rand::RngExt;

pub(super) async fn execute(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if action == "soul_break" {
        return break_soul(db, state, account, req).await;
    }
    if action == "soul_weapon_limit_break" {
        return limit_break(db, state, account, req).await;
    }
    if matches!(
        action,
        "restore_soul_stone" | "get_soul_stone_mileage_reward" | "confirm_soul_stone"
    ) {
        return restore(db, state, account, req, action).await;
    }
    let slot = req.number(
        "EquipItemSlotIndex",
        req.number("TargetEquipItemSlotIndex", 0)?,
    )?;
    let eq = equip(db, account, slot).await?;
    let data = meta(state, eq.item_index)?;
    if data["IsOpenSoulWeapon"] != true {
        return Err(rule("InvalidEquipItem"));
    }
    let mut info = get(db, account, "soul", slot).await?;
    let mut out = item::success();
    if action == "soul_weapon_liberation" {
        if !info.is_null() {
            return Err(rule("AlreadyLiberated"));
        }
        let price = row(
            state,
            "SoulWeaponUpgradePrice",
            &[("ItemIndex", eq.item_index as i64), ("Grade", 0)],
        )?;
        out["CurrencyResult"] = hero::currency(db, account, "Gold", -n(price, "ReqGold")).await?;
        out["RemovedItemResultInfos"] = json!([item::consume(
            db,
            account,
            n(price, "MaterialItemIndex") as i32,
            n(price, "MaterialItemCount") as i32
        )
        .await?]);
        info = initial_info(state, slot, eq.item_index);
    } else {
        if info.is_null() {
            return Err(rule("SoulWeaponNotFound"));
        }
        match action {
            "soul_weapon_upgrade" => {
                let grade = n(&info, "Grade") + 1;
                row(state, "SoulWeaponUpgrade", &[("Grade", grade)])?;
                if n(&info, "ReinforceLevel")
                    < grade * state.tables.hero_shop.constant("SoulWeaponUpgradeRatio", 5)
                {
                    return Err(rule("NotEnoughReinforceLevel"));
                }
                let price = row(
                    state,
                    "SoulWeaponUpgradePrice",
                    &[("ItemIndex", eq.item_index as i64), ("Grade", grade)],
                )?;
                out["CurrencyResult"] =
                    hero::currency(db, account, "Gold", -n(price, "ReqGold")).await?;
                out["RemovedItemResultInfo"] = item::consume(
                    db,
                    account,
                    n(price, "MaterialItemIndex") as i32,
                    n(price, "MaterialItemCount") as i32,
                )
                .await?;
                info["Grade"] = json!(grade);
            }
            "soul_weapon_ether_injection" if req.text("SprkAutoEnhance") == "1" => {
                auto_enhance(db, state, account, &mut info, &mut out).await?;
            }
            "soul_weapon_ether_injection" => {
                let level = n(&info, "ReinforceLevel");
                row(
                    state,
                    "SoulWeaponReinforce",
                    &[("ReinforceLevel", level + 1)],
                )?;
                let ruledata = row(state, "SoulWeaponReinforce", &[("ReinforceLevel", level)])?;
                let pairs = equipment::item_pairs(req, "InjectionItemInfos")?;
                if pairs.is_empty() {
                    return Err(rule("InvalidMaterial"));
                }
                let mut cost = 0;
                let mut exp = n(&info, "Exp");
                let mut rate = n(&info, "ReinforceRatio");
                let mut consumed = vec![];
                for (id, count) in pairs {
                    let ether = row(state, "SoulWeaponEther", &[("ItemIndex", id)])?;
                    if ruledata["UnAvailableItemCode"]
                        .as_array()
                        .is_some_and(|a| a.contains(&ether["ItemCode"]))
                    {
                        return Err(rule("InvalidMaterial"));
                    }
                    let bonus = row(
                        state,
                        "SoulWeaponEtherRate",
                        &[("ReinforceLevel", level), ("ItemIndex", id)],
                    )?;
                    cost += n(ether, "ReqGold") * count;
                    exp += n(ether, "ExpPoints") * count;
                    rate += n(bonus, "AddedBonusRate") * count;
                    consumed.push(item::consume(db, account, id as i32, count as i32).await?);
                }
                let max = state
                    .tables
                    .hero_shop
                    .constant("SoulWeaponReinforceMaxRate", 100000);
                if n(&info, "Exp") >= n(ruledata, "RequiredExp")
                    && n(&info, "ReinforceRatio") >= max
                {
                    return Err(rule("AlreadyMax"));
                }
                out["CurrencyResult"] = hero::currency(db, account, "Gold", -cost).await?;
                out["RemovedItemResultInfos"] = json!(consumed);
                info["Exp"] = json!(exp.min(n(ruledata, "RequiredExp")));
                info["ReinforceRatio"] = json!(rate.min(max));
            }
            "soul_weapon_reinforce" => {
                let level = n(&info, "ReinforceLevel");
                row(
                    state,
                    "SoulWeaponReinforce",
                    &[("ReinforceLevel", level + 1)],
                )?;
                let r = row(state, "SoulWeaponReinforce", &[("ReinforceLevel", level)])?;
                if n(&info, "Exp") < n(r, "RequiredExp")
                    || n(&info, "ReinforceRatio") < n(r, "SuccessRateMark") * 10
                {
                    return Err(rule("NotEnoughExp"));
                }
                out["CurrencyResult"] =
                    hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
                let success = roll(
                    n(&info, "ReinforceRatio"),
                    state
                        .tables
                        .hero_shop
                        .constant("SoulWeaponReinforceMaxRate", 100000),
                );
                if success {
                    info["ReinforceLevel"] = json!(level + 1);
                }
                info["Exp"] = json!(0);
                info["ReinforceRatio"] = json!(0);
                out["ReinforceSuccess"] = json!(success);
            }
            "soul_weapon_renew_option" => {
                if info["PendingRenew"] == true {
                    return Err(rule("UnconfirmedOption"));
                }
                if let Some(target) = RenewTarget::parse(req)? {
                    if matches!(req.text("SprkPreview"), "1" | "true" | "True") {
                        return renew_preview(db, account, slot, target).await;
                    }
                    renew_until(db, state, account, req, slot, &mut info, &mut out, target).await?;
                    stats(state, &mut info)?;
                    put(db, account, "soul", slot, &info).await?;
                    out["ResultSoulWeaponInfo"] = info;
                    return Ok(out);
                }
                let coupon = req.number("CouponItemIndex", 0)?;
                if coupon > 0 {
                    if coupon
                        != state
                            .tables
                            .hero_shop
                            .constant("SoulWeaponRenewTicketItemIndex", 2202)
                    {
                        return Err(rule("InvalidMaterial"));
                    }
                    out["RemovedItemResultInfos"] =
                        json!([item::consume(db, account, coupon as i32, 1).await?]);
                } else {
                    let prices: Vec<i64> = serde_json::from_str(
                        state
                            .tables
                            .hero_shop
                            .constants
                            .get("SoulWeaponRenewGem")
                            .ok_or_else(|| rule("ItemDataNotFound"))?,
                    )
                    .map_err(|_| rule("ItemDataNotFound"))?;
                    let price = *prices
                        .get(
                            (n(&info, "RenewOptionCount") as usize)
                                .min(prices.len().saturating_sub(1)),
                        )
                        .ok_or_else(|| rule("ItemDataNotFound"))?;
                    out["CurrencyResult"] = hero::currency(db, account, "Gem", -price).await?;
                    out["RemovedItemResultInfos"] = json!([]);
                }
                set_renew(&mut info, RenewTarget::ANY.roll());
                info["RenewOptionCount"] = json!(n(&info, "RenewOptionCount") + 1);
                info["PendingRenew"] = json!(true);
                super::super::progression::record(db, account, "SoulWeaponRenewOption", 0, 0, 1)
                    .await?;
            }
            "soul_weapon_confirm_renew_option" => {
                if info["PendingRenew"] != true {
                    return Err(rule("UnconfirmedOption"));
                }
                if matches!(req.text("IsNew"), "true" | "True" | "1") {
                    for key in [
                        "OptionRatio1",
                        "OptionRatio2",
                        "OptionBonusRatio1",
                        "OptionBonusRatio2",
                    ] {
                        info[key] = info[format!("Renew{key}")].clone();
                    }
                }
                for key in [
                    "OptionRatio1",
                    "OptionRatio2",
                    "OptionBonusRatio1",
                    "OptionBonusRatio2",
                ] {
                    info[format!("Renew{key}")] = json!(0);
                }
                info["PendingRenew"] = json!(false);
            }
            "soul_weapon_transition" => {
                let target = req.number("ReceiveEquipItemSlotIndex", 0)?;
                if target == slot || info["PendingRenew"] == true {
                    return Err(rule("InvalidEquipItem"));
                }
                let other = equip(db, account, target).await?;
                if eq.locked != 0
                    || other.locked != 0
                    || other.item_index != eq.item_index
                    || !get(db, account, "soul", target).await?.is_null()
                {
                    return Err(rule("InvalidEquipItem"));
                }
                out["CurrencyResult"] = hero::currency(
                    db,
                    account,
                    "Gold",
                    -state
                        .tables
                        .hero_shop
                        .constant("SoulWeaponTransition", 10000000),
                )
                .await?;
                info["EquipItemSlotIndex"] = json!(target);
                put(db, account, "soul", target, &info).await?;
                crate::db::query(
                    "DELETE FROM extension_state WHERE account_id=$1 AND kind='soul' AND idx=$2",
                )
                .bind(account)
                .bind(slot)
                .execute(db)
                .await?;
                out["ResultSoulWeaponInfo"] = info;
                return Ok(out);
            }
            _ => return Err(rule("Fail")),
        }
    }
    stats(state, &mut info)?;
    put(db, account, "soul", slot, &info).await?;
    if action == "soul_weapon_liberation" {
        crate::api::progression::play_record::liberated(db, account).await?;
    }
    out["ResultSoulWeaponInfo"] = info;
    Ok(out)
}
/// SPRK "Auto Enhance": repeats what a player does by hand with Max, Add Ether
/// and Attempt Enhancement until +20 or until the Ether or Gold runs out. Each
/// level is filled to full Ether and the maximum success rate (100%) before it
/// is attempted, with the same rates, costs and roll as the manual steps.
/// Ether is spent from the lowest tier up. Ether that cannot complete a level
/// stays injected, as it would after a manual Add Ether, and is not gambled.
async fn auto_enhance(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    info: &mut Value,
    out: &mut Value,
) -> Result<()> {
    let max = state
        .tables
        .hero_shop
        .constant("SoulWeaponReinforceMaxRate", 100000);
    let start = n(info, "ReinforceLevel");
    if state
        .tables
        .extensions
        .find("SoulWeaponReinforce", &[("ReinforceLevel", start + 1)])
        .is_none()
    {
        return Err(rule("AlreadyMaxLevel"));
    }
    // Spendable balances, excluding amounts reserved by battle entries.
    let held = crate::api::battle::entry_costs::held(db, account, "Currency", 3).await?;
    let balance: i64 = crate::db::query_scalar("SELECT gold FROM user_info WHERE account_id=$1")
        .bind(account)
        .fetch_one(&mut *db)
        .await?;
    let mut gold = (balance - held).max(0);
    let mut ethers: Vec<&Value> = state
        .tables
        .extensions
        .rows("SoulWeaponEther")
        .iter()
        .collect();
    ethers.sort_by_key(|e| (n(e, "ExpPoints"), n(e, "ItemIndex")));
    let mut owned = std::collections::BTreeMap::new();
    for ether in &ethers {
        let id = n(ether, "ItemIndex");
        let count: Option<i64> = crate::db::query_scalar(
            "SELECT count::bigint FROM items WHERE account_id=$1 AND item_index=$2 AND locked=0",
        )
        .bind(account)
        .bind(id as i32)
        .fetch_optional(&mut *db)
        .await?;
        let reserved = crate::api::battle::entry_costs::held(db, account, "Item", id).await?;
        owned.insert(id, (count.unwrap_or(0) - reserved).max(0));
    }
    let mut used = std::collections::BTreeMap::<i64, i64>::new();
    let (mut cost, mut attempts, mut successes) = (0i64, 0i64, 0i64);
    let mut short_of_gold = false;
    let mut stop = "MaxLevel";
    loop {
        let level = n(info, "ReinforceLevel");
        let Some(rule_row) = state
            .tables
            .extensions
            .find("SoulWeaponReinforce", &[("ReinforceLevel", level)])
        else {
            break;
        };
        if state
            .tables
            .extensions
            .find("SoulWeaponReinforce", &[("ReinforceLevel", level + 1)])
            .is_none()
        {
            break;
        }
        let required = n(rule_row, "RequiredExp");
        let blocked = rule_row["UnAvailableItemCode"].as_array();
        for ether in &ethers {
            let exp_left = required - n(info, "Exp");
            let rate_left = max - n(info, "ReinforceRatio");
            if exp_left <= 0 && rate_left <= 0 {
                break;
            }
            if blocked.is_some_and(|codes| codes.contains(&ether["ItemCode"])) {
                continue;
            }
            let id = n(ether, "ItemIndex");
            let Some(bonus) = state.tables.extensions.find(
                "SoulWeaponEtherRate",
                &[("ReinforceLevel", level), ("ItemIndex", id)],
            ) else {
                continue;
            };
            let (exp, rate, price) = (
                n(ether, "ExpPoints"),
                n(bonus, "AddedBonusRate"),
                n(ether, "ReqGold"),
            );
            let need = |left: i64, per: i64| if left > 0 && per > 0 { (left + per - 1) / per } else { 0 };
            let mut count = need(exp_left, exp).max(need(rate_left, rate)).min(owned[&id]);
            if price > 0 && count > gold / price {
                count = gold / price;
                short_of_gold = true;
            }
            if count <= 0 {
                continue;
            }
            gold -= price * count;
            cost += price * count;
            *owned.get_mut(&id).unwrap() -= count;
            *used.entry(id).or_default() += count;
            info["Exp"] = json!((n(info, "Exp") + exp * count).min(required));
            info["ReinforceRatio"] = json!((n(info, "ReinforceRatio") + rate * count).min(max));
        }
        let price = n(rule_row, "ReqGold");
        if n(info, "Exp") < required || n(info, "ReinforceRatio") < max {
            stop = if short_of_gold { "Gold" } else { "Ether" };
            break;
        }
        if gold < price {
            short_of_gold = true;
            stop = "Gold";
            break;
        }
        gold -= price;
        cost += price;
        attempts += 1;
        // The same roll as soul_weapon_reinforce; certain at the maximum rate.
        if roll(n(info, "ReinforceRatio"), max) {
            info["ReinforceLevel"] = json!(level + 1);
            successes += 1;
        }
        info["Exp"] = json!(0);
        info["ReinforceRatio"] = json!(0);
    }
    if used.is_empty() && attempts == 0 {
        return Err(rule(if short_of_gold { "NotEnoughGold" } else { "NotEnoughEther" }));
    }
    let mut removed = vec![];
    for (id, count) in used {
        removed.push(item::consume(db, account, id as i32, count as i32).await?);
    }
    out["CurrencyResult"] = hero::currency(db, account, "Gold", -cost).await?;
    out["RemovedItemResultInfos"] = json!(removed);
    out["SprkAutoStartLevel"] = json!(start);
    out["SprkAutoAttempts"] = json!(attempts);
    out["SprkAutoSuccesses"] = json!(successes);
    out["SprkAutoGold"] = json!(cost);
    out["SprkAutoStop"] = json!(stop);
    Ok(())
}

/// A soul weapon stat roll: ATK share of the split and the two bonuses, in the
/// stored 1/1000 units (HP's share is 1000 minus ATK's).
type Roll = (i64, i64, i64);

fn set_renew(info: &mut Value, (atk, atk_bonus, hp_bonus): Roll) {
    info["RenewOptionRatio1"] = json!(atk);
    info["RenewOptionRatio2"] = json!(1000 - atk);
    info["RenewOptionBonusRatio1"] = json!(atk_bonus);
    info["RenewOptionBonusRatio2"] = json!(hp_bonus);
}

/// SPRK "Rearrange until" target: ATK share within `atk_min..=atk_max` and each
/// bonus at least its minimum (0 accepts any bonus). Every value of a roll is
/// uniform and independent, so the odds are exact.
#[derive(Clone, Copy, Debug, PartialEq)]
pub(super) struct RenewTarget {
    atk_min: i64,
    atk_max: i64,
    atk_bonus: i64,
    hp_bonus: i64,
}

/// The native roll ranges.
const RATIO_MIN: i64 = 300;
const RATIO_MAX: i64 = 700;
const BONUS_MAX: i64 = 200;

/// Most rolls one "rearrange until" request may make, and the latest the
/// guarantee can land (rolls run in memory, so a long run is cheap).
pub(super) const SOUL_RENEW_UNTIL_MAX_TRIES: i64 = 100_000;

/// The guarantee lands at this many times the expected number of rolls for the
/// target, at most `SOUL_RENEW_UNTIL_MAX_TRIES`.
const SOUL_RENEW_PITY_FACTOR: i64 = 3;

impl RenewTarget {
    const ANY: Self = Self { atk_min: RATIO_MIN, atk_max: RATIO_MAX, atk_bonus: 0, hp_bonus: 0 };

    /// `None` for a native request (no `SprkAtkMin`).
    fn parse(req: &Request) -> Result<Option<Self>> {
        if req.text("SprkAtkMin").is_empty() {
            return Ok(None);
        }
        let target = Self {
            atk_min: req.number("SprkAtkMin", -1)?,
            atk_max: req.number("SprkAtkMax", -1)?,
            atk_bonus: req.number("SprkAtkBonusMin", 0)?,
            hp_bonus: req.number("SprkHpBonusMin", 0)?,
        };
        let valid = RATIO_MIN <= target.atk_min
            && target.atk_min <= target.atk_max
            && target.atk_max <= RATIO_MAX
            && (0..=BONUS_MAX).contains(&target.atk_bonus)
            && (0..=BONUS_MAX).contains(&target.hp_bonus);
        if !valid {
            return Err(rule("RenewOptionRaioFail"));
        }
        Ok(Some(target))
    }

    /// Winning outcomes and all outcomes; their ratio is the chance per roll.
    fn odds(&self) -> (i64, i64) {
        let hits = (self.atk_max - self.atk_min + 1)
            * (BONUS_MAX - self.atk_bonus + 1)
            * (BONUS_MAX - self.hp_bonus + 1);
        let all = (RATIO_MAX - RATIO_MIN + 1) * (BONUS_MAX + 1) * (BONUS_MAX + 1);
        (hits, all)
    }

    /// The roll that triggers the guarantee: `SOUL_RENEW_PITY_FACTOR` × the
    /// expected rolls, rounded up, at most one run.
    fn pity_limit(&self) -> i64 {
        let (hits, all) = self.odds();
        ((SOUL_RENEW_PITY_FACTOR * all + hits - 1) / hits).clamp(1, SOUL_RENEW_UNTIL_MAX_TRIES)
    }

    /// Saved with the guarantee progress: a different target starts over.
    fn key(&self) -> String {
        format!("{}_{}_{}_{}", self.atk_min, self.atk_max, self.atk_bonus, self.hp_bonus)
    }

    fn hits(&self, (atk, atk_bonus, hp_bonus): Roll) -> bool {
        (self.atk_min..=self.atk_max).contains(&atk) && atk_bonus >= self.atk_bonus && hp_bonus >= self.hp_bonus
    }

    /// A uniform roll inside the target (the native roll for `ANY`).
    fn roll(&self) -> Roll {
        let mut rng = rand::rng();
        (
            rng.random_range(self.atk_min..=self.atk_max),
            rng.random_range(self.atk_bonus..=BONUS_MAX),
            rng.random_range(self.hp_bonus..=BONUS_MAX),
        )
    }
}

/// `SprkPreview=1`: the exact odds and guarantee progress, without charging.
async fn renew_preview(db: &mut PgConnection, account: i64, slot: i64, target: RenewTarget) -> Result<Value> {
    let (hits, all) = target.odds();
    let mut out = item::success();
    out["SprkHits"] = json!(hits);
    out["SprkOutcomes"] = json!(all);
    out["SprkPityMisses"] =
        json!(equipment::pity_misses(db, account, slot as i32, "soul_renew", &target.key()).await?);
    out["SprkPityLimit"] = json!(target.pity_limit());
    Ok(out)
}

/// SPRK "Rearrange until": rearranges until the stats meet `target`.
///
/// Up to `SprkMaxTries` rolls run in memory, each paid like a native rearrange:
/// a Rearrange ticket while any are left, then the gem price for the weapon's
/// rearrange count. The run stops at the first hit or when the next roll is
/// unaffordable. Costs are charged once and only the last roll is left pending,
/// so the player keeps or takes it in the native confirmation.
///
/// Guarantee: misses add up across runs for this weapon and target
/// (`sprk_target_pity`); the `RenewTarget::pity_limit`th accumulated roll lands
/// inside the target.
#[allow(clippy::too_many_arguments)]
async fn renew_until(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    slot: i64,
    info: &mut Value,
    out: &mut Value,
    target: RenewTarget,
) -> Result<()> {
    let max_tries = req.number("SprkMaxTries", 0)?;
    if !(1..=SOUL_RENEW_UNTIL_MAX_TRIES).contains(&max_tries) {
        return Err(rule("RenewOptionRaioFail"));
    }
    let ticket = state.tables.hero_shop.constant("SoulWeaponRenewTicketItemIndex", 2202);
    let prices: Vec<i64> = state
        .tables
        .hero_shop
        .constants
        .get("SoulWeaponRenewGem")
        .and_then(|p| serde_json::from_str(p).ok())
        .filter(|p: &Vec<i64>| !p.is_empty())
        .ok_or_else(|| rule("EmptyPriceInfo"))?;
    let tickets = equipment::spendable_item(db, account, ticket).await?;
    let mut gems = equipment::spendable_gem(db, account).await?;
    let pity_limit = target.pity_limit();
    let key = target.key();
    let mut misses = equipment::pity_misses(db, account, slot as i32, "soul_renew", &key).await?;
    let mut count = n(info, "RenewOptionCount");
    let (mut tries, mut tickets_used, mut spent) = (0i64, 0i64, 0i64);
    let (mut last, mut hit, mut guaranteed) = (None, false, false);
    while tries < max_tries {
        if tickets_used < tickets {
            tickets_used += 1;
        } else {
            let price = prices[(count as usize).min(prices.len() - 1)];
            if price > gems {
                break;
            }
            gems -= price;
            spent += price;
        }
        tries += 1;
        count += 1;
        guaranteed = misses + 1 >= pity_limit;
        let roll = if guaranteed { target.roll() } else { RenewTarget::ANY.roll() };
        last = Some(roll);
        hit = target.hits(roll);
        if hit {
            misses = 0;
            break;
        }
        misses += 1;
    }
    let Some(roll) = last else {
        return Err(rule("NotEnoughGem"));
    };
    let mut removed = vec![];
    if tickets_used > 0 {
        removed.push(item::consume(db, account, ticket as i32, tickets_used as i32).await?);
    }
    out["RemovedItemResultInfos"] = json!(removed);
    if spent > 0 {
        out["CurrencyResult"] = hero::currency(db, account, "Gem", -spent).await?;
    }
    set_renew(info, roll);
    info["RenewOptionCount"] = json!(count);
    info["PendingRenew"] = json!(true);
    out["SprkTries"] = json!(tries);
    out["SprkHit"] = json!(hit);
    out["SprkTicketsUsed"] = json!(tickets_used);
    out["SprkGemsSpent"] = json!(spent);
    out["SprkGuaranteed"] = json!(guaranteed);
    out["SprkPityMisses"] = json!(misses);
    out["SprkPityLimit"] = json!(pity_limit);
    equipment::save_pity(db, account, slot as i32, "soul_renew", &key, misses).await?;
    super::super::progression::record(db, account, "SoulWeaponRenewOption", 0, 0, tries).await?;
    Ok(())
}

pub(super) fn initial_info(state: &AppState, slot: i64, item: i32) -> Value {
    json!({"EquipItemSlotIndex":slot,"ItemIndex":item,"Grade":0,"ReinforceLevel":0,"ReinforceRatio":0,"Exp":0,"OptionRatio1":500,"OptionRatio2":500,"OptionBonusRatio1":0,"OptionBonusRatio2":0,"RenewOptionCount":0,"CreatedTime":state.server_time_str()})
}

pub(super) fn stats(state: &AppState, info: &mut Value) -> Result<()> {
    let data = meta(state, n(info, "ItemIndex") as i32)?;
    let option = row(
        state,
        "SoulWeaponOption",
        &[("TagType", data["TagType"][0].as_i64().unwrap_or(0))],
    )?;
    let reinforce = row(
        state,
        "SoulWeaponReinforce",
        &[("ReinforceLevel", n(info, "ReinforceLevel"))],
    )?;
    let upgrade = row(state, "SoulWeaponUpgrade", &[("Grade", n(info, "Grade"))])?;
    let factor = (1000 + n(reinforce, "StatRate")) * (1000 + n(upgrade, "UpgradeRate")) / 1000;
    for i in 1..=2 {
        // Build-1558 coefficients are 7/8/9 ATK and 250 HP. The legacy
        // calculator divides here once; later clients use coefficients 1000
        // times larger and an extra division. Keep client/login/renewal equal.
        let base = n(info, &format!("OptionRatio{i}"))
            * n(option, &format!("OptionRatioPerValue{i}"))
            * factor
            / 1000;
        info[format!("OptionStat{i}")] = json!(base);
        info[format!("BonusOptionStat{i}")] =
            json!(base * n(info, &format!("OptionBonusRatio{i}")) / 1000);
    }
    Ok(())
}

async fn break_soul(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let slots = ids(req, "EquipItemSlotIndices")?;
    if slots.is_empty() {
        return Err(rule("InvalidMaterial"));
    }
    let mut rewards = Rewards::default();
    for slot in slots {
        let eq = equip(db, account, slot).await?;
        let info = get(db, account, "soul", slot).await?;
        if info.is_null() || info["PendingRenew"] == true {
            return Err(rule("SoulWeaponNotFound"));
        }
        // Removing the soul record inside this transaction lets the ordinary material checks apply.
        crate::db::query("DELETE FROM extension_state WHERE account_id=$1 AND kind='soul' AND idx=$2")
            .bind(account)
            .bind(slot)
            .execute(&mut *db)
            .await?;
        material(db, account, slot).await?;
        let tag = meta(state, eq.item_index)?["TagType"][0]
            .as_i64()
            .unwrap_or(0);
        let r = row(
            state,
            "SoulBreak",
            &[
                ("TagType", tag),
                ("Grade", n(&info, "Grade")),
                ("ReinforceLevel", n(&info, "ReinforceLevel")),
            ],
        )?;
        for reward in r["Rewards"].as_array().into_iter().flatten() {
            let count = rand::rng().random_range(n(reward, "Min")..=n(reward, "Max"));
            if count > 0 {
                item::give(
                    db,
                    state,
                    account,
                    ItemGrant {
                        index: n(reward, "ItemIndex") as i32,
                        count: count as i32,
                        star: 0,
                        custom: 0,
                    },
                    &mut rewards,
                )
                .await?;
            }
        }
        remove_equip(db, account, eq.slot_index).await?;
    }
    Ok(json!({"BaseResult":"Success","Result":"Success","ItemResults":rewards.items}))
}
async fn limit_break(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let slot = req.number("EquipItemSlotIndex", 0)?;
    let mut eq = equip(db, account, slot).await?;
    if get(db, account, "soul", slot).await?.is_null() {
        return Err(rule("SoulWeaponNotFound"));
    }
    let data = meta(state, eq.item_index)?;
    // ContentsDefine.SoulWeaponLimitBreak was not shipped with the extracted tables.
    // An administrator can supply its native fields without changing the handler.
    let r = row(
        state,
        "SoulWeaponLimitBreak",
        &[
            ("DetailIndex", n(data, "DetailIndex")),
            ("Star", eq.star as i64 + 1),
        ],
    )?;
    let materials = ids(req, "MaterialSlotIndices")?;
    if materials.len() as i64 != n(r, "WeaponUniqueCount") {
        return Err(rule("InvalidMaterial"));
    }
    for material_slot in &materials {
        if *material_slot == slot {
            return Err(rule("InvalidMaterial"));
        }
        let m = material(db, account, *material_slot).await?;
        if m.item_index != eq.item_index {
            return Err(rule("InvalidMaterial"));
        }
    }
    let stone = n(
        row(
            state,
            "SoulWeaponUpgradePrice",
            &[("ItemIndex", eq.item_index as i64), ("Grade", 0)],
        )?,
        "MaterialItemIndex",
    );
    let expected: std::collections::BTreeMap<_, _> = [
        (stone, n(r, "SoulStoneCount")),
        (n(r, "TransStoneIndex"), n(r, "TransStoneCount")),
    ]
    .into_iter()
    .filter(|(_, c)| *c > 0)
    .collect();
    let sent: std::collections::BTreeMap<_, _> = equipment::item_pairs(req, "MaterialItemInfors")?
        .into_iter()
        .collect();
    if expected != sent {
        return Err(rule("InvalidMaterial"));
    }
    let currency = hero::currency(
        db,
        account,
        "Gold",
        -n(r, "GoldCount") * materials.len() as i64,
    )
    .await?;
    let mut removed = vec![];
    for (id, count) in expected {
        removed.push(item::consume(db, account, id as i32, count as i32).await?);
    }
    for slot in &materials {
        remove_equip(db, account, *slot as i32).await?;
    }
    let success = roll(
        n(r, "SuccessRatio") + eq.upgrade_star_fail_bonus as i64,
        100,
    );
    if success {
        eq.star += 1;
        eq.upgrade_star_fail_bonus = 0;
    } else {
        eq.upgrade_star_fail_bonus += n(r, "FailBonusRatio") as i32;
    }
    save_equip(db, account, &eq).await?;
    Ok(
        json!({"BaseResult":"Success","Result":"Success","Success":success,"ResultEquipItem":eq,"CurrencyResult":currency,"ItemResults":[],"RemovedEquipItemSlotIndices":materials,"RemovedItemResultInfos":removed}),
    )
}

async fn restore(
    db: &mut PgConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let mut progress = get(db, account, "soul_restore", 0).await?;
    if progress.is_null() {
        progress = json!({"Mileage":0,"Choices":[]});
    }
    let mut rewards = Rewards::default();
    let mut removed = vec![];
    let mut out = item::success();
    if action == "confirm_soul_stone" {
        let id = req.number("SoulStoneItemIndex", 0)?;
        if !progress["Choices"]
            .as_array()
            .is_some_and(|a| a.contains(&json!(id)))
        {
            return Err(rule("InvalidSoulStoneIndex"));
        }
        item::give(
            db,
            state,
            account,
            ItemGrant {
                index: id as i32,
                count: 1,
                star: 0,
                custom: 0,
            },
            &mut rewards,
        )
        .await?;
        progress["Choices"] = json!([]);
        let r = item::reward_response(db, state, account, rewards).await?;
        out["ResultSoulStoneItemInfo"] = r["ItemResults"].get(0).cloned().unwrap_or(Value::Null);
    } else {
        if progress["Choices"]
            .as_array()
            .is_some_and(|a| !a.is_empty())
        {
            return Err(rule("UnconfirmedSoulStone"));
        }
        let requested_index = req.number("SoulStoneRestoreIndex", 0)?;
        let index = if action == "get_soul_stone_mileage_reward" && requested_index == 0 { 1 } else { requested_index };
        let r = state
            .tables
            .extensions
            .rows("SoulStoneRestore")
            .get(usize::try_from(index - 1).map_err(|_| rule("InvalidSoulStoneRestoreIndex"))?)
            .ok_or_else(|| rule("InvalidSoulStoneRestoreIndex"))?;
        let bonus = action == "get_soul_stone_mileage_reward";
        let max = state.tables.hero_shop.constant("MaxSoulStoneMileage", 20);
        let config = state.tables.extensions.find("LocalSoulStoneRestore", &[("Index", index)])
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        let soulstone = if bonus {
            if n(&progress, "Mileage") < max { return Err(rule("NotEnoughMileage")); }
            progress["Mileage"] = json!(n(&progress, "Mileage") - max);
            true
        } else {
            if n(&progress, "Mileage") >= max { return Err(rule("InvalidSoulStoneRestoreIndex")); }
            removed.push(item::consume(db, account, n(r, "MaterialItemIndex") as i32,
                n(r, "MaterialItemCount") as i32).await?);
            super::super::progression::record(db, account, "SoulRestore", 0, 0, 1).await?;
            rand::rng().random_range(0..10000) < n(config, "SoulStoneChancePer10000")
        };
        if soulstone {
            let protection: i64 = crate::db::query_scalar("SELECT COALESCE((SELECT count FROM items WHERE account_id=$1 AND item_index=5251),0)")
                .bind(account).fetch_one(&mut *db).await?;
            let protected = protection > 0;
            if protected { removed.push(item::consume(db, account, 5251, 1).await?); }
            let group = r["SoulStoneItemGroup"].as_str().unwrap_or("");
            let local = state
                .tables
                .extensions
                .find("LocalSoulStoneRestore", &[("Index", index)]);
            let mut local_pool = BTreeSet::new();
            if group.is_empty() {
                let config = local.ok_or_else(|| rule("ItemDataNotFound"))?;
                local_pool.extend(
                    config["ItemIndices"]
                        .as_array()
                        .into_iter()
                        .flatten()
                        .filter_map(Value::as_i64)
                        .map(|v| v as i32),
                );
                if config["UseLiberationStonePool"] == true {
                    for price in state
                        .tables
                        .extensions
                        .rows("SoulWeaponUpgradePrice")
                        .iter()
                        .filter(|v| n(v, "Grade") == 0)
                    {
                        let data = meta(state, n(price, "ItemIndex") as i32)?;
                        let npc = data["CreatureIndex"]
                            .as_array()
                            .into_iter()
                            .flatten()
                            .filter_map(Value::as_i64)
                            .any(|id| {
                                state
                                    .tables
                                    .extensions
                                    .rows("NPCFriendlyPoint")
                                    .iter()
                                    .any(|r| n(r, "HeroIndex") == id)
                            });
                        if !npc || config["IncludeNpcHeroes"] == true {
                            local_pool.insert(n(price, "MaterialItemIndex") as i32);
                        }
                    }
                }
            }
            let mut choices = BTreeSet::new();
            let needed = if protected { state.tables.hero_shop.constant("SelectSoulStoneCount", 3) } else { 1 };
            for _ in 0..100 {
                if choices.len() as i64 >= needed {
                    break;
                }
                let id = if group.is_empty() {
                    let pool: Vec<_> = local_pool
                        .iter()
                        .copied()
                        .filter(|id| !choices.contains(id))
                        .collect();
                    if pool.is_empty() {
                        return Err(rule("ItemDataNotFound"));
                    }
                    pool[rand::rng().random_range(0..pool.len())]
                } else {
                    state
                        .tables
                        .roll_item_from_group_code(group, &[])
                        .ok_or_else(|| rule("ItemDataNotFound"))?
                        .0
                };
                if state.tables.items.reward_item(id).is_none() {
                    return Err(rule("ItemDataNotFound"));
                }
                choices.insert(id);
            }
            if choices.len() as i64 != needed {
                return Err(rule("ItemDataNotFound"));
            }
            if protected {
                progress["Choices"] = json!(choices);
            } else {
                item::give(
                    db,
                    state,
                    account,
                    ItemGrant {
                        index: *choices.iter().next().unwrap(),
                        count: 1,
                        star: 0,
                        custom: 0,
                    },
                    &mut rewards,
                )
                .await?;
            }
        } else {
            // The Soulstone branch was already rolled above. Condition the native
            // judgment group on its Ether entries, retaining their weights/counts.
            let (group, filter) = crate::tables::parse_item_code(
                config["EtherItemGroup"]
                    .as_str()
                    .ok_or_else(|| rule("ItemDataNotFound"))?,
            );
            let (index, count, star, _) = state
                .tables
                .roll_item_from_group_code(&group, &filter)
                .ok_or_else(|| rule("ItemDataNotFound"))?;
            item::give(
                db,
                state,
                account,
                ItemGrant {
                    index,
                    count,
                    star,
                    custom: 0,
                },
                &mut rewards,
            )
            .await?;
            // Native SoulStoneMileageItemType is Ether, not every judgment.
            progress["Mileage"] = json!((n(&progress, "Mileage") + 1).min(max));
        }
        let r = item::reward_response(db, state, account, rewards).await?;
        out["CurrencyResults"] = r["CurrencyResults"].clone();
        out["ItemResults"] = r["ItemResults"].clone();
        out["EquipItemResults"] = r["EquipItemResults"].clone();
        out["RemovedItemResultInfos"] = json!(removed);
        out["SoulStoneMileage"] = progress["Mileage"].clone();
        out["SelectSoulStoneIndices"] = progress["Choices"].clone();
    }
    put(db, account, "soul_restore", 0, &progress).await?;
    Ok(out)
}
