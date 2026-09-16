"""Exercise native battle HTTP routes and restart recovery in an isolated database.

Run after cargo build --release. The workspace sprk.db is never opened.
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
    binary = root / "target" / "release" / ("sprk-server.exe" if os.name == "nt" else "sprk-server")
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

    stage = dict(ChapterIndex=1, DungeonIndex=1, DungeonDifficulty=1)
    with tempfile.TemporaryDirectory(prefix="sprk-battle-smoke-") as directory:
        room = None
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
                    login = post("/user/login", LoginId="battle-http-smoke")
                    assert login["BaseResult"] == "Success", login
                    session = login["UserInfo"]["SessionKey"]

                    def call(path, **fields):
                        return post(path, SessionKey=session, **fields)

                    lobby = call("/lobby/enter_lobby")
                    assert "TopClearDungeonInfos" in lobby and "DispatchBattleInfos" in lobby
                    assert call("/world_boss/get_world_boss_info")["WorldBossInfos"]
                    assert call("/match/get_season_info", ArenaType="Normal")["SeasonData"]["SeasonIndex"] > 0
                    if not restarted:
                        result = call("/campaign/begin_campaign", **stage, HeroIndices="[1]")
                        assert result["Result"] == "Success", result
                        assert result == call("/campaign/begin_campaign", **stage, HeroIndices="[1]")
                        result = call("/party_dungeon/create_party_dungeon_room", DungeonType=1,
                                      ChapterIndex=1, DungeonIndex=1, DungeonDifficulty="Normal", Opened=1)
                        assert result["Result"] == "Success", result
                        room = result["RoomNo"]
                        print("Battle entry and room HTTP checks passed", flush=True)
                    else:
                        result = call("/party_dungeon/join_party_dungeon_room", RoomNo=room)
                        assert result["Result"] == "Success", result
                        result = call("/campaign/end_campaign", **stage, Completed="true", Star=3, AliveHeroIndices="[1]")
                        assert result["Result"] == "Success", result
                        assert [h["HeroIndex"] for h in result["HeroExpResults"]] == [1]
                        assert call("/campaign/end_campaign", **stage, Completed="true")["Result"] != "Success"
                        dispatch = call("/dispatch/start_dispatch", ChapterIndex=1, DungeonIndex=1,
                                        Difficulty=1, HeroIndices="[1]", RepeatCount=2, DeckIndex=1)
                        assert dispatch["Result"] == "Success", dispatch
                        slot = dispatch["DispatchBattleInfo"]["SlotIndex"]
                        assert call("/dispatch/request_complete_dispatch", SlotIndex=slot)["Result"] != "Success"
                        assert call("/dispatch/cancel_dispatch", SlotIndex=slot)["Result"] == "Success"
                        lobby = call("/lobby/enter_lobby")
                        dungeon = next(d for d in lobby["DungeonInfos"] if d["ChapterIndex"] == 1 and d["DungeonIndex"] == 1)
                        assert dungeon["FirstRewardedDiff"] & 2
                        print("Restarted battle completion, replay rejection, dispatch and lobby checks passed", flush=True)
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
