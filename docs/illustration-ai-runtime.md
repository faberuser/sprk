# Illustration AI runtime

The `HeroIllustBook` viewer has a small **Off / x2 / x4** AI button immediately
to the left of **Hide UI**, using that button's native blue-and-gold skin and font.
Opening or navigating starts Off. Hover the button for the ordinary NGUI help
tooltip. The original stays visible during processing and is restored immediately
by Off. No graphics preference is saved. World Scale and Preview Scale are independent.
The label pulses while work is pending; processing and failure details appear in
the hover help instead of covering the artwork. Mouse-wheel zoom spans 50%–800%,
native touch zoom reaches 800%, and reset returns to 100%.

## Backend and model

This implementation uses the upstream portable **Real-ESRGAN NCNN Vulkan
20220424** Windows runtime and **RealESRGAN_x4plus_anime_6B** weights, pinned by
archive SHA-256 in `scripts/DllPatcher/prepare_illustration_runtime.py`.
It requires a working Vulkan GPU/driver, with no automatic CPU fallback.
The runtime is a hidden child process, limited to one image, one processing
thread and 128-pixel tiles. Cancellation kills that process and waits for its
exit before another job starts. A Windows job object caps child committed memory
at 1.5 GiB and terminates it if the client exits. The 120-second job deadline also applies to
incompatible user models. NCNN uses FP16 storage with FP32 arithmetic for this
distribution. The bundled runtime contains its Vulkan shaders; Unity shader
asset injection and Sentis/ONNX conversion are unnecessary.

The stable model interface is `sprk-ncnn-rrdb-rgb-x4-v1`: NCNN textual `.param`
and binary weights, input `data`, output `output`, RGB normalized to [0,1],
CHW tensors, three channels, native x4 output, RRDB-compatible operations.
The upstream runtime handles alpha separately with bicubic resampling. x2
uses native x4 inference followed by Unity downsampling; its inference cost is
the same as x4. The viewer retains its original flip, logical size, framing,
material/shader and zoom/pan transforms. Atlas regions are sampled at their
native pixel centers into an isolated crop; model edge padding cannot sample
another sprite.

Real-ESRGAN weights use BSD-3-Clause, the NCNN executable uses MIT and NCNN uses
BSD-3-Clause; redistributed notices are tracked in
`scripts/DllPatcher/Resources/IllustrationUpscaleNOTICE.txt` and installed beside
the runtime. The upstream portable OpenMP redistributable is included; the debug
OpenMP DLL is excluded.

## Writable files and first use

Files live under Unity's `Application.persistentDataPath/SprkIllustrationAI`:

- `active.sprkmodel`: the user's active, replaceable model.
- `cache/*.png`: completed outputs, with a 1 GiB LRU disk limit.

These directories are outside the installed client and all launcher includes.
The first explicit x2/x4 request provisions the model only if absent or empty.
The expected byte length, SHA-256 and interface are embedded in the signed
helper; the server metadata is informational and cannot change those authenticated
values. The launcher passes the selected profile's exact `ManifestUrl` to the
game as `SPRK_UPDATE_MANIFEST_URL`, alongside its game host URL. Model downloads
derive the sibling `models` route from that captured endpoint, preserving its
origin, custom port and path prefix. Editing the launcher profile after launch
cannot redirect an already running game's model downloads. The new launcher
requires no specific Local/Public profile names or fixed host/port pair.
For older launchers that do not pass the update URL, the game reads
`sprk-launcher.json` and uses `SPRK_HOST_URL` to select the matching profile.
HTTP is allowed for loopback; other origins require HTTPS. An unmatched active
server fails with a status message instead of falling back to the public server.
Legacy standalone clients without a profile retain the public default.
All downloads are size/hash verified before atomic activation.
A user model placed during download wins. Empty-file handling checks and deletes
the same exclusively opened Windows file handle, preventing replacement races.
A corrupt or incompatible nonempty file is preserved and produces a viewer
status message. Delete or empty it to allow provisioning on the next selection.
There are no automatic network/inference retries.
Existing nonempty models bypass profile resolution and remain usable offline.

To package a compatible replacement without downloading anything:

```powershell
python scripts/DllPatcher/prepare_illustration_runtime.py --pack-model replacement.sprkmodel --param model.param --weights model.bin
```

Close the viewer, replace `active.sprkmodel`, then reopen and select a scale.
The container has `SPRKSR01`, little-endian uint32 param/weights lengths, then
the param bytes and weights bytes. Model validation and cache identity use the
actual container hash, regardless of filename. Arbitrary Upscayl, ONNX, `.pth`,
or different tensor/scale conventions require conversion to this interface.

## Limits and measured behavior

Inputs are limited to 1024 pixels per axis; larger artwork stays original with
an explanatory status. Outputs are at most 4096 pixels per axis, without mipmaps.
Only the displayed result is retained in memory (at most 64 MiB GPU texture);
Off/navigation/close detach and destroy it and its owned material. Asset-manager
textures/materials are never destroyed. PNG decoding and Unity texture operations
run on the main thread; network, inference, cache validation/writes and eviction
run on the worker. A request holds at most one crop and one bounded PNG result;
decoded staging and x2 downsampling temporarily add up to about 100 MiB of texture
storage. Decoder input is capped at 80 MiB with fixed output dimensions and CRC
validation. Cache corruption is removed; storage failures do not prevent inference.
The one pending request and one running job bound rapid-navigation work.

