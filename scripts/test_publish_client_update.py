"""Publisher checks using small fixtures rather than the installed game client."""
import hashlib
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

import publish_client_update as publisher


class PublishTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="sprk-publish-test-")
        self.addCleanup(self.temp.cleanup)
        self.client = Path(self.temp.name) / "client"
        self.output = Path(self.temp.name) / "updates"
        for include in publisher.DEFAULT_INCLUDES:
            directory = self.client / include
            directory.mkdir(parents=True)
            (directory / "sample.bin").write_bytes(b"original")
        self.dll = self.client / publisher.DEFAULT_INCLUDES[0] / "sample.bin"

    def publish(self, version):
        return publisher.publish(self.client, self.output, version, "stable", publisher.DEFAULT_INCLUDES)

    def test_complete_snapshots_have_matching_hashes_and_exclude_backups(self):
        self.dll.with_suffix(".dll.backup-20261008").write_bytes(b"backup")
        self.dll.with_suffix(".dll.patched").write_bytes(b"staged")
        first = self.publish("1.0.0")
        self.dll.write_bytes(b"new code")
        second = self.publish("1.0.1")
        self.assertEqual(len(first["files"]), 4)
        self.assertEqual(len(second["files"]), 4)
        for manifest in [first, second]:
            for item in manifest["files"]:
                content = (self.output / "releases" / manifest["version"] / "files" / item["path"]).read_bytes()
                self.assertEqual(item["size"], len(content))
                self.assertEqual(item["sha256"], hashlib.sha256(content).hexdigest())
                self.assertIn("King%27s%20Raid_Data", item["url"])
        self.assertEqual(json.loads((self.output / "stable/manifest.json").read_text()), second)
        with self.assertRaisesRegex(ValueError, "already exists"):
            self.publish("1.0.0")

    def test_removals_survive_skipped_versions_and_readded_files(self):
        self.publish("1")
        self.dll.unlink()
        removed = self.publish("2")
        self.assertEqual(removed["deleted_files"], [self.dll.relative_to(self.client).as_posix()])
        self.assertEqual(self.publish("3")["deleted_files"], removed["deleted_files"])
        self.dll.write_bytes(b"restored")
        self.assertEqual(self.publish("4")["deleted_files"], [])

    def test_failed_copy_does_not_change_channel_or_expose_partial_release(self):
        self.publish("1")
        channel = (self.output / "stable/manifest.json").read_bytes()
        with patch.object(publisher, "copy_and_hash", side_effect=OSError("interrupted copy")):
            with self.assertRaises(OSError):
                self.publish("2")
        self.assertEqual((self.output / "stable/manifest.json").read_bytes(), channel)
        self.assertFalse((self.output / "releases/2").exists())
        self.assertFalse((self.output / ".publish-lock").exists())

    def test_rejects_unsafe_selection_and_output_within_client(self):
        with self.assertRaises(ValueError):
            publisher.publish(self.client, self.client / "updates", "1", "stable", publisher.DEFAULT_INCLUDES)
        for path in ["../secret", "/absolute", "dir/C:secret", "dir/file.", "dir/.hidden"]:
            with self.assertRaises(ValueError):
                publisher.safe_path(path)


if __name__ == "__main__":
    unittest.main()
