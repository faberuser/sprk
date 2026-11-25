-- progression::state reads only an equipment piece's slot, item, star and
-- inventory type (and its owner). Gear level, EXP, lock and option changes
-- (every enhancement, and the hero-level sync after most battles) no longer
-- invalidate the progression snapshot.
--
-- An inserted piece records the part `equip_items:+` instead of
-- `equip_items`: slots come from a non-cycling sequence and every writer of an
-- account's rows holds its account lock, so a piece inserted after a snapshot
-- has a higher slot than any the snapshot holds, and the snapshot loads only
-- those rows instead of every piece (1,600+ for active players). Updates and
-- deletes still record `equip_items`. An image built before this migration
-- does not know `equip_items:+` and rebuilds the snapshot fully, as before.

CREATE FUNCTION sprk_revision_insert() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
    PERFORM sprk_revision_bump_part(TG_ARGV[0], (to_jsonb(NEW) ->> TG_ARGV[1])::bigint, TG_TABLE_NAME || ':+');
    RETURN NULL;
END $$;

DROP TRIGGER rev_progression_equip_items_account_id_w ON equip_items;
DROP TRIGGER rev_progression_equip_items_account_id_u ON equip_items;

CREATE TRIGGER rev_progression_equip_items_account_id_i AFTER INSERT ON equip_items
    FOR EACH ROW EXECUTE FUNCTION sprk_revision_insert('progression', 'account_id');
CREATE TRIGGER rev_progression_equip_items_account_id_w AFTER DELETE ON equip_items
    FOR EACH ROW EXECUTE FUNCTION sprk_revision_owner('progression', 'account_id');
CREATE TRIGGER rev_progression_equip_items_account_id_u AFTER UPDATE ON equip_items
    FOR EACH ROW WHEN ((OLD.account_id, OLD.slot_index, OLD.item_index, OLD.star, OLD.inventory_type)
        IS DISTINCT FROM (NEW.account_id, NEW.slot_index, NEW.item_index, NEW.star, NEW.inventory_type))
    EXECUTE FUNCTION sprk_revision_owner('progression', 'account_id');
