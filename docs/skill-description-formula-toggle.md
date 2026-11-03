# Skill card calculated/formula toggle

Status: implementation proposal; no client changes implemented.
Date: 2026-11-03.

## Goal and scope

Add a dedicated button to the **top-left of the hero skill card's title bar**,
mirroring the circular-arrow linked-skill button at the top-right. The new button
switches the description between its current calculated values and readable
formulas derived from the actual SPRK client data. The button style's should match
with the current game's styling.

Preserve combat behavior, skill balance, server requests, and the native
calculated-description path. This feature adds UI behavior and an alternate
description renderer; replacing localization strings alone is insufficient.

The CBT phase 2 English hero descriptions in
`D:/kr/krinfo/public/kingsraid-data/table-data/cbt-phase-2/en/heroes` are a formatting
reference only. Do not import their coefficients, effects, awakening multipliers,
or complete descriptions as SPRK balance data.

Initial scope is the hero skill detail card, including active skills, passives,
and linked/transformed variants opened through that card. Battle hover tooltips,
soul-weapon cards, item descriptions, and transcendence perk popups are separate
surfaces and are outside this implementation unless explicitly added later.

## Player interaction

```text
+-------------------------------------------------------------+
| [fx / 123]           Skill name  Lv 100                 [arrows] |
+-------------------------------------------------------------+
| Skill icon             Mana                                 |
|                        Cooldown                             |
|                                                             |
| Description in the selected display mode                     |
+-------------------------------------------------------------+
```

This sketch specifies placement, not final pixel dimensions.

- Default to **Calculated** when no saved preference exists.
- One click switches to **Formula**; the next returns to **Calculated**.
- Use a compact `fx` label for formula mode and `123` for calculated mode, with
  native button styling. The label indicates the current mode. Hover/focus help
  states the action: "Show formulas" or "Show calculated values".
- Match the existing right button's vertical center, outer margin, visual size,
  and usable hit area. Mirror its placement, not its linked-skill action.
- Reserve title space between both controls, including for long localized names
  and skill levels. Do not overlap the title, skill icon, or description.
- Keep the formula toggle visible for skills without linked variants, even when
  the right button is hidden.
- Switching display mode updates the description and its scroll bounds, starting
  at the top. It does not switch skill variants or reopen the card.
- The circular-arrow button continues switching linked skills. Keep the selected
  display mode and render the newly selected variant's own description.
- Keep the mode across skill/hero changes and card reopen. Proposed persistence:
  one local client display preference, saved through the existing preference
  mechanism, without an account/server field. Validate this choice during UI work.
- A skill with no scalable quantities may show identical descriptions in both
  modes. This is valid and must not add artificial formulas.

## Formula presentation

Preserve the current localized prose, colors, effect names, durations, target
counts, and condition wording. Replace supported calculated quantities with their
symbolic expressions at the description's argument/expression level.

Illustrative format:

```text
Deals Base × Scaling(Skill Level, Awakening) + Coefficient% of ATK M.DMG.
```

The CBT example
`299 × (0.7 + 0.3 × Skill Level × Awakening Coefficient) + 189.1% of ATK`
is not an approved SPRK formula. Use that structure only where the installed
SPRK expression actually matches it. Other expressions must retain their own
scaling, terms, parentheses, caps, and conditions.

Display rules:

- Support damage, healing, shields, and scalable stat changes, rather than only
  expressions labeled as damage.
- Distinguish caster ATK/HP/DEF from target stats, maximum HP, and missing HP.
- Explain symbolic variables once beneath the description when needed. Show the
  current effective skill level and relevant awakening value; derive any
  coefficient legend from SPRK's actual lookup data.
- Include equipment-added skill levels exactly once, matching the native path.
- Preserve per-hit versus total damage and DoT tick versus duration semantics.
  Do not silently multiply or divide a displayed value to make it look simpler.
- Keep linked-skill, costume-specific, enhancement, and conditional branches
  aligned with the currently displayed skill and native description context.
- Describe relevant skill-book/perk modifiers explicitly where the original
  description includes them. Do not imply the card predicts post-mitigation
  battle damage or every possible temporary combat buff.
