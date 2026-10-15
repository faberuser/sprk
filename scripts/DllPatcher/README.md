# DLL patcher

`--image-recovery-only` preserves installed patches and adds recovery for blank
`UIBundleLoader` thumbnails. Cache lookups discard destroyed Unity objects;
visible widgets rebind intact textures or retry failed loads up to three times
with backoff. Failed shared atlas requests reopen through native reference
counting. Healthy images and story scene-loading waits are unchanged. A file
that is genuinely missing still needs its asset repaired.

Use `--stage-only` to produce `Assembly-CSharp.dll.image-recovery-staged` and
`SprkImageRecovery.dll.staged`. Test both together in an isolated client before
installing; `--verify-image-recovery` checks installed hooks and the helper.
After installation, publish and verify a new signed client update as described
in [client updates](../../docs/client-updates.md). Recovery applies after restart.

`--party-join-unlock-only` preserves the installed assembly and adds the
`DungeonLocked` result to chat, dragon and story party joins. Locked guests see
an unlock explanation while native join cleanup still closes the waiting popup.
Use `--verify-party-join-unlock` to inspect the installed patch and
`--self-test-party-join-unlock` to exercise its production message IL, fallback
behavior and repeat installation. Server join checks remain authoritative.

`Program.cs` handles arguments, assembly loading, patch order, and output installation.
Patch implementations are partial members of `Program` under `Patches/`:

`--websocket-only` preserves installed patches and also guards delayed native
socket close callbacks against replacement sessions. After installation, use
`--verify-websocket-lifecycle` to verify that guard. Verify consecutive party
battles with one host Start Battle click each before publishing the update.

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
| `LobbyAccessories.cs` | Uses equipped accessories and their transforms for the lobby avatar, and refreshes accessory edits |
| `Campaign.cs` | Campaign survivor filtering and its self-test |
| `Chat.cs` | Chat session and background repairs |
| `Shop.cs` | Shop shortcut, opening, and category visibility |
| `HeroVisibility.cs` | Hero availability |
| `CostumeVisibility.cs` | Costume browsing, previews, and motion button |
| `CosmeticSales.cs` | Cosmetic sale availability and fallback prices |
| `CostumeOwnership.cs` | Restores native ownership checks from embedded source |
| `PatchHelpers.cs` | Shared IL helpers |
| `TelemetryRemoval.cs` | Deletes telemetry types/call sites and removes telemetry SDK references |
| `DealerTicketPopup.cs` | Blocks all Recommended ticket promotion eligibility and popup opening |
| `AppReview.cs` | Disables automatic and forced app-store review prompts |
| `TrialHeroReward.cs` | Prevents hero recruitment animation after awakening/transcendence trial rewards |
| `AdventureUnlocks.cs` | Honors Portal tutorial skip and saved automatic dungeon milestones for mission categories |
| `StoryAuto.cs` | Opt-in campaign progression through required story gates, native story skipping, and automatic skills |
| `VoiceLanguage.cs` | Keeps the 4K screen-resolution filter from hiding Japanese character voices |
| `ShopCatalog.cs` | Connects Hot & New to featured products delivered by the server |
| `AccessoryShopDuplicates.cs` | Removes repeated accessory IDs from Dressing Room shop lists |

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
Use `--voice-language-only` to install only the Japanese voice-choice fix from the active DLL;
`--stage-only` writes `Assembly-CSharp.dll.voice-language-staged` for review.
`--stage-only` writes `Assembly-CSharp.dll.patched` without replacing the active DLL.
Omit it to install the result after patching. The patcher creates
`Assembly-CSharp.dll.backup_before_patch` if absent; full patching uses that backup,
while the individual patch modes use the active DLL.

`--background-chat-only` applies only the background-processing repair to the
installed DLL, preserving all other patches. With `--stage-only`, it writes
`Assembly-CSharp.dll.background-chat-staged`. Normal chat initialization enables
`Application.runInBackground` so incoming whispers and previews are processed
while another game instance/window has focus. Restart the client after installation.

`--remove-app-review-only` disables `AppReviewPopup.Open` and `ForceOpen` in the
active DLL while preserving other patches. Add `--stage-only` to write
`Assembly-CSharp.dll.app-review-staged` without installation. Standard patch modes
also apply this change. Restart the client after installation.

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

## Continuous Story

