# Hero and equipment extensions

The extension handlers use the extracted client request/response names. Costs, ownership checks, rewards, and saved choices are committed together in SQLite. Failed requests roll back their changes. Confirmations use the server's stored roll, and reconnect responses restore pending choices.

## Data

Regenerate the extracted rules from the original files:

```powershell
python scripts/export_extension_data.py ..\kingsraid-table-data\output\TableJit ..\kingsraid-table-data\data\TableJit
```

This writes `tables/ExtensionSupport.json`. Each source table uses its own string pool. `RecipeItemTable` is missing in the supplied extraction.

`tables/ExtensionRules.json` overrides named tables when the server loads them. Restart after editing it. These are local emulator rules, not recovered production-server values:

- `LocalClassBuffPoints` gives points for hero levels above 1, stars above 1, transcendence levels, and each unique equipment item's highest owned awakening. The current defaults are 1/5/10 points for hero progression and 10/5/5 for unique weapon/treasure/class weapon awakening. Each class keeps its highest observed total; spending or resetting buffs does not generate new earnings. Already-sold equipment from before this feature cannot be reconstructed.
- `LocalSoulStoneRestore` uses an equal-weight pool of the soul stones in grade-zero liberation recipes. NPC heroes are excluded by default. Set `UseLiberationStonePool` to false and supply explicit `ItemIndices` to use a curated pool. Three distinct choices are saved until confirmation. Costs and mileage rewards still come from extracted data.
- `SoulWeaponLimitBreak` is empty because the `ContentsDefine.SoulWeaponLimitBreak` payload is absent. Populated rows use the native fields `DetailIndex`, `Star` (resulting star), `SuccessRatio`, `FailBonusRatio`, `GoldCount`, `WeaponUniqueCount`, `SoulStoneCount`, `TransStoneIndex`, and `TransStoneCount`. Gold cost follows the client: `GoldCount * WeaponUniqueCount`. The client must also have matching definitions to expose the feature.
- An optional `RecipeItem` array can provide missing recipes with `ItemIndex`, `ReqGold`, `Materials` (`ItemIndex`/`Count` pairs), and `RewardIndex`. No recipes are fabricated by default.

Keep counts/prices nonnegative and use valid item/reward IDs. Replacement tables override the entire named array, not individual rows.

## Persistence and client initialization

- Extended numeric equipment fields and punishment-rune options are stored on `equip_items`.
- `equipment_pending` holds paid option/skill/enchantment rolls awaiting confirmation.
- `extension_state` holds soul weapons, NPC friendship, cosmetics, loadouts, class/team buffs, pet-selector ownership, and local restoration progress.
- Accessories are owned separately for each hero. Selector compensation requires ownership for every eligible hero, matching the client's collection check.
- Hero rune pages, flask state, and appearance fields use the existing `hero_details` storage.
- Login restores rune pages, cosmetics, souls, NPC friendship, and pending choices. First-lobby responses restore equipment presets, identified Valance information, pets, and team buffs.
- Equipment attached to rune pages, saved in loadouts, holding a soul weapon, or awaiting confirmation is protected from incompatible destructive operations.

## Limits of the extraction

The server does not enable closed accessory listings. All accessory listings in the supplied extraction are closed; selectors can still grant them. Hair and weapon customization predominantly unlock through owned body costumes.

Enhanced perk levels, dyes, legendary-costume progression, and Valance tier upgrades lack usable end-to-end contracts or rules in this extraction. Event flasks and consumables tied to unfinished modes are rejected. Random rolls use extracted option weights where available; soul renewal uses a local uniform distribution within the client's displayed bounds.

EXP flasks fill from EXP awarded to capped heroes by the existing campaign implementation. Campaign participation and battle-result verification remain separate backlog items. Full gameplay behavior still needs an actual client playthrough.

## Verification

`src/api/extensions/tests.rs` exercises rollback, duplicate requests, material validation, persistent choices, reconnects, rune preservation, loadouts, NPC claims, buffs, customization, tickets, and Valance flows against isolated SQLite databases and the shipped tables.

```powershell
cargo test
cargo build --release
```
