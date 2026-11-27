#!/usr/bin/env python3
"""Build server/tables/DispatchRewardBoxes.json: the sealed box each dispatch win pays.

The native game did not deliver Dispatch Battle loot loose. Each won run gave
one "<stage> Dispatch Battle Loot" box (DISPATCH_REWARD_BOX_*, a Package item
whose contents are the stage's own loot); the player opened the boxes later.
The native claim responses (RequestCompleteDispatch, CancelDispatch) carry
ItemResults and currencies but no equipment, so this is the only way they could
deliver the stage's gear.

The client tables keep every box, its contents and a RewardTable row that
grants exactly one box (550001-550401), but nothing names the box of a stage:
that link lived in the original server. This script rebuilds it from contents.
A stage's loot is its DropRewardIndex_<difficulty> plus, for a raid, the solo
raid row's IndividualReward; its box is the one whose contents cover that loot
best. Loot the box does not contain must be a fixed count of a plain item
(the dragons' MATERIAL_77) and is listed in DirectItems, to be granted beside
the box. Stages with no such box (story Hard and Very Hard) are left out, and
the server pays their loot directly as before.

Inputs are decoded client tables, one JSON file per table: either a list of
rows (kingsraid-table-data `output/MessagePack`) or a JitTables dump
(`{"upsert": [...]}`). Needed: RewardTable, PackageItemTable,
CampaignDungeonTable, RaidTable, ItemTable (numbered chunks such as
RewardTable1.json are read too).

    python server/scripts/tables/build_dispatch_boxes.py --tables <dir>
"""
from __future__ import annotations

import argparse
import json
import re
from collections import Counter
from pathlib import Path

REPO = Path(__file__).resolve().parents[3]
OUTPUT = REPO / "server" / "tables" / "DispatchRewardBoxes.json"
DIFFICULTIES = ["Easy", "Normal", "Hard", "Hell"]
GRANT_ROWS = range(550001, 550402)
BOX_PREFIX = "DISPATCH_REWARD_BOX_"


def load(tables: Path, name: str) -> list[dict]:
    rows: list[dict] = []
    for path in sorted(tables.glob(f"{name}*.json")):
        if not re.fullmatch(rf"{name}\d*", path.stem):
            continue
        data = json.loads(path.read_text(encoding="utf-8"))
        rows.extend(data["upsert"] if isinstance(data, dict) else data)
    if not rows:
        raise SystemExit(f"{tables} has no {name}")
    return rows


def code(value) -> str | None:
    # MessagePack exports keep interned strings as [text, length].
    return value[0] if isinstance(value, list) else value


def build(tables: Path) -> list[dict]:
    rewards = {r["Index"]: r for r in load(tables, "RewardTable")}
    items = {}
    for r in load(tables, "ItemTable"):
        name = code(r.get("Code", r.get("_slot_1")))
        items[name] = (r.get("Index", r.get("_slot_0")), r.get("Type", r.get("_slot_28")))
    packages = {r["ItemCode"]: r for r in load(tables, "PackageItemTable") if r["ItemCode"].startswith(BOX_PREFIX)}

    def loot(index: int) -> Counter:
        row = rewards.get(index) or {}
        return Counter(
            (code(row[f"Item{n}Code"]), row[f"Item{n}Rate"], row[f"Item{n}CountMin"], row[f"Item{n}CountMax"])
            for n in range(1, 16) if code(row.get(f"Item{n}Code"))
        )

    grants: dict[str, int] = {}
    for index in GRANT_ROWS:
        entries = list(loot(index).elements())
        if len(entries) == 1 and entries[0][0] in packages:
            grants.setdefault(entries[0][0], index)
    contents = {box: loot(packages[box]["RewardIndex1"]) for box in grants}

    raids: dict[tuple[int, int], list[dict]] = {}
    for raid in load(tables, "RaidTable"):
        if not raid["IsOnlineSingle"]:
            raids.setdefault((raid["ChapterIndex"], raid["DungeonIndex"]), []).append(raid)

    out = []
    for dungeon in load(tables, "CampaignDungeonTable"):
        key = (dungeon["ChapterIndex"], dungeon["DungeonIndex"])
        for difficulty, label in enumerate(DIFFICULTIES):
            if not dungeon[f"DispatchType_{label}"]:
                continue
            drop = loot(dungeon[f"DropRewardIndex_{label}"])
            candidates = [drop + loot(r["IndividualReward"]) for r in raids.get(key, [])] or [drop]
            best = None
            for stage in candidates:
                for box, inside in contents.items():
                    covered = sum((stage & inside).values())
                    left = stage - inside
                    score = (sum(left.values()), sum((inside - stage).values()), grants[box])
                    if covered > sum(left.values()) and (best is None or score < best[0]):
                        best = (score, box, left)
            if best is None:
                continue
            _, box, left = best
            direct = []
            for item, rate, low, high in sorted(left.elements()):
                index, kind = items.get(item, (None, None))
                if rate != 1000 or low != high or kind in (None, "Equip", "Package"):
                    break
                direct.append({"ItemIndex": index, "Count": low, "Item": item})
            else:
                out.append({
                    "ChapterIndex": key[0], "DungeonIndex": key[1], "Difficulty": difficulty,
                    "RewardIndex": grants[box], "Item": box,
                    **({"DirectItems": direct} if direct else {}),
                })
    out.sort(key=lambda r: (r["ChapterIndex"], r["DungeonIndex"], r["Difficulty"]))
    return out


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--tables", type=Path, required=True, help="folder of decoded client table JSON")
    parser.add_argument("--output", type=Path, default=OUTPUT)
    args = parser.parse_args()
    rows = build(args.tables)
    args.output.write_text(json.dumps(rows, indent=1) + "\n", encoding="utf-8")
    boxes = len({r["Item"] for r in rows})
    print(f"Wrote {len(rows)} dispatch stages using {boxes} boxes to {args.output}")


if __name__ == "__main__":
    main()