Uncleared Normal campaign/boss stages offer **Battle Once** and
**Continuous Story** after Start Battle. Eligibility uses the original map node's
saved clear status, including entries without a scenario. Cleared stages keep the
native **Select Battle** popup with **Continuous Battle**, **Auto Repeat**, and
**Start Battle**. Only choosing **Continuous Story** enables the new behavior;
native Continuous Battle retains its existing rules.

Continuous Story uses the selected party, enables
automatic skills for the run, skips dialogue through the native Skip actions,
collects the native battle rewards, and advances to the next available battle
including uncleared stages. The existing Stop button cancels it. Defeat, failed
entry, insufficient stamina, a full inventory, a locked next node, or the end of
available story content stops progression. At a chapter's final battle it continues
to the next unlocked WorldMap chapter's first story battle. Native chapter flow and
skippable events finish before the next party starts. Entry resumes after these
events, including when the overview's native countdown was interrupted. An event
that requires interaction times out after 90 seconds.

The run carries its selected main and sub party across chapter-specific saved
decks, including an empty or different Chapter 8 deck. Restoration uses native
hero-selection actions and their ownership, dispatch, restriction and slot checks.
Scripted or forced parties keep their native setup without replacing the run's
selection. If the selected team cannot fit or enter a stage, progression stops
for player attention. Cancelling the run clears the carried selection.

Chapter 7's first Ice Crystal Cave visit is part of the route: 7-6 continues to
117-3 and 117-4, then returns to 7-7. The cave uses its native Easy mode, while
ordinary chapters keep Normal. The second cave node uses its native map action
(`ExecuteCurrentNode` / `NodeFunctionMenu`) to start tutorial 70001, retaining its
scripted party, effects, completion, and unlock event; continuation waits until 7-7 is actually
available. Accounts with tutorials skipped clear both cave battles normally,
then submit the native cave tutorial completion and apply its server response
to unlock 7-7 in the same session. This request never precedes either real clear.
Completed cave battles are omitted from the detour. Native tutorial-step progress
refreshes the inactivity timer without bypassing unlock conditions.

Chapter 9 also follows its required Scarlet 2 bridge: 9-13 enters substory node
9-51 in Easy mode, then resumes 9-14 in Normal mode after its real clear unlocks
bridge 9-113. The substory keeps its scripted Kasel party; the player's selected
team resumes on the main route. If Scarlet 2 is already complete, 9-13 proceeds
directly to 9-14. Optional substories keep their existing behavior.

Continuous Story stops after completing 9-23, even when Chapter X is already
unlocked. Players complete the Chapter 9 epilogue and Chapter 9.5 manually, then
can choose Continuous Story again at uncleared 10-1. Chapter X continues in its
native Normal mode, including 10-1 to 10-2, and restores the run's selected team
where the stage permits it. Unlock and entry checks remain native.
The Chapter 9.5 route and its lobby-event reward acknowledgement have been
removed from the helper. Automated substories are limited to Scarlet 2 and the required Chapter X finale (10-30 through 10-41).

Choosing Continuous Story initializes its own Unity update loop and native
tutorial manager, queues entry until the choice popup closes, and keeps processing
while the game is in the background. This also applies to normal launcher and
executable starts without automation enabled. Repeated entry events are retried
at most once per second and do not reset the inactivity timeout.

Cutscene skipping follows the visible native Skip button, including stories whose
later groups cannot be skipped all at once. Native Skip stops at required script
groups and runs their finalization effects. Dialogue groups that explicitly hide
Skip and wait for a normal tap (such as 6-6) receive the native dialogue touch,
allowing Skip to appear afterward. Timed and interactive groups retain their
native behavior. Scarlet 2 and Chapter X also acknowledge noninteractive dialogue
when Skip is disabled;
actual script advancement resets the inactivity timer.

Illustrated narration that hides Skip and forwards taps to `ForegroundTextList`
(including the campfire after 9-9) advances through the native window tap path.
Each line keeps its normal effects and finalization. This applies only during
Continuous Story and refuses groups with other interactive click scripts; manual
story playback still waits for taps.

Single-screen foreground narration that hides Skip and waits for a group touch
(including the card after 9-22) receives one native window tap during Continuous
Story. This finalizes the group's effects and lets Skip appear on the next group.
Timed groups and other interactive click scripts retain their normal behavior.

