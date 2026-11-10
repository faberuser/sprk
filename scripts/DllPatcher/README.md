# DLL patcher

Patches the King's Raid client for SPRK. Run commands from the `sprk-server`
repository root; the examples use the client at `../sprk-client`.

## Setup

Requires .NET 8 and Python with the client asset dependencies:

```powershell
python -m pip install -r scripts/client/requirements-client.txt
dotnet build scripts/DllPatcher
```

## Apply a focused fix

`--hero-death-voices-only` corrects the battle creature's case-sensitive
Dead/Damage/Down sound prefixes and retains the dot when a costume voice falls
back to its hero's base voice. The original uppercase DEAD lookup cannot find
the installed or archived `Dead.Hero.*` definitions. Native death detection,
timers, voice language, audio settings and animation-sound fallback stay in use.
`--verify-hero-death-voices` checks the death path and executes the native sound
code builder against the installed catalogue, including missing costume voices,
plus native English/Korean/Japanese death sound loading. The full release build
installs and verifies this patch. Restart the game after installation.

`--lavril-circle-only` aligns Lavril S3's normal and Glory League ground circles
and impact flourish with the stationary damaging field, six metres in front of
the caster. The old tree used the self-targeted projectile-creation operation's
target (Lavril) for the circle. The restored build1558 tree (tracked override)
places them at caster origin + facing * 5m; both forms are accepted and rewritten
to the same 6m position. Combat tables, damage, stacks and targeting are
unchanged. `--verify-lavril-circle` checks the installed node tree against the
native projectile data and tests native radius boundaries with multiple enemy
positions for both facing directions. Only three visual position records change;
the complete tree is read back and checked for unrelated changes and idempotence.
The full `--build-release` installs and verifies this fix. Restart the game.

`--soulstone-ownership-only` makes the Soulstone Ticket's Owned and Not Owned
filters use the hero roster instead of unique-weapon inventory. The native class
filter and ticket pool remain in use. `--verify-soulstone-ownership` checks the
installed lookup and executes all twelve ownership/class combinations through
the installed method's control flow, plus patch idempotence. The full
`--build-release` installs and verifies this fix. Restart the game after installing.

`--skill-formula-only` adds a `123` / `fx` display toggle to the top-left of the
hero skill card, mirrored against its linked-skill button. It preserves the native
calculated description and combat data, and renders formulas from current native
expressions with parity checks and local preference persistence. Run
`--verify-skill-formula` for hook/IL validation and
`python scripts/client/verify_skill_formula.py --installed` for isolated native
coverage, card lifecycle, linked variants, screenshots and process-restart checks.
The full `--build-release` installs and verifies the patch. See
[the implementation and mirror workflow](../../docs/skill-description-formula-toggle.md)
for regenerating `krinfo`'s `table-data/sprk` export. Restart the client.

`--team-exp-refresh-only` applies the battle response's final `ExpResult2` through
the native UserManager after all other reward snapshots. Stamina-earned team EXP
and level changes then notify the HUD immediately, including when recruitment
EXP is also awarded. Battle level-up presentation uses the final result, with
the original `ExpResult` as fallback. Absolute snapshots are applied directly,
so a repeated response cannot add EXP twice. `--verify-team-exp-refresh` checks
the hook ordering and retained native reward paths. Stage this fix and
`--automation-only`, then run `python scripts/client/verify_team_exp_refresh.py`
for isolated native JSON parsing, account/observer, profile level-info, rollover,
recruitment ordering and fallback checks (`--installed` after installation).
The full release build installs and verifies this patch. Restart the game.

`--artifact-source-filter-only` fills the artifact ticket's right-hand All
dropdown with source categories available in that ticket: Normal, Event, World
Boss, Velkazar, Shakmeh, King's Pass and Divine Punishment. Membership comes from
the native archived artifact reward groups; unrecognized artifacts remain under
Other. The left dropdown retains All/Owned/Not Owned and combines with the source
filter. Changing ownership retains the source; reopening a ticket resets both.
Filtering retains the ticket's pool and original order, resets list scroll,
clears the selected artifact and disables Acquire until a visible item is chosen.
Other selector types retain their native filters. Run
`--verify-artifact-source-filter` for installed hook/eligibility checks. Stage
this patch and `--automation-only`, then run
`python scripts/client/verify_artifact_source_filter.py` for isolated native list,
source/ownership, selection and reopen checks (`--installed` after installation).
The full release build installs and verifies this patch.

