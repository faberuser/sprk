# Divine Punishment combat audit

Audited 2026-10-18 against the supplied pre-Doomsday build-1558 archive at
`Z:/media/kingsraid/Pre-Doomsday`. This was a read-only audit. No installed client,
server combat settings, account data or reward formulas were changed.

The checked base stats, scaling tables, skills and operations match after
resolving duplicate archive keys with the original loader's first-record-wins
policy. Skill **441399** already matches the record selected by that loader.
The earlier claim that it needed an ally-count-based toughness restoration was
an audit error: it selected a later duplicate that the original loader ignores.

## Scope and evidence

The audit covers both Karma routes, Apocalypsion, Solenis, all eight raid nodes,
side bosses, summoned creatures and the related story encounters:

- 59 encounter waves across chapters 70000, 70001, 70101â€“70104, 70201â€“70204,
  30050 and 30051; 31 dungeon definitions.
- 49 creatures including summons, 31 referenced tier rows, 490 star/grade rows,
  600 level-factor rows and 200 skill-level-factor rows.
- 439 skills, 1,100 nonempty archived operations, 139 referenced states,
  seven projectiles and 76 relevant affixes, including shards and triggers.
- All 27 Infinity curves, Karma stage/power-up/unlock definitions, relevant
  battle definitions/constants, shared attenuation and related-stat tables.

Compared **126,424 fields**. Native data was decoded with the installed client's
own JIT containers, including the operation dictionary and star-stat lookup
dictionary. This checks effective installed tables rather than assuming that
patch source or an old decoded export is still in use.

Verified 52 archived named exports against the copy manifest. Their MPB inputs
were also checked against validation hashes and the original Android backup.
Installed native table integrity manifests were verified. Skill chunks and
Campaign/MonsterTier chunks were read together, rather than treating the first
file as the complete table.

Evidence is under `target/divine-punishment-combat-audit/`:

- `comparison.json`: field comparisons and individual differences.
- `verification.json`: provenance, source and installed hashes, coverage.
- `field-differences.csv`: field differences (none after correct duplicate resolution).
- `encounter-base-stats.csv`: 243 encounter/creature stat snapshots.
- `scope.py`, `compare.py`, `stats.py`, `verify.py`, and `native-tool/`:
  the read-only audit utilities.
- `native/`, installed-method decompilations and `combat-checks.log`:
  the decoded data and formula/check evidence.

## Results

| Area | Result |
|---|---|
| Creature definitions and level-stat bases | Match for all 49 creatures |
| Encounter levels, stars, skills, tiers and spawning slots | Match after resolving native inheritance |
| Core star/grade stats | Match for 490 rows within the identified historical layout |
| Referenced monster tiers and level factors | Match |
| Karma stage loops and four enemy power-up skills | Match |
| Apocalypsion/Solenis Infinity curves and relevant affixes | Match |
| Relevant skill/state/projectile definitions | Match, including skill 441399 and its operations |
| Shared attenuation tables and relevant combat constants | Match |

Boss/creature detail lists were compared using their original wave-slot indices.
An archived per-creature star/level/tier/skill value of zero inherits the wave
default; the JIT stores the resolved value. These representation changes are not
reported as balance differences. Null/empty string interning and float32 export
rounding were normalized as well.

## Corrected duplicate resolution: Poison Dragon skill 441399

Creature 60005 (`Raid.Punishment.Poison_Punishment`) includes this extra skill.
The original `SkillTable8.json` contains two consecutive records with this key,
at zero-based positions 74 and 75. Both records are in the same archived MPB;
they are not evidence of an installed later-client replacement.

The original MessagePack table loader calls OnAddData, which returns when its
key dictionary already contains the key. It therefore selects the first record.
The earlier audit used a Python dictionary comprehension that selected the last
record instead. The audit now preserves the first record, reads numbered chunks
in numeric order and records duplicate keys explicitly.

| Field | First record, selected by original loader and installed client | Later duplicate, ignored by original loader |
|---|---|---|
| Trigger | OnTimer, 15000/15000 | OnStart |
| TriggerDisabled | true | false |
| IgnoreStateType | CC, RaidDown | none |
| IgnoreBlock / ViewTargetWhileUse | true / true | false / false |
| Operation 1 | ModSkillPowerR | ModToughness |
| Operation 1 values | skill 441300, operation 1, value 300 | `$AddByAllyCount(100)` |
| RecalculateValueByStats | none | AllyCount (93) |
| Operation 2 | ModSkillPowerR for skill 441301, operation 1, value 300 | absent |

All runtime skill fields and both operations match the selected first record.
No gameplay correction is required. Replacing this with the toughness duplicate
would change the behavior selected by the supplied pre-Doomsday client.

Native loader provenance is recorded in RestoredCombat.json's
OriginalDuplicateLoadPolicy: integer-key OnAddData RVA 0x29e39b1, object-key RVA
0x29e6844, native binary SHA-256
`a0f9256b2eba78eea1972f7358e299e0739e7dc4a936e3d33bc66f1bca8c3bb3`.
That hash was checked against the supplied September Android executable.
The duplicate-resolution evidence is retained at
`target/poison-dragon-skill/duplicate-resolution.json` and the original mistaken
comparison is preserved under `target/poison-dragon-skill/original-audit/`.

## Base-stat calculation and examples

The installed stat path uses the following order:

1. Scale each level-stat base by its selected CreatureLevelFactor value at the
   requested level divided by that value at level 1, with integer truncation.
2. Apply the star/grade level factor or per-stat CreatureStarStat override, then
   add the flat star/grade stat. Monster creation excludes physical/magical
   defense from this level-factor multiplication.