Informational battle tutorials with no Skip button, such as the 4-21 boss-warning
lesson, acknowledge native one-shot `WaitTouch` callbacks. Their remaining script
steps clear the overlay and resume combat. This is limited to Continuous Story in
battle and does not dispatch arbitrary widget clicks or long presses.

The patch preserves tutorial script effects and protected cutscene groups. It
also handles 10-23's fixed Crow party: while Continuous Story is active, it
presses Crow's third skill when available through the native skill callback.
The `Clause_Chapter10` scenario at 10-29 and the fixed-party battles at 10-36 and
10-38 also press available native skill buttons when normal Auto is disabled.
Manual play retains its normal controls, and each battle must reach its own
victory condition before advancing.

After 10-29, the run follows the mandatory Chapter X finale through 10-41, then
continues to unlocked 11-1. These substories use their native Easy-mode map
actions and forced parties. Scripted map movement uses the native tutorial
movement flag after chapter/node checks, retaining Chapter X's avatar flow.
Lobby story events at 10-30, 10-39 and 10-41 advance
only after native completion; their own reward popup is acknowledged through its
normal background click. Other popups remain interactive. The player's original
party is retained for Chapter XI. Unlock, inventory, stamina and defeat checks
still apply.
It does not mark stages cleared, auto-purchase stamina, or change saved skill-auto
preferences. Story replay, optional sub-stories, raids, and repeat farming keep their
existing behavior. Full patching includes this feature; to update it alone:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --story-auto-only
```

Add `--stage-only` to write `Assembly-CSharp.dll.story-auto-staged` and
`SprkStoryAuto.dll.staged`; install both together. Installation reads the active
DLL and retains other patches, with recovery copies under
`target/story-auto-backups`. Restart the client and publish a signed client update.

The opt-in automation bridge supports `story-auto-probe`: without text it reports
the running stage, auto state, and tutorial state; `text: "1:1"` opens party setup
for an accessible Normal stage using native chapter/node checks. It does not
create battle results or unlock stages.

The live regression accepts an isolated account with prerequisite progress and
uncleared target stages. For the chapter boundary and next-stage entry regression:

```powershell
python scripts/test_story_auto.py --session <isolated-run> --start 2:21 --expect 2:21,3:1,3:2
```

It verifies real saved first clears and manual cancellation; prerequisite progress
belongs only to the test fixture.

To watch an owned isolated fixture launched normally with the automation bridge
disabled, start the observer before choosing Continuous Story through the UI:

```powershell
python scripts/test_story_auto.py --native-session <isolated-run> --account faber1 --expect 2:20,2:21,3:1,3:2
```

This checks actual battle starts and saved clears, then clicks the native Stop
button in the 1280×720 test window and verifies that no further stage starts.

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
Successful login and registration remember each username/server pair. On subsequent
launches the login selector shows saved accounts, with a per-account **Log Out**
button and **Add Account**. Selecting an account renews its token and enters the
game. Add Account opens the existing login/register form; Back to Accounts returns
to the picker. The saved-account limit is four; Add Account is disabled at the limit and reenabled
when a saved account is logged out. Existing lists from earlier versions with more
than four entries remain accessible for removal; new entries are blocked until
there is space. The picker refreshes across open windows. Accounts currently used
by another window are marked In use and cannot be selected or removed there. First launch and removing the
last account show the login form directly. In-game logout forgets only the active
account; simply closing the game preserves saved logins.
The Others settings tab also has **Switch Account** beside Logout, styled like the
existing blue settings buttons. It clears the loaded game session using native
account/scene cleanup while preserving all saved refresh tokens, then opens the
account picker directly without requiring a title-screen click or server logout.
Logout remains a separate action that forgets the active saved login.

Only refresh tokens are saved, encrypted for the current Windows user with DPAPI
in `Application.persistentDataPath/sprk-accounts.dat`; passwords are never saved.
Entries are scoped to the configured server name and login endpoint. Renewed tokens
are saved before game verification, including renewals by the native client.
Expired/revoked tokens prompt for a password; connection failures retain the saved
account for retry. Picker logout forgets the login on this device; it does not delete
the game account or sign out other devices. Corrupt/unreadable storage falls back
to the login form. Storage failures are reported without claiming a successful save.
Automation profiles use a separate file beneath `SprkAutomation/<profile>`.

Run the Windows storage regression checks with:

```powershell
powershell -NoProfile -File scripts/test_saved_accounts.ps1
```

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
are skipped.
The Change Password button opens a centered form with Current Password, New
Password, and Confirm Password, Show/Hide controls, Tab navigation, Enter submission,
and retained values/focus after errors. The authenticated server endpoint verifies
the current password, atomically changes its Argon2 hash and rotates login tokens.
The current game session stays active; older tickets and other game sessions for
that account are revoked. The encrypted saved login receives the replacement token.
Successful native logout cancels queued lobby requests and clears password-account tokens and pending
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

`LobbyAccessories.cs` also runs in account and standard patch modes. The lobby
avatar keeps the selected portrait costume and uses the hero's equipped accessory
slots with their current position, rotation and scale. Accessory changes refresh
that avatar; chapter-forced story avatars remain under native control. The server
includes `PositionInfo1` through `PositionInfo4` in avatar selection and login
responses so the native avatar snapshot retains saved accessory transforms.

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

## First battle confirmation after launch

The native campaign entry throttle treats a zero request timestamp as a request
at process startup. Confirming Auto Repeat before its ten-second clock expires
can close the notice without entering battle. The focused fix bypasses that
comparison when no request has been sent, preserving the existing guard after a
request and the native Select Battle/confirmation popups.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --battle-entry-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --self-test-battle-entry
dotnet run --project scripts/DllPatcher -- ../sprk-client --battle-entry-only
```

