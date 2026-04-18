# GitHub Container Registry setup (one-time)

Switching the droplet from local `docker build` to `docker compose pull` removes the
biggest single failure mode of small-VM deploys: the .NET publish step OOM-killing
the entire droplet during Razor compilation.

## What's in place

- `.github/workflows/build-and-push-image.yml` — builds the production image on every
  push to `main` and publishes it to `ghcr.io/capnbigal/awblazor` with two tags:
  - `latest` — always the newest main build
  - `<short-sha>` — immutable per-commit tag, used for rollbacks
- `docker-compose.yml` — `app` service references `image: ghcr.io/capnbigal/awblazor:${APP_TAG:-latest}`
  (no more `build:` block).
- `docker-compose.build.yml` — optional override for local builds during dev.

## One-time setup

These steps only need to run once, after merging this PR.

### 1. Wait for the first build

When this PR merges to `main`, the workflow runs and pushes
`ghcr.io/capnbigal/awblazor:latest` for the first time. Watch it complete on the
[Actions tab](https://github.com/capnbigal/AWBlazor/actions). Expect ~5-15 minutes
on the cold cache; subsequent builds are faster.

### 2. Make the package public

GHCR packages default to private. Public makes the droplet pull without auth:

1. Go to <https://github.com/capnbigal?tab=packages>
2. Click the new `awblazor` package
3. **Package settings** (right sidebar) → **Change package visibility** → **Public**

You only need to do this once. From then on every new image inherits the package's
visibility.

### 3. First droplet deploy from GHCR

```bash
cd /opt/awblazor
git pull                                 # get the new docker-compose.yml
docker compose pull app                  # first pull from GHCR
docker compose up -d --force-recreate app
docker compose logs -f app               # watch for "Now listening on http://[::]:8080"
```

If the pull fails with `denied` you missed step 2 — package is still private.

### 4. Drop the old local image (cosmetic cleanup)

```bash
docker image rm awblazor:latest 2>/dev/null   # the old locally-built tag
docker image prune -f
```

## Day-to-day deploys after this

```bash
cd /opt/awblazor
docker compose pull app
docker compose up -d --force-recreate app
```

That's it. No more `docker compose build` on the droplet.

## Rollback to a prior image

Find the SHA of the commit you want to roll back to (e.g. from `git log` or the
GitHub Releases / Actions page), then:

```bash
APP_TAG=a1b2c3d docker compose up -d --force-recreate app
```

Roll forward the same way:

```bash
APP_TAG=latest docker compose up -d --force-recreate app
```

## Local build (dev only)

If you ever do install Docker locally and want to test a Dockerfile change before
pushing:

```bash
docker compose -f docker-compose.yml -f docker-compose.build.yml build app
docker compose -f docker-compose.yml -f docker-compose.build.yml up -d app
```

The override file points compose at `Dockerfile` instead of GHCR.
