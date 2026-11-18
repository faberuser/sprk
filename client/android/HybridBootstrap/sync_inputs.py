"""Stage the inputs of the HybridCLR Android bootstrap.

* AOT plugins: the game's third-party managed DLLs (from the installed Windows
  client's Managed folder) go to Assets/Plugins/GameRuntime so IL2CPP compiles
  them into the player.
* Hot-update assemblies: Assembly-CSharp and every Sprk*.dll from a
  `client/dev.py build` output go to HotUpdate/ (HybridCLR's external hot-update
  directory) and into StreamingAssets/sprk/hot as .bytes for the APK.

usage: python client/android/HybridBootstrap/sync_inputs.py [--client ../sprk-client] [--code artifacts/client-build/client-code]
"""
import argparse
import shutil
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[2]
MANAGED = Path("King's Raid_Data") / "Managed"

# Third-party / package assemblies the game ships besides the Unity engine and
# the class libraries IL2CPP already provides (ScriptingAssemblies.json, type 16).
AOT_PLUGINS = [
    "MessagePack.Annotations", "MessagePack", "spine-unity", "SuperSocket",
    "UniTask", "UniTask.Addressables", "UniTask.DOTween", "UniTask.Linq", "UniTask.TextMeshPro",
    "Unity.AI.Navigation", "Unity.Multiplayer.Center.Common", "Unity.Postprocessing.Runtime",
    "Unity.TextMeshPro", "Unity.Timeline", "UnityEngine.SpatialTracking", "UnityEngine.UI",
    "UnityEngine.XR.LegacyInputHelpers", "UniWebView-CSharp", "Vuplex.WebView",
    "Microsoft.NET.StringTools", "Newtonsoft.Json", "ICSharpCode.SharpZipLib",
    "Unity.Compat", "Unity.Tasks", "System.Runtime.CompilerServices.Unsafe",
]

PLUGIN_META = """fileFormatVersion: 2
guid: {guid}
PluginImporter:
  externalObjects: {{}}
  serializedVersion: 2
  iconMap: {{}}
  executionOrder: {{}}
  defineConstraints: []
  isPreloaded: 0
  isOverridable: 0
  isExplicitlyReferenced: 0
  validateReferences: 0
  platformData:
  - first:
      Any:
    second:
      enabled: 1
      settings: {{}}
  - first:
      Editor: Editor
    second:
      enabled: 1
      settings:
        DefaultValueInitialized: true
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def guid_for(name: str) -> str:
    import hashlib
    return hashlib.md5(f"sprk-android-plugin:{name}".encode()).hexdigest()


def sync(src: Path, dst: Path, names, suffix=".dll", meta=False):
    dst.mkdir(parents=True, exist_ok=True)
    for name in names:
        source = src / f"{name}.dll"
        target = dst / f"{name}{suffix}"
        if not source.is_file():
            raise SystemExit(f"missing input: {source}")
        if not target.exists() or target.read_bytes() != source.read_bytes():
            shutil.copyfile(source, target)
        if meta:
            m = dst / f"{name}{suffix}.meta"
            text = PLUGIN_META.format(guid=guid_for(name))
            if not m.exists() or m.read_text(encoding="utf-8") != text:
                m.write_text(text, encoding="utf-8")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--client", type=Path, default=REPO.parent / "sprk-client")
    ap.add_argument("--code", type=Path, default=REPO / "artifacts/client-build/client-code")
    a = ap.parse_args()
    runtime = a.client / MANAGED
    code = a.code / MANAGED
    if not (runtime / "UnityEngine.dll").is_file():
        raise SystemExit(f"not a client Managed folder: {runtime}")
    hot = sorted(p.stem for p in code.glob("*.dll"))
    if "Assembly-CSharp" not in hot:
        raise SystemExit(f"no compiled client code in {code}; run python client/dev.py build")

    sync(runtime, HERE / "Assets/Plugins/GameRuntime", AOT_PLUGINS, meta=True)
    # HybridCLR's generators read the hot-update DLLs from here.
    sync(code, HERE / "HotUpdate", hot)
    sync(code, HERE / "Assets/StreamingAssets/sprk/hot", hot, suffix=".dll.bytes")
    (HERE / "HotUpdate/hot-update.txt").write_text("\n".join(hot) + "\n", encoding="utf-8")
    print(f"staged {len(AOT_PLUGINS)} AOT plugins, {len(hot)} hot-update assemblies")


if __name__ == "__main__":
    main()
