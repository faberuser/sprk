# Build and host client updates through GitHub

Normal pushes to `main` compile the server and resolve the client update in
parallel on GitHub-hosted runners. Changed client inputs produce a new signed
release; unchanged inputs reuse the newest complete release with a matching
signed `build_inputs_sha256`. No local upload or SSH/SFTP deployment is required.

## Pipeline

[PR tests](../.github/workflows/test-pr.yml) run server, packaging and native
battle checks. [Build and deploy](../.github/workflows/build-and-deploy.yml)
runs on pushes to `main` or manual dispatch, publishing the client before
building and validating the server image and calling Portainer.

1. On PRs, Ubuntu runs the server and client build-tool tests in parallel with
   native battle integration checks under Wine using the latest complete signed
   published worker release, and Windows runs the complete client release build
   with a throwaway signing key (`--check-only`), so a client compile error
   fails the PR. New commits cancel obsolete PR runs. Rust dependencies
   are cached on the runner and in Docker dependency layers. `Dockerfile.tests`
   shares the production toolchain and Wine runtime through `docker-bake.hcl`
   without building the optimized server binary. The Wine checks use the
   published client, not the one built from the PR.
   Native combat tests no longer run in the deployment job.
2. Windows fingerprints the client source, overrides, build and release
   scripts, SDK pin, public key and the deployment workflow. Only the newest
   complete published client's verified manifest can authorize automatic reuse,
   preserving cumulative deleted paths. Manifests without a fingerprint require a
   new build. A manual `client_release_tag` override still verifies the release;
   `force_client_build` forces a rebuild and cannot be combined with an override.
   Reuse skips the toolchain setup. Builds report phase timings in logs and the
   job summary.
   When building, Windows downloads the published release pinned by
   `client/scripts/release/client_baseline.py` and verifies its SHA-256, its
   signature against the tracked public key, and every file hash.
3. It applies tracked files from `client/overrides/files/`, then compiles
   `client/managed` against that release's runtime DLLs with
   `client/scripts/build/build_managed_client.py` and replaces
   `Assembly-CSharp.dll` and every `Sprk*.dll`. A compile error fails the job.
4. It generates a complete signed snapshot, using the previous GitHub client
   release's verified manifest for deletion history. On the first build, the
   baseline distribution supplies that history.
5. It publishes both `client-update-manifest.json` and `client-update.tar.gz` as
   `client-ci-<commit-prefix>-<run-id>-<attempt>`. Assets go to a draft before
   publishing. Existing releases are never replaced.
6. Ubuntu assembles the [Linux battle worker](linux-battle-worker.md) from the
   pinned Unity runtime and that verified client release. A parallel Linux job
   exports the release-mode server binary as a seven-day artifact scoped to this run;
   final image assembly downloads that exact artifact by ID and restores executable permissions,
   without recompiling the server. It builds the image,
   and requires worker readiness in an isolated container before pushing to
   GHCR and calling Portainer.
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

The baseline is an ordinary published client release, pinned by tag and archive
SHA-256 in `client/scripts/release/client_baseline.py`. It supplies everything
that is not compiled: tables, asset bundles, Unity data files, privacy removals
and the runtime DLLs. The current pin is the last release produced by the retired
DLL patcher. Routine changes never edit the pin. To adopt a newer release as the
starting point (for example to fold accumulated overrides into it):

```powershell
python client/scripts/release/pin_client_baseline.py --tag client-<version>
```

Then delete the override files that release already contains. No signing keys
or user configuration are part of a release archive.

`config/client-update-public.pem` is the existing public key, tracked and included
in the server image. The existing private key is stored as
`CLIENT_UPDATE_SIGNING_KEY` in the repository's `client-release` Actions
environment, restricted to branch `main`. PR jobs receive no signing key and
cannot publish or deploy. The helper removes the secret value from child-process
environments and removes its temporary key file after signing. Private keys never
enter Git, release assets or container images.

The current repository's baseline and environment are already configured. A
new repository needs one complete signed client release to pin, the matching
public key in `config/`, and the environment secret.

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

Edit C# under `client/managed`, commit to `development`, then merge the PR to
`main`. CI builds and publishes the new client and server automatically. No
release ID, JSON recipe, or manual upload is needed.

Place table/asset replacements under `client/overrides/files/`, preserving
client-relative paths and in their final form. Keep changed tables and their
`SPRK-Native*.json` integrity metadata together. All inputs must be tracked;
local changes to an installed client cannot become CI inputs.

The isolated build starts with the baseline, applies overrides and installs the
compiled assemblies. CI packages all resulting client files automatically,
excluding generated backups/staging files. A feature assembly removed from
`client/managed` is removed from the client, and the publisher carries that
deletion forward relative to the previous signed release. Local hosting keeps
the active release plus two previous releases; downloads of a pruned release
must restart using the current stable manifest.

Paths listed in `client/scripts/release/unmanaged_files.txt` are left out of
every release without being marked deleted, so players keep their copies but
the launcher no longer updates or repairs them. Both CI and the local publisher
read the list; the CI log reports how many files it kept out. See the list file
for when a file may be added.

## Local validation and recovery

Run a complete local build without modifying the installed client:

```powershell
python client/scripts/release/build_client_release.py --work artifacts/release-check --version check-unique-id --check-only
```

Choose a fresh work path and release ID. `--check-only` signs with a throwaway
key; use `--signing-key client-updates/update-signing/private.pem` instead to
produce a release the launcher would accept. Without `--publish`, no GitHub
release or deployment is created and the normal local stable manifest stays
unchanged. Pass `--baseline <client-update.tar.gz>` to reuse a downloaded
baseline archive. CI uses `--publish` to fetch previous deletion history and
upload the new release. Manifest verification and packaging are shared through
`client_update_bundle.py`.

The workflow's optional `client_release_tag` input skips the client build and
reuses a known compatible published release for that run. Subsequent normal
pushes build a new client. Roll back by redeploying a previous commit-tagged
server image, which carries its previous client tag.

Pinning does not atomically switch separate game/update containers or downgrade
clients that already installed a newer patch. Use maintenance for incompatible
API changes, or maintain compatibility during rollout.

References: [GitHub releases API](https://docs.github.com/en/rest/releases/releases)
and [environment secrets](https://docs.github.com/en/actions/how-tos/write-workflows/choose-what-workflows-do/use-secrets).
