# Hero stat audit — 2026-10-16

The low ATK report was reproduced with Pansirone's existing level-100, T5,
A2/E20 build. A restored table was being used with a later Soul Weapon formula
in both the installed Windows client and the Rust server. That formula divided
Soul Weapon ATK and HP by an extra 1000.

## Verified result

The two runs used the same isolated copy of `sprk.db`, identical equipment,
options, runes and progression, and separate client processes. The first used
the pre-fix assembly; the second used the installed corrected assembly. No live
account equipment or progression was edited.

| Pansirone stat | Before | Corrected |
| --- | ---: | ---: |
| Character-sheet ATK | 231,408 | 872,707 |
| Max HP | 6,040,096 | 10,259,624 |
| P.DEF | 50,137 | 50,137 |
| M.DEF | 26,778 | 26,778 |
| ATK Spd | 1,000 | 1,000 |
| Crit | 310 | 310 |
| Crit DMG | 210% | 210% |
| Penetration | 100 | 100 |
| ACC | 200 | 200 |
| Recovery | 25% | 25% |

These are actual Unity `StatCalculator.GetEquippedHeroStat` results and native
`HeroStatPanel` label values, not estimates from a replacement stat calculator.
The current build's corrected ATK is 872,707; the audit does not establish the
equipment or buffs of the remembered one-million-ATK build.

Pansirone's Soul Weapon allocation is 688 ATK / 312 HP, with bonus ratios 154 /
49. Its base ATK should be 176,595 and bonus ATK 27,195; the old calculation
returned 176 and 27. Its base HP should be 2,224,560 and bonus HP 109,003;
the old calculation returned 2,224 and 108. Gear percentage bonuses magnify the
missing flat stats. Both physical and magical ATK use the corrected calculation.

All 31 owned heroes were compared in the real client with identical equipment.
Among the nine combat stats exposed by the existing automation probe, only
physical ATK, magical ATK and HP changed. The Pansirone detail-panel comparison
additionally checked the secondary stats shown above.

## Cause and reference versions

The build-1558 backup at
`Z:/media/kingsraid/Pre-Doomsday/com.vespainteractive.KingsRaid` stores Soul
Weapon ATK coefficients 7/8/9 and HP coefficient 250. The actual encrypted
`MessagePack/SoulWeaponOptionTable.mpb` was decoded again, independently of the
named export. Its MD5 `383f786f1c9283388d27f8f00727eed9` matches the backup's
own patch manifest. The installed native JIT and server support table contain
those same values. The data itself did not need multiplication or replacement.

For these coefficients the original calculation is:

```text
factor = (1000 + etherStatRate) * (1000 + advancementRate) / 1000
base   = allocationRatio * classCoefficient * factor / 1000
bonus  = base * bonusRatio / 1000
final  = base + bonus
```

Every division truncates before the next operation. Later Windows data uses
coefficients 7000/8000/9000 and 250000, paired with another `/1000` in the base
calculation. The table restoration retained the later arithmetic, producing
the mismatch. Login stat refresh and Soul Weapon operations in the Rust server
had copied that later arithmetic too. Saved pre-restoration Soul Weapon stats
also agree with the corrected outputs.

The archived **4.86.6 ARM64** routine at RVA `0x30d6b38` was disassembled and
executed with Unicorn, using the locally retained binary from the earlier
archive investigation (`target/arena-prereserve/libil2cpp.so`, SHA-256
`201ca83ea3fb77e1592046734d4914ed92c11ef6a8f1234ebec7b17188501660`). Six fixed
outputs were checked against the staged and installed Windows methods.

The currently supplied `KingsRaid 9-1-2022` APK identifies itself as **4.88.8**.
Its x86 routine at RVA `0x1cb19f2` has the extra division. These are different
client versions. Neither APK's metadata is asserted to be byte-identical to the
backup's installed metadata. The formula must be paired with its table units;
comparing table values alone, or copying the 4.88.8 arithmetic alone, does not
establish correctness for the restored build-1558 data.

## Broader coverage

Read the installed JIT row lists and string pools directly; JSON exports of the
current client were not treated as authoritative. Across 41 distinct stat and
equipment tables, the shared reference data matched, including:

- All 11,804 hero/star/transcendence rows and their 85 recovered fields.
- Hero level growth, base HP/ATK/DEF, skill level factors and equipment upgrades.
- All 23 general soft-cap rows, multiplicative attenuation and related stats.
- All 2,686 equipment rows, 1,453 equipment-option rows, set bonuses, slots,
  enchantment rules, 224 rune rows and Punishment rune options.
- Soul Weapon coefficients, reinforcement, advancement, node trees and skills.
- Class buffs, Valance enchantments/awakening and stat display data.

The existing restored defense helper and Season1 damage pipeline remain active.
Their tests cover modifier accumulation, toughness, separate boss bonuses,
post-defense shields, boss HP gates and Technomagic mechanics. One-million
incoming damage against 100,000 effective DEF at level 100 still yields 178,000
at the defense step, before subsequent modifiers.

