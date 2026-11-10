#!/usr/bin/env python3
"""Preview/apply one fixed, inventory-based Divine Punishment compensation.

Run with the game service stopped, before enabling 4x rewards. By default only
the JSON report is written. --apply revalidates that report, takes a SQLite
backup, and atomically creates claimable mail plus a permanent campaign marker.
Python standard library only; keep the adjacent frozen rules JSON with this file.
"""
from __future__ import annotations

import argparse
from collections import defaultdict
from contextlib import closing
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import sqlite3
import sys
import uuid

CAMPAIGN = "divine-punishment-x4-20261110-v1"
RULES = Path(__file__).with_name("divine_punishment_compensation_rules.json")
INT32_MAX = 2**31 - 1
LIMITATIONS = [
    "Estimate from retained inventory, not a historical farming audit.",
    "Awakened artifacts count as 2^star base copies; failed awakening losses are not recovered.",
    "Artifact awakening stones and items obtained through gifts, shops or other sources may overestimate farmed materials.",
    "Rune blood costs use the retained runes in slot creation order across both raid families; dismantled runes and their historical effect on costs cannot be reconstructed.",
    "Only bag/chest/equipped items in items/equip_items are counted; unopened packages, unused selectors and unclaimed mail are excluded.",
    "Apply before 4x rewards go live; otherwise newly earned materials would also be compensated.",
]


def canonical(value):
    return json.dumps(value, sort_keys=True, separators=(",", ":"), ensure_ascii=True)


def digest(value):
    return hashlib.sha256(canonical(value).encode()).hexdigest()


def connect(path, writable=False):
    path = Path(path).resolve(strict=True)
    connection = sqlite3.connect(path.as_uri() + ("?mode=rw" if writable else "?mode=ro"), uri=True, timeout=30)
    connection.row_factory = sqlite3.Row
    connection.execute("PRAGMA foreign_keys=ON")
    if not writable:
        connection.execute("PRAGMA query_only=ON")
    return connection


def rules():
    value = json.loads(RULES.read_text(encoding="utf-8"))
    if value["Campaign"] != CAMPAIGN or value["Multiplier"] != 4:
        raise ValueError("Unexpected frozen compensation policy")
    if set(map(int, value["ArtifactFragments"])) != {151000, 151001, 151002, 151003}:
        raise ValueError("Unexpected artifact recipe set")
    return value


def already_applied(connection):
    exists = connection.execute("SELECT 1 FROM sqlite_master WHERE type='table' AND name='sprk_compensation_campaigns'").fetchone()
    if exists:
        row = connection.execute("SELECT applied_at,recipient_count FROM sprk_compensation_campaigns WHERE campaign=?", (CAMPAIGN,)).fetchone()
        if row:
            return dict(row)
    return None


def integer(value, label, low=0, high=INT32_MAX):
    if not isinstance(value, int) or not low <= value <= high:
        raise ValueError(f"Invalid {label}: {value!r}")
    return value


