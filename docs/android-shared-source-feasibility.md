# Native Android with one maintained C# source tree

Research checkpoint: 2026-11-16. This is a feasibility/design note, not an
Android installation guide or a claim that an Android port works.

## Goal and recommendation

Run SPRK natively on modern ARM64 Android phones while keeping gameplay and
QoLs in **one maintained source tree: `client/managed/`**.

Requirements from the discussion:

- Reuse present and future Windows gameplay changes and optimizations.
- Do not maintain a second JavaScript implementation of gameplay/features.
- Do not use streaming (Moonlight) or Windows emulation (Winlator).
- Do not make ARMv7/32-bit support a requirement for the user's phone.
- Prefer a reusable player/bootstrap over reconstructing the entire original
  Unity project, if experiments show that this is possible.

**Recommended next investigation: a matching Unity Android ARM64 bootstrap
with HybridCLR.** It offers a documented ARM64 dynamic-C# path, but the hardest
SPRK-specific questions—archived scene/component binding and content loading—
remain unproven. This is not yet a decision to adopt it.

Keep conventional shared-source IL2CPP as the baseline alternative. Investigate
IL2CPP2MONO only as a time-boxed experiment: it most closely resembles today's
DLL replacement workflow, but has substantial runtime and licensing uncertainty.
Do not spend more time hunting stock ARM64 Mono APKs unless new evidence appears.

## What “one source” does and does not mean

One source tree can produce Windows Mono and Android IL2CPP/HybridCLR outputs.
It does **not** require identical DLLs, runtime libraries, packaging or update
mechanisms. Small platform adapters and separate reference/build profiles are
expected; gameplay should stay shared.

The Windows client uses Unity **6000.0.53f1** (changeset `283510a092d9`) and Mono.
The repository builds `Assembly-CSharp.dll`, `Sprk*.dll` and the Windows launcher;
it does not contain a complete editable Unity project with original scenes and
assets. The managed build pins 144 Windows runtime/dependency DLLs in
`client/managed/references.lock.json`.

- Windows `UnityPlayer.dll` cannot simply be converted into an Android library.
  Android requires its own native player, architecture and packaging.
- Official Unity Android Mono targets ARMv7, not ARM64. Official ARM64 builds
  use IL2CPP. This restriction does not rule out unofficial Mono bridges.
- A stock IL2CPP APK does not execute replacement gameplay DLLs dropped into it.
  Its executable game code and metadata were generated together ahead of time.
- IL2CPP inspection/dummy DLLs are not recovered executable managed bodies.
- Unity's matching Android Build Support is a better player source than an
  unrelated game's APK. Availability does not solve scene/bootstrap integration.

## Options worth keeping

| Approach                      | One gameplay source? | DLL-based gameplay updates?               | Assessment                                                                                        |
| ----------------------------- | -------------------- | ----------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Own ARM64 IL2CPP build        | Yes                  | Not ordinary DLL replacement              | Official runtime baseline; requires a real conversion/link/build pipeline and content integration |
| Own ARM64 HybridCLR bootstrap | Yes                  | Yes, for configured hot-update assemblies | Recommended feasibility investigation; modified IL2CPP plus interpreter/AOT integration           |
| ARM64 IL2CPP2MONO bridge      | Potentially          | Intended to support managed DLL loading   | Closest workflow match, but experimental APIs/GC/version/license risks                            |
| Archived ARM64 APK plus hooks | Not automatically    | No shared C# execution                    | Useful reference, not the desired long-term architecture                                          |
| Official Mono ARMv7 donor     | Potentially          | Yes in principle                          | Rejected final target: excludes ARM64-only devices and still needs integration                    |

### HybridCLR: investigate first

HybridCLR extends IL2CPP with interpreted execution of managed code. Its docs
list Android ARM64 and Unity 6000 LTS support, dynamic loading through
`Assembly.Load(byte[])`, and hot-update MonoBehaviours, including components on
AssetBundle prefabs.

Important limits:

- Requires an **own HybridCLR-enabled player/bootstrap**; it is not a DLL to
  drop into the archived stock IL2CPP APK.
- Exact compatibility with **6000.0.53f1** and a pinned HybridCLR release still
  needs verification. Family-level support is not an exact-version test.
- Configure hot-update assemblies, preservation, AOT metadata/generic coverage
  and native/managed bridges. Do not assume every dependency can be hot-loaded.
- Preserve serialized component identities and field layout. Loading a small
  unrelated DLL is not proof that existing game scenes bind to the shared code.
- Measure combat performance; one source does not mean all code should execute
  in an interpreter. Frequently executed code may need AOT compilation.
- Current v9 documentation drops Unity 2019–2021; v8 is needed for those versions.
  Unity 2018 is not a standard supported target. Do not plan around replacing
  the old APK runtime in place.

### IL2CPP2MONO: experimental fallback

Upstream describes Android ARM64 support by replacing `libil2cpp.so` with a
bridge forwarding IL2CPP APIs to Mono. Its packaging includes
`assets/Mono/Managed.zip`, `mono.zip` and native runtime libraries.

