import hashlib
import io
import json
from pathlib import Path
import subprocess
import tarfile
import tempfile
import unittest

import sys

# Allow both direct execution and package imports from any working directory.
if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from unittest.mock import patch

from client.scripts.release import build_client_release
from client.scripts.release.build_client_release import extract_baseline, file_hash, install_managed, managed_build_files, throwaway_key
from client.scripts.release.client_update_bundle import package, verified_manifest
from client.scripts.release.publish_client_update import find_openssl, ignored, publish


class BaselineTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="sprk-ci-input-test-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.key = self.root / "private.pem"
        self.public = self.root / "public.pem"
        subprocess.run([find_openssl(), "genpkey", "-algorithm", "RSA", "-pkeyopt", "rsa_keygen_bits:2048", "-out", str(self.key)], check=True, capture_output=True)
        subprocess.run([find_openssl(), "pkey", "-pubout", "-in", str(self.key), "-out", str(self.public)], check=True, capture_output=True)
        self.client = self.root / "installed"
        target = self.client / "King's Raid_Data/Managed/fixture.dll"
        target.parent.mkdir(parents=True)
        target.write_bytes(b"distributed fixture")
        self.updates = self.root / "updates"
        publish(self.client, self.updates, "fixture", "stable", ["King's Raid_Data/Managed"], self.key)
        # The baseline is an ordinary published release bundle.
        self.archive, _ = package(self.updates, "fixture", self.public, self.root / "assets")
        self.sha = file_hash(self.archive)

    def test_baseline_roundtrip_preserves_signed_snapshot(self):
        client = self.root / "build"
        raw = extract_baseline(self.archive, client, self.sha, self.public)
        self.assertEqual(verified_manifest(raw, self.public)["version"], "fixture")
        self.assertEqual((client / "King's Raid_Data/Managed/fixture.dll").read_bytes(), b"distributed fixture")

    def test_pinned_hash_and_foreign_signature_rejection(self):
        with self.assertRaisesRegex(ValueError, "SHA-256"):
            extract_baseline(self.archive, self.root / "wrong", "0" * 64, self.public)
        self.assertFalse((self.root / "wrong").exists())
        _, other = throwaway_key(self.root)
        with self.assertRaises(subprocess.CalledProcessError):
            extract_baseline(self.archive, self.root / "foreign", self.sha, other)

    def test_source_build_replaces_assemblies_and_drops_removed_helpers(self):
        managed = self.client / "King's Raid_Data/Managed"
        for name in ("Assembly-CSharp.dll", "SprkKept.dll", "SprkRemoved.dll"):
            (managed / name).write_bytes(b"baseline " + name.encode())
        built = self.root / "built"
        built.mkdir()
        outputs = {}
        for name in ("Assembly-CSharp", "SprkKept", "SprkAdded"):
            outputs[name] = built / (name + ".dll")
            outputs[name].write_bytes(b"source " + name.encode())
        with patch.object(build_client_release, "compile_managed", return_value=outputs) as compiler:
            self.assertEqual(install_managed(self.client, self.root / "work"), sorted(outputs))
        compiler.assert_called_once_with(managed, self.root / "work")
        self.assertEqual({p.name: p.read_bytes() for p in managed.iterdir()}, {
            "fixture.dll": b"distributed fixture", "Assembly-CSharp.dll": b"source Assembly-CSharp",
            "SprkKept.dll": b"source SprkKept", "SprkAdded.dll": b"source SprkAdded"})

    def test_rejects_links_unexpected_and_duplicate_files(self):
        with tarfile.open(self.archive) as source:
            originals = [(m.name, source.extractfile(m).read()) for m in source]
        for case in ("link", "unexpected", "duplicate"):
            destination = self.root / (case + ".tar.gz")
            with tarfile.open(destination, "w:gz") as archive:
                for name, payload in originals:
                    member = tarfile.TarInfo(name)
                    member.size = len(payload)
                    archive.addfile(member, io.BytesIO(payload))
                member = tarfile.TarInfo("manifest.json" if case == "duplicate" else "unexpected")
                if case == "link":
                    member.type = tarfile.SYMTYPE
                    member.linkname = "../outside"
                archive.addfile(member, io.BytesIO(b""))
            with self.assertRaises(ValueError):
                extract_baseline(destination, self.root / case, file_hash(destination), self.public)

    def test_generated_automation_backup_is_not_a_managed_client_file(self):
        self.assertTrue(ignored(Path("Assembly-CSharp.dll.before_automation_20261019_001")))
        self.assertTrue(ignored(Path("Assembly-CSharp.dll.backup_before_patch")))
        self.assertFalse(ignored(Path("Assembly-CSharp.dll")))

    def test_build_automatically_publishes_new_assets_and_carries_removed_files(self):
        original = "King's Raid_Data/Managed/fixture.dll"
        (self.client / original).unlink()
        added = "King's Raid_Data/NewAssets/options.json"
        asset = self.client / added
        asset.parent.mkdir(parents=True)
        asset.write_bytes(b"new tracked or generated asset")
        (self.client / (original + ".before_automation_001")).write_bytes(b"backup")
        manifest = publish(self.client, self.updates, "next-fixture", "stable",
                           managed_build_files(self.client), self.key)
        self.assertEqual([item["path"] for item in manifest["files"]], [added])
        self.assertIn(original, manifest["deleted_files"])


class CheckOnlyTests(unittest.TestCase):
    def test_throwaway_key_signs_verifiable_manifests_without_release_key(self):
        with tempfile.TemporaryDirectory(prefix="sprk-check-key-") as folder:
            root = Path(folder)
            private, public = throwaway_key(root)
            target = root / "client/King's Raid_Data/Managed/fixture.dll"
            target.parent.mkdir(parents=True)
            target.write_bytes(b"pr check fixture")
            publish(root / "client", root / "updates", "pr-check", "stable", ["King's Raid_Data/Managed"], private)
            raw = (root / "updates/stable/manifest.json").read_bytes()
            self.assertEqual(verified_manifest(raw, public)["version"], "pr-check")
            release = Path(__file__).resolve().parents[3] / "config/client-update-public.pem"
            with self.assertRaises(subprocess.CalledProcessError):
                verified_manifest(raw, release)

    def test_check_only_refuses_publish_or_release_key_before_network_access(self):
        script = Path(__file__).resolve().with_name("build_client_release.py")
        for extra in (["--publish"], ["--signing-key", "unused.pem"]):
            result = subprocess.run([sys.executable, "-I", str(script), "--work", "unused", "--version", "x",
                                     "--check-only", *extra], capture_output=True, text=True)
            self.assertEqual(result.returncode, 2, result.stderr)
            self.assertIn("--check-only cannot be combined", result.stderr)


if __name__ == "__main__":
    unittest.main()
