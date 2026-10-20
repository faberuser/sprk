# Client build inputs

Place tracked replacement files under `files/`, preserving their path relative
to the client root, for example:

`files/King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit/SomeTable.jit`

CI copies these files onto the pinned distributed baseline before patching.
Update the corresponding SPRK-Native\*.json integrity manifests when changing
baked tables. Source-generated transformations belong in the patcher's full build
code. All necessary sources/tools must be tracked in this repository; no JSON
build recipe is required.

The patcher can delete obsolete files from its disposable build directory; the
publisher records their removal relative to the previous signed release.
Do not place complete installed clients, backups, or signing keys here.

The three Jane Cos21 bundles under `files/` restore missing legend body/face
materials and textures from the pre-Doomsday archive.

`PlainAsset/Text/CutScene/CH9_Sub_S_3_1_1.bjs` under the native patch
directory restores chapter 9 Scarlet 1 from the pre-Doomsday Android build
1558. The distributed Windows file was a 9-byte binary JSON null, which
triggers the cutscene player's `Empty` fallback. The restored script is
121,733 bytes with 47 scene groups, SHA-256
`4cd1ef5caa8180aec94438cbf6cbe8f7dcbe2c0af0f960936f30b07ae7a32bd7`.
Include this file alongside the existing complete managed selection when
publishing locally; the isolated CI build includes override files automatically.

The October 20, 2026 audit compared all 30 chapter 9/X SubStory rows and
their event sequences against the pre-Doomsday tables, and decoded 244
chapter-related cutscene scripts. Theo 1 (`CH9_SUB_T_2_1_1`) and Ophelia 1
(`CH9_Sub_O_30_1_1`) match the original scripts, including their short length.
Three differences in main-story/replay scripts are outside this substory fix;
existing Windows versions are preserved.
