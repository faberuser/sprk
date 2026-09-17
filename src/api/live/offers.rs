use super::*;
fn products(s: &AppState) -> impl Iterator<Item = &Value> {
    s.tables.live.rules["Products"]
        .as_array()
        .into_iter()
        .flatten()
        .filter(|v| active(v) && n(v, "PriceType") > 0 && n(v, "Price") > 0)
}
fn period(v: &Value) -> i64 {
    if n(v, "ResetSeconds") > 0 {
        now() / n(v, "ResetSeconds")
    } else {
        0
    }
}
async fn ledger(db: &mut SqliteConnection, a: i64, d: &Value) -> Result<Value> {
    let id = n(d, "Index");
    let mut v = get(db, a, "product", id).await?;
    if v.is_null() || n(&v, "Period") != period(d) {
        v = json!({"ProductIndex":id,"PurchasedCount":0,"PurchasedTime":null,"Period":period(d),"PurchaseCountResetTime":time(if n(d,"ResetSeconds")>0{(period(d)+1)*n(d,"ResetSeconds")}else{4102444800})});
    }
    Ok(v)
}
pub(super) async fn purchases(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
) -> Result<Vec<Value>> {
    let mut out = vec![];
    for d in products(s) {
        let v = ledger(db, a, d).await?;
        if n(&v, "PurchasedCount") > 0 {
            out.push(v);
        }
    }
    Ok(out)
}
async fn selection(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    category: &str,
    refresh: bool,
) -> Result<(Value, Value)> {
    let options = s
        .tables
        .live
        .rows("GachaSelectShop")
        .iter()
        .filter(|v| {
            v["CategoryGroup"] == category
                && products(s).any(|p| n(p, "Index") == n(v, "ProductIndexGem"))
        })
        .collect::<Vec<_>>();
    if options.is_empty() {
        return Err(rule("InvalidValue"));
    }
    let key = n(options[0], "Index");
    let mut v = get(db, a, "select_shop", key).await?;
    let mut currency = Value::Null;
    if v.is_null() || timestamp(&v["EndTime"]) <= now() || refresh {
        let expired = v.is_null() || timestamp(&v["EndTime"]) <= now();
        let resets = if expired { 0 } else { n(&v, "ResetCount") };
        if refresh {
            if resets >= setting(s, "SelectionMaxResets", 10) {
                return Err(rule("InvalidValue"));
            }
            currency =
                hero::currency(db, a, "Gem", -setting(s, "SelectionRestockGem", 100)).await?;
        }
        let mut items = vec![];
        let mut infos = vec![];
        for option in options {
            let (i, c, _, _) = s
                .tables
                .roll_item_from_group_code(option["ItemGroupCode"].as_str().unwrap_or(""), &[])
                .ok_or_else(|| rule("ItemDataNotFound"))?;
            let mut d = products(s)
                .find(|p| n(p, "Index") == n(option, "ProductIndexGem"))
                .unwrap()
                .clone();
            d["ItemInfos"] = json!([{"ItemIndex":i,"ItemCount":c}]);
            items.push(json!({"ItemIndex":i,"ItemCount":c}));
            infos.push(d);
        }
        v = json!({"Index":key,"BeginTime":time(now()),"EndTime":time((day()+1)*86400),"RestockEndTime":time((day()+1)*86400),"CategoryGroup":category,"ResetCount":resets+i64::from(refresh),"ShowIndex":1,"ItemInfos":items,"EquipItemInfos":[],"PayShopProductInfos":infos});
        put(db, a, "select_shop", key, &v).await?;
    }
    Ok((v, currency))
}
async fn price(db: &mut SqliteConnection, a: i64, d: &Value) -> Result<(i64, Value)> {
    let local = ledger(db, a, d).await?;
    let total = get(db, 0, "product_global", n(d, "Index")).await?;
    let used = if n(&total, "Period") == period(d) {
        n(&total, "Count")
    } else {
        0
    };
    let discount = if n(d, "DiscountLimit") > used {
        n(d, "DiscountPercent").clamp(0, 99)
    } else {
        0
    };
    let p = n(d, "Price") * (100 - discount) / 100;
    if p <= 0 {
        return Err(rule("InvalidCost"));
    }
    Ok((
        p,
        json!({"ProductIndex":d["Index"],"PurchasedCount":local["PurchasedCount"],"Step":if discount>0{1}else{0},"DiscountPrice":p,"DefaultPrice":d["Price"],"ServerLimitedCount":(n(d,"DiscountLimit")-used).max(0)}),
    ))
}
async fn product(db: &mut SqliteConnection, s: &AppState, a: i64, id: i64) -> Result<Value> {
    let mut d = products(s)
        .find(|v| n(v, "Index") == id)
        .ok_or_else(|| rule("ProductNotFound"))?
        .clone();
    if let Some(category) = d["SelectionCategory"].as_str() {
        let (v, _) = selection(db, s, a, category, false).await?;
        d = v["PayShopProductInfos"]
            .as_array()
            .and_then(|v| v.iter().find(|v| n(v, "Index") == id))
            .cloned()
            .ok_or_else(|| rule("ProductNotFound"))?;
    }
    let (p, _) = price(db, a, &d).await?;
    d["Price"] = json!(p);
    Ok(d)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    match action {
        "get_payshop_products" => {
            let cat = int(r, "CategoryType")?;
            let name = s
                .tables
                .live
                .enums
                .get("PayShopCategory")
                .and_then(|e| e.iter().find(|(_, v)| **v == cat).map(|(k, _)| k.as_str()))
                .unwrap_or("");
            let mut out = vec![];
            for d in products(s).filter(|d| cat == 0 || d["Category"] == name) {
                out.push(product(db, s, a, n(d, "Index")).await?);
            }
            Ok(
                json!({"PayShopProductInfos":out,"PlayerProductPurchaseInfos":purchases(db,s,a).await?}),
            )
        }
        "get_active_product_info" | "get_right_away_product_info" => {
            Ok(json!({"PayShopProductInfo":product(db,s,a,int(r,"ProductIndex")?).await?}))
        }
        "get_select_shop_info" | "restock_select_shop_info" => {
            let (v, c) = selection(
                db,
                s,
                a,
                r.text("CategoryGroup"),
                action == "restock_select_shop_info",
            )
            .await?;
            Ok(json!({"SelectShopInfo":v,"CurrencyResult":c}))
        }
        "refresh_payshop_discount_info" => {
            let mut out = vec![];
            for d in products(s).filter(|v| n(v, "DiscountLimit") > 0) {
                out.push(price(db, a, d).await?.1);
            }
            Ok(json!({"PayShopDiscountInfos":out,"RefreshTimeSec":(day()+1)*86400-now()}))
        }
        "refresh_right_away_product_info" => {
            let mut notices = vec![];
            let mut seen = std::collections::BTreeSet::new();
            for d in products(s) {
                if let Some(c) = d["SelectionCategory"].as_str() {
                    if seen.insert(c) {
                        let (v, _) = selection(db, s, a, c, false).await?;
                        notices.push(json!({"OpenCategory":c,"CategoryGroups":[c],"CategoryNameKeys":[c],"EndTime":v["EndTime"]}));
                    }
                }
            }
            let infos=products(s).filter(|d|d["RightAway"]==true).map(|d|json!({"Index":d["Index"],"EndTime":d["EndTime"],"LimitedQuantity":d["PurchasableCount"],"BannerPath":d["BannerPath"],"ShowIndex":n(d,"ShowIndex")})).collect::<Vec<_>>();
            Ok(json!({"RightAwayProductInfos":infos,"SelectShopNoticeInfos":notices}))
        }
        "get_recommend_popup_info" => {
            let popup = &s.tables.live.rules["RecommendPopup"];
            let value = if products(s).any(|v| n(v, "Index") == n(popup, "ProductIndex")) {
                popup.clone()
            } else {
                Value::Null
            };
            Ok(json!({"RecommendPopupInfo":value}))
        }
        "set_purchase_marketing" => {
            let ids = ids(r, "PurchaseMarketingIndices", 100, true)?;
            let mut out = vec![];
            for id in ids {
                if !products(s).any(|v| n(v, "Index") == id) {
                    return Err(rule("ProductNotFound"));
                }
                let v = json!({"PurchaseIndex":id,"BeginTime":time(now()),"EndTime":time(4102444800),"PurchasedTime":null,"Count":0,"Active":u8::from(flag(r,"IsActive")?),"IsOpenPopup":u8::from(flag(r,"IsOpenPopup")?)});
                put(db, a, "product_notice", id, &v).await?;
                out.push(v);
            }
            Ok(json!({"PurchaseMarketingResult":out}))
        }
        "buy_payshop_product" => {
            let id = int(r, "Index")?;
            let base = products(s)
                .find(|v| n(v, "Index") == id)
                .ok_or_else(|| rule("ProductNotFound"))?;
            let d = product(db, s, a, id).await?;
            let mut v = ledger(db, a, base).await?;
            let count = n(&v, "PurchasedCount");
            if n(base, "PurchasableCount") > 0 && count >= n(base, "PurchasableCount") {
                return Err(rule("ExceededPurchaseCount"));
            }
            let dec = charge(db, s, a, n(base, "PriceType"), n(&d, "Price")).await?;
            let mut rewards = Rewards::default();
            for i in d["ItemInfos"].as_array().into_iter().flatten() {
                item::give(
                    db,
                    s,
                    a,
                    n(i, "ItemIndex") as i32,
                    n(i, "ItemCount") as i32,
                    0,
                    0,
                    &mut rewards,
                )
                .await?;
            }
            for key in [
                "Gold",
                "Gem",
                "Mileage",
                "LuaPoint",
                "ShopEventPoint",
                "LimitedShopEventPoint",
                "GrowWorldTreePoint",
                "CraftEventPoint",
            ] {
                let amount = n(&d, key);
                if amount > 0 {
                    rewards
                        .currencies
                        .push(hero::currency(db, a, key, amount).await?);
                }
            }
            v["PurchasedCount"] = json!(count + 1);
            v["PurchasedTime"] = json!(time(now()));
            put(db, a, "product", id, &v).await?;
            let total = get(db, 0, "product_global", id).await?;
            let used = if n(&total, "Period") == period(base) {
                n(&total, "Count")
            } else {
                0
            };
            put(
                db,
                0,
                "product_global",
                id,
                &json!({"Period":period(base),"Count":used+1}),
            )
            .await?;
            let mut out = item::reward_response(db, s, a, rewards).await?;
            out["DecCurrencyResult"] = dec;
            out["PlayerProductPurchaseInfo"] = v;
            out["firstPurchase"] = json!(count == 0);
            out["EquipItemInfos"] = out["EquipItemResults"].clone();
            Ok(out)
        }
        "buy_purchase_dungeon" => {
            let c = int(r, "ChapterIndex")?;
            let d = int(r, "DungeonIndex")?;
            let data = row(
                s,
                "PurchaseDungeon",
                &[("ChapterIndex", c), ("DungeonIndex", d)],
            )?;
            if data["Buyable"] != true || int(r, "Price")? != n(data, "Price") {
                return Err(rule("InvalidCost"));
            }
            let id = s
                .tables
                .get_item_index(data["BoosterItemCode"].as_str().unwrap_or(""))
                .ok_or_else(|| rule("ItemDataNotFound"))?;
            let currency = charge(db, s, a, n(data, "CurrencyType"), n(data, "Price")).await?;
            let booster = s
                .tables
                .inventory
                .boosters
                .get(&id)
                .ok_or_else(|| rule("BoosterDataNotFound"))?;
            let duration = n(booster, "Duration");
            if duration <= 0 || duration > 31536000 {
                return Err(rule("InvalidValue"));
            }
            let old: Option<String> = sqlx::query_scalar(
                "SELECT end_time FROM item_boosters WHERE account_id=? AND item_index=?",
            )
            .bind(a)
            .bind(id)
            .fetch_optional(&mut *db)
            .await?;
            let end = timestamp(&json!(old)).max(now()) + duration;
            let info = json!({"ItemIndex":id,"StartTime":time(now()),"EndTime":time(end)});
            sqlx::query("INSERT INTO item_boosters(account_id,item_index,start_time,end_time) VALUES(?,?,?,?) ON CONFLICT(account_id,item_index) DO UPDATE SET start_time=excluded.start_time,end_time=excluded.end_time").bind(a).bind(id).bind(time(now())).bind(time(end)).execute(&mut *db).await?;
            Ok(json!({"CurrencyResults":[currency],"ItemTimeDurations":[info]}))
        }
        _ => Err(rule("Fail")),
    }
}
