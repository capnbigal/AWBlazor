---
title: Using the Production Analytics Dashboard
summary: How to monitor work order volume, scrap rates, throughput, and product lifecycle metrics.
tags: [analytics, production]
category: how-to
author: AWBlazor
---

The Production Analytics Dashboard gives you visibility into Adventure Works' manufacturing operations. It draws data from `Production.WorkOrder`, `Production.ScrapReason`, `Production.Product`, and related tables to present operational KPIs, trend charts, and drill-down views focused on shop floor performance.

## Work Order Volume

The primary chart tracks the number of work orders created, in progress, and completed over time. Each work order in `Production.WorkOrder` has a `StartDate`, `EndDate`, and `DueDate`, which the dashboard uses to plot volume by period. You can toggle between weekly, monthly, and quarterly views. A stacked bar variant breaks volume down by product or product category, revealing which product lines are driving the most manufacturing activity. Spikes in work order volume often correlate with seasonal demand or new product launches visible on the Sales Analytics Dashboard.

## Scrap Rate Trend

The scrap rate chart shows the percentage of manufactured units that were scrapped, calculated as `ScrappedQty / (OrderQty)` aggregated by period. Each scrapped unit links to a `Production.ScrapReason` record that explains why it was rejected — reasons like paint process failure, drill-size tolerance issues, or thermoform temperature problems. The dashboard displays both the overall scrap rate trend line and a breakdown by scrap reason. Rising scrap rates for a specific reason may indicate equipment maintenance needs or quality control issues that warrant investigation.

## Throughput Metrics

Throughput measures how efficiently the production floor converts work orders into finished goods. The dashboard calculates average cycle time (the difference between `StartDate` and `EndDate` per work order) and plots it over time. A declining cycle time suggests process improvements or increased efficiency, while a rising cycle time may indicate bottlenecks, material shortages, or equipment downtime. The KPI card at the top of the page shows the current period's average cycle time alongside the previous period's value for comparison.

## Product Lifecycle View

This section visualizes the current state of the product catalog based on lifecycle dates. Products are grouped into categories: Active (has `SellStartDate`, no `SellEndDate`), End of Sale (has `SellEndDate`, no `DiscontinuedDate`), and Discontinued (has `DiscontinuedDate`). A summary chart shows the count in each category, and a timeline view plots when products entered and exited each stage. This helps production planners understand which products are still being manufactured, which are winding down, and which have been fully retired.

## Filtering and Drill-Through

Like all analytics dashboards in AWBlazor, the Production Dashboard supports period selection via date range controls and filtering by product category. Clicking on a data point in any chart filters the other views to the same context — for example, clicking a specific month in the work order volume chart updates the scrap rate and throughput views to show only that month's data. The drill-through capability lets you navigate from an aggregated metric directly to the underlying reference data rows in the Production reference data pages.
