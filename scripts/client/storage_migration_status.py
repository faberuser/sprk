"""Read-only adoption report for the one-time SPRK storage migration."""
import argparse
import json
from pathlib import Path
import sqlite3
from contextlib import closing


def status(database, days=30):
    database=Path(database).resolve(strict=True)
    with closing(sqlite3.connect(database.as_uri()+"?mode=ro",uri=True)) as db:
        db.execute("PRAGMA query_only=ON")
        db.execute("BEGIN")
        if not db.execute("SELECT 1 FROM sqlite_master WHERE type='table' AND name='client_storage_reports'").fetchone():
            raise ValueError("Deploy the storage-status server update first; no adoption table exists.")
        active=db.execute("SELECT COUNT(*) FROM accounts WHERE last_login>=datetime('now',?)",(f"-{days} days",)).fetchone()[0]
        migrated=db.execute("SELECT COUNT(*) FROM accounts a WHERE a.last_login>=datetime('now',?) AND EXISTS(SELECT 1 FROM client_storage_reports r WHERE r.account_id=a.account_id AND r.schema_version>=1)",(f"-{days} days",)).fetchone()[0]
        missing=db.execute("SELECT a.account_id,a.last_login FROM accounts a WHERE a.last_login>=datetime('now',?) AND NOT EXISTS(SELECT 1 FROM client_storage_reports r WHERE r.account_id=a.account_id AND r.schema_version>=1) ORDER BY a.last_login DESC",(f"-{days} days",)).fetchall()
        profiles=db.execute("SELECT COUNT(*),COUNT(DISTINCT installation),MAX(last_seen) FROM client_storage_reports WHERE schema_version>=1").fetchone()
        return dict(active_days=days,active_accounts=active,confirmed_active_accounts=migrated,missing_active_accounts=len(missing),account_profile_reports=profiles[0],observed_profiles=profiles[1],latest_report_utc=profiles[2],missing=[dict(account_id=a,last_login_utc=t) for a,t in missing])


def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument("--database",type=Path,default=Path("sprk.db"));p.add_argument("--active-days",type=int,default=30);p.add_argument("--json",action="store_true");p.add_argument("--missing",action="store_true");a=p.parse_args()
    if not 1<=a.active_days<=3650:p.error("--active-days must be between 1 and 3650")
    report=status(a.database,a.active_days)
    if not a.missing:report.pop("missing")
    if a.json:print(json.dumps(report,indent=2))
    else:
        print(f"Active accounts ({a.active_days} days): {report['active_accounts']}")
        print(f"Confirmed schema 1 accounts: {report['confirmed_active_accounts']}")
        print(f"Active accounts without a receipt: {report['missing_active_accounts']}")
        print(f"Observed profiles: {report['observed_profiles']} (account/profile reports: {report['account_profile_reports']})")
        print(f"Latest report UTC: {report['latest_report_utc']}")
        if a.missing:
            for entry in report['missing']:print(f"Missing account {entry['account_id']}, last login UTC {entry['last_login_utc']}")
        print("Receipts show successful launches, not all offline devices. See docs/client-storage-migration.md before retiring migration.")


if __name__=="__main__":main()
