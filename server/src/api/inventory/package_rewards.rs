//! Coalesce positive package grants without changing per-roll client results.
use std::collections::BTreeMap;

use super::{data, give, n, rule, AppState, ItemGrant, PgConnection, Result, Rewards, Value};
use serde_json::json;

#[derive(Default)]
pub(super) struct PackageRewards {
    enabled: bool,
    currencies: BTreeMap<String, (Value, i64)>,
    items: BTreeMap<i32, (Value, i32, i64)>,
}

impl PackageRewards {
    pub(super) fn new() -> Self {
        Self { enabled: true, ..Self::default() }
    }

    pub(super) async fn currency(
        &mut self,
        db: &mut PgConnection,
        state: &AppState,
        account: i64,
        kind: &str,
        amount: i64,
        rewards: &mut Rewards,
    ) -> Result<()> {
        // These paths may clamp a balance or affect state outside the reward row.
        if !self.enabled || amount <= 0
            || matches!(kind, "ShakmehMiddleBossPoint" | "PayGem" | "FriendshipPoint")
        {
            self.flush(db, state, account).await?;
            rewards.currencies.push(crate::api::heroes::currency(db, account, kind, amount).await?);
            return Ok(());
        }
        if let Some((last, pending)) = self.currencies.get_mut(kind) {
            let overflow = || rule(&format!("NotEnough{kind}"));
            let new_value = n(last, "NewValue").checked_add(amount).ok_or_else(overflow)?;
            *pending = pending.checked_add(amount).ok_or_else(overflow)?;
            last["AddValue"] = json!(amount);
            last["NewValue"] = json!(new_value);
            if kind == "Gem" {
                // Ordinary ruby drops add free rubies; paid rubies stay unchanged.
                last["Value1"] = json!(n(last, "Value1").checked_add(amount).ok_or_else(overflow)?);
            }
            rewards.currencies.push(last.clone());
        } else {
            // Use the canonical grant once to retain its validation and wire fields.
            let result = crate::api::heroes::currency(db, account, kind, amount).await?;
            self.currencies.insert(kind.to_owned(), (result.clone(), 0));
            rewards.currencies.push(result);
        }
        Ok(())
    }

    pub(super) async fn give(
        &mut self,
        db: &mut PgConnection,
        state: &AppState,
        account: i64,
        grant: ItemGrant,
        rewards: &mut Rewards,
    ) -> Result<()> {
        if grant.count <= 0 { return Err(rule("InvalidItemCount")); }
        if crate::models::item::disabled_reward(grant.index) { return Ok(()); }
        if let Some((last, pending, cap)) = self.items.get_mut(&grant.index) {
            let new_count = n(last, "NewCount") + i64::from(grant.count);
            if new_count > i64::from(i32::MAX) || (*cap > 0 && new_count > *cap) {
                return Err(rule("InvalidItemCount"));
            }
            *pending = pending.checked_add(grant.count).ok_or_else(|| rule("InvalidItemCount"))?;
            last["AddCount"] = json!(grant.count);
            last["NewCount"] = json!(new_count);
            rewards.items.push(last.clone());
            return Ok(());
        }
        let ordinary = self.enabled
            && state.tables.items.reward_item(grant.index).is_some_and(|m| m.kind == "Item")
            && state.tables.tutorials.support.items.get(&grant.index).is_none_or(|m| m.kind == "Item")
            && state.tables.live.find("Pet", &[("Index", i64::from(grant.index))]).is_none();
        if !ordinary {
            // Flush and discard cached balances before grants that may convert
            // duplicates, award other items/currencies, or inspect current state.
            self.flush(db, state, account).await?;
            return Box::pin(give(db, state, account, grant, rewards)).await;
        }
        let index = grant.index;
        let cap = n(data(state, index)?, "ItemMaxCap");
        let start = rewards.items.len();
        give(db, state, account, grant, rewards).await?;
        if rewards.items.len() == start + 1 {
            let result = rewards.items[start].clone();
            self.items.insert(index, (result, 0, cap));
        }
        Ok(())
    }

    pub(super) async fn flush(
        &mut self,
        db: &mut PgConnection,
        state: &AppState,
        account: i64,
    ) -> Result<()> {
        for (kind, (_, amount)) in std::mem::take(&mut self.currencies) {
            if amount != 0 {
                crate::api::heroes::currency(db, account, &kind, amount).await?;
            }
        }
        for (index, (_, count, _)) in std::mem::take(&mut self.items) {
            if count != 0 {
                // Preserve caps, first-grant timestamps, locks and gameplay triggers.
                Box::pin(give(db, state, account,
                    ItemGrant { index, count, star: 0, custom: 0 }, &mut Rewards::default())).await?;
            }
        }
        Ok(())
    }
}
