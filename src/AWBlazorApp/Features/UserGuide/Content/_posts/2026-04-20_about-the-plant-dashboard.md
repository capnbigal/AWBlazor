---
title: About the Plant Dashboard
summary: The shop-floor overview at /dashboard/plant — live runs, downtime, alerts, and links into every operational module.
tags: [dashboard, production, getting-started]
category: entity-guide
author: AWBlazor
---

The Plant dashboard at `/dashboard/plant` is designed to be the one page a supervisor keeps open during a shift. It pulls the most time-sensitive information from Production, Maintenance, Quality, and Workforce into a single view so you can tell at a glance whether the floor is running as expected or something needs attention. Every card on the dashboard is a link into the full module, so the dashboard doubles as a launch pad for deeper investigation.

## Live production snapshot

The top of the page summarizes what is currently being built. It shows the count of active production runs, the count of idle stations, and the most recent OEE rollup value. Clicking the active runs card navigates to `/mes/runs` filtered to in-flight work. The OEE card links to `/mes/oee`, which holds the live rollup view rather than the historical snapshots stored under Performance.

## Downtime and alerts

The second row surfaces anything that is keeping production from running. Recent downtime events from `/mes/downtime` appear with their reason codes, and any unacknowledged qualification alerts from `/workforce/qualification-alerts` show up beside them. These two data sources are the most common reasons a line stops, so putting them next to each other makes it easier to see whether a stoppage is a people problem or an equipment problem.

## Maintenance backlog

A maintenance summary card shows open maintenance work orders and overdue PM schedules. The counts link into `/maintenance/work-orders` and `/maintenance/pm-schedules` respectively. When a PM goes overdue it usually means either the scheduler needs adjustment or the asset is not being released for service — both conversations start from this card.

## Quality indicators

The Quality strip at the bottom aggregates the count of open inspections, new non-conformances, and CAPA cases nearing their due date. The counts are coloured by severity so issues needing action stand out. Each count is a direct link to the filtered page in the Quality module.

## Role-aware behaviour

The dashboard is the same page for every user but a few cards hide depending on role. Users without access to Maintenance see the maintenance strip collapse to a single "restricted" placeholder. Admin-only rollup triggers are only shown to admins. If a card is missing for you, it is either out of scope for your role or the underlying feature has no recent data to display.
