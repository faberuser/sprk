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

The shared `tables/CostumeTicketRules.json` also opens all 80 archived League of
Honor costume variants for direct sale at 10,000 Rubies, matching Legend outfits.
`--costume-tickets-only` installs their native Costume/CostumeGroup rows and
localized shop group alongside the existing ticket data. The server consumes
the same sale overrides at startup; its League of Honor item shop is retained.

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
400%. Drag to move the artwork, and double-click or middle-click to reset.
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
