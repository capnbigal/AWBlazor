using System.Data;
using System.Text;
using ElementaryApp.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ElementaryApp.Endpoints;

public static class ExportEndpoints
{
    /// <summary>
    /// Known schema+table pairs that may be exported. The key is "Schema.Table" (case-insensitive lookup).
    /// Only tables in this allowlist can be exported — prevents arbitrary SQL injection via the route.
    /// </summary>
    private static readonly HashSet<string> AllowedTables = new(StringComparer.OrdinalIgnoreCase)
    {
        // dbo
        "dbo.Bookings", "dbo.Coupons", "dbo.ApiKeys", "dbo.ToolSlotConfigurations", "dbo.RequestLogs",

        // Person
        "Person.AddressType", "Person.Address", "Person.BusinessEntity", "Person.BusinessEntityAddress",
        "Person.BusinessEntityContact", "Person.ContactType", "Person.CountryRegion", "Person.EmailAddress",
        "Person.Person", "Person.PersonPhone", "Person.PhoneNumberType", "Person.StateProvince",

        // Production
        "Production.BillOfMaterials", "Production.Culture", "Production.Document", "Production.Illustration",
        "Production.Location", "Production.Product", "Production.ProductCategory", "Production.ProductCostHistory",
        "Production.ProductDescription", "Production.ProductDocument", "Production.ProductInventory",
        "Production.ProductListPriceHistory", "Production.ProductModel", "Production.ProductModelIllustration",
        "Production.ProductModelProductDescriptionCulture", "Production.ProductPhoto", "Production.ProductProductPhoto",
        "Production.ProductReview", "Production.ProductSubcategory", "Production.ScrapReason",
        "Production.TransactionHistory", "Production.TransactionHistoryArchive", "Production.UnitMeasure",
        "Production.WorkOrder", "Production.WorkOrderRouting",

        // Sales
        "Sales.CountryRegionCurrency", "Sales.CreditCard", "Sales.Currency", "Sales.CurrencyRate",
        "Sales.Customer", "Sales.PersonCreditCard", "Sales.SalesOrderDetail", "Sales.SalesOrderHeader",
        "Sales.SalesOrderHeaderSalesReason", "Sales.SalesPerson", "Sales.SalesPersonQuotaHistory",
        "Sales.SalesReason", "Sales.SalesTaxRate", "Sales.SalesTerritory", "Sales.SalesTerritoryHistory",
        "Sales.ShoppingCartItem", "Sales.SpecialOffer", "Sales.SpecialOfferProduct", "Sales.Store",

        // Purchasing
        "Purchasing.ProductVendor", "Purchasing.PurchaseOrderDetail", "Purchasing.PurchaseOrderHeader",
        "Purchasing.ShipMethod", "Purchasing.Vendor",

        // HumanResources
        "HumanResources.Department", "HumanResources.Employee", "HumanResources.EmployeeDepartmentHistory",
        "HumanResources.EmployeePayHistory", "HumanResources.JobCandidate", "HumanResources.Shift",
    };

    public static IEndpointRouteBuilder MapExportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/export")
            .WithTags("Export")
            .RequireAuthorization(p => p.RequireRole(AppRoles.Admin));

        group.MapGet("/{schema}/{table}", async (
            string schema,
            string table,
            int? take,
            IDbContextFactory<ApplicationDbContext> dbFactory,
            CancellationToken ct) =>
        {
            var qualifiedName = $"{schema}.{table}";
            if (!AllowedTables.Contains(qualifiedName))
                return Results.NotFound($"Table '{qualifiedName}' is not in the export allowlist.");

            var rowLimit = Math.Clamp(take ?? 10_000, 1, 50_000);

            await using var db = await dbFactory.CreateDbContextAsync(ct);
            var conn = (SqlConnection)db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            // Use bracket-quoted identifiers (already validated against the allowlist).
            var sql = $"SELECT * FROM [{schema}].[{table}] ORDER BY 1 OFFSET 0 ROWS FETCH NEXT @take ROWS ONLY";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@take", rowLimit);

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            var sb = new StringBuilder();
            var fieldCount = reader.FieldCount;

            // Header row
            for (var i = 0; i < fieldCount; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(EscapeCsv(reader.GetName(i)));
            }
            sb.AppendLine();

            // Data rows
            while (await reader.ReadAsync(ct))
            {
                for (var i = 0; i < fieldCount; i++)
                {
                    if (i > 0) sb.Append(',');
                    if (!reader.IsDBNull(i))
                        sb.Append(EscapeCsv(reader.GetValue(i)?.ToString() ?? ""));
                }
                sb.AppendLine();
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            return Results.File(bytes, "text/csv", $"{table}.csv");
        })
        .WithName("ExportTableCsv")
        .WithSummary("Export any registered table to CSV (Admin only).");

        return app;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
