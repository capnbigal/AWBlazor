# First Deployment — AWBlazorApp on DigitalOcean

Step-by-step runbook for the initial cutover from the current template site to AWBlazorApp at `https://alibalib.com`.

**Expected duration:** 90-120 minutes including verification.
**Downtime window:** ~5-10 minutes (between Nginx cutover and app health check).

---

## Prerequisites

- SSH access to the droplet
- `sqlcmd` or SSMS on the local Windows / ELITE machine
- DO account control-panel access (for snapshot)
- Local Docker installed (for pre-flight image build test)
- A strong SA password ready (min 8 chars, mix of upper/lower/digit/symbol)

---

## Part 1 — Local prep (do this first)

### 1. Back up the local SQL Server database

On ELITE (via SSMS, sqlcmd, or a `.sql` script):

```sql
BACKUP DATABASE [AdventureWorks2022]
    TO DISK = 'C:\Temp\AdventureWorks2022.bak'
    WITH FORMAT, INIT, COMPRESSION,
         NAME = 'AWBlazor production migration bak';
```

Verify: `C:\Temp\AdventureWorks2022.bak` exists, ≥ 200 MB.

### 2. Pre-flight: build + smoke-test the Docker image locally

```bash
cd C:\Users\capnb\source\repos\AWBlazor
docker build -t awblazor:local .
# If the build succeeds, quickly test the image has the expected entrypoint:
docker run --rm awblazor:local --help 2>&1 | head -5 || true
```

**Don't** run the full stack locally unless you have Docker Desktop + enough disk for the SQL Server image (~2 GB). It's optional.

---

## Part 2 — Back up the existing droplet

### 3. DO snapshot (takes ~5 min)

Via DigitalOcean control panel: **Droplet → Snapshots → Take Snapshot**. Name it `pre-awblazor-$(date)`.

### 4. Tarball current site + Nginx + Let's Encrypt

SSH to droplet:
```bash
sudo tar czf ~/template-site-backup-$(date +%Y%m%d).tar.gz \
    /var/www \
    /etc/nginx \
    /etc/letsencrypt 2>/dev/null || true

sudo chown $USER:$USER ~/template-site-backup-*.tar.gz
```

Download to local:
```bash
# From local machine:
scp user@droplet-ip:~/template-site-backup-*.tar.gz .
```

### 5. Document current Nginx config

```bash
sudo nginx -T > ~/current-nginx-config.txt 2>/dev/null
# Save it locally too:
scp user@droplet-ip:~/current-nginx-config.txt .
```

Open it and find the `server_name alibalib.com` block — note the filename it's in. You'll disable it in Part 4.

---

## Part 3 — Deploy the new stack

### 6. Install Docker (if not already installed)

On the droplet:
```bash
docker --version 2>/dev/null || curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER
# Log out and back in for the group change, then verify:
docker compose version
```

### 7. Set up the deployment directory

```bash
sudo mkdir -p /opt/awblazor/backups
sudo chown -R $USER:$USER /opt/awblazor
cd /opt/awblazor
```

### 8. Transfer artifacts from local

From local:
```bash
# From your AWBlazor repo root:
scp docker-compose.yml         user@droplet-ip:/opt/awblazor/
scp Dockerfile                 user@droplet-ip:/opt/awblazor/
scp .dockerignore              user@droplet-ip:/opt/awblazor/
scp -r deployment/             user@droplet-ip:/opt/awblazor/

# Transfer the bak (may take a few minutes for 200 MB)
scp C:/Temp/AdventureWorks2022.bak user@droplet-ip:/opt/awblazor/backups/

# Transfer the app source (for building the image on-droplet).
# Alternative: build locally and push to GHCR, then `docker pull` on droplet.
rsync -av --delete \
    --exclude='.git' --exclude='bin/' --exclude='obj/' --exclude='.vs/' \
    src AWBlazorApp.slnx \
    user@droplet-ip:/opt/awblazor/
```

### 9. Create the .env file on the droplet

```bash
cd /opt/awblazor
cp deployment/.env.template .env
chmod 600 .env
$EDITOR .env     # fill in SA_PASSWORD and any SMTP/OAuth values
```

### 10. Start SQL Server

```bash
cd /opt/awblazor
docker compose up -d sqlserver
docker compose logs -f sqlserver
# Wait for "SQL Server is now ready for client connections" (~20-30 s)
# Ctrl+C out of logs
```

### 11. Restore the .bak

```bash
./deployment/restore-sqlserver.sh /backups/AdventureWorks2022.bak
```

The script stops the app (no-op on first deploy), restores the DB with `WITH REPLACE`, then attempts to start the app. If the app image isn't built yet, the restart will fail — that's expected; continue to step 12.

