using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Api; using AWBlazorApp.Features.Sales.CreditCards.Api; using AWBlazorApp.Features.Sales.Currencies.Api; using AWBlazorApp.Features.Sales.CurrencyRates.Api; using AWBlazorApp.Features.Sales.Customers.Api; using AWBlazorApp.Features.Sales.PersonCreditCards.Api; using AWBlazorApp.Features.Sales.SalesOrderDetails.Api; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Api; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Api; using AWBlazorApp.Features.Sales.SalesPeople.Api; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Api; using AWBlazorApp.Features.Sales.SalesReasons.Api; using AWBlazorApp.Features.Sales.SalesTaxRates.Api; using AWBlazorApp.Features.Sales.SalesTerritories.Api; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Api; using AWBlazorApp.Features.Sales.ShoppingCartItems.Api; using AWBlazorApp.Features.Sales.SpecialOffers.Api; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Api; using AWBlazorApp.Features.Sales.Stores.Api; 
using AWBlazorApp.Features.Production.Boms.Api; using AWBlazorApp.Features.Production.Cultures.Api; using AWBlazorApp.Features.Production.Documents.Api; using AWBlazorApp.Features.Production.Illustrations.Api; using AWBlazorApp.Features.Production.Locations.Api; using AWBlazorApp.Features.Production.ProductCategories.Api; using AWBlazorApp.Features.Production.ProductCostHistories.Api; using AWBlazorApp.Features.Production.ProductDescriptions.Api; using AWBlazorApp.Features.Production.ProductDocuments.Api; using AWBlazorApp.Features.Production.ProductInventories.Api; using AWBlazorApp.Features.Production.ProductListPriceHistories.Api; using AWBlazorApp.Features.Production.ProductModels.Api; using AWBlazorApp.Features.Production.ProductModelIllustrations.Api; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Api; using AWBlazorApp.Features.Production.ProductPhotos.Api; using AWBlazorApp.Features.Production.ProductProductPhotos.Api; using AWBlazorApp.Features.Production.ProductReviews.Api; using AWBlazorApp.Features.Production.Products.Api; using AWBlazorApp.Features.Production.ProductSubcategories.Api; using AWBlazorApp.Features.Production.ScrapReasons.Api; using AWBlazorApp.Features.Production.TransactionHistories.Api; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Api; using AWBlazorApp.Features.Production.UnitMeasures.Api; using AWBlazorApp.Features.Production.WorkOrders.Api; using AWBlazorApp.Features.Production.WorkOrderRoutings.Api; 
using AWBlazorApp.Features.HumanResources.Departments.Api; using AWBlazorApp.Features.HumanResources.Employees.Api; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Api; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Api; using AWBlazorApp.Features.HumanResources.JobCandidates.Api; using AWBlazorApp.Features.HumanResources.Shifts.Api; 
using AWBlazorApp.Features.Purchasing.ProductVendors.Api; using AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Api; using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Api; using AWBlazorApp.Features.Purchasing.ShipMethods.Api; using AWBlazorApp.Features.Purchasing.Vendors.Api; 
using AWBlazorApp.Features.Person.Addresses.Api; using AWBlazorApp.Features.Person.AddressTypes.Api; using AWBlazorApp.Features.Person.BusinessEntities.Api; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Api; using AWBlazorApp.Features.Person.BusinessEntityContacts.Api; using AWBlazorApp.Features.Person.ContactTypes.Api; using AWBlazorApp.Features.Person.CountryRegions.Api; using AWBlazorApp.Features.Person.EmailAddresses.Api; using AWBlazorApp.Features.Person.Persons.Api; using AWBlazorApp.Features.Person.PersonPhones.Api; using AWBlazorApp.Features.Person.PhoneNumberTypes.Api; using AWBlazorApp.Features.Person.StateProvinces.Api; 
using AWBlazorApp.Features.Forecasting.Api;
using AWBlazorApp.Features.ProcessManagement.Api;
using AWBlazorApp.Features.ToolSlots.Api;
using AWBlazorApp.Features.Gallery.Api;
using AWBlazorApp.Features.UserGuide.Services;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Features.Admin.Api;
using AWBlazorApp.Features.Enterprise.Assets.Api; using AWBlazorApp.Features.Enterprise.CostCenters.Api; using AWBlazorApp.Features.Enterprise.OrgUnits.Api; using AWBlazorApp.Features.Enterprise.Organizations.Api; using AWBlazorApp.Features.Enterprise.ProductLines.Api; using AWBlazorApp.Features.Enterprise.Stations.Api; 

namespace AWBlazorApp.App.Routing;

