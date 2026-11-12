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
