use crate::database::DbPool;
pub(crate) async fn migrate_technomagic(db: &DbPool, tables: &crate::tables::GameTables) -> anyhow::Result<()> {
    let ids: Vec<i64> = tables.extensions.rows("EquipItem").iter()
        .filter(|v| v["EquipType"] == 1).filter_map(|v|v["ItemIndex"].as_i64()).collect();
    let ids = serde_json::to_string(&ids)?;
    let mut tx = db.begin().await?;
    crate::db::query("UPDATE equip_items SET extra_option_index_2=0,extra_option_step_2=0,extra_option_renew_count_2=0,is_renewed_extra_option_2=0 WHERE item_index IN (SELECT sprk_json_ints($1)) AND (extra_option_index_2!=0 OR extra_option_step_2!=0 OR extra_option_renew_count_2!=0 OR is_renewed_extra_option_2!=0)")
        .bind(&ids).execute(&mut *tx).await?;
    // An old pending second-skill reroll must not restore the removed skill.
    crate::db::query("DELETE FROM equipment_pending WHERE kind='skill' AND sprk_json_int(data,'$.Slot')=2 AND slot_index IN (SELECT slot_index FROM equip_items WHERE item_index IN (SELECT sprk_json_ints($1)))")
        .bind(&ids).execute(&mut *tx).await?;
    tx.commit().await?;
    Ok(())
}
