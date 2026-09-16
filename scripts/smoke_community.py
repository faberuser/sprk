"""Native arena/guild HTTP checks and restart recovery using a temporary database."""
import json
from contextlib import closing
import os
from pathlib import Path
import socket
import sqlite3
import subprocess
import tempfile
import time
import urllib.parse
import urllib.request


def main():
    root = Path(__file__).resolve().parents[1]
    binary = root / "target/release" / ("sprk-server.exe" if os.name == "nt" else "sprk-server")
    assert binary.is_file(), "Run cargo build --release first"
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        port = listener.getsockname()[1]
    env = dict(os.environ, PORT=str(port), CHAT_PORT="0", RUST_LOG="warn",
               GAME_TABLES_PATH=str(root / "tables"))
    base = f"http://127.0.0.1:{port}"

    def post(path, **fields):
        request = urllib.request.Request(base + path, urllib.parse.urlencode(fields).encode())
        with urllib.request.urlopen(request, timeout=30) as response:
            return json.load(response)

    with tempfile.TemporaryDirectory(prefix="sprk-community-smoke-") as directory:
        guild = None
        for restarted in (False, True):
            with open(Path(directory) / "server.log", "ab") as log:
                process = subprocess.Popen([str(binary)], cwd=directory, env=env,
                                           stdout=log, stderr=log,
                                           creationflags=subprocess.CREATE_NO_WINDOW if os.name == "nt" else 0)
                try:
                    deadline = time.monotonic() + 60
                    while True:
                        assert process.poll() is None, "Server exited before health check"
                        try:
                            with urllib.request.urlopen(base + "/health", timeout=1):
                                break
                        except OSError:
                            assert time.monotonic() < deadline, "Server did not become ready"
                            time.sleep(0.2)
                    master = post("/user/login", LoginId="guild-smoke-master")
                    member = post("/user/login", LoginId="guild-smoke-member")
                    account = master["UserInfo"]["AccountId"]

                    def call(path, player=master, **fields):
                        return post(path, SessionKey=player["UserInfo"]["SessionKey"], **fields)

                    def success(path, **fields):
                        result = call(path, **fields)
                        assert result["Result"] == "Success", (path, result)
                        return result

                    if not restarted:
                        with closing(sqlite3.connect(Path(directory) / "sprk.db")) as db, db:
                            db.execute("UPDATE user_info SET team_level=60,gold=100000000,gem=100000")
                        assert call("/shop/get_shop_list", ShopIndex=4)["Result"] != "Success"
                        created = success("/guild/create_guild", name="HttpGuild", logo=1,
                                          logoBackground=1, joinWay=2, reqTeamLevel=1)
                        guild = created["GuildId"]
                        success("/guild/request_join_guild", player=member, GuildId=guild)
                        success("/guild/accept_join_request", AccountId=member["UserInfo"]["AccountId"])
                        success("/guild/set_guild_attendance")
                        assert call("/guild/set_guild_attendance")["Result"] != "Success"
                        success("/guild/contribute_guild")
                        with closing(sqlite3.connect(Path(directory) / "sprk.db")) as db, db:
                            db.execute("UPDATE guilds SET exp=10000000 WHERE guild_id=?", (guild,))
                            db.execute("UPDATE community_state SET data=json_set(data,'$.ActivityPoint',10000000,'$.Wood',1000000,'$.Stone',1000000,'$.Metal',1000000) WHERE owner=? AND kind='guild'", (guild,))
                            db.execute("UPDATE battle_currencies SET value=100000 WHERE account=? AND kind='GuildPoint'", (account,))
                        success("/guild/level_up_guild", Level=2)
                        success("/guild/level_up_guild_building", BuildingIndex=3, BuildingLevel=1)
                        success("/shop/get_shop_list", ShopIndex=4)
                        bought = success("/shop/buy_shop_item", ShopIndex=4, ShopItemIndex=1, ShopItemPurchaseCount=1)
                        assert bought["GuildPointResult"]["AddValue"] < 0
                        success("/match/register_match", HeroIndices="[1]", ArenaType="Normal")
                        wait = call("/match/wait_match", PlayOfflineMatch="true")
                        assert wait["Result"] == "WaitMore" and wait["MatchedNpcInfo"]
                        assert wait == call("/match/wait_match", PlayOfflineMatch="true")
                        print("Guild membership, attendance, shop purchase and arena entry passed", flush=True)
                    else:
                        assert master["GuildInfo"]["Id"] == guild
                        result = success("/match/set_offline_match_result", Win=1, PlayTime=30, AliveHeroIndices="[1]")
                        assert result["MatchResult"]["GainedMatchScore"] == 20
                        assert call("/match/set_offline_match_result", Win=1)["Result"] != "Success"
                        raid = dict(ChapterIndex=6002, DungeonIndex=1, DungeonDifficulty=0)
                        started = success("/campaign/begin_campaign", **raid, HeroIndices="[1]")
                        assert started["StaminaResult"]["AddValue"] == -1
                        success("/campaign/end_campaign", **raid, Completed="false", TotalDamage=1000000000)
                        assert call("/campaign/end_campaign", **raid, TotalDamage=1000000000)["Result"] != "Success"
                        booty = success("/guild_raid/get_guild_raid_all_booty_items")
                        stock = booty["EquipItemInfos"] + booty["ItemInfos"]
                        assert stock, booty
                        item = stock[0]
                        bought = success("/guild_raid/buy_guild_raid_booty_item", Id=item["Id"],
                                         ShopIndex=4, ItemIndex=item["ItemIndex"], Count=1)
                        assert bought["GuildPointResult"]["AddValue"] < 0
                        assert bought["EquipItemInfo"] or bought["ItemResult"]
                        success("/mail/receive_all_mail")
                        lobby = call("/lobby/enter_lobby")
                        assert lobby["GuildInfo"]["Id"] == guild
                        assert lobby["BattleInfo"]["MatchScore"] == 1020
                        print("Restart recovery, result replay rejection, raid loot and reward mail passed", flush=True)
                except Exception:
                    print((Path(directory) / "server.log").read_text(errors="replace")[-6000:])
                    raise
                finally:
                    process.terminate()
                    try:
                        process.wait(timeout=15)
                    except subprocess.TimeoutExpired:
                        process.kill()
                        process.wait(timeout=15)


if __name__ == "__main__":
    main()
