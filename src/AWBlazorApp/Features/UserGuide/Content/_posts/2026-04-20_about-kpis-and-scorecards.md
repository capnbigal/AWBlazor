---
title: About KPIs and Scorecards
summary: How the curated KPIs are defined and displayed, and how scorecards group them into role-specific views.
tags: [performance, kpis, scorecards]
category: entity-guide
author: AWBlazor
---

KPIs at `/performance/kpis` are the curated list of top-level indicators — typically 10 to 20 metrics that management has selected as the ones that matter most for the business. Scorecards at `/performance/scorecards` bundle KPIs into role-specific views so a plant manager, a quality manager, and a maintenance supervisor each see the KPIs relevant to their role rather than a single one-size-fits-all dashboard.

## What a KPI definition holds

Each KPI has a name, a description, a current value, a target, a tolerance band (green / yellow / red thresholds), a data-source definition, a refresh cadence, and a trend indicator. The data-source definition specifies exactly how the current value is computed — which tables, which aggregations, which filters — so the KPI is reproducible and auditable. The refresh cadence determines how often the value is recomputed; some KPIs refresh hourly, some daily, some at period-end.

## The KPI editor

KPI definitions are maintained at `/kpis`, which lives under My workspace in the Admin tools section. Editing KPIs is restricted to admins because changing a KPI definition changes what the number means, and that propagates across every scorecard and report that consumes the KPI. Non-admins see the display-only view at `/performance/kpis` with current values, trends, and historical charts.

## Reading the KPI page

The `/performance/kpis` page shows every active KPI as a card with the current value, a trend sparkline, the target, and a colour band matching the tolerance zone. Clicking a KPI opens its detail view with the full historical series, the data source definition, and any annotations about events that may have affected the value (a process change, a one-time event, a known data issue). The detail view is where you go when a KPI has moved unexpectedly and you want to understand why.

## Scorecards as grouped views

A scorecard at `/performance/scorecards` is a named collection of KPIs relevant to a specific role or review cadence. "Plant manager weekly", "Quality manager monthly", "Maintenance supervisor shift review" are typical scorecards. A KPI can appear on multiple scorecards. Scorecards are the pragmatic compromise between the one-size-fits-all problem and the overhead of building completely separate dashboards for every role.

## Scorecards and meetings

Scorecards are typically designed around review cadences. The weekly operations review has a scorecard; the monthly management review has one; quarterly executive review has one. The scorecard's layout and the KPI selection reflect the agenda of the meeting. Scorecard detail pages can be exported to PDF and shared, which makes them the natural input to a meeting pack.