Stage output is `Assembly-CSharp.dll.battle-entry-staged`. Installation keeps a
recovery copy under `target/battle-entry-backups`. Full patching also applies this
fix. Close the client before installation, restart afterward, and publish a new
signed update. The self-test executes the patched IL with controlled startup and
duplicate-request clocks and checks idempotence.

## Awakening animation transition

Remove the fixed three-second purification-room pause before the full-screen hero
awakening animation:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --awaken-delay-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --awaken-delay-only
```

The shared awakening/transcendence animation transition yields one frame, then
opens the native cinematic. Upgrade requests, material and currency updates,
asset-loading waits, and completion callbacks remain intact. The patch also runs
during full patching. Staging writes `Assembly-CSharp.dll.awaken-delay-staged`;
installation saves a recovery copy under `target/awaken-delay-backups`. Close the
client before installation, restart afterward, and publish a signed client update.

## Mission and achievement completion banners

Disable the informational completion banners, including hero/general achievements
and daily/weekly/limited missions:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --completion-banners-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --completion-banners-only
```

The patch disables legacy achievement banners and the newer `ConfirmNotiPopup`
used for main/submission completion and new-mission announcements. It suppresses
their render entrypoints and notification reservations so these banners cannot
delay an awakening cinematic queued behind them. Pending-notification lists still
drain normally. Progress tracking, mission screens, content unlocks, reward claims,
and reward dialogs retain their native behavior. Full patching includes this change.
Installation backs up the active DLL under `target/completion-banners-backups`; staging writes
`Assembly-CSharp.dll.completion-banners-staged`. Close the client before installing,
restart afterward, and publish a signed update.

## Cinematic Skip confirmation

`--cinematic-skip-only` makes video Skip invoke its original Yes action directly,
without opening the confirmation popup. It applies to manual skipping and
Continuous Story, retaining native video cleanup and tutorial completion.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --cinematic-skip-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-cinematic-skip
```

Staging writes `Assembly-CSharp.dll.cinematic-skip-staged`. Remove `--stage-only`
to install with a backup under `target/cinematic-skip-backups`. Restart the client
and publish a signed update after installation. Full patching includes this change.

## Multiple game windows

Run `python scripts/enable_multiple_windows.py ../sprk-client --install` to disable
both Unity single-instance settings (PlayerSettings and boot.config). The script
validates the known serialization prefix, modifies only the one boolean byte,
preserves unknown trailing data, and backs up originals under target. The main
patch script also applies this; default update manifests include both startup files.
Rebuild the launcher too: it opens another window from the existing installation
when a game is running, and defers updates until every game window has closed.

Saved-account changes use a cross-process file lock and reload/merge before writing,
so stale windows cannot overwrite additions/removals or bypass the four-account cap.
A per-account file handle prevents two windows from consuming the same refresh token
or displacing each other's game session. Switching releases the account; process exit
or crash also releases its handle automatically. Different accounts can play at once.
The four-account cap covers saved logins, not empty game windows.

## Special Shop catalog

The focused `--shop-catalog-only` mode connects Hot & New to the reconstructed
server catalog while preserving installed patches. `--stage-only` prepares the
DLL without installing. Catalog configuration and recovery are documented in
[`docs/special-shop.md`](../../docs/special-shop.md).

Use `--accessory-list-only` to install the focused accessory list fix from the
installed DLL. It preserves direct ruby sales and ownership IDs, and filters
repeated IDs in category/tag lists without merging distinct variants.

## Mission category thumbnails

Restored Mission tables contain bare thumbnail names such as `Chapter2_Button`.
The current atlas registers them under `UI/Quest/SubCategory/`. This patch adds
that prefix when the native subcategory image loader receives a bare name,
restoring the Guideline chapter tabs and Growth Hero/Gear/Unique Gear tabs.
Empty and already-qualified paths keep their existing behavior.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --mission-thumbnails-only
```

