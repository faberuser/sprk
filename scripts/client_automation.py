"""Opt-in Windows/Unity client testing. Python standard library only.

Commands and observations are retained under target/client-automation/<run>.
The bridge does not bypass unlocks or synthesize battle results.
"""
import argparse
import base64
import ctypes
from contextlib import closing
from ctypes import wintypes
import datetime
import json
import os
import re
import shutil
from pathlib import Path
import socket
import sqlite3
import subprocess
import sys
import time
import urllib.request
import uuid

REPO = Path(__file__).resolve().parents[1]
RUNS = REPO / "target/client-automation"


def save(path, value):
    path = Path(path)
    temp = path.with_suffix(path.suffix + ".tmp")
    temp.write_text(json.dumps(value, indent=2, ensure_ascii=False), encoding="utf-8")
    temp.replace(path)


def read(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def client_view(client, profile):
    """A distinct executable name avoids Unity's native single-instance mutex.

    Reuse assets via directory junctions; never copy the 18 GB asset archive.
    """
    view = RUNS / "clients" / profile
    view.mkdir(parents=True, exist_ok=True)
    name = "SprkTest-" + profile
    shutil.copy2(client / "King's Raid.exe", view / (name + ".exe"))
    for source in client.iterdir():
        if source.is_dir():
            target = view / ((name + "_Data") if source.name == "King's Raid_Data" else source.name)
            if target.exists():
                if target.resolve() != source.resolve():
                    raise RuntimeError(f"Existing client junction points elsewhere: {target}")
                continue
            quote = lambda p: "'" + str(p).replace("'", "''") + "'"
            command = "$ErrorActionPreference='Stop'; New-Item -ItemType Junction -Path " + quote(target) + " -Target " + quote(source) + " | Out-Null"
            subprocess.run(["powershell", "-NoProfile", "-EncodedCommand",
                            base64.b64encode(command.encode("utf-16le")).decode()],
                           check=True, creationflags=subprocess.CREATE_NO_WINDOW)
        elif source.suffix.lower() == ".dll" or source.name.startswith("UnityCrashHandler"):
            target = view / source.name
            if not target.exists():
                try:
                    os.link(source, target)
                except OSError:
                    shutil.copy2(source, target)
    return view / (name + ".exe")


def database_query(path, query, params=()):
    # Read-only connection and query_only also prohibit mutations through PRAGMAs.
    with closing(sqlite3.connect(Path(path).resolve().as_uri() + "?mode=ro", uri=True)) as db:
        db.execute("PRAGMA query_only=ON")
        return [list(row) for row in db.execute(query, params).fetchall()]


def server_identity(pid, terminate=False, expected=None):
    """Use one process handle for identity checking and optional termination."""
    kernel = ctypes.WinDLL("kernel32", use_last_error=True)
    kernel.OpenProcess.argtypes = [wintypes.DWORD, wintypes.BOOL, wintypes.DWORD]
    kernel.OpenProcess.restype = wintypes.HANDLE
    kernel.CloseHandle.argtypes = [wintypes.HANDLE]
    kernel.GetProcessTimes.argtypes = [wintypes.HANDLE] + [ctypes.POINTER(wintypes.FILETIME)] * 4
    kernel.QueryFullProcessImageNameW.argtypes = [wintypes.HANDLE, wintypes.DWORD, wintypes.LPWSTR, ctypes.POINTER(wintypes.DWORD)]
    kernel.TerminateProcess.argtypes = [wintypes.HANDLE, wintypes.UINT]
    handle = kernel.OpenProcess(0x1000 | (1 if terminate else 0), False, pid)
    if not handle:
        raise OSError("Recorded server process is no longer accessible")
    try:
        times = [wintypes.FILETIME() for _ in range(4)]
        path = ctypes.create_unicode_buffer(32768)
        size = wintypes.DWORD(len(path))
        if not kernel.GetProcessTimes(handle, *(ctypes.byref(t) for t in times)) or not kernel.QueryFullProcessImageNameW(handle, 0, path, ctypes.byref(size)):
            raise ctypes.WinError(ctypes.get_last_error())
        identity = {"created": (times[0].dwHighDateTime << 32) | times[0].dwLowDateTime, "path": path.value}
        if terminate:
            if expected != identity:
                raise RuntimeError("PID now belongs to a different process; refusing to stop it")
            if not kernel.TerminateProcess(handle, 0):
                raise ctypes.WinError(ctypes.get_last_error())
        return identity
    finally:
        kernel.CloseHandle(handle)


def session(value):
    return Path(value).resolve() if value else Path(read(RUNS / "latest.json")["session"])


def request(root, action, timeout=20, **kwargs):
    root = Path(root)
    command = {"id": uuid.uuid4().hex, "action": action, **kwargs}
    # Exactly one writer and one outstanding command per client.
    lock = root / "controller.lock"
    fd = os.open(lock, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
    try:
        if (root / "command.json").exists():
            raise RuntimeError("Previous command is still pending; inspect the client/log before retrying")
        save(root / (command["id"] + ".request.json"), command)
        save(root / "command.json", command)
        result = root / (command["id"] + ".json")
        deadline = time.monotonic() + timeout
        while not result.exists():
            if time.monotonic() > deadline:
                raise TimeoutError(f"Client did not answer {action}; see {root / 'client.log'}")
            time.sleep(0.1)
        reply = read(result)
        if not reply["ok"]:
            raise RuntimeError(reply.get("error", "Client command failed"))
        return reply
    finally:
        os.close(fd)
        lock.unlink()


def select(reply, selector, kind="button"):
    found = [e for e in reply["elements"] if e["kind"] == kind and e["enabled"]
             and (str(e["id"]) == selector or e["path"] == selector or e["text"] == selector)]
    if len(found) != 1:
        raise RuntimeError(f"Expected one enabled {kind} for {selector!r}; found {len(found)}. Use its full path or ID.")
    return found[0]


def native_input(pid, action, x=0, y=0, key="ESC"):
    """Real input; refuses to send keys/clicks unless the selected client owns focus."""
    user = ctypes.WinDLL("user32", use_last_error=True)
    user.SetProcessDPIAware()
    user.GetForegroundWindow.restype = wintypes.HWND
    user.SetForegroundWindow.argtypes = [wintypes.HWND]
    user.ShowWindow.argtypes = [wintypes.HWND, ctypes.c_int]
    user.GetWindowThreadProcessId.argtypes = [wintypes.HWND, ctypes.POINTER(wintypes.DWORD)]
    user.IsWindowVisible.argtypes = [wintypes.HWND]
    user.GetClientRect.argtypes = [wintypes.HWND, ctypes.POINTER(wintypes.RECT)]
    user.ClientToScreen.argtypes = [wintypes.HWND, ctypes.POINTER(wintypes.POINT)]
    callback_type = ctypes.WINFUNCTYPE(wintypes.BOOL, wintypes.HWND, wintypes.LPARAM)
    found = []
    @callback_type
    def visit(hwnd, _):
        owner = wintypes.DWORD()
        user.GetWindowThreadProcessId(hwnd, ctypes.byref(owner))
        if owner.value == pid and user.IsWindowVisible(hwnd):
            found.append(hwnd)
        return True
    user.EnumWindows(visit, 0)
    if len(found) != 1:
        raise RuntimeError(f"Expected one visible client window, found {len(found)}")
    hwnd = found[0]
    user.ShowWindow(hwnd, 9)
    user.SetForegroundWindow(hwnd)
    if user.GetForegroundWindow() != hwnd:
        # Windows may deny a background console focus. Temporarily share the
        # foreground input queue; still verify focus before sending any input.
        foreground = user.GetForegroundWindow()
        foreground_thread = user.GetWindowThreadProcessId(foreground, None)
        current_thread = ctypes.windll.kernel32.GetCurrentThreadId()
        attached = user.AttachThreadInput(current_thread, foreground_thread, True)
        try:
            user.SetForegroundWindow(hwnd)
        finally:
            if attached:
                user.AttachThreadInput(current_thread, foreground_thread, False)
    time.sleep(0.2)
    if user.GetForegroundWindow() != hwnd:
        raise RuntimeError("Windows refused game focus; no input sent")
    if action == "click":
        rect = wintypes.RECT()
        user.GetClientRect(hwnd, ctypes.byref(rect))
        if not (0 <= x < rect.right and 0 <= y < rect.bottom):
            raise ValueError("Click is outside the client area")
        point = wintypes.POINT(int(x), int(y))
        user.ClientToScreen(hwnd, ctypes.byref(point))
        user.SetCursorPos(point.x, point.y)
        user.mouse_event(2, 0, 0, 0, 0)
        time.sleep(0.05)
        user.mouse_event(4, 0, 0, 0, 0)
    else:
        keys = {"ESC": 27, "ENTER": 13, "SPACE": 32, "TAB": 9}
        vk = keys[key]
        user.keybd_event(vk, 0, 0, 0)
        time.sleep(0.05)
        user.keybd_event(vk, 0, 2, 0)


def launch(args):
    client = Path(args.client).resolve()
    if not (client / "King's Raid_Data/Managed/SprkAutomation.dll").exists():
        raise RuntimeError("Install --automation-only with DllPatcher first")
    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S") + "-" + uuid.uuid4().hex[:6]
    root = RUNS / stamp
    root.mkdir(parents=True)
    if args.profile and not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", args.profile):
        raise ValueError("Profile must contain 1-64 letters, digits, hyphens or underscores")
    meta = {"session": str(root), "client": str(client), "server_pid": None, "profile": args.profile}
    meta["database"] = str(Path(args.database).resolve()) if args.external_server and args.database else (None if args.external_server else str(root / "sprk.db"))
    save(root / "session.json", meta)
    save(RUNS / "latest.json", meta)
    if not args.external_server:
        for port in (8080, 9001):
            with socket.socket() as probe:
                if probe.connect_ex(("127.0.0.1", port)) == 0:
                    raise RuntimeError(f"Port {port} is already in use; choose --external-server explicitly to reuse a server")
        if args.seed_db:
            with closing(sqlite3.connect(Path(args.seed_db).resolve().as_uri() + "?mode=ro", uri=True)) as source:
                with closing(sqlite3.connect(root / "sprk.db")) as dest:
                    source.backup(dest)
        env = dict(os.environ, GAME_TABLES_PATH=str(REPO / "tables"), PORT="8080", CHAT_PORT="9001", CHAT_BIND="127.0.0.1", CHAT_ADDRESS="127.0.0.1", RUST_LOG="info")
        with (root / "server.log").open("wb") as log:
            proc = subprocess.Popen([str(Path(args.server).resolve())], cwd=root,
                                    env=env, stdout=log, stderr=subprocess.STDOUT,
                                    creationflags=subprocess.CREATE_NO_WINDOW)
        meta["server_pid"] = proc.pid
        meta["server_identity"] = server_identity(proc.pid)
        save(root / "session.json", meta)
    deadline = time.monotonic() + 60
    while True:
        try:
            with urllib.request.urlopen("http://127.0.0.1:8080/health", timeout=2) as response:
                if response.status == 200:
                    break
        except OSError:
            pass
        if time.monotonic() > deadline:
            raise TimeoutError(f"Server startup failed; see {root / 'server.log'}")
        time.sleep(0.5)
    env = dict(os.environ, SPRK_AUTOMATION_DIR=str(root))
    env.pop("SPRK_AUTOMATION_PROFILE", None)
    if args.profile:
        env["SPRK_AUTOMATION_PROFILE"] = args.profile
    executable = client_view(client, args.profile) if args.profile else client / "King's Raid.exe"
    proc = subprocess.Popen([str(executable), "-screen-fullscreen", "0",
                             "-screen-width", "1280", "-screen-height", "720",
                             "-logFile", str(root / "client.log")], cwd=executable.parent, env=env)
    meta["client_pid"] = proc.pid
    meta["executable"] = str(executable)
    save(root / "session.json", meta)
    print(json.dumps(meta), flush=True)
    deadline = time.monotonic() + 90
    while not (root / "ready.json").exists():
        if proc.poll() is not None:
            raise RuntimeError(f"Client exited ({proc.returncode}); see client.log")
        if time.monotonic() > deadline:
            raise TimeoutError("Client bridge did not start; inspect client.log")
        time.sleep(0.5)
    print(json.dumps({"ready": True, "session": str(root)}))


def run_scenario(root, path):
    scenario = read(path)
    report = {"scenario": str(Path(path).resolve()), "passed": False, "steps": []}
    try:
        for step in scenario["steps"]:
            action = step["action"]
            entry = {"step": step, "passed": False}
            report["steps"].append(entry)
            if action == "wait":
                time.sleep(min(float(step["seconds"]), 60))
            elif action == "wait-ui":
                deadline = time.monotonic() + step.get("timeout", 30)
                while True:
                    reply = request(root, "snapshot")
                    matches = [e for e in reply["elements"] if e["enabled"] and
                               step["contains"] in (e["text"] + " " + e["path"])]
                    if matches:
                        break
                    if time.monotonic() > deadline:
                        raise AssertionError(f"UI not found: {step['contains']}")
                    time.sleep(0.5)
                entry["observation"] = reply["id"]
            elif action in ("click", "press", "native-click", "text"):
                reply = request(root, "snapshot")
                element = select(reply, step["selector"], "input" if action == "text" else "button")
                if action == "native-click":
                    native_input(reply["pid"], "click", element["x"], element["y"])
                else:
                    request(root, action, target=element["id"], text=step.get("text", ""))
            elif action == "key":
                native_input(read(root / "ready.json")["pid"], "key", key=step["key"])
            elif action == "assert-no-errors":
                reply = request(root, "snapshot")
                if reply["errors"]:
                    raise AssertionError(json.dumps(reply["errors"], ensure_ascii=False))
            elif action == "assert-ui-count":
                reply = request(root, "snapshot")
                matches = [e for e in reply["elements"] if e["kind"] == step.get("kind", "button")
                           and any(part in e["path"] for part in step["path_contains_any"])]
                entry["observation"] = reply["id"]
                entry["actual"] = len(matches)
                if len(matches) != step["expected"]:
                    raise AssertionError(f"Expected {step['expected']} controls, found {len(matches)}")
            elif action == "assert-db":
                meta = read(root / "session.json")
                db_path = meta.get("database") or str(root / "sprk.db")
                actual = database_query(db_path, step["query"], step.get("params", []))
                entry["actual"] = actual
                if actual != step["expected"]:
                    raise AssertionError(f"Database assertion: expected {step['expected']!r}, got {actual!r}")
            elif action in ("snapshot", "screenshot", "clear-errors"):
                reply = request(root, action)
                entry["observation"] = reply["id"]
            else:
                raise ValueError(f"Unknown scenario action: {action}")
            entry["passed"] = True
        report["passed"] = True
    except Exception as error:
        report["error"] = str(error)
        try:
            report["failure_screenshot"] = request(root, "screenshot")["screenshot"]
        except Exception as capture_error:
            report["capture_error"] = str(capture_error)
    report_path = root / ("report-" + uuid.uuid4().hex + ".json")
    save(report_path, report)
    print(json.dumps({"passed": report["passed"], "report": str(report_path)}))
    return 0 if report["passed"] else 1


def main():
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--session", help="Run directory; default: most recently launched run")
    sub = parser.add_subparsers(dest="action", required=True)
    start = sub.add_parser("launch")
    start.add_argument("--client", default=str(REPO.parent / "sprk-client"))
    start.add_argument("--server", default=str(REPO / "target/release/sprk-server.exe"), help="Server build to launch (debug builds are useful for test/fix iterations)")
    start.add_argument("--seed-db", help="Copy this SQLite database into the isolated test server")
    start.add_argument("--external-server", action="store_true", help="Explicitly use an already-running server")
    start.add_argument("--profile", help="Separate test-account preferences; use different names for two clients")
    start.add_argument("--database", help="Read-only assertion database path for --external-server")
    for action in ("snapshot", "screenshot", "clear-errors", "quit", "renderers"):
        sub.add_parser(action)
    for action in ("click", "press", "native-click", "text"):
        p = sub.add_parser(action)
        p.add_argument("selector")
        if action == "text":
            p.add_argument("text")
    p = sub.add_parser("key")
    p.add_argument("key", choices=["ESC", "ENTER", "SPACE", "TAB"])
    p = sub.add_parser("click-at", help="Real mouse click using game-client pixel coordinates")
    p.add_argument("x", type=int)
    p.add_argument("y", type=int)
    p = sub.add_parser("run")
    p.add_argument("scenario")
    sub.add_parser("stop-server", help="Stop only the server created by this run")
    args = parser.parse_args()
    if args.action == "launch":
        launch(args)
        return 0
    root = session(args.session)
    if args.action == "stop-server":
        meta = read(root / "session.json")
        if not meta.get("server_pid") or not meta.get("server_identity"):
            raise RuntimeError("This run has no owned server with a recorded identity")
        server_identity(meta["server_pid"], terminate=True, expected=meta["server_identity"])
        print(json.dumps({"stopped_server": meta["server_pid"]}))
        return 0
    if args.action == "run":
        return run_scenario(root, args.scenario)
    if args.action == "click-at":
        native_input(read(root / "ready.json")["pid"], "click", args.x, args.y)
        result = {"clicked": [args.x, args.y]}
    elif args.action == "key":
        native_input(read(root / "ready.json")["pid"], "key", key=args.key)
        result = {"sent": args.key}
    elif args.action in ("click", "press", "native-click", "text"):
        reply = request(root, "snapshot")
        element = select(reply, args.selector, "input" if args.action == "text" else "button")
        if args.action == "native-click":
            native_input(reply["pid"], "click", element["x"], element["y"])
            result = {"clicked": element}
        else:
            result = request(root, args.action, target=element["id"], text=getattr(args, "text", ""))
    else:
        result = request(root, args.action)
    if args.action == "screenshot":
        print(str(root / result["screenshot"]))
    else:
        print(json.dumps(result, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main())
    except Exception as error:
        print(f"ERROR: {error}", file=sys.stderr)
        sys.exit(1)
