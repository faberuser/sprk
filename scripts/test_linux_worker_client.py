import io
from pathlib import Path
import subprocess
import tarfile
import tempfile
import unittest

from client_update_bundle import package
from linux_worker import DATA, assemble, digest, extract_runtime, runtime_files
from publish_client_update import find_openssl, publish


class LinuxWorkerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.runtime = self.root / "runtime.tar.gz"
        self.runtime_entries = {"King's Raid.exe": b"engine", "UnityPlayer.dll": b"player",
                                "MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll": b"mono",
                                DATA + "/obsolete": b"old"}
        self.write_tar(self.runtime, self.runtime_entries)
        self.pin = {"sha256": digest(self.runtime)}
        self.private = self.root / "private.pem"
        self.public = self.root / "public.pem"
        subprocess.run([find_openssl(), "genpkey", "-algorithm", "RSA", "-pkeyopt", "rsa_keygen_bits:2048", "-out", str(self.private)], check=True, capture_output=True)
        subprocess.run([find_openssl(), "pkey", "-in", str(self.private), "-pubout", "-out", str(self.public)], check=True, capture_output=True)
        self.client = self.root / "client"
        for name in (DATA + "/globalgamemanagers", DATA + "/Managed/SprkConquest.dll", DATA + "/obsolete"):
            path = self.client / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(b"signed data")
        self.updates = self.root / "updates"
        publish(self.client, self.updates, "old", "stable", [DATA], self.private)
        (self.client / DATA / "obsolete").unlink()
        publish(self.client, self.updates, "current", "stable", [DATA], self.private)
        self.release, _ = package(self.updates, "current", self.public, self.root / "bundle")
        self.output = self.root / "worker"

    @staticmethod
    def write_tar(path, entries):
        with tarfile.open(path, "w:gz") as bundle:
            for name, value in entries.items():
                member = tarfile.TarInfo(name)
                member.size = len(value)
                bundle.addfile(member, io.BytesIO(value))

    def build(self, tag="client-current"):
        assemble(self.output, self.runtime, self.release, self.pin, self.public, tag)

    def test_verified_snapshot_overlays_runtime_and_applies_deletions(self):
        self.build()
        self.assertEqual((self.output / "King's Raid.exe").read_bytes(), b"engine")
        self.assertEqual((self.output / DATA / "Managed/SprkConquest.dll").read_bytes(), b"signed data")
        self.assertFalse((self.output / DATA / "obsolete").exists())
        with self.assertRaisesRegex(ValueError, "fresh"):
            self.build()

    def test_wrong_pin_or_release_version_never_activates(self):
        with self.assertRaisesRegex(ValueError, "signed version"):
            self.build("client-other")
        self.assertFalse(self.output.exists())
        self.pin["sha256"] = "0" * 64
        with self.assertRaisesRegex(ValueError, "SHA-256"):
            self.build()
        self.assertFalse(self.output.exists())

    def test_tampered_signed_file_never_activates(self):
        with tarfile.open(self.release) as bundle:
            entries = {m.name: bundle.extractfile(m).read() for m in bundle}
        entries["files/" + DATA + "/Managed/SprkConquest.dll"] = b"altered!!!!"
        self.write_tar(self.release, entries)
        with self.assertRaisesRegex(ValueError, "mismatch"):
            self.build()
        self.assertFalse(self.output.exists())

    def test_unsafe_runtime_members_are_rejected_before_extraction(self):
        self.write_tar(self.runtime, {"../escape": b"bad"})
        with self.assertRaisesRegex(ValueError, "Unsafe"):
            extract_runtime(self.runtime, self.output, digest(self.runtime))
        self.assertFalse(self.output.exists())
        self.assertFalse((self.root / "escape").exists())

    def test_runtime_selection_excludes_user_data_and_managed_code(self):
        for name in ("King's Raid.exe", "UnityPlayer.dll", "conquest-service.key", "sprk-launcher.json",
                     DATA + "/level0", DATA + "/resources.assets", DATA + "/Documents/account.json"):
            path = self.client / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(b"fixture")
        selected = {p.relative_to(self.client).as_posix() for p in runtime_files(self.client)}
        self.assertEqual(selected, {"King's Raid.exe", "UnityPlayer.dll", DATA + "/level0", DATA + "/resources.assets"})


if __name__ == "__main__":
    unittest.main()
