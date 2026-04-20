---
title: How to Create a Forecast
summary: Step-by-step instructions for creating and computing a time-series forecast.
tags: [forecasting]
category: how-to
author: AWBlazor
---

Creating a forecast in AWBlazor involves selecting a data source, choosing a statistical method, configuring parameters, and running the computation. This guide walks through each step from start to finish.

## Step 1: Navigate to the Forecasts Page

Open the left navigation drawer and click on Forecasts under the Insights section. The Forecasts page displays a MudDataGrid listing all existing forecasts with their name, data source, method, status, and last evaluation date. From here you can view, edit, or create forecasts.

## Step 2: Create a New Forecast

Click the New Forecast button in the toolbar above the grid. A dialog or form will open asking for the basic forecast configuration. Start by giving your forecast a descriptive name — something like "Q3 2026 Sales Revenue SMA" that identifies the time period, data source, and method at a glance. Select your data source from the dropdown. Available sources include SalesRevenue (aggregated from `Sales.SalesOrderHeader`), WorkOrderVolume (from `Production.WorkOrder`), PurchaseOrderSpend (from `Purchasing.PurchaseOrderDetail`), and others depending on your deployment. The data source determines which historical data the forecast will analyze.

## Step 3: Choose a Method and Parameters

Select one of the four forecasting methods. Simple Moving Average (SMA) requires a window size — the number of past periods to average. A window of 3 gives a responsive but noisy forecast; a window of 12 smooths aggressively but lags behind trends. Weighted Moving Average (WMA) uses the same window size parameter but weights recent observations more heavily. Exponential Smoothing requires an alpha value between 0 and 1: values near 1 react quickly to changes, while values near 0 produce a very smooth forecast. Linear Regression has no required parameters beyond the lookback period, though you can optionally set a confidence interval width.

## Step 4: Set Lookback and Horizon

The lookback period defines how many historical data points feed into the model. For monthly SalesRevenue data, a lookback of 24 means the model considers the last two years of orders. The horizon specifies how many future periods to project — a horizon of 6 on monthly data generates predictions for the next six months. Choose these values based on your business need: longer lookbacks capture more trend information but may include outdated patterns, while shorter lookbacks are more responsive to recent changes.

## Step 5: Save and Compute

Click Save to persist the forecast in Draft status. At this point the forecast is stored but no predictions have been generated. Click the Compute button to run the selected method against the historical data. The computation queries the underlying AdventureWorks tables, builds the time series, applies the statistical method, and stores the predicted values. Once computation completes, the forecast detail view shows a chart overlaying historical actuals with projected values. You can adjust parameters and recompute as many times as you like while the forecast is in Draft status.

## Step 6: Promote to Active

When you are satisfied with the forecast configuration and its predictions look reasonable, promote it to Active status using the status toggle or action button. Active forecasts appear on the Forecast Analytics Dashboard, are included in method comparison views, and become eligible for scheduled evaluation runs. Once active, parameters are locked to ensure consistency in evaluation metrics. If you later need to adjust parameters, archive the current forecast and create a new one with the updated configuration.
