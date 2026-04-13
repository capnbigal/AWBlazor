---
title: About Products
summary: Understanding the Product entity, its lifecycle, and its relationships in AdventureWorks.
tags: [production, reference-data]
category: entity-guide
author: Elementary App
---

The `Production.Product` table is one of the most interconnected entities in AdventureWorks2022. Every item that Adventure Works Cycles manufactures or resells is represented here, from finished bicycles to individual components like chains, pedals, and handlebars. Elementary App exposes the full product catalog through the Reference Data section, with sortable grids, expandable drill-throughs, and cross-entity navigation links to related production, sales, and inventory records.

## Product Lifecycle

Each product follows a lifecycle governed by three date columns. `SellStartDate` marks when the product first became available for sale — it must be populated for every product. `SellEndDate` is set when the product is withdrawn from active sale, typically when a newer model replaces it. `DiscontinuedDate` is the final stage, indicating the product will no longer be manufactured or stocked. A product with a `SellStartDate` in the past, no `SellEndDate`, and no `DiscontinuedDate` is currently active. The Production Analytics Dashboard uses these dates to show product lifecycle metrics, including how many products are active, approaching end-of-sale, or fully discontinued.

## Categories and Subcategories

Products are organized into a two-level hierarchy. `Production.ProductCategory` contains top-level groupings like Bikes, Components, Clothing, and Accessories. Each category contains multiple `Production.ProductSubcategory` entries — for example, the Bikes category includes Mountain Bikes, Road Bikes, and Touring Bikes. The `ProductSubcategoryID` on `Product` links to the subcategory, and from there to the category. Not all products have a subcategory assignment; raw materials and internal-use components may have a null `ProductSubcategoryID`. The reference data pages let you filter and group by category and subcategory, and the drill-through rows show the full hierarchy path.

## Cost vs. List Price

Two price columns define the economics of each product. `StandardCost` is the internal manufacturing or procurement cost — what it costs Adventure Works to produce or buy the item. `ListPrice` is the suggested retail price charged to customers. The margin between these two values drives profitability analysis. Products with a `ListPrice` of zero are typically internal components not sold individually. The `Production.ProductCostHistory` table tracks how `StandardCost` has changed over time, which is useful for understanding material cost trends. Similarly, `Production.ProductListPriceHistory` captures list price adjustments. Both history tables are accessible through the expandable drill-through on the product detail row.

## Related Entities

A product connects outward to many other tables. `Production.ProductInventory` tracks current stock quantities by location and shelf. `Production.WorkOrder` records manufacturing jobs that produce the product, including planned and actual quantities and scrap counts. `Production.BillOfMaterials` defines the component tree — which sub-products are assembled to create a finished product. On the sales side, `Sales.SalesOrderDetail` shows every order line that included the product, and `Sales.ShoppingCartItem` captures in-progress web cart additions. `Production.ProductReview` contains customer-submitted ratings and comments. All of these related records appear in the expandable drill-through rows when you view a product in the reference data grid.

## Product Model and Description

The `ProductModelID` column links to `Production.ProductModel`, which groups products that share a design or specification — for instance, multiple sizes or colors of the same bike frame. Each product model can have multiple `Production.ProductModelProductDescriptionCulture` records that provide localized marketing descriptions in different languages. These descriptions are stored in `Production.ProductDescription` and are useful for understanding what each product line represents. The reference data pages display the model name alongside the product name and include the description text in the drill-through detail panel.
