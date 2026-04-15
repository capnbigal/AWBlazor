# Vendor API integration guide

How an external vendor connects to the AWBlazor REST API, and how we monitor and manage that traffic from our side.

---

## Part 1 — What the vendor needs to know

### Base URL

```
https://alibalib.com
```

All API endpoints live under `/api/*`. Interactive Swagger UI (schemas + try-it-out) is published at:

```
https://alibalib.com/swagger
```

Swagger is Admin-gated in production — vendors won't have access. Give them the relevant endpoint paths and example payloads out-of-band.

### Authentication

Two schemes are accepted on every `/api/*` endpoint via the `ApiOrCookie` policy. For vendor integrations, **use API keys**.

#### API key (recommended for vendors)

1. We issue a key tied to a dedicated vendor user account (see Part 2 below).
2. The vendor sends the key on every request in the `X-Api-Key` header.
3. Keys are stored hashed (SHA-256) — we can't recover a lost key, only rotate it.

Example with `curl`:
```bash
curl -H "X-Api-Key: ek_01HW...yourkey..." \
     https://alibalib.com/api/aw/products?name=bike&take=5
```

PowerShell:
```powershell
$headers = @{ 'X-Api-Key' = 'ek_01HW...yourkey...' }
Invoke-RestMethod -Uri 'https://alibalib.com/api/aw/products?name=bike&take=5' -Headers $headers
```

C# (HttpClient):
```csharp
using var http = new HttpClient { BaseAddress = new Uri("https://alibalib.com") };
http.DefaultRequestHeaders.Add("X-Api-Key", "ek_01HW...yourkey...");
var products = await http.GetFromJsonAsync<PagedResult<Product>>("/api/aw/products?name=bike&take=5");
```

The key inherits whatever roles the owning user has (Admin / Manager / Employee). Role-gated endpoints check those roles exactly as they do for interactive users.

#### Cookie (browser / Blazor clients only)

Vendors hitting the API from backend services should **not** use cookies. Cookie auth is only for users browsing the app in a browser.

### Rate limiting

Every request counts toward a **fixed-window limit of 100 requests / minute per client IP**. A 429 response means the window is full — wait for the next minute and retry. The limiter is configured in `Program.cs` via `AddRateLimiter`.

If the vendor needs a higher limit, tell us up front and we'll adjust the policy. We don't currently apply a per-API-key limit, only per-IP.

### Response shapes

List endpoints return a paged result:

```json
{
  "items":      [ { ... }, { ... } ],
  "totalCount": 504,
  "skip":       0,
  "take":       50
}
```

Single-entity endpoints return the entity directly:

```json
{ "id": 780, "name": "Mountain-200 Black, 38", "productNumber": "BK-M68B-38", ... }
```

Errors follow standard HTTP status codes plus a JSON body:

| Status | Meaning                                | Body example                                          |
|-------:|----------------------------------------|-------------------------------------------------------|
|    200 | OK                                     | the payload                                           |
|    400 | Validation failed                      | `{ "errors": { "Name": ["Required."] } }`             |
|    401 | Missing / invalid API key              | (empty body + `WWW-Authenticate` header)              |
|    403 | API key OK, role insufficient          | (empty body)                                          |
|    404 | Entity not found                       | (empty body)                                          |
|    429 | Rate limited                           | `{ "status": 429, "title": "Too Many Requests" }`     |
|    500 | Unhandled server error (alert us!)     | `{ "traceId": "00-…", "title": "An error occurred." }`|

### Example endpoints

Curated set — full list in Swagger:

| Method | Path                                       | Purpose                             | Min role |
|--------|--------------------------------------------|-------------------------------------|----------|
| `GET`  | `/api/aw/products?name=&skip=&take=`        | List / search products              | any auth |
| `GET`  | `/api/aw/products/{id}`                    | Single product                      | any auth |
| `POST` | `/api/aw/products`                         | Create product                      | Employee |
| `PATCH`| `/api/aw/products/{id}`                    | Partial update                      | Employee |
| `DELETE`| `/api/aw/products/{id}`                   | Delete                              | Manager  |
| `GET`  | `/api/aw/sales-order-headers`               | Sales orders                        | any auth |
| `GET`  | `/api/geo/addresses?territoryId=&take=`    | Customer addresses with lat/lng     | any auth |
| `GET`  | `/api/geo/territories`                     | Sales territory list                | any auth |

### Security headers expected on responses

