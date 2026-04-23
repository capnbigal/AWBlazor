using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyAuditLogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[AddressAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[AddressAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[AddressTypeAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[AddressTypeAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[AnnouncementAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[AnnouncementAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[AssetAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[AssetAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[AssetMaintenanceProfileAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[AssetMaintenanceProfileAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[BillOfMaterialsAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[BillOfMaterialsAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[BomHeaderAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[BomHeaderAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[BusinessEntityAddressAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[BusinessEntityAddressAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[BusinessEntityAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[BusinessEntityAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[BusinessEntityContactAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[BusinessEntityContactAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CapaCaseAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CapaCaseAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ContactTypeAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ContactTypeAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CostCenterAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CostCenterAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CountryRegionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CountryRegionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CountryRegionCurrencyAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CountryRegionCurrencyAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CreditCardAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CreditCardAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CultureAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CultureAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CurrencyAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CurrencyAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CurrencyRateAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CurrencyRateAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[CustomerAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[CustomerAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DepartmentAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[DepartmentAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DeviationRequestAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[DeviationRequestAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DocumentAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[DocumentAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[DowntimeReasonAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[DowntimeReasonAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[EmailAddressAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[EmailAddressAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[EmployeeAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[EmployeeAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[EmployeeDepartmentHistoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[EmployeeDepartmentHistoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[EmployeePayHistoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[EmployeePayHistoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[EmployeeQualificationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[EmployeeQualificationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[EngineeringChangeOrderAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[EngineeringChangeOrderAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[EngineeringDocumentAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[EngineeringDocumentAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[GoodsReceiptAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[GoodsReceiptAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[GoodsReceiptLineAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[GoodsReceiptLineAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[IllustrationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[IllustrationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[InspectionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[InspectionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[InspectionPlanAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[InspectionPlanAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[InspectionPlanCharacteristicAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[InspectionPlanCharacteristicAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[InventoryAdjustmentAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[InventoryAdjustmentAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[InventoryItemAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[InventoryItemAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[InventoryLocationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[InventoryLocationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[JobCandidateAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[JobCandidateAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[KpiDefinitionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[KpiDefinitionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[LeaveRequestAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[LeaveRequestAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[LocationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[LocationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[LotAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[LotAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[MaintenanceWorkOrderAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[MaintenanceWorkOrderAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ManufacturingRoutingAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ManufacturingRoutingAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[NonConformanceActionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[NonConformanceActionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[NonConformanceAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[NonConformanceAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[OrgUnitAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[OrgUnitAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[OrganizationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[OrganizationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PerformanceReportAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PerformanceReportAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PersonAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PersonAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PersonCreditCardAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PersonCreditCardAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PersonPhoneAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PersonPhoneAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PhoneNumberTypeAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PhoneNumberTypeAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PmScheduleAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PmScheduleAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductCategoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductCategoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductCostHistoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductCostHistoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductDescriptionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductDescriptionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductDocumentAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductDocumentAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductInventoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductInventoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductLineAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductLineAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductListPriceHistoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductListPriceHistoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductModelAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductModelAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductModelIllustrationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductModelIllustrationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductModelProductDescriptionCultureAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductModelProductDescriptionCultureAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductPhotoAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductPhotoAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductProductPhotoAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductProductPhotoAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductReviewAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductReviewAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductSubcategoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductSubcategoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductVendorAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductVendorAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductionRunAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductionRunAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ProductionRunOperationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ProductionRunOperationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PurchaseOrderDetailAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PurchaseOrderDetailAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[PurchaseOrderHeaderAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[PurchaseOrderHeaderAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[QualificationAlertAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[QualificationAlertAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[QualificationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[QualificationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesOrderDetailAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesOrderDetailAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesOrderHeaderAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesOrderHeaderAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesOrderHeaderSalesReasonAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesOrderHeaderSalesReasonAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesPersonAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesPersonAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesPersonQuotaHistoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesPersonQuotaHistoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesReasonAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesReasonAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesTaxRateAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesTaxRateAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesTerritoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesTerritoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SalesTerritoryHistoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SalesTerritoryHistoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ScorecardDefinitionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ScorecardDefinitionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ScrapReasonAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ScrapReasonAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SerialUnitAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SerialUnitAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ShiftAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ShiftAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ShipMethodAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ShipMethodAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ShipmentAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ShipmentAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ShipmentLineAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ShipmentLineAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ShoppingCartItemAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ShoppingCartItemAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SparePartAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SparePartAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SpecialOfferAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SpecialOfferAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[SpecialOfferProductAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[SpecialOfferProductAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[StateProvinceAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[StateProvinceAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[StationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[StationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[StationQualificationAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[StationQualificationAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[StockTransferAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[StockTransferAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[StockTransferLineAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[StockTransferLineAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[StoreAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[StoreAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[ToolSlotAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[ToolSlotAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[TrainingCourseAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[TrainingCourseAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[TransactionHistoryArchiveAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[TransactionHistoryArchiveAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[TransactionHistoryAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[TransactionHistoryAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[UnitMeasureAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[UnitMeasureAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[VendorAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[VendorAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[WorkInstructionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[WorkInstructionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[WorkInstructionRevisionAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[WorkInstructionRevisionAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[WorkInstructionStepAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[WorkInstructionStepAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[WorkOrderAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[WorkOrderAuditLogs];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[dbo].[WorkOrderRoutingAuditLogs]', N'U') IS NOT NULL DROP TABLE [dbo].[WorkOrderRoutingAuditLogs];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new System.NotSupportedException(
                "DropLegacyAuditLogTables cannot be rolled back. The 116 legacy *AuditLog "
                + "tables were retired by this migration; writes have long since moved to "
                + "audit.AuditLog via AuditLogInterceptor. To recover historical rows, restore "
                + "a pre-deployment SQL backup.");
        }
    }
}
