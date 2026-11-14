# Editing game tables (.jit) and localization

Game data such as hero stats, skills, perks, items and text ships as binary
`.jit` files in `King's Raid_Data/Documents/Patch/StandaloneWindows/`:

| Folder | Contents |
| --- | --- |
| `TableJit/<Name>Table.jit` | One table each (`CreatureTable`, `SkillTable`, `HeroLimitBreakTable`, ...) |
| `LocalizationJit/LocalizationAll_<Language>.jit` | All text for one language, keyed by a hash of the text key |

They have no source form. SPRK keeps edits as **readable JSON patches** under
`client/tables/` and regenerates the binary files from the pinned baseline
release. The generated files are committed under `client/overrides/files/` like
any other override, so CI and releases need no extra step.

```text
client/tables/<feature>/*.json  ──┐
pinned baseline (TableJit, ...) ──┼─ client/scripts/build/build_tables.py ─> client/overrides/files/.../TableJit/*.jit
                                   │   (runs client/tools/JitTables)          .../LocalizationJit/*.jit
                                   └──────────────────────────────────────>  .../SPRK-Native*.json
```

`client/tools/JitTables` loads `Assembly-CSharp.dll` and uses the game's own
table containers, JSON row parser (`JsonMarshalerUsingReflection`) and
serializer (`JitPacker`), the same path the original developers used to turn
their JSON tables into `.jit`. Before patching a file it rebuilds the
**unpatched** file the same way and stops unless the result is byte-identical
to the input, so a patch changes only the rows it names.

## Quick workflow: change a value

Example: lower Maya's S1 cooldown.

1. Make sure managed references are staged: `python client/dev.py build` (once).
2. Find the row. Decoded copies of every table are in the sibling
   `kingsraid-table-data/output/` (string-pool indices) and the readable
   [krinfo](https://krinfo.net) data; the authoritative form is a dump:

   ```powershell
   cd client
   dotnet run --project tools/JitTables -c Release -- dump "<native-dir>/TableJit" SkillTable ../artifacts/s1.json 1410
   ```

   `<native-dir>` is a pristine baseline folder. After one run of
   `build_tables.py` it is `artifacts/table-baseline/<tag>/native`. Do not dump
   from `../sprk-client` once it contains your patches.
3. Copy the row into the feature's patch file (for Maya,
   `client/tables/maya/SkillTable.json`) or edit it there if it already exists,
   for example `"CoolTimeMs": 6500`.
4. Regenerate the overrides from the repository root:

   ```powershell
   python client/scripts/build/build_tables.py
   ```

5. Test in the real game, publish the required local release, and commit the
   patch file together with the regenerated overrides and
   `client/overrides/generated-tables.json`.

> **Testing limitation (to be fixed with the Maya work):** `client/dev.py`
> does not copy `client/overrides/files` into a game folder, and the test copy
> made by `dev.py test` links the real game's whole `Documents` folder (which
> holds `TableJit` and `LocalizationJit`) through a junction. Writing table
> overrides into the test copy would therefore change the real game. Until the
> dev loop gives the test copy its own table folders, test table changes only
> through the release build (`build_client_release.py --check-only`) or by
> installing into `../sprk-client` and publishing a local release.

`python client/scripts/build/build_tables.py --check` fails when the committed
overrides do not match the patches; run it before committing.

## Patch files

Any `*.json` file under `client/tables/` (any depth) is a patch. Files are
applied in path order. A patch edits exactly one table **or** localization.

### Table patch

```json
{
  "table": "SkillTable",
  "removeWhere": [ { "HeroIndex": 14 } ],
  "upsert": [ { "Index": 1410, "CoolTimeMs": 6500, "...": "..." } ],
  "append": [ { "...": "..." } ]
}
```

| Field | Effect |
| --- | --- |
| `upsert` | Replaces the row with the same key (in place, keeping order) or adds it at the end |
| `removeWhere` | Removes every row whose listed fields all equal the given values (run first) |
| `append` | Adds rows without matching keys (for list tables such as `HeroLimitBreakTable`, `HeroVoiceTable`) |

Rows are **complete**: an upserted row replaces the whole row, and any field
left out takes the data class default (0, empty, false). Always start from a
dump. A dump already omits fields that equal the default, so it is the
shortest correct row.

Row format is the game's original JSON table format:

- Field names are the C# property names of the row class (`NShared.SkillData`,
  `NShared.CreatureData`, ...). Search `client/managed/Assembly-CSharp/NShared`.
- Enums are written by name: `"OpenType": "Opened"`, `"TagType": "Archer"`,
  `"OperationType1": "GetPhysicalDamageR"`. Flag enums use `"A, B"`.
- Item references use item codes: `"GradeStoneItemCode": "SKILLBOOK_ARCHER_1"`.
- Several numeric-looking columns are string arrays: `"OperationValue1": ["2500"]`.

Keys come from the row class's `IDataKeySelector<K>.Key` (usually `Index`). A
duplicate key after patching is an error.

### SkillTable operations

`SkillTable` keeps a skill's effects (operations) beside the row. In a patch
they are flattened like the original tables: `OperationType1`,
`OperationTargetType1`, `OperationValue1`, ..., up to operation **8**. The
loader only reads operation `n` when `OperationType<n>` is present. Upserting a
skill replaces all of its operations.

Useful operation fields (see `NShared.OperationData.LoadData` for all):

