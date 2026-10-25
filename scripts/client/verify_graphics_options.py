"""Run native Unity graphics fixtures against a fresh, isolated client copy.

No login/server is required. Managed DLLs and PlayerSettings are private copies;
large native assets are shared for read-only loading. Preferences use a unique
automation profile. The staged mode never promotes installed client files.
"""
import argparse
import base64
import hashlib
import os
from pathlib import Path
import shutil
import subprocess
import uuid

REPO = Path(__file__).resolve().parents[2]
DATA = Path("King's Raid_Data")


def junction(source, target):
    quote = lambda p: "'" + str(p).replace("'", "''") + "'"
    command = "$ErrorActionPreference='Stop'; New-Item -ItemType Junction -Path " + quote(target) + " -Target " + quote(source) + " | Out-Null"
    subprocess.run(["powershell", "-NoProfile", "-EncodedCommand", base64.b64encode(command.encode("utf-16le")).decode()], check=True, creationflags=subprocess.CREATE_NO_WINDOW)


def prepare(client, root, installed=False):
    root.mkdir(parents=True)
    name = "SprkTest-graphics-" + root.name
    shutil.copy2(client / "King's Raid.exe", root / (name + ".exe"))
    for source in client.iterdir():
        if source.name == DATA.name:
            continue
        if source.is_dir() and source.name in ("MonoBleedingEdge", "D3D12"):
            junction(source, root / source.name)
        elif source.suffix.lower() == ".dll":
            os.link(source, root / source.name)
    data = root / DATA
    data.mkdir()
    for source in (client / DATA).iterdir():
        target = data / source.name
        if source.name == "Managed":
            target.mkdir()
            for dll in source.glob("*.dll"):
                shutil.copy2(dll, target / dll.name)
        elif source.name == "Documents":
            target.mkdir()
            junction(source / "Patch", target / "Patch")
        elif source.is_dir():
            junction(source, target)
        elif "." not in source.name or source.suffix in (".config", ".assets", ".resS", ".resource", ".json", ".info"):
            # Copy the settings asset; other files are immutable runtime inputs.
            if source.name == "globalgamemanagers":
                shutil.copy2(source, target)
            else:
                os.link(source, target)
    if not installed:
        for dll_name, staged in (("Assembly-CSharp.dll", "Assembly-CSharp.dll.graphics-options-staged"), ("SprkGraphicsOptions.dll", "SprkGraphicsOptions.dll.staged"), ("SprkAutomation.dll", "SprkAutomation.dll.staged")):
            shutil.copy2(client / DATA / "Managed" / staged, data / "Managed" / dll_name)
        shutil.copy2(client / DATA / "globalgamemanagers.graphics-options-staged", data / "globalgamemanagers")
    junction(data, root / (name + "_Data"))


