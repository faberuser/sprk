import base64
import hashlib
import json
from pathlib import Path
import sys
import tarfile
import tempfile
from types import SimpleNamespace
import unittest
from unittest.mock import patch

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release import asset_overlay
from client.scripts.release.build_client_release import throwaway_key
from client.scripts.release.publish_assets import PATCH_PREFIX


def fake_convert(source: str, target: str, shader_map: dict):
    data = b"android:" + Path(source).read_bytes()
    Path(target).parent.mkdir(parents=True, exist_ok=True)
    Path(target).write_bytes(data)
    return target, {"textures": 1}


FAKE_TOOLS = SimpleNamespace(SKIPPED_PREFIXES=("Unity_2018/",), ANDROID_ONLY=("Shader.unity3d",),
                             NEUTRAL=("BundlePack.bin",), convert_one=fake_convert)


class AssetOverlayTest(unittest.TestCase):
    def setUp(self):
        temporary = tempfile.TemporaryDirectory(prefix="sprk-overlay-")
        self.addCleanup(temporary.cleanup)
        self.root = Path(temporary.name)
        self.client = self.root / "client"
        files = {
            "King's Raid_Data/Managed/Assembly-CSharp.dll": b"code",
            PATCH_PREFIX + "TableData/table.bytes": b"table",
            PATCH_PREFIX + "SPRK-NativeData.json": b"{}",
            PATCH_PREFIX + "Assetbundle/Atlas.unity3d": b"atlas",
            PATCH_PREFIX + "Assetbundle/BundlePack.bin": b"pack",
            PATCH_PREFIX + "Assetbundle/Unity_2018/Shader.unity3d": b"d3d",
            PATCH_PREFIX + "Assetbundle/Shader.unity3d": b"shader",
            PATCH_PREFIX + "PlainAsset/Text/Scene.bjs": b"scene",
        }
        self.manifest = {"version": "ci-1", "files": []}
        for name, data in files.items():
            path = self.client / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
            self.manifest["files"].append({"path": name, "size": len(data),
                                           "sha256": hashlib.sha256(data).hexdigest()})
        self.shader_map = self.root / "shader-map.json"
        self.shader_map.write_text("{}", encoding="utf-8")
        self.key, _ = throwaway_key(self.root)

    def build(self):
        with patch.object(asset_overlay, "android_tools", return_value=FAKE_TOOLS):
            return asset_overlay.build(self.client, self.manifest, self.root / "out", self.key,
                                       self.shader_map, jobs=1, log=lambda _: None)

    def test_overlay_lists_release_assets_and_converted_android_bundles(self):
        archive, manifest = self.build()
        signed = json.loads(manifest.read_text(encoding="utf-8"))
        payload = json.loads(base64.b64decode(signed["signature"]["payload"]))
        self.assertEqual(payload, {k: v for k, v in signed.items() if k != "signature"})
        self.assertEqual(payload["release"], "ci-1")
        windows = payload["platforms"]["StandaloneWindows"]
        self.assertEqual(windows["from"], "release")
        self.assertEqual(sorted(f["name"] for f in windows["files"]), [
            "Assetbundle/Atlas.unity3d", "Assetbundle/BundlePack.bin", "Assetbundle/Shader.unity3d",
            "Assetbundle/Unity_2018/Shader.unity3d", "PlainAsset/Text/Scene.bjs"])
        scene = next(f for f in windows["files"] if f["name"] == "PlainAsset/Text/Scene.bjs")
        self.assertEqual(scene["md5"], hashlib.md5(b"scene").hexdigest())
        android = {f["name"]: f for f in payload["platforms"]["Android"]["files"]}
        # Converted bundles plus the neutral data as it is; Android's own shader bundle
        # and the D3D ones stay out.
        self.assertEqual(sorted(android), ["Assetbundle/Atlas.unity3d", "Assetbundle/BundlePack.bin",
                                           "PlainAsset/Text/Scene.bjs"])
        self.assertEqual(android["Assetbundle/Atlas.unity3d"]["source"], "Assetbundle/Atlas.unity3d")
        self.assertEqual(android["PlainAsset/Text/Scene.bjs"]["source"], "PlainAsset/Text/Scene.bjs")
        self.assertEqual(android["PlainAsset/Text/Scene.bjs"]["sha256"], hashlib.sha256(b"scene").hexdigest())
        with tarfile.open(archive) as bundle:
            members = {m.name: bundle.extractfile(m).read() for m in bundle.getmembers()}
        self.assertEqual(members, {"Android/Assetbundle/Atlas.unity3d": b"android:atlas",
                                   "Android/Assetbundle/BundlePack.bin": b"pack",
                                   "Android/PlainAsset/Text/Scene.bjs": b"scene"})
        self.assertEqual(android["Assetbundle/Atlas.unity3d"]["sha256"], hashlib.sha256(b"android:atlas").hexdigest())

    def test_a_file_that_differs_from_the_manifest_is_rejected(self):
        (self.client / PATCH_PREFIX / "PlainAsset/Text/Scene.bjs").write_bytes(b"other")
        with self.assertRaises(ValueError):
            self.build()

    def test_a_failed_conversion_fails_the_build(self):
        def broken(source, target, shader_map):
            return target, "ValueError: cannot convert"
        tools = SimpleNamespace(**{**vars(FAKE_TOOLS), "convert_one": broken})
        with patch.object(asset_overlay, "android_tools", return_value=tools), self.assertRaises(ValueError):
            asset_overlay.build(self.client, self.manifest, self.root / "out", self.key, self.shader_map,
                                jobs=1, log=lambda _: None)


if __name__ == "__main__":
    unittest.main()
