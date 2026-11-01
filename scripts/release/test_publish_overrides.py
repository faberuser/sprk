from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[2]))

from scripts.release.publish_client_update import DEFAULT_INCLUDES, publish


class RestoredOverridePublishingTests(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="sprk-restored-publish-")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.client = self.root / "client"
        self.overrides = self.root / "overrides"
        self.relative = "King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle/OriginalSpec/Creature_Hero_Jane_Cos21.unity3d"
        for relative in DEFAULT_INCLUDES:
            target = self.client / relative
            if target.name in {"app.info", "boot.config", "globalgamemanagers"}:
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(b"client fixture")
            else:
                target.mkdir(parents=True, exist_ok=True)
        (self.client / "King's Raid_Data/Managed/fixture.dll").write_bytes(b"managed fixture")
        source = self.overrides / self.relative
        source.parent.mkdir(parents=True)
        source.write_bytes(b"repository restoration")

    def publish(self, version, includes):
        with patch("scripts.release.publish_client_update.OVERRIDE_ROOT", self.overrides):
            return publish(self.client, self.root / "updates", version, "stable", includes)

    def test_complete_snapshots_keep_installed_restorations_across_updates(self):
        installed = self.client / self.relative
        installed.parent.mkdir(parents=True)
        installed.write_bytes(b"verified installed restoration")
        first = self.publish("first", DEFAULT_INCLUDES)
        second = self.publish("second", DEFAULT_INCLUDES)
        self.assertIn(self.relative, {f["path"] for f in first["files"]})
        self.assertNotIn(self.relative, second["deleted_files"])
        packaged = self.root / "updates/releases/second/files" / self.relative
        self.assertEqual(packaged.read_bytes(), installed.read_bytes())

    def test_missing_installation_is_not_published_from_repository(self):
        manifest = self.publish("missing", DEFAULT_INCLUDES)
        self.assertNotIn(self.relative, {f["path"] for f in manifest["files"]})

    def test_explicit_partial_selection_keeps_its_scope(self):
        installed = self.client / self.relative
        installed.parent.mkdir(parents=True)
        installed.write_bytes(b"installed restoration")
        manifest = self.publish("partial", ["King's Raid_Data/Managed"])
        self.assertEqual({f["path"] for f in manifest["files"]}, {"King's Raid_Data/Managed/fixture.dll"})


if __name__ == "__main__":
    unittest.main()