| Field | Meaning |
| --- | --- |
| `OperationType<n>` | What happens (`NShared.OperationType`): `GetPhysicalDamageR` (% of ATK, 1000 = 100%), `GetPhysicalDotR`, `GetHealR`, `SpawnCreature`, `ModSkillPower`... |
| `OperationTargetType<n>` | Who: `Target`, `Myself`, `Parent` (targets of operation `OperationParent<n>`)... |
| `OperationValue<n>` | Arguments as strings; meaning depends on the type (ratios are per mille) |
| `OperationValueFactor<n>` | Level scaling `[0, 0, factorType, flat]` |
| `OperationDurationMs<n>`, `OperationTickTimeMs<n>` | Buff/debuff duration and tick |
| `OperationState<n>` | State name shown and stacked (`Bleeding`, ...) |
| `OperationConditionType<n>` / `OperationConditionValue<n>` | Only apply when the condition holds |

How each type uses its values is in `client/managed/Assembly-CSharp/NShared.NOperationRuntime/<Type>.cs`.
[Damage formula](damage-formula.md) explains how ratios become damage.

Common balancing fields:

| Table | Row | Fields |
| --- | --- | --- |
| `SkillTable` | skill index (hero `n`: base `n00`, S1 `n10`, S2 `n20`... books `n11-n13`) | `Mp` (mana, 1000 = 1 orb), `CoolTimeMs`, `OperationValue<n>` |
| `SkillTable` | perks `6000nnxx`, UT `9100nnxx`, UW `900000nx`, soul weapon `9300nnxx` | as above |
| `CreatureTable` | hero index | skill links, perks (`TranscendSkill1-5`), `OpenType`, position |
| `CreatureStarGradeStatTable`, `CreatureLevelStatTable2` | `CreatureIndex` rows | base stats per star/grade/level |
| `SoulWeaponSkillTable` | soul weapon item code | `BasicSkillIndex`, `MaxUseCount`, add-on skills |
| `HeroLimitBreakTable` | `HeroIndex` × `LimitBreakLevel` | experience and materials |

Skill text that shows numbers also has a formula in `TableData/SkillDescription`;
see [SprkSkillDescriptionFormula](../client/managed/SprkSkillDescriptionFormula).

### Localization patch

```json
{
  "localization": {
    "English": { "SKILL_NAME_60001410": "Penetrating Boomerang [Light]" },
    "Korean":  { "SKILL_NAME_60001410": "..." }
  }
}
```

Languages are the file names: `English`, `Korean`, `Japanese`,
`ChineseSimplified`, `ChineseTraditional`, `French`, `German`, `Russian`. Keys
are hashed with `JitOption.StringToHashCode` (the same function as
`kingsraid-table-data/lookup_localization.py`); existing keys are replaced.
Color tags such as `[ffc800]...[-]` and `{1}` placeholders work as in the game.

## Integrity metadata and generated files

`build_tables.py` updates `SPRK-NativeData.json` (tables) and
`SPRK-NativeLocalization.json` (localization) in the overrides: `sha256` is the
generated file, `originalSha256` keeps its existing value or records the
baseline file. `client/overrides/generated-tables.json` lists the overrides the script
owns; when a patch stops touching a table, the next run deletes that override
so the baseline file ships again. Its metadata entry is left as is; correct
it by hand if that ever matters.

A table that also has a hand-made override is not supported: the script
always patches the baseline copy. Move such edits into a patch instead.

## When the baseline is re-pinned

The generated files depend on the baseline. After
`pin_client_baseline.py`, run `build_tables.py` again; it downloads the new
baseline (`gh` must be logged in) and regenerates every override.

## Tool reference and troubleshooting

Run from `client/` (so `global.json` selects the SDK):
`dotnet run --project tools/JitTables -c Release -- <command>`.

| Command | Use |
| --- | --- |
| `dump <TableJit-dir> <Table> <out.json> [key...]` | Rows as a patch (all rows, or the given keys) |
| `verify <TableJit-dir> <Table>...` | Dump and rebuild every row; must report `identical=True` |
| `roundtrip <TableJit-dir> <Table>...` | Load and save without the JSON step |
| `rebuild <TableJit-dir> <Table> <out.jit>` | Write the unpatched rebuild to diff a failed `verify` |
| `diffrows <TableJit-dir> <Table> [limit]` | Field-by-field differences between rows and their parsed dumps |
| `loc-verify <file.jit>...` | Round-trip localization files |
| `sprites <BundlePack.bin> [regex]` | Sprite path -> atlas / sprite name (for [art aliases](heroes/maya-art.md)) |
| `bundle-index <BundlePack.bin> <regex>` | Every index entry (camera, sound, effect, creature, sprite) whose key matches, e.g. to check a hero's `Get.<Name>` assets |
| `loc-get <file.jit> <key>...` | Text stored for localization keys |
| `bjs <file.bjs>` | A PlainAsset JsonBin file (node-tree pack, etc.) as JSON, e.g. to list a hero's cinematic trees |
| `build <native-dir> <patch-dir> <out-dir>` | What `build_tables.py` runs |

Known limits:

- `CostumeTable` cannot be rebuilt byte-identically: SPRK hard-codes the getters
  of `CostumeData.IsOpen`, `PreviewableWhenOwned` and `IsShow`, and the
  serializer writes those forced values. The game never reads the stored
  values, so `build` accepts this table when the rebuilt rows decode to the
  original rows apart from those three fields (`JitTable.ForcedGetterFields`);
  any other difference still refuses the patch.
- Tables whose containers need a running game are not listed; `dump` reports
  `Unknown table`.
- Only hashed localization packers are supported (all shipped languages are).

If `verify` fails for a new table, compare with `rebuild` and `diffrows`. So
far the causes were patched getters (the tool reads backing fields) and
dictionary insertion order (the tool restores the input's key order).
