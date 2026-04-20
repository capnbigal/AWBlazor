---
title: About the Performance Module
summary: The Performance module in the Insights section — summary, OEE snapshots, metrics, KPIs, scorecards, and reports.
tags: [performance, analytics]
category: entity-guide
author: AWBlazor
---

The Performance module lives in the Insights section of the nav rather than in Operations, because it reports across the whole factory rather than operating any one module. The Performance summary page at `/performance` is the landing view. Below it are OEE snapshots, production metrics, maintenance metrics, KPIs, scorecards, and performance reports — the full suite of historical and aggregated operational performance data.

## Performance summary

The Performance summary at `/performance` is the cross-module dashboard. It shows current-period KPI values against their targets, the trend from the previous period, and flags for any KPI that is out of tolerance. Clicking a KPI opens its detail page under `/performance/kpis` where the historical series and the underlying data source are shown.

## OEE snapshots vs live rollup

Historical OEE data lives in OEE snapshots at `/performance/oee`. Snapshots are daily and weekly values computed at the end of each period and stored permanently — you use them to chart trends and compare week-on-week or month-on-month. Live OEE during a shift is at `/mes/oee` and is not stored; it is recomputed every time you open the page. The two serve different purposes: snapshots are for trend analysis, rollup is for intervention during the shift.

## Production and maintenance metrics

Production metrics at `/performance/production-metrics` and Maintenance metrics at `/performance/maintenance-metrics` are the two detailed metric pages. Production metrics covers throughput, yield, scrap rate, changeover time, cycle-time adherence, and shift-to-shift comparison. Maintenance metrics covers MTBF (mean time between failures), MTTR (mean time to repair), PM compliance rate, reactive-vs-scheduled ratio, and cost per asset. Both pages support drill-down from summary metric to underlying runs or work orders.

## KPIs and scorecards

KPIs at `/performance/kpis` are the curated list of the organization's top-level indicators — typically 10 to 20 metrics picked by management as the ones that matter most. Each KPI has a target, a current value, a trend, and a data source definition. The KPI editor at `/kpis` (Admin only, in My workspace) is where the KPI definitions are maintained. Scorecards at `/performance/scorecards` aggregate KPIs into role-specific views — a plant manager's scorecard, a quality manager's scorecard, a maintenance supervisor's scorecard — each showing the KPIs relevant to that role.

## Performance reports

Performance reports at `/performance/reports` is the library of formatted report templates — monthly operational review, quarterly KPI package, annual performance summary. Running a report at `/performance/reports/{id}/run` produces a formatted output that can be exported or scheduled for automatic delivery. Scheduled delivery is configured at `/reports/schedule` (Admin only, in My workspace) where report runs can be set to fire on a calendar cadence and emailed to distribution lists.