`--ticket-search-only` adds a name search field to selector tickets: unique
weapon/treasure and "all unique" tickets, artifact tickets, soulstone (soul
weapon) tickets, hero tickets (both the single list and the Physical/Magical
layout) and costume tickets. In windows with the native filter bar, the field
takes the last slot beside the existing dropdowns and copies their pill sprite.
Natively stacked dropdowns share one slot. Narrowed dropdowns keep their arrow's
native inset from the pill edge. The Physical/Magical ticket gets a row above its
column headings using the native `Common_Box_2` input box. Both use the window's
font and the `Common_Close` clear button, inset inside the blue background with
reserved text space. The probe checks its icon/hit-area inset and clear action
across all seven layouts. Matching ignores case, spaces,
punctuation and accents. Every word must match the localized hero name or title,
the hero's internal English code name, or the item/costume name. When no entry in
the ticket matches exactly, matching becomes typo-tolerant. Words of 4-5 letters
allow 1 edit, 6-8 letters 2 edits, and longer words 3 edits. Adjacent swaps count
as one edit. Name initials also match (for example `dlk` for Dark Lord Kasel).
Typing filters after a short pause (Enter applies immediately). The native lists, their order, ownership
and class filters and empty-state labels are retained. Only non-matching
entries are skipped. Reopening a ticket clears the query. A unique/soulstone
selection hidden by the search is cleared and Acquire is disabled. Hero tickets
keep the chosen hero in the left panel. Hero tickets with two hero slots
(`InvenUseMultipleHeroSelect`) page through heroes without a list and are
unchanged. `--verify-ticket-search` checks the installed hooks. With this patch
and `--automation-only` installed, launch an isolated client with a
`ticket-search-*` profile and send the `ticket-search-probe` request with text
`test`. It opens every ticket type on the Login scene and checks exact and typo
matches, empty results, clearing, selection, native filter rebuilds and reopening.
The fixture loads lobby atlases from the `HighAndMidSpec` bundle folder.
`open:<weapon|treasure|artifact|soul|hero|attr|costume>` and `type:<query>`
leave a fixture on screen for screenshots. The full release build installs and
verifies this patch.

The same patch adds a selector-style class dropdown and search field above the
middle `HeroPortraitList` grid in Hero Info. Class and name filters intersect;
clearing search retains the class, and reopening resets both. The original
portraits, selected hero and full preferred-hero order are preserved. The scroll
viewport and drag area reserve the top row once per window instance. The
`ticket-search-probe` mode `hero-info-test` checks combined filtering, empty
results, clearing, full-list bookmark moves, selection, reopening and layout
idempotence on the real Unity client using an isolated Login fixture.

