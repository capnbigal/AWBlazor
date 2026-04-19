using AWBlazorApp.Features.Sales.Api;
using AWBlazorApp.Features.Production.Api;
using AWBlazorApp.Features.HumanResources.Api;
using AWBlazorApp.Features.Purchasing.Api;
using AWBlazorApp.Features.Person.Api;
using AWBlazorApp.Features.Forecasting.Api;
using AWBlazorApp.Features.ProcessManagement.Api;
using AWBlazorApp.Features.ToolSlots.Api;
using AWBlazorApp.Features.Gallery.Api;
using AWBlazorApp.Features.UserGuide.Services;
using AWBlazorApp.Shared.Api;
using AWBlazorApp.Features.Admin.Api;
using AWBlazorApp.Features.Enterprise.Api;

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
        AWBlazorApp.Features.Inventory.Api.InventoryItemEndpoints.MapInventoryItemEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.InventoryLocationEndpoints.MapInventoryLocationEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.LotEndpoints.MapLotEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.SerialUnitEndpoints.MapSerialUnitEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.InventoryTransactionEndpoints.MapInventoryTransactionEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.InventoryAdjustmentEndpoints.MapInventoryAdjustmentEndpoints(app);
        AWBlazorApp.Features.Inventory.Api.InventoryReadOnlyEndpoints.MapInventoryReadOnlyEndpoints(app);

        // Batch 12 — Logistics (lgx schema). Post workflows wire into IInventoryService.
        AWBlazorApp.Features.Logistics.Api.GoodsReceiptEndpoints.MapGoodsReceiptEndpoints(app);
        AWBlazorApp.Features.Logistics.Api.ShipmentEndpoints.MapShipmentEndpoints(app);
        AWBlazorApp.Features.Logistics.Api.StockTransferEndpoints.MapStockTransferEndpoints(app);

        // Batch 13 — MES (mes schema). Production-run completion writes WIP_ISSUE/WIP_RECEIPT via IInventoryService.
        AWBlazorApp.Features.Mes.Api.ProductionRunEndpoints.MapProductionRunEndpoints(app);
        AWBlazorApp.Features.Mes.Api.ShopFloorEndpoints.MapOperatorClockEventEndpoints(app);
        AWBlazorApp.Features.Mes.Api.ShopFloorEndpoints.MapDowntimeEventEndpoints(app);
        AWBlazorApp.Features.Mes.Api.ShopFloorEndpoints.MapDowntimeReasonEndpoints(app);
        AWBlazorApp.Features.Mes.Api.WorkInstructionEndpoints.MapWorkInstructionEndpoints(app);

        // Batch 14 — Quality (qa schema). NCR disposition writes inv transactions via IInventoryService.
        AWBlazorApp.Features.Quality.Api.InspectionPlanEndpoints.MapInspectionPlanEndpoints(app);
        AWBlazorApp.Features.Quality.Api.InspectionEndpoints.MapInspectionEndpoints(app);
        AWBlazorApp.Features.Quality.Api.NonConformanceEndpoints.MapNonConformanceEndpoints(app);
        AWBlazorApp.Features.Quality.Api.CapaCaseEndpoints.MapCapaCaseEndpoints(app);

        // Batch 15 — Workforce (wf schema). Training, qualifications, attendance/leave, comms.
        AWBlazorApp.Features.Workforce.Api.TrainingEndpoints.MapTrainingEndpoints(app);
        AWBlazorApp.Features.Workforce.Api.QualificationEndpoints.MapQualificationEndpoints(app);
        AWBlazorApp.Features.Workforce.Api.AttendanceLeaveEndpoints.MapAttendanceLeaveEndpoints(app);
        AWBlazorApp.Features.Workforce.Api.CommunicationEndpoints.MapCommunicationEndpoints(app);

        // Batch 16 — Engineering (eng schema). Routings, BOMs, ECOs, docs, deviations.
        AWBlazorApp.Features.Engineering.Api.RoutingEndpoints.MapRoutingEndpoints(app);
        AWBlazorApp.Features.Engineering.Api.BomEndpoints.MapBomEndpoints(app);
        AWBlazorApp.Features.Engineering.Api.EngineeringChangeOrderEndpoints.MapEngineeringChangeOrderEndpoints(app);
        AWBlazorApp.Features.Engineering.Api.DocumentDeviationEndpoints.MapDocumentDeviationEndpoints(app);

        // Batch 17 — Maintenance (maint schema). Asset profiles, PM schedules, work orders, spares.
        AWBlazorApp.Features.Maintenance.Api.AssetMaintenanceEndpoints.MapAssetMaintenanceEndpoints(app);
        AWBlazorApp.Features.Maintenance.Api.PmScheduleEndpoints.MapPmScheduleEndpoints(app);
        AWBlazorApp.Features.Maintenance.Api.MaintenanceWorkOrderEndpoints.MapMaintenanceWorkOrderEndpoints(app);
        AWBlazorApp.Features.Maintenance.Api.SparePartEndpoints.MapSparePartEndpoints(app);

        // Batch 18 — Performance (perf schema). OEE, metric rollups, KPIs, scorecards, reports.
        AWBlazorApp.Features.Performance.Api.MetricsEndpoints.MapMetricsEndpoints(app);
        AWBlazorApp.Features.Performance.Api.KpiEndpoints.MapKpiEndpoints(app);
        AWBlazorApp.Features.Performance.Api.ScorecardEndpoints.MapScorecardEndpoints(app);
        AWBlazorApp.Features.Performance.Api.ReportEndpoints.MapReportEndpoints(app);

        // Cross-module plant dashboard. Aggregates from every module schema.
        AWBlazorApp.Features.Dashboard.Api.DashboardEndpoints.MapDashboardEndpoints(app);

        // CSV export endpoint (Admin only).
        app.MapExportEndpoints();

        return app;
    }
}
