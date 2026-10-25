import hashlib
from pathlib import Path
import sqlite3
from contextlib import closing
import tempfile
import unittest
from storage_migration_status import status

class StatusTests(unittest.TestCase):
    def test_counts_missing_and_profiles_without_modifying_database(self):
        with tempfile.TemporaryDirectory() as folder:
            path=Path(folder)/"audit.sqlite"
            with closing(sqlite3.connect(path)) as db:
                db.executescript("CREATE TABLE accounts(account_id INTEGER PRIMARY KEY,last_login TEXT);CREATE TABLE client_storage_reports(account_id INTEGER,installation TEXT,schema_version INTEGER,last_seen TEXT);INSERT INTO accounts VALUES(1,datetime('now')),(2,datetime('now')),(3,datetime('now','-60 days'));INSERT INTO client_storage_reports VALUES(1,'profile-a',1,datetime('now')),(1,'profile-b',1,datetime('now'));")
            before=hashlib.sha256(path.read_bytes()).digest()
            result=status(path,30)
            self.assertEqual((result['active_accounts'],result['confirmed_active_accounts'],result['missing_active_accounts'],result['observed_profiles']),(2,1,1,2))
            self.assertEqual([r['account_id'] for r in result['missing']],[2])
            self.assertEqual(status(path,90)['missing_active_accounts'],2)
            self.assertEqual(hashlib.sha256(path.read_bytes()).digest(),before)
    def test_missing_database_is_not_created(self):
        with tempfile.TemporaryDirectory() as folder:
            path=Path(folder)/"missing.sqlite"
            with self.assertRaises(FileNotFoundError):status(path)
            self.assertFalse(path.exists())
    def test_old_server_schema_is_not_changed(self):
        with tempfile.TemporaryDirectory() as folder:
            path=Path(folder)/"old.sqlite"
            with closing(sqlite3.connect(path)) as db:db.execute("CREATE TABLE accounts(account_id INTEGER,last_login TEXT)")
            before=path.read_bytes()
            with self.assertRaisesRegex(ValueError,"Deploy"):status(path)
            self.assertEqual(path.read_bytes(),before)

if __name__=='__main__':unittest.main()
