import concurrent.futures
from contextlib import closing
import json
from pathlib import Path
import shutil
import sqlite3
import subprocess
import sys
import tempfile
import unittest

from scripts.server import compensate_divine_punishment as compensation


class CompensationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.db = self.root / "sprk.db"
        self.report = self.root / "report.json"
        self.backup = self.root / "before.bak"
        self.policy = compensation.rules()
        with closing(sqlite3.connect(self.db)) as c, c:
            c.executescript("""
                CREATE TABLE accounts(account_id INTEGER PRIMARY KEY,created_at TEXT NOT NULL);
                CREATE TABLE items(account_id INTEGER,item_index INTEGER,count INTEGER,UNIQUE(account_id,item_index));
                CREATE TABLE equip_items(slot_index INTEGER PRIMARY KEY,account_id INTEGER,item_index INTEGER,
                    star INTEGER DEFAULT 0,inventory_type INTEGER DEFAULT 0,upgrade_star_fail_bonus INTEGER DEFAULT 0,punishment_rune_option TEXT);
                CREATE TABLE mails(mail_id INTEGER PRIMARY KEY,account_id INTEGER,sender TEXT,title TEXT,content TEXT,
                    reward_items TEXT,expires_at TEXT,is_received INTEGER DEFAULT 0,reward_gold INTEGER DEFAULT 0,reward_gem INTEGER DEFAULT 0);
                CREATE TABLE mail_notifications(mail_id INTEGER PRIMARY KEY,account_id INTEGER,is_global INTEGER);
                CREATE TRIGGER mail_arrival_notification AFTER INSERT ON mails BEGIN
                    INSERT INTO mail_notifications VALUES(NEW.mail_id,NEW.account_id,0); END;
                INSERT INTO accounts VALUES(1,'2026-11-01'),(2,'2026-11-02'),(3,'2026-11-03');
                INSERT INTO items VALUES(1,45122,250),(1,45121,300),(2,45128,10),(3,45131,0);
                INSERT INTO equip_items(slot_index,account_id,item_index,star,inventory_type) VALUES(1,1,151001,2,1),(2,1,151000,0,0);
            """)
            rune_by_blood = {blood: int(rune) for rune, blood in self.policy["RuneBlood"].items()}
            # First Solenis, then Apocalypsion: costs share one global ordinal.
            c.executemany("INSERT INTO equip_items(slot_index,account_id,item_index,inventory_type,punishment_rune_option) VALUES(?,2,?,3,'{}')",
                          [(3, rune_by_blood[45129]), (4, rune_by_blood[45128])])

    def preview(self):
        return compensation.preview(self.db, self.report, self.policy)

    def query(self, query, parameters=()):
        with closing(sqlite3.connect(self.db)) as c, c:
            return c.execute(query, parameters).fetchall()

    def test_frozen_rules_match_crafting_tables(self):
        tables = Path(__file__).resolve().parents[2] / "tables"
        extension = json.loads((tables / "ExtensionSupport.json").read_text(encoding="utf-8"))
        self.assertEqual(self.policy["RuneBlood"], {str(r["PunishmentRuneIndex"]): r["ApostleBloodIndex"] for r in extension["PunishmentRune"]})
        self.assertEqual(self.policy["BloodCostByOwnedRuneCount"],
                         [r["ApostleBloodCount"] for r in sorted(extension["PunishmentRuneItemCount"], key=lambda r: r["PunishmentRuneCount"])])
        inventory = json.loads((tables / "InventorySupport.json").read_text(encoding="utf-8"))
        for artifact, fragment in self.policy["ArtifactFragments"].items():
            recipe = next(r for r in inventory["Crafts"].values() if r["ItemIndex"] == int(artifact) and r["IsOpen"])
            self.assertEqual(recipe["Materials"], [{"ItemIndex": fragment, "Count": 1000}])

    def test_preview_is_read_only_and_counts_awakened_artifacts(self):
        before = self.db.read_bytes()
        result = self.preview()
        self.assertEqual(before, self.db.read_bytes())
        self.assertEqual(result["status"], "dry_run")
        report = json.loads(self.report.read_text())
        self.assertEqual(report["recipient_count"], 2)
        recipients = {r["account_id"]: r for r in report["recipients"]}
        materials = {m["item_index"]: m for m in recipients[1]["materials"]}
        self.assertEqual(materials[45122]["grant"], (4 * 1000 + 250) * 3)
        self.assertEqual(materials[45122]["artifact_base_copy_equivalents"], 4)
        self.assertEqual(materials[45121]["grant"], (1000 + 300) * 3)
        self.assertNotIn("sprk_compensation_campaigns", [r[0] for r in self.query("SELECT name FROM sqlite_master")])

    def test_blood_costs_share_ordinal_across_raid_families(self):
        self.preview()
        report = json.loads(self.report.read_text())
        recipient = next(r for r in report["recipients"] if r["account_id"] == 2)
        grants = {m["item_index"]: m["grant"] for m in recipient["materials"]}
        self.assertEqual(grants, {45129: 150 * 3, 45128: (152 + 10) * 3})
        self.assertEqual([r["estimated_owned_before_craft"] for r in recipient["retained_runes"]], [0, 1])

    def test_apply_mails_once_with_backup_and_notifications(self):
        self.preview()
        items_before = self.query("SELECT * FROM items")
        equipment_before = self.query("SELECT * FROM equip_items")
        result = compensation.apply(self.db, self.report, self.policy, self.backup)
        self.assertEqual(result["status"], "applied")
        self.assertEqual(self.query("SELECT * FROM items"), items_before)
        self.assertEqual(self.query("SELECT * FROM equip_items"), equipment_before)
        self.assertEqual(self.query("SELECT COUNT(*) FROM mails"), [(2,)])
        self.assertEqual(self.query("SELECT COUNT(*) FROM mail_notifications"), [(2,)])
        for raw, expires in self.query("SELECT reward_items,expires_at FROM mails"):
            self.assertIsNone(expires)
            for item in json.loads(raw):
                self.assertEqual(set(item), {"ItemIndex", "ItemCount"})
                self.assertGreater(item["ItemCount"], 0)
        with closing(sqlite3.connect(self.backup)) as c, c:
            self.assertEqual(c.execute("PRAGMA quick_check").fetchone(), ("ok",))
            self.assertEqual(c.execute("SELECT COUNT(*) FROM mails").fetchone(), (0,))
            self.assertFalse(c.execute("SELECT 1 FROM sqlite_master WHERE name='sprk_compensation_campaigns'").fetchone())
        # Even claiming/deleting the mail does not reset the campaign marker.
        with closing(sqlite3.connect(self.db)) as c, c:
            c.execute("DELETE FROM mails")
        self.assertEqual(compensation.apply(self.db, self.report, self.policy)["status"], "already_applied")
        self.assertEqual(self.query("SELECT COUNT(*) FROM mails"), [(0,)])

    def test_stale_or_edited_report_cannot_grant(self):
        self.preview()
        with closing(sqlite3.connect(self.db)) as c, c:
            c.execute("UPDATE items SET count=count+1 WHERE account_id=1 AND item_index=45122")
        with self.assertRaisesRegex(ValueError, "does not match"):
            compensation.apply(self.db, self.report, self.policy, self.backup)
        self.assertFalse(self.backup.exists())
        self.assertEqual(self.query("SELECT COUNT(*) FROM mails"), [(0,)])

    def test_edited_grant_is_rejected(self):
        self.preview()
        report = json.loads(self.report.read_text())
        report["recipients"][0]["materials"][0]["grant"] += 1
        self.report.write_text(json.dumps(report))
        with self.assertRaisesRegex(ValueError, "does not match"):
            compensation.apply(self.db, self.report, self.policy, self.backup)

    def test_partial_failure_rolls_back_all_mail_and_marker(self):
        self.preview()
        with closing(sqlite3.connect(self.db)) as c, c:
            c.execute("CREATE TRIGGER fail_second_recipient BEFORE INSERT ON mails WHEN NEW.account_id=2 BEGIN SELECT RAISE(ABORT,'test failure'); END")
        with self.assertRaises(sqlite3.IntegrityError):
            compensation.apply(self.db, self.report, self.policy, self.backup)
        self.assertTrue(self.backup.exists())
        self.assertEqual(self.query("SELECT COUNT(*) FROM mails"), [(0,)])
        self.assertEqual(self.query("SELECT COUNT(*) FROM mail_notifications"), [(0,)])
        self.assertFalse(self.query("SELECT 1 FROM sqlite_master WHERE name='sprk_compensation_campaigns'"))
        with closing(sqlite3.connect(self.db)) as c, c:
            c.execute("DROP TRIGGER fail_second_recipient")
        self.assertEqual(compensation.apply(self.db, self.report, self.policy)["status"], "applied")

    def test_concurrent_runs_cannot_double_grant(self):
        self.preview()
        with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
            results = list(pool.map(lambda _: compensation.apply(self.db, self.report, self.policy), range(2)))
        self.assertEqual(sorted(r["status"] for r in results), ["already_applied", "applied"])
        self.assertEqual(self.query("SELECT COUNT(*) FROM mails"), [(2,)])
        self.assertEqual(self.query("SELECT COUNT(*) FROM sprk_compensation_campaigns"), [(1,)])

    def test_existing_backup_is_never_overwritten(self):
        self.preview()
        self.backup.write_bytes(b"existing backup")
        with self.assertRaises(FileExistsError):
            compensation.apply(self.db, self.report, self.policy, self.backup)
        self.assertEqual(self.backup.read_bytes(), b"existing backup")
        self.assertEqual(self.query("SELECT COUNT(*) FROM mails"), [(0,)])

    def test_committed_wal_materials_are_in_report_and_backup(self):
        with closing(sqlite3.connect(self.db)) as writer:
            writer.execute("PRAGMA journal_mode=WAL")
            writer.execute("UPDATE items SET count=275 WHERE account_id=1 AND item_index=45122")
            writer.commit()
            self.assertTrue(Path(str(self.db) + "-wal").exists())
            self.preview()
            compensation.apply(self.db, self.report, self.policy, self.backup)
            with closing(sqlite3.connect(self.backup)) as saved:
                self.assertEqual(saved.execute("SELECT count FROM items WHERE account_id=1 AND item_index=45122").fetchone(), (275,))
            mail = json.loads(self.query("SELECT reward_items FROM mails WHERE account_id=1")[0][0])
            self.assertEqual(next(i["ItemCount"] for i in mail if i["ItemIndex"] == 45122), 3 * (4000 + 275))

    def test_two_file_cli_runs_outside_repository_and_is_once_only(self):
        tools = self.root / "tools"
        tools.mkdir()
        script = tools / Path(compensation.__file__).name
        shutil.copy2(compensation.__file__, script)
        shutil.copy2(compensation.RULES, tools / compensation.RULES.name)
        command = [sys.executable, str(script), "--database", str(self.db), "--report", str(self.report)]
        def run(*extra):
            return subprocess.run(command + list(extra), cwd=self.root, check=True, capture_output=True, text=True, timeout=20).stdout
        self.assertIn('"status": "dry_run"', run())
        self.assertIn('"status": "applied"', run("--apply"))
        self.assertIn('"status": "already_applied"', run("--apply"))
        self.assertEqual(self.query("SELECT COUNT(*) FROM mails"), [(2,)])

    def test_invalid_stars_unknown_runes_and_overflow_are_rejected(self):
        for statement in ["UPDATE equip_items SET star=6 WHERE slot_index=1",
                          "UPDATE equip_items SET item_index=999999 WHERE slot_index=3",
                          "UPDATE items SET count=1000000000 WHERE account_id=1 AND item_index=45122"]:
            with closing(sqlite3.connect(self.db)) as c, c:
                c.row_factory = sqlite3.Row
                c.execute("SAVEPOINT test")
                c.execute(statement)
                with self.assertRaises(ValueError):
                    compensation.plan(c, self.policy)
                c.execute("ROLLBACK TO test")


if __name__ == "__main__":
    unittest.main()
