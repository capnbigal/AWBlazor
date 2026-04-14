#!/bin/bash
# /opt/awblazor/deployment/backup-sqlserver.sh
#
# Daily SQL Server .bak backup. Intended to run from cron:
#   0 3 * * * /opt/awblazor/deployment/backup-sqlserver.sh >> /var/log/awblazor-backup.log 2>&1
#
# Keeps 14 days of backups in ./backups. Older files are deleted.

set -euo pipefail

# Load env
if [[ -f /opt/awblazor/.env ]]; then
    # shellcheck disable=SC1091
    set -a && source /opt/awblazor/.env && set +a
fi

if [[ -z "${SA_PASSWORD:-}" ]]; then
    echo "[$(date -Is)] ERROR: SA_PASSWORD not set. Aborting." >&2
    exit 1
fi

cd /opt/awblazor

DATE=$(date +%Y%m%d-%H%M%S)
BACKUP_FILE="/backups/aw-${DATE}.bak"
RETENTION_DAYS=14

echo "[$(date -Is)] Starting SQL Server backup → $BACKUP_FILE"

docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "BACKUP DATABASE [AdventureWorks2022] TO DISK = '${BACKUP_FILE}' WITH COMPRESSION, INIT, FORMAT, NAME = 'aw-${DATE}'"

echo "[$(date -Is)] Backup complete."

# Prune old backups (host-side cleanup — backups dir is bind-mounted from ./backups)
PRUNED=$(find /opt/awblazor/backups -type f -name 'aw-*.bak' -mtime +${RETENTION_DAYS} -print -delete | wc -l)
if [[ "$PRUNED" -gt 0 ]]; then
    echo "[$(date -Is)] Pruned $PRUNED backup(s) older than ${RETENTION_DAYS} days."
fi
