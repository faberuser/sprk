"""Windows integration check: children release their ports when the host exits."""
import ctypes
import json
import os
from pathlib import Path
import socket
import subprocess
import sys
import tempfile
import time
import unittest


@unittest.skipUnless(os.name == "nt", "Windows process lifetime helper")
class LocalServerLifetimeTests(unittest.TestCase):
    def check_exit(self, forced):
        helper = Path(__file__).resolve().parents[1] / "scripts/LocalServerLifetime.cs"
        quote = lambda text: "'" + str(text).replace("'", "''") + "'"
        with tempfile.TemporaryDirectory(prefix="sprk-lifetime-") as directory:
            root = Path(directory)
            worker = root / "worker.py"
            worker.write_text(
                "import json,os,socket,sys,time\n"
                "from pathlib import Path\n"
                "s=socket.socket();s.bind(('127.0.0.1',0));s.listen()\n"
                "Path(sys.argv[1]).write_text(json.dumps([os.getpid(),s.getsockname()[1]]))\n"
                "time.sleep(60)\n"
            )
            script = root / "host.ps1"
            # Hidden service matches the real updater. The forced case also
            # exercises the foreground native invocation used for the game.
            args = lambda i: quote('"' + str(worker) + '" "' + str(root / (str(i) + '.json')) + '"')
            script.write_text(
                "$ErrorActionPreference='Stop'\n"
                f"Add-Type -Path {quote(helper)}\n[LocalServerLifetime]::Enable()\n"
                f"Start-Process -FilePath {quote(sys.executable)} -WindowStyle Hidden -ArgumentList {args(0)}\n"
                + (f"& {quote(sys.executable)} {quote(worker)} {quote(root / '1.json')}\n" if forced else
                   f"Start-Process -FilePath {quote(sys.executable)} -WindowStyle Hidden -ArgumentList {args(1)}\n"
                   f"while (-not (Test-Path -LiteralPath {quote(root / 'exit')})) {{ Start-Sleep -Milliseconds 50 }}\n")
            )
            parent = subprocess.Popen(
                ["powershell.exe", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(script)],
                stdout=subprocess.DEVNULL, stderr=subprocess.PIPE,
                creationflags=subprocess.CREATE_NO_WINDOW,
            )
            children = []
            try:
                deadline = time.monotonic() + 20
                while not all((root / f"{i}.json").exists() for i in range(2)):
                    if parent.poll() is not None:
                        self.fail(parent.stderr.read().decode(errors="replace"))
                    self.assertLess(time.monotonic(), deadline, "Children did not start")
                    time.sleep(.05)
                children = [json.loads((root / f"{i}.json").read_text()) for i in range(2)]
                for _, port in children:
                    with socket.create_connection(("127.0.0.1", port), timeout=1):
                        pass
                if forced:
                    parent.kill()  # Bypass PowerShell finally entirely.
                else:
                    (root / "exit").touch()
                parent.wait(timeout=10)
                for pid, port in children:
                    deadline = time.monotonic() + 5
                    while self.running(pid):
                        self.assertLess(time.monotonic(), deadline, f"Child {pid} survived")
                        time.sleep(.05)
                    with socket.socket() as probe:
                        probe.bind(("127.0.0.1", port))
            finally:
                if parent.poll() is None:
                    parent.kill()
                    parent.wait(timeout=10)
                parent.stderr.close()
                for pid, _ in children:
                    if self.running(pid):
                        os.kill(pid, 9)

    @staticmethod
    def running(pid):
        kernel = ctypes.WinDLL("kernel32", use_last_error=True)
        kernel.OpenProcess.restype = ctypes.c_void_p
        kernel.GetExitCodeProcess.argtypes = [ctypes.c_void_p, ctypes.POINTER(ctypes.c_ulong)]
        kernel.CloseHandle.argtypes = [ctypes.c_void_p]
        handle = kernel.OpenProcess(0x1000, False, pid)
        if not handle:
            return False
        try:
            code = ctypes.c_ulong()
            if not kernel.GetExitCodeProcess(handle, ctypes.byref(code)):
                raise ctypes.WinError(ctypes.get_last_error())
            return code.value == 259
        finally:
            kernel.CloseHandle(handle)

    def test_forced_host_exit_stops_hidden_and_foreground_children(self):
        self.check_exit(forced=True)

    def test_normal_host_exit_stops_children(self):
        self.check_exit(forced=False)


if __name__ == "__main__":
    unittest.main()
