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

See Motions no longer chooses a random animation when the model texture is clicked. The focused free-camera installer disables HeroDetailView.OnTouchForRandomAnimation, which is bound to the preview's click event, while preserving press/release camera controls and explicit animation-button playback. The full release path verifies both behaviors, and the Unity probe dispatches the real preview click event with a populated animation list.

Use `--free-camera-only --stage-only` to compile and stage the helper and hooks; `--verify-free-camera --stage-only` verifies the staged assembly. The focused installer preserves existing patches. The full `--build-release` path includes installation and verification. `python scripts/run_free_camera_probe.py` runs isolated Unity checks of orbit axes/limits, zoom limits, reset, native camera transform application and pointer ownership. Native pointer-event interaction on the two actual screens still requires a play check; the isolated probe does not establish that scene-specific colliders route input correctly. Close game clients and workers before installation, then generate and verify the signed client release required by AGENTS.md.


Graphics settings use the native GameOptionToggle4Component checkbox row rather than a cycling button. AA exposes Off/FXAA/SMAA/MSAA/TAA; Render Scale exposes five shared 100-200 percent presets plus Custom. World Scale and Preview Scale are indented child rows, each offering 100/125/150/175/200 percent. Choices remain exclusive and preserve the Screen/Resolution/AA/render-scale order. `python scripts/client/verify_graphics_options.py` checks every selection, exclusive checkmarks, refresh, duplicate repair and ordering in Unity. HeroDetailView free-camera input and its fixed orbit stay enabled across Animation, Cutscene, Costume and Skills tabs, preserving the current orbit, pan and zoom when switching tabs.




Temporary AA comparison hotkeys and their popup have been removed. Change AA presets through Settings. The old preview hook remains inert for compatibility with previously patched clients.

The native Max Frame row exposes 30 / 60 / 144 / Auto / Auto+ on all display refresh rates. The 144 choice uses EFrameControl.Manual_144 and native settings persistence; fixed frame modes disable V-Sync. The fifth native-style toggle is reused across refreshes. Validate the staged helper and native FPS setters with `python scripts/client/verify_graphics_options.py` in a fresh isolated client. This verifies the requested FPS target, not sustained gameplay performance.

Lobby AA includes the orthographic Bottom3D model camera and the preceding background cameras in the shared scaled world target. Main UI cameras remain at native resolution. TAA uses the native TAA implementation for both orthographic and perspective model cameras. The native package includes orthographic projection jitter; do not force SMAA solely because a camera is orthographic. Methods are exclusive: MSAA uses supported 2x/4x/8x targets without a post-process AA pass, while FXAA/SMAA/TAA use single-sample targets. Unsupported TAA is disabled for that camera and reported through the helper status; no other method is silently substituted. The previous On setting migrates to TAA, Off stays Off, and render scale is preserved. A command buffer copies the finished world texture into the first subsequent UI camera target, after Unity binds and clears that target. The world stack ends at the final model camera; trailing map-label cameras retain native rendering. This avoids both an empty overlay erasing the world and a disabled trailing camera preventing presentation. World PostProcessLayer instances disable finalBlitToCameraTarget so their results remain in the shared texture. Owned preview textures update size and MSAA in place on the next LateUpdate, preserving UI bindings and camera pose. Native base dimensions prevent repeated scale changes from compounding. Only helper-created camera targets are resized; arbitrary native targets and UI textures are excluded. The isolated lobby graphics probe checks live preset/scale switching on one perspective/orthographic preview target, unchanged camera pose, all presets, scaled targets, UI exclusion, nonblank presentation and pixel differences.

Battle AA treats the perspective BattleUI-layer camera as a native overlay, not a model camera. Composite the scaled world before that HUD camera renders; leave its damage and skill overlays at native resolution. BattleBackgroundUI joins the background stack. The GlobalCameraManager.HasMainRenderTarget hook recognizes only our redirected screen cameras as still belonging to the display, preserving native enable/depth/clear handling through scene changes and captures. The Unity regression covers this perspective HUD layout for all five AA methods and verifies native camera-manager classification.

The AA method row and the native Texture choices share the Screen row option-area center. Texture alignment is measured in row coordinates against AA, accounting for the different native prefab origins. Explicit positions survive native UIGrid refreshes; the Texture hook runs after native SetToggle and preserves its values and callbacks.

