namespace AWBlazorApp.Models.ApiExplorer;

/// <summary>
/// Curated catalog of featured endpoints for the interactive API explorer page.
/// Not an auto-generated OpenAPI spec — a hand-picked set that shows off the surface area
/// and includes meaningful default parameter values for a good demo out of the box.
/// </summary>
public static class ApiEndpointCatalog
{
    public static readonly IReadOnlyList<ApiEndpoint> Endpoints = new List<ApiEndpoint>
    {
        // ── AdventureWorks reference ───────────────────────────────────────────────────
        new("products", "GET", "/api/aw/products", "Sales",
            "List products with optional name filter.",
            new[]
            {
                new ApiParam("name", ApiParamType.String, "Product name contains (case-insensitive).", DefaultValue: "bike"),
                new ApiParam("skip", ApiParamType.Int,    "Rows to skip for paging.",                  DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int,    "Page size (1-1000).",                       DefaultValue: "20"),
            }),

        new("sales-orders", "GET", "/api/aw/sales-order-headers", "Sales",
            "List sales order headers.",
            new[]
            {
                new ApiParam("skip", ApiParamType.Int, "Rows to skip.", DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int, "Page size.",    DefaultValue: "25"),
            }),

        new("customers", "GET", "/api/aw/customers", "Sales",
            "List customer rows.",
            new[]
            {
                new ApiParam("skip", ApiParamType.Int, "Rows to skip.", DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int, "Page size.",    DefaultValue: "25"),
            }),

        // ── Production ─────────────────────────────────────────────────────────────────
        new("work-orders", "GET", "/api/aw/work-orders", "Production",
            "List work orders with optional filters.",
            new[]
            {
                new ApiParam("skip", ApiParamType.Int, "Rows to skip.", DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int, "Page size.",    DefaultValue: "25"),
            }),

        new("product-categories", "GET", "/api/aw/product-categories", "Production",
            "List product categories.",
            new[]
            {
                new ApiParam("name", ApiParamType.String, "Category name contains.", DefaultValue: ""),
                new ApiParam("skip", ApiParamType.Int,    "Rows to skip.",           DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int,    "Page size.",              DefaultValue: "50"),
            }),

        // ── Purchasing ─────────────────────────────────────────────────────────────────
        new("vendors", "GET", "/api/aw/vendors", "Purchasing",
            "List vendors.",
            new[]
            {
                new ApiParam("skip", ApiParamType.Int, "Rows to skip.", DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int, "Page size.",    DefaultValue: "25"),
            }),

        // ── HR ─────────────────────────────────────────────────────────────────────────
        new("departments", "GET", "/api/aw/departments", "HR",
            "List departments.",
            new[]
            {
                new ApiParam("name", ApiParamType.String, "Department name contains.", DefaultValue: ""),
                new ApiParam("skip", ApiParamType.Int,    "Rows to skip.",             DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int,    "Page size.",                DefaultValue: "50"),
            }),

        new("employees", "GET", "/api/aw/employees", "HR",
            "List employees.",
            new[]
            {
                new ApiParam("skip", ApiParamType.Int, "Rows to skip.", DefaultValue: "0"),
                new ApiParam("take", ApiParamType.Int, "Page size.",    DefaultValue: "25"),
            }),

        // ── Geo ────────────────────────────────────────────────────────────────────────
        new("geo-addresses", "GET", "/api/geo/addresses", "Geo",
            "List address rows with lat/lng from Person.Address.SpatialLocation.",
            new[]
            {
                new ApiParam("territoryId", ApiParamType.Int, "Optional sales territory id (0 = all).", DefaultValue: "0"),
                new ApiParam("take",        ApiParamType.Int, "Max markers (1-10000).",                 DefaultValue: "500"),
            }),

        new("geo-territories", "GET", "/api/geo/territories", "Geo",
            "List sales territories (ids + names + country code).",
            Array.Empty<ApiParam>()),
    };
}

public sealed record ApiEndpoint(
    string Id,
    string Method,
    string Path,
    string Category,
    string Summary,
    IReadOnlyList<ApiParam> Parameters);

public sealed record ApiParam(
    string Name,
    ApiParamType Type,
    string? Description = null,
    string? DefaultValue = null);

public enum ApiParamType { String, Int, Bool }