3. Apply the monster tier and spawn stat ratio, each in per-mille units, using
   decimal arithmetic and `Convert.ToInt64` rounding.
4. Apply combat modifiers, passives, affixes and phase mechanics in battle.

Examples from the native wave templates at level 100, one star, ordinary spawn
ratio, **before Infinity affixes, Karma growth, passives and phase changes**:

| Creature | HP | Physical ATK | Magical ATK | P.DEF | M.DEF |
|---|---:|---:|---:|---:|---:|
| Apocalypsion (60002) | 1,055,632,424,239 | 28,203,032 | 28,203,032 | 69,998 | 69,998 |
| Solenis (60009) | 2,386,339,400,000 | 55,678,000 | 55,678,000 | 78,085 | 78,085 |
| Karma Infira (61005), first stage | 509,280,000 | 40,740 | 40,740 | 34,629 | 34,629 |
| Karma KibehOd (61010), first stage | 628,112,000 | 36,860 | 31,040 | 39,576 | 39,576 |

These are reproducible table-derived snapshots, not assertions that the battle
HUD must display these values after modifiers. The CSV supplies the remaining
encounter snapshots.

## Infinity scaling

For step S, native GetSumValue returns zero when S <= 0. Otherwise it adds the
first Sâˆ’1 increments to StartValue, repeats the final increment beyond the
array, and multiplies by Multiplier (zero means one).

GetInfinityStep uses the selected raid level when InfinityStartLevel is zero;
otherwise it uses `raid level - InfinityStartLevel + 1`.

Both boss groups share matching attack curves (10011/20011, start level 2) and
HP curves (10012/20012, start level 6). Their affixes use ModPowerR and ModMaxHpR.
The additions below are per-mille values, before combining other modifiers:

| Raid level | ATK addition | HP addition |
|---:|---:|---:|
| 1 | 0 | 0 |
| 2 | 100 | 0 |
| 5 | 400 | 0 |
| 6 | 500 | 200 |
| 10 | 700 | 600 |
| 50 | 4600 | 4600 |
| 100 | 9600 | 9600 |

For example, +100 means +10%, not Ã—100. Side-clear trigger affixes, selected
shards and the other battle modifiers still apply separately. The server uses
RaidLevel 1 as the template and passes the selected native Level for Infinity.

## Karma enemy scaling

Enemy combat growth is separate from the custom Karma reward formula. Its
archived data remains intact. Let N be native Snapshot.MaxStageLevel, and q be
the operation's interval parameter:

- ModPowerRByStageLevel uses repeated fixed-point multiplication of
  `(1000 + value)` for `floor(N/q)` steps, subtracts 1000, and clamps an individual
  result to int.MaxValue. It is approximately
  `1000 * ((1 + value/1000)^floor(N/q) - 1)`, with integer rounding at every step.
- ModDefenceRByStageLevel and ModMaxHpRByStageLevel return
  `value * floor(N/q)`.
- Matching stat modifiers add their per-mille results. They are not multiplied
  together as separate full-stat multipliers.

| Route | Every-stage ATK parameter | Every-stage DEF addition | Every-stage HP addition |
|---|---:|---:|---:|
| Apocalypsion Karma | 430, interval 1 | 10 | 580 |
| Solenis Karma | 410, interval 1 | 10 | 600 |

Both have the archived interval-35 power/DEF/HP operations with value 10000,
the interval-30 and interval-31 operations with value 9000, and the additional
ATK operation with value 150 at interval 5. These large growth steps are present
in the source data. No gentler replacement enemy-growth curve was introduced.

## Defense and damage formula

Both installed CalculateDamage entry points use RestoredPortal's legacy
calculator. For attack A, defense D, attacker level LA and defender level LD:

```text
a = min(LA, 80)
d = min(LD, 80)
R = fixed-point exponential ratio(1047294, 3*a - 2*d - 1), unit 1000000
divisor = floor(2*D*1000000 / max(R,1)) + 1000
damage = floor(A * (20 + floor(980000 / max(divisor,1))) / 1000)
```

The archived constants match: defense growth factor 1047294, minimum fraction
20, change factor 2, and attenuation level cap 80. The level cap applies to this
calculator's inputs; encounter creatures remain level 100 with level-100 stats.

Executed the installed method directly with A = 1,000,000 and both levels 100:

| Defense | Damage after this calculator |
|---:|---:|
| 0 | 1,000,000 |
| 10,000 | 665,000 |
| 50,000 | 292,000 |
| 100,000 | 178,000 |
| 500,000 | 56,000 |
| 1,000,000 | 38,000 |

This is the defense step, not the complete final damage formula. Crit, damage
flags, toughness, boss/race modifiers, shields, HP gates and phase effects have
their own processing. The existing native self-checks passed 32 reducer cases,
34 legacy pipeline/mechanic cases and 23 Technomagic/shared mechanic cases;
18 direct defense-calculator samples were also executed.

## Verification limits

Matching tables do not prove every historical executable formula. The current
legacy calculator and Karma implementations were inspected, and their constants
and operation parameters match the supplied archive. This audit did not recover
and execute every corresponding function from the original Android binary, so
it does not claim complete binary-level formula equivalence.

CreatureStarGradeStat's original flat export has 203 positions. The first 83
identified positions establish the core stats and creature/star/factor keys;
those match. Unidentified trailing positions, including unestablished threat
mapping, are left explicitly unverified. They are not guessed or reported as
confirmed mismatches. Wave ArchiveCreatureIndex fields are archive/UI metadata;
skill Name/Description fields are absent from the runtime skill object. Flattened
operation fields were compared through the separate native operation dictionary.

No signed client update was generated because no installed assets were changed.
