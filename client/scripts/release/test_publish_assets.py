import hashlib
import json
from pathlib import Path
import sys
import tempfile
import unittest

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.publish_assets import PATCH_PREFIX, publish
from client.scripts.release.publish_client_update import load_asset_set


class PublishAssetsTest(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="sprk-assets-")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.client = self.root / "client"
        self.store = self.root / "store"
        self.pin = self.root / "pin" / "StandaloneWindows.json"
        self.files = {
            "King's Raid.exe": b"player",
            "MonoBleedingEdge/mono.dll": b"mono",
            "King's Raid_Data/level0": b"level",
            "King's Raid_Data/Managed/Assembly-CSharp.dll": b"code",
            "King's Raid_Data/boot.config": b"startup",
            PATCH_PREFIX + "Video/intro.mp4": b"video",
            PATCH_PREFIX + "Assetbundle/[English]/A b.unity3d": b"bundle-a",
            PATCH_PREFIX + "Assetbundle/Shared.unity3d": b"same",
            PATCH_PREFIX + "Assetbundle/Copy.unity3d": b"same",
            PATCH_PREFIX + "PlainAsset/text.txt": b"plain",
            PATCH_PREFIX + "TableData/table.bytes": b"code-coupled",
            PATCH_PREFIX + "PatchInfo/patch.json": b"{}",
            PATCH_PREFIX + "patch.bjs": b"local state",
            PATCH_PREFIX + "SPRK-NativeData.json": b"{}",
            PATCH_PREFIX + "Assetbundle/Old.unity3d.part": b"partial download",
            "SprkLauncher.exe": b"launcher",
            "sprk-launcher.json": b"{}",
            ".sprk-launcher/file-hashes.json": b"{}",
        }
        for name, data in self.files.items():
            path = self.client / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
        (self.client / PATCH_PREFIX / "patch.json").write_text(json.dumps({"Archives": [{
            "AssetInfos": [{"EntryName": "PlainAsset/text.txt", "Cat": "3", "Pri": "7", "Time": "99"}],
        }]}))
        self.files[PATCH_PREFIX + "patch.json"] = (self.client / PATCH_PREFIX / "patch.json").read_bytes()

    def run_publish(self, **kwargs):
        return publish(self.client, self.store, "StandaloneWindows", pin=self.pin, log=lambda _: None, **kwargs)

    def version_dir(self, version):
        return self.store / "StandaloneWindows" / "versions" / str(version)

    def test_set_covers_the_client_except_code_launcher_and_patch_metadata(self):
        result = self.run_publish()
        self.assertEqual(result["version"], 1)
        version, files = load_asset_set(self.pin)
        self.assertEqual(version, "assets-1")
        self.assertEqual(sorted(f["path"] for f in files), sorted([
            "King's Raid.exe", "MonoBleedingEdge/mono.dll", "King's Raid_Data/level0",
            PATCH_PREFIX + "Assetbundle/[English]/A b.unity3d", PATCH_PREFIX + "Assetbundle/Copy.unity3d",
            PATCH_PREFIX + "Assetbundle/Shared.unity3d", PATCH_PREFIX + "PlainAsset/text.txt",
            # The bundle loader needs the game's patch lists; patch.bjs is game state.
            PATCH_PREFIX + "PatchInfo/patch.json", PATCH_PREFIX + "patch.json",
        ]))
        for item in files:
            data = self.files[item["path"]]
            self.assertEqual((item["size"], item["sha256"]), (len(data), hashlib.sha256(data).hexdigest()))
            self.assertEqual((self.store / "objects" / item["sha256"]).read_bytes(), data)
        self.assertEqual((self.version_dir(1) / "set.json").read_bytes(), self.pin.read_bytes())
        # Identical files share one object.
        self.assertEqual(result["new_objects"], 8)

    def test_game_patch_list_has_patch_folder_files_with_original_metadata(self):
        self.run_publish()
        patch = json.loads((self.version_dir(1) / "patch.json").read_text(encoding="utf-8"))
        names = sorted(a["Name"] for a in patch["Archives"])
        self.assertEqual(names, ["Assetbundle/Copy.unity3d", "Assetbundle/Shared.unity3d",
                                 "Assetbundle/[English]/A b.unity3d", "PlainAsset/text.txt"])
        index = json.loads((self.version_dir(1) / "index.json").read_text(encoding="utf-8"))["files"]
        text = next(a for a in patch["Archives"] if a["Name"] == "PlainAsset/text.txt")
        self.assertEqual((text["ArchiveType"], text["Cat"], text["Pri"], text["Time"]), ("Raw", "3", "7", "99"))
        self.assertEqual(text["AssetInfos"][0]["Md5"], hashlib.md5(b"plain").hexdigest())
        self.assertEqual(index["PlainAsset/text.txt"], hashlib.sha256(b"plain").hexdigest())
        self.assertEqual((self.store / "StandaloneWindows/current.txt").read_text(), "1")

    def test_next_version_adds_only_changed_objects_and_keeps_old_version(self):
        self.run_publish()
        (self.client / PATCH_PREFIX / "Assetbundle/Shared.unity3d").write_bytes(b"changed")
        result = self.run_publish()
        self.assertEqual((result["version"], result["new_objects"]), (2, 1))
        self.assertTrue((self.version_dir(1) / "set.json").exists())
        self.assertEqual(load_asset_set(self.pin)[0], "assets-2")
        with self.assertRaises(ValueError):
            self.run_publish(version=2)

    def test_not_activated_does_not_pin(self):
        self.run_publish(includes=["MonoBleedingEdge/"], activate=False)
        self.assertFalse(self.pin.exists())
        self.assertFalse((self.store / "StandaloneWindows/current.txt").exists())
        self.assertTrue((self.version_dir(1) / "set.json").exists())

    def test_rejects_names_the_server_cannot_serve(self):
        (self.client / "MonoBleedingEdge/.hidden").write_bytes(b"x")
        with self.assertRaises(ValueError):
            self.run_publish()
        self.assertFalse((self.store / "StandaloneWindows/.publish-lock").exists())


    def test_sync_copies_objects_then_versions_and_never_overwrites(self):
        from client.scripts.release.sync_assets import sync
        self.run_publish()
        dest = self.root / "host" / "assets"
        first = sync(self.store, dest, log=lambda _: None)
        self.assertEqual(first, {"objects": 8, "versions": 1})
        self.assertEqual((dest / "StandaloneWindows/current.txt").read_text(), "1")
        self.assertEqual((dest / "StandaloneWindows/versions/1/set.json").read_bytes(), self.pin.read_bytes())
        for item in load_asset_set(self.pin)[1]:
            self.assertEqual((dest / "objects" / item["sha256"]).read_bytes(), self.files[item["path"]])
        self.assertFalse((dest / ".sync-lock").exists())
        self.assertEqual(sync(self.store, dest, log=lambda _: None), {"objects": 0, "versions": 0})
        # A corrupt local object is refused, not uploaded.
        (self.client / "King's Raid.exe").write_bytes(b"player v2")
        self.run_publish()
        sha = hashlib.sha256(b"player v2").hexdigest()
        (self.store / "objects" / sha).write_bytes(b"tampered")
        with self.assertRaises(ValueError):
            sync(self.store, dest, log=lambda _: None)
        self.assertFalse((dest / "objects" / sha).exists())

if __name__ == "__main__":
    unittest.main()
