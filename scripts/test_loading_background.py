"""Run the real loading image lifecycle in an isolated graphical Unity client.

Build --image-recovery-only --stage-only first. Installed client files are read
only; the probe uses copied staged DLLs and exits without logging into an account.
"""
import json
import os
from pathlib import Path
import shutil
import subprocess
import time

REPO = Path(__file__).resolve().parents[1]


def main():
    client = REPO.parent / "sprk-client"
    run = REPO / "target/loading-background-probe" / time.strftime("%Y%m%d-%H%M%S")
    run.mkdir(parents=True)
    data = run / "SprkLoadingProbe_Data"
    data.mkdir()
    source = client / "King's Raid_Data"
    for path in source.iterdir():
        target = data / path.name
        if path.name == "Managed":
            target.mkdir()
            for dll in path.glob("*.dll"):
                shutil.copy2(dll, target / dll.name)
            for name in ("Assembly-CSharp.dll", "SprkImageRecovery.dll"):
                staged = path / (name + (".image-recovery-staged" if name == "Assembly-CSharp.dll" else ".staged"))
                shutil.copy2(staged, target / name)
        elif path.is_dir():
            quote = lambda p: "'" + str(p).replace("'", "''") + "'"
            subprocess.run(["powershell", "-NoProfile", "-Command",
                            "New-Item -ItemType Junction -Path " + quote(target) + " -Target " + quote(path) + " | Out-Null"],
                           check=True, creationflags=subprocess.CREATE_NO_WINDOW)
        elif path.suffix not in (".log", ".backup"):
            os.link(path, target)
    exe = run / "SprkLoadingProbe.exe"
    shutil.copy2(client / "King's Raid.exe", exe)
    for dll in client.glob("*.dll"):
        shutil.copy2(dll, run / dll.name)
    runtime = client / "MonoBleedingEdge"
    quote = lambda p: "'" + str(p).replace("'", "''") + "'"
    subprocess.run(["powershell", "-NoProfile", "-Command",
                    "New-Item -ItemType Junction -Path " + quote(run / runtime.name) + " -Target " + quote(runtime) + " | Out-Null"],
                   check=True, creationflags=subprocess.CREATE_NO_WINDOW)
    report = run / "report.json"
    env = os.environ.copy()
    env.pop("SPRK_TP_ICON_PROBE", None)
    env["SPRK_LOADING_BACKGROUND_PROBE"] = str(report)
    env["SPRK_AUTOMATION_DIR"] = str(run)
    env["SPRK_AUTOMATION_PROFILE"] = "loading-background-probe"
    subprocess.run(["dotnet", str(REPO / "scripts/DllPatcher/bin/Debug/net8.0/DllPatcher.dll"),
                    str(data / "Managed"), "--prepare-loading-background-probe"], cwd=REPO, check=True)
    startup = subprocess.STARTUPINFO()
    startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
    startup.wShowWindow = 0
    process = subprocess.Popen([str(exe), "-screen-fullscreen", "0",
                                "-screen-width", "1280", "-screen-height", "720",
                                "-logFile", str(run / "client.log")],
                               cwd=run, env=env, startupinfo=startup, creationflags=subprocess.CREATE_NO_WINDOW)
    try:
        deadline = time.monotonic() + 90
        while not report.exists() and process.poll() is None and time.monotonic() < deadline:
            time.sleep(0.5)
        if not report.exists():
            raise RuntimeError(f"Probe did not finish; inspect {run / 'client.log'}")
        result = json.loads(report.read_text())
        print(json.dumps({"report": str(report), **result}, indent=2))
        if not result["success"]:
            raise RuntimeError("Loading background regression probe failed")
    finally:
        if process.poll() is None:
            process.terminate()
        process.wait(timeout=15)


if __name__ == "__main__":
    main()
