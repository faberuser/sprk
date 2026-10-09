use crate::database::DbPool;

pub(crate) async fn migrate(pool: &DbPool) -> anyhow::Result<()> {
    for sql in [
        "CREATE TABLE IF NOT EXISTS progression_claims(account_id INTEGER NOT NULL,family TEXT NOT NULL,idx INTEGER NOT NULL,step INTEGER NOT NULL DEFAULT 1,period TEXT NOT NULL DEFAULT 'all',claimed_time TEXT NOT NULL DEFAULT (datetime('now')),PRIMARY KEY(account_id,family,idx,step,period))",
        "CREATE TABLE IF NOT EXISTS progression_metrics(account_id INTEGER NOT NULL,day TEXT NOT NULL,kind TEXT NOT NULL,arg INTEGER NOT NULL DEFAULT 0,arg2 INTEGER NOT NULL DEFAULT 0,value INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(account_id,day,kind,arg,arg2))",
        "CREATE TABLE IF NOT EXISTS progression_login(account_id INTEGER PRIMARY KEY,days INTEGER NOT NULL DEFAULT 0,last_day TEXT NOT NULL DEFAULT '')",
        "CREATE TABLE IF NOT EXISTS attendance_calendar_state(account_id INTEGER NOT NULL,idx INTEGER NOT NULL,claims INTEGER NOT NULL DEFAULT 0,last_day TEXT NOT NULL DEFAULT '',completed_time TEXT,PRIMARY KEY(account_id,idx))",
        "CREATE TABLE IF NOT EXISTS progression_entitlements(account_id INTEGER NOT NULL,product_index INTEGER NOT NULL,created_time TEXT NOT NULL DEFAULT (datetime('now')),PRIMARY KEY(account_id,product_index))",
        "CREATE TABLE IF NOT EXISTS progression_world_events(account_id INTEGER NOT NULL,chapter INTEGER NOT NULL,dungeon INTEGER NOT NULL,event INTEGER NOT NULL,data TEXT NOT NULL,PRIMARY KEY(account_id,chapter,dungeon,event))",
        "CREATE TABLE IF NOT EXISTS progression_main_quest(account_id INTEGER PRIMARY KEY,step INTEGER NOT NULL DEFAULT 0,progress INTEGER NOT NULL DEFAULT 0)",
        "INSERT OR IGNORE INTO progression_login(account_id,days,last_day) SELECT account_id,MAX(0,total_days),last_attendance FROM attendance",
    ] {sqlx::query(sql).execute(pool).await?;}
    // Metrics share the transaction of their originating mutation, including rollback.
    for (name,table,op,when,kind,arg,arg2,amount) in [
        ("spend_gold","user_info","UPDATE","NEW.gold<OLD.gold","ConsumeGold","0","0","OLD.gold-NEW.gold"),
        ("gain_gold","user_info","UPDATE","NEW.gold>OLD.gold","GetGold","0","0","NEW.gold-OLD.gold"),
        ("spend_gem","user_info","UPDATE","NEW.gem+NEW.pay_gem<OLD.gem+OLD.pay_gem","ConsumeGem","0","0","OLD.gem+OLD.pay_gem-NEW.gem-NEW.pay_gem"),
        ("spend_stamina","user_info","UPDATE","NEW.stamina<OLD.stamina","ConsumeStamina","0","0","OLD.stamina-NEW.stamina"),
        ("stamina_uses","user_info","UPDATE","NEW.stamina<OLD.stamina","StaminaUses","0","0","1"),
        ("friend_send","friend_points","UPDATE","NEW.last_send_time IS NOT NULL AND (OLD.last_send_time IS NULL OR NEW.last_send_time!=OLD.last_send_time)","SendFriendshipPoint","0","0","1"),
        ("friend_request","friends","INSERT","NEW.status='pending'","FriendRequest","0","0","1"),
        ("clear_insert","campaign_progress","INSERT","NEW.clear_count>0","ClearDungeon","NEW.chapter_id","NEW.dungeon_id","NEW.clear_count"),
        ("clear_update","campaign_progress","UPDATE","NEW.clear_count>OLD.clear_count","ClearDungeon","NEW.chapter_id","NEW.dungeon_id","NEW.clear_count-OLD.clear_count"),
        ("roulette_first","hero_inn_roulette_spins","INSERT","NEW.spin_count>0","RotateRulletDaily","0","0","NEW.spin_count"),
        ("roulette_more","hero_inn_roulette_spins","UPDATE","NEW.spin_count>OLD.spin_count","RotateRulletDaily","0","0","NEW.spin_count-OLD.spin_count"),
        ("item_new","items","INSERT","NEW.count>0","AddItem","NEW.item_index","0","NEW.count"),
        ("item_gain","items","UPDATE","NEW.count>OLD.count","AddItem","NEW.item_index","0","NEW.count-OLD.count"),
        ("equip_gain","equip_items","INSERT","1","AddEquip","NEW.item_index","0","1"),
        ("shop_new","shop_purchase_ledger","INSERT","NEW.purchased>0","BuyShopItem","NEW.shop_index","NEW.item_index","NEW.purchased"),
        ("shop_more","shop_purchase_ledger","UPDATE","NEW.purchased>OLD.purchased","BuyShopItem","NEW.shop_index","NEW.item_index","NEW.purchased-OLD.purchased"),
        ("shop_restock","shop_stock","UPDATE","NEW.restock_count>OLD.restock_count","ResetShop","NEW.shop_index","0","1"),
        ("hero_friendly","hero_friendly_state","UPDATE","NEW.friendly_point>OLD.friendly_point","DoHeroFriendly","NEW.hero_index","0","1"),
        ("mail_received","mails","UPDATE","NEW.is_received=1 AND OLD.is_received=0","ReceiveMail","0","0","1"),
    ] {
        let sql=format!("CREATE TRIGGER IF NOT EXISTS progression_{name} AFTER {op} ON {table} WHEN {when} BEGIN INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES(NEW.account_id,date('now'),'{kind}',{arg},{arg2},{amount}) ON CONFLICT(account_id,day,kind,arg,arg2) DO UPDATE SET value=value+excluded.value; END");
        sqlx::query(&sql).execute(pool).await?;
    }
    Ok(())
}
