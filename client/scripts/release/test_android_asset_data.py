import importlib
import json
from pathlib import Path
import sys
import tempfile
import types
import unittest

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

CONTENT = Path(__file__).resolve().parents[2] / "android/HybridBootstrap/content"


def load_builder():
    """build_android_assets without UnityPy (the converter is not used here)."""
    sys.path.insert(0, str(CONTENT))
    try:
        importlib.import_module("convert_bundles")
    except ImportError:
        sys.modules["convert_bundles"] = types.SimpleNamespace(VERSION="test")
    return importlib.import_module("build_android_assets")


class AndroidAssetDataTest(unittest.TestCase):
    """The Android asset set carries the Windows set's platform-neutral patch data:
    without it phones had no cinema node trees (black Hall of Heroes / Dressing Room)."""

    def setUp(self):
        self.builder = load_builder()
        temporary = tempfile.TemporaryDirectory(prefix="sprk-android-data-")
        self.addCleanup(temporary.cleanup)
        root = Path(temporary.name)
        self.windows, self.out = root / "windows", root / "out"
        self.write({
            "PlainAsset/Text/NodeTree/Common/Cinema.bjs": b"cinema",
            "PlainAsset/Text/CutScene/Intro.bjs": b"cutscene",
            "LoginPresentation/a/b.png": b"login",
            "ContentsDefine/Define.json": b"define",
            "AiScript/DefaultAi.xml": b"ai",
            # Installed by code releases or owned by the game's downloader:
            "TableJit/Hero.jit": b"table",
            "TableData/t.bytes": b"table",
            "LocalizationJit/en.jit": b"loc",
            "Video/intro.mp4": b"video",
            "PatchInfo/info": b"x",
            "patch.json": b"{}",
            "SPRK-NativeData.json": b"{}",
            # Bundles are converted separately; tool leftovers never ship.
            "Assetbundle/A.unity3d": b"bundle",
            "PlainAsset/Text/old.bjs.bak": b"backup",
        })

    def write(self, files):
        for name, data in files.items():
            path = self.windows / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)

    def placed(self):
        return sorted(p.relative_to(self.out).as_posix() for p in self.out.rglob("*") if p.is_file())

    def test_mirrors_the_neutral_data_the_windows_set_publishes(self):
        data = {}
        self.builder.mirror_data(self.windows, self.out, data)
        expected = ["AiScript/DefaultAi.xml", "ContentsDefine/Define.json", "LoginPresentation/a/b.png",
                    "PlainAsset/Text/CutScene/Intro.bjs", "PlainAsset/Text/NodeTree/Common/Cinema.bjs"]
        self.assertEqual(self.placed(), expected)
        self.assertEqual(sorted(data), expected)
        self.assertEqual((self.out / "PlainAsset/Text/NodeTree/Common/Cinema.bjs").read_bytes(), b"cinema")
        json.dumps(data)  # kept in .sprk-android.json

    def test_updates_changed_files_and_removes_deleted_ones(self):
        data = {}
        self.builder.mirror_data(self.windows, self.out, data)
        cinema = self.windows / "PlainAsset/Text/NodeTree/Common/Cinema.bjs"
        cinema.unlink()  # a new file, as an edit by another tool would leave
        cinema.write_bytes(b"cinema v2")
        (self.windows / "AiScript/DefaultAi.xml").unlink()
        self.builder.mirror_data(self.windows, self.out, data)
        self.assertEqual((self.out / "PlainAsset/Text/NodeTree/Common/Cinema.bjs").read_bytes(), b"cinema v2")
        self.assertFalse((self.out / "AiScript/DefaultAi.xml").exists())
        self.assertNotIn("AiScript/DefaultAi.xml", data)


if __name__ == "__main__":
    unittest.main()