- Use readable operators and parentheses without changing expression order or
  early rounding. Confirm font support for `×`; use `*` if required.
- Localize the new labels and variable names. Format decimals consistently with
  the existing locale, without mixing translated prose with unexplained tokens.

For an unsupported expression, retain that quantity's native calculated value
and append a short localized note that some values remain calculated. If the
whole alternate description fails, show the complete native description with
"Formula view unavailable for this skill." Never show guessed coefficients,
raw node names, empty text, or a stale formula from another skill.

Partial coverage is acceptable during development. A roster-wide completion
claim requires an explicit coverage audit; fallback alone does not complete it.

## Existing code landmarks and required confirmation

Reference source inspected under
`../kingsraid-table-data/dlls/Assembly-CSharp/Assembly-CSharp/`:

| Reference type                           | Relevant behavior                                                                                |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------ |
| `NGame2.NUI.NWindow.HeroSkillDesc`       | Owns the card, `Label_SkillDesc`, `ScrollView_SkillDesc`, and `Button_LinkedSkill`.              |
| `HeroSkillDesc.Refresh`                  | Resolves the current skill with `Helper.GetSkillDetailInfo`, then sets description and controls. |
| `HeroSkillDesc.SetupCurSkillLevelDesc`   | Calls `SkillDescContainer.GetByHeroInfo`, assigns the description, and resets scrolling.         |
| `HeroSkillDesc.OnClickChangeLinkedSkill` | Changes `_linkedIndex` and refreshes the card.                                                   |
| `SkillDescContainer.GetDesc`             | Selects costume-specific description data, then the default skill description.                   |
| `SkillDesc.GetDescription`               | Evaluates a string argument tree using `SkillDescVariable`.                                      |
| `SkillDescVariable.Open`                 | Supplies skill/hero context, stats, and equipment-added skill levels.                            |

The reference card prefab path is
`Resources/Prefab2/Window/Hero/HeroSkillDesc`; the reference description data path
is `TableData/SkillDescription.json`.

These are discovery leads, not verification of the currently installed patched
DLL. Before implementation, hash the installed source assemblies, inspect these
specific methods with the repository tooling or a decompiler, and inspect the
loaded native prefab. Confirm actual signatures, layout/anchors, event bindings,
description data, expression node types, and any existing SPRK overrides. Keep
generated inspection output outside tracked source and associate it with DLL
hashes. Avoid relying on stale decompilation or reference table exports.

## Implementation design

1. **Trace and inventory expressions.** Start with the supplied screenshot's
   skill, "A Present For You!", including its linked follow-up and DoT quantity.
   Resolve its actual SPRK skill IDs rather than inferring them from the name.
   Inspect one simpler hero's four skills as a second sample. Trace displayed
   arguments to native description expressions and the corresponding gameplay
   data. Record supported operations and discrepancies before generalizing.
2. **Build a separate symbolic renderer.** Prefer traversing the existing
   description expression tree and formatting numeric arguments symbolically,
   while evaluating text selection/conditional branches in the current native
   hero context. Reuse localized templates. Do not globally replace the numeric
   evaluator, change shared pooled variables, parse final damage strings, or
   reverse-engineer coefficients from a single calculated number.
3. **Define supported operations.** Cover constants, stat references, level and
   awakening scaling, arithmetic, percent conversion, rounding, min/max, and
   conditional/lookup expressions found in the inventory. Preserve precedence
   and native evaluation semantics. For opaque operations, use the documented
   fallback. If generic traversal is insufficient, keep reviewed mappings
   keyed by actual skill/expression IDs with source provenance and validation;
   never maintain an unverified duplicate balance table.
4. **Add the card control.** Implement a small helper attached to the card and
   inject focused hooks at confirmed setup/refresh points. Reuse native button
   styling or clone only the visual shell of a suitable control. Clear inherited
   click handlers, animation targets, and linked-skill bindings before wiring the
   toggle. Anchor it to the title bar independently of the right button's active
   state. Ensure repeated refreshes and pooled reopen create exactly one control
   with one callback; release subscriptions on destruction.
