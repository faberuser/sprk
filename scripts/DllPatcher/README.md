# DLL patcher

`Program.cs` handles arguments, assembly loading, patch order, and output installation.
Patch implementations are partial members of `Program` under `Patches/`:

| File | Responsibility |
| --- | --- |
| `HeroInn.cs` | Loads Unity references and coordinates hero inn patches |
| `HeroInnRecruiting.cs` | Recruiting button selection |
| `HeroInnPortraits.cs` | Portrait array bounds checks |
| `HeroInnButtons.cs` | Hides unsupported hero inn actions |
| `Payment.cs` | Payment initialization compatibility |
| `SoulWeapon.cs` | Disables unsupported limit-break stars |
| `HeroLevels.cs` | Restores Pre-Doomsday hero growth caps and removes hero Limit Break |
| `AccessorySales.cs` | Shows all accessories and enables direct ruby purchases in the Dressing Room |
| `Campaign.cs` | Campaign survivor filtering and its self-test |
| `Chat.cs` | Chat session and background repairs |
| `Shop.cs` | Shop shortcut, opening, and category visibility |
| `HeroVisibility.cs` | Hero availability |
| `CostumeVisibility.cs` | Costume browsing, previews, and motion button |
| `CosmeticSales.cs` | Cosmetic sale availability and fallback prices |
| `CostumeOwnership.cs` | Restores native ownership checks from embedded source |
| `PatchHelpers.cs` | Shared IL helpers |
| `TelemetryRemoval.cs` | Deletes telemetry types/call sites and removes telemetry SDK references |
| `DealerTicketPopup.cs` | Blocks Recommended Dealer and Support Ticket promotion eligibility and popup opening |
| `TrialHeroReward.cs` | Prevents hero recruitment animation after awakening/transcendence trial rewards |
| `AdventureUnlocks.cs` | Honors Portal tutorial skip and saved automatic dungeon milestones for mission categories |

`Resources/CostumeOwnership.cs.txt` is embedded with the stable resource name
`DllPatcher.CostumeOwnership.cs.txt`; it is compiled by the ownership patch at runtime.
The SDK automatically includes the C# files in `Patches/`.

Run from the server directory:

```powershell
dotnet build scripts/DllPatcher
dotnet run --project scripts/DllPatcher -- ../sprk-client --shop-only --stage-only
dotnet run --project scripts/DllPatcher -- --self-test-survivors
```

Omit `--shop-only` for all patches, or use `--chat-only` / `--campaign-only`.
`--stage-only` writes `Assembly-CSharp.dll.patched` without replacing the active DLL.
Omit it to install the result after patching. The patcher creates
`Assembly-CSharp.dll.backup_before_patch` if absent; full patching uses that backup,
while the individual patch modes use the active DLL.

`--portal-only` also initializes the native `NShader.ShaderVariableUpdater` after
login tables and account managers load. Its normal per-frame updates restore the
lighting, outlines, and projected-shadow direction in the lobby and other scenes.
Without it, the zero shadow direction draws shadow geometry over the character
and looks like overlapping models. This repair preserves the original shader
passes, meshes, materials, and saved costumes.

The portal patch also clears a surviving startup-logo overlay when the login
background opens after `LogoScene.IsLogoPlayed`. Scene startup can leave that
black widget visible over the loaded animation. Cleanup at the login handoff
preserves the startup sequence and the original animated background.

## Adventure unlock checks

Repair Adventure Board and mission category entry checks:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --adventure-unlocks-only --stage-only
```

The Portal's display and click checks accept tutorial skip while retaining all
entry conditions. Mission categories also accept the saved dungeon clear for
automatic `DungeonCleared` prerequisite quests without requiring a reward claim.
The server emits matching categories from saved progress. The client fallback
also handles an older server or a category list cached before the dungeon clear.
Restart the client after installing the staged DLL.

## Player privacy

Remove telemetry from an existing client (requires Python with `UnityPy` and .NET 8):

```powershell
python scripts/remove_client_telemetry.py ../sprk-client
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-no-telemetry
```

Remove the Recommended Dealer and Support Ticket promotions from an existing patched client:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --dealer-popup-only
```

