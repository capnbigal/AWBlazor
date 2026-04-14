# Rollback procedures

Three layers of rollback, from fastest to most thorough.

## Layer 1 — Container-level (minutes)

**Use when:** App container is crash-looping, throwing unhandled exceptions, or degraded performance after a deploy.

```bash
cd /opt/awblazor

# Option A — restart both containers
docker compose down
docker compose up -d

# Option B — revert to a previous image tag
docker compose pull app:v1.2.3    # replace with your tag
docker compose up -d app

# Option C — full rebuild from source
docker compose build --no-cache app
docker compose up -d app
```

**Does NOT recover from:** DB schema corruption, Nginx misconfiguration, host-level breakage.

---

## Layer 2 — Database restore (minutes)

**Use when:** Data was corrupted, wrong data was loaded, or someone accidentally deleted something important.

```bash
cd /opt/awblazor

# Stop the app
docker compose stop app

# List available backups
ls -lt backups/aw-*.bak

# Restore the most recent good backup
./deployment/restore-sqlserver.sh /backups/aw-YYYYMMDD-HHMMSS.bak
# (script restarts the app automatically)
```

**Does NOT recover from:** Data that was never backed up (anything written after the last nightly backup).

---

## Layer 3 — Full droplet rollback via DO snapshot (10-15 min)

**Use when:** Nothing else works — the droplet is in an unrecoverable state, or the AWBlazor cutover was fundamentally wrong and you need the old template site back.

### Steps

1. DigitalOcean control panel → **Droplet → Snapshots**
2. Find the snapshot you took in step 3 of `first-deploy.md`
3. Click **Restore Droplet**
4. Confirm. The droplet is unavailable for ~5 min.
5. When it comes back, the template site is live again at `https://alibalib.com`.
6. DNS unchanged — no Porkbun action needed.
7. If you had any work you want to preserve from the rolled-back state:
   - SSH in while the droplet is still up (before restoring)
   - Export anything you need: `.bak` files, logs, config changes

### After rollback

- The template site is live as it was pre-cutover
- The DO snapshot is **consumed** by the restore (it becomes the new droplet state)
- Take a fresh snapshot IMMEDIATELY if you want another rollback point
- You can re-attempt the AWBlazor deploy once you've diagnosed what went wrong

---

## Recovery scenarios

### "I can't reach the site at all"

```bash
# Check Nginx
sudo systemctl status nginx
sudo nginx -t                          # config syntax OK?
sudo journalctl -u nginx -n 50         # recent errors

# Check containers
docker compose ps
docker compose logs --tail 100 app
docker compose logs --tail 50 sqlserver
```

If Nginx is the problem:
```bash
# Restore template config
sudo rm /etc/nginx/sites-enabled/awblazor.conf
# Re-enable the old config from the tarball if needed:
sudo tar xzf ~/template-site-backup-*.tar.gz -C / etc/nginx
sudo nginx -t && sudo systemctl reload nginx
```

### "The app runs but I can't log in"

Most common cause: DataProtection keys in the `appdata` volume are mismatched.

```bash
# Inspect the DataProtection keys volume
docker compose exec app ls -la /app/App_Data

# Last resort: wipe DataProtection keys (all existing sessions invalidated)
docker compose stop app
docker volume rm awblazor-appdata
docker volume create awblazor-appdata
docker compose up -d app
```

You'll need to log in fresh.

### "SQL Server container won't start"

```bash
docker compose logs sqlserver --tail 100
# Common causes:
# - Volume permissions mismatch: `sudo chown -R 10001:10001 /var/lib/docker/volumes/awblazor-sqldata/_data`
# - SA_PASSWORD too short / missing complexity
# - Port 1433 already bound by another process (check with `sudo ss -tlnp | grep 1433`)
```

### "All containers gone after host reboot"

They should auto-start because of `restart: unless-stopped` in `docker-compose.yml`. If not:

```bash
cd /opt/awblazor
docker compose up -d
```

If Docker daemon itself isn't running:
```bash
sudo systemctl enable --now docker
```

---

## Rollback decision tree

```
Something's wrong
├── App container only → Layer 1 (container restart / image revert)
├── Database is bad    → Layer 2 (restore from .bak)
├── Nginx / host OS    → Restore nginx from tarball OR Layer 3
├── Total disaster     → Layer 3 (DO snapshot restore)
└── Still broken after → Open support ticket, share docker logs + nginx error log
```

---

## What NOT to do

- **Don't** `docker compose down -v` — the `-v` wipes volumes, including SQL data and DataProtection keys. Without a recent `.bak`, that's data loss.
- **Don't** `rm -rf /opt/awblazor` without first exporting `.env` and the latest `.bak`.
- **Don't** modify `/etc/letsencrypt` files manually — let certbot manage them.
- **Don't** edit files inside the `sqldata` volume. That's the SQL Server data directory; only SQL Server should write there.
- **Don't** panic. DNS is unchanged. The domain still points to the droplet. Any rollback that gets the droplet into a good state brings the site back up.
