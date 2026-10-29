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
order, native error paths, and scoped refresh. Installation also executes the
native response callback with isolated service traces and checks idempotence.
The full release build installs and verifies this patch. The server prices TM
star upgrades using the target star, matching `ValanceAwakenWindow.AfterStar`.

`--legendary-rune-tickets-only` upgrades legacy random ticket 91029 and selector
94609 to the archived 2022 pools (24 legendary runes, including Swiftness, Life,
Tenacity and Vitality). It bakes native ItemGroup/SelectItem rows and refreshes
their integrity hashes; `--verify-legendary-rune-tickets` validates both pools.
The full release build runs this transformation and verification automatically.
Server reward pools in `tables/` must stay aligned. Restart the game and server
after installing their respective changes.

`--runes-only` restores both the hero rune selector and the separate Rune of
Blasphemy inventory subcategory, retaining the native equipment and grind paths.
Stage it and automation, then run `python scripts/client/verify_inventory_runes.py`
to validate the original inventory prefab and both category selections in Unity.

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
inside gear awakening, then compacts that material grid. It retains the native
eligibility and selected-item checks and restores items on later refreshes.
`--verify-awakening-materials` checks the scoped hooks and IL stack. Stage this
patch and automation before running `python scripts/client/verify_awakening_materials.py`
for isolated native icon/grid visibility, restoration and spacing checks.

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

Lobby hero snapshots use the current manager's hero and owned accessories, reject
a portrait snapshot carrying another account ID, and clone accessory transforms
so previews cannot share mutable positions with the live hero. Portrait body
costumes retain their independent native behavior. Story-forced avatars and
unrelated hero notifications retain their existing exclusions.

Closed CreatureLoad completion callbacks must return before accessing released
OpenData. Existing guards are retained; the patch adds the same guard if missing
from an older input. The account patcher preserves the newer snapshot hook when
its focused mode is run afterward.

Checks cover readiness deferral, event coalescing, stale owner rejection, A-B-A
switches, cancellation/reactivation, native hook scope, render-body retention,
closed-load guard and patch idempotence. These validate the identified refresh
risk; the player's intermittent visual symptom still needs an in-game retest.
