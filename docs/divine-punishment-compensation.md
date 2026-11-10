# One-time Divine Punishment compensation

Run this before enabling the 4x reward update, with the game service stopped.
It estimates previously earned materials from retained artifacts/runes and
remaining inventory. It is not an exact historical farming refund.

Copy these two files together if running outside the repository:

- `scripts/server/compensate_divine_punishment.py`
- `scripts/server/divine_punishment_compensation_rules.json`

Python 3.10+ is sufficient; there are no pip dependencies. No server rebuild or
client update is needed for this script. The current production game image does
not contain Python; run it on the Docker host against the real database volume,
or use the temporary Python container example below.

## Compensation calculation

For each of the four artifacts, the extra fragment grant is:

```text
3 × (remaining corresponding fragments + 1,000 × artifact base-copy equivalents)
```

Each retained artifact contributes `2^star` base copies, including equipment in
bags/chests and on heroes. For example, two 0-star artifacts and 300 remaining
fragments produce `3 × (2,000 + 300) = 6,900` extra fragments. Subtract the entire
old equivalent value from its quadrupled value, rather than subtracting only
the loose fragments; otherwise already-crafted items are reimbursed too much.

For each raid's blood, the extra grant is:

```text
3 × (remaining blood + estimated blood spent on retained Divine Punishment runes)
```

Rune costs depend on the total number of Divine Punishment runes owned across
both raids. The script orders retained runes by their creation slot and assigns
the frozen cost tiers globally: 150 for the first, 152 for the second, 153 for
the third, etc. Each cost is returned in the blood type used by that rune. The
report includes those per-rune estimates.

Only the four artifact fragment types and two blood types are granted. Ordinary
Rune Fragments, Void Runes, Legendary Runes, shard flasks, gold and rubies are
not part of this compensation, because their rewards were not quadrupled.

Current holdings cannot reveal dismantled/sold artifacts or runes, materials
lost on failed awakenings, or earlier rune counts before dismantling. Awakening
stones, gifted gear and other acquisition sources may overestimate farming.
Unopened packages, unused selectors and unclaimed mail are excluded. These
limitations are also included in every preview report.

## Preview, then apply

Stop the public game service first and keep it stopped through both commands.
Use the actual persistent `sprk.db`, not an empty file or a database downloaded
without its current WAL state. The script opens an existing database only;
SQLite's backup API includes committed WAL changes.

From the repository root, replace the example database/report paths:

```bash
python3 scripts/server/compensate_divine_punishment.py \
  --database /path/to/sprk.db --report /path/to/divine-compensation.json
```

This writes only a new JSON report; the database is opened read-only. Review
`recipient_count`, `totals`, and the per-account `materials` and `retained_runes`.
Existing report files are never overwritten. If holdings change, generate a new
report under a new filename before attempting to apply.

Apply that exact report:

```bash
python3 scripts/server/compensate_divine_punishment.py \
  --database /path/to/sprk.db --report /path/to/divine-compensation.json --apply
```

The apply operation:

1. Takes an exclusive writer reservation and checks the permanent campaign marker.
2. Recalculates the plan and rejects changed holdings, a different database, an
   edited report, or changed calculation rules.
3. Creates and integrity-checks a uniquely named SQLite backup beside the database.
   `--backup /path/to/new-backup.bak` can choose a different new backup file.
4. Creates one non-expiring, claimable compensation mail per eligible account.
   Existing items/equipment are not modified. Normal mail claiming grants the items.
5. Commits every mail and its audit receipts in one transaction.

Failures roll back all grants and the campaign marker. A complete pre-change
backup remains available. A successful retry reports `already_applied` and
creates no new mail, regardless of whether the original mail has been claimed
or deleted. Concurrent invocations cannot double-grant.

The fixed campaign identifier is `divine-punishment-x4-20261110-v1`; it cannot be
changed through a command option. Audit tables are `sprk_compensation_campaigns`
and `sprk_compensation_grants`. Preserve them. Restoring a database backup from
before compensation naturally also restores the state before these receipts.

After successful compensation, deploy the matching 4x reward update and restart
the game service. Players will find their compensation in their normal inbox.

## Docker/Portainer deployment

The repository Compose service is named `game` and uses a named `game-data`
volume, usually with a Compose-project prefix. In Portainer, stop the game
container and find its actual `/data` volume name under **Mounts**. Leave the
updates service running if desired. Do not assume a volume name: Docker may
create a new empty volume if the name is wrong.

On a Linux Docker host, with this repository available locally, replace
`ACTUAL_GAME_DATA_VOLUME` below with that existing volume name:

```bash
docker run --rm --user 10001:10001 \
  --mount type=volume,src=ACTUAL_GAME_DATA_VOLUME,dst=/data \
  --mount type=bind,src="$PWD/scripts/server",dst=/tools,readonly \
  python:3.12-slim-bookworm python /tools/compensate_divine_punishment.py \
  --database /data/sprk.db --report /data/divine-compensation.json
```

Review the report on that volume, then rerun the same command with `--apply`
appended. The report, backup, mails and permanent marker stay in the persistent
game volume when the temporary Python container exits. Do not run the script
against the update-service volume.

## Validation

```bash
python -m unittest scripts.server.test_divine_punishment_compensation -v
```

Tests use fresh temporary databases, not the installed or public game database.
