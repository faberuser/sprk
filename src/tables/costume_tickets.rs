//! Local all-roster costume tickets; archived restricted tickets remain unchanged.
use std::path::Path;
use serde_json::Value;
use super::{HeroShopTable, InventoryTable};

pub(super) fn rules(path: &Path) -> Result<Value, Box<dyn std::error::Error>> {
    Ok(serde_json::from_reader(std::fs::File::open(path.with_file_name("CostumeTicketRules.json"))?)?)
}
pub(super) fn hero_shop(table: &mut HeroShopTable, path: &Path) -> Result<(), Box<dyn std::error::Error>> {
    let r=rules(path)?;
    let id=r["LightTicketItemIndex"].as_i64().ok_or("Missing light ticket index")? as i32;
    table.items.insert(id,r["NewItem"].clone());
    for row in r["NewCostumes"].as_array().ok_or("Missing light costumes")? {
        let id=row["CostumeIndex"].as_i64().ok_or("Missing costume index")? as i32;
        if table.costumes.contains_key(&id) { return Err(format!("New light costume index already used: {id}").into()); }
        table.costumes.insert(id,row.clone());
    }
    for update in r["CostumeUpdates"].as_array().ok_or("Missing costume updates")? {
        let id=update["CostumeIndex"].as_i64().ok_or("Missing updated costume index")? as i32;
        let row=table.costumes.get_mut(&id).ok_or("Updated costume missing")?;
        for (key,value) in update.as_object().unwrap() { row[key]=value.clone(); }
    }
    let dark=r["DarkTicketItemIndex"].as_i64().unwrap();
    table.costume_selectors.iter_mut().find(|s|s["ItemIndex"]==dark).ok_or("Missing darkness ticket")?["CostumeIndices"]=r["DarkCostumeIndices"].clone();
    let mut selector=r["NewSelector"].clone();selector["ItemIndex"]=id.into();table.costume_selectors.push(selector);
    let expected: std::collections::BTreeSet<i64>=r["HeroIndices"].as_array().unwrap().iter().map(|v|v.as_i64().unwrap()).collect();
    if expected.len()!=102 || expected.contains(&14) { return Err("Invalid costume ticket roster".into()); }
    for key in ["DarkCostumeIndices","LightCostumeIndices"] {
        let indices=r[key].as_array().ok_or("Missing costume pool")?;
        let mut heroes=std::collections::BTreeSet::new();
        for index in indices {
            let costume=table.costumes.get(&(index.as_i64().ok_or("Invalid costume index")? as i32)).ok_or("Ticket costume missing")?;
            heroes.insert(costume["HeroIndex"].as_i64().ok_or("Costume hero missing")?);
        }
        if indices.len()!=102 || heroes!=expected { return Err(format!("Invalid 102-hero pool: {key}").into()); }
    }
    Ok(())
}
pub(super) fn inventory(table: &mut InventoryTable, path: &Path) -> Result<(), Box<dyn std::error::Error>> {
    let r=rules(path)?;table.items.insert(r["LightTicketItemIndex"].as_i64().unwrap() as i32,r["NewItem"].clone());Ok(())
}
