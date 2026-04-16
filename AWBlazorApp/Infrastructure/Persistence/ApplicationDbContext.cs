using AWBlazorApp.Shared.Domain;
using AWBlazorApp.Features.UserGuide.Domain;
using AWBlazorApp.Features.ToolSlots.Domain;
using AWBlazorApp.Data;
using AWBlazorApp.Features.AdventureWorks.Domain;
using AWBlazorApp.Features.Sales.Domain;
using AWBlazorApp.Features.Production.Domain;
using AWBlazorApp.Features.HumanResources.Domain;
using AWBlazorApp.Features.Purchasing.Domain;
using AWBlazorApp.Features.Person.Domain;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Forecasting.Domain;
using AWBlazorApp.Features.Insights.Domain;
using AWBlazorApp.Features.ProcessManagement.Domain;
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
    public DbSet<ToolSlotAuditLog> ToolSlotAuditLogs => Set<ToolSlotAuditLog>();
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
    public DbSet<AddressTypeAuditLog> AddressTypeAuditLogs => Set<AddressTypeAuditLog>();
    public DbSet<ContactTypeAuditLog> ContactTypeAuditLogs => Set<ContactTypeAuditLog>();
    public DbSet<CountryRegionAuditLog> CountryRegionAuditLogs => Set<CountryRegionAuditLog>();
    public DbSet<PhoneNumberTypeAuditLog> PhoneNumberTypeAuditLogs => Set<PhoneNumberTypeAuditLog>();
    public DbSet<CultureAuditLog> CultureAuditLogs => Set<CultureAuditLog>();
    public DbSet<ProductCategoryAuditLog> ProductCategoryAuditLogs => Set<ProductCategoryAuditLog>();
    public DbSet<ScrapReasonAuditLog> ScrapReasonAuditLogs => Set<ScrapReasonAuditLog>();
    public DbSet<UnitMeasureAuditLog> UnitMeasureAuditLogs => Set<UnitMeasureAuditLog>();
    public DbSet<CurrencyAuditLog> CurrencyAuditLogs => Set<CurrencyAuditLog>();
    public DbSet<SalesReasonAuditLog> SalesReasonAuditLogs => Set<SalesReasonAuditLog>();
    public DbSet<DepartmentAuditLog> DepartmentAuditLogs => Set<DepartmentAuditLog>();
    public DbSet<ShiftAuditLog> ShiftAuditLogs => Set<ShiftAuditLog>();

    // Batch 2 audit logs.
    public DbSet<LocationAuditLog> LocationAuditLogs => Set<LocationAuditLog>();
    public DbSet<ShipMethodAuditLog> ShipMethodAuditLogs => Set<ShipMethodAuditLog>();
    public DbSet<ProductSubcategoryAuditLog> ProductSubcategoryAuditLogs => Set<ProductSubcategoryAuditLog>();
    public DbSet<ProductDescriptionAuditLog> ProductDescriptionAuditLogs => Set<ProductDescriptionAuditLog>();
    public DbSet<SpecialOfferAuditLog> SpecialOfferAuditLogs => Set<SpecialOfferAuditLog>();
    public DbSet<StateProvinceAuditLog> StateProvinceAuditLogs => Set<StateProvinceAuditLog>();
    public DbSet<SalesTerritoryAuditLog> SalesTerritoryAuditLogs => Set<SalesTerritoryAuditLog>();
    public DbSet<SalesTaxRateAuditLog> SalesTaxRateAuditLogs => Set<SalesTaxRateAuditLog>();
    public DbSet<ShoppingCartItemAuditLog> ShoppingCartItemAuditLogs => Set<ShoppingCartItemAuditLog>();

    // Batch 3 audit logs.
    public DbSet<CustomerAuditLog> CustomerAuditLogs => Set<CustomerAuditLog>();
    public DbSet<SalesPersonAuditLog> SalesPersonAuditLogs => Set<SalesPersonAuditLog>();
    public DbSet<SalesPersonQuotaHistoryAuditLog> SalesPersonQuotaHistoryAuditLogs => Set<SalesPersonQuotaHistoryAuditLog>();
    public DbSet<SalesOrderHeaderSalesReasonAuditLog> SalesOrderHeaderSalesReasonAuditLogs => Set<SalesOrderHeaderSalesReasonAuditLog>();
    public DbSet<CountryRegionCurrencyAuditLog> CountryRegionCurrencyAuditLogs => Set<CountryRegionCurrencyAuditLog>();
    public DbSet<CurrencyRateAuditLog> CurrencyRateAuditLogs => Set<CurrencyRateAuditLog>();
    public DbSet<BillOfMaterialsAuditLog> BillOfMaterialsAuditLogs => Set<BillOfMaterialsAuditLog>();
    public DbSet<WorkOrderAuditLog> WorkOrderAuditLogs => Set<WorkOrderAuditLog>();
    public DbSet<EmployeeDepartmentHistoryAuditLog> EmployeeDepartmentHistoryAuditLogs => Set<EmployeeDepartmentHistoryAuditLog>();
    public DbSet<ProductCostHistoryAuditLog> ProductCostHistoryAuditLogs => Set<ProductCostHistoryAuditLog>();

    // Batch 4 audit logs.
    public DbSet<AddressAuditLog> AddressAuditLogs => Set<AddressAuditLog>();
    public DbSet<BusinessEntityAuditLog> BusinessEntityAuditLogs => Set<BusinessEntityAuditLog>();
    public DbSet<PersonAuditLog> PersonAuditLogs => Set<PersonAuditLog>();
    public DbSet<EmailAddressAuditLog> EmailAddressAuditLogs => Set<EmailAddressAuditLog>();
    public DbSet<PersonPhoneAuditLog> PersonPhoneAuditLogs => Set<PersonPhoneAuditLog>();
    public DbSet<BusinessEntityAddressAuditLog> BusinessEntityAddressAuditLogs => Set<BusinessEntityAddressAuditLog>();
    public DbSet<BusinessEntityContactAuditLog> BusinessEntityContactAuditLogs => Set<BusinessEntityContactAuditLog>();

    // Batch 5 audit logs.
    public DbSet<ProductAuditLog> ProductAuditLogs => Set<ProductAuditLog>();
    public DbSet<ProductModelAuditLog> ProductModelAuditLogs => Set<ProductModelAuditLog>();
    public DbSet<IllustrationAuditLog> IllustrationAuditLogs => Set<IllustrationAuditLog>();
    public DbSet<ProductPhotoAuditLog> ProductPhotoAuditLogs => Set<ProductPhotoAuditLog>();
    public DbSet<ProductReviewAuditLog> ProductReviewAuditLogs => Set<ProductReviewAuditLog>();
    public DbSet<ProductInventoryAuditLog> ProductInventoryAuditLogs => Set<ProductInventoryAuditLog>();
    public DbSet<ProductListPriceHistoryAuditLog> ProductListPriceHistoryAuditLogs => Set<ProductListPriceHistoryAuditLog>();
    public DbSet<ProductProductPhotoAuditLog> ProductProductPhotoAuditLogs => Set<ProductProductPhotoAuditLog>();

    // Batch 6 audit logs.
    public DbSet<ProductModelIllustrationAuditLog> ProductModelIllustrationAuditLogs => Set<ProductModelIllustrationAuditLog>();
    public DbSet<ProductModelProductDescriptionCultureAuditLog> ProductModelProductDescriptionCultureAuditLogs => Set<ProductModelProductDescriptionCultureAuditLog>();
    public DbSet<WorkOrderRoutingAuditLog> WorkOrderRoutingAuditLogs => Set<WorkOrderRoutingAuditLog>();
    public DbSet<TransactionHistoryAuditLog> TransactionHistoryAuditLogs => Set<TransactionHistoryAuditLog>();
    public DbSet<TransactionHistoryArchiveAuditLog> TransactionHistoryArchiveAuditLogs => Set<TransactionHistoryArchiveAuditLog>();

    // Batch 7 audit logs.
    public DbSet<CreditCardAuditLog> CreditCardAuditLogs => Set<CreditCardAuditLog>();
    public DbSet<PersonCreditCardAuditLog> PersonCreditCardAuditLogs => Set<PersonCreditCardAuditLog>();
    public DbSet<SalesOrderHeaderAuditLog> SalesOrderHeaderAuditLogs => Set<SalesOrderHeaderAuditLog>();
    public DbSet<SalesOrderDetailAuditLog> SalesOrderDetailAuditLogs => Set<SalesOrderDetailAuditLog>();
    public DbSet<SalesTerritoryHistoryAuditLog> SalesTerritoryHistoryAuditLogs => Set<SalesTerritoryHistoryAuditLog>();
    public DbSet<SpecialOfferProductAuditLog> SpecialOfferProductAuditLogs => Set<SpecialOfferProductAuditLog>();
    public DbSet<StoreAuditLog> StoreAuditLogs => Set<StoreAuditLog>();

    // Batch 8 audit logs.
    public DbSet<VendorAuditLog> VendorAuditLogs => Set<VendorAuditLog>();
    public DbSet<ProductVendorAuditLog> ProductVendorAuditLogs => Set<ProductVendorAuditLog>();
    public DbSet<PurchaseOrderHeaderAuditLog> PurchaseOrderHeaderAuditLogs => Set<PurchaseOrderHeaderAuditLog>();
    public DbSet<PurchaseOrderDetailAuditLog> PurchaseOrderDetailAuditLogs => Set<PurchaseOrderDetailAuditLog>();
    public DbSet<EmployeeAuditLog> EmployeeAuditLogs => Set<EmployeeAuditLog>();
    public DbSet<EmployeePayHistoryAuditLog> EmployeePayHistoryAuditLogs => Set<EmployeePayHistoryAuditLog>();
    public DbSet<JobCandidateAuditLog> JobCandidateAuditLogs => Set<JobCandidateAuditLog>();

    // Batch 9 audit logs.
    public DbSet<DocumentAuditLog> DocumentAuditLogs => Set<DocumentAuditLog>();
    public DbSet<ProductDocumentAuditLog> ProductDocumentAuditLogs => Set<ProductDocumentAuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

        // ToolSlotAuditLogs IS EF-managed (unlike the source table). No FK to
        // ToolSlotConfigurations because that table is excluded from migrations and lives in
        // a different-namespace PK ("CID"); we just store the integer id.
        builder.Entity<ToolSlotAuditLog>(b =>
        {
            b.HasIndex(x => x.ToolSlotConfigurationId);
            b.HasIndex(x => x.ChangedDate);
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
        builder.Entity<AddressTypeAuditLog>(b =>
        {
            b.ToTable("AddressTypeAuditLogs");
            b.HasIndex(x => x.AddressTypeId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ContactTypeAuditLog>(b =>
        {
            b.ToTable("ContactTypeAuditLogs");
            b.HasIndex(x => x.ContactTypeId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<CountryRegionAuditLog>(b =>
        {
            b.ToTable("CountryRegionAuditLogs");
            b.HasIndex(x => x.CountryRegionCode);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<PhoneNumberTypeAuditLog>(b =>
        {
            b.ToTable("PhoneNumberTypeAuditLogs");
            b.HasIndex(x => x.PhoneNumberTypeId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<CultureAuditLog>(b =>
        {
            b.ToTable("CultureAuditLogs");
            b.HasIndex(x => x.CultureId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductCategoryAuditLog>(b =>
        {
            b.ToTable("ProductCategoryAuditLogs");
            b.HasIndex(x => x.ProductCategoryId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ScrapReasonAuditLog>(b =>
        {
            b.ToTable("ScrapReasonAuditLogs");
            b.HasIndex(x => x.ScrapReasonId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<UnitMeasureAuditLog>(b =>
        {
            b.ToTable("UnitMeasureAuditLogs");
            b.HasIndex(x => x.UnitMeasureCode);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<CurrencyAuditLog>(b =>
        {
            b.ToTable("CurrencyAuditLogs");
            b.HasIndex(x => x.CurrencyCode);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesReasonAuditLog>(b =>
        {
            b.ToTable("SalesReasonAuditLogs");
            b.HasIndex(x => x.SalesReasonId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<DepartmentAuditLog>(b =>
        {
            b.ToTable("DepartmentAuditLogs");
            b.HasIndex(x => x.DepartmentId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ShiftAuditLog>(b =>
        {
            b.ToTable("ShiftAuditLogs");
            b.HasIndex(x => x.ShiftId);
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 2 audit-log configuration.
        builder.Entity<LocationAuditLog>(b =>
        {
            b.ToTable("LocationAuditLogs");
            b.HasIndex(x => x.LocationId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ShipMethodAuditLog>(b =>
        {
            b.ToTable("ShipMethodAuditLogs");
            b.HasIndex(x => x.ShipMethodId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductSubcategoryAuditLog>(b =>
        {
            b.ToTable("ProductSubcategoryAuditLogs");
            b.HasIndex(x => x.ProductSubcategoryId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductDescriptionAuditLog>(b =>
        {
            b.ToTable("ProductDescriptionAuditLogs");
            b.HasIndex(x => x.ProductDescriptionId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SpecialOfferAuditLog>(b =>
        {
            b.ToTable("SpecialOfferAuditLogs");
            b.HasIndex(x => x.SpecialOfferId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<StateProvinceAuditLog>(b =>
        {
            b.ToTable("StateProvinceAuditLogs");
            b.HasIndex(x => x.StateProvinceId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesTerritoryAuditLog>(b =>
        {
            b.ToTable("SalesTerritoryAuditLogs");
            b.HasIndex(x => x.SalesTerritoryId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesTaxRateAuditLog>(b =>
        {
            b.ToTable("SalesTaxRateAuditLogs");
            b.HasIndex(x => x.SalesTaxRateId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ShoppingCartItemAuditLog>(b =>
        {
            b.ToTable("ShoppingCartItemAuditLogs");
            b.HasIndex(x => x.ShoppingCartItemId);
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 3 audit-log configuration.
        builder.Entity<CustomerAuditLog>(b =>
        {
            b.ToTable("CustomerAuditLogs");
            b.HasIndex(x => x.CustomerId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesPersonAuditLog>(b =>
        {
            b.ToTable("SalesPersonAuditLogs");
            b.HasIndex(x => x.SalesPersonId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesPersonQuotaHistoryAuditLog>(b =>
        {
            b.ToTable("SalesPersonQuotaHistoryAuditLogs");
            // Composite-key audit — index both key components for the history filter.
            b.HasIndex(x => new { x.BusinessEntityId, x.QuotaDate });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesOrderHeaderSalesReasonAuditLog>(b =>
        {
            b.ToTable("SalesOrderHeaderSalesReasonAuditLogs");
            b.HasIndex(x => new { x.SalesOrderId, x.SalesReasonId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<CountryRegionCurrencyAuditLog>(b =>
        {
            b.ToTable("CountryRegionCurrencyAuditLogs");
            b.HasIndex(x => new { x.CountryRegionCode, x.CurrencyCode });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<CurrencyRateAuditLog>(b =>
        {
            b.ToTable("CurrencyRateAuditLogs");
            b.HasIndex(x => x.CurrencyRateId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<BillOfMaterialsAuditLog>(b =>
        {
            b.ToTable("BillOfMaterialsAuditLogs");
            b.HasIndex(x => x.BillOfMaterialsId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<WorkOrderAuditLog>(b =>
        {
            b.ToTable("WorkOrderAuditLogs");
            b.HasIndex(x => x.WorkOrderId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<EmployeeDepartmentHistoryAuditLog>(b =>
        {
            b.ToTable("EmployeeDepartmentHistoryAuditLogs");
            // 4-column composite key — index the (business entity, start date) prefix for
            // history-page filters. Queries with more specific predicates still benefit.
            b.HasIndex(x => new { x.BusinessEntityId, x.StartDate });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductCostHistoryAuditLog>(b =>
        {
            b.ToTable("ProductCostHistoryAuditLogs");
            b.HasIndex(x => new { x.ProductId, x.StartDate });
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 4 audit-log configuration.
        builder.Entity<AddressAuditLog>(b =>
        {
            b.ToTable("AddressAuditLogs");
            b.HasIndex(x => x.AddressId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<BusinessEntityAuditLog>(b =>
        {
            b.ToTable("BusinessEntityAuditLogs");
            b.HasIndex(x => x.BusinessEntityId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<PersonAuditLog>(b =>
        {
            b.ToTable("PersonAuditLogs");
            b.HasIndex(x => x.PersonId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<EmailAddressAuditLog>(b =>
        {
            b.ToTable("EmailAddressAuditLogs");
            b.HasIndex(x => new { x.BusinessEntityId, x.EmailAddressId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<PersonPhoneAuditLog>(b =>
        {
            b.ToTable("PersonPhoneAuditLogs");
            // 3-column composite — index the leading (BusinessEntityId, PhoneNumberTypeId)
            // pair for the most common history-page filter; full-key lookups still work.
            b.HasIndex(x => new { x.BusinessEntityId, x.PhoneNumberTypeId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<BusinessEntityAddressAuditLog>(b =>
        {
            b.ToTable("BusinessEntityAddressAuditLogs");
            b.HasIndex(x => new { x.BusinessEntityId, x.AddressId, x.AddressTypeId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<BusinessEntityContactAuditLog>(b =>
        {
            b.ToTable("BusinessEntityContactAuditLogs");
            b.HasIndex(x => new { x.BusinessEntityId, x.PersonId, x.ContactTypeId });
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 5 audit-log configuration.
        builder.Entity<ProductAuditLog>(b =>
        {
            b.ToTable("ProductAuditLogs");
            b.HasIndex(x => x.ProductId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductModelAuditLog>(b =>
        {
            b.ToTable("ProductModelAuditLogs");
            b.HasIndex(x => x.ProductModelId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<IllustrationAuditLog>(b =>
        {
            b.ToTable("IllustrationAuditLogs");
            b.HasIndex(x => x.IllustrationId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductPhotoAuditLog>(b =>
        {
            b.ToTable("ProductPhotoAuditLogs");
            b.HasIndex(x => x.ProductPhotoId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductReviewAuditLog>(b =>
        {
            b.ToTable("ProductReviewAuditLogs");
            b.HasIndex(x => x.ProductReviewId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductInventoryAuditLog>(b =>
        {
            b.ToTable("ProductInventoryAuditLogs");
            b.HasIndex(x => new { x.ProductId, x.LocationId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductListPriceHistoryAuditLog>(b =>
        {
            b.ToTable("ProductListPriceHistoryAuditLogs");
            b.HasIndex(x => new { x.ProductId, x.StartDate });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductProductPhotoAuditLog>(b =>
        {
            b.ToTable("ProductProductPhotoAuditLogs");
            b.HasIndex(x => new { x.ProductId, x.ProductPhotoId });
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 6 audit-log configuration.
        builder.Entity<ProductModelIllustrationAuditLog>(b =>
        {
            b.ToTable("ProductModelIllustrationAuditLogs");
            b.HasIndex(x => new { x.ProductModelId, x.IllustrationId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductModelProductDescriptionCultureAuditLog>(b =>
        {
            b.ToTable("ProductModelProductDescriptionCultureAuditLogs");
            b.HasIndex(x => new { x.ProductModelId, x.ProductDescriptionId, x.CultureId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<WorkOrderRoutingAuditLog>(b =>
        {
            b.ToTable("WorkOrderRoutingAuditLogs");
            b.HasIndex(x => new { x.WorkOrderId, x.ProductId, x.OperationSequence });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<TransactionHistoryAuditLog>(b =>
        {
            b.ToTable("TransactionHistoryAuditLogs");
            b.HasIndex(x => x.TransactionId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<TransactionHistoryArchiveAuditLog>(b =>
        {
            b.ToTable("TransactionHistoryArchiveAuditLogs");
            b.HasIndex(x => x.TransactionId);
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 7 audit-log configuration.
        builder.Entity<CreditCardAuditLog>(b =>
        {
            b.ToTable("CreditCardAuditLogs");
            b.HasIndex(x => x.CreditCardId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<PersonCreditCardAuditLog>(b =>
        {
            b.ToTable("PersonCreditCardAuditLogs");
            b.HasIndex(x => new { x.BusinessEntityId, x.CreditCardId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesOrderHeaderAuditLog>(b =>
        {
            b.ToTable("SalesOrderHeaderAuditLogs");
            b.HasIndex(x => x.SalesOrderId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesOrderDetailAuditLog>(b =>
        {
            b.ToTable("SalesOrderDetailAuditLogs");
            b.HasIndex(x => new { x.SalesOrderId, x.SalesOrderDetailId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SalesTerritoryHistoryAuditLog>(b =>
        {
            b.ToTable("SalesTerritoryHistoryAuditLogs");
            b.HasIndex(x => new { x.BusinessEntityId, x.StartDate });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<SpecialOfferProductAuditLog>(b =>
        {
            b.ToTable("SpecialOfferProductAuditLogs");
            b.HasIndex(x => new { x.SpecialOfferId, x.ProductId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<StoreAuditLog>(b =>
        {
            b.ToTable("StoreAuditLogs");
            b.HasIndex(x => x.StoreId);
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 8 audit-log configuration.
        builder.Entity<VendorAuditLog>(b =>
        {
            b.ToTable("VendorAuditLogs");
            b.HasIndex(x => x.VendorId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductVendorAuditLog>(b =>
        {
            b.ToTable("ProductVendorAuditLogs");
            b.HasIndex(x => new { x.ProductId, x.BusinessEntityId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<PurchaseOrderHeaderAuditLog>(b =>
        {
            b.ToTable("PurchaseOrderHeaderAuditLogs");
            b.HasIndex(x => x.PurchaseOrderId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<PurchaseOrderDetailAuditLog>(b =>
        {
            b.ToTable("PurchaseOrderDetailAuditLogs");
            b.HasIndex(x => new { x.PurchaseOrderId, x.PurchaseOrderDetailId });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<EmployeeAuditLog>(b =>
        {
            b.ToTable("EmployeeAuditLogs");
            b.HasIndex(x => x.EmployeeId);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<EmployeePayHistoryAuditLog>(b =>
        {
            b.ToTable("EmployeePayHistoryAuditLogs");
            b.HasIndex(x => new { x.BusinessEntityId, x.RateChangeDate });
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<JobCandidateAuditLog>(b =>
        {
            b.ToTable("JobCandidateAuditLogs");
            b.HasIndex(x => x.JobCandidateId);
            b.HasIndex(x => x.ChangedDate);
        });

        // Batch 9 audit-log configuration — Document + ProductDocument.
        // DocumentNode is stored as a string in the audit log (not HierarchyId) so it's
        // indexed as a regular nvarchar column.
        builder.Entity<DocumentAuditLog>(b =>
        {
            b.ToTable("DocumentAuditLogs");
            b.HasIndex(x => x.DocumentNode);
            b.HasIndex(x => x.ChangedDate);
        });
        builder.Entity<ProductDocumentAuditLog>(b =>
        {
            b.ToTable("ProductDocumentAuditLogs");
            b.HasIndex(x => new { x.ProductId, x.DocumentNode });
            b.HasIndex(x => x.ChangedDate);
        });
    }
}