The graphics installer changes the native Outline preference initializer to Off. Existing saved Outline choices remain authoritative; the preference key and native getters/setters are preserved and the default is verified on staged and installed assemblies.

Texture displays Low / Medium / High. The middle label is presentation-only; native texture values and persistence are unchanged.

Method-specific adjustment rows appear directly below Anti-aliasing: FXAA quality (Fast/Standard), SMAA quality (Low/Medium/High), MSAA samples (supported 2x/4x/8x), or TAA sharpness (Low/Medium/High = 0/0.5/1). Off hides all four. Native UIGrid compacts hidden rows. Defaults are TAA, Standard FXAA, High SMAA, 4x MSAA (or the nearest supported level), and Medium TAA sharpness. Each adjustment is saved independently, with TAA jitter/history blends retained at the tested fixed values. The isolated validator checks all choices, live adjustment application, active native layout without gaps, migration, rendered pixel differences and persistence across a fresh Unity process.

The My Info presentation hook hides only WidgetAvatar/Sprite_TextureDeco, the legacy Bg_Deco backdrop behind the hero and pet portraits. Individual portrait textures, gold card frames and selection buttons are preserved.

Lobby Camera Sensitivity and Model Camera Sensitivity in Game Options each offer 50% / 75% / 100% / 125% / 150%, default 100%. The Model Camera Sensitivity multiplier applies immediately to free-camera orbit/pan drags and mouse-wheel zoom in supported model preview windows; resetting the camera preserves sensitivity.

Free-camera orbit uses the hero RenderFacade body bounds center, with a native target fallback while the model loads. Native framing offsets rotate and scale with the camera, preserving the right-side Dressing Room composition and initial/reset view. The projection regression checks centered and off-center framing across yaw, pitch and zoom using a real native CinemaCamera.

Model previews update the cinema camera aspect from the UITexture's displayed drawing area and UV rectangle each frame. This preserves model proportions on ultrawide fullscreen displays even when the render texture retains the startup resolution. The Unity probe covers 16:9, ultrawide, 32:9, resolution transitions and cropped UVs against a stale 16:9 render target, plus orbit framing at 16:9 and ultrawide.

Resolution choices restore the declared UHD toggle after native screen-mode callbacks and row initialization. The native width check otherwise hides it on displays below 3840 pixels and leaves it hidden when returning to Window. Fullscreen still uses the desktop resolution and native preset availability rules. The graphics probe's `SPRK_GRAPHICS_RESOLUTION_ONLY=1` mode checks repeated Full/Window callbacks and row rebuilds on the shipped control prefab.

Resolution also offers Custom in the same five-choice row. In Window mode it enables the Windows resize border and maximize button, remembers the last normal client size, and restores it when returning from a fixed preset, fullscreen or a restart. HD/FHD/QHD/UHD retain their native setters; fullscreen retains desktop sizing. `EResolutionControl.Custom` uses value 5 and the native resolution preference; custom dimensions use the isolated preference adapter. Only the current process's Unity window is modified, and the original resize flags are restored for fixed presets. The opt-in graphics probe mode `SPRK_GRAPHICS_CUSTOM_WINDOW_ONLY=1` checks native resizing at 1493×821 and 1137×777, menu refreshes, fixed-preset/fullscreen transitions and saved selection. A second run with the same isolated profile and `SPRK_CUSTOM_WINDOW_RESTART=1` verifies size and border restoration after restart.

On Windows, the shared resolution callback bypasses window presets whenever Full is selected. Settings initialization and parent/child row synchronization must not resize borderless fullscreen to a saved 16:9 preset. `SPRK_GRAPHICS_FULLSCREEN_ONLY=1` checks all five native resolution setters, repeated settings callbacks, desktop fullscreen dimensions, windowed HD and a fullscreen round trip. It uses the existing isolated graphics probe profile.

Heroes List (HeroDeck2), supported model previews, lobby menus and windows whose root spans an unclipped screen panel refresh their native layout anchors on first display, resolution changes and camera replacement. Disabling a cached window invalidates its layout state; three settling frames allow native initialization and root scaling to finish before caching the result. This includes first-start-only anchors in the root and descendants, letting backgrounds, frames, scroll viewports and edge controls expand horizontally without scaling portraits or models. Lobby menus refresh their complete hierarchy because their edge controls can anchor directly to a screen panel instead of a stretchable root widget. Fixed-width popup roots are excluded. The existing BaseWindow.UpdateDerived hook and verified OnDisable invalidation run in focused and full release builds. The Unity probe checks shipped lobby, party/raid, hero-list and preview prefabs at 16:9, 3440×1440, 32:9 and back, plus recycled layout state and popup exclusion.

