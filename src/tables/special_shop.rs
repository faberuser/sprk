//! Expand the editable reconstructed catalog into the existing purchase rules.
use super::BattleTable;
use serde_json::{json, Value};
use std::{collections::{HashMap, HashSet}, path::Path};

pub(super) fn append(live: &mut BattleTable, directory: &Path) -> Result<(), Box<dyn std::error::Error>> {
    let path = directory.join("SpecialShopRules.json");
    if !path.exists() {
        return Ok(()); // Older table installations still support their local offers.
    }
    let catalog: Value = serde_json::from_reader(std::fs::File::open(path)?)?;
    if catalog["Enabled"] == false {
        return Ok(());
    }
    let codes: HashMap<String, i32> = serde_json::from_reader(std::fs::File::open(directory.join("ItemCodeToIndex.json"))?)?;
    let mut ids: HashSet<i64> = live.rules["Products"].as_array().into_iter().flatten()
        .filter_map(|p| p["Index"].as_i64()).collect();
    let mut products = Vec::new();
    let rows = catalog["Products"].as_array().ok_or("SpecialShopRules.Products must be an array")?;
    for row in rows {
        let id = row["Index"].as_i64().filter(|v| *v > 0 && *v <= i32::MAX as i64).ok_or("Invalid Special Shop product index")?;
        if !ids.insert(id) { return Err(format!("Duplicate Special Shop product index {id}").into()); }
        let category = row["Category"].as_str().ok_or("Missing Special Shop category")?;
        let defaults = &catalog["CategoryDefaults"][category];
        let setting = |key: &str| if row[key].is_null() { &defaults[key] } else { &row[key] };
        let currency = setting("PriceType").as_str().ok_or("Missing Special Shop currency")?;
        if !matches!(currency, "Gold" | "Gem" | "LuaPoint" | "Mileage") {
            return Err(format!("Unsupported Special Shop currency {currency}").into());
        }
        let price_type = live.enums.get("CurrencyType").and_then(|e| e.get(currency)).ok_or("Unknown Special Shop currency")?;
        let price = setting("Price").as_i64().filter(|v| *v >= 0 && *v <= i32::MAX as i64).ok_or("Invalid Special Shop price")?;
        let limit = setting("PurchasableCount").as_i64().filter(|v| *v >= 0 && *v <= i32::MAX as i64).ok_or("Invalid Special Shop stock limit")?;
        let reset = setting("ResetSeconds").as_i64().filter(|v| *v == 0 || *v == 86400 || *v == 604800).ok_or("Special Shop reset must be 0, 86400 or 604800 seconds")?;
        let name = row["ProductName"].as_str().filter(|v| !v.is_empty()).ok_or("Missing Special Shop product name")?;
        let mut product = json!({"Index":id,"Category":category,"ProductName":name,"ProductDesc":name,"ProductDetailDesc":name,
            "PriceType":price_type,"Price":price,"PurchasableCount":limit,"ResetSeconds":reset,"ResetType":if reset==86400{1}else if reset==604800{2}else{0},
            "DailyReset":reset==86400,"WeeklyReset":if reset==604800{vec![1]}else{vec![]},"MonthlyResetDate":[],
            "Begin":row["Begin"].as_i64().unwrap_or(0),"End":row["End"].as_i64().unwrap_or(0),"Enabled":row["Enabled"]!=false,
            "BeginTime":"","EndTime":"","Durational":0,
            "CardType":2,"ShowIndex":id,"Flag":if price==0{2}else{0},"UseItemCode":[],"Count":[],"ItemInfos":[],
            "StepProductGroup":[],"ExceptServer":[],"SelectAttendanceIndices":[],"BonusProductInfos":[],"ProbabilityIndices":[]});
        if let Some(code) = row["ItemCode"].as_str() {
            let index = codes.get(code).filter(|v| **v > 0).ok_or_else(|| format!("Unknown Special Shop item {code}"))?;
            if row["ItemIndex"].as_i64() != Some(*index as i64) { return Err(format!("Special Shop item/code mismatch for {code}").into()); }
            let count = row["ItemCount"].as_i64().filter(|v| *v > 0 && *v <= i32::MAX as i64).ok_or("Invalid Special Shop item count")?;
            // Use Item cards for both packages and consumables. The native item
            // popup then shows the actual item, with its normal contents tooltip.
            product["ProductType"] = json!(4);
            product["UseItemCode"] = json!([code]);
            product["Count"] = json!([count]);
            product["ItemInfos"] = json!([{"ItemIndex":index,"ItemCount":count}]);
            product["IconPath"] = json!(format!("Icon/Item/{}", code.replace('.', "/")));
        } else {
            let gem = row["Gem"].as_i64().unwrap_or(0);
            let gold = row["Gold"].as_i64().unwrap_or(0);
            if gem < 0 || gold < 0 || gem > i32::MAX as i64 || gold > i32::MAX as i64 || (gem == 0) == (gold == 0) {
                return Err("Special Shop currency offer must grant one positive currency amount".into());
            }
            product["ProductType"] = json!(if gem>0 {2} else {3});
            product["Gem"] = json!(gem);
            product["Gold"] = json!(gold);
            product["IconPath"] = row["IconPath"].clone();
        }
        if let Some(icon)=row["IconPath"].as_str() { product["IconPath"]=json!(icon); }
        products.push(product);
    }
    live.rules["Products"].as_array_mut().ok_or("LiveRules.Products must be an array")?.extend(products);
    Ok(())
}
