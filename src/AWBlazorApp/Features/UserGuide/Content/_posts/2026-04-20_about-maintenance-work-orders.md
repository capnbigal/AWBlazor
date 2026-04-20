---
title: About Maintenance Work Orders
summary: The maintenance work-order lifecycle, scheduled vs reactive work, and how work orders tie assets to labour and parts.
tags: [maintenance, work-orders]
category: entity-guide
author: AWBlazor
---

A maintenance work order at `/maintenance/work-orders` is the unit of planned or reactive maintenance work. Every hour a mechanic spends on an asset — from a 15-minute adjustment to a day-long overhaul — is expected to be recorded against a work order, because that is where the data lives for every downstream report: MTBF, MTTR, maintenance cost per asset, technician utilization, and PM compliance. The detail view at `/maintenance/work-orders/{id}` is where the actual work is planned and signed off.

## Reactive vs scheduled work orders

Reactive work orders are opened in response to something breaking. An operator reports a fault, a downtime event is logged at `/mes/downtime`, a meter reading crosses a threshold — each can spawn a reactive work order. Scheduled work orders come from PM schedules at `/maintenance/pm-schedules`. A PM schedule fires on a calendar interval or usage threshold and creates a work order automatically. The work order's type field distinguishes the two so reports can treat them separately; the workflow from there on is identical.

## The work-order lifecycle

Every work order moves through a defined lifecycle. New is the starting state — created but not yet assigned. Planned means a technician has been assigned, parts have been reserved, and scheduled start/end dates are set. In progress is self-explanatory. On hold is used when work is blocked (waiting on parts, access, or approval). Completed means the technician has finished; Closed means the supervisor has reviewed and signed off. Closed work orders are the ones that count in reports.

## Labour, parts, and costs

Each work order accumulates labour hours (by technician, with start/stop timestamps), spare parts consumed (with quantity and cost), and any third-party charges. The cost roll-up at the bottom of the detail page sums everything so the total maintenance cost per asset is always current. Parts consumption decrements the inventory at `/maintenance/spare-parts` automatically on work-order closure; parts reservations are made at planning time so you do not plan two jobs against the same last unit.

## Links to assets and downtime

A work order references the asset it applies to. From the asset's service history, every work order that has ever touched the asset is visible — this is how you trace recurring problems. When a work order is the response to a downtime event at `/mes/downtime`, the downtime record is cross-linked; the downtime's duration and the work order's repair time are both visible in the MTTR calculation.

## Reporting

Maintenance performance rolls up through `/performance/maintenance-metrics`. MTBF, MTTR, PM compliance percentage, emergency-versus-planned ratio, and cost-per-asset are all driven by the work-order data. Keeping work orders accurate — correct start/stop times, real parts usage, real labour hours — is what makes those reports actionable. Sloppy data produces misleading KPIs that either hide real problems or raise false alarms.
