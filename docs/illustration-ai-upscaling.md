# Illustration AI upscaling proposal

Status: planned, not implemented. Requested on 2026-10-27.

Add an optional client setting that applies AI super-resolution to artwork in
the illustration viewer at runtime. This document records the intended behavior
for later implementation; it does not authorize starting implementation now.

## Scope and current behavior

The first version should cover `NGame2.NUI.NWindow.HeroIllustBook`, including its
normal costume illustrations and season-reward illustrations. Leave other image
views outside the initial scope. This feature does not change 3D models, costume
ownership or source artwork in the installed asset bundles.

The viewer assigns artwork asynchronously through `SetupIllust` and
`UIHelper.ResetPlaneAssetTextureAsync` to `HeroIllustTexture`. It resets
`IllustRoot` when switching artwork. The existing
[illustration zoom helper](../scripts/DllPatcher/Resources/IllustrationZoom.cs.txt)
supports mouse-wheel zoom from 50% to 400%, while retaining native drag, touch
input and reset behavior.

Sampled illustration atlases in the installed client contain 2048 x 2048 textures
with individual artwork regions around 1022 x 1022 pixels. This is sample evidence,
not a guarantee that every viewer image has those dimensions. Before choosing a
model, inspect the actual texture, UV rectangle, resolution, format, color space
and alpha used by this viewer. Do not infer an individual image's resolution from
the atlas dimensions alone.

Existing World Scale and Preview Scale settings handle 3D rendering. Illustration
AI upscaling should be independent of both settings and of adaptive battle scale.
AI can reconstruct plausible detail, but cannot recover the original artist's
missing pixels exactly; faces and fine accessories may change.

## Settings and player experience

Add **Illustration AI Upscaling**, with **Off / On**, to Graphics settings.
Proposed default: **Off**, including when upgrading an existing installation.
Persist it through the existing client preferences system and use the existing
graphics help and localization conventions.

Suggested help text:

> Enhances illustrations when you open the illustration viewer. The first view
> may take longer and use additional GPU memory. AI may alter small details.
> Turn this off to display the original artwork.

Off displays the original texture and schedules no inference. On displays the
original immediately, then replaces it with the enhanced result when ready.
Keep controls responsive during processing. Switching the option should apply
to an already open viewer without requiring a game restart.

If processing is unavailable or fails, continue displaying the original artwork.
Explain unavailability through the setting's help/status text without interrupting
the player with repeated popups. Keep the saved preference separate from runtime
availability so a temporary failure does not silently erase the player's choice.

## Runtime processing

1. Wait for the actual source artwork assignment to finish. Associate each request
   with the viewer instance, source identity and a monotonically increasing
   generation number.
2. Reuse a valid cached result, or queue one inference job for the requested image.
   Start with a proposed 2x output size; confirm quality and memory costs before
   committing to that factor. Do not run inference every frame or wheel tick.
3. Process only the artwork region if the source is an atlas. Include sufficient
   edge padding for inference without sampling adjacent illustrations. Preserve
   orientation, framing, color space and transparent edges.
4. Publish a result only if the viewer is still open, the option is still On and
   the request still matches its instance, generation and source. Discard stale
   completions after navigation, closure, recycling or a settings change.
5. Assign the enhanced texture without changing the UI widget's logical size,
   image aspect ratio, current zoom, pan or reset behavior. Restore the original
   texture and UVs when turning the option Off.

Texture assignments and Unity resource destruction must happen on the main
thread. Schedule inference in bounded work appropriate to the selected backend;
an asynchronous API alone does not guarantee the GPU will avoid long stalls.
Coalesce duplicate requests and bound queued work during rapid navigation.

Keep the native source texture's ownership and release path intact. Never destroy
an asset-manager-owned texture. Enhanced textures and inference resources need
explicit ownership, release and cancellation rules so native `Close` and
`ReleaseTextureInfo` cannot release the wrong object or leak a source reference.

## Cache and resource limits

Use a bounded in-memory cache initially. A persistent disk cache is a later
decision, not a requirement for the first version. Key results by source asset
identity/content version, crop/UV region, model hash, output factor and processing
version. A source or model update must invalidate incompatible results.

Use least-recently-used eviction, with the displayed result pinned until detached.
Set measured limits for resident textures, queued requests, input dimensions and
inference workspace. Release resources on eviction and client shutdown. Turning
Off must stop new work, detach enhanced output and release resources when safe.

A 2048 x 2048 RGBA8 output alone occupies 16 MiB without mipmaps. Source textures,
input/output tensors, intermediate activations and staging buffers add to this;
budget for peak processing memory, not just the cached output texture.

Handle unsupported hardware, missing/corrupt model files, allocation failures,
device errors and invalid output by retaining the original image. Bound retries
to avoid recurring stalls. Record useful diagnostics without per-frame log spam.

## Model and backend selection remains open

The installed resource asset reports Unity `6000.0.53f1`. Verify the actual player,
rendering environment and shipped dependencies before choosing an integration.
Unity AI Inference/Sentis or a separate inference runtime are candidates to
evaluate; neither is currently selected, installed or verified for this feature.

Compare illustration-oriented super-resolution models on representative artwork.
Evaluate faces, hair, linework, gradients, jewelry, text-like markings and alpha
edges at normal size and throughout the existing zoom range. Preserve alpha
deliberately; do not assume an RGB model handles transparency correctly. If tiled
inference is needed, check overlap and stitching for seams.

Record the chosen model's source, redistribution license, version/hash, supported
operators, inference precision and hardware requirements. Benchmark cold and warm
processing time, viewer responsiveness, peak memory and output quality on supported
hardware. Decide whether any CPU fallback is acceptable from measurements rather
than enabling an unexpectedly slow fallback by default.

## Implementation and release integration

Keep future patch hooks, helper sources and verifiers under
`scripts/DllPatcher`. Extend the existing Graphics settings/help resources and
preserve the illustration zoom patch. Add installation and verification to
`BuildRelease.cs --build-release`, with a focused stage/install path for the
installed client.

Ship any required model, shader and runtime files as reproducible release inputs.
Verify that the complete signed manifest includes them without losing previously
managed assets. Generated cache files belong in writable client storage outside
the launcher-managed snapshot. This feature should run locally without an online
image-processing service.

Follow [GitHub client releases](github-client-updates.md) and
[client updates](client-updates.md) when implementing. Installing the eventual
client changes requires a new verified signed local update. This documentation-only
task changes no installed files and generates no client update.

## Acceptance checks for implementation

- Off is the default, persists across restarts and preserves original rendering.
- On enhances a normal and season-reward illustration without blocking controls.
- Toggling during processing, rapid navigation and closing/reopening cannot assign
  stale results or reuse destroyed textures.
- Zoom, drag, touch gestures, reset, Hide UI and artwork navigation still work;
  enhancement does not alter framing or logical bounds.
- Atlas crops, transparent edges and color remain correct, with no neighboring
  artwork bleeding or tile seams.
- Duplicate requests reuse work; cache eviction and repeated open/close cycles
  remain within measured memory limits.
- Unsupported hardware and processing failures leave the viewer usable with the
  original artwork; World Scale and Preview Scale remain independent.
- The full isolated build and focused patch verification pass, and all required
  files appear with verified hashes in the signed client update.