public static class EndpointMappingExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHelloEndpoints();
        app.MapAdminEndpoints();
        app.MapForecastEndpoints();
        app.MapProcessEndpoints();
        app.MapToolSlotConfigurationEndpoints();
        app.MapUserEndpoints();

        // AdventureWorks reference-data endpoints — one group per table, each with its own
        // audit history endpoint.
        app.MapAddressTypeEndpoints();
        app.MapContactTypeEndpoints();
        app.MapCountryRegionEndpoints();
        app.MapPhoneNumberTypeEndpoints();
        app.MapCultureEndpoints();
        app.MapProductCategoryEndpoints();
        app.MapScrapReasonEndpoints();
        app.MapUnitMeasureEndpoints();
        app.MapCurrencyEndpoints();
        app.MapSalesReasonEndpoints();
        app.MapDepartmentEndpoints();
        app.MapShiftEndpoints();

        // Batch 2.
        app.MapLocationEndpoints();
        app.MapShipMethodEndpoints();
        app.MapProductSubcategoryEndpoints();
        app.MapProductDescriptionEndpoints();
        app.MapSpecialOfferEndpoints();
        app.MapStateProvinceEndpoints();
        app.MapSalesTerritoryEndpoints();
        app.MapSalesTaxRateEndpoints();
        app.MapShoppingCartItemEndpoints();

        // Batch 3 — business entities + composite-key tables.
        app.MapCustomerEndpoints();
        app.MapSalesPersonEndpoints();
        app.MapWorkOrderEndpoints();
        app.MapBillOfMaterialsEndpoints();
        app.MapCurrencyRateEndpoints();
        app.MapSalesPersonQuotaHistoryEndpoints();
        app.MapSalesOrderHeaderSalesReasonEndpoints();
        app.MapProductCostHistoryEndpoints();
        app.MapCountryRegionCurrencyEndpoints();
        app.MapEmployeeDepartmentHistoryEndpoints();

        // Batch 4 — Person hierarchy (Address, BusinessEntity, Person + dependents).
        app.MapAddressEndpoints();
        app.MapBusinessEntityEndpoints();
        app.MapPersonEndpoints();
        app.MapEmailAddressEndpoints();
        app.MapPersonPhoneEndpoints();
        app.MapBusinessEntityAddressEndpoints();
        app.MapBusinessEntityContactEndpoints();

        // Batch 5 — Production / Product hierarchy.
        app.MapProductEndpoints();
        app.MapProductModelEndpoints();
        app.MapIllustrationEndpoints();
        app.MapProductPhotoEndpoints();
        app.MapProductReviewEndpoints();
        app.MapProductInventoryEndpoints();
        app.MapProductListPriceHistoryEndpoints();
        app.MapProductProductPhotoEndpoints();

        // Batch 6 — Production junction + transaction tables.
        app.MapProductModelIllustrationEndpoints();
        app.MapProductModelProductDescriptionCultureEndpoints();
        app.MapWorkOrderRoutingEndpoints();
        app.MapTransactionHistoryEndpoints();
        app.MapTransactionHistoryArchiveEndpoints();

        // Batch 7 — Sales: CreditCard, PersonCreditCard, SalesOrderHeader/Detail, SalesTerritoryHistory, SpecialOfferProduct, Store.
        app.MapCreditCardEndpoints();
        app.MapPersonCreditCardEndpoints();
        app.MapSalesOrderHeaderEndpoints();
        app.MapSalesOrderDetailEndpoints();
        app.MapSalesTerritoryHistoryEndpoints();
        app.MapSpecialOfferProductEndpoints();
        app.MapStoreEndpoints();

        // Batch 8 — Purchasing + HR entities.
        app.MapVendorEndpoints();
        app.MapProductVendorEndpoints();
        app.MapPurchaseOrderHeaderEndpoints();
        app.MapPurchaseOrderDetailEndpoints();
        app.MapEmployeeEndpoints();
        app.MapEmployeePayHistoryEndpoints();
        app.MapJobCandidateEndpoints();

        // Batch 9 — Production: Document + ProductDocument (hierarchyid PK support).
        app.MapDocumentEndpoints();
        app.MapProductDocumentEndpoints();

        // Batch 10 — Enterprise master data (org schema, EF-managed).
        app.MapOrganizationEndpoints();
        app.MapOrgUnitEndpoints();
        app.MapStationEndpoints();
        app.MapAssetEndpoints();
        app.MapCostCenterEndpoints();
        app.MapProductLineEndpoints();

        // Batch 11 — Advanced inventory (inv schema). Transaction writes go through IInventoryService.
        AWBlazorApp.Features.Inventory.Items.Api.InventoryItemEndpoints.MapInventoryItemEndpoints(app);
        AWBlazorApp.Features.Inventory.Locations.Api.InventoryLocationEndpoints.MapInventoryLocationEndpoints(app);
        AWBlazorApp.Features.Inventory.Lots.Api.LotEndpoints.MapLotEndpoints(app);
        AWBlazorApp.Features.Inventory.Serials.Api.SerialUnitEndpoints.MapSerialUnitEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.InventoryTransactionEndpoints.MapInventoryTransactionEndpoints(app);
        AWBlazorApp.Features.Inventory.Adjustments.Api.InventoryAdjustmentEndpoints.MapInventoryAdjustmentEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.InventoryReadOnlyEndpoints.MapInventoryReadOnlyEndpoints(app);

        // Batch 12 — Logistics (lgx schema). Post workflows wire into IInventoryService.
        AWBlazorApp.Features.Logistics.Receipts.Api.GoodsReceiptEndpoints.MapGoodsReceiptEndpoints(app);
        AWBlazorApp.Features.Logistics.Shipments.Api.ShipmentEndpoints.MapShipmentEndpoints(app);
        AWBlazorApp.Features.Logistics.Transfers.Api.StockTransferEndpoints.MapStockTransferEndpoints(app);

        // Batch 13 — MES (mes schema). Production-run completion writes WIP_ISSUE/WIP_RECEIPT via IInventoryService.
        AWBlazorApp.Features.Mes.Runs.Api.ProductionRunEndpoints.MapProductionRunEndpoints(app);
        AWBlazorApp.Features.Mes.Api.ShopFloorEndpoints.MapOperatorClockEventEndpoints(app);
        AWBlazorApp.Features.Mes.Api.ShopFloorEndpoints.MapDowntimeEventEndpoints(app);
        AWBlazorApp.Features.Mes.Api.ShopFloorEndpoints.MapDowntimeReasonEndpoints(app);
        AWBlazorApp.Features.Mes.Instructions.Api.WorkInstructionEndpoints.MapWorkInstructionEndpoints(app);

        // Batch 14 — Quality (qa schema). NCR disposition writes inv transactions via IInventoryService.
        AWBlazorApp.Features.Quality.Plans.Api.InspectionPlanEndpoints.MapInspectionPlanEndpoints(app);
        AWBlazorApp.Features.Quality.Inspections.Api.InspectionEndpoints.MapInspectionEndpoints(app);
        AWBlazorApp.Features.Quality.Ncrs.Api.NonConformanceEndpoints.MapNonConformanceEndpoints(app);
        AWBlazorApp.Features.Quality.Capa.Api.CapaCaseEndpoints.MapCapaCaseEndpoints(app);

        // Batch 15 — Workforce (wf schema). Training, qualifications, attendance/leave, comms.
        AWBlazorApp.Features.Workforce.Api.TrainingEndpoints.MapTrainingEndpoints(app);
        AWBlazorApp.Features.Workforce.Api.QualificationEndpoints.MapQualificationEndpoints(app);
        AWBlazorApp.Features.Workforce.Api.AttendanceLeaveEndpoints.MapAttendanceLeaveEndpoints(app);
        AWBlazorApp.Features.Workforce.Api.CommunicationEndpoints.MapCommunicationEndpoints(app);

        // Batch 16 — Engineering (eng schema). Routings, BOMs, ECOs, docs, deviations.
        AWBlazorApp.Features.Engineering.Routings.Api.RoutingEndpoints.MapRoutingEndpoints(app);
        AWBlazorApp.Features.Engineering.Boms.Api.BomEndpoints.MapBomEndpoints(app);
        AWBlazorApp.Features.Engineering.Ecos.Api.EngineeringChangeOrderEndpoints.MapEngineeringChangeOrderEndpoints(app);
        AWBlazorApp.Features.Engineering.Api.DocumentDeviationEndpoints.MapDocumentDeviationEndpoints(app);

        // Batch 17 — Maintenance (maint schema). Asset profiles, PM schedules, work orders, spares.
        AWBlazorApp.Features.Maintenance.AssetProfiles.Api.AssetMaintenanceEndpoints.MapAssetMaintenanceEndpoints(app);
        AWBlazorApp.Features.Maintenance.PmSchedules.Api.PmScheduleEndpoints.MapPmScheduleEndpoints(app);
        AWBlazorApp.Features.Maintenance.WorkOrders.Api.MaintenanceWorkOrderEndpoints.MapMaintenanceWorkOrderEndpoints(app);
        AWBlazorApp.Features.Maintenance.SpareParts.Api.SparePartEndpoints.MapSparePartEndpoints(app);

        // Batch 18 — Performance (perf schema). OEE, metric rollups, KPIs, scorecards, reports.
        AWBlazorApp.Features.Performance.ProductionMetrics.Api.MetricsEndpoints.MapMetricsEndpoints(app);
        AWBlazorApp.Features.Performance.Kpis.Api.KpiEndpoints.MapKpiEndpoints(app);
        AWBlazorApp.Features.Performance.Scorecards.Api.ScorecardEndpoints.MapScorecardEndpoints(app);
        AWBlazorApp.Features.Performance.Reports.Api.ReportEndpoints.MapReportEndpoints(app);

        // Cross-module plant dashboard. Aggregates from every module schema.
        AWBlazorApp.Features.Dashboard.Api.DashboardEndpoints.MapDashboardEndpoints(app);

        // CSV export endpoint (Admin only).
        app.MapExportEndpoints();

        return app;
    }
}
