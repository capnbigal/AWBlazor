---
title: Managing API Keys
summary: How to create, use, and revoke API keys for REST API authentication.
tags: [api-keys, admin]
category: how-to
author: AWBlazor
---

API keys let you authenticate against AWBlazor's REST API from scripts, external tools, or other applications without needing a browser session. This guide covers the full lifecycle of creating, using, and revoking keys.

## Creating a Key

Navigate to Account > Manage > API Keys from the user menu in the top-right corner. The page displays a list of your existing keys with their creation date, last-used timestamp, and status (active or revoked). Click the Create New Key button to generate a fresh key. You will be prompted to provide an optional description — use something like "CI/CD Pipeline" or "Power BI Refresh" to remind yourself what the key is for. After clicking Create, the full key value (prefixed with `ek_`) is displayed exactly once. Copy it immediately and store it in a secure location such as a password manager or a secrets vault. Once you navigate away from the page, the raw key value cannot be retrieved again — only the SHA-256 hash is stored in the database.

## Using a Key in HTTP Requests

Include your API key in the `X-Api-Key` header of every request to the `/api/*` endpoints. For example, using curl:

```
curl -H "X-Api-Key: ek_your_key_here" https://localhost:5001/api/products
```

The key authenticates you as the user who created it and inherits that user's roles. If your user account has the Admin role, the key grants access to admin-only endpoints. If your account is an Employee, the key is limited to Employee-level endpoints. No separate permissions are configured on the key itself — it is purely a proxy for your user identity.

## Monitoring Key Usage

The API Keys management page shows a "Last Used" column for each key. This timestamp updates every time the key is used to authenticate a request, giving you visibility into which keys are actively in use and which may be stale. If you see a key that has not been used in months, consider revoking it to reduce your attack surface. Administrators can view API key usage across all users through the Admin Dashboard.

## Revoking a Key

To revoke a key, click the Revoke button next to it on the API Keys management page. Revocation is immediate — any in-flight or future requests using that key will receive a 401 Unauthorized response. Revocation is permanent and cannot be undone. If you need to restore API access after revoking a key, create a new one and update the key value in whatever external system was using the old key.

## Best Practices

Create separate keys for each integration or script rather than sharing a single key across multiple systems. This way, if one integration is decommissioned or compromised, you can revoke its key without disrupting other consumers. Use descriptive names so you can tell at a glance what each key is for. Rotate keys periodically — even though they do not expire automatically, regular rotation limits the blast radius of an undetected compromise. Avoid embedding keys directly in source code; use environment variables or secret management services instead.
