"""Supporting APIs over HTTP/TCP and a process restart; uses a temporary database only.
Run cargo build --release first. No native Unity playback or combat simulator is launched.
"""
import base64
import json
import os
from pathlib import Path
import secrets
import socket
import sqlite3
import subprocess
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request
from contextlib import closing


def free_port():
    with socket.socket() as s:
        s.bind(('127.0.0.1', 0))
        return s.getsockname()[1]


def main():
    root = Path(__file__).resolve().parents[1]
    binary = root / 'target/release' / ('sprk-server.exe' if os.name == 'nt' else 'sprk-server')
    port, chat_port = free_port(), free_port()
    secret = secrets.token_hex(32)
    env = dict(os.environ, PORT=str(port), CHAT_PORT=str(chat_port), CHAT_BIND='127.0.0.1',
               BATTLE_SERVICE_KEY=secret, RUST_LOG='warn', GAME_TABLES_PATH=str(root / 'tables'))
    base = f'http://127.0.0.1:{port}'

    def post(path, headers=None, **fields):
        req = urllib.request.Request(base + path, urllib.parse.urlencode(fields).encode(), headers=headers or {})
        with urllib.request.urlopen(req, timeout=30) as response:
            return json.load(response)

    def packet(sock, name, body):
        sock.sendall(name.encode() + b' ' + base64.b64encode(json.dumps(body).encode()) + b'\r\n')
        # Only one login response is needed; no buffered read-ahead is discarded.
        line = bytearray()
        while not line.endswith(b'\n'):
            part = sock.recv(1)
            assert part, 'Chat socket closed'
            line.extend(part)
        return json.loads(base64.b64decode(line.split(b' ', 1)[1]))

    with tempfile.TemporaryDirectory(prefix='sprk-services-smoke-') as directory:
        replay_id = run_id = None
        for restart in (False, True):
            with open(Path(directory) / 'server.log', 'ab') as log:
                process = subprocess.Popen([str(binary)], cwd=directory, env=env, stdout=log, stderr=log,
                    creationflags=subprocess.CREATE_NO_WINDOW if os.name == 'nt' else 0)
                try:
                    deadline = time.monotonic() + 60
                    while True:
                        assert process.poll() is None, (Path(directory) / 'server.log').read_text()
                        try:
                            with urllib.request.urlopen(base + '/health', timeout=1):
                                break
                        except OSError:
                            assert time.monotonic() < deadline, 'Server did not start'
                            time.sleep(.2)
                    login = post('/user/login', LoginId='services-smoke')
                    key, account = login['UserInfo']['SessionKey'], login['UserInfo']['AccountId']

                    def call(path, **fields):
                        return post(path, SessionKey=key, **fields)

                    def ok(path, **fields):
                        result = call(path, **fields)
                        assert result['Result'] == 'Success', (path, result)
                        return result

                    entry = dict(ChapterIndex=1, DungeonIndex=1, DungeonDifficulty=1)
                    if not restart:
                        with closing(sqlite3.connect(Path(directory) / 'sprk.db')) as db, db:
                            db.execute('UPDATE user_info SET stamina=1000,gem=5000')
                            db.execute('INSERT INTO equip_items(account_id,slot_index,item_index) VALUES(?,50,1010101)', (account,))
                        stamina = ok('/user/buy_stamina', StaminaType='Chicken', Amount=999999)
                        assert stamina['StaminaResult']['AddValue'] == 150
                        assert stamina['CurrencyResult']['AddValue'] == -50
                        ok('/user/recharge_stamina', StaminaType='UndergroundPrisonKey')
                        replay_id = ok('/replay/save_replay', Type='Arena', Info='{}', BattleLogs='opaque-lz4')['ReplayUid']
                        # A logged-in account ID alone must not authenticate a socket.
                        with socket.create_connection(('127.0.0.1', chat_port), timeout=5) as chat:
                            assert packet(chat, 'LoginReq', dict(AccountId=str(account), RequestId='1'))['Result'] == 'Fail'
                        with socket.create_connection(('127.0.0.1', chat_port), timeout=5) as chat:
                            assert packet(chat, 'LoginReq', dict(AccountId=str(account), SessionKey=key, RequestId='2'))['Result'] == 'Success'
                        linked = ok('/chat/world', Chat='equipment', LinkedItem=json.dumps([dict(EquipItemInfo=dict(SlotIndex='50', ItemIndex=999))]))
                        content = json.loads(linked['Message']['Content'])
                        assert content['LinkedItem'][0]['EquipItemInfo']['ItemIndex'] == 1010101
                        run_id = ok('/campaign/begin_campaign', HeroIndices='[1]', **entry)['RunId']
                        try:
                            post('/internal/b2g_battle_start', AccountId=account, **entry)
                            raise AssertionError('Missing service secret was accepted')
                        except urllib.error.HTTPError as error:
                            assert error.code == 401
                        headers = {'x-battle-service-key': secret, 'x-battle-run-id': run_id}
                        assert post('/internal/b2g_battle_start', headers=headers, AccountId=account, **entry)['Result'] == 'Success'
                    else:
                        assert login['ServiceBattle']['RunId'] == run_id
                        assert login['ServiceBattle']['ServiceOwned']
                        assert ok('/replay/get_replay', ReplayUid=replay_id)['Replay']['Data'] == 'opaque-lz4'
                        assert ok('/user/get_stamina', StaminaType=5)['StaminaResult']['RechargeCount'] == 1
                        assert ok('/battle/recover')['Battle']['RunId'] == run_id
                        assert call('/campaign/end_campaign', Completed='true', AliveHeroIndices='[1]', **entry)['Result'] != 'Success'
                        headers = {'x-battle-service-key': secret, 'x-battle-run-id': run_id}
                        result = dict(AccountId=account, Win='true', PlayTime=30, AliveHeroIndices='[1]', TotalDamage=100)
                        assert post('/internal/b2g_set_campaign_result', headers=headers, **result)['Result'] == 'Success'
                        with closing(sqlite3.connect(Path(directory) / 'sprk.db')) as db:
                            balance = db.execute('SELECT gold,gem FROM user_info WHERE account_id=?', (account,)).fetchone()
                        assert post('/internal/b2g_set_campaign_result', headers=headers, **result)['Result'] == 'Success'
                        with closing(sqlite3.connect(Path(directory) / 'sprk.db')) as db:
                            assert balance == db.execute('SELECT gold,gem FROM user_info WHERE account_id=?', (account,)).fetchone()
                        decks = ok('/recommend_deck/get_recommend_deck_list', ChapterIndex=1, DungeonIndex=1, Difficulty=1)
                        assert len(decks['RecommendDecks']) == 2
                        ok('/records_of_honor/get_records_of_honor_ranking_list', ContentType='Match', Season=0)
                        lobby = ok('/enter/lobby')
                        assert lobby['ServiceBattle']['Completed']
                        assert any(v['Type'] == 'Chicken' for v in lobby['StaminaResults'])
                finally:
                    process.terminate()
                    try:
                        process.wait(timeout=10)
                    except subprocess.TimeoutExpired:
                        process.kill()
                        process.wait(timeout=10)
    print('Supporting HTTP/TCP services, callback idempotency, and restart recovery passed.')


if __name__ == '__main__':
    main()
