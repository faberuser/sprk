#!/usr/bin/env python3
"""Select the newest complete published client release for PR server checks.

Writes tag=<client-...> to GITHUB_OUTPUT. Shared by the PR native-battle and
server-image jobs so both test against the same signed worker release.
"""
import json
import os
import re
import subprocess

REQUIRED_ASSETS = {"client-update-manifest.json", "client-update.tar.gz"}


def latest(releases: list[dict]) -> str:
    candidates = [r for r in releases if not r["draft"] and not r["prerelease"]
                  and re.fullmatch(r"client-[A-Za-z0-9][A-Za-z0-9_.-]*", r["tag_name"])
                  and REQUIRED_ASSETS <= {a["name"] for a in r["assets"] if a["state"] == "uploaded"}]
    if not candidates:
        raise SystemExit("No complete published client release is available for PR server tests")
    return max(candidates, key=lambda r: r["published_at"])["tag_name"]


def main():
    repo = os.environ["RELEASE_REPOSITORY"]
    releases = json.loads(subprocess.check_output(["gh", "api", f"repos/{repo}/releases?per_page=100"]))
    tag = latest(releases)
    with open(os.environ["GITHUB_OUTPUT"], "a") as output:
        output.write(f"tag={tag}\n")
    print(f"PR server compatibility tests will use signed worker release {tag}")


if __name__ == "__main__":
    main()
