# DLL patcher

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

`--orvel-visuals-only` removes the floating EVENT effects and red event portal
from Orvel by filtering their prop records before native asset creation. Town
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
