"""Non-cash shops, summons, events and pets over HTTP, including restart recovery.

Run cargo build --release first. Uses only a temporary database and an ephemeral port.
"""
import json
import os
from pathlib import Path
import socket
import sqlite3
import subprocess
import tempfile
import time
import urllib.parse
import urllib.request
from contextlib import closing


def main():
    root = Path(__file__).resolve().parents[1]
    binary = root / 'target/release' / ('sprk-server.exe' if os.name == 'nt' else 'sprk-server')
    assert binary.is_file(), 'Run cargo build --release first'
    with socket.socket() as listener:
        listener.bind(('127.0.0.1', 0))
        port = listener.getsockname()[1]
    env = dict(os.environ, PORT=str(port), CHAT_PORT='0', RUST_LOG='warn', GAME_TABLES_PATH=str(root / 'tables'))
    base = f'http://127.0.0.1:{port}'

    def post(path, **fields):
        req = urllib.request.Request(base + path, urllib.parse.urlencode(fields).encode())
        with urllib.request.urlopen(req, timeout=30) as response:
            return json.load(response)

    with tempfile.TemporaryDirectory(prefix='sprk-live-smoke-') as directory:
        expected_pet = None
        for restarted in (False, True):
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
                            assert time.monotonic() < deadline, 'Server did not become ready'
                            time.sleep(.2)
                    login = post('/user/login', LoginId='live-http-smoke')
                    key = login['UserInfo']['SessionKey']

                    def call(path, **fields):
                        return post(path, SessionKey=key, **fields)

                    def success(path, **fields):
                        result = call(path, **fields)
                        assert result['Result'] == 'Success', (path, result)
                        return result

                    assert login['EquipGachaInfos'] and login['freeEquipGachaInfos']
                    assert all(v['OnSale'] and v['IsUTCBeginTime'] == 1 for v in login['EquipGachaInfos'])
                    if not restarted:
                        with closing(sqlite3.connect(Path(directory) / 'sprk.db')) as db, db:
                            db.execute('UPDATE user_info SET gold=100000000,gem=100000,friendship_point=10000')
                        success('/shop/get_payshop_products', CategoryType='Goods')
                        success('/shop/buy_payshop_product', Index=920001)
                        assert call('/shop/buy_payshop_product', Index=920001)['Result'] != 'Success'
                        assert call('/shop/buy_payshop_product', Index=1)['Result'] != 'Success'
                        success('/shop/get_select_shop_info', CategoryGroup='Fragment')
                        success('/shop/restock_select_shop_info', CategoryGroup='Fragment')
                        success('/shop/refresh_payshop_discount_info')
                        success('/equip_gacha/exec_equip_gacha', GachaIndex=3, Free='true')
                        success('/equip_gacha/exec_equip_gacha', GachaIndex=4, HighGachaCategory='All')
                        pet = success('/equip_gacha/exec_equip_gacha', GachaIndex=28)['GachaItemResults'][0]['PetItemResult']
                        expected_pet = pet['PetIndex']
                        success('/pet/change_pet_layout', PetIndices=json.dumps([expected_pet, 0, 0]))
                        success('/pet/feed_the_pet', PetIndex=expected_pet, FeedCount=1)
                        success('/pet/egg_supplier')
                        success('/pet/set_egg_in_pet_incubator', SlotIndex=101, IncubatorIndex=11001, ItemIndex=7500010)
                        assert call('/pet/get_egg_rewards', SlotIndex=101, IncubatorIndex=11001, ItemIndex=7500010)['Result'] != 'Success'
                        success('/shop/buy_payshop_product', Index=920002)
                        success('/event_step/put_event_step')
                        success('/event_step/put_event_step')
                        success('/event_step/get_event_step_reward', EventStepType='Daily')
                        success('/event/event_calendar', Language='en')
                        success('/item/reward_event_roulette', RouletteIndex=1, RouletteCount=1)
                        success('/shop/buy_purchase_dungeon', ChapterIndex=40001, DungeonIndex=1, Price=200)
                    else:
                        assert any(p['PetIndex'] == expected_pet for p in login['PetInfos'])
                        assert any(p['ProductIndex'] == 920001 and p['PurchasedCount'] == 1 for p in login['PlayerProductPurchaseInfos'])
                        assert call('/equip_gacha/exec_equip_gacha', GachaIndex=3, Free='true')['Result'] != 'Success'
                        house = success('/pet/get_pet_house_info')
                        assert house['PetIncubatorSlotInfos'][0]['SetItemIndex'] == 7500010
                        assert success('/event_step/get_event_step_info')['EventStepInfo']['DailyStep'] == 1
                        assert call('/pet/egg_supplier')['Result'] != 'Success'
                        assert login['ItemTimeDurations']
                        lobby = success('/user/first_lobby')
                        assert any(p['PetIndex'] == expected_pet for p in lobby['PetInfos'])
                finally:
                    process.terminate()
                    try:
                        process.wait(timeout=10)
                    except subprocess.TimeoutExpired:
                        process.kill()
                        process.wait(timeout=10)
    print('Non-cash shops, summons, events, pets and restart recovery passed.')


if __name__ == '__main__':
    main()
