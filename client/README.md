# Maintained SPRK client code

This is part of the `sprk` monorepo: server code is in `server/src/`, PostgreSQL
schema changes are in `server/migrations/`, client code is here, and the launcher
is in `client/launcher/` (paths relative to the repository root).
The repository's sibling `../sprk-client` directory remains the installed game;
it is not a separate source repository.

This directory contains the recovered game source and all 47 installed SPRK
companion libraries as ordinary C# projects. Together they build 48 managed DLLs.
The launcher is maintained in `launcher/` within this directory and included by the build
command. Unity player binaries, native plugins, framework/third-party libraries,
scenes, asset bundles and Documents are external inputs, not recovered source.

Commands are grouped by purpose under `client/scripts/`: `build/`, `release/`,
`legacy/`, `recovery/`, `verification/`, `android/`, `launcher/` and `automation/`.
Legacy assembly and inspection tools are documented in
[tools/README.md](tools/README.md). See [the scripts inventory](../docs/scripts.md)
for entrypoints and tests.

## Build

Requirements: Windows, Python 3.11+, .NET SDK **10.0.100**, and the pinned API
reference package. NuGet access is required for the launcher's Windows runtime
packs. If the SDK is installed normally:

```powershell
python client/scripts/build/build_managed_client.py --references artifacts/client-source-api-references.zip --work artifacts/my-client-code-build
```

Run from the repository root. `--work` must name a new directory. A private SDK
installation is also supported:

```powershell
python client/scripts/build/build_managed_client.py --references artifacts/client-source-api-references.zip --work artifacts/my-client-code-build --dotnet artifacts/dotnet-sdk-10/dotnet.exe
```

Use `--managed-only` to omit the launcher. This does not change the game runtime
target: .NET 10 runs the compiler and launcher, while the game projects use the
Unity player's pinned Mono/framework API references. SDK framework references
are disabled. The `netstandard2.1` project label enables SDK tooling; the actual
reference set in `dependencies.lock.json` defines the game's available APIs.

The output is:

```text
<work>/client-code/
  SprkLauncher.exe
  King's Raid_Data/Managed/
    Assembly-CSharp.dll
    Sprk*.dll
<work>/result.json
<work>/build-inputs.json
<work>/logs/
```

The output deliberately contains no Unity executable, player DLL, native
plugins, scenes, asset bundles, Documents, accounts, tokens or launcher state.
It is a code payload, not a standalone game installation. Small existing helper
resources (four PNGs and one JSON file) remain embedded to preserve helper
behavior. The launcher uses its existing icon and maintained source.

## Dependencies and circular references

`dependencies.lock.json` pins 191 compile-time DLLs by SHA-256 and size:
144 runtime/vendor APIs and 47 original companion APIs for bootstrapping.
**The original Assembly-CSharp.dll is not a build input.**

The build first compiles the game using the pinned helper APIs. It then compiles
all helpers against that source-built game, recompiles the game against the
source-built helpers, and compiles the final helpers against the final game.
Only the final source-built DLLs enter the output. No decompiler, DLL patcher,
baseline game assembly template, or source generator runs during this build.

The API ZIP is a separate binary input and is not committed as recovered source.
Its current SHA-256 is
`f9bff180d9cc13466fd69e76f5570079d830878e01cb984bef1fe15da6f51c3b`.
Every file is verified against the tracked lock before compilation. A clean
checkout plus that ZIP and the SDK can build without the game installation.
The package has been produced locally; a remote artifact location has not yet
been configured. Keep it alongside the source when moving this codebase.

To recreate the reference ZIP from a directory containing the exact pinned DLLs:

```powershell
python client/scripts/build/prepare_managed_references.py --references <managed-api-directory> --output <new-api.zip>
```

Individual projects can be opened in an IDE or built with `dotnet build` after
pointing `ManagedReferenceDirectory` at a staged reference directory. Use the
orchestrated command for a complete build so circular dependencies are handled.

## Recovery and compatibility

`recovery.json` records the original assembly hashes and per-file repair hashes.
`recovery.patch` records the source edits relative to the untouched ILSpy 9.1
exports. Recovery is complete as a compilation exercise; maintained edits should
now be made directly in `managed/`, without rerunning decompilation.

Repairs include qualified type names hidden by enclosing namespaces, typed null
discards, explicit interface properties, stale iterator attributes, generic IL
parameter notation, and a null-list branch reconstructed from the original IL.
Compiler-owned RefSafetyRules attributes are emitted by the compiler rather than
declared explicitly in recovered helper source. The lobby helper qualifies the
game's Avatar type instead of UnityEngine.Avatar.

The old Currency64 patches left several explicit interface methods with mismatched
Int32/Int64 signatures. Implementations now match their declared interfaces;
remaining Int32 storage/request boundaries use checked conversion. Values outside
Int32 range throw instead of silently truncating. This is a compatibility decision
that needs gameplay testing, particularly selectors, pet feeding, roulette,
punishment shards and persisted timestamps. It is not a claim of runtime parity.
The CachedHeroInfo base constructor now executes in normal C# constructor order;
its dictionary initialization is independent of the preceding recovered parameter
normalization, but exact exception/allocation timing can differ.

Compilation alone does not certify Unity serialization, reflection, scene/UI
bindings, live server behavior or Linux battle-worker compatibility. The legacy
release path remains active. This source payload must pass those checks before
installation, signing or public cutover. No installed-client update is generated
by this command, and `result.json` explicitly leaves release eligibility false.

## Verified local result

On 2026-11-11, `artifacts/client-code-build-02/client-code` was built from the
reference ZIP using SDK 10.0.100. All 48 managed projects and the launcher compile.
The recovered source still produces compiler warnings; zero errors does not mean
warning-free or behaviorally validated code.

- All 48 assembly identities and their referenced assembly identities match the
  required metadata; embedded resource names and bytes match the original DLLs.
- All 49 output files match the SHA-256 manifest in `result.json`.
- Nine Python snapshot/reference-package/repair tests pass.
- Twelve existing launcher update, signing, rollback and profile checks pass.
- All installed managed DLL hashes remain unchanged.

No Unity gameplay, serialized-contract or Linux-worker parity test was run.

## Development integration

On 2026-11-12, the working branch was based on development commit `ad1bd52`,
including the PostgreSQL migration. The recovered `SprkStorageMigration` now
matches development's retirement of legacy storage import: the default path
records an empty receipt without reading the old files or registry values. The
explicit legacy overload remains available for the existing regression tests.
The upstream storage tests pass against the maintained source, and the updated
SprkAutomation project compiles. Recovery hashes describe the original recovery,
before this subsequent maintained-source change.