The independent Lobby Camera Sensitivity preference scales native desktop lobby/world-map drag and wheel zoom at WorldMapMode.DragWorldMap / ZoomWorldMap. Native movement bounds, chase/focus transitions, menu scrolling and mobile touch handling are retained; sensitivity is applied exactly once at each camera-input consumer. The Unity probe invokes the real native methods at all five percentages.

Both camera controls inherit the previous shared sensitivity when their own value has not yet been saved, and otherwise default to 100%. Each selection is persisted independently. The settings labels use Anti-Aliasing, FXAA Quality, SMAA Quality, MSAA Samples, TAA Sharpness and Render Scale.

Graphics camera discovery uses a reusable `GetAllCameras` buffer, stable depth sorting and cached layer references. Unchanged frames retain their world bindings, presentation command and target descriptors. At 100% with AA Off, world cameras render directly without the helper's world texture or presentation copy. Helper-created post-processing layers disable themselves when AA is unused; native layers retain their effects. Unity's global MSAA setting must stay enabled for visible multisampling in this player, while display UI cameras explicitly disable `allowMSAA`. The shared `FilterBase` texture factory stays native; only HeroPreview, ItemPreview, RenderTargetCinema and Cinema calls opt into scaled model textures. Screen filters, captures and portal UI effect textures retain native sizing and samples.

`python scripts/client/verify_graphics_options.py` stages the focused patch and runs isolated Unity checks of all 25 AA/scale combinations, rendered AA differences, live adjustments, direct-render transitions, UI presentation, disabled trailing cameras, stable targets and native effect preservation. A second Unity process checks preference persistence. Use `--installed` to validate the installed DLLs instead. The probe copies managed assemblies, uses a unique preference profile, retains screenshots/logs under `target/graphics-performance-validation`, and records helper CPU tick timings separately from gameplay/GPU performance. Screen capture uses an AfterEverything command buffer to inspect the rendered fixture rather than stale startup artwork.

Graphics integer/string preferences and existence checks are cached in `SprkTestPrefs`, scoped to the graphics/render setting families and the complete isolated-profile key. Setters, deletion and focus regain invalidate cached values. Different fallback values retain native getter semantics. The graphics focused installer stages and installs the matching automation adapter together with its helper; the full release path uses the same integration. Fixed-window style checks are throttled, with immediate refresh on Custom-mode changes; custom size tracking and saving remain live.

Leaving TAA releases its history textures and removes only the depth/motion-vector bits it added to a camera. Pre-existing depth flags remain; enabled native post-processing layers calculate their own current requests during rendering. Cleanup also runs when cameras leave the managed set or the helper disables. The verifier checks the shipped native history-release signature. The Unity probe renders TAA-to-FXAA/SMAA/MSAA/Off transitions with both empty and pre-existing depth requirements, checking history release and retained flags.

Render Scale is the shared parent: 100%, 125%, 150%, 175%, 200% or Custom. A percentage applies to both effective World Scale and Preview Scale; the child rows use the native ChildRoot indentation/arrow and SetGrayScale behavior and reject input unless Custom is selected. Disabled children show the effective shared percentage. Custom restores their independently saved values. The parent has six checkbox choices, while both children have five. The children refresh existing selection delegates instead of rebuilding their onChange lists during event dispatch.

The new `Sprk.Graphics.RenderScale` preference stores 0 for Custom or the shared percentage. Existing `Scale` and `PreviewScale` keys retain custom child values. On first use, matching legacy world/preview values become a shared preset; different values become Custom. Legacy Follow World resolves to the old world percentage. Values are normalized to the new 25-point choices, with legacy 75% previews becoming 100%. All controls apply live and persist across restart.

World Scale controls the main battle/lobby/world-map scene, including heroes and monsters drawn in that scene. Preview Scale controls dedicated hero/detail/item previews, See Motions, Dressing Room and preview cinematics; its tooltip lists those places and distinguishes battle/lobby characters. Preview targets retain their original dimensions so switches do not compound or change UI texture bindings. Explicitly hidden preview cameras skip camera updates, and hidden facades under HeroPreview, RenderTargetCinema or Cinema skip the final creature visual pass. Native facade action/animation clocks, item-effect lifetime updates and cinematic script updates retain their original calls. Battle facades use the native visual path even when hidden.