At the research checkpoint:

- README reported **75 of 241 API calls unimplemented**.
- No verified SPRK or Unity 6000.0.53f1 compatibility was established.
- Open issue #9 reported Mono GC crashes and managed/native memory corruption.
  A reported SGen pinning fix for one case is not proof all GC problems are fixed;
  Boehm failures remained unexplained in the reviewed discussion.
- Android CI was observed using Unity Mono's `unity-2022.3-mbe` branch. Recheck
  the pinned source/toolchain before assuming Unity 6 compatibility.
- No clear license was identified (GitHub metadata reported `license: null`).
  Clarify permission before integration or redistribution.
- An `Android.zip` release existed, but was **not downloaded or executed**.

This could reduce pipeline work if it survives tests; it could also create a
larger runtime-maintenance obligation than a conventional port. Test component
binding, callbacks, GC, threads, scene reloads and lifecycle before gameplay work.

### PortMaster bridge: reference only

`portmaster-unity-toolkit` documents an ARM64 IL2CPP-to-Mono bridge and Unity
6000.0 experiments. Its target is ARM Linux handhelds with a glibc runtime and
custom host, **not Android APKs**. It is a technical reference, not a verified
Android donor or ready-made solution.

## Preserved APK findings

Local archive inventory examined 317 APK files across `D:/apk/apks` and
`D:/apk/downloaded` (including duplicate copies). No verified stock ARM64 Mono
candidate was found locally or in the online investigation. This is a search
result, not proof that no such unofficial runtime exists anywhere.

### Matching original ARM64 client

- File: `D:/apk/apks/kings-raid-4.88.8__1136912122.apk`
- SHA-256: `c6691c87d97c80c850e0576f9edd99e835f4ca155a16869525686d7f9d641528`
- Manifest version `4.88.8`, code `4088008`; config version `04.88.008`.
- Unity **2018.4.36f1**, IL2CPP, ARM64 + ARMv7.
- Corresponds to `../Pre-Doomsday/KingsRaid 9-1-2022.apk` (original x86 archive).
- `global-metadata.dat`, `globalgamemanagers` and the Config asset are
  byte-identical to that x86 archive. Of 3,758 shared asset entries, 3,755 matched
  CRC/size; differences were AppGuard signing assets.
- Endpoint patch passed in-memory validation. Representative IL2CPP bridge APIs
  and startup-hook assumptions passed static inspection, not device execution.

This is a valuable Android content/protocol reference and potential hook-based
proof, **not a player that directly runs the maintained Windows assemblies**.
Matching archived metadata does not establish current Windows feature parity.

Modern-device caution: six inspected ARM64 support libraries have only 4 KB ELF
LOAD alignment. Core Unity/IL2CPP/main libraries have 64 KB alignment, but that
alone does not establish 16 KB-page compatibility for the application. ZIP
alignment does not repair incompatible ELF segments or hardcoded page assumptions.

### Archived Mono donor

`D:/apk/apks/kings-raid-3.65.5__2652914.apk` was the highest game version found
using Mono: Unity 2018.2.20f1, ARMv7 + x86, with 95 real managed DLLs. It is useful
historical evidence, but not the ARM64 target. Current C# also uses newer Unity
and framework APIs, so downgrading to that runtime is not a simple recompile.

## Content is a separate feasibility gate

Android assets have previously been reused successfully on Windows. Therefore,
do not assume cross-platform bundles categorically cannot work. Equally, that
does not prove Windows-to-Android compatibility for every asset.

Test actual scenes, serialized types, textures/compression, shaders, materials,
GPU APIs and runtime versions. Copying bundles or changing a platform marker is
not a general conversion pipeline. Rebuilding Android-compatible bundles may
require recovering/importing source assets and validating shader equivalents.

Original Android content is available under:

`../Pre-Doomsday/com.vespainteractive.KingsRaid/files/Documents/Patch/Android`

`client/scripts/android/convert_android_bundle_pack.py` converts indexed BJS
metadata to `BundlePack.binv2`; it **does not convert GPU assets**. The untouched
archive has `Assetbundle/BundlePack.bin`, not the converted `.binv2` output.

Preserve assembly, namespace, type names and serialized field layout when
binding old content to shared C#. Also validate current tables/IDs and overrides:
Maya, four-hero Challenge Raid and dynamic Conquest are examples of divergence.

## Server compatibility is necessary, but not the primary runtime blocker

Changing only the old APK endpoint is no longer sufficient for full parity:

- Password/bearer verification and one-use game-ticket login replaced guest-only
  flows; chat and battle socket login require SessionKey.
- Remote play needs the advertised WebSocket/TLS transport, not scheme stripping
  or public exposure of private TCP listeners.
- Worker battle completion needs `BattleStopped` and `ReplayWaveIndex` in pings.
- Conquest requires current dynamic sessions, initial creatures, clocks/replays
  and score handling.