Every response includes:

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options:    nosniff
X-Frame-Options:           DENY
Referrer-Policy:           strict-origin-when-cross-origin
Content-Security-Policy:   default-src 'self'; ...
```

Vendors should not need to handle these — they're for browsers.

### CORS

The API does **not** set `Access-Control-Allow-Origin`. Vendors must call from their server-side backend, not from a browser JS frontend on a different origin. If they need cross-origin access, tell us and we'll add their origin to the CORS config.

---

## Part 2 — What we need from the vendor

Collect up front:

- [ ] **Contact email** — goes on the dedicated user account and receives key-rotation notices.
- [ ] **Business contact name + phone** — for outage/security notifications.
- [ ] **Use case** — which endpoints, expected shape of workload. Informs role assignment + rate-limit review.
- [ ] **Outbound IP range(s)** — optional but recommended; lets us pre-whitelist in firewall/WAF if we ever add one, and flag unexpected geos in Serilog.
- [ ] **Expected volume** — requests/minute peak and average. If it's >50/min sustained, we should either raise the rate limit or apply a per-key bucket.
- [ ] **Shared secret delivery preference** — signed PGP email, password-protected archive, or in-person handoff. Never send a key over plain email or Slack DM.

---

## Part 3 — Onboarding checklist (done by us)

Walk through as an Admin user at https://alibalib.com:

1. **Create the vendor user** — `/admin/users` → **New user** → email = vendor contact email. Flag: `EmailConfirmed=true` so they never need to click a confirmation link they won't receive.
2. **Assign the right role.** Rule of thumb:
   - Read-only integration → **Employee** (default read on all `/api/aw/*`).
   - Needs to create/update rows → **Employee** (Employee already covers create/update on most endpoints).
   - Needs to delete → **Manager**.
   - Full admin → **Admin** (rare — only when the vendor is operating the app, not just integrating).
3. **Sign in as that user** (use "Sign in as user" if the feature exists, or ask the vendor to set their password then switch back).
4. Navigate to `/Account/Manage/ApiKeys` and click **Generate new key**.
5. **Name** the key with vendor + environment (e.g. `acme-corp-prod`, `acme-corp-staging`). Name is searchable in `/admin/users` drilldowns.
6. **Copy the plain-text key once** — it's shown only at generation; after that only the SHA-256 hash is stored.
7. **Deliver securely** via the vendor's chosen method.
8. **Rotate the admin seed password** if you used it to impersonate.
9. **Schedule rotation reminder**: keys don't auto-expire by default, so note the issue date and set a 90-day rotation reminder.

### Key expiration

- If you set `ExpiresDate` at generation, the auth handler rejects expired keys with 401.
- Leaving it null makes the key valid indefinitely until revoked — fine for long-running integrations, but rotate every 90 days anyway.

---

## Part 4 — Monitoring

### Live traffic

- **`/admin/request-log`** — every HTTP request Serilog wrote to the `dbo.RequestLogs` sink, filterable by user, path, status, and time window. `UserName` on a row is the API key's owning user.
- **`/admin/activity`** — aggregated KPIs (requests today, unique users, errors) across all users. Quick health check.
- **`/my-activity`** — any user (including the vendor themselves, if you give them the link) sees a 52-week contribution grid of their own requests. Handy for confirming with a vendor that yes, we see your traffic.

### Scheduled jobs

- **`/hangfire`** — Admin-only dashboard showing recurring jobs, queued jobs, failed jobs. Check here if a vendor's API activity triggered any Hangfire processing (audit log cleanup, forecast evaluation, etc.).

### Errors

- Serilog pipes to the console inside the container (`docker compose logs app`) and to `dbo.RequestLogs`. Filter by status 4xx/5xx or by TraceId.
- `SecurityAuditLog` records login failures, API key generation/revocation, and role grants. Query it in the Database Explorer at `/reports` if you need a security timeline.

### Rate-limit hits

Rate-limited requests return 429 but don't write a dedicated log row beyond the Serilog request log. Filter `RequestLog` for `StatusCode=429` to find clients getting throttled.

### Low-level (droplet)

When you SSH into the droplet:

```bash
cd /opt/awblazor
docker compose logs -f app | grep -E 'error|warn|4[0-9]{2}|5[0-9]{2}'   # live tail filtering bad statuses
docker compose logs app --since 24h > today.log                           # snapshot of the day
```

---

## Part 5 — Management

### Rotating a key

Same flow as issuing a new one:

1. Admin → `/admin/users` → find the vendor user → "API keys" link.
2. Generate new key, deliver.
3. Ask vendor to confirm cutover.
4. Revoke the old key by clicking **Revoke** on the old row — sets `RevokedDate` and the auth handler rejects future requests with 401.

### Revoking on short notice (compromise)

If a key leaks:

1. Revoke immediately at `/admin/users` → user → ApiKeys → **Revoke**.
2. Check `SecurityAuditLog` for recent activity under that user.
3. Filter `RequestLog` for the user's `UserName` over the suspected compromise window. Flag any suspicious paths, especially `DELETE` / `PATCH` calls.
4. If anything looks off, restore affected rows from the nightly `.bak` (see `deployment/commands.md` → SQL backup / restore).
5. Send a brief post-mortem to the vendor contact.

### Offboarding

1. Revoke all the vendor's API keys.
2. `/admin/users` → disable the user (`LockoutEnabled=true` or delete).
3. Document the date in an internal ticket for future reference.

### Raising the rate limit

If 50 req/min sustained isn't enough, edit `Program.cs` `AddRateLimiter` policy and redeploy. Consider switching to a sliding window or adding a per-key bucket if you're onboarding multiple high-volume vendors. For a one-off boost, simply bump the limit in appsettings without a code change if you add a config binding (not currently wired — ask for it when you need it).

---

## Part 6 — Rollback / emergency

If a vendor integration is causing production pain:

1. Revoke the key (above) — stops new traffic immediately.
2. If the app is already impacted, restart: `docker compose up -d --force-recreate app` on the droplet.
3. Last resort: `git checkout <known-good-commit>` on the droplet, rebuild. See `deployment/rollback.md`.

---

## Appendix A — Sharing with a vendor

Copy Parts 1, 2, and 4 into a doc you give the vendor. Omit Parts 3, 5, 6 (internal-only).

## Appendix B — Local testing before handing over

Before a vendor tries the key, verify it works with a quick curl:

```bash
curl -i -H "X-Api-Key: <the-key>" https://alibalib.com/api/aw/products?take=1
# Expect: HTTP/2 200 + a PagedResult<Product> JSON body
```

If you get 401, the key didn't hash correctly or the user was disabled — regenerate.
