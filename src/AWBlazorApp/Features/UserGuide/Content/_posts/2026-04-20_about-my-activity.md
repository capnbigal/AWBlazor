---
title: About My Activity
summary: The My activity page at /my-activity — your personal audit log of what you did in the app.
tags: [my-workspace, activity]
category: entity-guide
author: AWBlazor
---

The My activity page at `/my-activity` is your personal audit log. Everything you did in the app — pages visited, records created, records updated, exports downloaded, API keys rotated — is captured and displayed here in reverse chronological order. The page exists partly for you (to retrace what you did yesterday) and partly for compliance (to prove what you did when asked). It lives in the My workspace section of the nav rather than in Account because it is a working view, not a setting.

## What gets logged

Every user action that changes data or produces an exportable output is logged. Creating a new forecast, updating a work order, posting an adjustment, exporting a CSV from an analytics dashboard, generating or revoking an API key, changing your notification rules — all of these produce an activity entry. Pure navigation (opening a page without interacting with it) is logged at a coarser granularity — the page visit is recorded but not every click within the page.

## The row format

Each activity row has a timestamp, an action type, the affected entity type and ID, a short description, and a link to the affected record if it still exists. If the record has been deleted, the description preserves enough identifying information (name, number, key) that the historical context makes sense even without a working link. This matters because audit reconstruction often happens after records have been cleaned up.

## Filters and search

The page supports filters by action type, entity type, and date range. Full-text search across the description field lets you find a specific activity without remembering exactly when it happened. The common investigation — "I remember I updated something about customer X last Tuesday but I cannot remember what" — is exactly what the filters are designed to support.

## Your data vs the admin view

My activity shows only your own activity. Administrators can see every user's activity at `/admin/activity`, which is the global version of the same data. The two pages share the underlying log table and the same filter model; the only difference is scope. If you need to see another user's activity for a legitimate reason, the request goes through an administrator.

## Retention and export

Activity entries are retained for a configurable period — typically one year for active records and longer for compliance-sensitive actions like API key operations or permission changes. The retention settings are administered through `/admin/permissions` and rollup. You can export your activity to CSV for your own records at any time — the download respects whatever filter is active, so you get exactly what you see on screen.
