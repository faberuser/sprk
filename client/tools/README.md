# Client development tools

`ManagedSource` reads .NET metadata with Mono.Cecil. It never loads or runs game
code. Run it from `client/` so `global.json` selects the SDK.

```powershell
# IL of one type, optionally filtered by method name
dotnet run --project tools/ManagedSource -- inspect <assembly.dll> <full-type-name> [method-name]

# Types, fields, methods, attributes and references of every DLL in a folder
dotnet run --project tools/ManagedSource -- inventory <managed-dir> <output.json>

# Built DLLs keep the identity, references and embedded resources of a reference build
dotnet run --project tools/ManagedSource -- verify-build <reference-managed-dir> <built-managed-dir>
```

Use `inspect` to compare compiled output with a released DLL, and `inventory`
on two builds to compare field layouts before a risky refactor of serialized
types.

`JitTables` is different: it loads `Assembly-CSharp.dll` and runs the game's
table code (outside Unity) to dump, verify and rebuild `TableJit` and
`LocalizationJit` files. Normally it runs through
`client/scripts/build/build_tables.py`; see [docs/jit-tables.md](../../docs/jit-tables.md).

```powershell
dotnet run --project tools/JitTables -c Release -- dump <TableJit-dir> SkillTable out.json 1410
dotnet run --project tools/JitTables -c Release -- verify <TableJit-dir> SkillTable CreatureTable
```

`PerkIcons/make_perk_icons.py` composes framed Light/Dark perk icons for a hero
the game never drew them for (Maya). It lifts the frames and tints from a
released hero's perk icons and writes `SprkIcons.Skill.<index>.png`, which
`client/managed/SprkImageRecovery` embeds; see
[docs/heroes/maya-art.md](../../docs/heroes/maya-art.md). It needs Pillow and
NumPy and the source sprites extracted from `NguiAtlas_Icon_Skill_Trans`,
`NguiAtlas_Icon_Skill2` and the hero icon atlases (for example with UnityPy).
