"""Verify actual Unity gzip negotiation and native manager preservation in isolation."""
import argparse
import gzip
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
import os
from pathlib import Path
import shutil
import subprocess
import threading
import uuid

from verify_graphics_options import prepare, REPO, DATA


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--client', type=Path, default=REPO.parent / 'sprk-client')
    parser.add_argument('--installed', action='store_true')
    parser.add_argument('--release-client', type=Path, help='Use managed assemblies from an isolated full release build with the installed Unity runtime')
    args = parser.parse_args()
    root = REPO / 'target/network-performance' / uuid.uuid4().hex[:12]
    prepare(args.client.resolve(), root, installed=True)
    if args.release_client:
        for source in (args.release_client / DATA / 'Managed').glob('*.dll'):
            shutil.copy2(source, root / DATA / 'Managed' / source.name)
    if not args.installed and not args.release_client:
        subprocess.run(['dotnet', 'run', '--configuration', 'Release', '--project', str(REPO / 'scripts/DllPatcher'), '--', str(root), '--network-performance-only'], cwd=REPO, check=True)
    subprocess.run(['dotnet', 'run', '--configuration', 'Release', '--project', str(REPO / 'scripts/DllPatcher'), '--', str(root), '--network-runtime-probe'], cwd=REPO, check=True)
    payload = json.dumps({'BaseResult': 'Success', 'Result': 'Success',
                         'AchievementInfos': [{'AchievementIndex': i, 'LastStep': 0, 'Achievement': 1} for i in range(1, 2001)],
                         'ReservedPlayRecordInfos': [{'RecordIndex': 1, 'RecordValue': '7'}],
                         'ReservedMainQuestInfo': {'Step': 1, 'Progress': 0},
                         'ReservedClearMissionInfos': [{'MissionIndex': 1, 'LastStep': 2}],
                         'SprkMainQuestToken': 'b' * 64, 'SprkClearMissionToken': 'c' * 64,
                         'SprkAchievementToken': 'a' * 64}).encode()
    compressed = gzip.compress(payload)
    observations = []

    class Handler(BaseHTTPRequestHandler):
        protocol_version = 'HTTP/1.1'

        def do_POST(self):
            self.rfile.read(int(self.headers.get('Content-Length', 0)))
            accept = self.headers.get('Accept-Encoding', '')
            observations.append({'accept_encoding': accept, 'compressed_bytes': len(compressed), 'json_bytes': len(payload)})
            self.send_response(200)
            self.send_header('Content-Type', 'application/json')
            self.send_header('Content-Encoding', 'gzip')
            self.send_header('Content-Length', str(len(compressed)))
            self.send_header('Server-Timing', 'app;dur=0.123')
            self.end_headers()
            self.wfile.write(compressed)

        def log_message(self, *args):
            pass

    with ThreadingHTTPServer(('127.0.0.1', 0), Handler) as server:
        server.daemon_threads = True
        thread = threading.Thread(target=server.serve_forever, daemon=True)
        thread.start()
        env = dict(os.environ, SPRK_NETWORK_PROBE=str(root), SPRK_NETWORK_PROBE_ORIGIN=f'http://127.0.0.1:{server.server_port}/',
                   SPRK_NETWORK_TRACE='1', SPRK_NETWORK_TRACE_FILE=str(root / 'network.log'), SPRK_AUTOMATION_DIR=str(root / 'automation'), SPRK_AUTOMATION_PROFILE='network-' + root.name)
        for key in list(env):
            if key.startswith('SPRK_GRAPHICS_'):
                env.pop(key)
        startup = subprocess.STARTUPINFO()
        startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        startup.wShowWindow = 0
        executable = next(root.glob('SprkTest-graphics-*.exe'))
        process = subprocess.Popen([str(executable), '-screen-fullscreen', '0', '-screen-width', '640', '-screen-height', '480', '-logFile', str(root / 'unity.log')], cwd=root, env=env, startupinfo=startup)
        try:
            code = process.wait(timeout=90)
        except subprocess.TimeoutExpired:
            process.terminate()
            process.wait(timeout=15)
            raise RuntimeError(f'Network probe timed out: {root}')
        finally:
            server.shutdown()
        (root / 'transport.json').write_text(json.dumps(observations, indent=2))
        result = (root / 'result.txt').read_text()
        if code or not result.startswith('PASS:'):
            raise AssertionError(result)
        if not observations or 'gzip' not in observations[0]['accept_encoding'].lower():
            raise AssertionError(f'Unity did not negotiate gzip: {observations}')
        (root / 'transport.json').write_text(json.dumps(observations, indent=2))
        log = (root / 'network.log').read_text(encoding='utf-8', errors='replace')
        assert 'SPRK network transport path=/snapshot' in log
        assert 'SPRK network apply path=snapshot' in log
        print(result)
        print(json.dumps(observations[0]))
        print('Artifacts:', root)


if __name__ == '__main__':
    main()
