"""Stage the inputs of the HybridCLR Android bootstrap.

* AOT plugins: the game's third-party managed DLLs (from the installed Windows
  client's Managed folder) go to Assets/Plugins/GameRuntime so IL2CPP compiles
  them into the player.
* Game assemblies: Assembly-CSharp and every Sprk*.dll from a `client/dev.py build`
  output. They always go to HotUpdate/ with their list (hot-update.txt) and SHA-256
  hashes (game-code.txt; the APK compares them with a server's release and asks the
  player to download a new APK when they differ). Then, by --mode:
  - aot (default): to Assets/Plugins/GameCode (Android only), so IL2CPP compiles them
    into the player like the original game. Every code change needs a new APK.
  - interpreted: to StreamingAssets/sprk/hot as .bytes for HybridCLR's interpreter
    (slower; `dev.py hot` and code updates from the server work).

usage: python client/android/HybridBootstrap/sync_inputs.py [--mode aot|interpreted] [--client ../sprk-client] [--code artifacts/client-build/client-code]
"""
import argparse
import hashlib
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


# Game assemblies compiled into the player: Android only. The editor never loads them
# (the bootstrap reaches the game through reflection), so the game's static code does
# not run in the editor and the editor's own Assembly-CSharp is not shadowed.
GAME_CODE_META = """fileFormatVersion: 2
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
      enabled: 0
      settings: {{}}
  - first:
      Android: Android
    second:
      enabled: 1
      settings: {{}}
  - first:
      Editor: Editor
    second:
      enabled: 0
      settings:
        DefaultValueInitialized: true
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def guid_for(name: str) -> str:
    import hashlib
    return hashlib.md5(f"sprk-android-plugin:{name}".encode()).hexdigest()


def sync(src: Path, dst: Path, names, suffix=".dll", meta=None):
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
            text = meta.format(guid=guid_for(name))
            if not m.exists() or m.read_text(encoding="utf-8") != text:
                m.write_text(text, encoding="utf-8")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--client", type=Path, default=REPO.parent / "sprk-client")
    ap.add_argument("--code", type=Path, default=REPO / "artifacts/client-build/client-code")
    ap.add_argument("--mode", choices=["aot", "interpreted"], default="aot")
    a = ap.parse_args()
    runtime = a.client / MANAGED
    code = a.code / MANAGED
    if not (runtime / "UnityEngine.dll").is_file():
        raise SystemExit(f"not a client Managed folder: {runtime}")
    hot = sorted(p.stem for p in code.glob("*.dll"))
    if "Assembly-CSharp" not in hot:
        raise SystemExit(f"no compiled client code in {code}; run python client/dev.py build")

    sync(runtime, HERE / "Assets/Plugins/GameRuntime", AOT_PLUGINS, meta=PLUGIN_META)
    # HybridCLR's generators read the hot-update DLLs from here.
    sync(code, HERE / "HotUpdate", hot)
    (HERE / "HotUpdate/hot-update.txt").write_text("\n".join(hot) + "\n", encoding="utf-8")
    hashes = [f"{n} {hashlib.sha256((code / f'{n}.dll').read_bytes()).hexdigest()}" for n in hot]
    (HERE / "HotUpdate/game-code.txt").write_text("\n".join(hashes) + "\n", encoding="utf-8")
    (HERE / "HotUpdate/mode.txt").write_text(a.mode + "\n", encoding="utf-8")
    game_code = HERE / "Assets/Plugins/GameCode"
    hot_bytes = HERE / "Assets/StreamingAssets/sprk/hot"
    # Only one copy of the game goes into the APK: compiled in, or for the interpreter.
    stale = hot_bytes if a.mode == "aot" else game_code
    if stale.exists():
        shutil.rmtree(stale)
        Path(str(stale) + ".meta").unlink(missing_ok=True)
    if a.mode == "aot":
        sync(code, game_code, hot, meta=GAME_CODE_META)
        for extra in game_code.glob("*.dll"):
            if extra.stem not in hot:
                extra.unlink()
                Path(str(extra) + ".meta").unlink(missing_ok=True)
    else:
        sync(code, hot_bytes, hot, suffix=".dll.bytes")
    # App icon: the same artwork as the Windows launcher (largest frame of its .ico).
    from PIL import Image
    icon_dir = HERE / "Assets/SprkBootstrap/Generated"
    icon_dir.mkdir(parents=True, exist_ok=True)
    ico = Image.open(REPO / "client/launcher/Assets/KingsRaid.ico")
    ico.size = max(ico.info["sizes"])
    ico.convert("RGBA").save(icon_dir / "AppIcon.png")
    print(f"staged {len(AOT_PLUGINS)} AOT plugins, {len(hot)} game assemblies ({a.mode}), app icon {ico.size[0]}px")


if __name__ == "__main__":
    main()
