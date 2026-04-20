---
title: About Maintenance
summary: The Maintenance module — work orders, PM schedules, asset profiles, spare parts, meter readings, tool slots, and the maintenance log.
tags: [maintenance]
category: entity-guide
author: AWBlazor
---

The Maintenance module at `/maintenance` keeps the equipment running. Work orders capture the reactive and scheduled work that gets done. PM schedules drive the scheduled work into the work-order backlog automatically. Asset profiles describe each machine in enough detail to plan maintenance intelligently. Spare parts track what you have and what you need. Meter readings feed usage data back into the PM scheduler. Tool slots manage the machine-tool configurations that link to the DBA-managed `ToolSlotConfigurations` table. And the Maintenance log at `/maintenance/logs` is the narrative history of everything that has happened.

## Maintenance summary

The Maintenance summary page at `/maintenance` is the landing view. It shows open work order counts, overdue PM counts, low-stock spare parts, and pending meter readings. Each count is a filtered link into the detail page. The page is designed so a maintenance supervisor can see their queue at a glance and decide where to spend their shift.

## The four anchors: work orders, PMs, assets, parts

Four pages form the core of daily work. Work orders at `/maintenance/work-orders` are the units of work — both reactive and scheduled. PM schedules at `/maintenance/pm-schedules` define recurring work and generate work orders automatically when the trigger fires. Asset profiles at `/maintenance/asset-profiles` describe each piece of equipment with its service history, typical PMs, and known issues. Spare parts at `/maintenance/spare-parts` tracks the parts inventory against the demand pattern.

## Data sources that drive maintenance

Meter readings at `/maintenance/meter-readings` record cycle counts, run hours, and other usage metrics that trigger usage-based PMs — "service every 1000 hours" requires someone recording the hours. Tool slots at `/maintenance/tool-slots` manage machine-tool configurations through the externally managed `dbo.ToolSlotConfigurations` table, which is owned by a DBA rather than EF migrations. Together these feed real-world usage into the maintenance plan.

## The maintenance log

The Maintenance log at `/maintenance/logs` is the chronological history of everything the maintenance team did. It aggregates work-order completions, ad-hoc notes, and inspection outcomes into a single timeline per asset. When investigating a recurring problem, the log is the first place to look — it surfaces patterns that are hard to see when the same history is split across work orders, inspections, and downtime events.

## Cross-links to other modules

The Maintenance module links heavily into other parts of the app. Assets cross-link to `/enterprise/assets`, where the asset catalog lives. Maintenance metrics roll up into `/performance/maintenance-metrics`. Downtime events recorded at `/mes/downtime` can be the reason a work order was opened. Cost centers attached to assets feed into financial roll-ups. The module is rarely used in isolation.
