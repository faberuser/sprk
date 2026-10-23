use super::*;

fn repair_final_claim(state: &AppState, info: &mut Value) -> bool {
    let id=n(info,"HeroIndex");
    let rows=state.tables.extensions.rows("NPCFriendlyPoint");
    let Some(last)=rows.iter().filter(|r|n(r,"HeroIndex")==id).max_by_key(|r|n(r,"Step")) else {return false;};
    let threshold:i64=rows.iter().filter(|r|n(r,"HeroIndex")==id).map(|r|n(r,"ReqFriendlyPoint")).sum();
    // Old code granted this final reward but advanced beyond the table without
    // consuming its points. Repair only that specific completed-claim shape.
    if n(last,"EndRewardRepeatCount")==0 && n(info,"Step")==n(last,"Step")+1
        && n(info,"EndRewardCount")==0 && n(info,"FriendlyPoint")>=threshold {
        info["Step"]=last["Step"].clone();
        info["FriendlyPoint"]=json!(n(info,"FriendlyPoint")-n(last,"ReqFriendlyPoint"));
        info["EndRewardCount"]=json!(1);
        return true;
    }
    false
}

pub(super) async fn snapshot(db: &mut SqliteConnection,state: &AppState,account:i64)->Result<Vec<Value>> {
    let mut infos=list(db,account,"npc").await?;
    for info in &mut infos {
        if repair_final_claim(state,info) {
            put(db,account,"npc",n(info,"HeroIndex"),info).await?;
        }
    }
    Ok(infos)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let id = req.number("HeroIndex", 0)?;
    let rows: Vec<&Value> = state
        .tables
        .extensions
        .rows("NPCFriendlyPoint")
        .iter()
        .filter(|r| n(r, "HeroIndex") == id)
        .collect();
    if rows.is_empty() {
        return Err(rule("NPCNotFound"));
    }
    let episode = crate::api::episodes::require_npc(state, id)?;
    if !episode && !((90..=99).contains(&id) || id == 111 || id == 805) {
        return Err(rule("NPCNotFound"));
    }
    let mut info = get(db, account, "npc", id).await?;
    if info.is_null() {
        info = json!({"HeroIndex":id,"Step":1,"FriendlyPoint":0,"EndRewardCount":0});
    }
    repair_final_claim(state,&mut info);
    let step = n(&info, "Step");
    let max = rows.iter().map(|r| n(r, "Step")).max().unwrap();
    let row = rows
        .iter()
        .find(|r| n(r, "Step") == step)
        .copied()
        .ok_or_else(|| rule("MaxFriendlyPoint"))?;
    // Reject an exhausted limited final reward before consuming gifts or granting loot.
    if step == max && n(row, "EndRewardRepeatCount") > 0 &&
        n(&info, "EndRewardCount") >= n(row, "EndRewardRepeatCount") {
        return Err(rule("MaxFriendlyPoint"));
    }
    let needed: i64 = rows
        .iter()
        .filter(|r| n(r, "Step") <= step)
        .map(|r| n(r, "ReqFriendlyPoint"))
        .sum();
    let mut out = item::success();
    if action == "give_gift_item" {
        let items = ids(req, "ItemIndices")?;
        let counts: Vec<i64> =
            serde_json::from_str(req.text("ItemCount")).map_err(|_| rule("InvalidItemCount"))?;
        if items.is_empty()
            || items.len() != counts.len()
            || counts.iter().any(|c| !(1..=100000).contains(c))
        {
            return Err(rule("InvalidItemCount"));
        }
        let mut points: i64 = 0;
        let mut consumed = vec![];
        for (item, count) in items.into_iter().zip(counts) {
            let gift = super::row(state, "NPCFriendlyPointItem", &[("ItemIndex", item)])?;
            if !gift["HeroIndices"]
                .as_array()
                .is_some_and(|a| a.contains(&json!(id)))
                || n(gift, "EventGiftPoint") != 0
            {
                return Err(rule("InvalidItem"));
            }
            if row["RestrictionItemCode"]
                .as_array()
                .is_some_and(|a| !a.is_empty() && !a.contains(&gift["ItemCode"]))
            {
                return Err(rule("InvalidItem"));
            }
            points = n(gift, "FriendlyPoint").checked_mul(count)
                .and_then(|value| points.checked_add(value))
                .filter(|value| *value > 0 && *value <= i32::MAX as i64)
                .ok_or_else(|| rule("InvalidItemCount"))?;
            consumed.push(item::consume(db, account, item as i32, count as i32).await?);
        }
        let new = n(&info, "FriendlyPoint")
            .checked_add(points)
            .filter(|p| *p <= i32::MAX as i64)
            .ok_or_else(|| rule("InvalidItemCount"))?;
        if row["LimitFriendlyPoint"] == true && new > needed {
            return Err(rule("MaxFriendlyPoint"));
        }
        info["FriendlyPoint"] = json!(new);
        out["ItemResults"] = json!(consumed);
        out["EventGiftPointResult"] = Value::Null;
        super::super::progression::record(db, account, "GiveFriendlyPoint", id, 0, points).await?;
        super::super::progression::record(db, account, "GiveMultipleGiftItem", id, 0, 1).await?;
    } else {
        if req.number("Step", 0)? != step || n(&info, "FriendlyPoint") < needed {
            return Err(rule("NotEnoughFriendlyPoint"));
        }
        let mut rewards = Rewards::default();
        item::reward(
            db,
            state,
            account,
            n(row, "RewardIndex") as i32,
            &mut rewards,
        )
        .await?;
        let r = item::reward_response(db, state, account, rewards).await?;
        for key in [
            "CurrencyResults",
            "ItemResults",
            "EquipItemResults",
            "HeroInfos",
            "ExpResultInfos",
        ] {
            out[key] = r[key].clone();
        }
        out["StaminaResultInfos"] = json!([]);
        out["HeroResults"] = json!([]);
        if step < max {
            info["Step"] = json!(step + 1);
        } else {
            // Zero means an unlimited repeat count. Keep the final valid step
            // and spend one reward's points, including for single-step NPCs.
            info["EndRewardCount"] = json!(n(&info, "EndRewardCount") + 1);
            info["FriendlyPoint"] = json!(n(&info, "FriendlyPoint") - n(row, "ReqFriendlyPoint"));
        }
    }
    put(db, account, "npc", id, &info).await?;
    out["FriendlyInfo"] = info;
    Ok(out)
}
