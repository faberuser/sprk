# Damage formula

This is the hit-damage formula used by the installed SPRK client. It was taken
from the decompiled `Assembly-CSharp.dll` (SHA-256 `022752624c61…08797`) and
the client's `TableJit` tables. All battle maths is integer maths in
**permille** (1000 = 100%), and each step truncates toward zero. The formulas
below use percentages for readability. The exact integer form is given where
rounding or a curve matters.

Entry point: `NShared.StatController.GiveDamage` →
`GiveDamage_Season1`. Every damaging operation (`GetDamage`, `GetDamageR`, DoT
ticks and so on) works out a raw amount and then passes it through this
pipeline.

## Short version

For a normal hit that is not blocked and does not ignore DEF:

```text
Raw    = FinalATK × SkillRatio + SkillFlat
Damage = Raw
       × DefenceMultiplier(EffectiveDEF, levels)
       × (1 + Weakness − Tough)
       × [if crit] (2 + CritDMG + target's CritDMG taken)
       × (1 + SkillPower%)  + SkillPowerFlat           (not on normal attacks)
       × StackCount
       ↑ floored at Raw × 2%
       × (1 + DMG increase) × (1 − DMG Reduction)      (AttackPower / DefensePower)
       × Π (attacker modifiers: vs-boss, vs-race, next-normal-attack, …)
       × Π (target modifiers: DMG taken ±, by-state, from-boss, …)
       → shields / caps / escape-death
```

`Π` means a product: each separate effect multiplies on its own. Stacks of
**one** effect add together inside that effect's factor (`1 + value × stacks`).

## 1. Final stats

Every battle stat is computed by `GetFinalStat` in this order:

1. **Sheet stat.** This is the hero's base stat plus gear, soul weapon, runes and
   other equipment stats (`StatCalculator.GetEquippedHeroStat`).
2. **Pre-battle layers** (`InitExtraStats`). Each layer is applied in turn as
   `stat × (1 + Σ%) + Σflat`, in this order:
   1. extra stats (`CreateInfo.ExtraStats`)
   2. pet stats
   3. Ordeal Arena stats
   4. affix stats

   This is where the tweet's `(sheet ATK × %pet ATK) + flat pet ATK` comes from.
3. **Content scaling.** Some battle defines scale the base stat by
   `ModStatRatioM` (`GetBaseStat`).
4. **In-battle buffs and debuffs.** All active stat modifiers add into one
   percentage total and one flat total:

   ```text
   Final = Base × max(1 + Σ%, 0) + Σflat
   ```

   Buffs and debuffs are summed **before** they are applied. A +40% ATK buff
   and a −30% ATK debuff give +10%, not 1.4 × 0.7.
5. **Negative-percentage softening.** This applies to ATK, DEF and every stat
   that has an attenuation curve. If the summed percentage is below −50%, it is
   bent toward −100% and never reaches it:

   ```text
   m < −500‰:  m' = −1000 + 1 000 000 000 / (2m² − 1000m + 1 000 000)
   ```

   | Net % | −50% | −60% | −80% | −100% | −150% | −300% |
   |---|---|---|---|---|---|---|
   | Applied | −50% | −56.9% | −67.6% | −75% | −85.8% | −95.5% |

   This is the tweet's "%Shred above 50%" rule. It applies to the **net** DEF%
   (all shreds minus all DEF buffs), not to shred alone. ATK debuffs follow the
   same rule.
6. **Final-value curve** (`StatAttenuationTable`, see section 9). This applies
   to stats such as Crit, ACC, Dodge, Block, Crit Resistance and Tough. ATK and
   DEF have no final curve.

Flat DEF shred is a negative flat modifier, so the tweet's
`Target DEF × (1 − %Shred) − Flat Shred` is correct.

## 2. Raw amount

`GetDamageR` (the usual "deals X% ATK damage" operation):

