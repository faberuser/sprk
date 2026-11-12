# Managed client source migration plan

Status: maintained game source, all 47 SPRK helpers and launcher compile through
the [code-only build](../client/README.md). The runtime parity/release gate
has not passed. See [experiment results and tooling](managed-client-source-experiment.md).

Date: 2026-11-11

## Objective

Maintain the client's managed game code as ordinary C# source and compile its
assemblies instead of expressing game-code changes as DLL patches. Preserve the
existing Unity player, packaged assets, server compatibility, launcher, and
distribution process.

This plan does not reconstruct a Unity Editor project or rebuild Unity itself.
It does not assume that decompiled source will compile successfully or preserve
behavior without repairs. An isolated feasibility experiment determines whether
the migration should proceed.

## Current state

- `../sprk-client` is an installed Unity player with `MonoBleedingEdge` and
  `King's Raid_Data/Managed/Assembly-CSharp.dll`.
- `client/tools/DllPatcher` contains IL transformations and C# feature implementations
  embedded as `Resources/*.cs.txt`. Its own .NET 8 target is a tool requirement,
  not evidence of the game's runtime target.
- `client/tools/DllPatcher/BuildRelease.cs` coordinates the full release patch path,
  companion assemblies, and verification.
- CI assembles the client from a pinned distributed baseline, tracked asset
  overrides, telemetry cleanup, and the full patcher build.
- New players download a complete `sprk-client.zip` from Google Drive and launch
  `SprkLauncher.exe`. Existing players receive signed updates through the
  launcher. The complete ZIP is refreshed after client updates to reduce update
  downloads for new players.

The existing baseline may already contain SPRK changes. Recovering an original
assembly alone would not necessarily preserve the current client behavior.

## Target architecture

```text
Maintained C# source + pinned runtime references
                    |
                    v
       Compiled game and SPRK assemblies
                    |
Pinned player/assets + tracked data/asset transformations
                    |
                    v
         Isolated assembled client folder
                    |
          Compatibility verification
                    |
             +------+------+
             |             |
             v             v
     Signed update     Complete client ZIP
     for launcher      for Google Drive
```

Maintain the recovered source under `client/` in this monorepo, alongside
`server/`. Keep `../sprk-client` as an installation. Recovery experiments and
compiled outputs remain in ignored isolated workspaces under `artifacts/`.

Keep maintained C# projects, build configuration, dependency manifests, and tests
in source control. Fetch large player/assets and required binary dependencies
from immutable, hash-verified inputs. Exclude build output, player credentials,
local settings, caches, and signing keys from source control and distribution
staging.

If source moves to a separate repository, the server release must pin its commit
and include that identity in the client build-input fingerprint. It must not
build from a moving branch or silently reuse an update built from older source.

## Phase 1: inventory and reference behavior

1. Identify the exact Unity player version, Mono/.NET API profile, architecture,
   assembly references, native plugins, and managed assembly identities.
2. Inventory all game-owned managed assemblies and SPRK companion assemblies.
   Identify which assemblies must be reconstructed and which can remain pinned
   dependencies. Inspect reflection, native interop, and runtime code loading.
3. Map every release transformation to its owner: game source, companion source,
   table/data override, serialized asset transformation, or packaging cleanup.
   Record patch ordering, added members, exposed private members, and helper
   dependencies. Include changes baked into the distributed baseline.
4. Select a known-good, fully assembled SPRK release as the behavioral reference.
   Record hashes, source revisions, baseline identity, and tool versions.
5. Document representative runtime checks using the existing automation and
   verifiers. Record the Linux battle worker's managed-code dependencies too.

Deliverable: dependency inventory, patch migration matrix, and a reproducible
reference client. No installed files or production release pointers change.

## Phase 2: isolated rebuild feasibility experiment

1. Export the reference game assembly with a pinned .NET decompiler into an
   isolated directory. Preserve the untouched export separately from repairs.
2. Create a compilable project targeting the actual game runtime, using its
   compatible Unity/framework references. Do not adopt .NET 8 merely because
   the patcher uses it.
3. Resolve decompiler artifacts, missing references, generated code, and assembly
   attributes. Record each repair and why it should preserve behavior. Do not
   refactor or add features during this experiment.
4. Preserve assembly identity, namespaces, type names, inheritance, member
   signatures, serialized fields and attributes, interop declarations, and
   reflection-visible names. Audit serialized type compatibility explicitly.
5. Compile the replacement assembly and assemble a separate test client with
   the reference player/assets. Rebuild dependent helpers when necessary;
   avoid dependency cycles introduced by patched game/helper references.
6. Compare the reconstructed assembly's relevant API and serialization contracts
   against the reference, then run runtime parity checks.

The first gate is a successfully rebuilt `Assembly-CSharp.dll` that passes the
reference scenarios. Compilation alone is insufficient. Binary equality is not
required; externally observable behavior and compatibility are required.

If substantial game logic is in other managed assemblies, extend this gate to
those assemblies before claiming the managed game code is source-buildable.

### Required parity coverage

