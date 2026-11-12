import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

from client.scripts.recovery.managed_source_experiment import DATA, digest, prepare, repair_null_discards


class SnapshotTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.client = self.root / "client"
        self.managed = self.client / DATA / "Managed"
        self.managed.mkdir(parents=True)
        (self.managed / "Assembly-CSharp.dll").write_bytes(b"fixture")

    def test_rejects_overlapping_paths_before_writes(self):
        for work in (self.client, self.client / "experiment", self.root):
            with self.subTest(work=work), self.assertRaises(ValueError):
                prepare(self.client, work)
        self.assertFalse((self.client / "experiment").exists())

    def test_null_discard_repair_preserves_other_statements_and_is_idempotent(self):
        source = self.root / "Sample.cs"
        source.write_bytes(b"\t_ = null;\r\n\t_ = SideEffect();\r\n")
        repairs = repair_null_discards(self.root)
        self.assertEqual(source.read_bytes(), b"\t_ = (object)null;\r\n\t_ = SideEffect();\r\n")
        self.assertEqual(repairs[0]["count"], 1)
        self.assertEqual(repair_null_discards(self.root), [])

    @patch("client.scripts.recovery.managed_source_experiment.subprocess.check_output", return_value="fixture\n")
    def test_snapshot_excludes_credentials_and_refuses_reuse(self, _):
        (self.client / "accounts.json").write_text("secret", encoding="utf-8")
        (self.managed / "local-settings.json").write_text("secret", encoding="utf-8")
        work = self.root / "experiment"
        refs = prepare(self.client, work)
        self.assertEqual([p.name for p in refs.iterdir()], ["Assembly-CSharp.dll"])
        manifest = json.loads((work / "inputs.json").read_text(encoding="utf-8"))
        self.assertEqual(manifest["managed"][0]["sha256"], digest(refs / "Assembly-CSharp.dll"))
        with self.assertRaises(FileExistsError):
            prepare(self.client, work)
        self.assertEqual((self.managed / "Assembly-CSharp.dll").read_bytes(), b"fixture")


if __name__ == "__main__":
    unittest.main()
