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

## Portal categories

`Patches/Portal.cs` injects the embedded `Resources/Portal.cs.txt` helper into the client DLL. It restores 15 missing categories (23 total including the existing categories) and native destination activities without replacing existing table rows. New panels use the standard Small layout and English fallback labels. Native entry checks remain in effect; exposing a category does not implement its server gameplay. Pet uses the client's single Pet panel route.

Apply only this change to an already patched client:

```powershell
dotnet run --project scripts/DllPatcher -- <client-root> --portal-only
```

Add `--stage-only` to write `Assembly-CSharp.dll.patched` without installing. The full patch includes the Portal restoration too. Reapplying is idempotent.
