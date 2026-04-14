# Deployment — AWBlazorApp on DigitalOcean

Self-hosted deployment of AWBlazorApp to a single Linux droplet using Docker Compose. The stack is SQL Server 2022 + the .NET 10 Blazor app, fronted by the host's existing Nginx + Let's Encrypt.

## Files

| File | Purpose |
|---|---|
| [`first-deploy.md`](./first-deploy.md) | Step-by-step runbook for the initial cutover |
| [`rollback.md`](./rollback.md) | How to revert if deployment goes sideways |
| [`commands.md`](./commands.md) | Glossary of the droplet/docker/nginx commands used most often |
| [`nginx-awblazor.conf`](./nginx-awblazor.conf) | Nginx reverse-proxy config (installed on host) |
| [`backup-sqlserver.sh`](./backup-sqlserver.sh) | Cron-driven daily `.bak` backup script |
| [`restore-sqlserver.sh`](./restore-sqlserver.sh) | One-shot restore-from-bak utility |
| [`.env.template`](./.env.template) | Template for the production secrets file |
| `/Dockerfile` (repo root) | Multi-stage build for the .NET 10 app |
| `/docker-compose.yml` (repo root) | SQL Server + app stack |
| `/.dockerignore` (repo root) | Files excluded from the Docker build context |

## High-level layout on the droplet

```
/opt/awblazor/
├── docker-compose.yml       # copied from repo
├── Dockerfile               # copied from repo (only if building on-droplet)
├── .env                     # SECRET: created from .env.template; chmod 600
├── deployment/              # copied from repo
│   ├── nginx-awblazor.conf
│   ├── backup-sqlserver.sh
│   └── restore-sqlserver.sh
└── backups/                 # bind-mounted into sqlserver container at /backups
    ├── AdventureWorks2022.bak      # initial migration bak
    └── aw-YYYYMMDD-HHMMSS.bak      # nightly cron backups
```

Named Docker volumes (auto-managed by Docker, persist across container restarts):
- `awblazor-sqldata` → SQL Server data files
- `awblazor-appdata` → App_Data (DataProtection keys, etc.)

## Operational commands

```bash
# On the droplet, from /opt/awblazor:

# Start everything
docker compose up -d

# View logs
docker compose logs -f app
docker compose logs -f sqlserver

# Restart just the app (after pulling a new image)
docker compose pull app && docker compose up -d app

# Stop everything
docker compose down

# Manual backup
./deployment/backup-sqlserver.sh

# Restore from a specific .bak
./deployment/restore-sqlserver.sh /backups/aw-20260501-030000.bak

# Shell into the SQL Server container
docker compose exec sqlserver bash
# Then: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C
```

## Security checklist

- [ ] `.env` is `chmod 600` and owned by the deploy user
- [ ] SA_PASSWORD was rotated after initial deploy (template value not retained)
- [ ] `ufw` is enabled: `ufw allow OpenSSH`, `ufw allow 'Nginx Full'`, `ufw deny 1433`
- [ ] SQL Server container port 1433 is bound to `127.0.0.1` only in `docker-compose.yml`
- [ ] Let's Encrypt auto-renewal verified with `sudo certbot renew --dry-run`
- [ ] Nightly backup cron is installed and has successfully run at least once
- [ ] DO snapshot was taken BEFORE cutover and retained for at least 7 days
- [ ] Default seed user `admin@email.com` / `p@55wOrd` has had its password changed

## Phase 2 work (not included yet)

- GitHub Actions → GHCR → droplet pull workflow (CI/CD)
- External SMTP provider (SendGrid / Mailgun / SES)
- Monitoring stack (Uptime Kuma or similar)
- Off-host backup replication (S3 / Spaces)
