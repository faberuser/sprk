#!/bin/sh
# Daily logical backups of the game database (docker-compose.yml service
# postgres-backup). Writes /backups/sprk-<UTC time>.dump (pg_dump custom
# format) at SPRK_DB_BACKUP_HOUR_UTC and keeps the newest SPRK_DB_BACKUP_KEEP
# dumps. With SPRK_DB_BACKUP_COPY set, also copies them to /backup-copy (an
# off-host directory such as a NAS), keeping SPRK_DB_BACKUP_COPY_KEEP there.
# Restore with pg_restore; see docs/postgresql-migration.md, "Backups and restore".
set -eu
: "${PGHOST:?}" "${PGUSER:?}" "${PGPASSWORD:?}" "${PGDATABASE:?}"
keep="${SPRK_DB_BACKUP_KEEP:-14}"
hour="${SPRK_DB_BACKUP_HOUR_UTC:-4}"
copy="${SPRK_DB_BACKUP_COPY:-}"
copy_keep="${SPRK_DB_BACKUP_COPY_KEEP:-$keep}"
for n in "$keep" "$copy_keep"; do
    case "$n" in ''|*[!0-9]*|0) echo "SPRK_DB_BACKUP_KEEP and SPRK_DB_BACKUP_COPY_KEEP must be positive numbers" >&2; exit 1 ;; esac
done

dumps() { ls -1 "$1" | grep -E '^sprk-[0-9]{8}T[0-9]{6}Z\.dump$' | sort -r; }

# Keep the newest $2 dumps in directory $1 (names sort by time).
prune() {
    dumps "$1" | tail -n +"$(($2 + 1))" | while read -r old; do
        rm -f "$1/$old" && echo "Removed old backup: $1/$old"
    done
}

# Copy every kept dump the copy directory lacks, so a NAS outage catches up
# on the next run. A failed copy keeps everything and only warns.
sync_copy() {
    [ -n "$copy" ] || return 0
    if ! touch /backup-copy/.sprk-write-test 2>/dev/null; then
        echo "Backup copy FAILED: $copy is not writable by UID $(id -u)" >&2
        return 0
    fi
    rm -f /backup-copy/.sprk-write-test
    for name in $(dumps /backups | head -n "$copy_keep"); do
        [ -e "/backup-copy/$name" ] && continue
        if cp "/backups/$name" "/backup-copy/.$name.partial" && mv "/backup-copy/.$name.partial" "/backup-copy/$name"; then
            echo "Copied to $copy: $name"
        else
            rm -f "/backup-copy/.$name.partial"
            echo "Backup copy FAILED: $name to $copy" >&2
            return 0
        fi
    done
    prune /backup-copy "$copy_keep"
}

backup() {
    name="sprk-$(date -u +%Y%m%dT%H%M%SZ).dump"
    # Write to a temporary name so a failed dump never looks complete.
    if pg_dump --format=custom --no-owner --file="/backups/.$name.partial"; then
        mv "/backups/.$name.partial" "/backups/$name"
        echo "Backup written: $name ($(du -h "/backups/$name" | cut -f1))"
        # Only after a good dump, so a failing backup never deletes good ones.
        prune /backups "$keep"
    else
        rm -f "/backups/.$name.partial"
        echo "Backup FAILED; older backups kept" >&2
    fi
    sync_copy
}

until pg_isready --quiet; do sleep 5; done
# Back up at start only when the newest dump is a day old (or missing), so
# redeploys do not push older daily copies out. Always catch up the copy.
if [ -z "$(find /backups -maxdepth 1 -name 'sprk-*.dump' -mmin -1440 2>/dev/null)" ]; then
    backup
else
    sync_copy
fi
while true; do
    # Sleep until the next backup hour (UTC).
    now=$(date -u +%s)
    next=$(date -u -d "$(date -u +%Y-%m-%d) ${hour}:00:00" +%s)
    [ "$next" -le "$now" ] && next=$((next + 86400))
    sleep $((next - now))
    backup
done