Known dataset differences remain: ten additional equipment star rows (6–15),
176 additional distinct monster-tier IDs, four extra constants and duplicate
reference rows. The shared equipment star rows 0–5 match. Duplicate monster IDs
require care: first/last-row comparison alone does not prove Android runtime
precedence. No balance change was made for these differences.

This is not a complete Android-versus-Windows replay comparison of every skill,
artifact, conditional modifier, AI script or encounter. Matching their input
tables does not prove all runtime behavior identical.

## Changes and checks

- `HeroStats.cs` removes only the extra Soul Weapon divisor in the installed
  calculator, rejects unknown IL layouts and checks patch idempotence. Full
  patching also applies the correction. The focused installer validates baked
  native combat table hashes first and retains a timestamped DLL backup.
- Server Soul Weapon calculations now use the same legacy units for login,
  liberation, progression and renewal responses. Saved progression is retained.
- Six independently executed native reference outputs and 4,032 installed-client
  stat/bonus cases pass.
- Existing combat checks pass: 32 reducer, 34 legacy and 23 Technomagic cases.
- Seven Rust tests selected by `cargo test soul_ -- --nocapture` pass, including
  login refresh, max Soul Weapon ATK/HP, ownership and snapshot checks.
- An optimized server build completed in
  `target/stat-audit-20261016/server-build/release/sprk-server.exe`; a convenient
  copy is staged at `target/release/sprk-server.hero-stats-staged.exe`.
- Existing Orvel visual and Conquest session-cache/party-cleanup hooks verify.

Runtime observations, native disassembly/emulation, table comparisons and the
before/after detail-panel labels are retained under `target/stat-audit-20261016`.
The reused core table comparison is under `target/formula-audit`.

The running local server and battle worker need a restart with the rebuilt
server/client code. The normal `start_server.bat` workflow builds the updated
server after the old local services stop. The audit's isolated server and test
clients were closed; it did not restart the existing game/update services.

## Signed client release

Installed correction published as **`local-20261016-hero-stat-scaling-01`**.
All 656 release files were checked against their manifest sizes and SHA-256
hashes. The stable/release manifests are byte-identical, their RSA-SHA256
signature verifies with the existing public key, and the installed changed DLL
matches its published hash
`e549f3f779282529b7a4c8645aaddb27e7036d56be42830e56429c594d3c22cd`.
The previous complete managed-file selection was retained, including engine
assets outside the publisher's defaults. There are no new deletions; the 154
historical removal entries were carried forward unchanged. All three native
data/localization/combat manifests also verify after the Unity runs.

For public deployment, upload
`client-updates/releases/local-20261016-hero-stat-scaling-01/` and the updated
`client-updates/stable/manifest.json`. Local artifacts do not deploy the homelab
update host. Deploy the rebuilt server as well so login and renewal displays use
the same units as the client and battle worker.

## Transcendence perk follow-up — 2026-10-16

Selected transcendence perks are combat passives and are excluded from the
Common-tab equipment stat calculation. This matches the archived client.
Transcendence-stage growth itself is part of the hero's base stats; selecting
or resetting a perk is a separate operation.

The archived 4.86.6 ARM64 call paths confirm this separation:

- `HeroStatPanel.SetupMainStat` (`0x2f2e284`) reads `StatDataCache` hero and
  equipped-hero stats.
- `StatDataCache.GetCachedHeroStatDetail` (`0x318bdf8`) calls
  `StatCalculator.GetEquippedHeroStat`. That calculator reads base growth,
  equipment, Soul Weapons, runes and set/options, without loading transcendence
  skills.
- `SkillController.InitPassiveSkill` (`0x28c8064`) reads the selected
  `HeroInfo.GetTranscendSkillPageInfo` and registers each selected perk as an
  extra passive skill. The installed client follows the same paths.

Pansirone's T1 attack perk is skill `60000001`, selected-page code `10`. Its
archived operation is `ModPowerR` with value `300`. Her T2 attack perk is skill
`60000021`, code `20`, with attack value `200` and a separate penetration
operation. Both are startup passives. The installed skill-data probe agrees
with the archived definitions.

Tested three independent Unity client sessions with a copy of the current save,
changing only Pansirone's selected perk list in that isolated database:

| Selected perks | Common-tab ATK | Battle ATK at 1,000 ms |
| --- | ---: | ---: |
| None | 872,707 | 915,451 |
| ATK +30% | 872,707 | 1,177,263 |
| ATK +30% and +20% | 872,707 | 1,351,804 |

The battle values come from the native detached battle simulation against
Apocalypsion, chapter 70000/dungeon 1, using her existing equipment and normal
startup passives. The isolated client performs no battle-entry or reward API
calls. The extra ATK attributable to the selected perks is exactly
`floor(872707 * 300 / 1000) = 261812` and
`floor(872707 * 500 / 1000) = 436353`, respectively. The two perk percentages add
together in this test. Both physical and magical ATK changed by those amounts.
All three battle-creation stat inputs remain 872,707 before the passives apply.

