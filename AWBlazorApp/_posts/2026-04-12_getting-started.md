---
title: Getting Started with AWBlazor
summary: A quick orientation to AWBlazor's features, navigation, and seed accounts.
tags: [getting-started]
category: getting-started
author: AWBlazor
---

AWBlazor is a .NET 10 Blazor Server application built on top of the AdventureWorks2022 SQL Server database. It provides interactive analytics dashboards, forecasting tools, reference data management for over 90 AdventureWorks entities, and administrative utilities — all wrapped in a MudBlazor 9 UI with dark mode support. Whether you are exploring sales trends, managing production work orders, or building time-series forecasts, AWBlazor gives you a single place to work with your data.

## Navigating the App

The left-hand navigation drawer organizes features into logical groups. At the top you will find the Home page and the Admin Dashboard (visible to users in the Admin role). Below that, the Analytics section links to Sales, Production, HR, Purchasing, and Forecast dashboards. The Reference Data section contains pages for every AdventureWorks schema — Sales, Production, Person, HumanResources, and Purchasing — each with sortable, filterable MudDataGrid tables and expandable row drill-throughs. The Database Explorer lives under the Reports section and gives you a bird's-eye view of table sizes and schema distribution.

## Key Features at a Glance

Global Search is accessible from any page with the Ctrl+K shortcut. It searches across entities and navigation items, letting you jump directly to a specific customer, product, or page without drilling through menus. Dark mode can be toggled from the toolbar — the setting persists across sessions. Every analytics dashboard includes date-range selectors, KPI summary cards, and interactive charts. CSV export is available on most data grids and dashboards for users who need to pull data into Excel or other tools.

## Seed User Accounts

When the application starts for the first time, the DatabaseInitializer seeds four user accounts so you can explore immediately. The default admin account uses the email `admin@email.com` with the password `p@55wOrd`. Three additional users are created with the Employee role: `manager@email.com`, `employee@email.com`, and `newuser@email.com`, all sharing the same default password. Admin users have access to the Admin Dashboard, Database Explorer CSV export, user management, and Hangfire job monitoring. Employee users can access all analytics dashboards, reference data pages, and create their own API keys and forecasts.

## Technical Requirements

AWBlazor requires a SQL Server instance hosting the AdventureWorks2022 database. The application connects via the connection string configured in `appsettings.json` (production) or `appsettings.Development.json` (development). The server named SHOOSHEE hosts both the production and dev databases. No additional infrastructure is needed — there is no NPM toolchain, no separate front-end build step, and no external service dependencies beyond SQL Server. Simply run `dotnet run --project AWBlazorApp` and navigate to https://localhost:5001.

## What to Explore First

If you are new to the app, start with the Sales Analytics Dashboard to see revenue trends and territory breakdowns. Then visit the Reference Data section and open the Products page — try expanding a row to see subcategory details, cost history, and related sales data. Once you are comfortable with navigation, create your first forecast from the Forecasts page using the Simple Moving Average method on the SalesRevenue data source. Finally, visit Account > Manage > API Keys to generate a key and try calling the REST API endpoints with an HTTP client.