`--selector-acquire-state-only` fixes stale grayscale on Acquire after a selector
filter/search temporarily disables the button. In both
`WeaponUniqueSelect.SetItemDescription(ItemData)` and its `SoulStoneSelect`
override, it changes only the native `UIHelper.EnableButton(Button_Buy, true,
false, false)` grayscale argument to `true`. The resulting `(true, true, false)`
keeps Acquire enabled and clears stale grayscale without forcing gray. Native
empty/null handling, eligibility and selection stay intact. The ticket-search
probe also checks Acquire's click/grayscale state through selection, hidden
results, clearing, reselection and reopening for weapon, treasure, artifact and
soulstone tickets. Installation patches the current DLL to
preserve existing patches, validates both exact call/argument shapes and IL
stacks, checks idempotence, stages and re-verifies the result, then checks for
client/shared-worker processes and backs up the DLL before replacement.
Unexpected or ambiguous native shapes fail closed.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --selector-acquire-state-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-selector-acquire-state --stage-only
```

Stage-only writes `Assembly-CSharp.dll.selector-acquire-state-staged` beside the
current DLL without replacing it. Omit `--stage-only` from the first command to
install (restart the client afterward); omit it from the verification command
to check the installed DLL. Full `--build-release` installs and verifies this
fix immediately after TicketSearch.

`--arena-practice-only` adds Arena Practice immediately after Battlefield of
Ordeals. Its Practice button opens the native four-hero Arena party selector,
with a separate saved roster and no entry cost. It uses the equipped-hero
factory from League of Victory and the See Motions field/dummy. A private combat
copy uses Victory's chapter, battle definition and stat modifiers; the dummy
counts as a hero target and cannot die. Preview mana regeneration is excluded.
The card uses League of Victory artwork. Practice enables health/status HUDs and
floating damage/heal text, retains manual skills and Auto, and fixes battle speed
at x1 with no speed button. These display rules do not change saved damage-text
preferences or other battle modes. Exit returns to Arena
selection without ranked results or rewards. Regular See Motions is unchanged.
Run `--verify-arena-practice` for installed hooks/helper checks and
`python scripts/client/verify_arena_practice.py` for isolated native prefab,
equipped-party, combat damage and preview-isolation checks (`--installed` checks
the installed patch). The full `--build-release` installs and verifies this patch.

`--battle-result-flask-only` keeps the flask bar, icon, final fill and gain text
after the battle-result flask animation. It removes the visual-only completion
callback that initialized hero EXP after every flask animation; native reward
application, level-up state, flask fill effects and non-flask results are retained.
`--verify-battle-result-flask` checks the scoped callback and native result paths.
Stage this patch and automation, then run
`python scripts/client/verify_battle_result_flask.py` for isolated native EXP/Ruby
partial-fill, rollover and reused non-flask card checks. Run `--unpatched` before
installation as a negative control, or `--installed` after installation. The
full release build installs and verifies the patch.

`--accounts-only` places Use Coupon above Language in Others and assigns the
Delete Account button's actual background target to the native red Common atlas
artwork for normal, hover and pressed states. Run
`python scripts/client/verify_login_settings.py` after installation to check the
native settings prefab, repeated refreshes, button states and screenshots in an
isolated client.

`--technomagic-refresh-only` applies TM tier-upgrade account state before UI
notification, then rebuilds the open awakening window's hero/inventory icons and
clears the old 5-star target. `--verify-technomagic-refresh` verifies the response
order, native error paths, and scoped refresh. After restoration replaces an item,
it also rebuilds Hero Info's equipped icons and inventory, discards the old
equipped-item details/menu and refreshes treasure slots. Returning with Back then
uses the restored item and its Enhancement eligibility without switching heroes.
Installation executes the native response callback with isolated service traces,
the Hero Info refresh across 16 hero/panel presence cases and patch idempotence.
The full release build installs and verifies this patch. The server prices TM
star upgrades using the target star, matching `ValanceAwakenWindow.AfterStar`.

`--technomagic-awaken-target-only` adds a target-star slider to Technomagic
Enhancement, so one tap can upgrade several stars. The slider reuses the native
Settings slider artwork (atlases assigned through its native assigners) beside
the required-material icon, and its star readout clones the window's material
count label. On selection it defaults to the highest star the player can afford,
or the next star if none is affordable. It is hidden when only one star remains.
The window's native data, labels, red/white colors, Enhance state, result-icon
stars and stat preview all show the summed cost and stats for the chosen target.
Enhance sends `TargetStar` (added in `JM_NShared_ValanceAwaken_Request`) only for
multi-star requests. The server prices each step at its after-star row, charges
the total in one transaction and keeps the native one-star request when the
field is absent. `--verify-technomagic-awaken-target` checks hooks, native
members and IL. Installation also runs the native request encoder against the
compiled helper. The full release build installs it before `Currency64`.

`--forge-awaken-chance-only` adds a success-rate slider to normal/UW/UT Forge
awakening. Each stop is a distinct positive chance achievable with the available
eligible materials, using the target's next-star success table and failure bonus.
For example, a one-star target with two zero-star copies has just 50% and 100%
stops. There are no fixed percentage intervals or empty-material stop. Selection defaults to the
highest achievable chance, capped at 100%, whenever a target is selected or its
star/failure bonus changes. The readout shows the actual achievable chance,
including the native failure bonus. The planner minimizes awakening points and
then material count for each distinct achievable chance. Ties prefer
lower-star duplicates before universal stones. Locked gear and native-ineligible
stones/gear are excluded. Native material icons, gold cost, button and request
arrays are refreshed; awakening still attempts only the next star when pressed.
Manual material selection remains available. `--verify-forge-awaken-chance`
checks hooks, eligibility/table calls and IL stacks. Installation compares the
planner with exhaustive subsets for 350 inventories, including mixed materials,
nonlinear rates, failure bonuses and the two-copy 50%/100% example. A single
achievable chance is selected automatically with dragging disabled; no eligible
materials disables dragging and selects nothing.
The slider sits in a separate row above Gold and Awaken Gear; the track and
right-aligned success readout span the combined width of those controls. Track
widgets widen while the thumb and row height retain their original proportions.
The target/result,
materials/success-rate and complete stat/effect scroll groups move upward together;
their internal spacing and stat viewport size are preserved. Layout is applied
once per pooled window. An active discount banner fits beside the raised instruction.
The opt-in Login automation action `forge-awaken-chance-probe` exercises the
native prefab, normal/UW/UT selection, dynamic stops across next-star tables,
single/empty inventories, stones, locks, manual edits and failure
refresh; `text: "preview"` retains its isolated fixture for a screenshot.
The full release build installs and verifies the patch.

`--skill-upgrade-all-only` adds an upgrade-all button (labelled Upgrade) to the
hero Skills tab's skill-book row. It clones the HeroList window's native All Heroes button as a taller sliced
button with a larger label. The four book counters get narrower count boxes
(restored before the pooled components are released) and tighter native-grid
spacing. The counters and button are then scaled together and centred in the panel. It is hidden for other
players' heroes and greyed out when no learned skill can gain a tier. Each skill
slot uses its own book, so every learned skill rises independently through
II-IV as far as its book allows. The confirmation uses the native skill-tier
popup and lists each skill, its tier change and its book cost. The button sends
the native `extend_hero_skill` request with `SkillExtend=-1`, which the encoder
turns into `ExtendAll=1`. The server applies every tier and book cost in one
transaction (locked or reserved books are excluded). It returns `ItemResults`
and `SkillExtends` alongside the native `ItemResult`; a parser hook captures
them, so all book counts and tiers update through native account managers.
Native single-tier requests are unchanged. `--verify-skill-upgrade-all` checks
hooks and IL. Installation also runs the native encoder and response parser
against the compiled helper. The full release build installs it before
`Currency64`.

`--legendary-rune-tickets-only` upgrades legacy random ticket 91029 and selector
94609 to the archived 2022 pools (24 legendary runes, including Swiftness, Life,
Tenacity and Vitality). It bakes native ItemGroup/SelectItem rows and refreshes
their integrity hashes; `--verify-legendary-rune-tickets` validates both pools.
The C# patcher edits the native JIT fields directly, without a Python or msgpack
dependency. `--self-test-legendary-rune-tickets` checks upgrade, preservation,
idempotence and corrupt-hash rejection. The full release build runs these tests,
the transformation and verification automatically.
Server reward pools in `tables/` must stay aligned. Restart the game and server
after installing their respective changes.

`--runes-only` restores both the hero rune selector and the separate Rune of
Blasphemy inventory subcategory, retaining the native equipment and grind paths.
Stage it and automation, then run `python scripts/client/verify_inventory_runes.py`
to validate the original inventory prefab and both category selections in Unity.

`--hero-rune-filter-only` adds Sort Option beside the Heroes rune category
dropdown. It reuses the ticket's native stat picker: select up to two stats,
then OK to show runes containing every selected stat. Deselect and OK restores
the full list. Normal runes and crafted runes' rolled main stats use the native
stat conversion; category, slot eligibility, equipment actions and existing
ordering remain in place. The button shows the active selection count. Filters
persist across category changes while the page is open and reset on close/reopen.
`--verify-hero-rune-filter` checks hooks, native eligibility and popup lifecycle.
Stage this patch and `--automation-only`, then run
`python scripts/client/verify_hero_rune_filter.py` for isolated native prefab,
ticket matching, Deselect/OK, crafted-stat and close/reopen checks. Use
`--installed` after installation. The full release build installs and verifies it.

The shared `tables/CostumeTicketRules.json` also opens all 80 archived League of
Honor costume variants for direct sale at 10,000 Rubies, matching Legend outfits.
`--costume-tickets-only` installs their native Costume/CostumeGroup rows and
localized shop group alongside the existing ticket data. The server consumes
the same sale overrides at startup; its League of Honor item shop is retained.
This mode also restores Dressing Room browsing after native getter resets and
replaces the obsolete local Light/LoH visibility filter. LoH costumes appear in
the Dressing Room for purchase; unowned Light and Darkness costumes remain
ticket-only, and owned ones remain visible for equipment. The Light label is simply
`Light Costume`. The full release build runs this mode and its verifier, including
execution of the patched visibility and ownership methods against the roster.
The same rules enable Ashen costumes for Dark Lord Kasel (8408), Fallen Frey
(8508), and Rebel Clause (8307) in the shop and Dressing Room at 3,000 Rubies.
Their dedicated tickets (600066, 600067, 600068 respectively) remain valid.
Table baking reads original backing fields so patched UI getters cannot change
unrelated costume data.

`--hero-material-tickets-only` lets players tap fragment/essence material icons
on the hero awakening page and the required-material row in Hall of Heroes /
Room of Ordeals to use owned selection tickets. Matching tickets are found from
their reward pools; multiple eligible tickets open a ticket chooser.
The native use popup offers only the tapped material and retains its quantity
limit and explicit confirmation. The quantity starts at the missing material count,
capped by owned tickets and the 1,000-ticket limit; it starts at zero when enough
materials are already owned. Players can edit the quantity before confirming.
The single material is preselected through the native icon click handler, so
players can confirm immediately without clicking its icon first.
Awakening fragment icons show the native “+” when the requirement is unmet and
at least one matching unlocked fragment ticket is available. The indicator
updates on refresh and also applies to the Hall of Heroes material row.
Stage the hero-ticket and automation patches and run
`python scripts/client/verify_awakening_fragment_tickets.py` for native indicator,
refresh and ticket-popup checks; `--installed` tests the installed DLLs.
Ordinary inventory ticket popups keep their native defaults.
Essence crafting counts loose fragments plus owned, unlocked tickets whose
reward pools contain the required fragment. The available count, Max quantity,
and Craft button all use that combined balance.
The essence material's native “+” crafting indicator also counts those tickets,
including mixtures of loose fragments and tickets, and refreshes as balances
change. It stays hidden when the essence requirement is met or fewer than one
craft's ingredients are available. When opened from a material
shortcut, the quantity starts at the required essence amount, capped by the
native material/gold limits. Craft redeems only the fragment shortage through
the native ticket endpoint (up to 1,000 per request), applies confirmed balances,
then invokes normal crafting. Failed redemption prevents crafting; any fragments
already redeemed remain in inventory. Other recipes keep their native behavior.
Recipe matching ignores the native JIT's empty ingredient slots. After staging
the hero-ticket and automation patches, run
`python scripts/client/verify_essence_crafting.py` for an isolated Unity check
of all seven actual essence recipes, visible combined balances, required
quantities, Max, Craft availability, and material shortage clamping. It uses
temporary in-memory balances at Login and never redeems tickets or crafts.
Material counts stay visible. Native inventory observers
refresh counts and awakening availability after use. With no matching ticket,
essences retain the native crafting action; fragments show an unavailable-ticket
notice because they cannot be crafted. Transcendence stones, limit break,
and other screens retain their native behavior.
`--verify-hero-material-tickets` checks the scoped hooks, signatures and observer;
installation also runs isolated service/UI probes against the exact helper source
for successful/failed use, duplicate requests/responses, stale sessions, depleted
tickets, multiple ticket choices, and fallback behavior. These probes do not use
player inventory. Both installation and verification run in `--build-release`.

`--awakening-materials-only` hides gray inventory equipment and awakening stones
inside gear awakening, then compacts that material grid. It also rejects every
nonzero equipment lock flag in the shared native awakening-material predicate,
so locked artifacts, unique weapons and unique treasures cannot be selected as
materials. Locked gear remains usable as the awakening target. Native soul-weapon,
metadata and required-level checks remain intact, as do selected-item checks and
restoration on later refreshes. Compaction also requests native next-frame grid
layout: initialization while the window hierarchy is hidden must not leave all
icons at the first-slot origin on cold opens or pooled reopens. All five equipped
and unequipped sort fields are initialized in the constructor to the native
close/reset defaults. Previously they started at zero and became one only after
closing, so the first open after each game restart used different ordering and
arrows. User sort toggles and the native comparators remain unchanged. The full
release build installs this patch.
`--verify-awakening-materials` checks the selection/visibility hooks, retained
native checks, immediate/deferred layout, constructor/reset default parity,
80 native sort-toggle transitions with sprite arguments, IL stack, patch
idempotence and 16 lock/null/eligibility cases. Install this patch and `--automation-only`, launch
an isolated client with `scripts/automation/client_automation.py`, then send the
`awakening-material-probe` action on the Login scene. It checks eight native icons
across a cold hidden initialization and two pooled reopens without sorting, plus
compact filtering, hide-all, restore-all and spacing. It also checks two fresh
native window instances start with all five reset defaults, and verifies the
sort arrows in both directions. Repeat in a new client process to cover restart.

Tower continuous-battle startup uses `--continuous-battle-only` (and
`--verify-continuous-battle`). Its native popup keeps a 32-bit serialized prefab
slot while the runtime booster count remains 64-bit. The currency migration and
full release build apply and verify this compatibility repair. Stage it and
automation, then run `python scripts/client/verify_tower_continuous.py` to check
the real native popup in an isolated Unity process. `--unpatched` is a negative
control that deliberately reproduces the original fatal prefab load; it must run
before installing the repair. These checks cover popup startup, stop/reopen,
serialized defaults and large counts, and do not simulate tower clears.

Use a focused mode when updating an already patched client. For example:

```powershell
# Prepare and check without installing.
dotnet run --project scripts/DllPatcher -- ../sprk-client --world-boss-keys-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-world-boss-keys --stage-only

