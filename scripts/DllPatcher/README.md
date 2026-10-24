# DLL patcher

For routine fixes, modify the patcher source and commit it. CI invokes the single
`--build-release` entry point, which runs the full patch path, companion helpers,
and verification before signing. There is no separate JSON patch recipe to edit.
Wire new patches into the patcher's normal full build code, just as for local
patching; fixes to an existing patch need no additional registration in CI.
`--build-release` is for a fresh disposable build workspace, not an installed
client. Focused installation commands below remain available for local changes.

## Telemetry cleanup

Install the asset dependency once with
`python -m pip install -r scripts/client/requirements-client.txt` (.NET 8 is also required).
`scripts/client/patch.ps1` and the Python CI release builder automatically run the complete
cleanup. The DLL patcher alone removes managed telemetry code; the Python wrapper
also removes SDK files, serialized telemetry scripts/prefabs, startup hooks, Unity
service destinations and cloud identifiers.

```powershell
python scripts/client/remove_client_telemetry.py ../sprk-client --stage-only
python scripts/client/remove_client_telemetry.py ../sprk-client
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-no-telemetry
```

The first command validates without installing. Recovery files and reports stay
under `target/telemetry-removal/`, outside the client. Cleanup preserves existing
privacy deletion history and is safe to rerun. Close the game before installation;
after changing installed files, publish a signed update using `docs/client-updates.md`.
Rerun cleanup after restoring original files; `patch.ps1 -Restore` does not undo
the full privacy cleanup.

Settings changes that reload the game preserve saved accounts. Native session
cleanup synchronizes any renewed refresh token and releases the account lease;
only confirmed Log Out or account deletion forgets the active saved account.
Install with `--accounts-only`, then check the installed routing with
`--verify-account-session`. Settings such as texture quality and sound remain
local Unity PlayerPrefs shared by accounts under the same Windows user.

Saved settings are reapplied once when the UI starts, even on clients whose
legacy `SingletonCreator.InitializeOnStartup` was disabled. The original option
initializer restores the FPS limit and rendering options; legacy battery-mode
migration only runs when no saved FPS preference exists. User option changes and
texture changes flush PlayerPrefs to disk.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --settings-persistence-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-settings-persistence
python scripts/check_settings_persistence.py --client ../sprk-client
```

The real-client check launches and closes three test windows using a new isolated
preference profile. It verifies saved and applied 30/60 FPS limits, measured frame
rates, texture quality, BGM volume, outline, and preservation of the FPS preference
when an old battery-mode value exists. It requires a graphics device. Normal player
preferences are untouched. Publish a signed client update after installation.

Recommended Friends samples the server's full eligible player population, including
offline players regardless of last logout time. The client preserves the random
server order and fetches a fresh sample whenever Accept/Request opens. Existing
friends and pending invitations remain excluded; the server bounds the response.

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --friend-recommendations-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-friend-recommendations --stage-only
# Close King's Raid, then install and publish a new signed client update.
dotnet run --project scripts/DllPatcher -- ../sprk-client --friend-recommendations-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-friend-recommendations
dotnet run --project scripts/DllPatcher -- ../sprk-client --self-test-friend-recommendations
```

