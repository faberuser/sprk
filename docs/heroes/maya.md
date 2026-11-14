# Maya: enabling the unreleased hero

Maya (hero 14, `Hero.Maya`, Archer, physical) and her summoned beast Seves were
never released. Her base kit, model and voice exist; her perks, soul weapon and
unique treasures were empty, and Seves had no creature row. This page tracks the
work to make her playable. Art slots and stand-ins: [maya-art.md](maya-art.md).
Table editing: [jit-tables.md](../jit-tables.md).

The agreed design (kit review, perk/soul weapon/treasure proposals) is in the
shared design doc "Maya — Current Kit & Proposed Perks/Soul Weapon". This page
is the implementation record.

## Decisions

| Topic | Decision |
| --- | --- |
| Table delivery | Rebuilt `.jit` overrides generated from JSON patches in `client/tables/maya/` |
| Seves | A hidden, non-playable summon creature (row 25004, model `Hero.Maya.Pet`), 50% of Maya's ATK/HP/DEF; T5 Dark raises his ATK to 100% |
| Base kit | Keep the existing book IV text and make the effects match it; S1 gains Bleed; S3's strike moves to Seves; damage converted to the released `[ATK‰, flat]` format at the same % of ATK |
| Bleed | Stays able to crit (her signature); gains `Unavoidable, IgnoreBlock` |
| T5 Light | ATK, DEF, HP +15% / Crit DMG +20% |
| Art | Stand-ins through `SprkAtlasAliases` (see the art page) |
| Profile | New lore below |
| Text | English first; translated into the other 7 shipped languages after review |

## Status

