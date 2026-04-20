---
title: About OEE Snapshots and Production Metrics
summary: Two Performance pages — /performance/oee for OEE history and /performance/production-metrics for detailed production KPIs.
tags: [performance, oee, production-metrics]
category: entity-guide
author: AWBlazor
---

OEE snapshots at `/performance/oee` and Production metrics at `/performance/production-metrics` are the two Performance pages that drill into day-to-day production health. The first focuses specifically on the OEE family of metrics — overall equipment effectiveness and its components — while the second covers the broader set of production KPIs. Both are snapshot-based: values are computed at period end and stored, so historical comparisons are stable.

## OEE snapshots

Each OEE snapshot at `/performance/oee` is a period's OEE value and its three components (availability, performance, quality) for a specific station, line, or plant. Snapshots are typically computed at shift end, day end, and week end, so the page shows three granularities simultaneously. The trend chart plots OEE over time and lets you overlay a target line, making it easy to see whether the facility is converging on goal or drifting.

## Reading the component breakdown

When OEE is below target, the three components tell you where to focus. Low availability means the line was not running as planned — downtime is the issue, go to `/mes/downtime` and look at reason codes. Low performance means the line ran but too slowly — cycle time is not meeting the routing standard, investigate by station and operator. Low quality means the line produced but with too much scrap — yield is the issue, go to `/quality/inspections` and `/quality/ncrs` for root cause.

## Production metrics

Production metrics at `/performance/production-metrics` covers the non-OEE production KPIs. Throughput (units per period by station and line), yield (good-unit percentage), changeover time (minutes per changeover), scrap rate (scrap-unit percentage, split by reason), and cycle-time adherence (actual cycle vs routing standard). Each metric is computed from the underlying run and scrap transaction data, so drilling down to the raw events is always possible.

## Comparative views

Both pages support side-by-side comparison — shift-to-shift, day-to-day, week-to-week, and station-to-station. The comparison view is what production managers use during weekly reviews: it surfaces which station or shift is consistently behind and makes the gap specific enough to act on. Absolute numbers tell you where you are; comparative views tell you where to look.

## Feeding into KPIs and reports

Production metrics and OEE snapshots feed the KPI definitions at `/performance/kpis` and the report templates at `/performance/reports`. A KPI is usually a rolled-up view of one or more metrics with a target, a tolerance band, and a data-quality flag. A report pulls together KPIs, metrics, and analysis text into a formatted package suitable for distribution. The snapshots and metrics are the raw layer; KPIs and reports are the curated layer built on top.