Existing `client/scripts/android/accounts.js` and `lobby.js` provide historical
hook-based compatibility work, not an automatic path to shared-source parity.
Earlier x86 BlueStacks work reached login/chat/lobby/intro/tutorial; ARM combat
and remote WSS were not demonstrated. Many performance optimizations are opt-in;
they are not themselves the main reason the archived APK needs adaptation.

Useful shared-source references:

- `client/managed/SprkWebSocketTransport/SprkWebSocketTransport.cs`
- `client/managed/SprkConquest/SprkConquest.cs`
- `server/src/api/account/auth.rs`, `server/src/api/account/user.rs`
- `server/src/api/community/chat.rs`
- `server/src/api/battle/cooperative.rs`, `server/src/api/battle/campaign.rs`

## Resume plan and decision gates

Do not start by attempting the whole game or publishing an Android release.

1. **Inventory tools and pin versions.** Check installed Unity Editor, Android
   Build Support, SDK/NDK/JDK and available physical ARM64 devices. Verify exact
   Unity/HybridCLR compatibility and licenses before downloads/builds. Do not
   alter the Windows pinned references to make Android compile.
2. **Build an isolated minimal ARM64 player.** Prefer the matching Unity version
   and HybridCLR first. Keep bootstrap/build glue separate from gameplay source.
   If that path fails, record the blocker before testing IL2CPP2MONO separately.
3. **Prove shared-code execution.** Compile/load a shared assembly using an
   Android reference profile; instantiate a real representative game component,
   not only a Hello World. Verify serialized references and Unity callbacks.
4. **Stress the runtime.** Exercise repeated scene load/unload, GC pressure,
   threads/callbacks, suspend/resume and low-memory recovery. Test a modern
   ARM64-only/16 KB-page device before claiming broad compatibility.
5. **Prove real content.** Load one original scene/prefab with dependencies;
   validate component binding, shaders, textures and storage paths. If this needs
   extensive asset reconstruction, reassess effort before porting more systems.
6. **Reach a first local battle.** Use an isolated local stack, scratch database
   and starter account: auth → lobby → early solo campaign → results → relog.
   Verify rewards/progress and interrupted/repeated submissions. Ordinary solo
   campaign is the smallest target; `RequireBattleService: false` in
   `tables/ServicesRules.json` was the researched configuration, not a guarantee
   for every mode or future configuration.
7. **Only then expand scope.** Validate untethered TLS/WebSockets, multiplayer,
   Conquest, current content, Android UI/input/storage/native adapters, packaging
   and signed content updates. Reuse existing shared gameplay implementations.

For an initial tethered proof, ADB reverse on 8080/9001/9002 may avoid implementing
remote transport immediately; all advertised endpoints must still target the
isolated stack. This is not the final untethered architecture.

**Stop/reassess** if the bootstrap cannot bind real serialized game components,
content needs wholesale reconstruction, runtime GC is unstable, or the required
bridge becomes a second major engine-maintenance project. One source tree alone
does not guarantee an effortless or low-maintenance port.

## Evidence, links and limitations

Local generated static evidence (ignored, machine-local; may need regeneration):

- `artifacts/android-feasibility/inspect_matching_apk.py`
- `artifacts/android-feasibility/matching-apk-static-report.json`
- `artifacts/android-feasibility/feasibility.md` — older, hook-oriented report;
  this document adds the subsequent shared-source runtime findings.

Upstream references reviewed during the investigation (recheck before adoption):

- [Unity 6000.0.53f1 and modules](https://unity.com/releases/editor/whats-new/6000.0.53f1)
- [HybridCLR](https://github.com/focus-creative-games/hybridclr)
- [HybridCLR supported platforms/Unity versions](https://www.hybridclr.cn/en/docs/basic/supportedplatformanduniyversion)
- [HybridCLR dynamic code loading](https://www.hybridclr.cn/en/docs/basic/runhotupdatecodes)
- [HybridCLR installation](https://www.hybridclr.cn/en/docs/basic/install)
- [HybridCLR quick start](https://www.hybridclr.cn/en/docs/beginner/quickstart)
- [IL2CPP2MONO](https://github.com/MelvinShwuaner/IL2CPP2MONO)
- [IL2CPP2MONO release assets](https://github.com/MelvinShwuaner/IL2CPP2MONO/releases/tag/latest)
- [IL2CPP2MONO GC issue #9](https://github.com/MelvinShwuaner/IL2CPP2MONO/issues/9)
- [IL2CPP2MONO Android workflow](https://github.com/MelvinShwuaner/IL2CPP2MONO/blob/main/.github/workflows/Android.yml)
- [PortMaster ARM64 Mono notes](https://github.com/Producdevity/portmaster-unity-toolkit/blob/main/docs/mono-arm64.md)
- [Android 16 KB page-size guidance](https://developer.android.com/guide/practices/page-sizes)

No SPRK ARM64 runtime compatibility, HybridCLR build or IL2CPP2MONO execution
was demonstrated in this investigation. Static APK checks and upstream support
claims must not be presented as physical-device test results. No game/archive
files were modified, no APKs installed and no public server tests performed.
Documentation-only work requires no installed-client release.
