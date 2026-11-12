# Client migration and inspection tools

The maintained source build is `python client/scripts/build/build_managed_client.py`.
It does not invoke these tools.

| Tool | Current use |
| --- | --- |
| `DllPatcher` | Legacy release builder, asset/data transformations, companion compilation and parity verifiers. Still called by the production release pipeline. |
| `ManagedSource` | Recovery experiments, read-only metadata/IL inspection and assembly identity/resource verification. Used to validate source-built output. |

The obsolete DllDisasm project was removed; use ManagedSource for inspection:

```powershell
dotnet run --project client/tools/ManagedSource -- inspect <assembly.dll> <full-type-name> [method-name]
```

The game source compiles, but Unity serialization, gameplay and Linux-worker
parity have not been accepted for release. Keep DllPatcher until its release
responsibilities are migrated and validated. Source compilation does not replace
asset cleanup, table/integrity generation, native packaging or signing.

These tools target .NET 8 and have their own SDK selection. The maintained client
and launcher use the SDK pinned in `client/global.json`.