Particle Density offers Full (default), Reduced (75%) and Minimal (50%) continuous emission for eligible dense billboard systems. Node ParticleObject and asset ParticleInstance creation/release paths register and restore their emitters. Distance-driven systems, bursts/manual emission, mesh particles, trails, collision/trigger/sub-emitter/light systems, sparse emitters and indicator-like hierarchies retain native behavior. All effect spawning and callbacks remain native. Rates restore on disable/release and recapture from their original values on pooled reuse; active eligible emitters update when the density setting changes. The tests exercise native close/finalize hooks, live changes, pooling and emitted particle counts. Eligibility is conservative and does not cover every visual effect.

Battle Effects offers Full (default), Balanced and Minimal budgets for the same eligible continuous emitters. During battle, original emission, lifetime and particle capacity estimate concurrent population, with a shared 4,800/2,400-particle budget distributed proportionally at 4 Hz. This combines with Particle Density and affects only layers drawn by battle world cameras. Preview layers are excluded. The existing burst/manual/indicator exclusions remain: this does not cap every active effect, skip combat nodes, or alter effect completion callbacks.

Effect Coverage offers Full (default), Reduced and Minimal during battle. Eligible billboard renderers retain their native maximum screen size or cap it at 0.15/0.08; smaller native limits are preserved. This changes the appearance of large decorative particles and restores original values outside battle and on release. It is not a separate low-resolution render pass, which still requires shader/assets work and transparency/depth validation. This patch does not replace shaders, suppress burst effects or prewarm shaders.

Adaptive Battle Scale offers Off (default), 30 FPS, 60 FPS, 120 FPS and 144 FPS targets. Existing saved 60/120 FPS selections retain their meaning. Six fresh GPU samples above 110% of the target budget lower world scale by 25 points; twelve below 70% restore 25 points, up to the selected World Scale. Samples are taken at 4 Hz, with a 75% floor. CPU-dominated samples do not trigger reductions. Missing/stale GPU timings, focus loss and leaving battle restore the selected scale. AA and the frame limit remain independent; the target does not guarantee FPS. Display UI and Preview Scale remain independent of temporary adaptive reductions. Target changes use existing bindings and reset TAA histories.

The graphics installer stages `globalgamemanagers` with its DLLs and changes only the native `enableFrameTimingStats` byte. `FrameTimingPlayerSettings.bin` is the original 808-byte PlayerSettings payload from Unity 6000.0.53f1. Exactly one complete original/already-enabled payload must match; unsupported layouts fail without installation. Every other asset byte is retained. Focused/full release paths verify the setting. GPU timing capture is requested during active adaptive battles. Native validation windows must present frames: hidden/minimized D3D11 windows can return CPU timings with zero GPU timings, which never drive adaptation.

Native circle shadows cache transform reads within each call and omit unchanged XZ position, local height and world rotation writes. Their original pose operations are tested under translated, rotated and scaled parents. Creature action/animation and material-effect updates remain native. Pool dequeue skips destroyed entries and continues to a reusable object instead of returning null at the first dead entry. Particle observation reuses component lists and value-type emitter records; overlapping observers claim each system once and release ownership on cleanup. Existing effect pools remain in use.

`BattleGraphics.cs.txt` compiles into the existing graphics helper; its hooks/verifiers run through the graphics installer in `--build-release`. The Unity probe covers the new setting rows and restart values, crowded emission budgets, preview exclusion, coverage restoration, adaptive hysteresis/bounds/CPU exclusion, world-versus-preview targets, actual GPU timing availability, native shadow poses and destroyed-head pool reuse. Fixture timings and particle counts are not measurements of a real effects-heavy battle or a player's GPU.

Graphics settings provide native question-mark tooltips for all 14 SPRK rows plus Texture, Max Frame, Screen, Resolution and V-Sync. `GraphicsHelp.cs.txt` supplies concise descriptions of scope, quality/performance tradeoffs and interactions. Max Frame describes fixed 30/60/144 FPS targets, Auto's display-refresh V-Sync, Auto+'s scene/content targets and V-Sync precedence; the obsolete mobile 30-60 FPS range and 90 Hz availability claims are removed. Row initialization restores help icons and tooltip colliders. Scoped show/hide hooks use the original `GameOptionTooltip` popup without changing shared option metadata or localization tables; unrelated rows keep their native tooltip behavior. Native checks open all 19 tooltips, verify actual title/body text, show/hide, pooling and unchanged table objects, and export representative screenshots. The same hooks/verifiers run through the graphics installer in `--build-release`.