# Close game clients and workers, then install and check.
dotnet run --project scripts/DllPatcher -- ../sprk-client --world-boss-keys-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-world-boss-keys
```

Focused modes preserve existing patches. `--stage-only` prepares output without
replacing active files; output names vary by mode. Verifiers that support
`--stage-only` check the staged output instead of the installed files.
Restart the client after installation.

Find feature-specific flags and checks in the [patch reference](REFERENCE.md).
Argument routing lives in [Program.cs](Program.cs).

## Illustration AI

The viewer's small Off / On button displays Remacri's full native x4 output
when On and restores the original when Off. No x2 option or downsampling remains.
Disk cache entries identify source content, actual model hash and processing
version, so turning On again can load a completed result without new inference.
Off, navigation and close cancel pending processing and release owned textures.

A gold loading bar below the button reports downloaded bytes and NCNN tile
percentages. Preparation/cache/image-loading stages show indeterminate activity.
The button text stays Off / On. Hide UI also hides the loading indicator.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --illustration-ai-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-illustration-ai --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --self-test-illustration-ai
python scripts/client/verify_illustration_ai.py
```

The native verifier uses private managed DLLs, runtime, model storage and cache.
It checks Off/On toggling, native x4 output, progress rendering, rejected x2
selection and disk cache reuse with the executable unavailable. Use `--installed` to test the
installed helper. See the [custom-model guide](../../docs/illustration-ai-custom-model.md)
for client-only model swaps.

