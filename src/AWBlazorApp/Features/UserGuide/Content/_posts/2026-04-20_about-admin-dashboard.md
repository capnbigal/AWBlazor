---
title: About the Admin Dashboard
summary: The Admin dashboard at /admin — system health, user counts, recent activity, and quick links into every admin page.
tags: [admin]
category: entity-guide
author: AWBlazor
---

The Admin dashboard at `/admin` is the landing page for administrators. It surfaces system health, user activity, and a set of quick-access cards that link into every other admin page. Only users with the Admin role can see the `/admin` routes; for non-admins the entire Admin nav section is hidden and navigating to `/admin` directly returns a 403 Access denied page.

## System health

The top of the dashboard shows the system health strip. This covers database connectivity, background-job health (from Hangfire), notification worker status, outbox drain rate, and the latest database-migration state. Green, yellow, and red indicators flag any component that needs attention. Clicking into a component opens its detailed diagnostic view — for Hangfire this is the Hangfire dashboard at `/hangfire`, for the database it is the Database explorer at `/reports`, for outbox it is the Inventory outbox.

## User and session summary

A user-summary card shows the count of active users today, the count over the last seven days, and the top-five most-active users by session count. This is useful for capacity planning and for noticing unusual activity patterns. A spike in active users on a Sunday evening, for example, might indicate legitimate weekend work or might indicate a compromised account being used outside normal hours.

## Recent-activity feed

A feed of recent high-value actions across all users streams on the dashboard — new users created, permission changes, API keys generated, bulk exports, security-sensitive operations. The feed is the admin's version of the My activity page: where My activity shows your own actions, the admin dashboard's feed shows the significant actions of everyone. Clicking an entry opens the full `/admin/activity` page filtered to the affected user and action.

## Quick-link cards

The lower section of the dashboard is a grid of cards linking to the other admin pages — Users, Permissions, Request log, User activity, Guide stats, Performance rollup, and so on. Each card shows a small summary (user count, pending request count, etc.) so you can see at a glance whether that area needs attention. The grid is designed so the common admin morning routine — check users, check pending approvals, check system health — is one-click-per-task rather than multi-click.

## When to trigger rollup

The Performance rollup card at `/admin/rollup` is the one admin page with a scheduled purpose. Rollups compute snapshot values for KPIs and OEE at period boundaries — daily rollups at midnight, weekly rollups at end of week, monthly at end of month. The admin dashboard flags when a scheduled rollup has been missed or failed. Manually re-running a missed rollup from this page refills the gap without waiting for the next scheduled run.
