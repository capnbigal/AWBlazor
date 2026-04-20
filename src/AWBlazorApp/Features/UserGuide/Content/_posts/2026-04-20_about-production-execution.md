---
title: About Production Execution (MES)
summary: The Production module — runs, downtime, work instructions, OEE rollup, and the shop-floor data flow.
tags: [production, mes]
category: entity-guide
author: AWBlazor
---

The Production group in the Operations section holds AWBlazor's manufacturing-execution features. The MES summary page at `/mes` is the landing view. Below it, production runs at `/mes/runs` are the units of work on the floor. Downtime events at `/mes/downtime` capture the minutes when the floor is not running, and downtime reason codes at `/mes/downtime/reasons` classify them. Work instructions at `/mes/instructions` are the documents operators follow. And OEE rollup at `/mes/oee` is the live overall-equipment-effectiveness calculation, distinct from the historical OEE snapshots under Performance.

## MES summary

The MES summary page at `/mes` is what a production supervisor keeps open. It shows active runs by station, current downtime events, the rolling OEE figure, and the production metrics from the last completed shift. Cross-links from each card navigate into the detail view. The Production analytics dashboard at `/analytics/production` — linked from the top of the nav group — covers longer-horizon trends; the summary covers now.

## Production runs

A production run at `/mes/runs` is a unit of shop-floor work. Each run references a work order (the planned demand), a routing revision (how to build it), a station (where it is being built), an operator, and a start/end timestamp. The run records planned quantity, good quantity, and scrap quantity. Runs are the granularity at which most operational reporting happens — throughput, OEE, yield, and labour efficiency are all computed per-run and then aggregated.

## Downtime events and reasons

A downtime event is any time a run is not producing. When a run goes into a non-producing state, a downtime event opens. When the run resumes, the event closes. The operator selects a reason code from `/mes/downtime/reasons` — categories like Mechanical, Setup, Material shortage, Operator break, Quality hold. Reason codes matter because they are how improvement initiatives are targeted; high "Material shortage" time points at supply-chain issues, while high "Mechanical" time points at maintenance.

## Work instructions

Work instructions at `/mes/instructions` are the documents the operator follows. They reference engineering documents at `/engineering/documents` for the specifications but are presented in an operator-friendly form — big images, numbered steps, tool callouts. Each routing operation points to a work instruction so the operator opens the relevant document by navigating through the run rather than searching the document library.

## OEE rollup (live)

The OEE rollup at `/mes/oee` is the live overall-equipment-effectiveness calculation for the current shift. OEE combines availability (how much of the planned time the line ran), performance (how fast it ran relative to cycle time), and quality (how much of the output was good). The rollup page shows OEE by station, by product, and by shift. This is distinct from OEE snapshots at `/performance/oee`, which hold historical daily and weekly snapshots for trend analysis.

## Reference data

The Production group's Reference data subsection holds the raw AdventureWorks production tables — manufacturing work orders, work-order routings (raw), transaction history, scrap reasons, and product documents. You use the workflow pages above for day-to-day operation and open the reference data when you need to audit a record or build a report against the underlying catalogs.
