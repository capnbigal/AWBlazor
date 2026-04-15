---
title: About API Keys
summary: How API key authentication works, from generation to role inheritance.
tags: [api-keys]
category: entity-guide
author: AWBlazor
---

API keys provide a way for external tools, scripts, and integrations to authenticate against AWBlazor's REST API endpoints without using browser-based cookie authentication. Each key is tied to a specific user account and inherits that user's roles, so an API key created by an Admin user carries Admin privileges, while one created by an Employee user is limited to Employee-level access.

## Key Format and Storage

Every API key is prefixed with `ek_` followed by a cryptographically random string. This prefix makes it easy to identify AWBlazor keys in configuration files, environment variables, or secret managers — if you see a string starting with `ek_`, you know it belongs to this application. When a key is generated, the raw value is displayed exactly once for the user to copy. The application stores a SHA-256 hash of the key in the `ApiKeys` table rather than the plaintext value. This means that if the database is compromised, the actual key values cannot be recovered from the stored hashes.

## Authentication Flow

When an external client sends a request to any `/api/*` endpoint, it includes the key in the `X-Api-Key` HTTP header. The `ApiKeyAuthenticationHandler` intercepts the request, computes the SHA-256 hash of the provided key, and looks it up in the `ApiKeys` table. If a matching hash is found and the key has not been revoked, the handler constructs a `ClaimsPrincipal` with the owning user's identity and roles. From that point forward, the request is treated exactly as if the user had authenticated via a browser session — `[Authorize]` attributes, role checks, and policy evaluations all work identically.

## The ApiOrCookie Policy

AWBlazor defines an `ApiOrCookie` authorization policy in `Program.cs` that accepts either ASP.NET Core Identity cookies or the API key authentication scheme. All `/api/*` minimal API endpoints use this policy. This dual-scheme approach means the same endpoints are accessible from both the interactive Blazor UI (which uses cookies) and external HTTP clients (which use API keys). You do not need separate endpoint definitions for internal and external consumers.

## Role Inheritance

Because the API key is linked to a user account, it inherits whatever roles that user holds. If the user is in the Admin role, API requests made with their key can access admin-only endpoints. If the user is demoted or their roles change, the next API request with that key will reflect the updated permissions — there is no cached role snapshot on the key itself. This design keeps role management centralized in ASP.NET Core Identity rather than duplicating it in the API key system.

## Key Lifecycle

Users create and manage their API keys from the Account > Manage > API Keys page. A key can be revoked at any time, which immediately invalidates it for all future requests. Revocation is permanent — once a key is revoked, it cannot be reactivated, and a new key must be generated instead. There is no expiration date on keys by default; they remain valid until explicitly revoked. Administrators can view and revoke keys belonging to any user through the Admin Dashboard.
