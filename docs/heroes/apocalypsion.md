# Apocalypsion: making the raid boss a playable hero

Goal: turn **Apocalypsion in her humanoid form** (the second boss of the
Apocalypsion raid, beside the dragon Tersio) into a playable hero balanced like
the released ones. This page is the plan and, later, the implementation
record. It follows the Maya work: [maya.md](maya.md) (process and checklist),
[maya-art.md](maya-art.md) (art stand-ins), [jit-tables.md](../jit-tables.md)
(table patches).

Status: **planning**. Nothing has been implemented yet.

## Source data

| What | Value |
| --- | --- |
| Name / title | `CREATURE_NAME_60002` "Apocalypsion", `CREATURE_DESC_60002` "Calamity of Destruction" |
| Creature rows | 37004 `Monster.Story.Apocalypsion` (story), 60002 `Raid.Punishment.Apocalypsion` (raid) |
| Model | `Boss.Apocalypsion` (humanoid). Tersio is the separate dragon, `Boss.Tersio` (rows 37000/60001) |
| Size | `SizeMm` 3000, `HeightMm` 4500 (Maya: 500 / 1500) |
| Boss traits | Race 36 (Dragon), `IgnoreCc` CC, `BlockDash`, `PositionWeight` 10000, `TransformType` 5 |
| Visual | `CreatureVisualTable` `PunishmentRaidApocalypsion` / `StroyApocalypsion` (hidden HP bar, boss intro). No `ClientCreatureTable` row |
| Profile | `CREATURE_STORY_60002` (her fall: Lea's incarnation, the experiments on her descendants) |

Indices come from the decoded tables in `../kingsraid-table-data` and
`../Pre-Doomsday`. Confirm every row against a dump of the pinned baseline
before patching.

### Boss kit (raid 60002, `SkillTable16`)

| Skill | Name | Effect (English text) |
| --- | --- | --- |
| 6000201 | Audience With the Matriarch | On auto attack with no enemy nearby: dispels 1 enemy's buffs and pulls it in front of her |
| 6000210 (22 s) | Engraving Aura | 3 frontal M.DMG hits, then the sword aura explodes for M.DMG and Stun; heals per enemy hit; DMG grows each use |
| 6000220 (25 s) | Shaking Earth | Line M.DMG, destroys Shields; each hit burns Mana and Stuns; more DMG to CC'd targets |
| 6000230 (33 s) | Invitation to the Abyss | All-enemy M.DMG, Stun and [Abyssal Curse]; gains [Judgment] and charges; the release knocks back, or pulls cursed targets for more DMG. Crits remove [Judgment] |
| 6000241 | Destructive Demise | Fatal DMG triggers [Power of Hatred]: meteors, then a wipe meteor if not depleted |
| 6000250 | Wrath of the Matriarch | Stages over time: % Max HP regen and DMG reduction → +DMG → ignores DEF |
| 6000260 | Unblemished Form | Immune to ATK Spd/DEF/ATK changes; always hits; +Crit |
| 6000151 | Hauled Into Hell | Periodically removes the highest-M.DEF enemy from battle and stacks [Infernal Curse] |

## Asset inventory

Bundles in `King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle`:

| Bundle | Contents |
| --- | --- |
| `Creature_Boss_Apocalypsion.unity3d` | Model |
| `Creature_Boss_Apocalypsion_Animation.unity3d` | 19 clips (below) |
| `Effect_Skill_Boss_Apocalypsion_*` | Skill effects: Attack1/2, Skill1-6, Advent, RuneSkill A/C |
| `Effect_BattleFieldMap_WorldBoss_ApocalypsionField_1-3` | Raid arena effects (not needed) |
| `Sound_VOICE_kr_Raid_Apocalypsion.unity3d` | Raid voice (Korean) |
| `Sound_BGM_Raid_Apocalypsion_*` | BGM (not needed) |

### Animation clips compared with a hero

| Use | Apocalypsion | Hero (Maya) |
| --- | --- | --- |
| Auto attack | `Attack1`, `Attack2` | `Attack1` |
| Skills | `Skill1`, `Skill2`, `Skill3-1/2/3`, `Skill5`, `Skill6-1/2/3`, `Advent` | `Skill1`-`Skill3` |
| Combat reactions | `Damage`, `Stun`, `Down`, `Getup`, `Dead`, `Cancel` | same set |
| UI | `IdleUI` | `Get`, `Victory`, `Lose`, `Looting`, `Purify`, `PurifyReaction`, `Transcendence`, `InnIdle`, `InnReaction` (facial bundle) |

Combat clips are complete. Presentation clips are missing. Neither bundle has
idle or run clips, so heroes get them elsewhere; find where before the spike.

### Missing compared with a hero

- Presentation clips (above) and a `_Facial` bundle
- `ClientCreatureTable` row (hero view, inn, shop, pick/ban, result framing)
- Illustration, portrait, icons, skill icons, perk icons
- Hero voice (only Korean raid lines)
- `GetHero` / transcendence effects and node trees, soul weapon model, unique item art

## Approach

Hero row with `ModelCode: Boss.Apocalypsion`, hero-sized collision and hero
traits. Missing presentation goes through fallbacks and stand-ins, as for Maya.
New skill rows reuse her boss clips and effects but use hero-level numbers.
The boss rows are not reused, because they are tuned against boss stats.

## Plan

### Phase 0: spike (decides go / no-go)

- [ ] Find where heroes get idle and run clips; check whether `Boss.Apocalypsion` can use them or has its own
- [ ] Confirm a free hero index in the pinned baseline (103 is free in the Pre-Doomsday table) and the matching skill, perk, unique weapon/treasure and soul weapon ID ranges
- [ ] Throwaway hero row: `ModelCode: Boss.Apocalypsion`, hero `SizeMm`/`HeightMm`/`PositionType`/`PositionWeight`, `IgnoreCc` `False`, `BlockDash` false, `TransformType` as Maya
- [ ] Three placeholder skills on `Skill1`, `Skill2`, `Skill6` (or `Skill5`); auto attack on `Attack1`
- [ ] `combat-simulation` run, then a real battle in the test copy. Check: she idles, moves, attacks, takes CC, dies; skill effects play from the hero side; her size beside four heroes
- [ ] Record results here and decide

### Phase 1: engine and presentation

- [ ] Model scale override for her creature code (C#, new `Sprk*` assembly or an existing one) if 4.5 m is too large
- [ ] Clip fallbacks: `Get`/`Victory` → `IdleUI` or `Advent`; `Lose` → `Damage`; inn → `IdleUI`
- [ ] Node-tree fallbacks (`SprkNodeTreeFallbacks`) for `GetNewHero`, `Transcendence`, `GetSoulWeapon`, reinforce
- [ ] `ClientCreatureTable` row tuned on every screen (hero list, info zoom, inn, shop, pick/ban, result)
- [ ] Art stand-ins through `SprkAtlasAliases` (raid boss portrait if a sprite exists, otherwise renders)
- [ ] Voice: map raid lines to skill/attack/death cues

### Phase 2: kit and balance

- [ ] Choose class and peer hero (below)
- [ ] Base stats: copy the peer's `CreatureStarGradeStatTable` / `CreatureLevelStatTable2` rows
- [ ] Base kit S1-S4 with books I-IV
- [ ] Perks: shared class T1/T2; new T3 (Light/Dark per skill) and T5
- [ ] Unique weapon, 4 unique treasures, soul weapon (+ soulstone item, prices, display)
- [ ] Combat simulation against the peer (dragon raid, Ordeal Tower, World Boss)

### Phase 3: obtaining and text

- [ ] `OpenType` Opened (client and server `tables/HeroShopSupport.json`)
- [ ] Inn rows and dialogue; hero/soulstone/unique selectors (client and server); Special Shop
- [ ] Default costumes (Hero Index profile and avatar need them)
- [ ] English text, then the 7 other languages
- [ ] Server tests, `build_tables.py --check`, local release

## Balance approach

1. **Class and peer.** She deals M.DMG. Candidates: Wizard (ranged nuker) or a
   magic frontliner. Pick one released hero of that class and role as the
   baseline for stats, mana costs (3/3/4 orbs typical), cooldowns and ratios.
2. **Remove boss traits.** No CC immunity, no % Max HP regen, no flat DMG
   reduction, no buff immunity, no "always hits", no wipe meteor, no
   remove-from-battle.
3. **Ratios in the released format** (`[ATK‰, flat]`), inside the peer's
   envelope. Lifesteal, Mana burn and Shield break are costed against the peer's
   utility.
4. **Verify** with `combat-simulation` (`chapter;dungeon;difficulty;heroes`,
   `;soul` for soul weapons): her DPS and survival within the peer's range in
   the same team and content.

### Draft kit mapping (to refine in a design pass)

| Slot | From | Hero version |
| --- | --- | --- |
| Auto attack | `Attack1`, 6000201 | Normal M.DMG; optionally dispels 1 buff on a cooldown |
| S1 | Engraving Aura (`Skill1`) | 3 frontal hits + explosion with short Stun; heals a % of DMG dealt |
| S2 | Shaking Earth (`Skill2`) | Line M.DMG, destroys Shields, small Mana burn; +DMG to CC'd targets |
| S3 | Invitation to the Abyss (`Skill6` or `Skill3`) | AoE M.DMG + [Abyssal Curse]; follow-up hit deals more to cursed targets |
| S4 (passive) | Wrath of the Matriarch | Stacks over time: +M.DMG, later partial DEF ignore; capped, resets per wave or on death |
| T5 / perks | Unblemished Form | +Crit / Accuracy, not immunities |
| Soul weapon | Destructive Demise / meteors | Meteor shower M.DMG over a few sec |

## Open questions

- Class (Wizard or other) and peer hero
- Final on-field scale
- Whether to keep Race 36 (Dragon) on the hero row: race affects DMG-by-race effects, so check the effects that target Dragons
- Art: is there a usable boss portrait or illustration sprite?
- Voice: Korean raid lines only, or silent in other languages

## Risks

- Scaling a boss model can break effect anchors, camera framing and hit positions
- Boss skill effects may be sized for a 4.5 m caster
- Boss node trees and AI scripts may assume raid-only systems (charge, [Judgment], phase changes); hero skills must not call them
- Missing presentation clips make several UI scenes static until fallbacks exist
- `CostumeTable` cannot be rebuilt byte-identically (see [maya.md](maya.md)); default costume rows need that fixed or another route
