from pathlib import Path
import sys
import tempfile
import unittest

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.publish_client_update import DEFAULT_INCLUDES, UNMANAGED_LIST, load_unmanaged, publish


class ClientFixture(unittest.TestCase):
    """A minimal client with the default selection and one extra asset file."""

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



class CarriedFilePublishingTests(ClientFixture):
    """Files outside the default selection stay managed once a release contains them."""

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



class UnmanagedFileTests(ClientFixture):
    """Files a release stops managing are left out without being deleted on players' machines."""

    def test_unmanaged_file_leaves_the_release_without_a_deletion(self):
        self.publish("first", DEFAULT_INCLUDES + [self.relative])
        manifest = self.publish("second", DEFAULT_INCLUDES, unmanaged=[self.relative])
        self.assertNotIn(self.relative, {f["path"] for f in manifest["files"]})
        self.assertNotIn(self.relative, manifest["deleted_files"])
        self.assertTrue(self.extra.is_file())
        # Later releases do not pick it up again, and still never delete it.
        later = self.publish("third", DEFAULT_INCLUDES, unmanaged=[self.relative])
        self.assertNotIn(self.relative, {f["path"] for f in later["files"]})
        self.assertNotIn(self.relative, later["deleted_files"])

    def test_folder_entries_cover_everything_inside(self):
        folder = self.relative.rsplit("/", 1)[0] + "/"
        self.publish("first", DEFAULT_INCLUDES + [self.relative])
        manifest = self.publish("second", DEFAULT_INCLUDES + [self.relative], unmanaged=[folder])
        self.assertNotIn(self.relative, {f["path"] for f in manifest["files"]})
        self.assertNotIn(self.relative, manifest["deleted_files"])
        self.assertIn("King's Raid_Data/Managed/fixture.dll", {f["path"] for f in manifest["files"]})

    def test_an_earlier_deletion_is_withdrawn_once_unmanaged(self):
        self.publish("first", DEFAULT_INCLUDES + [self.relative])
        self.assertIn(self.relative, self.publish("second", DEFAULT_INCLUDES, deleted_files=[self.relative])["deleted_files"])
        self.assertNotIn(self.relative, self.publish("third", DEFAULT_INCLUDES, unmanaged=[self.relative])["deleted_files"])

    def test_deleting_and_unmanaging_the_same_file_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "both delete and stop managing"):
            self.publish("first", DEFAULT_INCLUDES, deleted_files=[self.relative], unmanaged=[self.relative])

    def test_list_parsing_and_the_tracked_list(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "list.txt"
            path.write_text("# comment\n\nKing's Raid_Data/a.bin\nKing's Raid_Data/Video/\n", encoding="utf-8")
            self.assertEqual(load_unmanaged(path), ["King's Raid_Data/a.bin", "King's Raid_Data/Video/"])
            path.write_text("../outside.bin\n", encoding="utf-8")
            with self.assertRaises(ValueError):
                load_unmanaged(path)
            self.assertEqual(load_unmanaged(Path(folder) / "missing.txt"), [])
        load_unmanaged(UNMANAGED_LIST)  # the tracked list must always parse


if __name__ == "__main__":
    unittest.main()
