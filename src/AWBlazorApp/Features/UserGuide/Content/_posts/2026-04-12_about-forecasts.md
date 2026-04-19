---
title: About Forecasts
summary: An overview of the forecasting system, its data sources, methods, and evaluation lifecycle.
tags: [forecasting]
category: entity-guide
author: AWBlazor
---

The forecasting module in AWBlazor lets you build time-series predictions against real AdventureWorks data. A forecast is a named entity that combines a data source, a statistical method, and a set of parameters to project future values over a configurable horizon. Forecasts are stored in the database alongside their computed results, so you can compare predictions against actual outcomes over time.

## Data Sources

Each forecast draws its historical data from one of several predefined data sources derived from AdventureWorks2022 tables. SalesRevenue aggregates order totals from `Sales.SalesOrderHeader` by period. WorkOrderVolume counts production work orders from `Production.WorkOrder` grouped by time bucket. PurchaseOrderSpend sums line totals from `Purchasing.PurchaseOrderDetail`. Additional sources may cover employee headcount growth or product listing activity. The data source determines which table is queried, how rows are aggregated, and what the Y-axis of your forecast represents.

## Forecast Methods

Four statistical methods are available, each suited to different data characteristics. Simple Moving Average (SMA) computes the arithmetic mean of the most recent N observations — it smooths out noise but lags behind trends. Weighted Moving Average (WMA) assigns linearly increasing weights to more recent observations, making it more responsive to recent changes than SMA. Exponential Smoothing applies an exponentially decaying weight to past values via a smoothing factor (alpha), balancing responsiveness and stability. Linear Regression fits a straight line through the lookback window using ordinary least squares, capturing steady upward or downward trends but ignoring seasonality.

## Parameters and Configuration

When creating a forecast you configure several parameters. The lookback period defines how many historical data points feed into the model — a 12-month lookback on monthly SalesRevenue data uses the last year of orders. The horizon specifies how many future periods to project. Method-specific parameters include the window size for SMA and WMA, the alpha value (0 to 1) for Exponential Smoothing, and optional confidence interval width for Linear Regression. These parameters directly affect forecast accuracy and should be tuned based on the volatility and trend behavior of your chosen data source.

## Status Lifecycle

Every forecast moves through a three-stage lifecycle: Draft, Active, and Archived. A newly created forecast starts in Draft status. While in Draft, you can edit its parameters, change the data source or method, and run trial computations. Once you are satisfied with the configuration, promoting the forecast to Active locks its parameters and makes it eligible for scheduled evaluation. Active forecasts are the ones displayed on the Forecast Analytics Dashboard and included in accuracy comparisons. When a forecast is no longer relevant — perhaps the underlying business conditions have changed — you can archive it. Archived forecasts remain in the database for historical reference but are excluded from dashboards and evaluation runs.

## Evaluation Process

Forecast evaluation compares predicted values against actual outcomes once enough real data has accumulated to cover one or more forecast periods. The system calculates accuracy metrics including Mean Absolute Error (MAE), Mean Absolute Percentage Error (MAPE), and Root Mean Square Error (RMSE). These metrics appear on the Forecast Analytics Dashboard, where you can compare methods side-by-side to see which approach best fits a given data source. Evaluation can be triggered manually or scheduled via Hangfire background jobs. Each evaluation run is timestamped, so you can track whether a forecast's accuracy is improving or degrading over time.
