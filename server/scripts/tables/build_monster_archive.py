#!/usr/bin/env python3
"""Build server/tables/MonsterArchiveSupport.json for the Monster Index.

The server records the monsters a player defeats so the Monster Index
(Illustrated Book) can show them in color. It needs to know which archive
monsters each dungeon contains, which only the client's tables describe. This
script mirrors the client's lookup (`UIHelper.GetMonsterInfoFromDungeon` over
`CampaignWaveDataContainer.GetData`):

* a battle at difficulty D reads the waves of difficulty D, from wave 1 to the
  first missing wave;
* a story (scenario) battle reads each wave from the scenario waves, falling
  back to the dungeon's lowest difficulty, up to `ScenarioEndWave`;
* allied and hidden wave members (`Ally`/`Hide` wave flags) are skipped;
* only creatures whose `ArchiveType` has a Monster row in PlayerArchiveTable
  are kept, under that archive type.

Inputs are JitTables dumps of the three client tables (pristine baseline or the
installed game; they are not patched by SPRK):

    cd client
    dotnet run --project tools/JitTables -c Release -- dump "<TableJit>" CampaignWaveTable <dumps>/CampaignWaveTable.json
    dotnet run --project tools/JitTables -c Release -- dump "<TableJit>" CreatureTable <dumps>/CreatureTable.json
    dotnet run --project tools/JitTables -c Release -- dump "<TableJit>" PlayerArchiveTable <dumps>/PlayerArchiveTable.json
    cd ..
    python server/scripts/tables/build_monster_archive.py --dumps <dumps>

`ScenarioEndWave` and tutorial dungeons come from server/tables/BattleSupport.json.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from collections import defaultdict
from pathlib import Path

REPO = Path(__file__).resolve().parents[3]
TABLES = REPO / "server" / "tables"
OUTPUT = TABLES / "MonsterArchiveSupport.json"

# NShared.ArchiveType and NShared.DungeonDifficultyType, in declaration order.
ARCHIVE_TYPES = ["None", "Raid", "ChallengeRaid", "GuildRaid", "WorldBoss", "ChapterBoss", "Conquest",
                 "Normal", "GuildSuppress", "GodkingTrial", "Eclipse", "ShakmehDungeon", "TechnoMagic",
                 "PunishmentRaid"]
DIFFICULTIES = ["Easy", "Normal", "Hard", "Hell"]
# The Monster Index skips these dungeons (MonsterListBook.GetDungeonMonsterIndexList).
TUTORIAL_CODES = {"Tutorial", "Tutorial2"}
SKIPPED_FLAGS = {"Ally", "Hide"}


def rows(path: Path) -> list[dict]:
    return json.loads(path.read_text(encoding="utf-8"))["upsert"]


def enum_value(names: list[str], value) -> int:
    if isinstance(value, int):
        return value
    return names.index(value or names[0])


def build(dumps: Path) -> dict:
    sources = {name: dumps / f"{name}.json" for name in ("CampaignWaveTable", "CreatureTable", "PlayerArchiveTable")}
    archive_main = None
    archive_subs = set()
    for row in rows(sources["PlayerArchiveTable"]):
        if row.get("ArchiveMainType") == "Monster":
            archive_main = row["MainIndex"]
            archive_subs.add(row["SubIndex"])
    if archive_main is None:
        raise SystemExit("PlayerArchiveTable has no Monster rows")
    creature_types = {}
    for row in rows(sources["CreatureTable"]):
        kind = enum_value(ARCHIVE_TYPES, row.get("ArchiveType"))
        if kind in archive_subs:
            creature_types[row["Index"]] = kind

    support = json.loads((TABLES / "BattleSupport.json").read_text(encoding="utf-8"))["Tables"]["CampaignDungeon"]
    dungeons = {(d["ChapterIndex"], d["DungeonIndex"]): d for d in support}

    # (chapter, dungeon) -> difficulty -> wave -> row, and scenario waves.
    by_difficulty: dict = defaultdict(lambda: defaultdict(dict))
    scenario: dict = defaultdict(dict)
    for row in rows(sources["CampaignWaveTable"]):
        key = (row["ChapterIndex"], row["DungeonIndex"])
        if row.get("Scenario"):
            scenario[key][row["WaveIndex"]] = row
        else:
            by_difficulty[key][enum_value(DIFFICULTIES, row.get("Difficulty"))][row["WaveIndex"]] = row

    def monsters(waves) -> list[list[int]]:
        found = set()
        for row in waves:
            for entry in (row.get("BossList") or []) + (row.get("CreatureList") or []):
                flags = {f.strip() for f in str(entry.get("_WaveFlag") or "None").split(",")}
                kind = creature_types.get(entry["_Index"])
                if kind is not None and not flags & SKIPPED_FLAGS:
                    found.add((kind, entry["_Index"]))
        return [list(pair) for pair in sorted(found)]

    def consecutive(table: dict, limit: int = 9999):
        wave = 1
        while wave <= limit and wave in table:
            yield table[wave]
            wave += 1

    out = {}
    for key in sorted(set(by_difficulty) | set(scenario)):
        dungeon = dungeons.get(key)
        if dungeon is not None and dungeon.get("BattleDefineCode") in TUTORIAL_CODES:
            continue
        entry = {}
        levels = by_difficulty.get(key, {})
        for difficulty in sorted(levels):
            found = monsters(consecutive(levels[difficulty]))
            if found:
                entry[str(difficulty)] = found
        if key in scenario:
            end = (dungeon or {}).get("ScenarioEndWave") or 9999
            fallback = levels.get(min(levels)) if levels else {}
            waves = []
            wave = 1
            while wave <= end and (wave in scenario[key] or wave in fallback):
                waves.append(scenario[key].get(wave) or fallback[wave])
                wave += 1
            found = monsters(waves)
            if found:
                entry["Scenario"] = found
        if entry:
            out[f"{key[0]}:{key[1]}"] = entry

    return {
        "Description": "Archive monsters per dungeon for the Monster Index: Dungeons[\"Chapter:Dungeon\"][Difficulty or \"Scenario\"] = [[ArchiveType, CreatureIndex], ...]. Generated by server/scripts/tables/build_monster_archive.py.",
        "Provenance": {
            "Source": "JitTables dumps of the client TableJit",
            "SourceHashes": {path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in sources.values()},
        },
        "ArchiveMainIndex": archive_main,
        "ArchiveTypes": sorted(archive_subs),
        "Dungeons": out,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--dumps", type=Path, required=True, help="Folder with the three JitTables dumps")
    parser.add_argument("--output", type=Path, default=OUTPUT)
    args = parser.parse_args()
    table = build(args.dumps)
    args.output.write_text(json.dumps(table, separators=(",", ":")) + "\n", encoding="utf-8")
    total = sum(len(v) for d in table["Dungeons"].values() for v in d.values())
    print(f"Wrote {len(table['Dungeons'])} dungeons ({total} monster entries) to {args.output}")


if __name__ == "__main__":
    main()
