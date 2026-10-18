# Client build inputs

Place tracked replacement files under `files/`, preserving their path relative
to the client root, for example:

`files/King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit/SomeTable.jit`

CI copies these files onto the pinned distributed baseline before patching.
Update the corresponding SPRK-Native*.json integrity manifests when changing
baked tables. Source-generated transformations belong in the patcher's full build
code. All necessary sources/tools must be tracked in this repository; no JSON
build recipe is required.

The patcher can delete obsolete files from its disposable build directory; the
publisher records their removal relative to the previous signed release.
Do not place complete installed clients, backups, or signing keys here.
