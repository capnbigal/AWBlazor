# Deployment command glossary

Quick reference for the commands used most often on the DigitalOcean droplet and in the local dev loop. For narrative runbooks see [`first-deploy.md`](./first-deploy.md) and [`rollback.md`](./rollback.md).

---

## SSH & shell basics

```bash
ssh user@droplet-ip                     # connect to droplet
Ctrl+C                                  # exit `docker compose logs -f` tail
Ctrl+\                                  # hard-interrupt (SIGQUIT) if Ctrl+C hangs
```

## Droplet git (pull updates)

```bash
cd /opt/awblazor
git pull                                # grab latest main
```

## Docker Compose — app lifecycle

The droplet **pulls** the app image from GitHub Container Registry — it does NOT build
locally. Builds happen in CI (`.github/workflows/build-and-push-image.yml`) on every
push to `main` and publish two tags: `latest` and `<short-sha>`.

```bash
docker compose pull app                 # fetch newest image (latest tag)
docker compose up -d app                # (re)start app with the pulled image
docker compose up -d sqlserver          # start just the SQL Server container
docker compose up -d                    # start everything
docker compose down                     # stop everything
docker compose ps                       # list containers + health
docker compose logs -f app              # stream app logs (Ctrl+C to exit)
docker compose logs app --since 24h     # app logs from last 24h, non-streaming
docker compose logs app --tail 20       # last 20 lines
docker compose up -d --force-recreate app   # rotate container (picks up new .env / new image)
```

### Standard deploy after a PR merges to main

```bash
cd /opt/awblazor
docker compose pull app
docker compose up -d --force-recreate app
docker compose logs -f app              # watch startup; Ctrl+C when "Now listening on" appears
```

### Pin to a specific image SHA (for rollback)

```bash
APP_TAG=a1b2c3d docker compose up -d --force-recreate app   # immutable per-commit tag
APP_TAG=latest  docker compose up -d --force-recreate app   # back to newest
```

### Local build (dev only — droplet never does this)

```bash
docker compose -f docker-compose.yml -f docker-compose.build.yml build app
docker compose -f docker-compose.yml -f docker-compose.build.yml up -d app
```

## Docker — peek inside containers

```bash
docker compose exec app printenv                       # list all env vars
docker compose exec app printenv | grep -i demo        # filter to Demo_* vars
docker compose exec sqlserver bash                     # shell into SQL container
docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$(. .env && echo $SA_PASSWORD)" -C \
    -Q "SELECT COUNT(*) FROM AdventureWorks2022.Production.Product"
```

## Health / smoke-test after deploy

```bash
curl -I http://127.0.0.1:8080/                         # expect 200 or 302
curl  http://127.0.0.1:8080/healthz                    # expect {"status":"Healthy"}
curl -I https://alibalib.com/                          # external: 200 + HSTS
# Browser: Ctrl+F5 on https://alibalib.com to bypass CSS cache
```

## Demo-mode autofill toggle

When `DEMO_AUTOFILL_LOGIN=true`, `/Account/Login` prefills `admin@email.com` / `p@55wOrd`. Never enable in real prod.

```bash
cd /opt/awblazor

# Check current state
grep -i 'demo' .env docker-compose.yml
docker compose exec app printenv | grep -i demo

# Turn ON
echo 'DEMO_AUTOFILL_LOGIN=true' >> .env
docker compose up -d app

# Turn OFF
sed -i 's/^DEMO_AUTOFILL_LOGIN=true/DEMO_AUTOFILL_LOGIN=false/' .env
docker compose up -d app
```

## AdventureWorks date-shift toggle

When `DEMO_SHIFT_DATES=true`, the app slides every date column in Sales / Purchasing / Production / HumanResources / Person forward on startup (and exposes a "Shift dates to today" button on `/admin`) so the 2011-2014 sample data stays current. Uniform shift preserves every relative gap (lead times, hire-age spreads, price-history windows). Idempotent — a re-run while already within a week of today no-ops. Never enable in real prod.

```bash
cd /opt/awblazor

# Turn ON
echo 'DEMO_SHIFT_DATES=true' >> .env
docker compose up -d app

# Turn OFF
sed -i 's/^DEMO_SHIFT_DATES=true/DEMO_SHIFT_DATES=false/' .env
docker compose up -d app
```

## Nginx

```bash
sudo nginx -t                           # syntax check before reload
sudo systemctl reload nginx             # apply config changes
sudo nginx -T                           # dump full effective config
sudo ls /etc/nginx/sites-enabled/       # list active site configs
```

## Firewall (ufw)

```bash
sudo ufw status verbose                 # see current rules
sudo ufw allow OpenSSH                  # keep SSH open
sudo ufw allow 'Nginx Full'             # 80 + 443
sudo ufw deny 1433/tcp                  # block SQL Server from internet
sudo ufw --force enable
```

## SQL backup / restore

```bash
./deployment/backup-sqlserver.sh                                   # manual bak
./deployment/restore-sqlserver.sh /backups/aw-YYYYMMDD-HHMMSS.bak  # restore
ls -lh /opt/awblazor/backups/                                      # list baks
```

## Let's Encrypt

```bash
sudo certbot renew --dry-run            # verify auto-renewal config
```

## Local git workflow

Claude drives these; listed here as reference. See `feedback_branching_workflow.md` in Claude memory for the "always branch, never commit to main" rule.

```bash
git checkout -b feat/<short-name>       # new branch off main
git add <files> && git commit -m "..."  # commit on branch
git push -u origin feat/<short-name>    # push branch
gh pr create --title "..." --body "..." # open PR against main
gh pr merge <number> --squash           # merge after review (user triggers this)
```

---

## Typical update flow

One-liner covering git pull → build → deploy → tail logs:

```bash
cd /opt/awblazor && git pull && docker compose build app && docker compose up -d app && docker compose logs -f app
```

Ctrl+C out of the log tail once you see `Application started`, then hard-refresh https://alibalib.com (Ctrl+F5) to bust the browser CSS cache.
