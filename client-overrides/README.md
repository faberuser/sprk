# Client build inputs

Place tracked replacement files under `files/`, preserving their path relative
to the client root, for example:

`files/King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit/SomeTable.jit`

CI copies these files onto the pinned distributed baseline before patching.
Update the corresponding SPRK-Native*.json integrity manifests when changing
baked tables. For transformations built from source, add a command array to
`prepare_steps` in `client-build.json`; `{client}` expands to the disposable CI
client directory. All necessary sources/tools must be tracked in this repository.

Manage obsolete files through `deleted_files` in `client-build.json`.
Do not place complete installed clients, backups, or signing keys here.
