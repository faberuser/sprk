import json
from pathlib import Path
import subprocess
import tarfile
import tempfile
import unittest

from publish_client_update import find_openssl, publish
from client_update_bundle import package


class BundleTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory(prefix="sprk-bundle-test-")
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.private = self.root / "test-private.pem"
        self.public = self.root / "test-public.pem"
        subprocess.run([find_openssl(), "genpkey", "-algorithm", "RSA", "-pkeyopt", "rsa_keygen_bits:2048", "-out", str(self.private)], check=True, capture_output=True)
        subprocess.run([find_openssl(), "pkey", "-pubout", "-in", str(self.private), "-out", str(self.public)], check=True, capture_output=True)
        client = self.root / "client"
        self.relative = "King's Raid_Data/Managed/test.dll"
        (client / self.relative).parent.mkdir(parents=True)
        (client / self.relative).write_bytes(b"signed client fixture")
        self.updates = self.root / "updates"
        publish(client, self.updates, "fixture-1", "stable", ["King's Raid_Data/Managed"], self.private)
        self.release = self.updates / "releases/fixture-1"
        self.output = self.root / "bundle"

    def build(self):
        return package(self.updates, "fixture-1", self.public, self.output)

    def test_bundle_contains_only_signed_files_and_unchanged_manifest(self):
        (self.release / "private.pem").write_text("must not be included")
        archive, manifest = self.build()
        self.assertEqual(manifest.read_bytes(), (self.release / "manifest.json").read_bytes())
        with tarfile.open(archive) as bundle:
            self.assertEqual(set(bundle.getnames()), {"manifest.json", "files/" + self.relative})
            self.assertTrue(all(member.isfile() for member in bundle))
            self.assertEqual(bundle.extractfile("files/" + self.relative).read(), b"signed client fixture")

    def test_corruption_does_not_replace_existing_bundle(self):
        archive, manifest = self.build()
        before = archive.read_bytes(), manifest.read_bytes()
        (self.release / "files" / self.relative).write_bytes(b"wrong")
        with self.assertRaisesRegex(ValueError, "size/hash"):
            self.build()
        self.assertEqual((archive.read_bytes(), manifest.read_bytes()), before)

    def test_tampered_unsigned_fields_are_rejected(self):
        path = self.release / "manifest.json"
        manifest = json.loads(path.read_bytes())
        manifest["deleted_files"] = ["important.dll"]
        path.write_text(json.dumps(manifest))
        with self.assertRaisesRegex(ValueError, "signed payload"):
            self.build()
        self.assertFalse((self.output / "client-update.tar.gz").exists())

    def test_unsigned_and_missing_file_releases_are_rejected(self):
        (self.release / "files" / self.relative).unlink()
        with self.assertRaises(FileNotFoundError):
            self.build()
        path = self.release / "manifest.json"
        manifest = json.loads(path.read_bytes())
        manifest.pop("signature")
        path.write_text(json.dumps(manifest))
        with self.assertRaisesRegex(ValueError, "signed RSA"):
            self.build()


if __name__ == "__main__":
    unittest.main()
