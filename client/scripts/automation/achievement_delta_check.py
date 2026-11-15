#!/usr/bin/env python3
"""Real-client check of partial AchievementInfos replies (achievement deltas).

Usage, against a run launched with an `achievement-delta-*` profile and logged in:
    python -I client/scripts/automation/achievement_delta_check.py <run dir> <label> [SQL]

Runs SQL (if given) on the run's isolated database to change the test
account's progress, then has the client send a gameplay request through its
native requester (`achievement-delta-probe`), so OnBaseResponse applies the
reply exactly as in play. Prints the section headers sent, the reply size, how
many achievements it carried (-1: omitted), and compares the client's
resulting achievement list with the server's full list. Exits 1 on a mismatch.
"""
import json
import sys
import time
import urllib.parse
import urllib.request
from contextlib import closing
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import client_automation as ca  # noqa: E402


def probe(root: Path) -> dict:
    ca.request(root, "achievement-delta-probe", target=1)
    deadline = time.monotonic() + 60
    while (status := ca.request(root, "achievement-delta-probe")["materials"][-1]) == "Running":
        if time.monotonic() > deadline:
            raise SystemExit("probe timed out")
        time.sleep(0.5)
    if not status.startswith("DONE "):
        raise SystemExit(status[:300])
    return dict(part.split("=", 1) for part in status[len("DONE "):].split(" "))


def server_list(session: str) -> str:
    # Without section headers the lobby reply always carries the full list.
    body = urllib.parse.urlencode({"SessionKey": session}).encode()
    request = urllib.request.Request("http://127.0.0.1:8080/lobby/enter_lobby", data=body)
    with urllib.request.urlopen(request, timeout=60) as response:
        lobby = json.loads(response.read())
    return ";".join(f"{a['AchievementIndex']}:{a['LastStep']}:{a['Achievement']}:{a['ClearTeamLevel']}"
                    for a in sorted(lobby["AchievementInfos"], key=lambda a: a["AchievementIndex"]))


def main() -> None:
    root, label = ca.session(sys.argv[1]), sys.argv[2]
    if len(sys.argv) > 3:
        url = ca.read(root / "session.json")["database"]
        with closing(ca.psycopg().connect(url, autocommit=True)) as db:
            db.execute(sys.argv[3])
    fields = probe(root)
    expected = server_list(fields["session"])
    total = len(expected.split(";"))
    print(f"{label}: sent {fields['sent']}; reply {fields['replyBytes']} bytes, token {fields['replyToken']}")
    print(f"{label}: result={fields['result']} carried={fields['carried']} of {total} achievements; "
          f"client list {'matches' if fields['list'] == expected else 'DIFFERS FROM'} the server's")
    if fields["list"] != expected:
        client, server = fields["list"].split(";"), expected.split(";")
        print("  client only:", [x for x in client if x not in server][:5])
        print("  server only:", [x for x in server if x not in client][:5])
        sys.exit(1)


if __name__ == "__main__":
    main()
