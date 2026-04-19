using AWBlazorApp.Features.Sales.Endpoints;
using AWBlazorApp.Features.Production.Endpoints;
using AWBlazorApp.Features.HumanResources.Endpoints;
using AWBlazorApp.Features.Purchasing.Endpoints;
using AWBlazorApp.Features.Person.Endpoints;
using AWBlazorApp.Features.Forecasting.Endpoints;
using AWBlazorApp.Features.ProcessManagement.Endpoints;
using AWBlazorApp.Features.ToolSlots.Endpoints;
using AWBlazorApp.Features.Gallery.Endpoints;
using AWBlazorApp.Features.UserGuide.Services;
using AWBlazorApp.Shared.Endpoints;
using AWBlazorApp.Features.Admin.Endpoints;
using AWBlazorApp.Features.Enterprise.Endpoints;

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
        AWBlazorApp.Features.Inventory.Endpoints.InventoryItemEndpoints.MapInventoryItemEndpoints(app);
        AWBlazorApp.Features.Inventory.Endpoints.InventoryLocationEndpoints.MapInventoryLocationEndpoints(app);
        AWBlazorApp.Features.Inventory.Endpoints.LotEndpoints.MapLotEndpoints(app);
        AWBlazorApp.Features.Inventory.Endpoints.SerialUnitEndpoints.MapSerialUnitEndpoints(app);
        AWBlazorApp.Features.Inventory.Endpoints.InventoryTransactionEndpoints.MapInventoryTransactionEndpoints(app);
        AWBlazorApp.Features.Inventory.Endpoints.InventoryAdjustmentEndpoints.MapInventoryAdjustmentEndpoints(app);
        AWBlazorApp.Features.Inventory.Endpoints.InventoryReadOnlyEndpoints.MapInventoryReadOnlyEndpoints(app);

        // Batch 12 — Logistics (lgx schema). Post workflows wire into IInventoryService.
        AWBlazorApp.Features.Logistics.Endpoints.GoodsReceiptEndpoints.MapGoodsReceiptEndpoints(app);
        AWBlazorApp.Features.Logistics.Endpoints.ShipmentEndpoints.MapShipmentEndpoints(app);
        AWBlazorApp.Features.Logistics.Endpoints.StockTransferEndpoints.MapStockTransferEndpoints(app);

        // Batch 13 — MES (mes schema). Production-run completion writes WIP_ISSUE/WIP_RECEIPT via IInventoryService.
        AWBlazorApp.Features.Mes.Endpoints.ProductionRunEndpoints.MapProductionRunEndpoints(app);
        AWBlazorApp.Features.Mes.Endpoints.ShopFloorEndpoints.MapOperatorClockEventEndpoints(app);
        AWBlazorApp.Features.Mes.Endpoints.ShopFloorEndpoints.MapDowntimeEventEndpoints(app);
        AWBlazorApp.Features.Mes.Endpoints.ShopFloorEndpoints.MapDowntimeReasonEndpoints(app);
        AWBlazorApp.Features.Mes.Endpoints.WorkInstructionEndpoints.MapWorkInstructionEndpoints(app);

        // Batch 14 — Quality (qa schema). NCR disposition writes inv transactions via IInventoryService.
        AWBlazorApp.Features.Quality.Endpoints.InspectionPlanEndpoints.MapInspectionPlanEndpoints(app);
        AWBlazorApp.Features.Quality.Endpoints.InspectionEndpoints.MapInspectionEndpoints(app);
        AWBlazorApp.Features.Quality.Endpoints.NonConformanceEndpoints.MapNonConformanceEndpoints(app);
        AWBlazorApp.Features.Quality.Endpoints.CapaCaseEndpoints.MapCapaCaseEndpoints(app);

        // Batch 15 — Workforce (wf schema). Training, qualifications, attendance/leave, comms.
        AWBlazorApp.Features.Workforce.Endpoints.TrainingEndpoints.MapTrainingEndpoints(app);
        AWBlazorApp.Features.Workforce.Endpoints.QualificationEndpoints.MapQualificationEndpoints(app);
        AWBlazorApp.Features.Workforce.Endpoints.AttendanceLeaveEndpoints.MapAttendanceLeaveEndpoints(app);
        AWBlazorApp.Features.Workforce.Endpoints.CommunicationEndpoints.MapCommunicationEndpoints(app);

        // Batch 16 — Engineering (eng schema). Routings, BOMs, ECOs, docs, deviations.
        AWBlazorApp.Features.Engineering.Endpoints.RoutingEndpoints.MapRoutingEndpoints(app);
        AWBlazorApp.Features.Engineering.Endpoints.BomEndpoints.MapBomEndpoints(app);
        AWBlazorApp.Features.Engineering.Endpoints.EngineeringChangeOrderEndpoints.MapEngineeringChangeOrderEndpoints(app);
        AWBlazorApp.Features.Engineering.Endpoints.DocumentDeviationEndpoints.MapDocumentDeviationEndpoints(app);

        // Batch 17 — Maintenance (maint schema). Asset profiles, PM schedules, work orders, spares.
        AWBlazorApp.Features.Maintenance.Endpoints.AssetMaintenanceEndpoints.MapAssetMaintenanceEndpoints(app);
        AWBlazorApp.Features.Maintenance.Endpoints.PmScheduleEndpoints.MapPmScheduleEndpoints(app);
        AWBlazorApp.Features.Maintenance.Endpoints.MaintenanceWorkOrderEndpoints.MapMaintenanceWorkOrderEndpoints(app);
        AWBlazorApp.Features.Maintenance.Endpoints.SparePartEndpoints.MapSparePartEndpoints(app);

        // Batch 18 — Performance (perf schema). OEE, metric rollups, KPIs, scorecards, reports.
        AWBlazorApp.Features.Performance.Endpoints.MetricsEndpoints.MapMetricsEndpoints(app);
        AWBlazorApp.Features.Performance.Endpoints.KpiEndpoints.MapKpiEndpoints(app);
        AWBlazorApp.Features.Performance.Endpoints.ScorecardEndpoints.MapScorecardEndpoints(app);
        AWBlazorApp.Features.Performance.Endpoints.ReportEndpoints.MapReportEndpoints(app);

        // Cross-module plant dashboard. Aggregates from every module schema.
        AWBlazorApp.Features.Dashboard.Endpoints.DashboardEndpoints.MapDashboardEndpoints(app);

        // CSV export endpoint (Admin only).
        app.MapExportEndpoints();

        return app;
    }
}
