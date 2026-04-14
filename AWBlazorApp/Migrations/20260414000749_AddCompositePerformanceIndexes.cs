using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AWBlazorApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositePerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.CreateTable(
                name: "AddressAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    City = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StateProvinceId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddressTypeAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressTypeAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleReads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ArticleSlug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArticleReads_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillOfMaterialsAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillOfMaterialsId = table.Column<int>(type: "int", nullable: false),
                    ProductAssemblyId = table.Column<int>(type: "int", nullable: true),
                    ComponentId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    BomLevel = table.Column<short>(type: "smallint", nullable: false),
                    PerAssemblyQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfMaterialsAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessEntityAddressAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    AddressTypeId = table.Column<int>(type: "int", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEntityAddressAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessEntityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEntityAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessEntityContactAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    ContactTypeId = table.Column<int>(type: "int", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessEntityContactAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactTypeAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContactTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactTypeAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CountryRegionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryRegionCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryRegionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CountryRegionCurrencyAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountryRegionCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryRegionCurrencyAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CreditCardAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditCardId = table.Column<int>(type: "int", nullable: false),
                    CardType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CardNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    ExpMonth = table.Column<byte>(type: "tinyint", nullable: false),
                    ExpYear = table.Column<short>(type: "smallint", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditCardAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CultureAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CultureId = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyRateAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyRateId = table.Column<int>(type: "int", nullable: false),
                    CurrencyRateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ToCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    AverageRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EndOfDayRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyRateAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PersonId = table.Column<int>(type: "int", nullable: true),
                    StoreId = table.Column<int>(type: "int", nullable: true),
                    TerritoryId = table.Column<int>(type: "int", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<short>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentNode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Owner = table.Column<int>(type: "int", nullable: false),
                    FolderFlag = table.Column<bool>(type: "bit", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    FileExtension = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Revision = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ChangeNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailAddressAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    EmailAddressId = table.Column<int>(type: "int", nullable: false),
                    EmailAddressValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAddressAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    NationalIDNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    LoginID = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalariedFlag = table.Column<bool>(type: "bit", nullable: false),
                    CurrentFlag = table.Column<bool>(type: "bit", nullable: false),
                    VacationHours = table.Column<short>(type: "smallint", nullable: false),
                    SickLeaveHours = table.Column<short>(type: "smallint", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeDepartmentHistoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<short>(type: "smallint", nullable: false),
                    ShiftId = table.Column<byte>(type: "tinyint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeDepartmentHistoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePayHistoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    RateChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayFrequency = table.Column<byte>(type: "tinyint", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePayHistoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForecastDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataSource = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    Granularity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LookbackMonths = table.Column<int>(type: "int", nullable: false),
                    HorizonPeriods = table.Column<int>(type: "int", nullable: false),
                    MethodParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastComputedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IllustrationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IllustrationId = table.Column<int>(type: "int", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IllustrationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobCandidateAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobCandidateId = table.Column<int>(type: "int", nullable: false),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCandidateAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<short>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CostRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Availability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonId = table.Column<int>(type: "int", nullable: false),
                    PersonType = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    NameStyle = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Suffix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EmailPromotion = table.Column<int>(type: "int", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonCreditCardAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    CreditCardId = table.Column<int>(type: "int", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonCreditCardAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonPhoneAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    PhoneNumberTypeId = table.Column<int>(type: "int", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonPhoneAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PhoneNumberTypeAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumberTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneNumberTypeAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Processes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DepartmentID = table.Column<short>(type: "smallint", nullable: false),
                    DefaultProcessorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    CronSchedule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NextRunDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Processes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Processes_AspNetUsers_DefaultProcessorUserId",
                        column: x => x.DefaultProcessorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Processes_Department_DepartmentID",
                        column: x => x.DepartmentID,
                        principalSchema: "HumanResources",
                        principalTable: "Department",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    MakeFlag = table.Column<bool>(type: "bit", nullable: false),
                    FinishedGoodsFlag = table.Column<bool>(type: "bit", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    SafetyStockLevel = table.Column<short>(type: "smallint", nullable: false),
                    ReorderPoint = table.Column<short>(type: "smallint", nullable: false),
                    StandardCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ListPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    SizeUnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    WeightUnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DaysToManufacture = table.Column<int>(type: "int", nullable: false),
                    ProductLine = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Class = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Style = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    ProductSubcategoryId = table.Column<int>(type: "int", nullable: true),
                    ProductModelId = table.Column<int>(type: "int", nullable: true),
                    SellStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SellEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscontinuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCostHistoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StandardCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCostHistoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductDescriptionAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductDescriptionId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDescriptionAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductDocumentAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    DocumentNode = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDocumentAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductInventoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<short>(type: "smallint", nullable: false),
                    Shelf = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Bin = table.Column<byte>(type: "tinyint", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInventoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductListPriceHistoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ListPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductListPriceHistoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductModelAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductModelId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModelAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductModelIllustrationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductModelId = table.Column<int>(type: "int", nullable: false),
                    IllustrationId = table.Column<int>(type: "int", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModelIllustrationAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductModelProductDescriptionCultureAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductModelId = table.Column<int>(type: "int", nullable: false),
                    ProductDescriptionId = table.Column<int>(type: "int", nullable: false),
                    CultureId = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModelProductDescriptionCultureAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductPhotoAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductPhotoId = table.Column<int>(type: "int", nullable: false),
                    ThumbnailPhotoFileName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LargePhotoFileName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPhotoAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductPhotoAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductPhotoId = table.Column<int>(type: "int", nullable: false),
                    Primary = table.Column<bool>(type: "bit", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductPhotoAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductReviewAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductReviewId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ReviewerName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(3850)", maxLength: 3850, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviewAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductSubcategoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductSubcategoryId = table.Column<int>(type: "int", nullable: false),
                    ProductCategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSubcategoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductVendorAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    AverageLeadTime = table.Column<int>(type: "int", nullable: false),
                    StandardPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastReceiptCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LastReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MinOrderQty = table.Column<int>(type: "int", nullable: false),
                    MaxOrderQty = table.Column<int>(type: "int", nullable: false),
                    OnOrderQty = table.Column<int>(type: "int", nullable: true),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVendorAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderDetailAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderQty = table.Column<short>(type: "smallint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RejectedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderDetailAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderHeaderAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ShipMethodId = table.Column<int>(type: "int", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderHeaderAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetailAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    CarrierTrackingNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    OrderQty = table.Column<short>(type: "smallint", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SpecialOfferId = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPriceDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetailAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderHeaderAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShipDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    OnlineOrderFlag = table.Column<bool>(type: "bit", nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    SalesPersonId = table.Column<int>(type: "int", nullable: true),
                    TerritoryId = table.Column<int>(type: "int", nullable: true),
                    BillToAddressId = table.Column<int>(type: "int", nullable: false),
                    ShipToAddressId = table.Column<int>(type: "int", nullable: false),
                    ShipMethodId = table.Column<int>(type: "int", nullable: false),
                    CreditCardId = table.Column<int>(type: "int", nullable: true),
                    CreditCardApprovalCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CurrencyRateId = table.Column<int>(type: "int", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderHeaderAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderHeaderSalesReasonAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    SalesReasonId = table.Column<int>(type: "int", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderHeaderSalesReasonAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesPersonAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesPersonId = table.Column<int>(type: "int", nullable: false),
                    TerritoryId = table.Column<int>(type: "int", nullable: true),
                    SalesQuota = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Bonus = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionPct = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesYtd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesLastYear = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPersonAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesPersonQuotaHistoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    QuotaDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesQuota = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesPersonQuotaHistoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReasonAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesReasonId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReasonType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReasonAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesTaxRateAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesTaxRateId = table.Column<int>(type: "int", nullable: false),
                    StateProvinceId = table.Column<int>(type: "int", nullable: false),
                    TaxType = table.Column<byte>(type: "tinyint", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTaxRateAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesTerritoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesTerritoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SalesYtd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesLastYear = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostYtd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostLastYear = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTerritoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesTerritoryHistoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessEntityId = table.Column<int>(type: "int", nullable: false),
                    TerritoryId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesTerritoryHistoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScrapReasonAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScrapReasonId = table.Column<short>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrapReasonAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftId = table.Column<byte>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipMethodAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShipMethodId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShipBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipMethodAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCartItemAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShoppingCartItemId = table.Column<int>(type: "int", nullable: false),
                    ShoppingCartId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartItemAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialOfferAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialOfferId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DiscountPct = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OfferType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MinQty = table.Column<int>(type: "int", nullable: false),
                    MaxQty = table.Column<int>(type: "int", nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialOfferAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecialOfferProductAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialOfferId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialOfferProductAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StateProvinceAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateProvinceId = table.Column<int>(type: "int", nullable: false),
                    StateProvinceCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CountryRegionCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    IsOnlyStateProvinceFlag = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TerritoryId = table.Column<int>(type: "int", nullable: false),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateProvinceAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoreAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SalesPersonId = table.Column<int>(type: "int", nullable: true),
                    RowGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionHistoryArchiveAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ReferenceOrderId = table.Column<int>(type: "int", nullable: false),
                    ReferenceOrderLineId = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHistoryArchiveAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionHistoryAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ReferenceOrderId = table.Column<int>(type: "int", nullable: false),
                    ReferenceOrderLineId = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHistoryAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnitMeasureAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitMeasureCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitMeasureAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAreaPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Area = table.Column<int>(type: "int", nullable: false),
                    PermissionLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAreaPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAreaPermissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreditRating = table.Column<byte>(type: "tinyint", nullable: false),
                    PreferredVendorStatus = table.Column<bool>(type: "bit", nullable: false),
                    ActiveFlag = table.Column<bool>(type: "bit", nullable: false),
                    PurchasingWebServiceUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OrderQty = table.Column<int>(type: "int", nullable: false),
                    StockedQty = table.Column<int>(type: "int", nullable: false),
                    ScrappedQty = table.Column<short>(type: "smallint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScrapReasonId = table.Column<short>(type: "smallint", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderRoutingAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OperationSequence = table.Column<short>(type: "smallint", nullable: false),
                    LocationId = table.Column<short>(type: "smallint", nullable: false),
                    ScheduledStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualResourceHrs = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PlannedCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SourceModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeSummary = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderRoutingAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForecastDataPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForecastDefinitionId = table.Column<int>(type: "int", nullable: false),
                    PeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ForecastedValue = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ActualValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Variance = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    VariancePercent = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    EvaluatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastDataPoints_ForecastDefinitions_ForecastDefinitionId",
                        column: x => x.ForecastDefinitionId,
                        principalTable: "ForecastDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForecastHistoricalSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForecastDefinitionId = table.Column<int>(type: "int", nullable: false),
                    PeriodDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastHistoricalSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastHistoricalSnapshots_ForecastDefinitions_ForecastDefinitionId",
                        column: x => x.ForecastDefinitionId,
                        principalTable: "ForecastDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<int>(type: "int", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessExecutions_AspNetUsers_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcessExecutions_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessSteps_Processes_ProcessId",
                        column: x => x.ProcessId,
                        principalTable: "Processes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessStepExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcessExecutionId = table.Column<int>(type: "int", nullable: false),
                    ProcessStepId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessStepExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessStepExecutions_AspNetUsers_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProcessStepExecutions_ProcessExecutions_ProcessExecutionId",
                        column: x => x.ProcessExecutionId,
                        principalTable: "ProcessExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessStepExecutions_ProcessSteps_ProcessStepId",
                        column: x => x.ProcessStepId,
                        principalTable: "ProcessSteps",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressAuditLogs_AddressId",
                table: "AddressAuditLogs",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressAuditLogs_ChangedDate",
                table: "AddressAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_AddressTypeAuditLogs_AddressTypeId",
                table: "AddressTypeAuditLogs",
                column: "AddressTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressTypeAuditLogs_ChangedDate",
                table: "AddressTypeAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleReads_ArticleSlug",
                table: "ArticleReads",
                column: "ArticleSlug");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleReads_UserId_ArticleSlug",
                table: "ArticleReads",
                columns: new[] { "UserId", "ArticleSlug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialsAuditLogs_BillOfMaterialsId",
                table: "BillOfMaterialsAuditLogs",
                column: "BillOfMaterialsId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfMaterialsAuditLogs_ChangedDate",
                table: "BillOfMaterialsAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityAddressAuditLogs_BusinessEntityId_AddressId_AddressTypeId",
                table: "BusinessEntityAddressAuditLogs",
                columns: new[] { "BusinessEntityId", "AddressId", "AddressTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityAddressAuditLogs_ChangedDate",
                table: "BusinessEntityAddressAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityAuditLogs_BusinessEntityId",
                table: "BusinessEntityAuditLogs",
                column: "BusinessEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityAuditLogs_ChangedDate",
                table: "BusinessEntityAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityContactAuditLogs_BusinessEntityId_PersonId_ContactTypeId",
                table: "BusinessEntityContactAuditLogs",
                columns: new[] { "BusinessEntityId", "PersonId", "ContactTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessEntityContactAuditLogs_ChangedDate",
                table: "BusinessEntityContactAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContactTypeAuditLogs_ChangedDate",
                table: "ContactTypeAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContactTypeAuditLogs_ContactTypeId",
                table: "ContactTypeAuditLogs",
                column: "ContactTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryRegionAuditLogs_ChangedDate",
                table: "CountryRegionAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CountryRegionAuditLogs_CountryRegionCode",
                table: "CountryRegionAuditLogs",
                column: "CountryRegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_CountryRegionCurrencyAuditLogs_ChangedDate",
                table: "CountryRegionCurrencyAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CountryRegionCurrencyAuditLogs_CountryRegionCode_CurrencyCode",
                table: "CountryRegionCurrencyAuditLogs",
                columns: new[] { "CountryRegionCode", "CurrencyCode" });

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardAuditLogs_ChangedDate",
                table: "CreditCardAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CreditCardAuditLogs_CreditCardId",
                table: "CreditCardAuditLogs",
                column: "CreditCardId");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAuditLogs_ChangedDate",
                table: "CultureAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CultureAuditLogs_CultureId",
                table: "CultureAuditLogs",
                column: "CultureId");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAuditLogs_ChangedDate",
                table: "CurrencyAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAuditLogs_CurrencyCode",
                table: "CurrencyAuditLogs",
                column: "CurrencyCode");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRateAuditLogs_ChangedDate",
                table: "CurrencyRateAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRateAuditLogs_CurrencyRateId",
                table: "CurrencyRateAuditLogs",
                column: "CurrencyRateId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAuditLogs_ChangedDate",
                table: "CustomerAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAuditLogs_CustomerId",
                table: "CustomerAuditLogs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAuditLogs_ChangedDate",
                table: "DepartmentAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAuditLogs_DepartmentId",
                table: "DepartmentAuditLogs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAuditLogs_ChangedDate",
                table: "DocumentAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAuditLogs_DocumentNode",
                table: "DocumentAuditLogs",
                column: "DocumentNode");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAddressAuditLogs_BusinessEntityId_EmailAddressId",
                table: "EmailAddressAuditLogs",
                columns: new[] { "BusinessEntityId", "EmailAddressId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailAddressAuditLogs_ChangedDate",
                table: "EmailAddressAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAuditLogs_ChangedDate",
                table: "EmployeeAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAuditLogs_EmployeeId",
                table: "EmployeeAuditLogs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentHistoryAuditLogs_BusinessEntityId_StartDate",
                table: "EmployeeDepartmentHistoryAuditLogs",
                columns: new[] { "BusinessEntityId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeDepartmentHistoryAuditLogs_ChangedDate",
                table: "EmployeeDepartmentHistoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayHistoryAuditLogs_BusinessEntityId_RateChangeDate",
                table: "EmployeePayHistoryAuditLogs",
                columns: new[] { "BusinessEntityId", "RateChangeDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePayHistoryAuditLogs_ChangedDate",
                table: "EmployeePayHistoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDataPoints_ForecastDefinitionId_PeriodDate",
                table: "ForecastDataPoints",
                columns: new[] { "ForecastDefinitionId", "PeriodDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDataPoints_PeriodDate",
                table: "ForecastDataPoints",
                column: "PeriodDate");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDefinitions_CreatedDate",
                table: "ForecastDefinitions",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDefinitions_DataSource",
                table: "ForecastDefinitions",
                column: "DataSource");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDefinitions_DeletedDate",
                table: "ForecastDefinitions",
                column: "DeletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDefinitions_Status",
                table: "ForecastDefinitions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastDefinitions_Status_DeletedDate",
                table: "ForecastDefinitions",
                columns: new[] { "Status", "DeletedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ForecastHistoricalSnapshots_ForecastDefinitionId_PeriodDate",
                table: "ForecastHistoricalSnapshots",
                columns: new[] { "ForecastDefinitionId", "PeriodDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IllustrationAuditLogs_ChangedDate",
                table: "IllustrationAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_IllustrationAuditLogs_IllustrationId",
                table: "IllustrationAuditLogs",
                column: "IllustrationId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCandidateAuditLogs_ChangedDate",
                table: "JobCandidateAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_JobCandidateAuditLogs_JobCandidateId",
                table: "JobCandidateAuditLogs",
                column: "JobCandidateId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationAuditLogs_ChangedDate",
                table: "LocationAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_LocationAuditLogs_LocationId",
                table: "LocationAuditLogs",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAuditLogs_ChangedDate",
                table: "PersonAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PersonAuditLogs_PersonId",
                table: "PersonAuditLogs",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonCreditCardAuditLogs_BusinessEntityId_CreditCardId",
                table: "PersonCreditCardAuditLogs",
                columns: new[] { "BusinessEntityId", "CreditCardId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonCreditCardAuditLogs_ChangedDate",
                table: "PersonCreditCardAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPhoneAuditLogs_BusinessEntityId_PhoneNumberTypeId",
                table: "PersonPhoneAuditLogs",
                columns: new[] { "BusinessEntityId", "PhoneNumberTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonPhoneAuditLogs_ChangedDate",
                table: "PersonPhoneAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumberTypeAuditLogs_ChangedDate",
                table: "PhoneNumberTypeAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumberTypeAuditLogs_PhoneNumberTypeId",
                table: "PhoneNumberTypeAuditLogs",
                column: "PhoneNumberTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_DefaultProcessorUserId",
                table: "Processes",
                column: "DefaultProcessorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_DeletedDate",
                table: "Processes",
                column: "DeletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_DepartmentID",
                table: "Processes",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_IsRecurring",
                table: "Processes",
                column: "IsRecurring");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_NextRunDate",
                table: "Processes",
                column: "NextRunDate");

            migrationBuilder.CreateIndex(
                name: "IX_Processes_Status",
                table: "Processes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessExecutions_AssignedUserId",
                table: "ProcessExecutions",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessExecutions_ExecutionDate",
                table: "ProcessExecutions",
                column: "ExecutionDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessExecutions_ProcessId",
                table: "ProcessExecutions",
                column: "ProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessExecutions_Status",
                table: "ProcessExecutions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepExecutions_CompletedByUserId",
                table: "ProcessStepExecutions",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepExecutions_ProcessExecutionId_ProcessStepId",
                table: "ProcessStepExecutions",
                columns: new[] { "ProcessExecutionId", "ProcessStepId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessStepExecutions_ProcessStepId",
                table: "ProcessStepExecutions",
                column: "ProcessStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessSteps_ProcessId_SequenceNumber",
                table: "ProcessSteps",
                columns: new[] { "ProcessId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductAuditLogs_ChangedDate",
                table: "ProductAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductAuditLogs_ProductId",
                table: "ProductAuditLogs",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryAuditLogs_ChangedDate",
                table: "ProductCategoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryAuditLogs_ProductCategoryId",
                table: "ProductCategoryAuditLogs",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostHistoryAuditLogs_ChangedDate",
                table: "ProductCostHistoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCostHistoryAuditLogs_ProductId_StartDate",
                table: "ProductCostHistoryAuditLogs",
                columns: new[] { "ProductId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductDescriptionAuditLogs_ChangedDate",
                table: "ProductDescriptionAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDescriptionAuditLogs_ProductDescriptionId",
                table: "ProductDescriptionAuditLogs",
                column: "ProductDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDocumentAuditLogs_ChangedDate",
                table: "ProductDocumentAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDocumentAuditLogs_ProductId_DocumentNode",
                table: "ProductDocumentAuditLogs",
                columns: new[] { "ProductId", "DocumentNode" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductInventoryAuditLogs_ChangedDate",
                table: "ProductInventoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInventoryAuditLogs_ProductId_LocationId",
                table: "ProductInventoryAuditLogs",
                columns: new[] { "ProductId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductListPriceHistoryAuditLogs_ChangedDate",
                table: "ProductListPriceHistoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductListPriceHistoryAuditLogs_ProductId_StartDate",
                table: "ProductListPriceHistoryAuditLogs",
                columns: new[] { "ProductId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductModelAuditLogs_ChangedDate",
                table: "ProductModelAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModelAuditLogs_ProductModelId",
                table: "ProductModelAuditLogs",
                column: "ProductModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModelIllustrationAuditLogs_ChangedDate",
                table: "ProductModelIllustrationAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModelIllustrationAuditLogs_ProductModelId_IllustrationId",
                table: "ProductModelIllustrationAuditLogs",
                columns: new[] { "ProductModelId", "IllustrationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductModelProductDescriptionCultureAuditLogs_ChangedDate",
                table: "ProductModelProductDescriptionCultureAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductModelProductDescriptionCultureAuditLogs_ProductModelId_ProductDescriptionId_CultureId",
                table: "ProductModelProductDescriptionCultureAuditLogs",
                columns: new[] { "ProductModelId", "ProductDescriptionId", "CultureId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotoAuditLogs_ChangedDate",
                table: "ProductPhotoAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPhotoAuditLogs_ProductPhotoId",
                table: "ProductPhotoAuditLogs",
                column: "ProductPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductPhotoAuditLogs_ChangedDate",
                table: "ProductProductPhotoAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductPhotoAuditLogs_ProductId_ProductPhotoId",
                table: "ProductProductPhotoAuditLogs",
                columns: new[] { "ProductId", "ProductPhotoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviewAuditLogs_ChangedDate",
                table: "ProductReviewAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviewAuditLogs_ProductReviewId",
                table: "ProductReviewAuditLogs",
                column: "ProductReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubcategoryAuditLogs_ChangedDate",
                table: "ProductSubcategoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSubcategoryAuditLogs_ProductSubcategoryId",
                table: "ProductSubcategoryAuditLogs",
                column: "ProductSubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVendorAuditLogs_ChangedDate",
                table: "ProductVendorAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVendorAuditLogs_ProductId_BusinessEntityId",
                table: "ProductVendorAuditLogs",
                columns: new[] { "ProductId", "BusinessEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetailAuditLogs_ChangedDate",
                table: "PurchaseOrderDetailAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderDetailAuditLogs_PurchaseOrderId_PurchaseOrderDetailId",
                table: "PurchaseOrderDetailAuditLogs",
                columns: new[] { "PurchaseOrderId", "PurchaseOrderDetailId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeaderAuditLogs_ChangedDate",
                table: "PurchaseOrderHeaderAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeaderAuditLogs_PurchaseOrderId",
                table: "PurchaseOrderHeaderAuditLogs",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetailAuditLogs_ChangedDate",
                table: "SalesOrderDetailAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetailAuditLogs_SalesOrderId_SalesOrderDetailId",
                table: "SalesOrderDetailAuditLogs",
                columns: new[] { "SalesOrderId", "SalesOrderDetailId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeaderAuditLogs_ChangedDate",
                table: "SalesOrderHeaderAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeaderAuditLogs_SalesOrderId",
                table: "SalesOrderHeaderAuditLogs",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeaderSalesReasonAuditLogs_ChangedDate",
                table: "SalesOrderHeaderSalesReasonAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeaderSalesReasonAuditLogs_SalesOrderId_SalesReasonId",
                table: "SalesOrderHeaderSalesReasonAuditLogs",
                columns: new[] { "SalesOrderId", "SalesReasonId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonAuditLogs_ChangedDate",
                table: "SalesPersonAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonAuditLogs_SalesPersonId",
                table: "SalesPersonAuditLogs",
                column: "SalesPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonQuotaHistoryAuditLogs_BusinessEntityId_QuotaDate",
                table: "SalesPersonQuotaHistoryAuditLogs",
                columns: new[] { "BusinessEntityId", "QuotaDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesPersonQuotaHistoryAuditLogs_ChangedDate",
                table: "SalesPersonQuotaHistoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReasonAuditLogs_ChangedDate",
                table: "SalesReasonAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReasonAuditLogs_SalesReasonId",
                table: "SalesReasonAuditLogs",
                column: "SalesReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTaxRateAuditLogs_ChangedDate",
                table: "SalesTaxRateAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTaxRateAuditLogs_SalesTaxRateId",
                table: "SalesTaxRateAuditLogs",
                column: "SalesTaxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTerritoryAuditLogs_ChangedDate",
                table: "SalesTerritoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTerritoryAuditLogs_SalesTerritoryId",
                table: "SalesTerritoryAuditLogs",
                column: "SalesTerritoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesTerritoryHistoryAuditLogs_BusinessEntityId_StartDate",
                table: "SalesTerritoryHistoryAuditLogs",
                columns: new[] { "BusinessEntityId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesTerritoryHistoryAuditLogs_ChangedDate",
                table: "SalesTerritoryHistoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapReasonAuditLogs_ChangedDate",
                table: "ScrapReasonAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScrapReasonAuditLogs_ScrapReasonId",
                table: "ScrapReasonAuditLogs",
                column: "ScrapReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_Timestamp",
                table: "SecurityAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_UserId",
                table: "SecurityAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditLogs_UserId_Timestamp",
                table: "SecurityAuditLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAuditLogs_ChangedDate",
                table: "ShiftAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAuditLogs_ShiftId",
                table: "ShiftAuditLogs",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipMethodAuditLogs_ChangedDate",
                table: "ShipMethodAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ShipMethodAuditLogs_ShipMethodId",
                table: "ShipMethodAuditLogs",
                column: "ShipMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItemAuditLogs_ChangedDate",
                table: "ShoppingCartItemAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItemAuditLogs_ShoppingCartItemId",
                table: "ShoppingCartItemAuditLogs",
                column: "ShoppingCartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferAuditLogs_ChangedDate",
                table: "SpecialOfferAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferAuditLogs_SpecialOfferId",
                table: "SpecialOfferAuditLogs",
                column: "SpecialOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferProductAuditLogs_ChangedDate",
                table: "SpecialOfferProductAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOfferProductAuditLogs_SpecialOfferId_ProductId",
                table: "SpecialOfferProductAuditLogs",
                columns: new[] { "SpecialOfferId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_StateProvinceAuditLogs_ChangedDate",
                table: "StateProvinceAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StateProvinceAuditLogs_StateProvinceId",
                table: "StateProvinceAuditLogs",
                column: "StateProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreAuditLogs_ChangedDate",
                table: "StoreAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StoreAuditLogs_StoreId",
                table: "StoreAuditLogs",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistoryArchiveAuditLogs_ChangedDate",
                table: "TransactionHistoryArchiveAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistoryArchiveAuditLogs_TransactionId",
                table: "TransactionHistoryArchiveAuditLogs",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistoryAuditLogs_ChangedDate",
                table: "TransactionHistoryAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistoryAuditLogs_TransactionId",
                table: "TransactionHistoryAuditLogs",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitMeasureAuditLogs_ChangedDate",
                table: "UnitMeasureAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UnitMeasureAuditLogs_UnitMeasureCode",
                table: "UnitMeasureAuditLogs",
                column: "UnitMeasureCode");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreaPermissions_Area",
                table: "UserAreaPermissions",
                column: "Area");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreaPermissions_UserId_Area",
                table: "UserAreaPermissions",
                columns: new[] { "UserId", "Area" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorAuditLogs_ChangedDate",
                table: "VendorAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_VendorAuditLogs_VendorId",
                table: "VendorAuditLogs",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAuditLogs_ChangedDate",
                table: "WorkOrderAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderAuditLogs_WorkOrderId",
                table: "WorkOrderAuditLogs",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderRoutingAuditLogs_ChangedDate",
                table: "WorkOrderRoutingAuditLogs",
                column: "ChangedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderRoutingAuditLogs_WorkOrderId_ProductId_OperationSequence",
                table: "WorkOrderRoutingAuditLogs",
                columns: new[] { "WorkOrderId", "ProductId", "OperationSequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressAuditLogs");

            migrationBuilder.DropTable(
                name: "AddressTypeAuditLogs");

            migrationBuilder.DropTable(
                name: "ArticleReads");

            migrationBuilder.DropTable(
                name: "BillOfMaterialsAuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessEntityAddressAuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessEntityAuditLogs");

            migrationBuilder.DropTable(
                name: "BusinessEntityContactAuditLogs");

            migrationBuilder.DropTable(
                name: "ContactTypeAuditLogs");

            migrationBuilder.DropTable(
                name: "CountryRegionAuditLogs");

            migrationBuilder.DropTable(
                name: "CountryRegionCurrencyAuditLogs");

            migrationBuilder.DropTable(
                name: "CreditCardAuditLogs");

            migrationBuilder.DropTable(
                name: "CultureAuditLogs");

            migrationBuilder.DropTable(
                name: "CurrencyAuditLogs");

            migrationBuilder.DropTable(
                name: "CurrencyRateAuditLogs");

            migrationBuilder.DropTable(
                name: "CustomerAuditLogs");

            migrationBuilder.DropTable(
                name: "DepartmentAuditLogs");

            migrationBuilder.DropTable(
                name: "DocumentAuditLogs");

            migrationBuilder.DropTable(
                name: "EmailAddressAuditLogs");

            migrationBuilder.DropTable(
                name: "EmployeeAuditLogs");

            migrationBuilder.DropTable(
                name: "EmployeeDepartmentHistoryAuditLogs");

            migrationBuilder.DropTable(
                name: "EmployeePayHistoryAuditLogs");

            migrationBuilder.DropTable(
                name: "ForecastDataPoints");

            migrationBuilder.DropTable(
                name: "ForecastHistoricalSnapshots");

            migrationBuilder.DropTable(
                name: "IllustrationAuditLogs");

            migrationBuilder.DropTable(
                name: "JobCandidateAuditLogs");

            migrationBuilder.DropTable(
                name: "LocationAuditLogs");

            migrationBuilder.DropTable(
                name: "PersonAuditLogs");

            migrationBuilder.DropTable(
                name: "PersonCreditCardAuditLogs");

            migrationBuilder.DropTable(
                name: "PersonPhoneAuditLogs");

            migrationBuilder.DropTable(
                name: "PhoneNumberTypeAuditLogs");

            migrationBuilder.DropTable(
                name: "ProcessStepExecutions");

            migrationBuilder.DropTable(
                name: "ProductAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductCategoryAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductCostHistoryAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductDescriptionAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductDocumentAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductInventoryAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductListPriceHistoryAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductModelAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductModelIllustrationAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductModelProductDescriptionCultureAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductPhotoAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductProductPhotoAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductReviewAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductSubcategoryAuditLogs");

            migrationBuilder.DropTable(
                name: "ProductVendorAuditLogs");

            migrationBuilder.DropTable(
                name: "PurchaseOrderDetailAuditLogs");

            migrationBuilder.DropTable(
                name: "PurchaseOrderHeaderAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesOrderDetailAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesOrderHeaderAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesOrderHeaderSalesReasonAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesPersonAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesPersonQuotaHistoryAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesReasonAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesTaxRateAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesTerritoryAuditLogs");

            migrationBuilder.DropTable(
                name: "SalesTerritoryHistoryAuditLogs");

            migrationBuilder.DropTable(
                name: "ScrapReasonAuditLogs");

            migrationBuilder.DropTable(
                name: "SecurityAuditLogs");

            migrationBuilder.DropTable(
                name: "ShiftAuditLogs");

            migrationBuilder.DropTable(
                name: "ShipMethodAuditLogs");

            migrationBuilder.DropTable(
                name: "ShoppingCartItemAuditLogs");

            migrationBuilder.DropTable(
                name: "SpecialOfferAuditLogs");

            migrationBuilder.DropTable(
                name: "SpecialOfferProductAuditLogs");

            migrationBuilder.DropTable(
                name: "StateProvinceAuditLogs");

            migrationBuilder.DropTable(
                name: "StoreAuditLogs");

            migrationBuilder.DropTable(
                name: "TransactionHistoryArchiveAuditLogs");

            migrationBuilder.DropTable(
                name: "TransactionHistoryAuditLogs");

            migrationBuilder.DropTable(
                name: "UnitMeasureAuditLogs");

            migrationBuilder.DropTable(
                name: "UserAreaPermissions");

            migrationBuilder.DropTable(
                name: "VendorAuditLogs");

            migrationBuilder.DropTable(
                name: "WorkOrderAuditLogs");

            migrationBuilder.DropTable(
                name: "WorkOrderRoutingAuditLogs");

            migrationBuilder.DropTable(
                name: "ForecastDefinitions");

            migrationBuilder.DropTable(
                name: "ProcessExecutions");

            migrationBuilder.DropTable(
                name: "ProcessSteps");

            migrationBuilder.DropTable(
                name: "Processes");

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Discount = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BookingEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookingStartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cancelled = table.Column<bool>(type: "bit", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomNumber = table.Column<int>(type: "int", nullable: false),
                    RoomType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CouponId",
                table: "Bookings",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CreatedDate",
                table: "Bookings",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DeletedDate",
                table: "Bookings",
                column: "DeletedDate");
        }
    }
}
