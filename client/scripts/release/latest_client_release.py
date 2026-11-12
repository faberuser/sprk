#!/usr/bin/env python3
"""Find the newest complete published client release.

CI (no arguments): reads RELEASE_REPOSITORY, uses the gh CLI and writes
tag=<client-...> to GITHUB_OUTPUT, so the PR native-battle and server-image
jobs test against the same signed worker release.

Locally: `latest_client_release.py --repo owner/name` prints the tag, using the
public GitHub API (no gh CLI or token needed; GH_TOKEN/GITHUB_TOKEN is used if set).
"""
import argparse
import json
import os
import re
import subprocess
import urllib.request

REQUIRED_ASSETS = {"client-update-manifest.json", "client-update.tar.gz"}


def latest(releases: list[dict]) -> str:
    candidates = [r for r in releases if not r["draft"] and not r["prerelease"]
                  and re.fullmatch(r"client-[A-Za-z0-9][A-Za-z0-9_.-]*", r["tag_name"])
                  and REQUIRED_ASSETS <= {a["name"] for a in r["assets"] if a["state"] == "uploaded"}]
    if not candidates:
        raise SystemExit("No complete published client release is available")
    return max(candidates, key=lambda r: r["published_at"])["tag_name"]


def releases_from_api(repo: str) -> list[dict]:
    if not re.fullmatch(r"[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+", repo):
        raise SystemExit(f"Expected owner/repository, got {repo!r}")
    request = urllib.request.Request(f"https://api.github.com/repos/{repo}/releases?per_page=100",
                                     headers={"Accept": "application/vnd.github+json", "User-Agent": "sprk-local-server"})
    token = os.environ.get("GH_TOKEN") or os.environ.get("GITHUB_TOKEN")
    if token:
        request.add_header("Authorization", f"Bearer {token}")
    with urllib.request.urlopen(request, timeout=30) as response:
        return json.load(response)


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--repo", help="Print the newest release tag of this repository (local use)")
    args = parser.parse_args(argv)
    if args.repo:
        print(latest(releases_from_api(args.repo)))
        return
    repo = os.environ["RELEASE_REPOSITORY"]
    releases = json.loads(subprocess.check_output(["gh", "api", f"repos/{repo}/releases?per_page=100"]))
    tag = latest(releases)
    with open(os.environ["GITHUB_OUTPUT"], "a") as output:
        output.write(f"tag={tag}\n")
    print(f"PR server compatibility tests will use signed worker release {tag}")


if __name__ == "__main__":
    main()
