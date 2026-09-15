"""Check native progression HTTP routes and restart persistence in a temporary database.

Run after cargo build --release. Never opens the workspace's sprk.db.
"""
import json
import os
from pathlib import Path
import socket
import subprocess
import tempfile
import time
import urllib.parse
import urllib.request


def main():
    root = Path(__file__).resolve().parents[1]
    binary = root / 'target' / 'release' / ('sprk-server.exe' if os.name == 'nt' else 'sprk-server')
    assert binary.is_file(), 'Run cargo build --release first'
    with socket.socket() as listener:
        listener.bind(('127.0.0.1', 0))
        port = listener.getsockname()[1]
    env = dict(os.environ, PORT=str(port), CHAT_PORT='0', RUST_LOG='warn',
               GAME_TABLES_PATH=str(root / 'tables'))
    base = f'http://127.0.0.1:{port}'

    def post(path, **fields):
        request = urllib.request.Request(base + path, urllib.parse.urlencode(fields).encode())
        with urllib.request.urlopen(request, timeout=30) as response:
            return json.load(response)

    with tempfile.TemporaryDirectory(prefix='sprk-progression-smoke-') as directory:
        for restarted in (False, True):
            with open(Path(directory) / 'server.log', 'ab') as log:
                process = subprocess.Popen([str(binary)], cwd=directory, env=env,
                                           stdout=log, stderr=log,
                                           creationflags=subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0)
                try:
                    deadline = time.monotonic() + 60
                    while True:
                        assert process.poll() is None, 'Server exited; see temporary server.log'
                        try:
                            with urllib.request.urlopen(base + '/health', timeout=1):
                                break
                        except OSError:
                            assert time.monotonic() < deadline, 'Server did not become ready'
                            time.sleep(0.2)
                    login = post('/user/login', LoginId='progression-http-smoke')
                    assert login['BaseResult'] == 'Success'
                    session = login['UserInfo']['SessionKey']
                    assert login['MiscInfo']['LoginDailyCount'] == 1
                    assert len(login['AchievementInfos']) > 100
                    lobby = post('/lobby/enter_lobby', SessionKey=session)
                    assert lobby['AttendanceDatas'][0]['Index'] == 1
                    claimed = post('/attendance/get_attendance_reward', SessionKey=session, AttendanceIndex=1)
                    assert claimed['Result'] == ('Fail' if restarted else 'Success')
                    if not restarted:
                        reward = post('/achievement/reward_achievement', SessionKey=session,
                                      AchievementIndices='[1101]', Steps='[1]')
                        assert reward['Result'] == 'Success'
                        assert 'itemResults' in reward and 'ReservedSubQuestInfos' in reward
                    else:
                        assert login['AttendanceInfos'][0]['LastRewardedDay'] == 1
                        achievement = next(r for r in login['AchievementInfos'] if r['AchievementIndex'] == 1101)
                        assert achievement['LastStep'] == 1
                    print('Restart persistence passed' if restarted else 'Native login/lobby/reward HTTP smoke passed')
                finally:
                    process.terminate()
                    process.wait(timeout=15)


if __name__ == '__main__':
    main()