Quick verify:
```bash
docker compose exec sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$(. .env && echo $SA_PASSWORD)" -C \
    -Q "SELECT COUNT(*) FROM AdventureWorks2022.Production.Product"
# Should return ~504
```

### 12. Build the app image on the droplet

```bash
cd /opt/awblazor
docker compose build app
# ~3-5 min first time (pulls SDK image, restores NuGet, publishes)
```

### 13. Start the app

```bash
docker compose up -d app
docker compose logs -f app
```

Watch for:
- `Applying EF Core SQL Server migrations...` (no errors; migrations already applied from the .bak)
- `DatabaseInitializer: seeded 4 users` (roles + users exist; skips re-seeding)
- `Now listening on: http://[::]:8080`
- `Application started`

If you see schema errors, **stop here** — don't cut over Nginx. See `rollback.md`.

### 14. Internal smoke test (before Nginx cutover)

Still on the droplet:
```bash
curl -I http://127.0.0.1:8080/
# Expect: 200 OK or 302 (redirect to /Account/Login)

curl http://127.0.0.1:8080/healthz
# Expect: {"status":"Healthy"}
```

---

## Part 4 — Nginx cutover

### 15. Install the new Nginx config

```bash
sudo cp /opt/awblazor/deployment/nginx-awblazor.conf /etc/nginx/sites-available/awblazor.conf
sudo ln -s /etc/nginx/sites-available/awblazor.conf /etc/nginx/sites-enabled/awblazor.conf
```

### 16. Disable the template site (the risky step)

Find the old config from step 5 above. Typical names: `default`, `alibalib.com`, `template`.

```bash
sudo ls /etc/nginx/sites-enabled/
# Identify the OLD template config, e.g. "default"
sudo rm /etc/nginx/sites-enabled/default     # or whatever the filename is
```

### 17. Test + reload Nginx

```bash
sudo nginx -t
# Must say "syntax is ok" AND "test is successful". If not, STOP.

sudo systemctl reload nginx
```

### 18. External verification

From your local browser:
- https://alibalib.com → AWBlazor login page
- Log in as `admin@email.com` / `p@55wOrd`
- Navigate to Tool Slots, Forecasts, Analytics — pages load
- Log out, log back in — session works

From local terminal:
```bash
curl -I https://alibalib.com/
# Expect 200 OK + HSTS + security headers

curl https://alibalib.com/healthz
# Expect: {"status":"Healthy"}
```

---

## Part 5 — Housekeeping

### 19. Lock down the firewall

```bash
sudo ufw status
sudo ufw allow OpenSSH
sudo ufw allow 'Nginx Full'
sudo ufw deny 1433/tcp          # SQL Server is for localhost only
sudo ufw --force enable
sudo ufw status verbose
```

### 20. Verify Let's Encrypt auto-renewal

```bash
sudo certbot renew --dry-run
# Must say "Congratulations, all renewals succeeded"
```

### 21. Install the nightly backup cron

```bash
chmod +x /opt/awblazor/deployment/backup-sqlserver.sh
sudo crontab -e
```

Add:
```
0 3 * * * /opt/awblazor/deployment/backup-sqlserver.sh >> /var/log/awblazor-backup.log 2>&1
```

Test immediately:
```bash
sudo /opt/awblazor/deployment/backup-sqlserver.sh
ls -lh /opt/awblazor/backups/   # Should show today's aw-*.bak
```

### 22. Change the admin password

Log in to the app as `admin@email.com` / `p@55wOrd` → Account → Manage → Change Password. Set a strong one.

### 23. Rotate SA_PASSWORD (recommended)

```bash
# Generate a new password, update .env
cd /opt/awblazor
$EDITOR .env

# Apply to SQL Server:
OLD_PASSWORD='<current>'
NEW_PASSWORD='<new>'
docker compose exec -T sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$OLD_PASSWORD" -C \
    -Q "ALTER LOGIN sa WITH PASSWORD = '$NEW_PASSWORD'"

# Restart the app so it picks up the new connection string
docker compose up -d --force-recreate app
```

---

## Part 6 — 24-hour watch

Set a reminder to check 24 hours later:
- `docker compose ps` — both containers `healthy` / `up`
- `docker compose logs app --since 24h | grep -i error` — no unexplained errors
- `/opt/awblazor/backups/` — last night's backup exists
- https://alibalib.com loads and login works

If all green, **delete the old template tarball and keep only the DO snapshot as rollback insurance** for 7 days.

---

## Done

- [x] Template site backed up (DO snapshot + tarball)
- [x] AWBlazor live at https://alibalib.com
- [x] Data migrated from ELITE → droplet
- [x] Nightly backup cron installed
- [x] Firewall hardened
- [x] Admin password rotated
- [x] SA password rotated
- [x] Let's Encrypt renewal verified

Next up — see `rollback.md` (just in case) and the phase-2 list in `README.md`.
