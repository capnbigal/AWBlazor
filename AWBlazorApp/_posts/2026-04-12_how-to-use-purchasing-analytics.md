---
title: Using the Purchasing Analytics Dashboard
summary: How to analyze purchase order spend, vendor concentration, and lead times.
tags: [analytics, purchasing]
category: how-to
author: Elementary App
---

The Purchasing Analytics Dashboard aggregates data from `Purchasing.PurchaseOrderHeader`, `Purchasing.PurchaseOrderDetail`, `Purchasing.Vendor`, and `Purchasing.ShipMethod` to give you a clear picture of procurement operations. It answers questions about where money is being spent, which vendors dominate the supply chain, and how reliably materials arrive on schedule.

## PO Spend Overview

The spend overview section shows total purchase order expenditure for the selected period, broken down by vendor, product category, and time. A KPI card at the top displays the aggregate spend — the sum of `TotalDue` across all purchase order headers in the date range. A trend chart plots monthly or quarterly spend, making seasonal procurement patterns visible. Below the chart, a ranked table lists the top spending categories so you can quickly identify which product lines or raw materials are consuming the most procurement budget.

## Vendor Concentration

The vendor concentration view analyzes how procurement spend is distributed across suppliers. A pie or donut chart shows each vendor's share of total spend, and a summary metric indicates the concentration ratio — for example, the percentage of total spend going to the top 3 vendors. High concentration means the business depends heavily on a few suppliers, which creates supply chain risk. Low concentration suggests a diversified vendor base but may indicate missed volume discount opportunities. The table below the chart lists each vendor with their total spend, order count, and average order size for the period.

## Lead Time Analysis

Lead time measures the gap between when a purchase order is placed (`OrderDate` on `PurchaseOrderHeader`) and when it arrives (`ShipDate`). The dashboard calculates average lead time by vendor and by product, plotted over time to show whether delivery performance is improving or degrading. A KPI card shows the overall average lead time for the current period alongside the previous period's average. Vendors with consistently long or increasing lead times may need to be replaced or supplemented with alternative suppliers. The detail table lets you sort vendors by average lead time, worst-case lead time, and on-time delivery percentage.

## Period Selection and Filtering

The date range controls at the top of the page scope all metrics to a specific window. You can select predefined ranges or set custom dates. Additional filters let you narrow the view to a specific vendor, product category, or ship method. When you apply a vendor filter, the spend chart shows only that vendor's orders, and the lead time metrics recalculate for that vendor alone. This makes it easy to prepare for a vendor review meeting with all relevant performance data on a single screen.

## CSV Export

All tables and charts on the Purchasing Dashboard support CSV export. Click the export icon to download the currently filtered dataset. The export includes vendor names, order dates, amounts, lead times, and any other columns visible in the current view. This data can be imported into procurement planning tools, shared with the finance team for budget reconciliation, or used as input for PurchaseOrderSpend forecasts within Elementary App's forecasting module.