## Illustration zoom

The hero/costume illustration viewer supports mouse-wheel zoom from 50% to
800%. Native touch zoom also reaches 800%. Drag to move the artwork, and
double-click or middle-click to reset to 100%.
Switching illustrations restores the default view. Hide UI and the native
touch controls remain available; wheel input ignores other popups and buttons.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --illustration-zoom-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-illustration-zoom --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --self-test-illustration-zoom --stage-only
```

After closing the game and shared-asset workers, omit `--stage-only` to install.
The full release build installs and verifies this patch automatically.

## Full builds and releases

For routine development, edit the tracked patcher source. CI runs
`--build-release` in a fresh disposable client workspace, applying all patches,
companion helpers and verification before signing. See the
[GitHub release workflow](../../docs/github-client-updates.md).

Use `--build-release` only in a disposable build workspace. Full patching uses
`Assembly-CSharp.dll.backup_before_patch` (created if absent), whereas focused
modes use the active DLL.

For local full patching, [patch.ps1](../client/patch.ps1) coordinates DLL and
asset changes, including telemetry cleanup:

```powershell
./scripts/client/patch.ps1 -ClientPath ../sprk-client
```

The DLL patcher alone removes only managed telemetry code. The wrapper also
cleans serialized assets, SDK files and startup hooks. Recovery files and
reports stay under `target/telemetry-removal/`. Rerun cleanup after restoring
original files; `patch.ps1 -Restore` does not undo the full cleanup.

**After changing installed client files, generate and verify a new signed
client update** using the [client update workflow](../../docs/client-updates.md).
Deploy matching server support before distributing changes to client/server
protocols, including batch shop purchases, mail claims and dispatch results.
Documentation-only edits do not require a client release.

## Validation

Run the feature's verifier and self-tests listed in the reference. Some changes
also have isolated Unity checks (requires a graphics device):

```powershell
python scripts/client/verify_graphics_options.py
python scripts/client/verify_graphics_options.py --window-checks
python scripts/run_free_camera_probe.py
python scripts/check_settings_persistence.py --client ../sprk-client
python scripts/client/verify_client_storage.py
```

The graphics probe stages by default; add `--installed` to check installed DLLs.
Probe results validate fixtures and settings, not sustained gameplay performance.
Free-camera pointer routing on actual screens still needs a play check.

## Source layout and details

| Location | Purpose |
| --- | --- |
| [Program.cs](Program.cs) | Arguments, patch order and installation |
| [BuildRelease.cs](BuildRelease.cs) | Full release build integration |
| [Patches/](Patches/) | Patch implementations (partial members of `Program`) |
| [Resources/](Resources/) | Embedded helper sources, fixtures and assets |
| [REFERENCE.md](REFERENCE.md) | Feature behavior, focused modes and validation notes |

New patches and verifiers must run through `--build-release`, including
`BuildRelease.cs`; existing patch fixes use their existing integration.
There is no separate JSON patch recipe. C# files in `Patches/` are included by
the SDK automatically. Preserve stable embedded resource names such as
`DllPatcher.CostumeOwnership.cs.txt`.

For storage adoption and cleanup, see
[client storage migration](../../docs/client-storage-migration.md).

## See Motions costume previews

`HeroDetailView` builds its Costume tab from every costume row for the current
hero, including unowned, default/awakening, event, legendary and bonus variants.
Preview rows use neutral state and highlight the currently previewed costume;
shared ownership, purchase, equipment and Dressing Room state checks remain intact.
This only changes the local model preview and does not grant costumes.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --motion-costumes-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-motion-costumes --stage-only
# After closing the game and shared-asset worker:
dotnet run --project scripts/DllPatcher -- ../sprk-client --motion-costumes-only
```

