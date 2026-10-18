import hashlib
import io
import json
from pathlib import Path
import subprocess
import tarfile
import tempfile
import unittest

from build_client_release import extract_baseline, file_hash, validate_recipe
from prepare_client_baseline import prepare
from publish_client_update import find_openssl, ignored, publish


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
        self.archive = self.root / "seed.tar.gz"
        self.sha = prepare(self.updates, self.client, self.public, self.archive)

    def test_baseline_roundtrip_preserves_signed_snapshot(self):
        client = self.root / "build"
        metadata = extract_baseline(self.archive, client, self.sha, self.public)
        self.assertEqual(metadata["previous_manifest"]["version"], "fixture")
        self.assertEqual((client / "King's Raid_Data/Managed/fixture.dll").read_bytes(), b"distributed fixture")

    def test_reproducible_seed_and_pinned_hash_rejection(self):
        other = self.root / "other.tar.gz"
        self.assertEqual(prepare(self.updates, self.client, self.public, other), self.sha)
        with self.assertRaisesRegex(ValueError, "SHA-256"):
            extract_baseline(self.archive, self.root / "wrong", "0" * 64, self.public)
        self.assertFalse((self.root / "wrong").exists())

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
                member = tarfile.TarInfo("baseline.json" if case == "duplicate" else "unexpected")
                if case == "link":
                    member.type = tarfile.SYMTYPE
                    member.linkname = "../outside"
                archive.addfile(member, io.BytesIO(b""))
            with self.assertRaises(ValueError):
                extract_baseline(destination, self.root / case, file_hash(destination), self.public)

    def test_unknown_patcher_flags_fail_instead_of_silently_running_full_patch(self):
        config = {"schema_version": 1, "patch_steps": [["--does-not-exist"]], "verify_steps": [], "deleted_files": []}
        with self.assertRaisesRegex(ValueError, "Unknown DLL patcher"):
            validate_recipe(config)

    def test_generated_automation_backup_is_not_a_managed_client_file(self):
        self.assertTrue(ignored(Path("Assembly-CSharp.dll.before_automation_20261019_001")))
        self.assertTrue(ignored(Path("Assembly-CSharp.dll.backup_before_patch")))
        self.assertFalse(ignored(Path("Assembly-CSharp.dll")))


if __name__ == "__main__":
    unittest.main()
