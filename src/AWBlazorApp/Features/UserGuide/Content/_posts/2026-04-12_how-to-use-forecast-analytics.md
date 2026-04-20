---
title: Using the Forecast Analytics Dashboard
summary: How to interpret accuracy metrics, compare methods, and track evaluation results.
tags: [analytics, forecasting]
category: how-to
author: AWBlazor
---

The Forecast Analytics Dashboard brings together all your active forecasts in one place, showing how well each one predicts actual outcomes and helping you choose the best method for each data source. It is accessible from the Insights section in the navigation drawer and updates as new evaluation runs complete.

## Accuracy Metrics

Each active forecast displays its most recent accuracy scores. Mean Absolute Error (MAE) shows the average magnitude of prediction errors in the same units as the data — for a SalesRevenue forecast, an MAE of 5,000 means predictions are off by $5,000 on average. Mean Absolute Percentage Error (MAPE) expresses accuracy as a percentage, making it easier to compare across data sources with different scales — a MAPE of 8% means predictions are within 8% of actual values on average. Root Mean Square Error (RMSE) penalizes large errors more heavily than MAE, which is useful when occasional big misses are more costly than consistent small ones. All three metrics are displayed per forecast on KPI cards or in a comparison table.

## Method Comparison

The method comparison view groups forecasts by data source and shows accuracy metrics side by side for each method applied to the same data. For example, if you have SalesRevenue forecasts using SMA, WMA, Exponential Smoothing, and Linear Regression, this view reveals which method performs best on that particular data source. A bar chart or heatmap ranks methods by MAPE, making it visually obvious whether Exponential Smoothing with alpha=0.3 outperforms a 6-period SMA. Use this view to decide which method to keep active and which to archive or reconfigure.

## Upcoming Evaluations

The upcoming evaluations section lists active forecasts whose next evaluation date is approaching. Each entry shows the forecast name, data source, method, and the date when enough actual data will be available to evaluate the next forecast period. This helps you plan ahead — if a quarterly SalesRevenue forecast is due for evaluation next week, you know to check back for updated accuracy metrics. Forecasts with scheduled Hangfire evaluation jobs show their next run time here as well.

## Recently Evaluated

The recently evaluated section shows forecasts that have completed an evaluation run within the last 30 days. Each entry displays the evaluation date, the accuracy metrics from that run, and the change in accuracy compared to the previous evaluation. An improving trend (decreasing MAE/MAPE/RMSE) appears in green, while a degrading trend appears in red. This lets you quickly spot forecasts that are losing accuracy and may need parameter adjustments or a method change.

## Taking Action

The Forecast Analytics Dashboard is designed to drive decisions. When you identify a method that consistently outperforms others for a given data source, archive the underperforming forecasts and keep only the best one active. When a forecast's accuracy is degrading, investigate whether the underlying data patterns have shifted — perhaps a seasonal trend has changed, or a one-time event has skewed recent actuals. In that case, create a new forecast with an adjusted lookback period or a different method and compare it against the existing one before promoting it to active status.