The full `--build-release` path installs and verifies this patch. Verification
checks matching-hero enumeration, all three preview state refreshes, absence of
ownership/visibility filters, native shared ownership handling, unchanged costume
ownership method bodies, patch idempotence and the replacement method stack.

## All-roster costume tickets

See [costume tickets](../../docs/costume-tickets.md) for the expanded Darkness
pool, the new 102-hero Light ticket, shared asset evidence, native table/localization
baking, focused staging/verification and signed release requirements.

## Soul Weapon advancement playback

`--soul-upgrade-flash-only` shortens the advancement flash particle's lifetime
from 1.5 seconds to 0.35 seconds and selects unscaled particle time. The original
size-100 flash holds almost full opacity for roughly the first 66% of its life,
which looks like a one-second freeze even while the cinematic continues behind
it. The shorter flash still covers the background change at 1.9 seconds in both
A1/A2 timelines. `--verify-soul-upgrade-flash` checks the installed asset; both
modes accept `--stage-only`. The full release build applies and verifies it.

The asset transformation changes only that particle's lifetime and clock. It
roundtrips the native bundle, verifies all other objects byte-for-byte and
preserves every other field of the flash particle. The patched bundle is tracked
under `client-overrides/files` and included in signed client updates. Gameplay,
cinematic events, result reveal timing, sounds and existing lifecycle safeguards
remain unchanged. This visual fix is not evidence that old crash reports share
the same cause; the recording shows no crash or frame-time telemetry.

`--soul-upgrade-playback-only` (optionally `--stage-only`) installs scoped A1/A2
result playback safeguards. `--verify-soul-upgrade-playback` verifies the installed
or staged hook and runs the lifecycle tests. The full release build installs and
verifies this patch before currency migration.

The native 4.5-second result reveal and one-second input delay use real-time
waits. Only Common/Cinema SoulWeapon_RankUP_1 and SoulWeapon_RankUP_2 select
GlobalTimer.UnscaledLocal; other cinematics retain their incoming timer. The result
window Animator temporarily uses unscaled time at normal speed, restoring its
previous mode/speed on close or disable. Global time scale is never changed.

Before a reused result window starts, its previous NodeTreeUIPlayer load/play
queue is closed through native cleanup. A session generation guards coroutine
callbacks after close, disable or reopening; touch blockers are removed without
creating a new global window during teardown. Native result data, the initial
close guard, completion callback and cinematic release stay intact. Two log
entries record opening timing and release duration for subsequent investigations.

Tests cover exact A1/A2 scope, unrelated cinematic fallback, stale callbacks after
close/reopen, hiding the window, valid completion/disposal and patch idempotence.
Verification also checks both real-time waits, preserved durations and native
cleanup. The intermittent player crash was not reproduced; these checks establish
the timing/lifecycle fixes, not proof that every silent exit has the same cause.

## Lobby avatar account switching

`--lobby-avatar-stability-only` (optionally `--stage-only`) installs the lobby
accessory refresh safeguards. `--verify-lobby-avatar-stability` verifies installed
or staged files and runs account/session isolation tests. The full release build
installs and verifies this patch before Currency64 migration.

