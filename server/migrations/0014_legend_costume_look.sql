-- Legend costumes carry their own weapon and hair (see
-- `heroes::apply_legend_look`). Earlier servers kept the hero's previous
-- HideUniqueWeapon, weapon costume and hair costume when a Legend costume was
-- equipped, so the Unique Weapon (or a costume weapon/hair) was drawn over the
-- Legend model. Repair stored heroes, costume presets and lobby avatars.
--
-- The list is every CostumeTable row with CostumeType = 2 (Legend); the test
-- `legend_repair_migration_lists_every_legend_costume` keeps it in sync.
CREATE TEMPORARY TABLE legend_costumes (costume_index text PRIMARY KEY) ON COMMIT DROP;
INSERT INTO legend_costumes VALUES
    ('1112'), ('1113'), ('1212'), ('2519'), ('2520'), ('4515'), ('10010');

UPDATE hero_details
SET data = (data::jsonb || '{"HideUniqueWeapon":1,"WeaponCostumeIndex":0,"HairCostumeIndex":0}'::jsonb)::text
WHERE data::jsonb->>'CostumeIndex' IN (SELECT costume_index FROM legend_costumes)
  AND (coalesce(data::jsonb->>'HideUniqueWeapon', '0') <> '1'
       OR coalesce(data::jsonb->>'WeaponCostumeIndex', '0') <> '0'
       OR coalesce(data::jsonb->>'HairCostumeIndex', '0') <> '0');

UPDATE costume_presets
SET data = (data::jsonb || '{"HideUniqueWeapon":1,"WeaponCostumeIndex":0,"HairCostumeIndex":0}'::jsonb)::text
WHERE data::jsonb->>'CostumeIndex' IN (SELECT costume_index FROM legend_costumes)
  AND (coalesce(data::jsonb->>'HideUniqueWeapon', '0') <> '1'
       OR coalesce(data::jsonb->>'WeaponCostumeIndex', '0') <> '0'
       OR coalesce(data::jsonb->>'HairCostumeIndex', '0') <> '0');

UPDATE lobby_appearance
SET hide_unique_weapon = 1,
    weapon_costume_index = 0,
    hair_costume_index = 0,
    snapshot = CASE WHEN snapshot IS NULL THEN NULL
        ELSE (snapshot::jsonb || '{"HideUniqueWeapon":1,"WeaponCostumeIndex":0,"HairCostumeIndex":0}'::jsonb)::text END
WHERE costume_index::text IN (SELECT costume_index FROM legend_costumes)
  AND (hide_unique_weapon <> 1 OR weapon_costume_index <> 0 OR hair_costume_index <> 0);