The initial story stage was unsuitable for this test because it contained no
enemy combat and ended at 400 ms; it was replaced with the Apocalypsion test
above. The reported battle results require a real combat sample after startup.

Evidence is retained in `target/perk-audit-20261016`: `native-callpaths.json`,
native disassembly, `archived-perks.json`, `installed-perks.json`, the three
simulation captures, `results.json` and `verified-summary.json`. All isolated
test clients and the fixture server were closed. No installed game files or
live account perks were changed, so this follow-up requires no client release.

## Rune comparison with the old Theo video — 2026-10-16

The reported video shows Theo at 1,452,834 ATK in the Common tab, with 93.5%
total rune ATK. The exact Theo equipment/Soul Weapon build was not supplied.

Pansirone's currently equipped rune page contains three `RUNE_FIERCE_5` runes
(item 100005), each contributing 250 per thousand (25%) ATK, plus HP runes in
the other two slots. The total rune ATK is therefore 750 (75%). Her equipped
ATK options contribute another 1400 (140%): four 16% gear options, 20% total
ATK enchantments, and four 14% treasure options. The native calculator adds
these percentage sources before applying them to the combined flat ATK.

Holding every other ATK input fixed gives:

```text
Combined flat ATK: 277050
Current: floor(277050 * (1000 + 1400 + 750) / 1000) = 872707
93.5% runes: floor(277050 * (1000 + 1400 + 935) / 1000) = 923961
```

The combined flat ATK is the unique integer consistent with the measured
872,707 Common-tab ATK and the installed option/rune coefficients. This is a
calculated same-build rune comparison, not an actual rune swap or a measurement
of Theo's build. It predicts +51,254 ATK, approximately +5.87%, leaving 528,873
between that hypothetical Pansirone build and the video's Theo. Rune ATK is an
additional 18.5 percentage points in the shared ATK-bonus sum, rather than an
independent 18.5% multiplier on the displayed final ATK.

The rune difference alone therefore does not account for this comparison.
Theo's hero/unique-weapon stats, Soul Weapon allocation and bonus rolls, ATK
option/enchantment totals and other equipped items must also be compared.
Pansirone's current main gear includes Crit-DMG options as well as ATK options.
The screenshot's Common-tab value excludes selected transcendence perk bonuses,
as established in the preceding follow-up.

Velkazar rune definitions already exist in the installed `RuneItemTable.jit`
(tag `VELKAZAR_RUNE`, including dual-stat runes with 11% ATK). Their presence in
the data does not establish that every acquisition route is implemented or
that the player's inventory contains them. This comparison assumes the user's
93.5% total; the video's precise rune composition was not independently checked.
The arithmetic and current equipment sources are recorded in
`target/rune-audit-20261016/comparison.json`. No client or live account changes
were made.

### Rune implementation and the Normal Rune filter

The installed `HeroRuneListPanel.GenerateCategoryButtonComponent` creates only
one category button, for `ItemType.Rune`. There is no inventory or account gate
around adding a second category: the method simply never adds
`ItemType.PunishmentRune`. The installed DLL was decompiled into
`target/rune-audit-20261016/installed` to confirm this directly.

Velkazar runes use `ItemType.Rune`, so they belong under **Normal Rune**, rather
than requiring their own dropdown entry. Both the installed RuneItem JIT and
server ExtensionSupport contain 95 definitions tagged `VELKAZAR_RUNE`.
For example, item 100167 (`RUNE_FIERCE_SHARPNESS_5`) has 110 per thousand
(11%) physical and magical ATK in the installed JIT and supports all five rune
slots. The server's normal `equip_rune` branch accepts RuneItem definitions
without excluding this tag. Guild Conquest session settlement in
`src/api/community/conquest.rs` mails the table-selected GuildSuppressReward;
the rank 1/2 rewards 995318/995319 include Velkazar rune packages. Acquisition
was traced in code/data, not exercised through a complete live season.

Lucern-crafted runes use the internal PunishmentRune system: the server has
44 definitions and implements crafting, storage, equipping, preservation and
dismantling. The existing isolated test
`punishment_rune_crafting_equipping_preservation_and_break_are_atomic`
passed during this audit. The installed RuneHelper also handles their main
option stats and class-dependent extra skill descriptions. The restored Portal
source exposes Rune Crafting through `OpenPunishmentRuneCraft`. However, the
missing PunishmentRune category button prevents selecting these runes through
the Runehenge inventory dropdown shown by the user. Server support and client
stat code therefore do not amount to a complete usable equip flow.

A read-only query of account 1 found no Velkazar-tagged items in inventory and
no equipment with PunishmentRuneOption. This explains the absence of special
items, while the single dropdown entry has the separate hardcoded cause above.
No runes were granted and no client/server runtime files were changed by this
follow-up audit.