The original accessory observer released/rebuilt the entire lobby hero for each
notification. It now queues one update on that MapLayout, bound to the exact
UserManager instance captured when its player avatar was created. Old account
work is rejected, including A-B-A switches. Disabling the map cancels queued work
while allowing reactivation for the same account; a new avatar creation rebinds
the owner. Requests wait for a valid completed creature load, with a bounded
10-second readiness window, then use the native ChangeCustomizeStock path for
changed parts. The existing hero render facade is never replaced by this path.
Reconciliation is also queued after initial creation and avatar replacement,
including basic-weapon outfits. While the lobby is active, it detects a new load
or live creature after a model reload and reconciles again. Missing ownership
data during login defers the operation instead of clearing equipped parts.
Comparisons use the live creature's OpenData stock, since the load's original
request stock is not updated by ChangeCustomizeStock. Missing live accessory
instances are repaired even when their original requested stock already matches.
Tests execute reconciliation against native stock structures and simulated
creature interfaces for basic and costume weapons, missing instances, changed
accessories, unequips, stable no-ops, readiness and A-B-A account switching.

Lobby appearances use the saved `PlayerAvatarHeroInfo` selection, including its
costume, hair, weapon and accessories. Equipping different hero cosmetics leaves
that selection intact. The profile icon and portrait use the selected avatar
index. Account isolation, readiness checks, accessory instance repair and cloned
transforms remain in place. Story-forced avatars retain their native behavior.

The server persists the complete `PlayerAvatarHeroInfo` at lobby selection time:
body, hair, weapon, unique-weapon visibility, all four supported accessory slots
and their serialized position/rotation/scale transforms. It also saves awakening
rank and equipped item values, including nested soul-weapon data, in
`SprkLobbyRenderInfo`. The client captures this optional metadata while parsing
the native avatar response and supplies the saved rank and equipment explicitly
to the lobby renderer. Older servers retain native rank/equipment behavior.
Explicit normal cosmetics
and empty accessory slots are retained. Later equipment edits, accessory movement,
replacement, removal and dressing-room resets leave the lobby snapshot unchanged,
including across logins. Selecting a lobby hero again replaces the snapshot,
even when its portrait index is unchanged. Existing accounts freeze their first
post-upgrade appearance; cosmetic-only snapshots retain their saved cosmetics
and capture accessories once. Earlier choices cannot be recovered, so reselect
the lobby hero to save the intended look. Deploy the server and signed client
update together, then restart the client. The focused lobby patch installs and
verifies the parser, renderer and same-portrait reselection hooks; its native
JSON fixtures cover independent rank, weapon, soul-weapon data, empty equipment,
selection isolation and stable/coalesced reloads.

Closed CreatureLoad completion callbacks must return before accessing released
OpenData. Existing guards are retained; the patch adds the same guard if missing
from an older input. The account patcher preserves the newer snapshot hook when
its focused mode is run afterward.

Checks cover readiness deferral, event coalescing, stale owner rejection, A-B-A
switches, cancellation/reactivation, native hook scope, render-body retention,
closed-load guard and patch idempotence. These validate the identified refresh
risk; the player's intermittent visual symptom still needs an in-game retest.

## Gear Custom slot wearer portraits

`dotnet run --project scripts/DllPatcher -- <client> --gear-preset-portraits-only`
installs `SprkGearPresetPortraits.dll` and updates the native slot portrait method.
`--verify-gear-preset-portraits` checks the installed hook and runs the regression
fixtures. Both installation and verification also run in `--build-release`.
The selected hero keeps priority when wearing any saved piece; other slots show
whichever hero wears the most saved pieces (lowest hero index breaks ties).
Empty slots, deleted gear, and unequipped/chest gear show no portrait. The helper
reads current equipment ownership whenever the native panel refreshes, rather
than remembering who originally saved or applied the slot.
# Legacy banner downloads

`--legacy-banners-only` disables remote shop and product-offer banner downloads,
returns an empty board URL, and clears the retired endpoint from the native
`ConstantTable.jit` and its compiled preload copy. It preserves other table
entries and updates `SPRK-NativeData.json` integrity metadata. Use
`--verify-legacy-banners` to check the installed methods, preload array size,
empty constants, hostname removal and table hash. The full `--build-release`
applies and verifies this cleanup too. Restart the game after installation.

# Batch grind confirmation

`--batch-grind-only` skips the individual unique weapon/treasure warning while
selecting equipment in grind mode. The existing final confirmation remains and
shows selected unique counts, combined unique-item reward amounts (or ranges),
and an irreversible destruction warning. Ordinary gear confirmations and other
selection modes retain their native behavior. Reward previews use native break
tables and each selected item's awakening level; grinding and result handling
remain native. `--stage-only` stages the DLLs without installing them.
`--verify-batch-grind` checks installed hooks/helper code and executes mode,
filter and reward summary checks. The full `--build-release` installs and
verifies this patch. Restart the game after installation.

# Hero's Inn presentation

