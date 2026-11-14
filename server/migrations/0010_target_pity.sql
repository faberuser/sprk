-- extensions::equipment / extensions::valance: SPRK "reforge until" and
-- "enchant until" guarantees. Misses toward the guarantee add up across runs
-- for one item and one target; a hit or a different target starts over.
-- `kind` is `reforge<line>` for a gear option line or `enchant` for a
-- Technomagic enchant; `target` is the target stat type(s) the misses count for.
CREATE TABLE sprk_target_pity (
    account_id bigint NOT NULL,
    slot_index bigint NOT NULL REFERENCES equip_items(slot_index) ON DELETE CASCADE,
    kind text NOT NULL,
    target text NOT NULL,
    misses bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (account_id, slot_index, kind)
);