- Startup, launcher handoff, login, and reconnect/error handling.
- Lobby loading, scene transitions, UI bindings, and representative asset bundles.
- Inventory, equipment, selectors, shop, mail, and representative SPRK features.
- Battle startup, skills, damage/rewards, battle completion, and repeat runs.
- Persistence across restart, settings, and isolated account/storage behavior.
- TableData, TableJit, LocalizationJit, and integrity metadata compatibility.
- Existing telemetry removal and storage-isolation behavior.
- Linux battle worker startup and native battle integration checks.

Use isolated accounts and storage. Investigate failures against the reference;
do not weaken a verifier merely to make the rebuilt assembly pass. Some existing
IL-shape checks will need equivalent behavioral or contract checks because a
compiler can emit different valid IL.

### Gate decision

- **Proceed:** required assemblies compile and representative parity checks pass.
- **Continue investigation:** failures are localized with understood repair paths.
- **Defer full reconstruction:** unresolved failures make equivalence impractical.
  As a fallback, organize existing SPRK feature code into normal C# projects and
  retain a small, explicit hooking layer. This improves maintenance but does not
  satisfy the full managed-source objective.

Estimate the remaining migration effort after this gate, using observed repair
complexity rather than an assumed decompilation success rate.

## Phase 3: establish maintained source and retire patches

1. Commit the validated recovered source and documented repairs as the initial
   maintained baseline. Decompilation becomes a recovery step, not a recurring
   build step that overwrites maintained edits.
2. Move SPRK helper implementations from embedded text resources into ordinary
   C# projects with explicit dependencies and compatible target frameworks.
3. Migrate one feature group at a time. Where recovery already includes a patch's
   behavior, document and verify that ownership rather than applying it again.
4. Remove each migrated IL transformation from the source-build path only after
   its replacement passes parity checks. Keep the legacy release path available
   during migration, with explicit selection of the build mode.
5. Retain asset/data overrides and transformations that remain necessary. Source
   compilation does not replace serialized asset cleanup, table regeneration,
   integrity metadata generation, or native dependency packaging.

Avoid a partially ambiguous build: each behavior must have one authoritative
implementation, and each build mode must declare its required transformations.

## Phase 4: reproducible build and release integration

Provide one tracked command that fetches verified inputs, compiles assemblies,
assembles an isolated client, applies required asset/data transformations, and
runs relevant checks. A clean checkout must not depend on files copied manually
from a developer's installation.

Integrate this command into the release orchestration described in
[GitHub client releases](github-client-updates.md). Preserve baseline verification,
telemetry cleanup, complete managed-file selection, signing, deletion history,
and image-pinned server/client deployment behavior. Extend dependency caching
and build fingerprints to cover the new source and compiler inputs.

Generate both delivery artifacts from the same verified staging snapshot:

- A complete signed launcher update with existing manifest and hash verification.
- A complete bootstrap ZIP containing the player, assets, launcher, and matching
  release metadata. Include only distributable files; exclude accounts, tokens,
  logs, backups, temporary outputs, and machine-specific launcher state.

The full ZIP is a superset of the launcher-managed update snapshot. Verify that
every managed path in the ZIP matches the signed release manifest. Test a fresh
ZIP extraction with the launcher against that release: it should require no
game-content update when it already contains the same managed snapshot.

Also test upgrading an older client, interrupted-download recovery, and the
expected removal of obsolete managed files. Preserve the current Google Drive
entry point; automating the upload is a separate integration decision and is
not required to prove the source migration.

## Phase 5: controlled cutover

1. Produce an isolated candidate release and run the full parity suite.
2. Install and validate the candidate locally only when implementation reaches
   that explicitly intended stage. Any installed-client changes require a new
   signed local update under [client updates](client-updates.md) and `AGENTS.md`.
3. Use the normal development branch, PR, and main-branch release process for
   public rollout. A local signed artifact is not a public deployment.
4. Publish the matching complete ZIP for new players after the release is ready.
5. Keep the prior verified artifacts and legacy build route until the source
   build is accepted. Rehearse rollback with a matched server/client release;
   account for launcher version handling and deletion history rather than
   assuming an older manifest can simply replace the active one.

## Completion criteria

- A clean checkout builds the required managed game assemblies from maintained
  C# source without the original compiled game assembly serving as the output
  template for IL patching.
- Existing player/assets load correctly, and required runtime parity checks pass
  on the Windows client and Linux battle worker.
- Migrated gameplay changes are direct source edits, with remaining binary or
  asset transformations explicitly documented.
- CI fingerprints all inputs and produces verified signed updates plus a matching
  complete bootstrap ZIP.
- Both fresh installation and launcher upgrades work through the existing player
  workflow, and a rollback procedure has been validated.

## Information needed later

No additional user input is required to begin the isolated inventory and rebuild
experiment when implementation is authorized. Discover runtime versions and
dependencies from the workspace first. Check available archives for original
source or symbols before relying exclusively on decompilation.

Before release integration, settle the source repository location and ownership,
artifact storage for pinned dependencies, and whether Google Drive upload remains
manual. These decisions do not block the feasibility experiment.

## Recovery tool references

- [ILSpy](https://github.com/icsharpcode/ILSpy) supports whole-project managed
  assembly decompilation. Export is a starting point, not proof of a correct
  rebuild.
- Full Unity project recovery and asset export tools are outside this plan's
  scope unless a specific compatibility investigation requires them.
