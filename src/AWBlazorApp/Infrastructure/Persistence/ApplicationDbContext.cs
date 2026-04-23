using AWBlazorApp.Shared.Domain;
using AWBlazorApp.Shared.Audit;
using AWBlazorApp.Features.UserGuide.Domain;
using AWBlazorApp.Features.Maintenance.ToolSlots.Domain;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Sales.CountryRegionCurrencies.Domain; using AWBlazorApp.Features.Sales.CreditCards.Domain; using AWBlazorApp.Features.Sales.Currencies.Domain; using AWBlazorApp.Features.Sales.CurrencyRates.Domain; using AWBlazorApp.Features.Sales.Customers.Domain; using AWBlazorApp.Features.Sales.PersonCreditCards.Domain; using AWBlazorApp.Features.Sales.SalesOrderDetails.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain; using AWBlazorApp.Features.Sales.SalesOrderHeaderSalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesPeople.Domain; using AWBlazorApp.Features.Sales.SalesPersonQuotaHistories.Domain; using AWBlazorApp.Features.Sales.SalesReasons.Domain; using AWBlazorApp.Features.Sales.SalesTaxRates.Domain; using AWBlazorApp.Features.Sales.SalesTerritories.Domain; using AWBlazorApp.Features.Sales.SalesTerritoryHistories.Domain; using AWBlazorApp.Features.Sales.ShoppingCartItems.Domain; using AWBlazorApp.Features.Sales.SpecialOffers.Domain; using AWBlazorApp.Features.Sales.SpecialOfferProducts.Domain; using AWBlazorApp.Features.Sales.Stores.Domain; 
using AWBlazorApp.Features.Production.Domain; using AWBlazorApp.Features.Production.Boms.Domain; using AWBlazorApp.Features.Production.Cultures.Domain; using AWBlazorApp.Features.Production.Documents.Domain; using AWBlazorApp.Features.Production.Illustrations.Domain; using AWBlazorApp.Features.Production.Locations.Domain; using AWBlazorApp.Features.Production.ProductCategories.Domain; using AWBlazorApp.Features.Production.ProductCostHistories.Domain; using AWBlazorApp.Features.Production.ProductDescriptions.Domain; using AWBlazorApp.Features.Production.ProductDocuments.Domain; using AWBlazorApp.Features.Production.ProductInventories.Domain; using AWBlazorApp.Features.Production.ProductListPriceHistories.Domain; using AWBlazorApp.Features.Production.ProductModels.Domain; using AWBlazorApp.Features.Production.ProductModelIllustrations.Domain; using AWBlazorApp.Features.Production.ProductModelProductDescriptionCultures.Domain; using AWBlazorApp.Features.Production.ProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductProductPhotos.Domain; using AWBlazorApp.Features.Production.ProductReviews.Domain; using AWBlazorApp.Features.Production.Products.Domain; using AWBlazorApp.Features.Production.ProductSubcategories.Domain; using AWBlazorApp.Features.Production.ScrapReasons.Domain; using AWBlazorApp.Features.Production.TransactionHistories.Domain; using AWBlazorApp.Features.Production.TransactionHistoryArchives.Domain; using AWBlazorApp.Features.Production.UnitMeasures.Domain; using AWBlazorApp.Features.Production.WorkOrders.Domain; using AWBlazorApp.Features.Production.WorkOrderRoutings.Domain; 
using AWBlazorApp.Features.HumanResources.Departments.Domain; using AWBlazorApp.Features.HumanResources.Employees.Domain; using AWBlazorApp.Features.HumanResources.EmployeeDepartmentHistories.Domain; using AWBlazorApp.Features.HumanResources.EmployeePayHistories.Domain; using AWBlazorApp.Features.HumanResources.JobCandidates.Domain; using AWBlazorApp.Features.HumanResources.Shifts.Domain; 
using AWBlazorApp.Features.Purchasing.ProductVendors.Domain; using AWBlazorApp.Features.Purchasing.PurchaseOrderDetails.Domain; using AWBlazorApp.Features.Purchasing.PurchaseOrderHeaders.Domain; using AWBlazorApp.Features.Purchasing.ShipMethods.Domain; using AWBlazorApp.Features.Purchasing.Vendors.Domain; 
using AWBlazorApp.Features.Person.Addresses.Domain; using AWBlazorApp.Features.Person.AddressTypes.Domain; using AWBlazorApp.Features.Person.BusinessEntities.Domain; using AWBlazorApp.Features.Person.BusinessEntityAddresses.Domain; using AWBlazorApp.Features.Person.BusinessEntityContacts.Domain; using AWBlazorApp.Features.Person.ContactTypes.Domain; using AWBlazorApp.Features.Person.CountryRegions.Domain; using AWBlazorApp.Features.Person.EmailAddresses.Domain; using AWBlazorApp.Features.Person.Persons.Domain; using AWBlazorApp.Features.Person.PersonPhones.Domain; using AWBlazorApp.Features.Person.PhoneNumberTypes.Domain; using AWBlazorApp.Features.Person.StateProvinces.Domain;
using AWBlazorApp.Features.Forecasting.Domain;
using AWBlazorApp.Features.Insights.Domain;
using AWBlazorApp.Features.ProcessManagement.Domain;
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Features.Logistics.Receipts.Domain; using AWBlazorApp.Features.Logistics.Shipments.Domain; using AWBlazorApp.Features.Logistics.Transfers.Domain; 
using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using AWBlazorApp.Features.Quality.Capa.Domain; using AWBlazorApp.Features.Quality.Inspections.Domain; using AWBlazorApp.Features.Quality.Ncrs.Domain; using AWBlazorApp.Features.Quality.Plans.Domain; 
using AWBlazorApp.Features.Engineering.Boms.Domain; using AWBlazorApp.Features.Engineering.Deviations.Domain; using AWBlazorApp.Features.Engineering.Documents.Domain; using AWBlazorApp.Features.Engineering.Ecos.Domain; using AWBlazorApp.Features.Engineering.Routings.Domain; 
using AWBlazorApp.Features.Maintenance.AssetProfiles.Domain; using AWBlazorApp.Features.Maintenance.Logs.Domain; using AWBlazorApp.Features.Maintenance.MeterReadings.Domain; using AWBlazorApp.Features.Maintenance.PmSchedules.Domain; using AWBlazorApp.Features.Maintenance.SpareParts.Domain; using AWBlazorApp.Features.Maintenance.WorkOrders.Domain; 
using AWBlazorApp.Features.Performance.Kpis.Domain; using AWBlazorApp.Features.Performance.MaintenanceMetrics.Domain; using AWBlazorApp.Features.Performance.Oee.Domain; using AWBlazorApp.Features.Performance.ProductionMetrics.Domain; using AWBlazorApp.Features.Performance.Reports.Domain; using AWBlazorApp.Features.Performance.Scorecards.Domain; 
using AWBlazorApp.Features.Workforce.Announcements.Domain; using AWBlazorApp.Features.Workforce.Attendance.Domain; using AWBlazorApp.Features.Workforce.EmployeeQualifications.Domain; using AWBlazorApp.Features.Workforce.LeaveRequests.Domain; using AWBlazorApp.Features.Workforce.Qualifications.Domain; using AWBlazorApp.Features.Workforce.Alerts.Domain; using AWBlazorApp.Features.Workforce.HandoverNotes.Domain; using AWBlazorApp.Features.Workforce.StationQualifications.Domain; using AWBlazorApp.Features.Workforce.TrainingCourses.Domain; using AWBlazorApp.Features.Workforce.TrainingRecords.Domain; 
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Process = AWBlazorApp.Features.ProcessManagement.Domain.Process;