Run `python scripts/client/verify_graphics_options.py --window-checks` to include native custom resize, saved-size restart, fullscreen and resolution-row regressions. The normal probe also checks saved setting values after restart, cache invalidation/profile isolation, zero steady storage reads, independent preview sizing, visibility scoping and TAA resources. Installation guards distinguish a fresh isolated copy from clients/workers actually sharing the destination's files, including junction-backed worker data directories.

The login menu replaces Language and Character Voice with one Settings button and hides the legacy Check Patch button. It opens the native GameOptionWindow through LoginManagement. The settings table and GameOption atlas load from installed client files without marking patch download complete. Texture and BGM callbacks handle the login context; account and lobby-only actions stay hidden there, with state restored before a pooled panel is opened in-game. The accounts installer owns these hooks, and `--build-release` verifies them with `VerifyLoginSettingsFile`; `--verify-login-settings` also supports `--stage-only`. An isolated Unity run with `SPRK_LOGIN_SETTINGS_TEST`, `SPRK_AUTOMATION_DIR` and `SPRK_AUTOMATION_PROFILE` checks native button cleanup, the panel's 52 rows, artwork, tabs, texture changes, refresh, close/reopen, pool cleanup and preserved download state.

Before constructing the login settings panel, load the installed full localization dictionary and enum redirects. Both GameOption (tooltip/child-row artwork) and PayShop (panel frame, title and close button) atlases must be available before login; their materials use the shipped Unlit/Transparent Colored shader because normal bundle shader preloading has not run yet. The existing shared Common atlas is preserved. The probe rejects raw localization keys, checks both sets of artwork and exports the visible panel with the standalone test's BI overlay disabled.

My Info removes the dark outer window border baked into Common_Fram_Small2 using a cached, window-specific atlas copy. The gold silhouette, original nine-slice dimensions, portrait frames and shared Common atlas remain intact. This is separate from the older portrait-backdrop cleanup. The pet presentation installer includes the correction in focused and full release builds. An isolated Unity launch with SPRK_MYINFO_FRAME_TEST, SPRK_AUTOMATION_PROFILE and SPRK_AUTOMATION_DIR set exports before/after sprite images and checks outer-border removal, unchanged gold pixels, native geometry, atlas isolation and cached reuse.

Model-preview orbit anchors are captured once the model is loaded and refreshed when the model or camera controller changes. The helper removes the native animated-head displacement with the same zoom-in/out blend used by CameraController, so Run and other body motions do not move the camera. Native framing and zoom controls remain active; resetting free-camera offsets preserves the fixed anchor. The Unity probe drives head/root movement across native zoom blends and free-camera orbit/pan/zoom, checks a stationary camera transform, and verifies model replacement refreshes the anchor.

### Special shop ordering and batch purchases

`--special-shop-only` installs the focused patch; add `--stage-only` to prepare it
without replacing installed DLLs. `--verify-special-shop` verifies hooks and runs
stock, affordability, quantity encoding and request isolation checks. The full
`--build-release` path installs and verifies the same patch.

Unclaimed free offers sort before paid offers; claimed free offers sort last.
The current category/tag rebuilds after a free purchase and at the daily reset.
Item detail popups (including package items) include a typed quantity field and minus/plus controls
centered above the native buy button. Total price follows the selected quantity. Maximum
quantity is 999, further bounded by affordability, stock and integer limits.
The request carries `Quantity`; older clients omit it and buy one offer. Deploy
the matching server batch support before distributing this client update.

The quantity helper reads gold, total rubies (free and paid), mileage and Lua
from UserManager, matching the native shop. The verifier executes its balance
accessor against real UserInfo/UserManager instances, including the 549,992-ruby
screenshot regression. Unaffordability disables Buy while retaining the price;
No Sale follows native offer availability. Selector backgrounds use simple
geometry so ornamental sprite borders cannot expand colliders over adjacent
controls.

### Gear inventory capacity

