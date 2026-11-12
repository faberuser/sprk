import hashlib
from pathlib import Path
import tempfile
import unittest
import zipfile

from client.scripts.build.build_managed_client import stage_references


class ReferenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.inputs = self.root / "inputs"
        self.inputs.mkdir()
        self.data = b"api reference fixture"
        (self.inputs / "mscorlib.dll").write_bytes(self.data)
        self.manifest = {"files": [{"path": "mscorlib.dll", "size": len(self.data),
                                    "sha256": hashlib.sha256(self.data).hexdigest()}]}
        self.output = self.root / "output"

    def test_directory_pins_and_ignores_original_game(self):
        (self.inputs / "Assembly-CSharp.dll").write_bytes(b"must not be copied")
        stage_references(self.inputs, self.output, self.manifest)
        self.assertEqual([p.name for p in self.output.iterdir()], ["mscorlib.dll"])

    def test_corrupt_directory_rejected_before_copy(self):
        (self.inputs / "mscorlib.dll").write_bytes(b"corrupt")
        with self.assertRaisesRegex(ValueError, "missing or changed"):
            stage_references(self.inputs, self.output, self.manifest)
        self.assertFalse(self.output.exists())

    def test_game_template_and_path_traversal_rejected(self):
        for name in ("../escape.dll", "..\\escape.dll", "Assembly-CSharp.dll"):
            with self.subTest(name=name), self.assertRaises(ValueError):
                stage_references(self.inputs, self.output, {"files": [{"path": name}]})

    def test_verified_archive(self):
        archive = self.root / "api.zip"
        with zipfile.ZipFile(archive, "w") as bundle:
            bundle.writestr("mscorlib.dll", self.data)
        stage_references(archive, self.output, self.manifest)
        self.assertEqual((self.output / "mscorlib.dll").read_bytes(), self.data)

    def test_archive_rejects_extra_paths(self):
        archive = self.root / "api.zip"
        with zipfile.ZipFile(archive, "w") as bundle:
            bundle.writestr("mscorlib.dll", self.data)
            bundle.writestr("../escape.dll", b"bad")
        with self.assertRaisesRegex(ValueError, "exactly"):
            stage_references(archive, self.output, self.manifest)
        self.assertFalse(self.output.exists())

    def test_archive_rejects_same_size_corruption(self):
        archive = self.root / "api.zip"
        with zipfile.ZipFile(archive, "w") as bundle:
            bundle.writestr("mscorlib.dll", b"x" * len(self.data))
        with self.assertRaisesRegex(ValueError, "changed"):
            stage_references(archive, self.output, self.manifest)


if __name__ == "__main__":
    unittest.main()