The patch excludes the 2019/2020 `RecommendDealer` and `RecommendSupporter` types
from automatic popup eligibility and prevents their direct popup opening. Full and individual DLL
patch modes preserve this change. A running client must be restarted to load it.

All standard patch modes also prevent new-hero recruitment animation for
`BattleType.Trial` victories. This handles older servers that send existing trial
heroes in `EndCampaign.HeroInfos`; the server now reserves that field for actual
hero rewards. Purification material rewards and the hero's saved trial state remain.

Use `--stage-only` on the Python command to prepare and validate all files without
installing. The command removes the Masang GA4 reporter, its hidden browser startup
hook, Adjust, Firebase, Unity Services SDKs, dedicated Unity Analytics/crash/reporting
modules, obsolete symbols, and serialized telemetry scripts/prefabs. It strips
analytics/diagnostic integrations from the retained purchasing libraries; store
actions previously wrapped in metrics still execute directly. Unity service URLs
and cloud project identifiers are cleared from serialized settings. Normal in-game
web views are retained for support/UI features.

Reports, staged files, and recovery copies are under `target/telemetry-removal/`,
outside the distributed client. `patch.ps1` runs the removal after patching. Every
standard DLL patch mode also strips telemetry from its output, including when it
reads an older backup. Rerun the Python command after restoring original files.
Publish updates with the removed paths in `deleted_files` so already-installed
clients lose the SDK files too. Removal writes `King's Raid_Data/sprk-privacy.json`;
the update publisher automatically includes its required startup assets and carries
its SDK deletions into every release. It rejects releases that include a file the
privacy policy requires removing. Additional obsolete files can use `--delete`.

This removes the application code and configured reporting destinations from the
prebuilt client. UnityPlayer.dll includes native engine functionality that cannot
be physically removed without an engine/player rebuild. Its cloud reporting
startup settings are cleared; the shipped engine binary is retained.
That engine may log missing analytics binding messages because those managed
types were physically deleted. Startup and guest login were validated with the
existing Unity 6 player in both headless and Direct3D modes.

## Portal categories

`Patches/Portal.cs` injects the embedded `Resources/Portal.cs.txt` helper into the client DLL. It restores 15 missing categories (23 total including the existing categories) and native destination activities without replacing existing table rows. New panels use the standard Small layout and English fallback labels. Native entry checks remain in effect; exposing a category does not implement its server gameplay. Pet uses the client's single Pet panel route.

Apply only this change to an already patched client:

```powershell
dotnet run --project scripts/DllPatcher -- <client-root> --portal-only
```

Add `--stage-only` to write `Assembly-CSharp.dll.patched` without installing. The full patch includes the Portal restoration too. Reapplying is idempotent.

## Settings row order

`scripts/restore_settings_layout.py` restores the Game Options row order from a
reference `GameOptionTable.jit`. The CCBT order places Screen, Resolution, and
V-Sync immediately after Frame Rate. The repair changes only the tab's row-reference
list and the matching archive checksums, preserving option data and other tabs.

```powershell
python scripts/restore_settings_layout.py ../sprk-client --reference "<reference-client>/kingsraid-table-data/data/TableJit/GameOptionTable.jit" --install
```

Omit `--install` to stage the repair. Reports and installation backups are saved
under `target/settings-layout/staged`. Restart the client after installation.

## Username/password accounts