5. **Select the description locally.** In calculated mode, call the existing
   native description path unchanged. In formula mode, pass the same resolved
   skill, hero, costume, and effective level context to the alternate renderer.
   Hook the card rather than globally changing `SkillDescContainer` callers.
   Refresh only the needed UI on toggle; do not trigger skill learning, equipment
   changes, requests, or level-up effects. Refresh formula content after native
   hero/stat changes. Avoid cached content surviving a skill/context change.
6. **Handle layout and localization.** Recalculate label size, scroll bounds,
   and clipping after mode or linked-skill changes. Keep a readable font size and
   allow scrolling for longer formulas. Put UI labels in the localization system;
   preserve all other entries and update integrity metadata if assets change.
7. **Integrate the reproducible build.** Proposed new inputs are
   `scripts/DllPatcher/Patches/SkillDescriptionFormula.cs` and
   `scripts/DllPatcher/Resources/SkillDescriptionFormula.cs.txt`, plus a focused
   verifier under `scripts/client/`. Names are provisional. Register patching and
   verification in `Program.cs`, `BuildRelease.cs`, and project resources as
   required by existing patterns. Store necessary asset overrides under
   `client-overrides/files/` with matching integrity metadata. CI must reproduce
   the feature without relying on local `krinfo` or table-data checkout paths.

## Validation and acceptance

| Area             | Required evidence                                                                                                                                                                                                                                                 |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Calculated mode  | Description matches the pre-feature native output for the same hero context; repeated toggles restore it exactly.                                                                                                                                                 |
| Formula accuracy | Evaluate supported symbolic expressions with the same input values and rounding rules and compare against the native description evaluator. Separately trace representative coefficients to gameplay data; description parity alone does not prove combat parity. |
| Context          | Test low/high skill levels, awakening states, equipment-added levels, costume descriptions, enhancement changes, and owned/reference heroes where the card supports them.                                                                                         |
| Skill shapes     | Cover direct damage, multiple hits, DoT totals/ticks, healing, shields, stat buffs, target-stat scaling, conditional damage, and passives.                                                                                                                        |
| Linked skills    | Test the screenshot's initial/follow-up pair and other multi-variant skills in both modes; the two buttons remain independent.                                                                                                                                    |
| Layout           | Verify top-left symmetry, title clearance, native click area, long formulas, scrolling, localized text, and supported resolutions/aspect ratios.                                                                                                                  |
| Lifecycle        | Reopen/recycle the card, switch heroes/slots repeatedly, change skill context, and restart the client to verify the saved preference; no duplicate buttons or callbacks.                                                                                          |
| Fallback         | Force unsupported expressions and renderer failure; native values/text remain usable and clearly identified.                                                                                                                                                      |
| Isolation        | Confirm toggling makes no gameplay requests or mutations; combat-related tables and calculation methods remain unchanged.                                                                                                                                         |
| Build            | Focused patch/verifier and full `--build-release` integration pass; repeated patching is safe and unrelated existing patches survive.                                                                                                                             |

Use a data-driven inventory to report coverage by skill ID, linked variant,
costume override, and unsupported expression type. Add meaningful renderer and
lifecycle tests rather than tests that merely repeat implementation constants.
Capture before/after screenshots of the supplied card in both modes.

## Delivery phases

1. Confirm installed code/data and produce a working representative sample:
   screenshot skill plus its linked form, and one simple hero's four skills.
2. Complete generic expression support and audit the remaining hero roster and
   variants. Resolve or explicitly report unsupported cases.
3. Finish localization, layout/lifecycle checks, full release integration, and
   regression validation. The main uncertainty is expression coverage, rather
   than placing the button; estimate total effort after the first phase.

This document authorizes no implementation or installation by itself. During a
future implementation task, any installed client/asset change must finish with a
new unique signed local update following [client-updates.md](client-updates.md)
and [github-client-updates.md](github-client-updates.md). Verify release/stable
manifest agreement, signature, file hashes/sizes, changed-file inclusion, and
intentional deletions. Report the release ID and restart requirement. Keep local
artifact creation distinct from GitHub publication and host deployment.

For this documentation-only task, no installed-client release is required.