- [x] Table tooling: `client/tools/JitTables`, `client/scripts/build/build_tables.py`; every table Maya needs and all localization files rebuild byte-identically
- [x] Art stand-ins: illustration, treasure icons, Light transcendence icons (`SprkAtlasAliases.cs`)
- [x] Lore and English text approved
- [x] Dev loop: `client/dev.py test` gives the test copy its own data folders (only `Assetbundle`, `Video`, `LoginPresentation` stay linked) and applies `client/overrides/files`; `install` applies overrides too
- [x] Seves: creature 25004 (`Hero.Maya.Pet`, the developers' unused skills `2500400`-`2500440`); `SpawnCreature` gives a summon without stat rows the summoner's stats (ATK/DEF/HP x ratio). Verified in game: spawns with exactly 50% of Maya's ATK/HP/DEF and fights on AI
- [x] Base kit: S1 Bleed and book IV crit; S2 summon/heal through Seves; S3 Seves strike, CC immunity, cooldown reset on kill; S4 condition fixed (`Blooding` -> `Bleeding`); damage format; text. Verified: tooltips show computed damage, battles complete
- [x] Perks T3/T5, unique weapon (per-star text), unique treasures, voice list, profile, `OpenType` Opened (client and server `tables/HeroShopSupport.json`)
- [x] Hero list, hero info, skills, gear and transcend tabs show correctly (perk icons through aliases)
- [x] Seves and perks verified in the native combat simulation: Seves spawns at 50% stats and fights; S2 book IV +30%, S2 Light +30%, T5 Dark +100% ATK reach him; Light and Dark builds finish without errors
- [ ] Seves's model in a UI battle not yet confirmed by screenshot (mechanics confirmed)
- [ ] Decide how Seves reaches the field in auto battle (see "Testing notes")
- [x] Soul weapon: skills `93001400`-`93001412`, `SoulWeaponSkillTable` (Time, 20 sec, 7 uses), soulstone 111014 (client item, icon alias, server catalogue, shop row, break reward, code map), upgrade prices, `IsOpenSoulWeapon`. Verified: the server liberates and advances it (test `maya_soul_weapon_liberates_and_advances_with_her_soulstone`); the Manage Soul Weapon screen shows name, skill, requirement, cooldown and uses; in the combat simulation it charges, fires (uses 7 -> 6), adds +40% base ATK and advancement 2 summons Seves with his buffs. Placeholders: Selene's bow model and node tree (see the art page)
- [x] Obtaining her: removed from `INN_RECRUITMENT_EXCLUDED_HEROES` (test `inn_offers_and_recruits_maya`); inn friendship rows and dialogue; 73 hero selectors and the 4 archer/all soulstone selectors (client and server); Special Shop (`Buyable`, 3 stars, 6,000 rubies). Verified in game: she visits the inn with her lines, Greet works, the shop sells her and the purchase adds her
- [x] Translations (Korean, Japanese, ChineseSimplified, ChineseTraditional, French, German, Russian): `client/tables/maya/91-97-text-*.json`, same keys as English; names follow each language's native strings
- [x] Installed; signed local release `local-20261113-201048`
- [x] Recruit cinematic: Maya's node-tree pack lacks `GetNewHero`, so buying or recruiting her froze on a black screen. `SprkNodeTreeFallbacks` now supplies it (and `Transcendence`, `GetSoulWeapon`, reinforce trees) from Selene's, and `GetNewHero2` ends the scene if a tree is still missing. Verified: buying her plays her `Get` motion and opens the NEW HERO panel. Not yet seen in game: liberation and transcendence scenes (testfaber lacks the story progress)
- [x] Second pass (in-game feedback): T1-T5 transcendence used Maya's camera at Selene's field-of-view scale (stretched view), now her own camera at scale 1 (verified T0 to T1); purification camera lowered to her kneeling pose (verified); Light/Dark framed perk icons (verified on the Transcend tab); unique weapon/treasure/soulstone selectors: Maya's items join every selector that lists all four original archers' items of that type (`WeaponUniqueSelectItemTable` 162 rows, `SelectItemTable` 6 gacha rows, matching server `EquipmentSelectors`/`Selectors`; tests updated for the 92/412-item pools)
- [x] Soul weapon screen: requirement text states the fixed 20 sec (the native `SkillDescription` rows for this line are constants too: Kasel 16, Selene 60; the table can be decrypted with the AES key in `D:/kr/sprk/kingsraid-table-data` and rebuilt with the game's `JsonToBinary` if computed values are ever needed); avatar falls back to her hero icon; her real UW mesh replaces Selene's bow (verified in the Manage Soul Weapon screen)
- [x] Third pass: default costumes 1403-1405 (client `CostumeTable` and server `Costumes`; test `maya_default_costumes_exist_and_serve_as_avatar`) fix the empty Hero Index profile (a null costume crashed `HeroBook.SetupHero3D`) and enable her as avatar; See Motions rows; soulstone icon with her badge; Special Shop preview scale/offset. Verified in game: Hero Index profile and model, See Motions list, soulstone icon, shop preview

Not needed: limit-break rows (the server rejects limit break; max level comes
from star and transcendence) and costume rows.

## Changes from the approved design

Made while implementing, where the game has no matching mechanic:

| Item | Approved | Implemented |
| --- | --- | --- |
| UW second line | Maya deals +X% DMG to bleeding enemies | Enemies inflicted with Bleed take +X% DMG (no attacker-side modifier exists) |
| S2 Light | Shield = 20% of Seves's Max HP | Shield = 300% of Seves's ATK (no max-HP shield operation) |
| S4 Light | Bleed stacks to 5, +20% per stack | +20% per stack; Bleed keeps the game-wide cap of 3 |
| T5 Dark | Seves inherits 100% ATK and Crit; extra hit when Seves also attacks the target | Seves's ATK +100% (crit stats are already inherited); extra hit on any bleeding target, 1 sec cooldown |
| UT4 | Mana on extra DMG; Seves takes 20% less DMG | +X% DMG to S4; Seves takes 20% less DMG |
| S3 Dark mana | When the target is killed | When it is killed by Maya's S3 hit (Seves's kills are his own) |

## Testing notes

- Use a test copy (`client/dev.py test`) and client automation; add Maya to
  the test account in the isolated database (`heroes`, `hero_details` JSON with
  `SkillExtend1-4` and `TranscendSkillPage1`, `equip_items` through the
  identity column).
- Auto battle rarely casts S2 (4 orbs) while S1/S3 (3 orbs) are ready, so Seves
  often appears late or not at all in short fights.
- `combat-simulation` (client automation) runs native battles headlessly; use
  `chapter;dungeon;difficulty;heroes`, for example `2001;5;0;14` (Ordeal
  Tower) or `7004;1;0;14` (World Boss). Campaign stages end immediately there.
  Add a fifth field `soul` (`2001;20;0;14,1,2,4,15;soul`) to fire charged soul
  weapons; each hero sample then reports `soul` (stack, max, uses left) and
  `stateSkills` (skill:operation of active states). Fights there end before
  20 sec, so to see Maya's soul weapon fire, build a throwaway table copy with
  `SoulWeapon14` `MaxStack` lowered and copy only its `TableJit` into the test copy.
- Unleashing in the UI needs the Temple of the God King (chapter 65-11) cleared;
  testfaber has not, so seed `extension_state` (`kind='soul'`) instead.
- The inn intro tutorial (2160) blocks inn requests on testfaber; mark it
  complete in the isolated database. Client automation launches
  `target/release/sprk-server.exe`: rebuild it (`cargo build --release`) after
  server changes.
- Two engine rules found while testing:
  - Damage from a passive triggered by attacks (`OnAttack` with no skill
    filter) re-triggers it. Such damage needs `IgnoreAttackersTrigger` (or a
    cooldown), otherwise several hits recurse until the game freezes. This was
    latent in Maya's original S4 and Seves's `2500440`.
  - `UseSommoneeSkill` only works for skills the summon owns; Seves lists his
    heal and buff skills in `ExtraSkill` with `TriggerDisabled`.

## Data map

| What | IDs |
| --- | --- |
| Hero row | `CreatureTable` 14 (`OpenType` is `CommingSoon` in the baseline) |
| Skills | base 1400, S1 1410 (+1411-1413), S2 1420 (+1421-1423), S3 1430 (+1431-1433), S4 1440 (+1441-1443) |
| S2 summon | `SpawnCreature` 25004 (row missing); Ordeal-boss Seves rows 25504/25604 have no skills |
| Perks | T1 shared `60000001-04, 33`; T2 archer `60000017-20, 37`; T3 `60001410/11, 20/21, 30/31, 40/41`; T5 `60001450/51` |
| Unique weapon | item 1014 "Serrated Claws, Satis", option 11014 -> skills 90000140-145 (Bleed 20% ATK/s, 10 s, no flags) |
| Unique treasures | options 14014/13014/12014/15014 -> skills 91001410-15 / 20-25 / 30-35 / 40-45 (all empty) |
| Soul weapon | `SoulWeaponSkillTable` `WEAPON_UNIQUE_MAYA_1`: base 93001400 (pounce 93001411, Bleed 93001412), advancements 93001401/02, condition 93001410 (state `SoulWeapon14`, `MaxStack` 20); soulstone 111014 `SOULSTONE_MAYA_1`; display models 901014/911014 |
| Obtaining | Inn: `HeroFriendlyTable`/`HeroFriendlyActionTable` 14, server `tables/HeroInnRecruitment.json`; selectors: `HeroSelectItemTable` (73), `SelectItemTable` (4), server `tables/InventorySupport.json`; shop: server `tables/HeroShopSupport.json` `Heroes.14.Buyable` |
| Transcendence | `HeroAwakeTable` hero 14 (present); items `DARKNESS_MAYA_3-5`, `LIGHTNESS_MAYA_1-5` |
| Text | `CREATURE_NAME_14` "Maya", `CREATURE_TITLE_14` "Valiant Beastmaster Maya", skill names/descriptions for 1410-1443 |
| Voice | `Sound_VOICE_Hero_Maya.unity3d`: 14 clips (attacks, skills 1-4, damage, death, victory, 5 Seves sounds) |

## Profile and lore (draft)

| Field | Value |
| --- | --- |
| Gender | Female |
| Race | Human |
| Age | 24 |
| Height | 168 cm |
| Birthday | Month 5, day 12 (constellation of month 5) |
| Likes | Seves's warm mane, the first snowfall in the deep forest, roasted boar, a boomerang that comes back true |
| Dislikes | Poachers, cages and leashes, the noise of big cities, being called a "tamer" |

**Story (`CREATURE_STORY_14`):**

Maya was born in a hunting village on the edge of Orvel's great northern
forest, where children learn to read tracks before they learn to read letters.
Her mother was the village's finest hunter, and her father carved the curved
throwing blades that the villagers used to bring down birds on the wing. Maya
inherited both gifts. By fifteen she could throw a boomerang through a gap
between two branches and catch it again without looking.

That same year she found a wounded cub tangled in a poacher's net: a strange
beast with teal fur, a fiery mane and a tail that ended in a hard, spear-like
spike. Beside the net lay his mother, felled by poisoned arrows. The poachers
came back for their prize. Maya drove them off with nothing
but her blades and her voice, then spent a whole winter nursing the cub in a
hunter's hut. She named him Seves, "the one who stays", because when spring
came and the forest called, he did not leave her side.

Seves grew far larger than any beast the villagers knew, and some of them
began to whisper that the creature would turn on them one day. Maya chose to
leave rather than see her partner caged. Since then the two have wandered
Orvel as hunters for hire, clearing monster nests from roads and farmland and
selling what they catch to merchants (though Maya will warn you which of it
tastes terrible). She forged gold-trimmed armor for Seves from the scales of a
drake they brought down together, and wears a hunter's helm carved from the
same horns.

Maya is blunt, proud and fiercely loyal. She has little patience for nobles who
call Seves a "pet" or for anyone who treats the forest as something to be
emptied. In battle the pair move as one: Maya's boomerangs open wounds, and
Seves finishes what she starts. People call her a beastmaster; Maya simply
calls Seves family.

## English text (draft)

Keys marked *new* do not exist in the game yet.

### Base kit corrections

| Key | Text |
| --- | --- |
| `SKILL_DESC_1410` | Throw a boomerang that travels back and forth in a straight line to deal a max of [ffc800]{1}[-] P.DMG and inflict Bleed for [64ffff]10[-] sec. |
| `SKILL_DESC_1443` | Seves recovers HP equal to [64ff64]{1}[-]% of the DMG he deals. |

S1 said "M.DMG" for a physical skill. S4 IV said "heals 50% of DMG taken"; the
new wording says what the effect will do. Other book texts stay as they are and
their effects change to match them.

### Perks

| Key | Text |
| --- | --- |
| `SKILL_DESC_60001410` *new* | DMG is increased by 40%. Bleed inflicted by this skill lasts 5 sec longer. |
| `SKILL_DESC_60001411` *new* | Reduces Mana Cost by 1. |
| `SKILL_DESC_60001420` *new* | When Seves is summoned or healed by this skill, Seves's ATK is increased by 30% and he gains a Shield equal to 20% of his Max HP for 15 sec. |
| `SKILL_DESC_60001421` *new* | If Seves is already summoned, also dispels all negative effects from Seves and makes him immune to CC for 5 sec. |
| `SKILL_DESC_60001430` *new* | DMG is increased by 40%. Against enemies inflicted with Bleed, DMG is increased by 80% instead. |
| `SKILL_DESC_60001431` *new* | Seves targets the enemy with the lowest HP %. When the target is killed, Maya also recovers 1000 Mana. |
| `SKILL_DESC_60001440` *new* | Bleed can be stacked up to 5 times. Extra DMG from this skill is increased by 20% per Bleed stack on the target. |
| `SKILL_DESC_60001441` *new* | Enemies inflicted with Bleed take 15% increased P.DMG and have their Heal Rate reduced by 30%. |
| `SKILL_DESC_60001450` *new* | ATK, DEF, HP +15% / Crit DMG +20% |
| `SKILL_DESC_60001451` *new* | [Pack Tactics] Seves inherits 100% of Maya's ATK and Crit stats. When Maya's auto attack hits an enemy inflicted with Bleed that Seves is also attacking, deals additional P.DMG equal to 200% of ATK that ignores DEF. |

### Unique weapon: Serrated Claws, Satis

| Key | Text |
| --- | --- |
| `SKILL_DESC_90000140`-`145` | Each auto attack inflicts Bleed on the enemy for 10 sec, dealing P.DMG equal to [64ff64]{0}[-]% of ATK every sec. Bleed can be stacked up to 3 times. Maya deals [64ff64]{1}[-]% increased DMG to enemies inflicted with Bleed. |
| values by star 0-5 | {0}: 20, 24, 29, 35, 42, 50 · {1}: 10, 12, 14, 17, 21, 25 |
| `ITEM_STORY_1014` *new* | A pair of hooked boomerangs carved from the claws of Seves's mother. They always return to Maya's hand, and the wounds they leave never close cleanly. |

### Unique treasures

| UT | Name *new* | Description *new* | {0} by star 0-5 |
| --- | --- | --- | --- |
| 1 | Hunter's Whetstone | [Penetrating Boomerang] Increases DMG by {0}%, and the boomerang inflicts 2 stacks of Bleed. | 20, 24, 29, 35, 42, 50 |
| 2 | Beast-Calling Whistle | [Start Hunting] Reduces Cooldown by 30%. When Seves is summoned or healed, his ATK is increased by {0}% for 15 sec. | 10, 12, 14, 17, 21, 25 |
| 3 | Fang Charm | [Seves] Increases DMG by {0}%. If the target is inflicted with Bleed, reduces its P.DEF by 15% for 10 sec. | 20, 24, 29, 35, 42, 50 |
| 4 | Pack Leader's Collar | [Bouncing Boomerang] Upon dealing extra DMG to an enemy inflicted with Bleed, recovers {0} Mana (0.5 sec cooldown). Seves takes 20% reduced DMG. | 100, 120, 140, 170, 210, 250 |

Treasure stories *new*:

1. **Hunter's Whetstone.** A flat river stone worn smooth by years of sharpening. Maya never throws a boomerang she has not honed on it herself.
2. **Beast-Calling Whistle.** Carved from a hollow antler. Its note is too low for human ears, but Seves hears it from the far side of a mountain.
3. **Fang Charm.** Seves's first milk tooth, bound in silver thread. Maya swears it has turned more than one arrow aside.
4. **Pack Leader's Collar.** A beaded leather band Maya wears on her wrist. Seves has its twin, and neither of them has ever taken it off.

### Soul weapon

| Key | Text |
| --- | --- |
As shipped (`client/tables/maya/90-text-english.json`). The soul is Fenra, the
soul of Serrated Claws, Satis; the weapon name follows the native "Unleashed
<weapon>" pattern.

| Key | Text |
| --- | --- |
| Name (`SOULWEAPON_NAME_1014`) | Unleashed Serrated Claws, Satis |
| Requirement (`SKILL_DESC_93001410`) | Charges up over 20 sec. 7 uses. |
| Description | Summons Fenra, the soul of Serrated Claws, Satis, for 12 sec. Increases the ATK of Maya and Seves by 40%, and Maya's attacks inflict Bleed. Every 3 sec, Fenra pounces on the enemy with the lowest HP %, dealing P.DMG equal to 250% of ATK that ignores DEF. For the duration, enemies inflicted with Bleed take 20% increased P.DMG. |
| Advancement 1 | Increases the duration by 4 sec. Each pounce recovers 500 Mana for Maya. |
| Advancement 2 | If Seves is not summoned, summons Seves. Seves becomes immune to CC for the duration of the Skill. |
| Soulstone story | Fenra, the first beast that ever hunted, sleeps within this stone. It answers only those who hunt beside a partner they would die for. |
| Soul weapon story (`SOULWEAPON_STORY_14`) | Fenra wakes in the claws the night a poacher's arrow strikes Seves down, and has run a step ahead of the pair ever since. |

## Risks found so far

- `ItemTable` references gacha effects `Scene.Gacha.Treasure.Maya.1-4` that have no bundles; check the treasure reveal.
- Maya has no `SoulWeaponGet`/`SoulWeaponSkill1` animations; liberation and soul weapon use play without her own motion.
- `CostumeTable` cannot be rebuilt byte-identically (see the table guide); avoid needing costume rows, or fix that first.
