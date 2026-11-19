#!/usr/bin/env bash
# Repeatable performance run on the 1920x1080 BlueStacks instance: launch, pick the saved
# account, idle in the lobby, replay King's Plains 1-4 once, then print the bootstrap's
# 5-second "perf" lines per scene. Tap coordinates match the current UI layout.
#
# usage: client/android/HybridBootstrap/measure_battle.sh [label] [--install]
set -uo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
LABEL="${1:-run}"
OUT="$HERE/../../../artifacts/android-bootstrap/perf-$LABEL.txt"
export MSYS_NO_PATHCONV=1
ADB="/c/Program Files/BlueStacks_nxt/HD-Adb.exe"
s() { "$ADB" -s emulator-5554 shell "$@"; }
probe() { s cat /sdcard/Android/data/com.sprk.hybridprobe/files/sprk-probe.txt 2>/dev/null; }
wait_for() { local n=0; until probe | grep -q "$1" || [ $n -ge "${2:-60}" ]; do sleep 2; n=$((n+1)); done; }
tap() { s input tap "$1" "$2"; sleep "${3:-3}"; }

if [ "${2:-}" = --install ]; then (timeout 200 "$HERE/run_probe.sh" --install >/dev/null 2>&1; true)
else (timeout 200 "$HERE/run_probe.sh" >/dev/null 2>&1; true); fi
wait_for "perf scene=Login"
tap 960 700 6        # tap to play
tap 884 539 2        # saved account
wait_for "perf scene=Lobby" 90
sleep 10
for _ in 1 2 3; do tap 960 1008 2; done   # attendance / login reward popups
sleep 20                                    # lobby idle sample
tap 674 742 4        # stage 1-4 (map focuses on 1-5, the newest stage)
tap 1600 985 5       # Get Ready for Battle
tap 1515 1013 3      # Start Battle
tap 717 787 2        # Battle Once
wait_for "perf scene=Battle2" 60
sleep 45                                    # battle sample
"$ADB" -s emulator-5554 exec-out screencap -p > "${OUT%.txt}.png"
probe | grep "^perf " > "$OUT"
echo "== $LABEL"
python - "$(cygpath -w "$OUT")" <<'EOF'
import re, sys, collections
rows = collections.defaultdict(list)
for line in open(sys.argv[1]):
    d = dict(re.findall(r"([\w>]+)=([\d.]+|\w+)", line))
    rows[d.get("scene")].append(d)
for scene in ("Lobby", "Battle2"):
    r = rows.get(scene, [])[1:]  # skip the scene-load window
    if not r: continue
    f = lambda k: sum(float(x[k].rstrip("ms")) for x in r) / len(r)
    print(f"{scene:8} windows={len(r)} fps={f('fps'):.1f} avg={f('avg'):.1f}ms p95={f('p95'):.1f}ms "
          f"max={max(float(x['max']) for x in r):.0f}ms hitches/5s={f('hitches>50ms'):.1f} gc0/5s={f('gc0'):.1f} heap={r[-1]['heap']}MB target={r[-1]['target']}")
EOF