Battlefield of Ordeals accepts the account's tutorial-skip setting as well as
completion of its configured League of Victory introduction (normally 2130).
The account guards and seasonal availability checks remain native. Apply this
focused change to an already patched client with:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --ordeal-unlock-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-ordeal-unlock
```

`--stage-only` prepares/verifies the DLL without installing it. Full adventure
unlock patching also includes this gate; signed publishing is required after
installing the client change.

| File                         | Responsibility                                                                                        |
| ---------------------------- | ----------------------------------------------------------------------------------------------------- |
| `HeroInn.cs`                 | Loads Unity references and coordinates hero inn patches                                               |
| `HeroInnRecruiting.cs`       | Recruiting button selection                                                                           |
| `HeroInnPortraits.cs`        | Portrait array bounds checks                                                                          |
| `HeroInnButtons.cs`          | Hides unsupported hero inn actions                                                                    |
| `Payment.cs`                 | Payment initialization compatibility                                                                  |
| `SoulWeapon.cs`              | Disables unsupported limit-break stars                                                                |
| `HeroLevels.cs`              | Restores Pre-Doomsday hero growth caps and removes hero Limit Break                                   |
| `HeroStats.cs`               | Restores build-1558 Soul Weapon ATK/HP units and tests the installed calculator                       |
| `AccessorySales.cs`          | Shows all accessories and enables direct ruby purchases in the Dressing Room                          |
| `LobbyAccessories.cs`        | Uses equipped accessories and their transforms for the lobby avatar, and refreshes accessory edits    |
| `Campaign.cs`                | Campaign survivor filtering and its self-test                                                         |
| `Chat.cs`                    | Chat session and background repairs                                                                   |
| `Shop.cs`                    | Shop shortcut, opening, and category visibility                                                       |
| `HeroVisibility.cs`          | Hero availability                                                                                     |
| `CostumeVisibility.cs`       | Costume browsing, previews, and motion button                                                         |
| `CosmeticSales.cs`           | Cosmetic sale availability and fallback prices                                                        |
| `CostumeOwnership.cs`        | Restores native ownership checks from embedded source                                                 |
| `PatchHelpers.cs`            | Shared IL helpers                                                                                     |
| `TelemetryRemoval.cs`        | Deletes telemetry types/call sites and removes telemetry SDK references                               |
| `DealerTicketPopup.cs`       | Blocks all Recommended ticket promotion eligibility and popup opening                                 |
| `AppReview.cs`               | Disables automatic and forced app-store review prompts                                                |
| `TrialHeroReward.cs`         | Prevents hero recruitment animation after awakening/transcendence trial rewards                       |
| `AdventureUnlocks.cs`        | Honors Portal tutorial skip and saved automatic dungeon milestones for mission categories             |
| `StoryAuto.cs`               | Opt-in campaign progression through required story gates, native story skipping, and automatic skills |
| `VoiceLanguage.cs`           | Keeps the 4K screen-resolution filter from hiding Japanese character voices                           |
| `ShopCatalog.cs`             | Connects Hot & New to featured products delivered by the server                                       |
| `AccessoryShopDuplicates.cs` | Removes repeated accessory IDs from Dressing Room shop lists                                          |

`Resources/CostumeOwnership.cs.txt` is embedded with the stable resource name
`DllPatcher.CostumeOwnership.cs.txt`; it is compiled by the ownership patch at runtime.
The SDK automatically includes the C# files in `Patches/`.

Run from the server directory:

`--honor-only` preserves the installed assembly and repairs the Honor tier reward
panel when a historical weekly box references a missing reward row. The panel
hides that unavailable box and continues showing the ranks and other rewards.
`--stage-only` prepares `Assembly-CSharp.dll.honor-staged`;
`--verify-honor` validates the installed hook (`--stage-only` selects staging).
Installation also checks repeat application and IL control flow. Honor combat
and its native ban/pick protocol run on the server; see
[League of Honor](../../docs/league-of-honor.md).

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

`--runes-only` restores the native Lucern/Punishment Rune category in Runehenge
alongside Normal Rune (which also includes Velkazar runes). It preserves the
installed client patches. `--stage-only` writes `Assembly-CSharp.dll.runes-staged`;
`--verify-runes` checks both categories and idempotence, also with `--stage-only`.
The same patch preserves crafted rune options when native battles copy a
CachedHeroInfo without explicitly passing its rune rolls. Server crafting accepts
native repeated ingredient fields; remote hero and battle snapshots carry rolls.
See [rune restoration](../../docs/runes.md) for validation and release details.

`--hero-stats-only` restores the Soul Weapon ATK/HP arithmetic used with the
build-1558 tables. Those tables store coefficients 7/8/9 and 250; later Windows
arithmetic divides their results by an extra 1000. The focused installer
validates the baked table hashes and preserves existing client patches.
`--stage-only` writes `Assembly-CSharp.dll.hero-stats-staged`.
Run `--self-test-hero-stats <dll>` with the DLL path as the first argument to
check archived native outputs, progression/bonus combinations and idempotence.
See the [stat audit](../../docs/hero-stat-audit.md) for provenance and coverage.

`--orvel-visuals-only` removes the floating EVENT effects, red event portal,
and obsolete hero-dungeon glow over Central Neighborhood from Orvel by
filtering their prop records before native asset creation. Town
labels, other markers and other chapters retain their existing props. It
preserves the installed client patches; `--stage-only` prepares the DLL and
`--verify-orvel-visuals` checks the hook (also supports `--stage-only`).

`--conquest-only` also releases the native online solo Conquest party when the
result screen's Lobby button is pressed. This prevents stale solo rooms from
blocking invitation popups after a battle. Cooperative rooms, battle repeats,
and returns to formation or inventory retain their native behavior.
`--verify-conquest-party-cleanup` checks the installed hook; add `--stage-only`
to check its staged output before installation.

The focused Conquest installer also caches server session definitions from
battle entry and result responses. Invited guests can then resolve the boss
when returning to the lobby without first opening the Conquest board.
`--verify-conquest-session-cache` checks both parser hooks; `--stage-only`
selects the staged assembly.

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

### Chat channel selection

`dotnet run --project scripts/DllPatcher -- ../sprk-client --chat-channels-only`
preserves installed patches and fixes the Change Channel dialog: custom channels
use 1–99, reopening shows the current channel, and the numeric input supports
Backspace, Delete, caret movement and clipboard shortcuts through native NGUI
editing. The language-channel option keeps its existing behavior. The focused
patch installs `SprkChatChannels.dll` and hooks the installed `Assembly-CSharp.dll`.
`--stage-only` prepares both files without installing them;
`--verify-chat-channels` checks the installed hooks (add `--stage-only` for the
staged assembly). Close all clients before installation, then publish a signed
client update as described in `docs/client-updates.md`.

`--chat-send-only` installs the same focused chat controls with a **Send** button
to the left of **Share** in Channel, Guild, Party and Whisper chat. Send submits the
existing text input, preserving native recipients and cooldowns; Share retains
its gear-link action. Empty or unavailable inputs disable Send. Use
`--verify-chat-send` to check the installed hooks and IL control flow.

### Mission Claim All

`--mission-claim-all-only` preserves installed patches and enables the native
Claim All header control in Growth, Battle, Raid, Arena, Story, Guild and other
node-based mission categories. Existing list-based and Guideline controls remain
available. Claims use the selected subcategory's native reward request, currency
cap warning, reward popup and refresh. The button stays visible and disables when
the selected page has no eligible rewards, is locked or has expired.

The focused installer adds `SprkMissionClaimAll.dll` and two header hooks to
`Assembly-CSharp.dll`, checking IL control flow and repeat installation. Add
`--stage-only` to prepare both files with `.mission-claim-all-staged` suffixes;
`--verify-mission-claim-all` checks the installed hooks and native bulk requests
(also supports `--stage-only`). Close clients before installation, restart after
installation, and publish a signed client update using the release workflow.

### Continuous Story cancellation

`--story-auto-only` also fixes stopping immediately after selecting Continuous
Story. Cancellation immediately disarms the queued start, clears the party's
auto-start timer and cancels the captured AutoNext option for an in-flight
request. If the native Stop overlay is still opening, its pooled window is
released after its opening task and input wrapper finish. Other native repeat
modes retain their behavior. Native close cleanup also completes before the
overlay returns to its pool, so its Back-stack registration and observers are
removed. Use `--verify-story-auto` to check installed hooks
and `--self-test-story-auto-cancel` to exercise the shipping cancellation source
with controlled native lifecycle and clock stand-ins. Close the game before
installing and publish a signed update afterwards.

The opt-in automation bridge supports `ui-navigation-probe` to inspect the native
Back stack and input-block owners. `click-and-stop-story` clicks a specified
Continuous Story button and immediately invokes the native Stop action in the
same update, for reproducible cancellation testing without a battle request.

### Soul Weapon unlock cinematic

`--soul-liberation-cost-only` restores the gold requirement on the initial
Unleash screen using the selected weapon's native upgrade-price table. A native
gold icon and formatted price appear above the button; insufficient gold is red.
The display updates on selection and balance changes and releases its pooled
icon when closed. The focused installer preserves other patches and installs
`SprkSoulLiberationCost.dll`. Use `--stage-only` to prepare the files or
`--verify-soul-liberation-cost` to check the installed hook and price source.
Close the client before installation and publish a signed update afterwards.

The full native unlock cinematic must remain enabled. Release verification checks
the waiting sequence, Kain action sequence, hero-specific liberation sequence,
fade/audio callbacks, native stat/skill result panel, and dismissal. Do not replace
the sequence with an immediate result panel to avoid a crash.

`--restore-soul-weapon-cinematic --source <original-assembly>` restores only
`SetNodeTreePlay` from a compatible original or pre-bypass assembly, preserving
other installed patches. Nested callbacks must already match the source.
Use `--stage-only` to stage the DLL and `--verify-soul-weapon-cinematic` to verify
the installed or staged cinematic. Publish a signed client update after installing.

### Inventory selector quantity limit

`--selector-count-limit-only` caps the shared item selector's Max button,
plus button, typed quantity and OK validation at the lower of owned boxes and
1,000 per use, matching the server limit. The inventory icon retains the full
owned count. Use `--stage-only` to prepare the patch,
`--verify-selector-count-limit` to verify installed/staged controls, and
`--self-test-selector-count-limit` for boundary and repeat-install checks.
Close the game before installing, then generate a signed client update.

### Shop purchase limits

`--shop-purchase-limits-only` fixes the shared multiple-buy popup treating
purchase-history entries for unlimited items (`PurchasableCount <= 0`) as
remaining-stock limits. This affects guild and other NPC shops. Affordability,
positive purchase limits, rotating stock and per-transaction caps retain native
behavior. The focused installer preserves other installed patches and requires
the game to be closed. Use `--stage-only` to prepare the DLL first,
`--verify-shop-purchase-limits` to verify it (also supports `--stage-only`), and
`--self-test-shop-purchase-limits` to execute the production IL guard with
unlimited/limited stock and missing/present history. Publish a signed update
after installation.


### Free preview camera

See Motions (HeroDetailView.OpenOnlyViewMotion), the standalone HeroModelView, and Dressing Room (HeroCostumeView) support desktop model-area controls: wheel to zoom, left-drag to orbit (drag right increases yaw; drag up decreases pitch), right/middle-drag or Shift-left-drag to pan, and R to reset free-camera offsets. Drags must begin over the model texture; menu buttons and text input do not start camera movement. Native mobile touch controls, preview animations and zoom buttons are preserved. Free-camera state resets when a window is recycled or its controller changes.

Use `--free-camera-only --stage-only` to compile and stage the helper and hooks; `--verify-free-camera --stage-only` verifies the staged assembly. The focused installer preserves existing patches. The full `--build-release` path includes installation and verification. `python scripts/run_free_camera_probe.py` runs isolated Unity checks of orbit axes/limits, zoom limits, reset, native camera transform application and pointer ownership. Native pointer-event interaction on the two actual screens still requires a play check; the isolated probe does not establish that scene-specific colliders route input correctly. Close game clients and workers before installation, then generate and verify the signed client release required by AGENTS.md.


Graphics settings use the native GameOptionToggle4Component checkbox row rather than a cycling button. AA exposes Off/FXAA/SMAA/MSAA/TAA; render scale exposes all five 100-200 percent choices in one row. Choices remain exclusive and preserve the Screen/Resolution/AA/render-scale order. `python scripts/client/verify_graphics_options.py` checks every selection, exclusive checkmarks, refresh, duplicate repair and ordering in Unity. HeroDetailView free camera is limited to Animation/None tabs so skill and cinematic tabs preserve native behavior.




Temporary AA comparison hotkeys and their popup have been removed. Change AA presets through Settings. The old preview hook remains inert for compatibility with previously patched clients.

The native Max Frame row exposes 30 / 60 / 144 / Auto / Auto+ on all display refresh rates. The 144 choice uses EFrameControl.Manual_144 and native settings persistence; fixed frame modes disable V-Sync. The fifth native-style toggle is reused across refreshes. Validate the staged helper and native FPS setters with python scripts/client/verify_lobby_graphics.py in a fresh isolated client. This verifies the requested FPS target, not sustained gameplay performance.

Lobby AA includes the orthographic Bottom3D model camera and the preceding background cameras in the shared scaled world target. Main UI cameras remain at native resolution. TAA uses the native TAA implementation for both orthographic and perspective model cameras. The native package includes orthographic projection jitter; do not force SMAA solely because a camera is orthographic. Methods are exclusive: MSAA uses supported 2x/4x/8x targets without a post-process AA pass, while FXAA/SMAA/TAA use single-sample targets. Unsupported TAA is disabled for that camera and reported through the helper status; no other method is silently substituted. The previous On setting migrates to TAA, Off stays Off, and render scale is preserved. A command buffer copies the finished world texture into the first subsequent UI camera target, after Unity binds and clears that target. The world stack ends at the final model camera; trailing map-label cameras retain native rendering. This avoids both an empty overlay erasing the world and a disabled trailing camera preventing presentation. World PostProcessLayer instances disable finalBlitToCameraTarget so their results remain in the shared texture. Owned preview textures update size and MSAA in place on the next LateUpdate, preserving UI bindings and camera pose. Native base dimensions prevent repeated scale changes from compounding. Only helper-created camera targets are resized; arbitrary native targets and UI textures are excluded. The isolated lobby graphics probe checks live preset/scale switching on one perspective/orthographic preview target, unchanged camera pose, all presets, scaled targets, UI exclusion, nonblank presentation and pixel differences.

Battle AA treats the perspective BattleUI-layer camera as a native overlay, not a model camera. Composite the scaled world before that HUD camera renders; leave its damage and skill overlays at native resolution. BattleBackgroundUI joins the background stack. The GlobalCameraManager.HasMainRenderTarget hook recognizes only our redirected screen cameras as still belonging to the display, preserving native enable/depth/clear handling through scene changes and captures. The Unity regression covers this perspective HUD layout for all five AA methods and verifies native camera-manager classification.

The AA method row and the native Texture choices share the Screen row option-area center. Texture alignment is measured in row coordinates against AA, accounting for the different native prefab origins. Explicit positions survive native UIGrid refreshes; the Texture hook runs after native SetToggle and preserves its values and callbacks.

The graphics installer changes the native Outline preference initializer to Off. Existing saved Outline choices remain authoritative; the preference key and native getters/setters are preserved and the default is verified on staged and installed assemblies.

Texture displays Low / Medium / High. The middle label is presentation-only; native texture values and persistence are unchanged.

Method-specific adjustment rows appear directly below Anti-aliasing: FXAA quality (Fast/Standard), SMAA quality (Low/Medium/High), MSAA samples (supported 2x/4x/8x), or TAA sharpness (Low/Medium/High = 0/0.5/1). Off hides all four. Native UIGrid compacts hidden rows. Defaults are TAA, Standard FXAA, High SMAA, 4x MSAA (or the nearest supported level), and Medium TAA sharpness. Each adjustment is saved independently, with TAA jitter/history blends retained at the tested fixed values. The isolated validator checks all choices, live adjustment application, active native layout without gaps, migration, rendered pixel differences and persistence across a fresh Unity process.

The My Info presentation hook hides only WidgetAvatar/Sprite_TextureDeco, the legacy Bg_Deco backdrop behind the hero and pet portraits. Individual portrait textures, gold card frames and selection buttons are preserved.
