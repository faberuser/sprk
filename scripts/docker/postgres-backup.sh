#!/bin/sh
# Nightly logical backups of the game database (docker-compose.yml service
# postgres-backup). Writes /backups/sprk-<UTC time>.dump (pg_dump custom
# format) and keeps SPRK_DB_BACKUP_DAYS days. Restore with pg_restore; see
# docs/postgresql-migration.md, "Backups and restore".
set -eu
: "${PGHOST:?}" "${PGUSER:?}" "${PGPASSWORD:?}" "${PGDATABASE:?}"
days="${SPRK_DB_BACKUP_DAYS:-14}"
hour="${SPRK_DB_BACKUP_HOUR_UTC:-4}"

backup() {
    name="sprk-$(date -u +%Y%m%dT%H%M%SZ).dump"
    # Write to a temporary name so a failed dump never looks complete.
    if pg_dump --format=custom --no-owner --file="/backups/.$name.partial"; then
        mv "/backups/.$name.partial" "/backups/$name"
        echo "Backup written: $name ($(du -h "/backups/$name" | cut -f1))"
    else
        rm -f "/backups/.$name.partial"
        echo "Backup FAILED" >&2
    fi
    find /backups -name 'sprk-*.dump' -mtime +"$days" -print -delete
}

until pg_isready --quiet; do sleep 5; done
backup
while true; do
    # Sleep until the next backup hour (UTC).
    now=$(date -u +%s)
    next=$(date -u -d "$(date -u +%Y-%m-%d) ${hour}:00:00" +%s)
    [ "$next" -le "$now" ] && next=$((next + 86400))
    sleep $((next - now))
    backup
done