On the local RTX 3060, the 1022x1022 Cleo atlas sprite took 3.243/3.170 seconds
with the illustration model, compared with 1.615/1.621 seconds with animevideov3
at identical 4088x4088 output sizes. These are first/repeated child-process runs,
not persistent-model warm measurements. A separate run sampled 548 MiB additional
device memory and 604.7 MiB peak child working set. Device memory includes other
applications and is not a portable upper bound. Leave roughly 1 GiB headroom for
processing in addition to the game; allocation/device failure keeps the original.
Both outputs were compared at normal size; further subjective comparison against
the player's preferred Upscayl model awaits its exact name/version.

## Reproducible build and server artifact

`BuildRelease.cs --build-release` installs and verifies the helper, both native
viewer assignment hooks, pre-navigation/pre-close cleanup, zoom preservation and
pinned runtime hashes. Focused commands:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --illustration-ai-only --stage-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --illustration-ai-only
dotnet run --project scripts/DllPatcher -- ../sprk-client --verify-illustration-ai
dotnet run --project scripts/DllPatcher -- ../sprk-client --self-test-illustration-ai
```

The signed client snapshot includes `Managed/SprkIllustrationUpscale.dll` and
`Managed/SprkIllustrationRuntime/*`, using the existing default managed selection.
Installing changes requires the normal complete signed client update.

Publish the versioned server artifact separately:

```powershell
python scripts/DllPatcher/prepare_illustration_runtime.py --server client-updates/illustration-models
```

The immutable files are `realesrgan-anime-20220424-v1/model.sprkmodel` and
`metadata.json`. The model is 8,973,806 bytes with SHA-256
`3aae12f7f04f602e301d82f84e7fa507a09827d99935e045c39d1d5ecccc7d67`.
The update service serves only those filenames under
`/updates/models/realesrgan-anime-20220424-v1/`, independently of client-release
retention. Local storage defaults to `SPRK_UPDATES_DIR/illustration-models`.
`SPRK_ILLUSTRATION_MODELS_DIR` can specify another artifact root. The Dockerfile
reproduces and verifies the model in a separate Python build stage, then includes
it in the server image at `/app/illustration-models`. Deploying that image makes
the first-use production URL available. Local artifacts alone do not deploy it.
A local update service can serve the generated `client-updates/illustration-models`
directory directly; local provisioning does not require Docker or public deployment.

## Validation

`python scripts/client/verify_illustration_ai.py --patch-zoom` applies both zoom
and AI controls to a private managed
copy, uses native asset loading, checks normal and season viewer rendering,
transforms, Off, cancellation, navigation and closure, and records screenshots
under `target/illustration-ai-validation/`. The model fixture also checks absent
and zero-length downloads, a concurrent user replacement and truncated downloads.
No credentials/login are needed. Each probe uses a separate preferences profile
and model/cache directory. `--installed` validates installed files instead.
Touch input and visual quality on other GPUs still require interactive checks.

The local Unity 6000.0.53f1/Direct3D11/Gamma probe passed on the RTX 3060:
normal and season-reward artwork, native source callbacks, 2044x2044/x2 and
4088x4088/x4 output, retained transforms and logical size, cycle-button events,
native help, Hide UI/show, Off during processing, navigation and close cleanup.
The tested source was an RGBA32 2048x2048 atlas with UV region
`(0, 0.500977, 0.499023, 0.499023)`. End-to-end viewer processing was about
4.1–4.3 seconds with frames continuing. Separate backend checks confirmed disk
hits with the executable unavailable (about 0.05 seconds), CRC corruption
rejection and termination of an in-flight inference process on cancellation.

Installed files were exercised again and published as the local signed update
`local-20261029-illustration-ai`: 707 files, preserving the previous release's
extra managed assets and existing deletion history. Stable/release manifests,
the RSA signature against the tracked public key, every file size/SHA-256, and
the installed patch/runtime hashes were verified. This is a local release;
GitHub publication and production server/Portainer deployment are separate.
The model artifact generator and HTTP route tests passed locally. Docker image
validation could not run because the Docker Desktop Linux engine was stopped.

The profile-aware download fix is installed in signed local update
`local-20261029-illustration-local-profile` (707 verified files, no new deletions).
The full isolated build and installed Unity probe passed. First-use testing began
without an active model and verified exactly one download from the running
game's local update profile, including a custom update port and path prefix,
while a different saved profile was selected. Existing writable models are preserved.

The launcher now hands off the exact selected update URL directly in signed local
client update `local-20261029-illustration-launcher-url`. The rebuilt launcher is
installed separately using the existing self-contained launcher build path; saved
profiles are unchanged. All 12 launcher checks, direct-handoff and legacy Unity
download probes, the full isolated build, and signed snapshot verification passed.
The direct-handoff probe succeeded with an invalid profile file on disk and a
custom update port/prefix, proving provisioning uses the captured launch URL.

Signed local update `local-20261029-illustration-controls-zoom` adds the compact
native-skin button beside Hide UI and extends wheel/native zoom to 800%.
The installed Unity probe verified the 12-pixel logical gap, Off/x2/x4-only
labels, the native 800% clamp and reset to 100%, enhancement, help and Hide UI.
The full isolated build and all 707 signed snapshot sizes/hashes passed; prior
managed assets and the writable model were preserved.
