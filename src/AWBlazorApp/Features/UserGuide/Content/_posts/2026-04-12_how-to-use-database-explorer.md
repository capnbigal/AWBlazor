---
title: Using the Database Explorer
summary: How to explore table sizes, schema distribution, and per-schema details in the Database Explorer.
tags: [database-explorer]
category: how-to
author: AWBlazor
---

The Database Explorer provides a structural overview of the AdventureWorks2022 database directly within AWBlazor. Accessible from the My workspace section in the navigation drawer as "Database explorer", it shows table counts, row counts, and size information organized by schema. This is useful for understanding the shape of the data you are working with, identifying large tables that may need indexing attention, and getting a quick inventory of what exists in the database.

## Schema Distribution

The top section of the Database Explorer shows a chart or summary of how tables are distributed across the AdventureWorks schemas: `dbo`, `HumanResources`, `Person`, `Production`, `Purchasing`, and `Sales`. Each schema is displayed with its table count, giving you an immediate sense of which areas of the database are the most complex. Production and Sales typically have the most tables, reflecting the depth of manufacturing and order management data in AdventureWorks.

## Top 10 Tables by Size

A ranked list shows the ten largest tables in the database, measured by row count or disk space. This view helps you identify which tables dominate storage and query performance. In a typical AdventureWorks deployment, `Sales.SalesOrderDetail`, `Production.TransactionHistory`, and `Person.Person` are among the largest. Knowing which tables are big is important when writing queries, building forecasts, or planning index maintenance — operations on million-row tables behave differently than those on small lookup tables.

## Per-Schema Drill-Down

Clicking on a schema name in the distribution chart opens a detailed view of every table in that schema. Each table entry shows its name, row count, column count, and any indexes defined on it. This is effectively a lightweight version of SQL Server Management Studio's Object Explorer, accessible without leaving the browser. You can sort by row count to find the largest table in a schema, or by column count to identify wide tables that may benefit from column pruning or vertical partitioning.

## CSV Export (Admin Only)

Users in the Admin role can export the Database Explorer data to CSV. The export button appears in the toolbar and generates a file containing every table's schema, name, row count, and size metrics. This is useful for documentation, capacity planning reports, or as input to database governance processes. Non-admin users can view all the same information on screen but cannot export it — this restriction prevents casual distribution of database structure details outside the organization.

## Practical Uses

The Database Explorer is a starting point for several common tasks. When building a new forecast, check the row count of the underlying table to understand how much historical data is available. When investigating slow dashboard loads, look at the top tables to see if a large table might need an additional index. When onboarding a new team member, walk through the schema distribution to explain how AdventureWorks is organized. The explorer is read-only — it queries system catalog views and never modifies any data or schema objects.