```text
Raw = FinalATK × Ratio / 1000 + Flat          (P.ATK or M.ATK by skill type)
Ratio = BaseValue + LevelGrowth + Σ skill-modifier adds
LevelGrowth = Factor × SkillLevelFactor(level) × HeroStarFactor
```

Skill level ("books"), perks, transcendence and unique-weapon skill changes all
edit this **ratio additively** through `SkillModifierContainer`. They are not a
separate multiplier, unlike the `× Books` term in the tweet.

## 3. Hit and dodge

Hit and dodge happen before any damage maths. A dodged hit deals 0 damage.

```text
HitChance = 100% + ACC                  (ACC is ignored for DoT)
Chance    = min(HitChance, 100%) × (1 − Dodge) + max(HitChance − 100%, 0)
```

ACC above 0 adds directly on top of `1 − Dodge`. Blind and dodge-all states,
and `DodgeDamage` effects, cause an automatic miss. `Unavoidable` damage skips
this step.

## 4. DEF, penetration and the DEF multiplier

### Penetration

```text
PenPoints = max(basePen + flatPen + skillPen, 0) + TargetDEF × %Pen
Pen       = Curve(PenPoints)               (P./M. Pierce Power curve)
EffectiveDEF = TargetFinalDEF × (1 − Pen)
```

"%Pen" (a multiplicative modifier on Pierce Power) does **not** scale your pen.
It adds a share of the **target's DEF** as pen points before the curve is
applied.

Pen curve (`PhysicalPiercePower` / `MagicalPiercePower`), with `x` in points:

| Range | Pen |
|---|---|
| x ≤ 450 | x / 1000 |
| 450 < x ≤ 1000 | (0.409x + 266) / 1000 |
| x > 1000 | (900 − 900 000 000 / (2x² + 1000x + 1 000 000)) / 1000 |

| Points | 450 | 600 | 800 | 1000 | 1500 | 2000 | 3000 | 5000 |
|---|---|---|---|---|---|---|---|---|
| Pen | 45% | 51.1% | 59.3% | 67.5% | 77.2% | 81.9% | 86% | 88.4% |

The hard cap is 90%. The tweet's middle segment, `0.45 + Pen/100 × 22.5/550`,
leaves out the `− 450` offset and gives 85.9% at 1000 instead of 67.5%.

### DEF multiplier

The code is `RestoredPortal.LegacyPunishmentDamage`, using the constants
`DefenceCalcLevelBaseFactor = 1047294` and `DefenceCalcMinFraction = 20`:

```text
L = 1.047294 ^ (3 × min(atkLv,80) − 2 × min(defLv,80) − 1)      (fixed-point, ×10⁶)
K = L × 500
DefenceMultiplier = 2% + 98% × K / (K + EffectiveDEF)
                  = 1 − 0.98 × EffectiveDEF / (EffectiveDEF + K)
```

| Attacker vs target | K |
|---|---|
| Lv80 vs Lv80 | **19,248** |
| Lv80 vs Lv75 | 30,554 |
| Lv75 vs Lv80 | 9,624 |
| Lv80 vs Lv70 | 48,502 |
| Lv60 vs Lv60 | 7,639 |

The tweet's `0.98 × DEF / (DEF + 19205)` is the **reduction**, not the
multiplier. Its constant is also slightly off: 19,248 at Lv80 vs Lv80. The
constant also depends on both levels.

Precision: the multiplier is rounded down to 0.1% (`20 + 980000/val` is in
permille).

| Effective DEF (Lv80 vs 80) | 0 | 5,000 | 10,000 | 19,248 | 30,000 | 50,000 | 100,000 |
|---|---|---|---|---|---|---|---|
| Damage kept | 100% | 79.8% | 66.5% | 51% | 40.3% | 29.2% | 17.8% |

**Ignore-DEF hits** (`DamageFlag.IgnoreDefence`) set EffectiveDEF to 0. Later
they are multiplied by the attacker's **IgnoreDefensePower** (base 100%, plus
"DMG that ignores DEF +X%" effects), as shown in section 6.

## 5. Tough and Weakness

