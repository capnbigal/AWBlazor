---
title: About AdventureWorks Reference Data
summary: An overview of the AdventureWorks2022 schemas and how AWBlazor exposes them.
tags: [reference-data]
category: entity-guide
author: AWBlazor
---

AdventureWorks2022 is Microsoft's sample database for SQL Server, modeling a fictional bicycle manufacturer called Adventure Works Cycles. It covers the full spectrum of business operations — sales, production, purchasing, human resources, and person/contact management — spread across six schemas with over 70 tables. AWBlazor maps these tables to EF Core entities and exposes them through more than 90 reference data pages with full CRUD capabilities, sortable/filterable MudDataGrid tables, and expandable row drill-throughs.

## The Six Schemas

The database is organized into schemas that correspond to business domains. The `Sales` schema contains orders, customers, territories, salespeople, quotas, currencies, and shopping cart data. The `Production` schema holds products, work orders, bill of materials, inventory, product photos, and manufacturing transaction history. The `Purchasing` schema covers vendors, purchase orders, and ship methods. The `HumanResources` schema manages employees, departments, shifts, and pay history. The `Person` schema stores people, addresses, email addresses, phone numbers, and contact types — it serves as a shared contact directory used by both Sales and HR. Finally, the `dbo` schema contains cross-cutting utility tables like `ErrorLog` and the externally managed `ToolSlotConfigurations`.

## Reference Data Pages

Each AdventureWorks table has a dedicated page in the Reference Data section of the navigation drawer, grouped by schema. The pages use MudDataGrid with server-side sorting, filtering, and pagination powered by EF Core queries. Columns are configurable — you can show or hide them, reorder them, and apply per-column filters. The grids support multi-column sorting by holding Shift while clicking column headers. Inline editing is available for tables where updates are meaningful, and new rows can be added through dialog forms.

## Expandable Row Drill-Throughs

The most distinctive feature of the reference data pages is the expandable row. Clicking the expand arrow on any row reveals a detail panel showing related data from other tables. For example, expanding a Customer row shows their recent orders, addresses, and contact information. Expanding a Product row shows inventory levels, work orders, bill of materials components, cost history, and sales order lines that include that product. These drill-throughs are implemented as nested MudDataGrid instances or detail panels that lazy-load their data when expanded, so they do not slow down the initial page load.

## Cross-Entity Navigation Links

Within the drill-through panels, entity names and IDs are rendered as clickable links that navigate to the related entity's reference data page, pre-filtered to that specific record. Clicking a ProductID in a SalesOrderDetail drill-through takes you to the Products page filtered to that product. Clicking a SalesPersonID navigates to the SalesPerson page. This cross-entity linking lets you follow relationships through the data model without manually searching — you can trace a path from a sales order to its customer, to the customer's territory, to other salespeople in that territory, and to their quota history, all through a chain of clicks.

## Practical Value

The reference data pages serve multiple purposes. For analysts, they provide a quick way to look up specific records without writing SQL queries. For developers, they act as a visual data dictionary — browsing the grids reveals column names, data types, and relationships faster than reading entity class files. For testers, they offer a way to verify that seed data and migration changes have been applied correctly. And for administrators, the CRUD capabilities allow correcting data issues or adding test records without connecting to SQL Server Management Studio.

## Search Integration

All reference data entities are indexed by the global search feature (Ctrl+K). Typing a product name, customer account number, or order number into the search box returns matching records with links directly to their reference data pages. The search queries the database in real time, so it always reflects the current state of the data. This is often the fastest way to find a specific record when you know its name or identifier but do not want to navigate through the menu hierarchy.
