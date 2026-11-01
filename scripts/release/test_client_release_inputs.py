"""Regressions for client reuse, signed provenance and conservative invalidation."""
import json
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch

from scripts.release import client_release_inputs as inputs
from scripts.release import test_build_client_release as fixtures
from scripts.release.publish_client_update import publish


class FingerprintTests(unittest.TestCase):
    def test_content_additions_deletions_and_server_only_changes(self):
        with tempfile.TemporaryDirectory() as folder:
            repo = Path(folder)
            subprocess.run(["git", "init", "-q", str(repo)], check=True)
            script = repo / "scripts/client/patch.py"
            script.parent.mkdir(parents=True)
            script.write_bytes(b"original")
            subprocess.run(["git", "-C", str(repo), "add", "."], check=True)
            original = inputs.fingerprint(repo)
            (repo / "src").mkdir()
            (repo / "src/main.rs").write_text("server-only change")
            self.assertEqual(inputs.fingerprint(repo), original)
            script.write_bytes(b"changed")
            self.assertNotEqual(inputs.fingerprint(repo), original)
            script.write_bytes(b"original")
            table = repo / "tables/new.json"
            table.parent.mkdir()
            table.write_text("{}")
            self.assertNotEqual(inputs.fingerprint(repo), original)
            table.unlink()
            self.assertEqual(inputs.fingerprint(repo), original)
            script.unlink()
            self.assertNotEqual(inputs.fingerprint(repo), original)


class SelectionTests(unittest.TestCase):
    @staticmethod
    def release(tag, date="2026-11-01", **extra):
        return dict(tag_name=tag, published_at=date, draft=False, prerelease=False,
                    assets=[dict(name=n, state="uploaded") for n in inputs.REQUIRED_ASSETS], **extra)

    def select(self, releases, manifest, **kwargs):
        with patch.object(inputs.subprocess, "check_output", return_value=json.dumps(releases).encode()), \
                patch.object(inputs, "release_manifest", return_value=manifest) as verify:
            result = inputs.select("owner/repo", Path("public.pem"), "a" * 64, **kwargs)
            return result, verify

    def test_reuses_latest_matching_signed_fingerprint(self):
        release = self.release("client-latest")
        result, verify = self.select([release], {"build_inputs_sha256": "a" * 64})
        self.assertEqual(result, "client-latest")
        verify.assert_called_once_with("owner/repo", release, Path("public.pem"))

    def test_legacy_and_changed_inputs_require_new_build(self):
        for manifest in [{}, {"build_inputs_sha256": "b" * 64}]:
            self.assertEqual(self.select([self.release("client-latest")], manifest)[0], "")

    def test_never_reuses_older_matching_release_or_incomplete_release(self):
        latest = self.release("client-new", "2026-11-02")
        incomplete = self.release("client-incomplete", "2026-11-03")
        incomplete["assets"] = []
        result, verify = self.select([self.release("client-old"), latest, incomplete], {})
        self.assertEqual(result, "")
        verify.assert_called_once_with("owner/repo", latest, Path("public.pem"))

    def test_empty_list_builds_and_force_does_not_access_network(self):
        self.assertEqual(self.select([], {})[0], "")
        with patch.object(inputs.subprocess, "check_output") as network:
            self.assertEqual(inputs.select("repo", Path("key"), "a" * 64, force=True), "")
            network.assert_not_called()

    def test_manual_override_is_verified_and_conflicting_options_rejected(self):
        release = self.release("client-manual")
        result, verify = self.select(release, {}, tag="client-manual")
        self.assertEqual(result, "client-manual")
        verify.assert_called_once()
        for tag, force in [("invalid/tag", False), ("client-manual", True)]:
            with patch.object(inputs.subprocess, "check_output") as network:
                with self.assertRaises(ValueError):
                    inputs.select("repo", Path("key"), "a" * 64, tag, force)
                network.assert_not_called()


class SignedProvenanceTests(unittest.TestCase):
    def test_fingerprint_is_signed_and_tampering_is_rejected(self):
        fixture = fixtures.BaselineTests()
        fixture.setUp()
        self.addCleanup(fixture.doCleanups)
        manifest = publish(fixture.client, fixture.updates, "provenance", "stable",
                           ["King's Raid_Data/Managed"], fixture.key, build_inputs_sha256="a" * 64)
        release = SelectionTests.release("client-provenance")

        def download(command, **kwargs):
            folder = Path(command[command.index("--dir") + 1])
            (folder / "client-update-manifest.json").write_text(json.dumps(manifest))

        with patch.object(inputs, "subprocess", wraps=subprocess) as gh:
            gh.run.side_effect = download
            self.assertEqual(inputs.release_manifest("repo", release, fixture.public)["build_inputs_sha256"], "a" * 64)
            with self.assertRaisesRegex(ValueError, "signed manifest version"):
                inputs.release_manifest("repo", SelectionTests.release("client-wrong"), fixture.public)
            manifest["build_inputs_sha256"] = "b" * 64
            with self.assertRaisesRegex(ValueError, "signed payload"):
                inputs.release_manifest("repo", release, fixture.public)

    def test_incomplete_release_is_rejected_before_download(self):
        release = SelectionTests.release("client-incomplete")
        release["assets"] = []
        with patch.object(inputs.subprocess, "run") as network:
            with self.assertRaisesRegex(ValueError, "complete published"):
                inputs.release_manifest("repo", release, Path("key"))
            network.assert_not_called()
