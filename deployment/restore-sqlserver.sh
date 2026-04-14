#!/bin/bash
# /opt/awblazor/deployment/restore-sqlserver.sh
#
# One-shot restore of a .bak file into the running SQL Server container.
# Usage:
#   ./restore-sqlserver.sh /backups/AdventureWorks2022.bak
#
# The .bak file must be readable INSIDE the sqlserver container (i.e. placed
# in /opt/awblazor/backups on the host, which is bind-mounted to /backups).

set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 /backups/<file>.bak" >&2
    exit 1
fi

BAK_PATH="$1"

# Load env
if [[ -f /opt/awblazor/.env ]]; then
    # shellcheck disable=SC1091
    set -a && source /opt/awblazor/.env && set +a
fi

if [[ -z "${SA_PASSWORD:-}" ]]; then
    echo "ERROR: SA_PASSWORD not set. Aborting." >&2
    exit 1
fi

cd /opt/awblazor

echo "Restoring ${BAK_PATH} into AdventureWorks2022..."

# Stop the app first so no active connections block the restore.
docker compose stop app || true

docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "IF DB_ID('AdventureWorks2022') IS NOT NULL
        BEGIN
            ALTER DATABASE [AdventureWorks2022] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        END
        RESTORE DATABASE [AdventureWorks2022]
            FROM DISK = '${BAK_PATH}'
            WITH REPLACE,
                 MOVE 'AdventureWorks2022'     TO '/var/opt/mssql/data/AdventureWorks2022.mdf',
                 MOVE 'AdventureWorks2022_log' TO '/var/opt/mssql/data/AdventureWorks2022_log.ldf';
        ALTER DATABASE [AdventureWorks2022] SET MULTI_USER;"

echo "Restore complete. Starting app..."
docker compose up -d app
docker compose logs --tail 50 -f app
