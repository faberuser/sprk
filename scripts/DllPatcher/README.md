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
