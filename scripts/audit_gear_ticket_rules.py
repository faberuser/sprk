"""Compare server gear-ticket rules with decoded Pre-Doomsday tables."""
import argparse
import json
from pathlib import Path


def load(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def audit(archive, tables):
    source = archive / "kingsraid-table-data/output/MessagePack"
    inventory = load(tables / "InventorySupport.json")
    codes = load(tables / "ItemCodeToIndex.json")
    details = {r["DetailIndex"]: r for r in load(source / "EquipItemDetailTable.json")}
    extension = load(tables / "ExtensionSupport.json")
    overrides = tables / "ExtensionRules.json"
    if overrides.exists():
        extension.update(load(overrides))
    equipment = {r["ItemIndex"]: r for r in extension["EquipItem"]}
    compared = {"selectors": 0, "equipment": 0, "duplicate_normal_stats_allowed": 0}
    errors = []

    def same(label, actual, expected):
        if actual != expected:
            errors.append({"field": label, "server": actual, "archive": expected})

    for row in load(source / "WeaponUniqueSelectItemTable.json"):
        index = codes[row["ItemCode"]]
        current = inventory["EquipmentSelectors"].get(str(index), {})
        for key in ("ItemIndices", "SelectOptionCount", "SelectUniqueOption", "Star", "Level"):
            same(f"ticket {index} {key}", current.get(key), row[key])
        compared["selectors"] += 1

    for row in load(source / "EquipItemTable.json"):
        code = row["EquipCode"]
        index = codes[code[0] if isinstance(code, list) else code]
        current = inventory["Equipment"].get(str(index), {})
        for key in ("OptionIndex", "OptionGroupIndex", "EnableDuplicationOption", "MinRuneCount", "MaxRuneCount",
                    "UniqueOptionIndex", "UniqueOptionGroupIndex"):
            same(f"equipment {index} {key}", current.get(key), row[key])
        same(f"equipment {index} OptionCount", current.get("OptionCount"), details[row["DetailIndex"]]["OptionCount"])
        for key in ("EnableDuplicationUniqueOption", "EquipType", "EquipGrade"):
            same(f"equipment {index} {key}", equipment.get(index, {}).get(key), row[key])
        compared["equipment"] += 1
        compared["duplicate_normal_stats_allowed"] += int(row["EnableDuplicationOption"])
    return {"archive": str(archive), "compared": compared, "differences": errors, "passed": not errors}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--tables", type=Path, default=Path(__file__).resolve().parents[1] / "tables")
    parser.add_argument("--report", type=Path)
    args = parser.parse_args()
    report = audit(args.archive, args.tables)
    payload = json.dumps(report, indent=2) + "\n"
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(payload, encoding="utf-8")
    print(payload, end="")
    raise SystemExit(0 if report["passed"] else 1)
