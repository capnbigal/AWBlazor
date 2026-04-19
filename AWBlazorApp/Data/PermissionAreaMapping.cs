namespace AWBlazorApp.Data;

/// <summary>
/// Maps API route prefixes and Blazor page routes to <see cref="PermissionArea"/> values.
/// Used by <see cref="AWBlazorApp.Startup.AreaPermissionMiddleware"/> to resolve which
/// area a request targets without modifying individual endpoint files.
/// </summary>
public static class PermissionAreaMapping
{
    // Sorted longest-prefix-first so more specific routes match before general ones.
    private static readonly (string Prefix, PermissionArea Area)[] ApiRoutes =
    [
        // Human Resources
        ("/api/departments", PermissionArea.HumanResources),
        ("/api/employees", PermissionArea.HumanResources),
        ("/api/employee-department-histories", PermissionArea.HumanResources),
        ("/api/employee-pay-histories", PermissionArea.HumanResources),
        ("/api/job-candidates", PermissionArea.HumanResources),
        ("/api/shifts", PermissionArea.HumanResources),

        // Production
        ("/api/products", PermissionArea.Production),
        ("/api/product-categories", PermissionArea.Production),
        ("/api/product-subcategories", PermissionArea.Production),
        ("/api/product-models", PermissionArea.Production),
        ("/api/product-descriptions", PermissionArea.Production),
        ("/api/product-photos", PermissionArea.Production),
        ("/api/product-product-photos", PermissionArea.Production),
        ("/api/product-model-illustrations", PermissionArea.Production),
        ("/api/product-model-product-description-cultures", PermissionArea.Production),
        ("/api/product-reviews", PermissionArea.Production),
        ("/api/product-inventories", PermissionArea.Production),
        ("/api/product-list-price-histories", PermissionArea.Production),
        ("/api/product-cost-histories", PermissionArea.Production),
        ("/api/product-documents", PermissionArea.Production),
        ("/api/bill-of-materials", PermissionArea.Production),
        ("/api/work-orders", PermissionArea.Production),
        ("/api/work-order-routings", PermissionArea.Production),
        ("/api/transaction-histories", PermissionArea.Production),
        ("/api/transaction-history-archives", PermissionArea.Production),
        ("/api/illustrations", PermissionArea.Production),
        ("/api/locations", PermissionArea.Production),
        ("/api/cultures", PermissionArea.Production),
        ("/api/documents", PermissionArea.Production),
        ("/api/scrap-reasons", PermissionArea.Production),
        ("/api/unit-measures", PermissionArea.Production),

        // Sales
        ("/api/sales-order-headers", PermissionArea.Sales),
        ("/api/sales-order-details", PermissionArea.Sales),
        ("/api/sales-order-header-sales-reasons", PermissionArea.Sales),
        ("/api/sales-persons", PermissionArea.Sales),
        ("/api/sales-person-quota-histories", PermissionArea.Sales),
        ("/api/sales-territories", PermissionArea.Sales),
        ("/api/sales-territory-histories", PermissionArea.Sales),
        ("/api/sales-tax-rates", PermissionArea.Sales),
        ("/api/sales-reasons", PermissionArea.Sales),
        ("/api/customers", PermissionArea.Sales),
        ("/api/stores", PermissionArea.Sales),
        ("/api/credit-cards", PermissionArea.Sales),
        ("/api/person-credit-cards", PermissionArea.Sales),
        ("/api/currencies", PermissionArea.Sales),
        ("/api/currency-rates", PermissionArea.Sales),
        ("/api/country-region-currencies", PermissionArea.Sales),
        ("/api/special-offers", PermissionArea.Sales),
        ("/api/special-offer-products", PermissionArea.Sales),
        ("/api/shopping-cart-items", PermissionArea.Sales),

        // Purchasing
        ("/api/vendors", PermissionArea.Purchasing),
        ("/api/product-vendors", PermissionArea.Purchasing),
        ("/api/purchase-order-headers", PermissionArea.Purchasing),
        ("/api/purchase-order-details", PermissionArea.Purchasing),
        ("/api/ship-methods", PermissionArea.Purchasing),

        // Person
        ("/api/persons", PermissionArea.Person),
        ("/api/addresses", PermissionArea.Person),
        ("/api/address-types", PermissionArea.Person),
        ("/api/business-entities", PermissionArea.Person),
        ("/api/business-entity-addresses", PermissionArea.Person),
        ("/api/business-entity-contacts", PermissionArea.Person),
        ("/api/contact-types", PermissionArea.Person),
        ("/api/country-regions", PermissionArea.Person),
        ("/api/email-addresses", PermissionArea.Person),
        ("/api/person-phones", PermissionArea.Person),
        ("/api/phone-number-types", PermissionArea.Person),
        ("/api/state-provinces", PermissionArea.Person),

        // App features
        ("/api/forecasts", PermissionArea.Forecasts),
        ("/api/tool-slots", PermissionArea.ToolSlots),
        ("/api/admin", PermissionArea.Admin),
        ("/api/permissions", PermissionArea.Admin),
        ("/api/processes", PermissionArea.Processes),

        // Enterprise master data
        ("/api/organizations", PermissionArea.Enterprise),
        ("/api/org-units", PermissionArea.Enterprise),
        ("/api/stations", PermissionArea.Enterprise),
        ("/api/assets", PermissionArea.Enterprise),
        ("/api/cost-centers", PermissionArea.Enterprise),
        ("/api/product-lines", PermissionArea.Enterprise),

        // Advanced inventory (Phase B)
        ("/api/inventory-items", PermissionArea.Inventory),
        ("/api/inventory-locations", PermissionArea.Inventory),
        ("/api/lots", PermissionArea.Inventory),
        ("/api/serial-units", PermissionArea.Inventory),
        ("/api/inventory-balances", PermissionArea.Inventory),
        ("/api/inventory-transactions", PermissionArea.Inventory),
        ("/api/inventory-transaction-types", PermissionArea.Inventory),
        ("/api/inventory-adjustments", PermissionArea.Inventory),
        ("/api/inventory-outbox", PermissionArea.Inventory),
        ("/api/inventory-queue", PermissionArea.Inventory),

        // Logistics (Module M3)
        ("/api/goods-receipts", PermissionArea.Logistics),
        ("/api/shipments", PermissionArea.Logistics),
        ("/api/stock-transfers", PermissionArea.Logistics),

        // MES — production execution (Module M4)
        ("/api/production-runs", PermissionArea.Mes),
        ("/api/operator-clock-events", PermissionArea.Mes),
        ("/api/downtime-events", PermissionArea.Mes),
        ("/api/downtime-reasons", PermissionArea.Mes),
        ("/api/work-instructions", PermissionArea.Mes),

        // Quality (Module M5)
        ("/api/inspection-plans", PermissionArea.Quality),
        ("/api/inspections", PermissionArea.Quality),
        ("/api/non-conformances", PermissionArea.Quality),
        ("/api/capa-cases", PermissionArea.Quality),

        // Workforce (Module M7)
        ("/api/training-courses", PermissionArea.Workforce),
        ("/api/training-records", PermissionArea.Workforce),
        ("/api/qualifications", PermissionArea.Workforce),
        ("/api/employee-qualifications", PermissionArea.Workforce),
        ("/api/station-qualifications", PermissionArea.Workforce),
        ("/api/qualification-alerts", PermissionArea.Workforce),
        ("/api/attendance-events", PermissionArea.Workforce),
        ("/api/leave-requests", PermissionArea.Workforce),
        ("/api/shift-handover-notes", PermissionArea.Workforce),
        ("/api/announcements", PermissionArea.Workforce),

        // Engineering (Module M9)
        ("/api/manufacturing-routings", PermissionArea.Engineering),
        ("/api/boms", PermissionArea.Engineering),
        ("/api/engineering-change-orders", PermissionArea.Engineering),
        ("/api/engineering-documents", PermissionArea.Engineering),
        ("/api/deviation-requests", PermissionArea.Engineering),

        // Maintenance (Module M6)
        ("/api/asset-maintenance-profiles", PermissionArea.Maintenance),
        ("/api/pm-schedules", PermissionArea.Maintenance),
        ("/api/maintenance-work-orders", PermissionArea.Maintenance),
        ("/api/spare-parts", PermissionArea.Maintenance),
        ("/api/meter-readings", PermissionArea.Maintenance),
        ("/api/maintenance-logs", PermissionArea.Maintenance),

        // Performance (Module M8)
        ("/api/oee-snapshots", PermissionArea.Performance),
        ("/api/production-metrics", PermissionArea.Performance),
        ("/api/maintenance-metrics", PermissionArea.Performance),
        ("/api/kpi-definitions", PermissionArea.Performance),
        ("/api/kpi-values", PermissionArea.Performance),
        ("/api/scorecards", PermissionArea.Performance),
        ("/api/performance-reports", PermissionArea.Performance),

        // Cross-module plant dashboard.
        ("/api/dashboard", PermissionArea.Analytics),
    ];

    /// <summary>
    /// Resolves a <see cref="PermissionArea"/> from an API request path.
    /// Returns null if the path doesn't match any known area (request passes through).
    /// </summary>
    public static PermissionArea? ResolveFromApiRoute(string path)
    {
        foreach (var (prefix, area) in ApiRoutes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return area;
        }
        return null;
    }

    /// <summary>
    /// Maps an HTTP method to the minimum <see cref="PermissionLevel"/> required.
    /// </summary>
    public static PermissionLevel RequiredLevelForMethod(string method) => method.ToUpperInvariant() switch
    {
        "GET" or "HEAD" or "OPTIONS" => PermissionLevel.Read,
        "POST" or "PUT" or "PATCH" => PermissionLevel.Write,
        "DELETE" => PermissionLevel.Admin,
        _ => PermissionLevel.Read,
    };
}
