use crate::{database::DbPool, models::equip::EquipItemInfo};
use sqlx::Row;
pub(crate) async fn migrate_technomagic(db: &DbPool, tables: &crate::tables::GameTables) -> anyhow::Result<()> {
    let ids: Vec<i64> = tables.extensions.rows("EquipItem").iter()
        .filter(|v| v["EquipType"] == 1).filter_map(|v|v["ItemIndex"].as_i64()).collect();
    let ids = serde_json::to_string(&ids)?;
    let mut tx = db.begin().await?;
    sqlx::query("UPDATE equip_items SET extra_option_index_2=0,extra_option_step_2=0,extra_option_renew_count_2=0,is_renewed_extra_option_2=0 WHERE item_index IN (SELECT value FROM json_each(?)) AND (extra_option_index_2!=0 OR extra_option_step_2!=0 OR extra_option_renew_count_2!=0 OR is_renewed_extra_option_2!=0)")
        .bind(&ids).execute(&mut *tx).await?;
    // An old pending second-skill reroll must not restore the removed skill.
    sqlx::query("DELETE FROM equipment_pending WHERE kind='skill' AND json_extract(data,'$.Slot')=2 AND slot_index IN (SELECT slot_index FROM equip_items WHERE item_index IN (SELECT value FROM json_each(?)))")
        .bind(&ids).execute(&mut *tx).await?;
    tx.commit().await?;
    Ok(())
}
pub(crate) async fn migrate(db: &DbPool) -> anyhow::Result<()> {
    let columns: std::collections::BTreeSet<String> = sqlx::query("PRAGMA table_info(equip_items)")
        .fetch_all(db)
        .await?
        .iter()
        .map(|r| r.get("name"))
        .collect();
    for (key, v) in serde_json::to_value(EquipItemInfo::default())?
        .as_object()
        .unwrap()
    {
        let col = EquipItemInfo::column(key);
        if v.is_number() && !columns.contains(&col) {
            sqlx::query(&format!(
                "ALTER TABLE equip_items ADD COLUMN {col} INTEGER NOT NULL DEFAULT 0"
            ))
            .execute(db)
            .await?;
        }
    }
    for query in [
        "CREATE TABLE IF NOT EXISTS equipment_pending(account_id INTEGER NOT NULL,slot_index INTEGER NOT NULL,kind TEXT NOT NULL,data TEXT NOT NULL,PRIMARY KEY(account_id,slot_index,kind),FOREIGN KEY(slot_index) REFERENCES equip_items(slot_index) ON DELETE CASCADE)",
        "CREATE TABLE IF NOT EXISTS extension_state(account_id INTEGER NOT NULL,kind TEXT NOT NULL,idx INTEGER NOT NULL,data TEXT NOT NULL,PRIMARY KEY(account_id,kind,idx))",
    ] { sqlx::query(query).execute(db).await?; }
    if !columns.contains("punishment_rune_option") {
        sqlx::query("ALTER TABLE equip_items ADD COLUMN punishment_rune_option TEXT")
            .execute(db)
            .await?;
    }
    Ok(())
}
