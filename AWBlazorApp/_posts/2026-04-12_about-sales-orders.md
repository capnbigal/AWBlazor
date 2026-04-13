---
title: About Sales Orders
summary: Understanding the SalesOrderHeader and SalesOrderDetail structure in AdventureWorks.
tags: [sales, reference-data]
category: entity-guide
author: Elementary App
---

Sales orders are the central transactional records in the AdventureWorks Sales schema. Every customer purchase is represented by a `Sales.SalesOrderHeader` row linked to one or more `Sales.SalesOrderDetail` rows. Together, these tables capture the full lifecycle of an order from initial placement through shipment and payment, and they serve as the primary data source for the Sales Analytics Dashboard and SalesRevenue forecasts.

## SalesOrderHeader

The header record contains order-level information: the order date, due date, ship date, customer, salesperson, territory, shipping method, and financial totals. The `SalesOrderNumber` is a computed column formatted as `SO` followed by the `SalesOrderID` (e.g., SO43659). Key financial fields include `SubTotal` (sum of line items before tax and freight), `TaxAmt`, `Freight`, and `TotalDue` (the grand total the customer owes). The `OnlineOrderFlag` distinguishes web orders from salesperson-assisted orders, which affects how territory credit and commission calculations work.

## Order Status Codes

Every order carries a `Status` integer that tracks its position in the fulfillment pipeline. Status 1 means In Process — the order has been placed but not yet picked or packed. Status 2 is Approved, indicating the order has passed credit and inventory checks. Status 3 is Backordered, meaning one or more line items could not be fulfilled from current inventory. Status 4 is Rejected, used when an order fails validation or is cancelled before shipment. Status 5 is Shipped, confirming the order has left the warehouse with a populated `ShipDate`. Status 6 is Cancelled, a terminal state for orders withdrawn after approval. The Sales Analytics Dashboard filters and groups by these statuses to show pipeline health and fulfillment rates.

## SalesOrderDetail

Each line item on an order is a `SalesOrderDetail` row. It references the `ProductID` being sold, the `OrderQty`, the `UnitPrice`, the `UnitPriceDiscount` (as a decimal fraction), and the computed `LineTotal`. The `SpecialOfferID` links to any active promotion applied to the line. `CarrierTrackingNumber` is populated at shipment time and can be used to trace individual packages. The detail rows are what you see when you expand a sales order row in the reference data grid — they show exactly which products were ordered, in what quantities, and at what prices.

## Relationships

Sales orders connect to several other entities in AdventureWorks. The `CustomerID` on the header links to `Sales.Customer`, which in turn references either a `Person.Person` (individual buyer) or a `Sales.Store` (business buyer). The `SalesPersonID` ties to `Sales.SalesPerson`, extending `HumanResources.Employee`, and determines commission tracking and quota assignments. The `TerritoryID` maps to `Sales.SalesTerritory`, which groups orders geographically for regional analytics. The `ShipMethodID` references `Purchasing.ShipMethod` for carrier and rate information. All of these relationships are navigable through the expandable drill-through rows in the reference data pages.

## How Sales Data Feeds Analytics

The Sales Analytics Dashboard aggregates `SalesOrderHeader` data to compute KPIs like total revenue, average order value, and order count by period. Territory breakdowns use the `TerritoryID` join to show which regions are performing above or below target. The quota-versus-actual comparison pulls from `Sales.SalesPersonQuotaHistory` and matches it against summed `TotalDue` values per salesperson per quarter. When you create a SalesRevenue forecast, the forecasting engine queries these same tables, grouping `TotalDue` by month or quarter to build the historical time series that feeds into your chosen statistical method.