```text
× (1 + Weakness − min(Tough, 100%))
```

Weakness is "P./M. Weakness" (more damage taken). Tough is "P./M. Tough". They
are **added together in one factor**. Tough goes through the curve first
(linear to 45%, cap 90%). Damage flags can ignore either of them.

## 6. Crit, skill power, stacks and the 2% floor

**Crit chance:**

```text
Chance = clamp(Curve(Crit + skillCrit) − Curve(CritResistance), 0, 100%)
```

- The Crit curve is linear to 150%, then 150% + 0.5 × (x − 150%) up to 175% at
  200%, then approaches a cap of 200%. Crit above 100% only helps against Crit
  Resistance.
- The Crit Resistance curve is linear to 50% and reaches 75% at 100%. The cap
  is 100%.

**Crit damage:**

```text
× (2 + CritDMG + skillCritDMG + target's CritDMG taken)
```

`PhysicalCriticalDamageFactor` is 2000, so the base crit multiplier is 200%.
"CritDMG taken" is `CriticalWeakness` on the target. This matches the tweet's
`(2 + CDMG)`.

**Skill power:**

```text
× (1 + SkillPower% + skill-specific%) + SkillPowerFlat
```

The `SkillPower` stat is skipped for normal attacks (`IsBaseAttack`).
Skill-specific `SkillPowerR` and `SkillPower` modifiers still apply to them.

**Stacks:** `× StackCount` (when it is above 1).

**Floor:** the damage at this point is raised to at least `Raw × 2%`, using
`MinAttackDamageFactor = 20`. Block and the later multipliers can still push it
lower.

## 7. Block, special hit types and DMG increase or reduction

**Block.** The block chance is the target's Block after its curve: linear to
50%, 75% at 100%. Block-all states make it 100%.

```text
blocked: × (1 − clamp(50% + Curve(Block DEF), 0, 100%))
```

Base block reduction is 50% (`DefaultBlockPower = 500`). The Block DEF curve
is linear to 22.5% and caps at 45%, so a block reduces damage by 50% to 95%.

**Special hit types.** Base 100% for all heroes, with additive bonuses:

- **Ignore-block hits** cannot be blocked. Instead they are
  `× IgnoreBlockPower`.
- **Ignore-DEF hits** are `× IgnoreDefensePower`.
- **DoT ticks** are `× DotPower`.

**DMG increase and reduction:**

```text
× (1 + AttackPower) × (1 − DefensePower)
```

`AttackPower` is the attacker's P./M. damage increase. `DefensePower` is the
target's "P./M. DMG Reduction", which can be ignored with
`IgnoreDefensePower` flags. Neither has a curve in the current tables. Note
that `DefenceMaxReducePower = 900` exists as a constant but is **not used** by
this client.

Two more constants exist but have no effect in the current data:

- `WeakAttackAttrDamageFactor` is 1000, so the element/type weakness factor is
  ×1.
- `LevelDamageAttenuationFactor` is 1000, so there is no extra level penalty
  beyond K.

## 8. Effect modifiers (boss damage, race damage, etc.)

There are two lists of effect modifiers. Each one is applied **in the order it
was added**, and each multiplies the running damage separately:

1. **Attacker** `IAttackModifier`s:
   - `ModAttackByBoss`: DMG to bosses or non-bosses.
   - `ModAttackByRace`: DMG to a race.
   - `ModNextBaseAttackR`: next normal attack(s).
   - `ModAttackAmount`: random roll table.

   Each one applies `× (1 + value × stacks) + flat`.
2. **Target** damage modifiers, in pipeline order:
   - `Beginning` and `AfterDodge`: before the maths above.
   - `Final`:
     - `ModDamage`: DMG taken ±, `× (1 + v)`.
     - `ModDamageByState`: `× (1 + v × state stacks)`.
     - `ModAttackByBoss` / `ModAttackByRace`, DMG-taken side.
     - `SetDamage`.
     - `ReflectDamage`.
     - **Shields**: `HpShield`, `CreateShield*`.
   - `AfterFinal`: `IgnoreDamageByMaxHpR`, `IgnoreDamageUnderHpR` (caps).
   - `EndOfFinal`: `EscapeDeath` and similar effects.

   Shields share the `Final` list with DMG-taken effects. Whether a DMG-taken
   effect is counted before or after a shield absorbs damage therefore depends
   on which was applied first.