`--inventory-expansion-only` applies the focused 1,400-slot gear cap patch;
`--stage-only` prepares it without installation, and
`--verify-inventory-expansion` executes the actual native row getter across gear,
chest and rune cases. `--build-release` installs and verifies it automatically.
The gear row's 90 extensions become 140, preserving the 280 base slots, 8 slots
per extension and existing ruby price. Both native ticket checks and ruby
expansion pricing read this same row. Other inventory types retain their caps.
The matching server InventorySupport rule uses 140 extensions. Expansion tickets
add their normal amount, truncate the final use to remaining capacity and reject
full inventories or redundant batch tickets without consuming them.

The selector count-limit patch also caps SelectItemMessagePopup (stamina potions
and other shared consumable popups) at 1,000 per use. Max, plus/minus, typed input
and confirmation share the cap, preserving smaller caller limits and full owned
counts on inventory icons. Typed values normalize immediately. Server consumable
validation already allows 1,000; the dedicated hero EXP bulk-use path is unchanged.

### Mail Claim All until inventory fills

The existing `--mail-updates-only` installer now includes automatic Claim All.
One click drains sequential 50-mail batches until empty, blocked by capacity,
cancelled by closing the mailbox, or interrupted by an error/session change.
It applies each response once and shows one combined reward summary. The old
50-mail notice is replaced without changing inventory quantities or mail data.
`--self-test-mail-updates` runs the shipped loop against controlled request and
UI stand-ins, including 1,505 messages, partial capacity, duplicate clicks,
late callbacks, cancellation, timeout and session replacement. The existing
`--build-release` path installs and verifies these same hooks and tests.

Updated clients encode `ClaimUntilFull=true`; the matching server uses a
savepoint per mail, commits earlier successful claims, and leaves every
attachment of the blocked mail untouched. Legacy clients retain their atomic
batch behavior. Deploy the matching server before distributing this client.


### Progressive dispatch results

`--dispatch-only` installs the native simulator helper with progressive uploads.
Each polling pass prepares at most one battle per slot, saves the cumulative
outcome/time prefix, and refreshes native dispatch UI. It resumes from server
prefixes after relogging and retries an unacknowledged prefix before simulating
another run. The matching server anchors time at entry, rejects rewritten
prefixes, prevents collection while any runs remain unprepared, and settles
only completed victories on cancellation. Saved outcomes progress offline;
unprepared runs require a logged-in client to simulate them. Legacy full-batch
reports keep their existing contract. Deploy the matching server first.

`--self-test-dispatch` exercises the shipping coroutine with controlled native
engine/network stand-ins: first result from a 200-run job, upload retry, persisted
resume, slot fairness, timeout and session replacement. `--verify-dispatch` checks
installed hooks and protocol fields. Both run in `--build-release`.


### Honor ranking server buff tokens

`--honor-buff-only` installs a separate activation path for the three Honor
ranking tokens (110033-110035). These items are marked Booster in ItemTable but
have no BoosterItemTable definition; native confirmation dereferenced that missing
row. The helper confirms the server-wide effect and uses the existing native
UseBoosterItem request, applies its item receipt once, then displays the server's
activation/expiry acknowledgment. Ordinary boosters retain their native path.

The matching server derives gold rates from HonorTierReward.ServerBuff (5%, 3%,
2%), consumes only owned quantities, and stores the stacked shared bonus through
the current Honor season's end. Battle gold calculation reads that bonus for
every account. No personal timer or fixed seven-day duration is invented.
Deploy the server before distributing this client patch.

`--self-test-honor-buff` checks the shipping helper against native API stand-ins;
`--verify-honor-buff` checks the installed hook and ordinary fallback. Both are
included in `--build-release`, along with the new helper installation.


### Isolated client storage and one-time migration

`--client-storage-only` changes native Unity company `masangsoft` to `sprk`,
retaining the product name. It stages/validates raw PlayerSettings without
reserializing an incompatible Unity schema. Graphics verification accepts the
original and isolated payloads, with the corresponding timing flag offsets.
The automation/preference bridge performs a one-time, non-overwriting copy of
SPRK account stores and selected preferences before normal settings load.
Receipts prevent later official-client data from being imported again.

`--self-test-client-storage` covers migration/retry/receipt behavior;
`--verify-client-storage` checks installed assets and startup hooks. The full
`--build-release` path installs, verifies and tests this change. Windows native
validation is `python scripts/client/verify_client_storage.py` after staging.
See [client storage migration and retirement](../../docs/client-storage-migration.md)
for adoption reports, limits of offline-player coverage, and exact cleanup steps.
