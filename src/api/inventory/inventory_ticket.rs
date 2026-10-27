//! Compensate the unused capacity of one Large Inventory Expansion Ticket.
use super::item::{self,n,rule};
use crate::{api::system::request::Request,error::Result,state::AppState};
use serde_json::{json,Value};
use sqlx::{Row,SqliteConnection};

pub(crate) async fn redeem(db:&mut SqliteConnection,state:&AppState,account:i64,req:&Request,count:i32)->Result<Option<Value>> {
    let token=req.text("InventoryTicketToken");
    let fields=json!({"ItemIndex":2401,"ItemCount":count,
        "ExpectedInventoryExtend":req.number("ExpectedInventoryExtend",-1)?,
        "ExpectedCompensationGem":req.number("ExpectedCompensationGem",-1)?}).to_string();
    if !token.is_empty() {
        if uuid::Uuid::parse_str(token).is_err() {return Err(rule("InvalidActionValue"));}
        if let Some(saved)=sqlx::query("SELECT request,response FROM inventory_ticket_redemptions WHERE account_id=? AND token=?")
            .bind(account).bind(token).fetch_optional(&mut *db).await? {
            if saved.get::<String,_>("request")!=fields {return Err(rule("InvalidActionValue"));}
            let mut out:Value=serde_json::from_str(&saved.get::<String,_>("response")).map_err(|_|rule("InvalidActionValue"))?;
            // A retry acknowledges the original grant but refreshes current
            // balances, including changes from another logged-in client.
            out["CurrencyResult"]=item::money(db,account,"Gem",0).await?;
            let item=sqlx::query("SELECT count,locked FROM items WHERE account_id=? AND item_index=2401").bind(account).fetch_optional(&mut *db).await?;
            out["ItemResult"]["NewCount"]=json!(item.as_ref().map(|r|r.get::<i32,_>("count")).unwrap_or(0));
            out["ItemResult"]["AddCount"]=json!(0);
            out["NewInventoryExtend"]=json!(sqlx::query_scalar::<_,i64>("SELECT inventory_extend FROM inventory_settings WHERE account_id=?").bind(account).fetch_one(&mut *db).await?);
            out["_TicketReplay"]=json!(true);
            return Ok(Some(out));
        }
    }
    let old:i64=sqlx::query_scalar("SELECT inventory_extend FROM inventory_settings WHERE account_id=?").bind(account).fetch_one(&mut *db).await?;
    let row=state.tables.inventory.extensions.iter().filter(|r|n(r,"InventoryType")==0)
        .max_by_key(|r|n(r,"EquipItemExtendCount")).ok_or_else(||rule("InvalidActionValue"))?;
    let max=n(row,"EquipItemExtendCount");
    let potion=state.tables.inventory.potions.get(&2401).ok_or_else(||rule("PotionDataNotFound"))?;
    let unit=n(potion,"ActionValue");
    let price=n(row,"EquipItemExtendPrice");
    if old<0 || old>max || unit<=0 || price<=0 || n(potion,"ActionType")!=6 {return Err(rule("InvalidActionValue"));}
    if unit.checked_mul(count as i64).is_some_and(|v|v<=max-old) && token.is_empty() {return Ok(None);}
    // Confirm one ticket at a time. Ordinary fitting expansions retain their
    // native behavior; compensation always requires an exact confirmed quote.
    if count!=1 || token.is_empty() {return Err(rule("InvalidActionValue"));}
    let added=unit.min(max-old);
    let unused=unit-added;
    let gems=unused.checked_mul(price).filter(|g|*g>0).ok_or_else(||rule("InvalidActionValue"))?;
    if req.number("ExpectedInventoryExtend",-1)?!=old || req.number("ExpectedCompensationGem",-1)?!=gems {return Err(rule("InvalidActionValue"));}
    let mut out=item::success();
    out["ItemResult"]=item::consume(db,account,2401,1).await?;
    let wallet:(i64,i64)=sqlx::query_as("SELECT gem,pay_gem FROM user_info WHERE account_id=?").bind(account).fetch_one(&mut *db).await?;
    if wallet.0.checked_add(wallet.1).and_then(|v|v.checked_add(gems)).is_none() {return Err(rule("NotEnoughGem"));}
    out["CurrencyResult"]=item::money(db,account,"Gem",gems).await?;
    out["NewInventoryExtend"]=json!(old+added);
    sqlx::query("UPDATE inventory_settings SET inventory_extend=? WHERE account_id=?").bind(old+added).bind(account).execute(&mut *db).await?;
    out["InternalErrorMessage"]=json!(format!("Inventory expanded by {} slots. {} unused ticket slots converted into {gems} Rubies.",added*n(row,"EquipItemExtendSize"),unused*n(row,"EquipItemExtendSize")));
    sqlx::query("INSERT INTO inventory_ticket_redemptions VALUES(?,?,?,?)").bind(account).bind(token).bind(fields).bind(out.to_string()).execute(&mut *db).await?;
    Ok(Some(out))
}