def plan(connection, policy):
    fragments = {int(k): v for k, v in policy["ArtifactFragments"].items()}
    rune_blood = {int(k): v for k, v in policy["RuneBlood"].items()}
    materials = set(fragments.values()) | set(rune_blood.values())
    accounts = [dict(r) for r in connection.execute("SELECT account_id,created_at FROM accounts ORDER BY account_id")]
    account_ids = {r["account_id"] for r in accounts}
    marks = ",".join("?" for _ in materials)
    stacks = [dict(r) for r in connection.execute(
        f"SELECT account_id,item_index,count FROM items WHERE item_index IN ({marks}) ORDER BY account_id,item_index", sorted(materials))]
    equipment_ids = sorted(set(fragments) | set(rune_blood))
    marks = ",".join("?" for _ in equipment_ids)
    equipment = [dict(r) for r in connection.execute(
        f"SELECT slot_index,account_id,item_index,star,inventory_type,upgrade_star_fail_bonus,"
        f"punishment_rune_option IS NOT NULL AS is_punishment_rune FROM equip_items "
        f"WHERE item_index IN ({marks}) OR punishment_rune_option IS NOT NULL ORDER BY slot_index", equipment_ids)]
    balances = defaultdict(dict)
    copies = defaultdict(lambda: defaultdict(int))
    blood_spent = defaultdict(lambda: defaultdict(int))
    rune_details = defaultdict(list)
    for row in stacks:
        if row["account_id"] not in account_ids:
            raise ValueError("Material stack references a missing account")
        balances[row["account_id"]][row["item_index"]] = integer(row["count"], "material stack")
    for row in equipment:
        account = row["account_id"]
        if account not in account_ids:
            raise ValueError("Equipment references a missing account")
        item = row["item_index"]
        if item in fragments:
            copies[account][item] += 2 ** integer(row["star"], "artifact star", high=5)
        elif item in rune_blood and row["is_punishment_rune"]:
            ordinal = len(rune_details[account])
            costs = policy["BloodCostByOwnedRuneCount"]
            if ordinal >= len(costs):
                raise ValueError(f"Account {account} has more runes than the frozen cost table supports")
            blood = rune_blood[item]
            cost = integer(costs[ordinal], "blood crafting cost", low=1)
            blood_spent[account][blood] += cost
            rune_details[account].append({"slot_index": row["slot_index"], "rune_index": item,
                                          "estimated_owned_before_craft": ordinal, "blood_index": blood,
                                          "estimated_blood_cost": cost})
        else:
            raise ValueError(f"Unrecognized or incomplete Divine Punishment rune in slot {row['slot_index']}")
    recipients = []
    totals = defaultdict(int)
    for account in sorted(set(balances) | set(copies) | set(blood_spent)):
        breakdown = []
        for material in sorted(materials):
            remaining = balances[account].get(material, 0)
            equivalent_copies = sum(n for artifact, n in copies[account].items() if fragments[artifact] == material)
            spent = equivalent_copies * policy["ArtifactFragmentCost"] + blood_spent[account].get(material, 0)
            estimated_total = remaining + spent
            grant = estimated_total * (policy["Multiplier"] - 1)
            if not grant:
                continue
            integer(grant, "compensation attachment", low=1)
            integer(remaining + grant, "resulting material balance")
            breakdown.append({"item_index": material, "remaining": remaining,
                              "artifact_base_copy_equivalents": equivalent_copies,
                              "estimated_spent": spent, "estimated_old_total": estimated_total,
                              "grant": grant})
            totals[material] += grant
        if breakdown:
            recipients.append({"account_id": account, "materials": breakdown,
                               "retained_runes": rune_details[account]})
    payload = {"schema_version": 1, "campaign": CAMPAIGN, "policy_sha256": digest(policy),
               "holdings_sha256": digest({"accounts": accounts, "stacks": stacks, "equipment": equipment}),
               "estimated": True, "limitations": LIMITATIONS, "accounts_examined": len(accounts),
               "recipient_count": len(recipients), "totals": {str(k): v for k, v in sorted(totals.items())},
               "recipients": recipients}
    payload["plan_sha256"] = digest(payload)
    return payload


def preview(database, report, policy):
    with closing(connect(database)) as connection, connection:
        connection.execute("BEGIN")
        done = already_applied(connection)
        if done:
            return {"status": "already_applied", "campaign": CAMPAIGN, **done}
        result = plan(connection, policy)
    # Exclusive creation prevents accidentally replacing a previously reviewed plan.
    with Path(report).open("x", encoding="utf-8") as writer:
        json.dump(result, writer, indent=2)
        writer.write("\n")
    return {"status": "dry_run", "report": str(report), "recipient_count": result["recipient_count"], "totals": result["totals"]}


def backup(database, destination):
    destination = Path(destination)
    if destination.resolve() == Path(database).resolve():
        raise ValueError("Backup path cannot be the game database")
    # Refuse to overwrite an existing file, including any earlier backup.
    with destination.open("xb"):
        pass
    with closing(connect(database)) as source, closing(sqlite3.connect(destination)) as target, target:
        source.backup(target)
        if target.execute("PRAGMA quick_check").fetchone()[0] != "ok":
            raise ValueError("Backup integrity check failed")


