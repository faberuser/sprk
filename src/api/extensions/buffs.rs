use super::*;
use std::collections::BTreeMap;

pub(crate) async fn class_snapshot(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
) -> Result<Value> {
    let Some(rules) = state.tables.extensions.rows("LocalClassBuffPoints").first() else {
        return Ok(json!({"ClassBuffPointInfos":[],"ClassBuffInfos":[]}));
    };
    let mut earned = BTreeMap::<i64, i64>::new();
    for tag in state
        .tables
        .extensions
        .rows("ClassBuff")
        .iter()
        .map(|r| n(r, "TagType"))
    {
        earned.insert(tag, 0);
    }
    let heroes =
        sqlx::query("SELECT hero_index,level,star,transcend FROM heroes WHERE account_id=?")
            .bind(account)
            .fetch_all(&mut *db)
            .await?;
    for h in heroes {
        let id: i32 = h.get("hero_index");
        if let Some(data) = state.tables.hero_shop.heroes.get(&id) {
            let points = h.get::<i64, _>("level").saturating_sub(1) * n(rules, "HeroLevel")
                + h.get::<i64, _>("star").saturating_sub(1) * n(rules, "HeroAwaken")
                + h.get::<i64, _>("transcend") * n(rules, "HeroTranscend");
            *earned.entry(n(data, "TagType")).or_default() += points;
        }
    }
    // Keep the best awakening of each unique item, so duplicate copies cannot farm points.
    let equips=sqlx::query("SELECT item_index,MAX(star) AS star FROM equip_items WHERE account_id=? GROUP BY item_index").bind(account).fetch_all(&mut *db).await?;
    for eq in equips {
        let id: i32 = eq.get("item_index");
        let Some(data) = state
            .tables
            .extensions
            .find("EquipItem", &[("ItemIndex", id as i64)])
        else {
            continue;
        };
        let detail = row(
            state,
            "EquipItemDetail",
            &[("DetailIndex", n(data, "DetailIndex"))],
        )?;
        let key = match n(detail, "EquipItemDetailType") {
            11 => "UniqueWeaponAwaken",
            14 => "UniqueTreasureAwaken",
            13 => "ClassUniqueWeaponAwaken",
            _ => continue,
        };
        let mut tags = BTreeSet::new();
        for id in data["CreatureIndex"]
            .as_array()
            .into_iter()
            .flatten()
            .filter_map(Value::as_i64)
        {
            if let Some(h) = state.tables.hero_shop.heroes.get(&(id as i32)) {
                tags.insert(n(h, "TagType"));
            }
        }
        if tags.is_empty() {
            tags.extend(
                data["TagType"]
                    .as_array()
                    .into_iter()
                    .flatten()
                    .filter_map(Value::as_i64),
            );
        }
        for tag in tags {
            *earned.entry(tag).or_default() += eq.get::<i64, _>("star") * n(rules, key);
        }
    }
    let buffs = list(db, account, "class_buff").await?;
    let mut points = vec![];
    for (tag, total) in earned {
        if tag == 0 {
            continue;
        }
        let old = get(db, account, "class_earned", tag).await?;
        let total = total.max(n(&old, "Earned"));
        put(db, account, "class_earned", tag, &json!({"Earned":total})).await?;
        let spent: i64 = buffs
            .iter()
            .filter(|b| n(b, "TagType") == tag)
            .map(|b| {
                state
                    .tables
                    .extensions
                    .rows("ClassBuff")
                    .iter()
                    .filter(|r| {
                        n(r, "TagType") == tag
                            && n(r, "ClassBuffIndex") == n(b, "ClassBuffIndex")
                            && n(r, "ClassBuffLevel") <= n(b, "ClassBuffLevel")
                    })
                    .map(|r| n(r, "NeedPoint"))
                    .sum::<i64>()
            })
            .sum();
        points.push(json!({"TagType":tag,"BuffPoint":(total-spent).max(0),"Earned":total}));
    }
    Ok(json!({"ClassBuffPointInfos":points,"ClassBuffInfos":buffs}))
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if action == "reinforce_team_level_buff" {
        return team(db, state, account, req).await;
    }
    let snapshot = class_snapshot(db, state, account).await?;
    let mut out = item::success();
    if action != "get_class_buff_info" {
        let tag = req.number("TagType", 0)?;
        let points = snapshot["ClassBuffPointInfos"]
            .as_array()
            .unwrap()
            .iter()
            .find(|p| n(p, "TagType") == tag)
            .ok_or_else(|| rule("InvalidTagType"))?;
        let total: i64 = snapshot["ClassBuffPointInfos"]
            .as_array()
            .unwrap()
            .iter()
            .map(|p| n(p, "Earned"))
            .sum();
        if total
            < state
                .tables
                .hero_shop
                .constant("ClassBuffContentsOpenPoint", 40)
        {
            return Err(rule("NotEnoughPoint"));
        }
        if action == "init_class_buff" {
            let reset: Value = serde_json::from_str(
                state
                    .tables
                    .hero_shop
                    .constants
                    .get("ResetClassBuff")
                    .ok_or_else(|| rule("ItemDataNotFound"))?,
            )
            .map_err(|_| rule("ItemDataNotFound"))?;
            out["CurrencyResult"] = hero::currency(
                db,
                account,
                reset[0].as_str().unwrap_or(""),
                -reset[1].as_i64().unwrap_or(0),
            )
            .await?;
            sqlx::query("DELETE FROM extension_state WHERE account_id=? AND kind='class_buff' AND json_extract(data,'$.TagType')=?").bind(account).bind(tag).execute(&mut *db).await?;
        } else {
            let index = req.number("ClassBuffIndex", 0)?;
            let level = req.number("ClassBuffLevel", 0)?;
            let target = row(
                state,
                "ClassBuff",
                &[
                    ("TagType", tag),
                    ("ClassBuffIndex", index),
                    ("ClassBuffLevel", level),
                ],
            )?;
            let current = get(db, account, "class_buff", tag * 1000 + index).await?;
            if level <= n(&current, "ClassBuffLevel") {
                return Err(rule("InvalidLevel"));
            }
            let cost: i64 = state
                .tables
                .extensions
                .rows("ClassBuff")
                .iter()
                .filter(|r| {
                    n(r, "TagType") == tag
                        && n(r, "ClassBuffIndex") == index
                        && n(r, "ClassBuffLevel") > n(&current, "ClassBuffLevel")
                        && n(r, "ClassBuffLevel") <= level
                })
                .map(|r| n(r, "NeedPoint"))
                .sum();
            if cost > n(points, "BuffPoint") {
                return Err(rule("NotEnoughPoint"));
            }
            let required = get(
                db,
                account,
                "class_buff",
                tag * 1000 + n(target, "NeedClassBuffIndex"),
            )
            .await?;
            if n(&required, "ClassBuffLevel") < n(target, "NeedClassBuffLevel") {
                return Err(rule("NotEnoughClassBuffLevel"));
            }
            let heroes: Vec<i32> =
                sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=?")
                    .bind(account)
                    .fetch_all(&mut *db)
                    .await?;
            let count = heroes
                .iter()
                .filter(|id| {
                    state
                        .tables
                        .hero_shop
                        .heroes
                        .get(id)
                        .is_some_and(|h| n(h, "TagType") == tag)
                })
                .count() as i64;
            if count < n(target, "NeedHeroCount") {
                return Err(rule("NotEnoughHero"));
            }
            put(db,account,"class_buff",tag*1000+index,&json!({"TagType":tag,"ClassBuffIndex":index,"ClassBuffLevel":level,"UpdatedTime":state.server_time_str()})).await?;
        }
    }
    let snapshot = class_snapshot(db, state, account).await?;
    out["ClassBuffInfos"] = snapshot["ClassBuffInfos"].clone();
    out["ClassBuffPointInfos"] = snapshot["ClassBuffPointInfos"].clone();
    Ok(out)
}
async fn team(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let level = req.number("BuffTeamLevel", 0)?;
    let owned: i64 = sqlx::query_scalar("SELECT team_level FROM user_info WHERE account_id=?")
        .bind(account)
        .fetch_one(&mut *db)
        .await?;
    if level > owned {
        return Err(rule("NotEnoughTeamLevel"));
    }
    let data = state
        .tables
        .inventory
        .team_levels
        .iter()
        .find(|r| n(r, "Level") == level)
        .ok_or_else(|| rule("InvalidTeamLevel"))?;
    let option = n(data, "TeamLevelBuffOptionIndex");
    let bonus = n(data, "TeamLevelBuffBonusOptionIndex");
    if req.number("OptionIndex", 0)? != option || req.number("BonusOptionIndex", 0)? != bonus {
        return Err(rule("InvalidOption"));
    }
    let row = if option > 0 {
        row(state, "TeamLevelBuffOption", &[("Index", option)])?
    } else {
        row(state, "TeamLevelBonusBuffOption", &[("Index", bonus)])?
    };
    let old = get(db, account, "team_buff", level).await?;
    let current = n(&old, "BuffReinforce");
    let target = req.number("Reinforce", 0)?;
    if target != current + 1 {
        return Err(rule("InvalidReinforce"));
    }
    let cost = row["ReinforcePrice"]
        .get(current as usize)
        .and_then(Value::as_i64)
        .ok_or_else(|| rule("MaxReinforce"))?;
    let kind = currency_type(n(row, "ReinforcePriceType"))?;
    if req.number("ReinforcePrice", -1)? != cost || req.text("ReinforcePriceType") != kind {
        return Err(rule("InvalidPrice"));
    }
    let currency = hero::currency(db, account, kind, -cost).await?;
    let info =
        json!({"OpenBuffLevel":level,"BuffReinforce":target,"UpdatedTime":state.server_time_str()});
    put(db, account, "team_buff", level, &info).await?;
    Ok(
        json!({"BaseResult":"Success","Result":"Success","CurrencyResults":[currency],"TeamLevelBuffInfo":info}),
    )
}
