# Managed source feasibility experiment

Status: the follow-up [maintained codebase](../client-source/README.md) now builds
Assembly-CSharp, all 47 SPRK companions and the launcher. The runtime parity gate
has **not** passed. The legacy release build remains authoritative. No
installed-client release was generated. The initial experiment results below are
historical; its 76 remaining errors have since been repaired in maintained source.

## Run the experiment

From the repository root, with Python 3.11+ and the .NET SDK installed:

```powershell
python scripts/client/managed_source_experiment.py --client ../sprk-client --work artifacts/managed-source-new
```

The workspace must not exist and must not overlap the installation. Use
`--inventory-only` to skip recovery/compilation. The experiment reads the
installation but only copies managed DLLs, not accounts, tokens, settings or
downloaded assets. It records native file hashes without staging a player.
It does not produce distributable artifacts or change release pointers.

The command installs ILSpyCmd **9.1.0.7988** into the experiment, exports the game
assembly, hashes the untouched export, copies it into a separate repair directory,
applies documented repairs, then attempts compilation. See
[ILSpy's project documentation](https://github.com/icsharpcode/ILSpy) for the
decompiler. Decompilation is an experiment/recovery step, not the future release
build process.

The inspector/compiler host targets .NET 8. The recovered game compilation uses
only the copied player references, explicitly excluding Assembly-CSharp.dll.
Roslyn 4.8.0 and Mono.Cecil 0.11.6 are pinned in the tool project. No framework
references from the host SDK enter the recovered game compilation. The exported
project is preserved for inspection; the experiment compiles its C# files directly.
This initial compiler does not yet package embedded resources or validate assembly
identity/contracts, so even a successful compile cannot authorize release.

Outputs:

| Path | Purpose |
| --- | --- |
| `inputs.json` | Managed/native hashes, source revision/status, SDK and tool inputs |
| `references/` | Snapshot of managed inputs |
| `inventory.json` | Assembly identities/references, types, fields, methods, attributes, interop and dynamic call sites |
| `transformation-index.json` | Ordered source-line discovery index for release orchestration and patcher entry point; ownership is explicitly unreviewed |
| `export-original/`, `export-hashes.json` | Untouched recovery output and hashes |
| `source/`, `repairs.json` | Repair working copy and per-file before/after hashes |
| `output/*.diagnostics.txt` | Compiler diagnostics, including warnings |
| `gate.json` | Compile result and closed parity/release gate |

This inventory is discovery evidence, not a complete serialization-contract
comparator. Method bodies, property/event mappings, generic constraints, field
offsets/defaults and runtime serialization still require dedicated comparison.
The transformation index includes conditional paths and declarations; it must be
reviewed against actual control flow before becoming the migration matrix.

## Observations on 2026-11-11

Local experiment: `artifacts/managed-source-20261111-02`.
Repository revision: `7b1e824e373419adb0f9aea13be19b1e2cdbd39a`.
The initial tool files were uncommitted; their hashes are recorded in `inputs.json`.

- UnityPlayer product version: `6000.0.53f1 (283510a092d9)`.
- 192 managed DLLs; Assembly-CSharp contains 18,413 types, including nested types.
- Assembly identity: `Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null`.
- Assembly runtime header: `v4.0.30319`; `I386`, `ILOnly` (this is managed PE
  metadata, not proof of native player bitness). References include mscorlib 4.0
  and netstandard 2.1. The complete copied framework set is the experiment's API
  profile; an SDK target-framework label alone would be insufficient.
- Assembly-CSharp SHA-256:
  `038e4e88e467be4ffd8dc043ab26187624c571e016f8937269c527eed26c12e2`.
- The game references SPRK helpers, and many helpers reference the game. There
  is also a self assembly reference. Dependency-cycle handling is still required.
- 1 P/Invoke method and 1,952 distinct reflection/type/message call sites in the
  game inventory. These sites need review; discovery does not establish safety.
- No original game source archive was found in the scoped workspace search.
  Third-party symbols exist in Managed; these are not recovered game source.
- The local installed DLL is only a **candidate reference**. It has not been
  certified as a known-good assembled release by this experiment.

The first compile of 12,246 exported C# files produced **148 errors**:

| Error category | Count |
| --- | ---: |
| Untyped discards (CS8183) | 72 |
| Missing members (CS0117) | 38 |
| Interface contract/implementation mismatch (CS0535/CS0539) | 17 |
| Missing generated types (CS0246) | 4 |
| Numeric conversions (CS0266) | 4 |
| Ambiguous names (CS0104) | 3 |
| Argument conversions (CS1503) | 3 |
| Explicit accessor implementation (CS0683) | 2 |
| Static type arguments (CS0718) | 2 |
| Unassigned out parameters (CS0269) | 2 |
| Invalid null coalescing (CS0019) | 1 |

Repair rule `typed-null-discard` changes standalone `_ = null;` into
`_ = (object)null;`. This gives the discard a compile-time type; evaluating and
discarding null has no side effects. It preserves other expressions and records
each changed file. The original export is never modified.

The repaired compile has **76 remaining errors**: all 72 CS8183 errors were
resolved. Three snapshot/repair tests pass, and a minimal assembly using
UnityEngine.Vector3 plus framework collections compiles against the copied
references. SHA-256 verification confirmed the untouched export and all installed
managed DLLs still match the recorded inputs. The repaired compile is a second
attempt in the same workspace; `inputs.json` describes the initial tool invocation,
while `repairs.json` records the subsequent source changes.

Several other failures reflect patched IL that C# cannot directly express:
explicit interface implementations disagree on int/long signatures; one
constructor contains a base-constructor call after other statements; patched
non-iterator methods retain attributes naming removed iterator types. Do not
blindly delete those statements/attributes or change numeric widths. Review
the reference IL, patch implementation and callers before each repair.

## Ownership and validation still required

| Current owner | Migration responsibility |
| --- | --- |
| `DllPatcher/Program.cs` full patch path | Game methods, fields and helper hooks; audit each indexed operation |
| `DllPatcher/BuildRelease.cs` | Preserve ordering, companion compilation and all verifiers, including Currency64 ordering |
| `DllPatcher/Resources/*.cs.txt` | Recover ordinary helper projects after dependency-cycle review |
| `client-overrides/files/` and table generators | Retain data/asset ownership and matching integrity metadata |
| `remove_client_telemetry.py` | Retain serialized asset/SDK cleanup before managed transformations |
| `scripts/release/` | Preserve pinned baseline, signed complete snapshots and deletion history |
| `scripts/server/linux_worker.py` | Worker consumes verified client release plus pinned runtime; validate against rebuilt game/helpers |

Before advancing: certify a known-good reference; finish per-feature ownership
and baked-baseline audit; resolve remaining compile errors; preserve resources;
compare API/serialization contracts; then assemble an isolated player and run
every parity category in the migration plan. Use existing automation scenarios
under `scripts/automation/scenarios`, release verifiers and worker smoke/native
integration checks. Record reference/candidate results with isolated accounts and
storage. None of those runtime checks have been claimed as passed here.

Repository placement, immutable binary storage, bootstrap ZIP integration,
launcher upgrade/rollback tests and controlled cutover remain later gated work.
