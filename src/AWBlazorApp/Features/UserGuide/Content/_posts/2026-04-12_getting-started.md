---
title: Getting Started with AWBlazor
summary: A quick orientation to AWBlazor's features, navigation, and seed accounts.
tags: [getting-started]
category: getting-started
author: AWBlazor
---

AWBlazor is a .NET 10 Blazor Server application built on top of the AdventureWorks2022 SQL Server database. It provides interactive analytics dashboards, forecasting tools, reference data management for over 90 AdventureWorks entities, and administrative utilities — all wrapped in a MudBlazor 9 UI with dark mode support. Whether you are exploring sales trends, managing production work orders, or building time-series forecasts, AWBlazor gives you a single place to work with your data.

## Navigating the App

The left-hand navigation drawer is organized into seven sections so you can find what you need without scanning one long list. Overview at the top holds Home, the Plant dashboard, and this User guide. Operations contains the eleven domain modules — Enterprise, Engineering, Production, Quality, Maintenance, Workforce, Inventory, Logistics, Purchasing, Sales — each with a summary page first, a domain analytics dashboard if one exists, workflow pages, and a nested "Reference data" subgroup for raw AdventureWorks tables. Insights holds cross-domain planning and analytics — Forecasts, Processes, Geographic map, and Performance. My workspace holds your personal pages (My activity, Notification rules, Database explorer, Document tree). Admin and Developer sections only appear for users in the Admin role. Account sits at the bottom and links to your account settings; Sign out is in the app bar header.

## Auto-expand behaviour

Each top-level Operations group auto-expands when your current route matches it. Navigating to `/engineering/ecos` automatically opens the Engineering group; navigating to `/maintenance/work-orders` opens Maintenance. Nested "Reference data" subgroups inside each domain start collapsed so the top of each group shows only the workflow pages you use day-to-day.

## Key Features at a Glance

Global search is accessible from any page with the Ctrl+K shortcut. It searches across entities and navigation items, letting you jump directly to a specific customer, product, or page without drilling through menus. Dark mode can be toggled from the app bar — the setting persists across sessions via a cookie. Every analytics dashboard includes date-range selectors, KPI summary cards, and interactive charts. CSV export is available on most data grids and dashboards for users who need to pull data into Excel or other tools.

## Seed User Accounts

When the application starts for the first time, the DatabaseInitializer seeds four user accounts so you can explore immediately. The default admin account uses the email `admin@email.com` with the password `p@55wOrd`. Three additional users are created with the Employee role: `manager@email.com`, `employee@email.com`, and `newuser@email.com`, all sharing the same default password. Admin users see the Admin and Developer sections in the navigation; Employee users see everything except those two sections.

## What to Explore First

If you are new to the app, start with the Plant dashboard under Overview for a live view of shop-floor state. From there, open the Sales analytics dashboard under Operations → Sales to see revenue trends and territory breakdowns. Try opening Operations → Inventory → Product explorer and expanding a row to see related data. Once you are comfortable with navigation, create your first forecast from Insights → Forecasts using the Simple Moving Average method on the SalesRevenue data source. Finally, visit Account > My account > API Keys to generate a key and try calling the REST API endpoints with an HTTP client.

## Where to Learn More

This user guide covers every major module in the app. Start from the guide index at `/guide` and filter by category or tag to find what you need. The "About the Navigation" article is a good next stop; after that, each domain has an "About" article that orients you to its workflow pages, and most have "How to" articles for specific tasks.
