# Build and host client updates through GitHub

Normal pushes to `main` build the client update and server from the same commit
on GitHub-hosted runners. No local upload or SSH/SFTP deployment is required
after the one-time baseline setup.

## Pipeline

1. Ubuntu runs the server tests and client build-tool tests.
2. Windows downloads the immutable baseline pinned by `scripts/client_baseline.py`,
   verifies its SHA-256 and file hashes, and verifies the original distribution's
   signed manifest against the existing public key.
3. It applies tracked files from `client-overrides/files/` and runs the patcher's
   single `--build-release` command. That command owns the full patch path,
   companion helpers, and verification. The telemetry-free distribution is retained;
   no deleted telemetry-removal Python script is needed.
4. It generates a complete signed snapshot, using the previous GitHub client
   release's verified manifest for deletion history. On the first build, the
   baseline distribution supplies that history.
5. It publishes both `client-update-manifest.json` and `client-update.tar.gz` as
   `client-ci-<commit-prefix>-<run-id>-<attempt>`. Assets go to a draft before
   publishing. Existing releases are never replaced.
6. Ubuntu builds the server image with that exact client tag, pushes to GHCR,
   and calls Portainer only after the client release is available.
7. At startup, the update container downloads that exact release, verifies its
   signature and every file hash, installs it, atomically replaces the stable
   manifest last, removes older local releases, and only then starts serving requests.

Running servers never poll or follow GitHub's moving latest release. An old
image retains its old client tag. A cached release is verified and reused without
GitHub access. A missing/corrupt required release stops startup and leaves
previously installed releases on disk. After a successful activation (including
cached startup), cleanup keeps the active release and the two most recently
published other releases, ordered by their signed `published_at` timestamps.
There is no grace period. Older recognized release directories are deleted
immediately; GitHub releases and the CI baseline are unaffected. Unrecognized
folders, invalid manifests, and symlinks/junctions are left untouched. Cleanup
errors are logged and do not prevent a verified active release from being served.

## Baseline and signing

The initial baseline is the existing distributed signed snapshot plus a support
file referenced by its baked-data integrity metadata. It preserves existing
patches, native tables, privacy removals and managed file selection. Its archive
location and SHA-256 are recorded in `scripts/client_baseline.py`. This fixed pin
is maintained by the baseline preparation tool; routine patcher changes never
require editing it. No signing keys or user
configuration enter the archive.

`config/client-update-public.pem` is the existing public key, tracked and included
in the server image. The existing private key is stored as
`CLIENT_UPDATE_SIGNING_KEY` in the repository's `client-release` Actions
environment, restricted to branch `main`. PR jobs receive no signing key and
cannot publish or deploy. The helper removes the secret value from child-process
environments and removes its temporary key file after signing. Private keys never
enter Git, release assets or container images.

The current repository's baseline and environment are already configured. For a
new repository, run `python scripts/prepare_client_baseline.py --upload --repo
owner/repository` once against its signed distribution, commit the generated `scripts/client_baseline.py`
and the matching public key, and configure the environment secret. Baseline
preparation refuses to overwrite its local archive or an existing release.

## Portainer setup

Use `docker-compose.yml` as the existing stack's Compose path. The former
`docker-compose.ghcr.yml` has been consolidated into this file; change the path
once if the stack still uses that old filename. The update
bind mount must be writable, and its host directory must be writable by container
UID/GID `10001`. Retain the stack name and volumes. An old read-only mount cannot
sync releases.

The image supplies its repository, client tag and public key file. Leave
`SPRK_UPDATES_GITHUB_REPO` and `SPRK_UPDATES_GITHUB_TAG` unset in Portainer to
preserve those defaults. `SPRK_UPDATES_PUBLIC_KEY` is an optional multiline
public-key override; normally leave it empty. Never put a private key in
Portainer. Allow outbound HTTPS to GitHub's API and release asset hosts.
Public downloads require no GitHub token on the server.

The update container listens only after verification; its healthcheck has a longer
startup grace period for the first download. Logs identify the pinned tag and
cached-release reuse. Archives are limited to 2 GiB.

## Changing client inputs

Change patcher code and embedded resources under `scripts/DllPatcher`, commit to
`development`, then merge the PR to `main`. CI builds and publishes the new
client and server automatically. No release ID, JSON recipe, or manual upload
is needed. A new patch must be called by the patcher's full build path; existing
patch fixes use their existing calls. Both the build logic and its verification
live in patcher code, including `BuildRelease.cs`.

Place native table/asset replacements under `client-overrides/files/`, preserving
client-relative paths. Keep changed tables and their SPRK-Native*.json integrity
metadata together. Source-generated transformations belong in the patcher's
full build code. All sources and tools must be tracked. The sibling table-data project currently
decodes tables; it is not a complete baked-data regeneration pipeline. Untracked
local changes cannot become CI inputs automatically.

The isolated build starts with the baseline and tracked overrides. CI packages
all resulting client files automatically, excluding generated backups/staging
files. If the patcher deletes an obsolete file, the publisher carries that
removal forward relative to the previous signed release. Local hosting keeps
the active release plus two previous releases; downloads of a pruned release
must restart using the current stable manifest.

## Local validation and recovery

Run a complete local build without modifying the installed client:

```powershell
python scripts/build_client_release.py --baseline client-updates/ci-baseline/client-baseline.tar.gz --work client-updates/ci-validation/new-build --version validation-unique-id --signing-key client-updates/update-signing/private.pem
```

Choose a fresh work path and release ID. Without `--publish`, no GitHub release
or deployment is created and the normal local stable manifest stays unchanged.
CI uses `--publish` to fetch previous deletion history and upload the new release.
Manifest verification and packaging are shared through `client_update_bundle.py`;
the former standalone upload command has been removed.

The workflow's optional `client_release_tag` input skips the client build and
reuses a known compatible published release for that run. Subsequent normal
pushes build a new client. Roll back by redeploying a previous commit-tagged
server image, which carries its previous client tag.

Pinning does not atomically switch separate game/update containers or downgrade
clients that already installed a newer patch. Use maintenance for incompatible
API changes, or maintain compatibility during rollout.

References: [GitHub releases API](https://docs.github.com/en/rest/releases/releases)
and [environment secrets](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets).