def apply(database, report, policy, backup_path=None):
    reviewed = json.loads(Path(report).read_text(encoding="utf-8"))
    connection = connect(database, writable=True)
    try:
        connection.execute("BEGIN IMMEDIATE")
        done = already_applied(connection)
        if done:
            connection.rollback()
            return {"status": "already_applied", "campaign": CAMPAIGN, **done}
        current = plan(connection, policy)
        if reviewed != current:
            raise ValueError("Report does not match current holdings/policy. No grants written. Generate a fresh report with the game stopped.")
        if backup_path is None:
            stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ")
            backup_path = Path(database).with_name(Path(database).name + f".before-{CAMPAIGN}-{stamp}-{uuid.uuid4().hex[:8]}.bak")
        # A separate read connection can back up while BEGIN IMMEDIATE prevents
        # other writers. No game/audit writes have occurred yet.
        backup(database, backup_path)
        connection.execute("CREATE TABLE IF NOT EXISTS sprk_compensation_campaigns (campaign TEXT PRIMARY KEY,applied_at TEXT NOT NULL,recipient_count INTEGER NOT NULL,plan_sha256 TEXT NOT NULL,backup_path TEXT NOT NULL,report_json TEXT NOT NULL)")
        connection.execute("CREATE TABLE IF NOT EXISTS sprk_compensation_grants (campaign TEXT NOT NULL,account_id INTEGER NOT NULL,mail_id INTEGER NOT NULL,attachments TEXT NOT NULL,PRIMARY KEY(campaign,account_id),FOREIGN KEY(campaign) REFERENCES sprk_compensation_campaigns(campaign))")
        applied_at = datetime.now(timezone.utc).isoformat()
        connection.execute("INSERT INTO sprk_compensation_campaigns VALUES(?,?,?,?,?,?)",
                           (CAMPAIGN, applied_at, current["recipient_count"], current["plan_sha256"], str(Path(backup_path).resolve()), canonical(current)))
        for recipient in current["recipients"]:
            attachments = canonical([{"ItemIndex": m["item_index"], "ItemCount": m["grant"]} for m in recipient["materials"]])
            mail = connection.execute(
                "INSERT INTO mails(account_id,sender,title,content,reward_items,expires_at) VALUES(?,?,?,?,?,NULL)",
                (recipient["account_id"], "SPRK", "Divine Punishment reward adjustment",
                 "One-time compensation for the 4x Divine Punishment crafting-material reward increase, based on your retained artifacts, runes and materials before the update.", attachments))
            connection.execute("INSERT INTO sprk_compensation_grants VALUES(?,?,?,?)",
                               (CAMPAIGN, recipient["account_id"], mail.lastrowid, attachments))
        connection.commit()
        return {"status": "applied", "campaign": CAMPAIGN, "recipient_count": current["recipient_count"],
                "backup": str(backup_path), "totals": current["totals"]}
    except BaseException:
        connection.rollback()
        raise
    finally:
        connection.close()


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--database", required=True, type=Path)
    parser.add_argument("--report", required=True, type=Path, help="New JSON report for preview; reviewed existing report for --apply")
    parser.add_argument("--apply", action="store_true", help="Back up the database and grant exactly this reviewed plan once")
    parser.add_argument("--backup", type=Path, help="Optional new backup filename; otherwise generated beside the database")
    args = parser.parse_args(argv)
    if args.backup and not args.apply:
        parser.error("--backup is only used with --apply")
    try:
        policy = rules()
        result = apply(args.database, args.report, policy, args.backup) if args.apply else preview(args.database, args.report, policy)
    except (OSError, ValueError, KeyError, sqlite3.Error) as error:
        parser.exit(1, f"Compensation stopped: {error}\n")
    print(json.dumps(result, indent=2))
    if result["status"] == "dry_run":
        print("ESTIMATE ONLY: review the report limitations. Keep the game stopped; apply before enabling 4x rewards.")
    if result["status"] == "already_applied":
        print("This fixed campaign has already been applied. No additional rewards were granted.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