So two separate "+30% DMG to bosses" effects give 1.3 × 1.3. Two stacks of
**one** such effect give 1 + 0.3 × 2. The tweet's `(boss1 × boss2 × …)` is
right. Its additive groups only hold within one effect.

## 9. Curve reference (`StatAttenuationTable`)

All curves are piecewise:

- above `X1`: `MaxK − MaxK·10⁶/(A1x² + B1x + 10⁶)`
- between `X2` and `X1`: `A2·x/1000 + B2`
- below `X3` and `X4`: a matching negative-side curve
- otherwise: linear (unchanged)

| Stat (P. and M.) | Linear up to | Value at upper knee | Cap |
|---|---|---|---|
| Crit, ACC | 150% | 175% at 200% | 200% |
| Dodge, Block, Crit Resistance | 50% | 75% at 100% | 100% |
| Block DEF | 22.5% | 33.7% at 77.5% | 45% |
| Pen (Pierce Power), Tough | 45% | 67.5% at 100% | 90% |
| Anti-debuff chance | 45% | 67.5% at 100% | 90% |
| Debuff/CC duration, Lifesteal | 50% | 75% at 100% | 100% |
| Hit-CC chance | 90% | — | 90% |
| Attack speed | 160% (linear above 50%) | 200% at 240% | 250% |

`AddedStatAttenuationTable` is empty. `MultedStatAttenuationTable` only bends
`MpOnAttack`.

## 10. Worked example

The setup:

- Lv80 attacker vs Lv80 boss.
- Physical skill: 300% ATK + 5,000. ATK 100,000.
- Pen 600 points.
- Boss DEF 30,000.
- The hit crits, with +100% Crit DMG.
- No Tough, Weakness or DMG Reduction.
- One "+30% DMG to bosses" effect.

| Step | Value |
|---|---|
| Raw = 100,000 × 3.0 + 5,000 | 305,000 |
| Pen 600 → 51.1% → effective DEF = 30,000 × 0.489 | 14,670 |
| DEF multiplier = 2% + 98% × 19,248/(19,248+14,670) → 57.6% | 175,680 |
| Crit × (2 + 1.0) | 527,040 |
| Boss × 1.3 | **685,152** |

## Sources

All paths are in the decompiled `Assembly-CSharp`:

- Pipeline: `NShared/StatController.cs`. See `GiveDamage_Season1`,
  `GetFinalStat`, `MultAndAddStat`, `AttenuateStatUnderMinX` and
  `GetAttackAmountByPower`.
- DEF curve: `RestoredPortal.LegacyPunishmentDamage`, together with
  `NShared.NUtil/ExponentialTable.cs`.
- Curves: `NShared/StatAttenuationData.cs` with `TableJit/StatAttenuationTable.jit`.
- Constants: `NShared/BattleConstant.cs` with `TableJit/ConstantTable.jit`.
- Raw amount: `NShared.NOperationRuntime/GetDamageR.cs`,
  `NShared/OperationData.cs` (`GetOperationValueInt`) and
  `NShared/OperationInfo.cs` (`RecalculateValue`).
- Modifiers: the `ModAttackByBoss`, `ModAttackByRace`, `ModAttackAmount`,
  `ModNextBaseAttackR`, `ModDamage` and `ModDamageByState` classes in
  `NShared.NOperationRuntime/`.
- Pre-battle layers: `StatController.InitExtraStats` and
  `StatCalculator.ApplyExtraStat`.

If the DLL or tables change (for example a patch to `RestoredPortal` or
`ConstantTable`), re-check the K constant, the pen and Tough curves and the
constants above.