namespace AWBlazorApp.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    // Forecasting
    public DbSet<ForecastDefinition> ForecastDefinitions => Set<ForecastDefinition>();
    public DbSet<ForecastDataPoint> ForecastDataPoints => Set<ForecastDataPoint>();
    public DbSet<ForecastHistoricalSnapshot> ForecastHistoricalSnapshots => Set<ForecastHistoricalSnapshot>();

    // User guide read tracking
    public DbSet<ArticleRead> ArticleReads => Set<ArticleRead>();

    // Area-based permissions
    public DbSet<UserAreaPermission> UserAreaPermissions => Set<UserAreaPermission>();

    // Process Management
    public DbSet<Process> Processes => Set<Process>();
    public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();
    public DbSet<ProcessExecution> ProcessExecutions => Set<ProcessExecution>();
    public DbSet<ProcessStepExecution> ProcessStepExecutions => Set<ProcessStepExecution>();

    public DbSet<ToolSlotConfiguration> ToolSlotConfigurations => Set<ToolSlotConfiguration>();

    // Consolidated audit log — populated automatically by AuditLogInterceptor on every
    // SaveChangesAsync. Replaces the 117 per-entity *AuditLog tables (all dropped in
    // migration _DropLegacyAuditLogTables).
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();
    public DbSet<NotificationRule> NotificationRules => Set<NotificationRule>();
    public DbSet<SavedQuery> SavedQueries => Set<SavedQuery>();
    public DbSet<KpiSnapshot> KpiSnapshots => Set<KpiSnapshot>();
    public DbSet<DashboardItem> DashboardItems => Set<DashboardItem>();
    public DbSet<ReportSchedule> ReportSchedules => Set<ReportSchedule>();

    // AdventureWorks2022 reference-data tables — DBA owns all of these, we read/write but
    // NEVER alter or drop them. Every entity below is configured with ExcludeFromMigrations().
    public DbSet<AddressType> AddressTypes => Set<AddressType>();
    public DbSet<ContactType> ContactTypes => Set<ContactType>();
    public DbSet<CountryRegion> CountryRegions => Set<CountryRegion>();
    public DbSet<PhoneNumberType> PhoneNumberTypes => Set<PhoneNumberType>();
    public DbSet<Culture> Cultures => Set<Culture>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ScrapReason> ScrapReasons => Set<ScrapReason>();
    public DbSet<UnitMeasure> UnitMeasures => Set<UnitMeasure>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<SalesReason> SalesReasons => Set<SalesReason>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Shift> Shifts => Set<Shift>();

    // Batch 2 — more AdventureWorks reference data (also .ExcludeFromMigrations()).
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<ShipMethod> ShipMethods => Set<ShipMethod>();
    public DbSet<ProductSubcategory> ProductSubcategories => Set<ProductSubcategory>();
    public DbSet<ProductDescription> ProductDescriptions => Set<ProductDescription>();
    public DbSet<SpecialOffer> SpecialOffers => Set<SpecialOffer>();
    public DbSet<StateProvince> StateProvinces => Set<StateProvince>();
    public DbSet<SalesTerritory> SalesTerritories => Set<SalesTerritory>();
    public DbSet<SalesTaxRate> SalesTaxRates => Set<SalesTaxRate>();
    public DbSet<ShoppingCartItem> ShoppingCartItems => Set<ShoppingCartItem>();

    // Batch 3 — business entities, junction tables, and composite-key history tables.
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesPerson> SalesPersons => Set<SalesPerson>();
    public DbSet<SalesPersonQuotaHistory> SalesPersonQuotaHistories => Set<SalesPersonQuotaHistory>();
    public DbSet<SalesOrderHeaderSalesReason> SalesOrderHeaderSalesReasons => Set<SalesOrderHeaderSalesReason>();
    public DbSet<CountryRegionCurrency> CountryRegionCurrencies => Set<CountryRegionCurrency>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<BillOfMaterials> BillOfMaterials => Set<BillOfMaterials>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<EmployeeDepartmentHistory> EmployeeDepartmentHistories => Set<EmployeeDepartmentHistory>();
    public DbSet<ProductCostHistory> ProductCostHistories => Set<ProductCostHistory>();

    // Batch 4 — Person hierarchy tables (Address, BusinessEntity, Person + dependents).
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<BusinessEntity> BusinessEntities => Set<BusinessEntity>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<EmailAddress> EmailAddresses => Set<EmailAddress>();
    public DbSet<PersonPhone> PersonPhones => Set<PersonPhone>();
    public DbSet<BusinessEntityAddress> BusinessEntityAddresses => Set<BusinessEntityAddress>();
    public DbSet<BusinessEntityContact> BusinessEntityContacts => Set<BusinessEntityContact>();

    // Batch 5 — Production / Product hierarchy.
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductModel> ProductModels => Set<ProductModel>();
    public DbSet<Illustration> Illustrations => Set<Illustration>();
    public DbSet<ProductPhoto> ProductPhotos => Set<ProductPhoto>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<ProductInventory> ProductInventories => Set<ProductInventory>();
    public DbSet<ProductListPriceHistory> ProductListPriceHistories => Set<ProductListPriceHistory>();
    public DbSet<ProductProductPhoto> ProductProductPhotos => Set<ProductProductPhoto>();

    // Batch 6 — Production junction + transaction tables.
    public DbSet<ProductModelIllustration> ProductModelIllustrations => Set<ProductModelIllustration>();
    public DbSet<ProductModelProductDescriptionCulture> ProductModelProductDescriptionCultures => Set<ProductModelProductDescriptionCulture>();
    public DbSet<WorkOrderRouting> WorkOrderRoutings => Set<WorkOrderRouting>();
    public DbSet<TransactionHistory> TransactionHistories => Set<TransactionHistory>();
    public DbSet<TransactionHistoryArchive> TransactionHistoryArchives => Set<TransactionHistoryArchive>();

    // Batch 7 — Sales: CreditCard, PersonCreditCard, SalesOrderHeader/Detail, SalesTerritoryHistory, SpecialOfferProduct, Store.
    public DbSet<CreditCard> CreditCards => Set<CreditCard>();
    public DbSet<PersonCreditCard> PersonCreditCards => Set<PersonCreditCard>();
    public DbSet<SalesOrderHeader> SalesOrderHeaders => Set<SalesOrderHeader>();
    public DbSet<SalesOrderDetail> SalesOrderDetails => Set<SalesOrderDetail>();
    public DbSet<SalesTerritoryHistory> SalesTerritoryHistories => Set<SalesTerritoryHistory>();
    public DbSet<SpecialOfferProduct> SpecialOfferProducts => Set<SpecialOfferProduct>();
    public DbSet<Store> Stores => Set<Store>();

    // Batch 8 — Purchasing + HR entities.
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<ProductVendor> ProductVendors => Set<ProductVendor>();
    public DbSet<PurchaseOrderHeader> PurchaseOrderHeaders => Set<PurchaseOrderHeader>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeePayHistory> EmployeePayHistories => Set<EmployeePayHistory>();
    public DbSet<JobCandidate> JobCandidates => Set<JobCandidate>();

    // Batch 9 — Production: Document + ProductDocument (hierarchyid PK support).
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ProductDocument> ProductDocuments => Set<ProductDocument>();

    // Audit logs for the AdventureWorks reference-data tables. Unlike the source tables,
    // these ARE EF-managed (they live in dbo with plain PK/FK-free shapes).

    // Batch 2 audit logs.

    // Batch 3 audit logs.

    // Batch 4 audit logs.

    // Batch 5 audit logs.

    // Batch 6 audit logs.

    // Batch 7 audit logs.

    // Batch 8 audit logs.

    // Batch 9 audit logs.

    // Batch 10 — Enterprise master data (org.* schema, EF-managed). Entities and their audit logs.
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrgUnit> OrgUnits => Set<OrgUnit>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<ProductLine> ProductLines => Set<ProductLine>();

    // Batch 11 — Advanced inventory (inv.* schema). InventoryBalance is the derived on-hand
    // aggregate, InventoryTransaction is the append-only ledger, Outbox/Queue are operational
    // state machines (no separate audit log — changes are high-churn and audit would be noise).
    public DbSet<InventoryLocation> InventoryLocations => Set<InventoryLocation>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<SerialUnit> SerialUnits => Set<SerialUnit>();
    public DbSet<InventoryBalance> InventoryBalances => Set<InventoryBalance>();
    public DbSet<InventoryTransactionType> InventoryTransactionTypes => Set<InventoryTransactionType>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<InventoryAdjustment> InventoryAdjustments => Set<InventoryAdjustment>();
    public DbSet<InventoryTransactionOutbox> InventoryTransactionOutbox => Set<InventoryTransactionOutbox>();
    public DbSet<InventoryTransactionQueue> InventoryTransactionQueue => Set<InventoryTransactionQueue>();

    // Batch 12 — Logistics (lgx.* schema). Inbound receipts, outbound shipments, and
    // inter-location/inter-org stock transfers. Posting each writes inv.InventoryTransaction
    // rows through IInventoryService so the ledger stays the single source of truth.
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentLine> ShipmentLines => Set<ShipmentLine>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();

    // Batch 13 — Production execution / MES (mes.* schema). Run completion writes WIP_ISSUE
    // and WIP_RECEIPT inventory transactions through IInventoryService; downtime + clock
    // events are append-only audit-ish records that drive the OEE rollup.
    public DbSet<ProductionRun> ProductionRuns => Set<ProductionRun>();
    public DbSet<ProductionRunOperation> ProductionRunOperations => Set<ProductionRunOperation>();
    public DbSet<OperatorClockEvent> OperatorClockEvents => Set<OperatorClockEvent>();
    public DbSet<DowntimeEvent> DowntimeEvents => Set<DowntimeEvent>();
    public DbSet<DowntimeReason> DowntimeReasons => Set<DowntimeReason>();
    public DbSet<WorkInstruction> WorkInstructions => Set<WorkInstruction>();
    public DbSet<WorkInstructionRevision> WorkInstructionRevisions => Set<WorkInstructionRevision>();
    public DbSet<WorkInstructionStep> WorkInstructionSteps => Set<WorkInstructionStep>();

    // Batch 14 — Quality (qa.* schema). Inspections / NCRs / CAPA cases. NCR disposition writes
    // SCRAP or paired Available→Quarantine MOVE inventory transactions through IInventoryService.
    public DbSet<InspectionPlan> InspectionPlans => Set<InspectionPlan>();
    public DbSet<InspectionPlanCharacteristic> InspectionPlanCharacteristics => Set<InspectionPlanCharacteristic>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<InspectionResult> InspectionResults => Set<InspectionResult>();
    public DbSet<NonConformance> NonConformances => Set<NonConformance>();
    public DbSet<NonConformanceAction> NonConformanceActions => Set<NonConformanceAction>();
    public DbSet<CapaCase> CapaCases => Set<CapaCase>();
    public DbSet<CapaCaseNonConformance> CapaCaseNonConformances => Set<CapaCaseNonConformance>();

    // Batch 15 — Workforce (wf.* schema). Training, qualifications, attendance, leave,
    // shift handovers, announcements, and the qualification-alert inbox raised by the
    // operator-clock-in trigger hook.
    public DbSet<TrainingCourse> TrainingCourses => Set<TrainingCourse>();
    public DbSet<TrainingRecord> TrainingRecords => Set<TrainingRecord>();
    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<EmployeeQualification> EmployeeQualifications => Set<EmployeeQualification>();
    public DbSet<StationQualification> StationQualifications => Set<StationQualification>();
    public DbSet<QualificationAlert> QualificationAlerts => Set<QualificationAlert>();
    public DbSet<AttendanceEvent> AttendanceEvents => Set<AttendanceEvent>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<ShiftHandoverNote> ShiftHandoverNotes => Set<ShiftHandoverNote>();
    public DbSet<Announcement> Announcements => Set<Announcement>();

    // Batch 16 — Engineering (eng.* schema). Routings, BOMs, ECOs, engineering documents,
    // and deviation requests. Feeds MES (which consumes the active routing/BOM revisions
    // when launching a production run).
    public DbSet<ManufacturingRouting> ManufacturingRoutings => Set<ManufacturingRouting>();
    public DbSet<RoutingStep> RoutingSteps => Set<RoutingStep>();
    public DbSet<BomHeader> BomHeaders => Set<BomHeader>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<EngineeringChangeOrder> EngineeringChangeOrders => Set<EngineeringChangeOrder>();
    public DbSet<EcoAffectedItem> EcoAffectedItems => Set<EcoAffectedItem>();
    public DbSet<EcoApproval> EcoApprovals => Set<EcoApproval>();
    public DbSet<EngineeringDocument> EngineeringDocuments => Set<EngineeringDocument>();
    public DbSet<DeviationRequest> DeviationRequests => Set<DeviationRequest>();

    // Batch 17 — Maintenance (maint.* schema). PM schedules, work orders with tasks and spare
    // parts usage, meter readings driving usage-based PM, free-form maintenance logs.
    public DbSet<AssetMaintenanceProfile> AssetMaintenanceProfiles => Set<AssetMaintenanceProfile>();
    public DbSet<PmSchedule> PmSchedules => Set<PmSchedule>();
    public DbSet<PmScheduleTask> PmScheduleTasks => Set<PmScheduleTask>();
    public DbSet<MaintenanceWorkOrder> MaintenanceWorkOrders => Set<MaintenanceWorkOrder>();
    public DbSet<MaintenanceWorkOrderTask> MaintenanceWorkOrderTasks => Set<MaintenanceWorkOrderTask>();
    public DbSet<SparePart> SpareParts => Set<SparePart>();
    public DbSet<WorkOrderPartUsage> WorkOrderPartUsages => Set<WorkOrderPartUsage>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<MaintenanceLog> MaintenanceLogs => Set<MaintenanceLog>();

    // Batch 18 — Performance (perf.* schema). OEE snapshots, daily/monthly metric rollups,
    // KPI definitions and their evaluated values, scorecards, saved reports.
    public DbSet<OeeSnapshot> OeeSnapshots => Set<OeeSnapshot>();
    public DbSet<ProductionDailyMetric> ProductionDailyMetrics => Set<ProductionDailyMetric>();
    public DbSet<MaintenanceMonthlyMetric> MaintenanceMonthlyMetrics => Set<MaintenanceMonthlyMetric>();
    public DbSet<KpiDefinition> KpiDefinitions => Set<KpiDefinition>();
    public DbSet<KpiValue> KpiValues => Set<KpiValue>();
    public DbSet<ScorecardDefinition> ScorecardDefinitions => Set<ScorecardDefinition>();
    public DbSet<ScorecardKpi> ScorecardKpis => Set<ScorecardKpi>();
    public DbSet<PerformanceReport> PerformanceReports => Set<PerformanceReport>();
    public DbSet<PerformanceReportRun> PerformanceReportRuns => Set<PerformanceReportRun>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Consolidated audit log ---
        builder.Entity<AuditLog>(b =>
        {
            // Compound index on (EntityType, EntityId) is the hot-path lookup: "show me the
            // history of this cost center." Sort by ChangedDate descending via the secondary
            // column so History pages can ORDER BY without an extra scan.
            b.HasIndex(x => new { x.EntityType, x.EntityId, x.ChangedDate });
            // Time-based queries (e.g. "everything changed in the last 24 hours") use this.
            b.HasIndex(x => x.ChangedDate);
        });

        // --- Forecasting entities ---
        builder.Entity<ForecastDefinition>(b =>
        {
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.DataSource);
            b.HasIndex(x => x.CreatedDate);
            b.HasIndex(x => x.DeletedDate);
            // Composite (Status, DeletedDate) is created via DatabaseInitializer.EnsureCompositeIndexesAsync
            // so it doesn't show up in the model snapshot and trigger spurious migrations.
        });

        builder.Entity<ForecastDataPoint>(b =>
        {
            b.HasOne(x => x.ForecastDefinition)
                .WithMany(d => d.DataPoints)
                .HasForeignKey(x => x.ForecastDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ForecastDefinitionId, x.PeriodDate }).IsUnique();
            b.HasIndex(x => x.PeriodDate);
        });

        builder.Entity<ForecastHistoricalSnapshot>(b =>
        {
            b.HasOne(x => x.ForecastDefinition)
                .WithMany(d => d.HistoricalSnapshots)
                .HasForeignKey(x => x.ForecastDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ForecastDefinitionId, x.PeriodDate }).IsUnique();
        });

        // --- Article read tracking ---
        builder.Entity<ArticleRead>(b =>
        {
            b.HasIndex(x => new { x.UserId, x.ArticleSlug }).IsUnique();
            b.HasIndex(x => x.ArticleSlug);
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- User area permissions ---
        builder.Entity<UserAreaPermission>(b =>
        {
            b.HasIndex(x => new { x.UserId, x.Area }).IsUnique();
            b.HasIndex(x => x.Area);
            b.HasOne(x => x.User)
                .WithMany(u => u.AreaPermissions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Area).HasConversion<int>();
            b.Property(x => x.PermissionLevel).HasConversion<int>();
        });

        // --- Process Management entities ---
        builder.Entity<Process>(b =>
        {
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.DepartmentId);
            b.HasIndex(x => x.DeletedDate);
            b.HasIndex(x => x.IsRecurring);
            b.HasIndex(x => x.NextRunDate);
            b.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.DefaultProcessor).WithMany().HasForeignKey(x => x.DefaultProcessorUserId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.Status).HasConversion<int>();
        });

        builder.Entity<ProcessStep>(b =>
        {
            b.HasOne(x => x.Process).WithMany(p => p.Steps).HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ProcessId, x.SequenceNumber }).IsUnique();
        });

        builder.Entity<ProcessExecution>(b =>
        {
            b.HasOne(x => x.Process).WithMany(p => p.Executions).HasForeignKey(x => x.ProcessId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.AssignedUser).WithMany().HasForeignKey(x => x.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => x.ProcessId);
            b.HasIndex(x => x.ExecutionDate);
            b.HasIndex(x => x.Status);
            b.Property(x => x.Status).HasConversion<int>();
        });

        builder.Entity<ProcessStepExecution>(b =>
        {
            b.HasOne(x => x.ProcessExecution).WithMany(e => e.StepExecutions)
                .HasForeignKey(x => x.ProcessExecutionId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ProcessStep).WithMany().HasForeignKey(x => x.ProcessStepId)
                .OnDelete(DeleteBehavior.NoAction);
            b.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.ProcessExecutionId, x.ProcessStepId }).IsUnique();
            b.Property(x => x.Status).HasConversion<int>();
        });

        // ToolSlotConfigurations is a pre-existing table in AdventureWorks2022 that is managed
        // outside of EF migrations. EF will read/write it but never CREATE/DROP/ALTER it.
        builder.Entity<ToolSlotConfiguration>(b =>
        {
            b.ToTable("ToolSlotConfigurations", t => t.ExcludeFromMigrations());
            b.HasIndex(x => new { x.Family, x.MtCode, x.Destination });
        });

        builder.Entity<ApiKey>(b =>
        {
            b.HasIndex(x => x.Key).IsUnique();
            b.HasIndex(x => x.UserId);
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SecurityAuditLog>(b =>
        {
            b.ToTable("SecurityAuditLogs");
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Timestamp);
            // Composite (UserId, Timestamp DESC) is created via DatabaseInitializer.EnsureCompositeIndexesAsync.
        });

        // ── AdventureWorks reference-data tables ────────────────────────────────────────────
        // Every one of these is DBA-owned. EF may SELECT / INSERT / UPDATE / DELETE rows but
        // MUST NOT issue DDL — hence t.ExcludeFromMigrations() on each. The [Table] attribute
        // on the entity already specifies the schema; we repeat it here so the migration
        // tooling and design-time model see a consistent picture.
        builder.Entity<AddressType>(b          => b.ToTable("AddressType",     schema: "Person",         t => t.ExcludeFromMigrations()));
        builder.Entity<ContactType>(b          => b.ToTable("ContactType",     schema: "Person",         t => t.ExcludeFromMigrations()));
        builder.Entity<CountryRegion>(b        => b.ToTable("CountryRegion",   schema: "Person",         t => t.ExcludeFromMigrations()));
        builder.Entity<PhoneNumberType>(b      => b.ToTable("PhoneNumberType", schema: "Person",         t => t.ExcludeFromMigrations()));
        builder.Entity<Culture>(b              => b.ToTable("Culture",         schema: "Production",     t => t.ExcludeFromMigrations()));
        builder.Entity<ProductCategory>(b      => b.ToTable("ProductCategory", schema: "Production",     t => t.ExcludeFromMigrations()));
        builder.Entity<ScrapReason>(b          => b.ToTable("ScrapReason",     schema: "Production",     t => t.ExcludeFromMigrations()));
        builder.Entity<UnitMeasure>(b          => b.ToTable("UnitMeasure",     schema: "Production",     t => t.ExcludeFromMigrations()));
        builder.Entity<Currency>(b             => b.ToTable("Currency",        schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<SalesReason>(b          => b.ToTable("SalesReason",     schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<Department>(b           => b.ToTable("Department",      schema: "HumanResources", t => t.ExcludeFromMigrations()));
        builder.Entity<Shift>(b                => b.ToTable("Shift",           schema: "HumanResources", t => t.ExcludeFromMigrations()));

        // Batch 2 source tables — also DBA-owned.
        builder.Entity<Location>(b             => b.ToTable("Location",           schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ShipMethod>(b           => b.ToTable("ShipMethod",         schema: "Purchasing", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductSubcategory>(b   => b.ToTable("ProductSubcategory", schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductDescription>(b   => b.ToTable("ProductDescription", schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<SpecialOffer>(b         => b.ToTable("SpecialOffer",       schema: "Sales",      t => t.ExcludeFromMigrations()));
        builder.Entity<StateProvince>(b        => b.ToTable("StateProvince",      schema: "Person",     t => t.ExcludeFromMigrations()));
        builder.Entity<SalesTerritory>(b       => b.ToTable("SalesTerritory",     schema: "Sales",      t => t.ExcludeFromMigrations()));
        builder.Entity<SalesTaxRate>(b         => b.ToTable("SalesTaxRate",       schema: "Sales",      t => t.ExcludeFromMigrations()));
        builder.Entity<ShoppingCartItem>(b     => b.ToTable("ShoppingCartItem",   schema: "Sales",      t => t.ExcludeFromMigrations()));

        // Batch 3 source tables — also DBA-owned.
        builder.Entity<Customer>(b                    => b.ToTable("Customer",                    schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<SalesPerson>(b                 => b.ToTable("SalesPerson",                 schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<SalesPersonQuotaHistory>(b     => b.ToTable("SalesPersonQuotaHistory",     schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<SalesOrderHeaderSalesReason>(b => b.ToTable("SalesOrderHeaderSalesReason", schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<CountryRegionCurrency>(b       => b.ToTable("CountryRegionCurrency",       schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<CurrencyRate>(b                => b.ToTable("CurrencyRate",                schema: "Sales",          t => t.ExcludeFromMigrations()));
        builder.Entity<BillOfMaterials>(b             => b.ToTable("BillOfMaterials",             schema: "Production",     t => t.ExcludeFromMigrations()));
        builder.Entity<WorkOrder>(b                   => b.ToTable("WorkOrder",                   schema: "Production",     t => t.ExcludeFromMigrations()));
        builder.Entity<EmployeeDepartmentHistory>(b   => b.ToTable("EmployeeDepartmentHistory",   schema: "HumanResources", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductCostHistory>(b          => b.ToTable("ProductCostHistory",          schema: "Production",     t => t.ExcludeFromMigrations()));

        // Batch 4 source tables — Person hierarchy. Also DBA-owned. Note that Person.Address has
        // a SpatialLocation column (geography) that the entity intentionally does NOT map; SQL
        // Server lets that column be NULL on insert. Person.Person has two XML columns
        // (AdditionalContactInfo, Demographics) that we also intentionally do NOT map.
        builder.Entity<Address>(b                  => b.ToTable("Address",                schema: "Person", t => t.ExcludeFromMigrations()));
        builder.Entity<BusinessEntity>(b           => b.ToTable("BusinessEntity",         schema: "Person", t => t.ExcludeFromMigrations()));
        builder.Entity<Person>(b                   => b.ToTable("Person",                 schema: "Person", t => t.ExcludeFromMigrations()));
        builder.Entity<EmailAddress>(b             => b.ToTable("EmailAddress",           schema: "Person", t => t.ExcludeFromMigrations()));
        builder.Entity<PersonPhone>(b              => b.ToTable("PersonPhone",            schema: "Person", t => t.ExcludeFromMigrations()));
        builder.Entity<BusinessEntityAddress>(b    => b.ToTable("BusinessEntityAddress",  schema: "Person", t => t.ExcludeFromMigrations()));
        builder.Entity<BusinessEntityContact>(b    => b.ToTable("BusinessEntityContact",  schema: "Person", t => t.ExcludeFromMigrations()));

        // Batch 5 source tables — Production / Product hierarchy. Also DBA-owned. Notes:
        //   - Production.ProductModel has CatalogDescription + Instructions XML columns we don't map.
        //   - Production.Illustration has a Diagram XML column we don't map.
        //   - Production.ProductPhoto's image bytes are mapped (byte[]) but the dialog UI does
        //     not expose them for editing.
        builder.Entity<Product>(b                 => b.ToTable("Product",                schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductModel>(b            => b.ToTable("ProductModel",           schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<Illustration>(b            => b.ToTable("Illustration",           schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductPhoto>(b            => b.ToTable("ProductPhoto",           schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductReview>(b           => b.ToTable("ProductReview",          schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductInventory>(b        => b.ToTable("ProductInventory",       schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductListPriceHistory>(b => b.ToTable("ProductListPriceHistory", schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductProductPhoto>(b     => b.ToTable("ProductProductPhoto",    schema: "Production", t => t.ExcludeFromMigrations()));

        // Batch 6 source tables — also DBA-owned.
        builder.Entity<ProductModelIllustration>(b               => b.ToTable("ProductModelIllustration",               schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductModelProductDescriptionCulture>(b  => b.ToTable("ProductModelProductDescriptionCulture",  schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<WorkOrderRouting>(b                       => b.ToTable("WorkOrderRouting",                       schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<TransactionHistory>(b                     => b.ToTable("TransactionHistory",                     schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<TransactionHistoryArchive>(b              => b.ToTable("TransactionHistoryArchive",              schema: "Production", t => t.ExcludeFromMigrations()));

        // Batch 7 source tables — also DBA-owned.
        builder.Entity<CreditCard>(b             => b.ToTable("CreditCard",             schema: "Sales", t => t.ExcludeFromMigrations()));
        builder.Entity<PersonCreditCard>(b       => b.ToTable("PersonCreditCard",       schema: "Sales", t => t.ExcludeFromMigrations()));
        builder.Entity<SalesOrderHeader>(b       => b.ToTable("SalesOrderHeader",       schema: "Sales", t => t.ExcludeFromMigrations()));
        builder.Entity<SalesOrderDetail>(b       => b.ToTable("SalesOrderDetail",       schema: "Sales", t => t.ExcludeFromMigrations()));
        builder.Entity<SalesTerritoryHistory>(b  => b.ToTable("SalesTerritoryHistory",  schema: "Sales", t => t.ExcludeFromMigrations()));
        builder.Entity<SpecialOfferProduct>(b    => b.ToTable("SpecialOfferProduct",    schema: "Sales", t => t.ExcludeFromMigrations()));
        builder.Entity<Store>(b                  => b.ToTable("Store",                  schema: "Sales", t => t.ExcludeFromMigrations()));

        // Batch 8 source tables — Purchasing + HR. Also DBA-owned. Notes:
        //   - HumanResources.Employee has OrganizationNode (hierarchyid) and OrganizationLevel
        //     (computed) columns we don't map — same skip pattern as Address.SpatialLocation.
        //   - HumanResources.JobCandidate has a Resume XML column we don't map.
        //   - PurchaseOrderHeader.TotalDue, PurchaseOrderDetail.LineTotal and StockedQty are
        //     computed columns.
        builder.Entity<Vendor>(b                 => b.ToTable("Vendor",              schema: "Purchasing",     t => t.ExcludeFromMigrations()));
        builder.Entity<ProductVendor>(b          => b.ToTable("ProductVendor",       schema: "Purchasing",     t => t.ExcludeFromMigrations()));
        builder.Entity<PurchaseOrderHeader>(b    => b.ToTable("PurchaseOrderHeader", schema: "Purchasing",     t => t.ExcludeFromMigrations()));
        builder.Entity<PurchaseOrderDetail>(b    => b.ToTable("PurchaseOrderDetail", schema: "Purchasing",     t => t.ExcludeFromMigrations()));
        builder.Entity<Employee>(b               => b.ToTable("Employee",            schema: "HumanResources", t => t.ExcludeFromMigrations()));
        builder.Entity<EmployeePayHistory>(b     => b.ToTable("EmployeePayHistory",  schema: "HumanResources", t => t.ExcludeFromMigrations()));
        builder.Entity<JobCandidate>(b           => b.ToTable("JobCandidate",        schema: "HumanResources", t => t.ExcludeFromMigrations()));

        // Batch 9 source tables — Production: Document + ProductDocument. These use the
        // hierarchyid PK type supported by the EFCore.SqlServer.HierarchyId package.
        builder.Entity<Document>(b        => b.ToTable("Document",        schema: "Production", t => t.ExcludeFromMigrations()));
        builder.Entity<ProductDocument>(b => b.ToTable("ProductDocument", schema: "Production", t => t.ExcludeFromMigrations()));

        // Audit tables for the AdventureWorks reference data. Each lives in dbo and is
        // indexed by (source PK, ChangedDate) so the history pages can quickly filter rows
        // for one source row in chronological order.
        // Batch 2 audit-log configuration.
        // Batch 3 audit-log configuration.
        // Batch 4 audit-log configuration.
        // Batch 5 audit-log configuration.
        // Batch 6 audit-log configuration.
        // Batch 7 audit-log configuration.
        // Batch 8 audit-log configuration.
        // Batch 9 audit-log configuration — Document + ProductDocument.
        // DocumentNode is stored as a string in the audit log (not HierarchyId) so it's
        // indexed as a regular nvarchar column.
        // --- Enterprise master data (org schema) ---
        builder.Entity<Organization>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            // Filtered unique index — exactly one row may be marked IsPrimary.
            b.HasIndex(x => x.IsPrimary).IsUnique().HasFilter("[IsPrimary] = 1");
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.ParentOrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrgUnit>(b =>
        {
            b.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
            b.HasIndex(x => x.ParentOrgUnitId);
            b.HasIndex(x => x.Path);
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.ParentOrgUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<CostCenter>().WithMany().HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.Kind).HasConversion<byte>();
        });

        builder.Entity<Station>(b =>
        {
            b.HasIndex(x => new { x.OrgUnitId, x.Code }).IsUnique();
            b.HasIndex(x => x.OperatorBusinessEntityId);
            b.HasIndex(x => x.AssetId);
            b.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.OrgUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.StationKind).HasConversion<byte>();
        });

        builder.Entity<Asset>(b =>
        {
            b.HasIndex(x => x.AssetTag).IsUnique();
            b.HasIndex(x => x.OrganizationId);
            b.HasIndex(x => x.OrgUnitId);
            b.HasIndex(x => x.Status);
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.OrgUnitId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.ParentAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.AssetType).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<CostCenter>(b =>
        {
            b.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductLine>(b =>
        {
            b.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Enterprise audit logs — all live in dbo with the standard (entity Id + ChangedDate) indexes.
        // --- Advanced inventory (inv schema) ---
        builder.Entity<InventoryLocation>(b =>
        {
            b.HasIndex(x => new { x.OrganizationId, x.Code }).IsUnique();
            b.HasIndex(x => x.ParentLocationId);
            b.HasIndex(x => x.Path);
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.OrgUnitId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            // ProductionLocationId is a soft reference to Production.Location (short PK in AW) —
            // not modeled as a FK because Production.Location is not in ApplicationDbContext.
            b.Property(x => x.Kind).HasConversion<byte>();
        });

        builder.Entity<InventoryItem>(b =>
        {
            b.HasIndex(x => x.ProductId).IsUnique();
            b.HasIndex(x => x.DefaultLocationId);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.DefaultLocationId)
                .OnDelete(DeleteBehavior.SetNull);
            // ProductId is a soft FK into Production.Product (not in this DbContext).
        });

        builder.Entity<Lot>(b =>
        {
            b.HasIndex(x => new { x.InventoryItemId, x.LotCode }).IsUnique();
            b.HasIndex(x => x.VendorBusinessEntityId);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<SerialUnit>(b =>
        {
            b.HasIndex(x => new { x.InventoryItemId, x.SerialNumber }).IsUnique();
            b.HasIndex(x => x.LotId);
            b.HasIndex(x => x.CurrentLocationId);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.CurrentLocationId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<InventoryBalance>(b =>
        {
            // SQL Server treats NULLs as distinct in a unique index, so we need two filtered
            // indexes to enforce "exactly one balance row per (item, location, status)" across
            // both the lot-tracked and non-lot-tracked cases.
            b.HasIndex(x => new { x.InventoryItemId, x.LocationId, x.LotId, x.Status })
                .IsUnique()
                .HasFilter("[LotId] IS NOT NULL")
                .HasDatabaseName("UX_InventoryBalance_ItemLocationLotStatus");
            b.HasIndex(x => new { x.InventoryItemId, x.LocationId, x.Status })
                .IsUnique()
                .HasFilter("[LotId] IS NULL")
                .HasDatabaseName("UX_InventoryBalance_ItemLocationStatus_NoLot");
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<InventoryTransactionType>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<InventoryTransaction>(b =>
        {
            b.HasIndex(x => x.TransactionNumber).IsUnique();
            b.HasIndex(x => x.OccurredAt).IsDescending();
            b.HasIndex(x => new { x.InventoryItemId, x.OccurredAt });
            b.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
            b.HasIndex(x => x.CorrelationId);
            b.HasOne<InventoryTransactionType>().WithMany().HasForeignKey(x => x.TransactionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.FromLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.ToLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<SerialUnit>().WithMany().HasForeignKey(x => x.SerialUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.FromStatus).HasConversion<byte?>();
            b.Property(x => x.ToStatus).HasConversion<byte?>();
            b.Property(x => x.ReferenceType).HasConversion<byte?>();
        });

        builder.Entity<InventoryAdjustment>(b =>
        {
            b.HasIndex(x => x.AdjustmentNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => new { x.InventoryItemId, x.LocationId });
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.PostedTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.ReasonCode).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<InventoryTransactionOutbox>(b =>
        {
            b.HasIndex(x => x.InventoryTransactionId).IsUnique();
            b.HasIndex(x => new { x.Status, x.NextAttemptAt });
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.InventoryTransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<InventoryTransactionQueue>(b =>
        {
            b.HasIndex(x => new { x.ParseStatus, x.ProcessStatus, x.ReceivedAt });
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.PostedTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Source).HasConversion<byte>();
            b.Property(x => x.ParseStatus).HasConversion<byte>();
            b.Property(x => x.ProcessStatus).HasConversion<byte>();
        });

        // Inventory audit logs — dbo, matching the enterprise convention.
        // --- Logistics (lgx schema) ---
        builder.Entity<GoodsReceipt>(b =>
        {
            b.HasIndex(x => x.ReceiptNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.PurchaseOrderId);
            b.HasIndex(x => x.ReceivedLocationId);
            b.HasIndex(x => x.ReceivedAt).IsDescending();
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.ReceivedLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<GoodsReceiptLine>(b =>
        {
            b.HasIndex(x => x.GoodsReceiptId);
            b.HasIndex(x => x.PurchaseOrderDetailId);
            b.HasIndex(x => x.InventoryItemId);
            b.HasOne<GoodsReceipt>().WithMany().HasForeignKey(x => x.GoodsReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.PostedTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Shipment>(b =>
        {
            b.HasIndex(x => x.ShipmentNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.SalesOrderId);
            b.HasIndex(x => x.ShippedFromLocationId);
            b.HasIndex(x => x.ShippedAt).IsDescending();
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.ShippedFromLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<ShipmentLine>(b =>
        {
            b.HasIndex(x => x.ShipmentId);
            b.HasIndex(x => x.SalesOrderDetailId);
            b.HasIndex(x => x.InventoryItemId);
            b.HasOne<Shipment>().WithMany().HasForeignKey(x => x.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<SerialUnit>().WithMany().HasForeignKey(x => x.SerialUnitId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.PostedTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockTransfer>(b =>
        {
            b.HasIndex(x => x.TransferNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.FromLocationId);
            b.HasIndex(x => x.ToLocationId);
            b.HasIndex(x => x.CorrelationId);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.FromLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.ToLocationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.FromOrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.ToOrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<StockTransferLine>(b =>
        {
            b.HasIndex(x => x.StockTransferId);
            b.HasIndex(x => x.InventoryItemId);
            b.HasOne<StockTransfer>().WithMany().HasForeignKey(x => x.StockTransferId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<SerialUnit>().WithMany().HasForeignKey(x => x.SerialUnitId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.FromTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.ToTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Audit logs — dbo.
        // --- MES (mes schema) ---
        builder.Entity<ProductionRun>(b =>
        {
            b.HasIndex(x => x.RunNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.WorkOrderId);
            b.HasIndex(x => x.StationId);
            b.HasIndex(x => x.PlannedStartAt).IsDescending();
            b.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Kind).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<ProductionRunOperation>(b =>
        {
            b.HasIndex(x => x.ProductionRunId);
            b.HasOne<ProductionRun>().WithMany().HasForeignKey(x => x.ProductionRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OperatorClockEvent>(b =>
        {
            b.HasIndex(x => x.StationId);
            b.HasIndex(x => x.BusinessEntityId);
            b.HasIndex(x => x.ClockInAt).IsDescending();
            b.HasOne<ProductionRun>().WithMany().HasForeignKey(x => x.ProductionRunId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DowntimeEvent>(b =>
        {
            b.HasIndex(x => x.StationId);
            b.HasIndex(x => x.DowntimeReasonId);
            b.HasIndex(x => x.StartAt).IsDescending();
            b.HasOne<ProductionRun>().WithMany().HasForeignKey(x => x.ProductionRunId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<DowntimeReason>().WithMany().HasForeignKey(x => x.DowntimeReasonId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DowntimeReason>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
        });

        builder.Entity<WorkInstruction>(b =>
        {
            b.HasIndex(x => x.WorkOrderRoutingId).IsUnique();
            // NoAction (not SetNull) because the reverse FK from WorkInstructionRevision →
            // WorkInstruction uses Cascade, and SQL Server rejects the resulting cascade cycle.
            // Null-out on delete happens at the service layer instead.
            b.HasOne<WorkInstructionRevision>().WithMany().HasForeignKey(x => x.ActiveRevisionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<WorkInstructionRevision>(b =>
        {
            b.HasIndex(x => new { x.WorkInstructionId, x.RevisionNumber }).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasOne<WorkInstruction>().WithMany().HasForeignKey(x => x.WorkInstructionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<WorkInstructionStep>(b =>
        {
            b.HasIndex(x => new { x.WorkInstructionRevisionId, x.SequenceNumber }).IsUnique();
            b.HasOne<WorkInstructionRevision>().WithMany().HasForeignKey(x => x.WorkInstructionRevisionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MES audit logs — dbo.
        // --- Quality (qa schema) ---
        builder.Entity<InspectionPlan>(b =>
        {
            b.HasIndex(x => x.PlanCode).IsUnique();
            b.HasIndex(x => x.Scope);
            b.HasIndex(x => x.ProductId);
            b.HasIndex(x => x.VendorBusinessEntityId);
            b.Property(x => x.Scope).HasConversion<byte>();
        });

        builder.Entity<InspectionPlanCharacteristic>(b =>
        {
            b.HasIndex(x => new { x.InspectionPlanId, x.SequenceNumber }).IsUnique();
            b.HasOne<InspectionPlan>().WithMany().HasForeignKey(x => x.InspectionPlanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Kind).HasConversion<byte>();
        });

        builder.Entity<Inspection>(b =>
        {
            b.HasIndex(x => x.InspectionNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.InspectionPlanId);
            b.HasIndex(x => new { x.SourceKind, x.SourceId });
            b.HasOne<InspectionPlan>().WithMany().HasForeignKey(x => x.InspectionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.Status).HasConversion<byte>();
            b.Property(x => x.SourceKind).HasConversion<byte>();
        });

        builder.Entity<InspectionResult>(b =>
        {
            b.HasIndex(x => x.InspectionId);
            b.HasIndex(x => x.RecordedAt).IsDescending();
            b.HasOne<Inspection>().WithMany().HasForeignKey(x => x.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<InspectionPlanCharacteristic>().WithMany().HasForeignKey(x => x.InspectionPlanCharacteristicId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<NonConformance>(b =>
        {
            b.HasIndex(x => x.NcrNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.InspectionId);
            b.HasIndex(x => x.InventoryItemId);
            b.HasOne<Inspection>().WithMany().HasForeignKey(x => x.InspectionId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Lot>().WithMany().HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryLocation>().WithMany().HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<InventoryTransaction>().WithMany().HasForeignKey(x => x.PostedTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Status).HasConversion<byte>();
            b.Property(x => x.Disposition).HasConversion<byte?>();
        });

        builder.Entity<NonConformanceAction>(b =>
        {
            b.HasIndex(x => x.NonConformanceId);
            b.HasIndex(x => x.PerformedAt).IsDescending();
            b.HasOne<NonConformance>().WithMany().HasForeignKey(x => x.NonConformanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CapaCase>(b =>
        {
            b.HasIndex(x => x.CaseNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.OpenedAt).IsDescending();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<CapaCaseNonConformance>(b =>
        {
            b.HasIndex(x => new { x.CapaCaseId, x.NonConformanceId }).IsUnique();
            b.HasIndex(x => x.NonConformanceId);
            b.HasOne<CapaCase>().WithMany().HasForeignKey(x => x.CapaCaseId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<NonConformance>().WithMany().HasForeignKey(x => x.NonConformanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Quality audit logs — dbo.
        // --- Workforce (wf schema) ---
        builder.Entity<TrainingCourse>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.IsActive);
        });

        builder.Entity<TrainingRecord>(b =>
        {
            b.HasIndex(x => x.TrainingCourseId);
            b.HasIndex(x => x.BusinessEntityId);
            b.HasIndex(x => x.ExpiresOn);
            b.HasOne<TrainingCourse>().WithMany().HasForeignKey(x => x.TrainingCourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Qualification>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.Property(x => x.Category).HasConversion<byte>();
        });

        builder.Entity<EmployeeQualification>(b =>
        {
            b.HasIndex(x => new { x.BusinessEntityId, x.QualificationId }).IsUnique();
            b.HasIndex(x => x.ExpiresOn);
            b.HasOne<Qualification>().WithMany().HasForeignKey(x => x.QualificationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StationQualification>(b =>
        {
            b.HasIndex(x => new { x.StationId, x.QualificationId }).IsUnique();
            b.HasIndex(x => x.QualificationId);
            b.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Qualification>().WithMany().HasForeignKey(x => x.QualificationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<QualificationAlert>(b =>
        {
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.BusinessEntityId);
            b.HasIndex(x => x.StationId);
            b.HasIndex(x => x.RaisedAt).IsDescending();
            b.HasOne<Qualification>().WithMany().HasForeignKey(x => x.QualificationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Reason).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<AttendanceEvent>(b =>
        {
            b.HasIndex(x => new { x.BusinessEntityId, x.ShiftDate });
            b.HasIndex(x => x.ShiftDate).IsDescending();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<LeaveRequest>(b =>
        {
            b.HasIndex(x => x.BusinessEntityId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.StartDate).IsDescending();
            b.Property(x => x.LeaveType).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<ShiftHandoverNote>(b =>
        {
            b.HasIndex(x => new { x.StationId, x.ShiftDate });
            b.HasIndex(x => x.AuthoredAt).IsDescending();
            b.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Announcement>(b =>
        {
            b.HasIndex(x => x.PublishedAt).IsDescending();
            b.HasIndex(x => new { x.OrganizationId, x.OrgUnitId, x.IsActive });
            b.HasOne<Organization>().WithMany().HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne<OrgUnit>().WithMany().HasForeignKey(x => x.OrgUnitId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.Severity).HasConversion<byte>();
        });

        // Workforce audit logs — dbo.
        // --- Engineering (eng schema) ---
        builder.Entity<ManufacturingRouting>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => new { x.ProductId, x.IsActive });
            b.HasIndex(x => new { x.ProductId, x.RevisionNumber }).IsUnique();
        });

        builder.Entity<RoutingStep>(b =>
        {
            b.HasIndex(x => new { x.ManufacturingRoutingId, x.SequenceNumber }).IsUnique();
            b.HasOne<ManufacturingRouting>().WithMany().HasForeignKey(x => x.ManufacturingRoutingId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BomHeader>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => new { x.ProductId, x.IsActive });
            b.HasIndex(x => new { x.ProductId, x.RevisionNumber }).IsUnique();
        });

        builder.Entity<BomLine>(b =>
        {
            b.HasIndex(x => x.BomHeaderId);
            b.HasIndex(x => x.ComponentProductId);
            b.HasOne<BomHeader>().WithMany().HasForeignKey(x => x.BomHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EngineeringChangeOrder>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.RaisedAt).IsDescending();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<EcoAffectedItem>(b =>
        {
            b.HasIndex(x => x.EngineeringChangeOrderId);
            b.HasIndex(x => new { x.AffectedKind, x.TargetId });
            b.HasOne<EngineeringChangeOrder>().WithMany().HasForeignKey(x => x.EngineeringChangeOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.AffectedKind).HasConversion<byte>();
        });

        builder.Entity<EcoApproval>(b =>
        {
            b.HasIndex(x => x.EngineeringChangeOrderId);
            b.HasIndex(x => x.DecidedAt).IsDescending();
            b.HasOne<EngineeringChangeOrder>().WithMany().HasForeignKey(x => x.EngineeringChangeOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Decision).HasConversion<byte>();
        });

        builder.Entity<EngineeringDocument>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => new { x.ProductId, x.Kind });
            b.Property(x => x.Kind).HasConversion<byte>();
        });

        builder.Entity<DeviationRequest>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => new { x.ProductId, x.Status });
            b.HasIndex(x => x.RaisedAt).IsDescending();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        // Engineering audit logs — dbo.
        // --- Maintenance (maint schema) ---
        builder.Entity<AssetMaintenanceProfile>(b =>
        {
            b.HasIndex(x => x.AssetId).IsUnique();
            b.HasIndex(x => x.NextPmDueAt);
            b.HasOne<AWBlazorApp.Features.Enterprise.Assets.Domain.Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Criticality).HasConversion<byte>();
        });

        builder.Entity<PmSchedule>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => new { x.AssetId, x.IsActive });
            b.HasOne<AWBlazorApp.Features.Enterprise.Assets.Domain.Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.IntervalKind).HasConversion<byte>();
            b.Property(x => x.DefaultPriority).HasConversion<byte>();
        });

        builder.Entity<PmScheduleTask>(b =>
        {
            b.HasIndex(x => new { x.PmScheduleId, x.SequenceNumber }).IsUnique();
            b.HasOne<PmSchedule>().WithMany().HasForeignKey(x => x.PmScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MaintenanceWorkOrder>(b =>
        {
            b.HasIndex(x => x.WorkOrderNumber).IsUnique();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => new { x.AssetId, x.Status });
            b.HasIndex(x => x.ScheduledFor);
            b.HasIndex(x => x.RaisedAt).IsDescending();
            b.HasOne<AWBlazorApp.Features.Enterprise.Assets.Domain.Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne<PmSchedule>().WithMany().HasForeignKey(x => x.PmScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.Type).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
            b.Property(x => x.Priority).HasConversion<byte>();
        });

        builder.Entity<MaintenanceWorkOrderTask>(b =>
        {
            b.HasIndex(x => new { x.MaintenanceWorkOrderId, x.SequenceNumber }).IsUnique();
            b.HasOne<MaintenanceWorkOrder>().WithMany().HasForeignKey(x => x.MaintenanceWorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SparePart>(b =>
        {
            b.HasIndex(x => x.PartNumber).IsUnique();
            b.HasIndex(x => x.IsActive);
            b.HasIndex(x => x.ProductId);
        });

        builder.Entity<WorkOrderPartUsage>(b =>
        {
            b.HasIndex(x => x.MaintenanceWorkOrderId);
            b.HasIndex(x => x.SparePartId);
            b.HasOne<MaintenanceWorkOrder>().WithMany().HasForeignKey(x => x.MaintenanceWorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<SparePart>().WithMany().HasForeignKey(x => x.SparePartId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MeterReading>(b =>
        {
            b.HasIndex(x => new { x.AssetId, x.Kind, x.RecordedAt }).IsDescending(false, false, true);
            b.HasOne<AWBlazorApp.Features.Enterprise.Assets.Domain.Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Kind).HasConversion<byte>();
        });

        builder.Entity<MaintenanceLog>(b =>
        {
            b.HasIndex(x => new { x.AssetId, x.AuthoredAt }).IsDescending(false, true);
            b.HasIndex(x => x.MaintenanceWorkOrderId);
            b.HasOne<AWBlazorApp.Features.Enterprise.Assets.Domain.Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<MaintenanceWorkOrder>().WithMany().HasForeignKey(x => x.MaintenanceWorkOrderId)
                .OnDelete(DeleteBehavior.SetNull);
            b.Property(x => x.Kind).HasConversion<byte>();
        });

        // Maintenance audit logs — dbo.
        // --- Performance (perf schema) ---
        builder.Entity<OeeSnapshot>(b =>
        {
            b.HasIndex(x => new { x.StationId, x.PeriodKind, x.PeriodStart }).IsUnique();
            b.HasIndex(x => x.PeriodStart).IsDescending();
            b.HasOne<AWBlazorApp.Features.Enterprise.Stations.Domain.Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.PeriodKind).HasConversion<byte>();
        });

        builder.Entity<ProductionDailyMetric>(b =>
        {
            b.HasIndex(x => new { x.StationId, x.Date }).IsUnique();
            b.HasIndex(x => x.Date).IsDescending();
            b.HasOne<AWBlazorApp.Features.Enterprise.Stations.Domain.Station>().WithMany().HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MaintenanceMonthlyMetric>(b =>
        {
            b.HasIndex(x => new { x.AssetId, x.Year, x.Month }).IsUnique();
            b.HasOne<AWBlazorApp.Features.Enterprise.Assets.Domain.Asset>().WithMany().HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<KpiDefinition>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.IsActive);
            b.Property(x => x.Source).HasConversion<byte>();
            b.Property(x => x.Aggregation).HasConversion<byte>();
            b.Property(x => x.Direction).HasConversion<byte>();
        });

        builder.Entity<KpiValue>(b =>
        {
            b.HasIndex(x => new { x.KpiDefinitionId, x.PeriodKind, x.PeriodStart }).IsUnique();
            b.HasIndex(x => x.PeriodStart).IsDescending();
            b.HasOne<KpiDefinition>().WithMany().HasForeignKey(x => x.KpiDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.PeriodKind).HasConversion<byte>();
            b.Property(x => x.Status).HasConversion<byte>();
        });

        builder.Entity<ScorecardDefinition>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.IsActive);
        });

        builder.Entity<ScorecardKpi>(b =>
        {
            b.HasIndex(x => new { x.ScorecardDefinitionId, x.DisplayOrder });
            b.HasIndex(x => new { x.ScorecardDefinitionId, x.KpiDefinitionId }).IsUnique();
            b.HasOne<ScorecardDefinition>().WithMany().HasForeignKey(x => x.ScorecardDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne<KpiDefinition>().WithMany().HasForeignKey(x => x.KpiDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Visual).HasConversion<byte>();
        });

        builder.Entity<PerformanceReport>(b =>
        {
            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.IsActive);
            b.Property(x => x.Kind).HasConversion<byte>();
        });

        builder.Entity<PerformanceReportRun>(b =>
        {
            b.HasIndex(x => x.PerformanceReportId);
            b.HasIndex(x => x.RunAt).IsDescending();
            b.HasOne<PerformanceReport>().WithMany().HasForeignKey(x => x.PerformanceReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Performance audit logs — dbo.
    }
}
