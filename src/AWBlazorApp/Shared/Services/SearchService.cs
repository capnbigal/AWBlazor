using AWBlazorApp.Shared.Dtos;
using MudBlazor;

namespace AWBlazorApp.Shared.Services;

/// <summary>A single federated search hit — a real data record (not a page).</summary>
/// <param name="Category">User-facing type, e.g. "Product", "Customer".</param>
/// <param name="Label">Display text for the record.</param>
/// <param name="Icon">Material icon for the category.</param>
/// <param name="Url">Deep link — the record's detail page (e.g. aw/products/707).</param>
public sealed record RecordHit(string Category, string Label, string Icon, string Url);

/// <summary>
/// Federated record search across the most-used entities. Reuses <see cref="LookupService"/>'s
/// existing DB-backed, Take(25)-bounded search methods (each opens its own DbContext from the
/// factory, so they run safely in parallel) and projects the results into typed, deep-linkable
/// hits. Powers the app-bar <c>GlobalSearch</c>. Stateless → singleton.
/// </summary>
public sealed class SearchService(LookupService lookup)
{
    /// <summary>
    /// Searches Products, Customers, Sales orders, Vendors, Employees, Persons, Work orders, and Purchase orders for the query and
    /// returns up to <paramref name="perCategory"/> hits per category. Each hit deep-links to that
    /// record's detail page (aw/&lt;entity&gt;/{id}). Returns empty for queries shorter than 2 characters.
    /// </summary>
    public async Task<IReadOnlyList<RecordHit>> SearchRecordsAsync(string? query, int perCategory = 4)
    {
        query = query?.Trim() ?? "";
        if (query.Length < 2) return [];

        var products  = lookup.SearchProductsAsync(query);
        var customers = lookup.SearchCustomersAsync(query);
        var orders    = lookup.SearchSalesOrdersAsync(query);
        var vendors   = lookup.SearchVendorsAsync(query);
        var employees = lookup.SearchEmployeesAsync(query);
        var persons   = lookup.SearchPersonsAsync(query);
        var workOrders     = lookup.SearchWorkOrdersAsync(query);
        var purchaseOrders = lookup.SearchPurchaseOrdersAsync(query);
        await Task.WhenAll(products, customers, orders, vendors, employees, persons, workOrders, purchaseOrders);

        var hits = new List<RecordHit>();

        // Deep-link each hit to that record's detail page by id (e.g. aw/products/707).
        void Add(Task<IEnumerable<LookupItem<int>>> task, string category, string icon, string route)
        {
            foreach (var item in task.Result.Take(perCategory))
                hits.Add(new RecordHit(category, item.DisplayText, icon, $"{route}/{item.Id}"));
        }

        Add(products,  "Product",     Icons.Material.Filled.Inventory2, "aw/products");
        Add(customers, "Customer",    Icons.Material.Filled.PeopleAlt,  "aw/customers");
        Add(orders,    "Sales order", Icons.Material.Filled.Receipt,    "aw/sales-order-headers");
        Add(vendors,   "Vendor",      Icons.Material.Filled.Business,   "aw/vendors");
        Add(employees, "Employee",    Icons.Material.Filled.Badge,      "aw/employees");
        Add(persons,   "Person",      Icons.Material.Filled.Person,     "aw/persons");
        Add(workOrders,     "Work order",     Icons.Material.Filled.Construction, "aw/work-orders");
        Add(purchaseOrders, "Purchase order", Icons.Material.Filled.Receipt,      "aw/purchase-order-headers");

        return hits;
    }
}