def launch(root, profile, restart=False, modes=None, tag=None):
    tag = tag or ("restart" if restart else "probe")
    output = root / "probe"
    output.mkdir(exist_ok=True)
    env = dict(os.environ, SPRK_GRAPHICS_TEST=str(output), SPRK_AUTOMATION_DIR=str(root / "automation"), SPRK_AUTOMATION_PROFILE=profile)
    for key in list(env):
        if key.startswith("SPRK_GRAPHICS_") and key != "SPRK_GRAPHICS_TEST":
            env.pop(key)
    if restart:
        env["SPRK_GRAPHICS_RESTART_TEST"] = "1"
    env.update(modes or {})
    startup = subprocess.STARTUPINFO()
    startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
    startup.wShowWindow = 4  # Present frames without taking keyboard focus.
    executable = next(root.glob("SprkTest-graphics-*.exe"))
    proc = subprocess.Popen([str(executable), "-screen-fullscreen", "0", "-screen-width", "640", "-screen-height", "480", "-logFile", str(root / (tag + ".log"))], cwd=root, env=env, startupinfo=startup)
    try:
        code = proc.wait(timeout=150)
    except subprocess.TimeoutExpired:
        proc.terminate()
        proc.wait(timeout=15)
        raise RuntimeError(f"Graphics fixture timed out; inspect {root / (tag + '.log')}")
    if code:
        raise RuntimeError(f"Unity exited with {code}; inspect {root / (tag + '.log')}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--client", type=Path, default=REPO.parent / "sprk-client")
    parser.add_argument("--installed", action="store_true")
    parser.add_argument("--window-checks", action="store_true")
    args = parser.parse_args()
    if not args.installed:
        subprocess.run(["dotnet", "run", "--configuration", "Release", "--project", str(REPO / "scripts/DllPatcher"), "--", str(args.client), "--graphics-options-only", "--stage-only"], check=True, cwd=REPO)
    root = REPO / "target/graphics-performance-validation" / uuid.uuid4().hex[:12]
    prepare(args.client.resolve(), root, args.installed)
    print(root, flush=True)
    profile = "graphics-" + uuid.uuid4().hex[:12]
    launch(root, profile)
    report = (root / "probe/report.txt").read_text()
    print(report, flush=True)
    required = ("resources", "cameraSensitivityChoices", "performanceSettingChoices", "settingsAllChoices", "conditionalAARows", "rememberedAdjustments", "centeredGraphicsChoices", "compactConditionalLayout", "frameChoicesAnd144", "aaMethodMigration", "liveAdjustmentApplication", "livePreviewSwitching", "graphicsPreferenceCache", "nativePreviewVisibility", "particleDensityAndPooling", "independentPreviewScale", "temporalResourceCleanup", "adaptiveBattleController", "nativeFrameTimingEnabled", "crowdedBattleEffectBudget", "battleEffectCoverageAndRestore", "nativeShadowPosePreserved", "nativePoolSkipsDestroyedHead", "nativeWorldCameraClassification", "disabledTrailingCameraPresentation", "lobbyCameraStack", "battleCameraStack", "stableRenderTargets", "nativeFilterTargetsAndEffects")
    required += ("battleScaleTargetRouting", "nativeBattleGpuTimingsAvailable", "nativeShadowStableWrites", "adaptiveBattleTargetsAndCompatibility")
    required += ("graphicsHelpAndPooling",)
    required += ("renderScaleMigration", "renderScaleParentChildren", "renderScaleNativeHierarchy")
    for key in required:
        if key + "=True" not in report:
            raise AssertionError("Failed or missing graphics check: " + key)
    if "settingsRows=14" not in report or len([line for line in report.splitlines() if line.startswith("mode=")]) != 25:
        raise AssertionError("Missing settings or AA/scale combinations")
    for mode in (1, 2, 3, 4):
        for scale in (100, 125, 150, 175, 200):
            if mode == 3 and any(line.startswith(f"mode=3 scale={scale} ") and "samples=1 " in line for line in report.splitlines()):
                continue  # This device cannot create a multisampled target.
            image = root / f"probe/aa-{mode}-{scale}.png"
            off = root / f"probe/aa-0-{scale}.png"
            if hashlib.sha256(image.read_bytes()).digest() == hashlib.sha256(off.read_bytes()).digest():
                raise AssertionError(f"AA did not change rendered pixels: {mode}/{scale}")
    launch(root, profile, restart=True)
    assert "preferencesSurviveRestart=True" in (root / "probe/restart.txt").read_text()
    if args.window_checks:
        for tag, modes, artifact in (("custom", {"SPRK_GRAPHICS_CUSTOM_WINDOW_ONLY": "1"}, "custom.txt"), ("custom-restart", {"SPRK_GRAPHICS_CUSTOM_WINDOW_ONLY": "1", "SPRK_CUSTOM_WINDOW_RESTART": "1"}, "custom-restart.txt"), ("fullscreen", {"SPRK_GRAPHICS_FULLSCREEN_ONLY": "1"}, "fullscreen.txt"), ("resolution", {"SPRK_GRAPHICS_RESOLUTION_ONLY": "1"}, "resolution.txt")):
            launch(root, profile, modes=modes, tag=tag)
            assert (root / "probe" / artifact).read_text().startswith("PASS:")
    (REPO / "target/battle-graphics").mkdir(exist_ok=True)
    (REPO / "target/battle-graphics/latest-validation.txt").write_text(str(root))
    print("PASS: native graphics, battle controls, AA images and preference persistence", flush=True)


if __name__ == "__main__":
    main()
