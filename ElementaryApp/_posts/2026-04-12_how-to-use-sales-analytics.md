---
title: Using the Sales Analytics Dashboard
summary: How to read and interact with the Sales Analytics Dashboard's KPIs, charts, and exports.
tags: [analytics, sales]
category: how-to
author: Elementary App
---

The Sales Analytics Dashboard provides a consolidated view of Adventure Works' sales performance. It pulls data from `Sales.SalesOrderHeader`, `Sales.SalesOrderDetail`, `Sales.SalesTerritory`, and `Sales.SalesPersonQuotaHistory` to present KPIs, trend charts, and breakdowns that help you understand revenue patterns and team performance.

## KPI Cards

At the top of the dashboard, summary cards display headline metrics for the selected period. Total Revenue shows the sum of `TotalDue` across all completed orders. Order Count is the number of distinct sales orders. Average Order Value divides total revenue by order count. These cards update dynamically when you change the period selector. A small trend indicator on each card shows the percentage change compared to the previous equivalent period — for example, if you are viewing Q1 2014, the trend compares against Q1 2013.

## Revenue Over Time Chart

The main chart plots revenue by month or quarter over the selected date range. You can toggle between monthly and quarterly granularity using the buttons above the chart. The chart uses a MudBlazor line or bar visualization that supports hover tooltips showing exact values for each data point. If you have active SalesRevenue forecasts, their projected values can be overlaid on the chart as a dashed line, making it easy to compare predictions against actual outcomes.

## Period Selectors

The date range controls at the top of the page let you scope the entire dashboard to a specific window. You can pick a predefined range — Last 12 Months, Year to Date, Last Quarter — or set custom start and end dates using the date pickers. All KPI cards, charts, and breakdowns recalculate when you change the period. This makes it straightforward to compare seasonal patterns across years or zoom into a specific month to investigate an anomaly.

## Territory Breakdown

The territory section groups sales by `Sales.SalesTerritory` regions. A table or chart shows revenue, order count, and average order value per territory. You can sort by any column to quickly identify the highest-performing and lowest-performing regions. Clicking a territory name filters the revenue chart to show only orders from that territory, which is useful for regional deep-dives.

## Quota vs. Actual

For organizations that set sales quotas, the Quota vs. Actual view compares each salesperson's assigned quota (from `Sales.SalesPersonQuotaHistory`) against their actual revenue for the selected period. A bar chart shows quota as a reference line and actual revenue as the filled bar, colored green when the salesperson met or exceeded quota and red when they fell short. This view helps sales managers identify who needs coaching and who deserves recognition.

## CSV Export

Every data view on the dashboard supports CSV export. Click the export icon in the toolbar to download the currently displayed data as a comma-separated file. The export respects the active period filter and any territory or salesperson selections, so you get exactly the data you see on screen. This is useful for building custom reports in Excel, feeding data into external BI tools, or sharing snapshots with stakeholders who do not have access to the application.