`--inn-presentation-only` restores the September 2022 button group policy:
owned heroes use Closeness, the active recruit uses Friendly, and unselected
recruits use None. `SetListener` only stores its listener; each prefab retains
its own action types, click handlers and visibility. This removes the old
`HideUnsupportedButtons` override that assigned recruitment actions to every
group and caused overlapping cooldown, cost and max-Affinity labels.

The pre-doomsday x86 `InitUIBtnGroup` at `0x11B16D9` and
`ClosenessButtonSet` at `0x11B3B52` establish these behaviors. Its
`SetClosenessGauge` at `0x11B0A35` reads the saved `ClosenessPoint` and displays
MAX only when the hero-specific threshold is reached. Owning a hero does not
automatically fill this gauge. Server HeroInfo responses use `ClosenessPoint`
as well, including responses to greetings.

`--verify-inn-presentation` checks installed code. Run
`python scripts/client/verify_inn_presentation.py` to check the real prefab's
actions and all 24 owned-button label states in an isolated Unity Login.
`--unpatched` provides the negative control and `--installed` checks a copy of
the installed DLL. No login or account mutation occurs. The full
`--build-release` installs and verifies this patch. Restart the game afterward.
# LINE pet index and summons

`--line-pets-only` restores the pre-doomsday pet index by removing only the
`IsLinePet` exclusion from `PetListBook.SetupPetPortraitComponent`. The native
rarity filter, owned/unowned portraits and LINE-specific care/adventure rules
remain intact. `--verify-line-pets` checks installed code; `--stage-only` stages
the game DLL. The full `--build-release` installs and verifies the patch.

`python scripts/client/verify_line_pets.py` tests the real Unity index in an
isolated Login, including all 16 LINE entries and both rarity tabs. It never
logs in or changes an account. `--unpatched` is the negative control;
`--installed` checks a copy of the installed DLL.

`LiveRules.json` routes first-set banners 27/30 to archived `GACHA_PET_2_GROUP`
and second-set banners 26/29 to `GACHA_PET_3_GROUP`, including their bonus draws.
The existing 100-draw pity uses the corresponding LINE Legendary group. Event
pool weights match the September 2022 `ItemGroupTable2`; ordinary pets and pet
food remain possible. Native mileage rewards and ordinary banners are unchanged.
Run `cargo test --locked pet_ -- --test-threads=1` for summon/ownership regression
checks. Restart the game and server after installing the DLL/rules.

`--conquest-dungeon-icons-only` restores the boss icon and status-board
portrait of baked Guild Conquest dungeons 290-292 (empty since they were baked
from the old restoration hook). It points those fields of native
`GuildSuppressDungeonTable.jit` at the strings the same bosses' archived rows
already use and updates `SPRK-NativeData.json`. `--verify-conquest-dungeon-icons`
checks the rows and integrity; the full `--build-release` installs and verifies it.

`--conquest-round-label-only` changes the English banned-heroes week label
(`GUILD_SUPPRESS_SESSION_NUMBER`, the Korean 회차) from "{1} time(s)" to
"Round {1}", matching the portal. Other languages are unchanged. It updates the
English localization pins in `SPRK-NativeData.json` and `SPRK-NativeCombat.json`;
`--verify-conquest-round-label` checks both. The full `--build-release` installs it.

`--portal-refresh-only` replaces only the conquest session hooks of the
installed `RestoredPortal` helper (server-sent seasons replace archived rows with
the same number; weekly rows in the banned-heroes list stay ordered). Use it on an already-patched client; `--portal-only`
re-runs shared patches that cannot be reapplied. The full build compiles the
same source. Add `--stage-only` to write `Assembly-CSharp.dll.portal-staged`.

`--line-pet-currency-only` makes banners 26/27/29/30 permanent ruby purchases
at twice the matching normal banner price (5,000 rubies for 10+1 and 500 for a
single). It edits only their duration/date/payment/price fields in native
`NewEquipGachaTable.jit` and updates `SPRK-NativeData.json`. Existing tickets
remain usable, like normal summons. Server `LiveSupport.json` matches these
fields. `--verify-line-pet-currency` checks prices, permanence and integrity;
the full `--build-release` installs and verifies this transformation.
`python scripts/client/verify_line_pets.py --currency` validates the loaded
native table in isolation (`--unpatched` for the negative control).

### Preview shadow culling

`--free-camera-only` also preserves planar shadows when a caster leaves the
camera frustum in See Motions, Dressing Room and the hero model preview. The
preview bounds contain both the model and its projection along the native
`_ShadowDirection`. Animated bounds use the current bones and bind-pose mesh
bounds without baking vertices. Automatic offscreen bounds replacement is
temporarily disabled; reset, window disable and model replacement restore the
native settings. Static meshes use private copies whose bounds are restored on
release. Combat rendering is outside this window scope.

`--verify-free-camera` checks the projection hook and cleanup paths, including
through the existing full `--build-release` workflow.
`python scripts/client/verify_preview_shadows.py` reproduces the lost shadow
with the native shader, verifies static/skinned projection bounds, moving and
offset bones, stable repeated updates, cleanup, and the actual Isaiah model.
`--installed` checks the installed helper in isolation. `--camera-controls`
runs the existing native preview camera/layout regression checks. Neither test
logs in or changes an account.