The focused mode reads the installed DLL, preserves other patches, and backs it
up under `target/mission-thumbnail-backups`. Add `--stage-only` to prepare the
DLL without installing. Full patching also includes the fix. Restart the client
after installation and publish a new signed client update.

### Raid invite presence

`--raid-social-only` updates the raid invitation rows to read live
`CachedUserInfo.IsOnline` whenever their buttons refresh. It preserves the
installed DLL's other patches and saves a backup under
`target/raid-social-backups`. Use `--stage-only` to prepare the DLL, or
`--verify-raid-social` to check the installed patch. Full patching includes it.
Restart clients after installation and publish a signed client update.

The matching server implements `InvitePartyReq` for individual, Friend and
Guild recipients, synchronizes presence on socket login and membership changes,
and serves Hero Info through authenticated `user/inspect_user` snapshots.

### Co-op raid hero limits

`--raid-coop-limits-only` updates the installed native RaidTable and its integrity
metadata while preserving other rows and rebuilding every lookup cache with
the game's serializer. It sets Hero Suppression and Hero Protection to three
heroes per player, and Dark Legion and all three Technomagic bosses to four,
across every stage. Total party limits remain six and seven respectively;
solo layouts are preserved. Two players can fill the party with 3+3 or 4+3.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --raid-coop-limits-only --stage-only
```

Remove `--stage-only` to install with clients and shared-asset workers closed.
Staging and timestamped backups are under `target/raid-coop-limits`. The tool
verifies all native row properties and keyed lookups after readback. Reapplying
the limits preserves the installed table bytes. Keep the matching
`DeckCountPerPlayer` values in `tables/BattleSupport.json`, restart the server,
and generate a signed client update after installation. Class rules, party
totals, and earned unlocks still apply.

### Arcdim co-op readiness and wave teams

The server accepts an Arcdim guest's ready message when that guest owns heroes
only in the second wave team. Co-op Arcdim 401/402 uses four heroes per player,
four per wave, and eight total. Readiness and cancellation are broadcast over
the native party socket. Battle entry preserves authenticated main/sub decks,
and the shared worker separates Delta/Zeta rosters using `GroupHeroInfos`.
Both wave teams use battle area 0; a wave team is distinct from a battle area.
The worker registers both rosters with the native wave controller, emits
`SetNextTeamNot` between stages, and uses native per-player creature creation
logs after the single dungeon reset so Zeta heroes retain their owners.

Install the matching worker with `--conquest-only` (or stage it first with
`--stage-only`), verify with `--verify-arcdim-worker`, and publish a signed client
update after installation. Preserve the current installed DLL and stop game
clients/shared workers before installation. Both server and worker are required.

### Mail arrival latency

`--mail-updates-only` preserves the installed assembly and adds mailbox socket
wake-ups, a sixty-second fallback, coalesced checks, an in-flight guard and live
refresh of an open inbox. The server queues `NewMailArrived` hints transactionally
for every inserted mail and publishes committed hints every 250 ms; failed reward
transactions never announce mail. Native claim/attachment behavior is preserved.
Idle mailbox reads skip progression snapshots and avoid SQLite write locks when
there is no global mail to deliver. Attachment claims still update missions.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --mail-updates-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --self-test-mail-updates
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-mail-updates
```

Stage output and installation backups are under `target/mail-delivery-fix`.
Close clients and shared-asset workers, remove `--stage-only` to install, restart
the server/client and publish a new signed update after installation.