`--accounts-only` replaces the provider selector with a native login/register form and
installs `SprkAccounts.dll`. Standard DLL patch modes also retain this account flow.
The larger form includes focus borders, button hover/press feedback, a short fade-in,
and separate Show/Hide controls for password and confirmation. Tab and Shift+Tab
cycle through the visible fields; Enter submits from any text field. Passwords
return to hidden mode when opening, switching forms, or submitting.
Entered passwords and confirmation are retained after validation, authentication,
or connection errors. The field to correct regains focus with its text selected
for keyboard-only retries. Values clear after successful sign-in or leaving the form.
Server details appear as a status line; a separate Change control appears only with multiple
servers. The compact panel expands for errors without reserving empty footer space.
Both panels stay centered horizontally and vertically, with extra bottom padding
and no outer drop shadow.
The native company footer and account sign-in instruction are hidden at their refresh points.
The Others settings tab hides guest/provider account labels, Account Link and its
provider controls, Customer Center, Terms of Service, and Privacy Policy. Game ID,
Copy, Language, Use Coupon, and Logout remain available. Provider status lookups
are skipped. Successful native logout cancels queued lobby requests and clears password-account tokens and pending
tickets before returning to the login scene; the server must return both
`BaseResult: "Success"` and `Result: "Success"` and invalidate the supplied game session.
Both the new server and client must be installed. Existing saves can be linked with a
one-time code from `scripts/account_claim.py`; see [account setup and migration](../../docs/accounts.md).

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --accounts-only
```

Use `--stage-only` to prepare `Assembly-CSharp.dll.accounts-staged` and
`SprkAccounts.dll.staged`. Standard staged patch modes put the combined game assembly
in `Assembly-CSharp.dll.patched` and also produce the staged account helper.
Install both files together. Restart the client after installation.

## Hero level cap

The account/standard patch modes also apply `HeroLevels.cs`: hero level limits
come from CreatureStarTable (30–100, reaching 100 at Transcendence 3), without a
Limit Break increment. Hero Limit Break actions are unavailable. Fully transcended
heroes use the native compact Common panel with no Limit Break materials/button.
Awakening and Transcendence remain available until their normal caps. The server
rejects both hero Limit Break endpoints without consuming items.

## Dressing Room accessories

Account and standard patch modes apply `AccessorySales.cs` after native getter
restoration: accessory `IsOpen` and `IsBuy` are true, and `PreviewableWhenOwned`
is false. All 176 accessories can be previewed and bought directly, including
the 123 originally available only after acquiring reward items. Archived ruby
prices remain intact (those 123 cost 10,000 each). The server accepts their direct
purchase while validating ownership, compatibility, quoted prices and balances.
Acquisitions remain per hero, and accessory selector items continue to work.

## Client version

To update the bundled game and Unity player versions, stage or install the version
assets with `python scripts/set_client_version.py ../sprk-client --version 1.00.00 --install`.
The account helper displays versions as `1.00.00` in Login and Settings; the native
configuration keeps its padded comparison format. Login assigns the version label
from the current configuration instead of matching/replacing existing label text.
The separate patch-build label (such as `(229)`) is hidden on initialization and
after patch-version refreshes; patch checking still uses the original build data.
Reinstall the account helper
after changing its source. Asset backups are saved under `target/client-version/`.

## Remove obsolete provider SDKs

After installing username/password accounts, remove the unused Steam, Facebook and
Google MiniJson assemblies, native Steam plugins and Unity startup registrations:

```powershell
python scripts/remove_login_providers.py ../sprk-client
```

Use `--stage-only` to review the prepared files without installing. Recovery copies
and a removal manifest are stored outside the client under `target/login-provider-removal/`.
The tool removes Steam startup components and disables old provider linking routes,
while keeping the native billing interface to report external billing unavailable.
The client update policy carries the deleted
paths and required replacement assemblies/startup assets into future releases.
Standard DLL patches preserve this cleanup. Restart the client after installation.

## Remove real-money payments

This client uses in-game currencies only. Remove Unity's purchasing SDK, including
Apple, Google Play, Steam, Samsung, OneStore and Windows store integrations:

```powershell
python scripts/remove_client_payments.py ../sprk-client
```

The command bypasses billing initialization during login and disables cash products,
while preserving native gold, ruby and other in-game currency purchase paths.
It removes payment script metadata and startup registrations along with the SDK
files. Use `--stage-only` to inspect the changes. Recovery copies and the manifest
are stored under `target/payment-removal/`, outside the distributed client.
The update policy records `real_money_payments: false`, the removed files and the
required replacement assets; standard account patches retain this configuration.
Restart the client after installation.
