---
title: About the Navigation
summary: How the sidebar is organized — Overview, Operations, Insights, My workspace, Admin, Developer, and Account.
tags: [navigation, getting-started]
category: entity-guide
author: AWBlazor
---

AWBlazor's left-hand navigation drawer is organized into seven top-level sections so you can find pages without scanning a single long list. Each section reflects a different kind of work: daily operational screens live in one place, cross-domain analytics in another, your personal tools and notifications in a third, and administrative controls in clearly separated Admin and Developer sections.

## Overview

The first three links — Home, Plant dashboard, and User guide — sit above every group. Home is the landing page with quick links and recent activity. Plant dashboard at `/dashboard/plant` is the shop-floor overview of runs, downtime, and alerts. The user guide is where you are right now.

## Operations

Operations is where most of your day will happen. It contains eleven domain groups — Enterprise, Engineering, Production, Quality, Maintenance, Workforce, Inventory, Logistics, Purchasing, and Sales — each following the same pattern. The summary page comes first, then any analytics dashboard specific to that domain, then the workflow pages, and finally a nested "Reference data" subgroup that contains the raw AdventureWorks tables. The Reference data subgroup starts collapsed so the top of each group shows only what you need day-to-day. Each top-level Operations group auto-expands when your current route matches it, so navigating to `/engineering/ecos` opens the Engineering group automatically.

## Insights

Insights is where cross-domain planning and analytics live. Forecasts and Processes each have both a configuration page and a matching analytics dashboard. Below them is the Geographic map for spatial exploration and the Performance group, which contains OEE snapshots, KPIs, scorecards, and performance reports. Performance sits in Insights rather than Operations because it reports across the factory, not against one module.

## My workspace

My workspace is your personal section. It contains My activity (a log of your actions), Notification rules (which events ping you), the Database explorer, and the Document tree. Admin users see an extra "Admin tools" sub-section below that, with Saved queries, the KPI editor, the Insights dashboard, and Scheduled reports — these are admin-only because they run unbounded queries.

## Admin, Developer, and Account

Admin-only users see two additional sections below My workspace. Admin holds user management, permissions, request logs, and rollup tools. Developer holds the API explorer, Swagger, CSV import, and Hangfire — tools for integrating or operating the app rather than using it. Finally, the Account section at the bottom holds a single link to your account settings; sign-out is in the app bar header.
