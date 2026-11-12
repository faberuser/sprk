from pathlib import Path
import sys
import tempfile
import unittest

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.publish_client_update import DEFAULT_INCLUDES, publish


class CarriedFilePublishingTests(unittest.TestCase):
    """Files outside the default selection stay managed once a release contains them."""

    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="sprk-carried-publish-")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.client = self.root / "client"
        self.relative = "King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle/OriginalSpec/Creature_Hero_Jane_Cos21.unity3d"
        for relative in DEFAULT_INCLUDES:
            target = self.client / relative
            if target.name in {"app.info", "boot.config", "globalgamemanagers"}:
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(b"client fixture")
            else:
                target.mkdir(parents=True, exist_ok=True)
        (self.client / "King's Raid_Data/Managed/fixture.dll").write_bytes(b"managed fixture")
        self.extra = self.client / self.relative
        self.extra.parent.mkdir(parents=True)
        self.extra.write_bytes(b"installed asset")

    def publish(self, version, includes, **kwargs):
        return publish(self.client, self.root / "updates", version, "stable", includes, **kwargs)

    def test_default_selection_keeps_previously_managed_files(self):
        first = self.publish("first", DEFAULT_INCLUDES + [self.relative])
        self.assertIn(self.relative, {f["path"] for f in first["files"]})
        second = self.publish("second", DEFAULT_INCLUDES)
        self.assertIn(self.relative, {f["path"] for f in second["files"]})
        self.assertNotIn(self.relative, second["deleted_files"])
        packaged = self.root / "updates/releases/second/files" / self.relative
        self.assertEqual(packaged.read_bytes(), self.extra.read_bytes())

    def test_unmanaged_file_is_not_adopted(self):
        manifest = self.publish("first", DEFAULT_INCLUDES)
        self.assertNotIn(self.relative, {f["path"] for f in manifest["files"]})

    def test_removed_or_explicitly_deleted_file_is_retired(self):
        self.publish("first", DEFAULT_INCLUDES + [self.relative])
        explicit = self.publish("second", DEFAULT_INCLUDES, deleted_files=[self.relative])
        self.assertIn(self.relative, explicit["deleted_files"])
        self.publish("third", DEFAULT_INCLUDES + [self.relative])
        self.extra.unlink()
        missing = self.publish("fourth", DEFAULT_INCLUDES)
        self.assertIn(self.relative, missing["deleted_files"])

    def test_explicit_partial_selection_keeps_its_scope(self):
        self.publish("first", DEFAULT_INCLUDES + [self.relative])
        manifest = self.publish("partial", ["King's Raid_Data/Managed"])
        self.assertEqual({f["path"] for f in manifest["files"]}, {"King's Raid_Data/Managed/fixture.dll"})


if __name__ == "__main__":
    unittest.main()
