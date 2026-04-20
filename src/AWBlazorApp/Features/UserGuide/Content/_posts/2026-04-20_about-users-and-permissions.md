---
title: About Users and Permissions
summary: The admin pages for managing users, roles, and permissions — /admin/users and /admin/permissions.
tags: [admin, users, permissions]
category: entity-guide
author: AWBlazor
---

User and permission management is the responsibility of administrators. The Users page at `/admin/users` is the directory of every account in the system. The Permissions page at `/admin/permissions` is where roles are defined, permissions are assigned to roles, and the permission matrix is audited. Both pages require the Admin role and are gated accordingly; non-admins do not see them in the nav and cannot reach them by URL.

## The Users page

The `/admin/users` page lists every user with columns for email, display name, roles, last-login timestamp, active flag, and creation date. Filters narrow by role, active state, or last-login age. Clicking a user opens their detail view with the full profile, the list of roles assigned, the history of role changes, active sessions, generated API keys, and a recent-activity preview. From the detail view an admin can disable the account, force a password reset, revoke API keys, or change role assignments.

## The role model

AWBlazor uses ASP.NET Core Identity roles. The core roles are Admin (full access), Manager (write access to operational data plus approve-workflow rights), Employee (read and write access to operational data within scope), and Guest (read-only). Custom roles can be added for site-specific needs — Quality lead, Maintenance supervisor — and carry a subset of the standard permissions. A user can have multiple roles; their effective permission is the union of the roles' permissions.

## Permissions at /admin/permissions

The Permissions page at `/admin/permissions` shows the full permission catalog — every distinct permission defined in the system — cross-referenced with every role. The matrix view lets admins see at a glance which roles can do which things. Permissions are defined in code (as string constants) and are surfaced through the Permissions page without requiring a migration; when a new feature is deployed, its permissions appear on this page automatically.

## Adding a permission to a role

From the Permissions matrix, an admin can toggle a permission on or off for a given role. Changes are audited — the activity log records who changed which assignment when, with the previous and new state. Permission changes take effect on the user's next authenticated request; active sessions continue with their current permissions until the next round-trip refreshes them.

## API keys and permission inheritance

API keys inherit the permissions of the user who created them, so managing user permissions manages API access as well. When an admin revokes a role from a user, every API key held by that user immediately loses access to endpoints that required the revoked role. This is the design choice: permissions live on the user, keys are credentials; you do not have to separately hunt down and revoke API keys when a role changes because the keys automatically reflect the current user state.
