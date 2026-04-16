using AWBlazorApp.Data;
using AWBlazorApp.Infrastructure.Persistence;
using AWBlazorApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AWBlazorApp.Shared.Services;

/// <summary>
/// Centralized lookup provider for FK dropdown fields across all dialogs and filter panels.
/// Singleton — uses <see cref="IDbContextFactory{TContext}"/> for DB access and
/// <see cref="IMemoryCache"/> with 1-hour TTL for small-table lookups.
/// </summary>
public sealed class LookupService(IDbContextFactory<ApplicationDbContext> dbFactory, IMemoryCache cache)
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private static readonly string[] CacheKeys =
    [
        "lookup:AddressTypes",
        "lookup:ContactTypes",
        "lookup:CountryRegions",
        "lookup:Cultures",
        "lookup:Currencies",
        "lookup:Departments",
        "lookup:Locations",
        "lookup:PhoneNumberTypes",
        "lookup:ProductCategories",
        "lookup:ProductSubcategories",
        "lookup:ProductModels",
        "lookup:SalesReasons",
        "lookup:SalesTerritories",
        "lookup:ScrapReasons",
        "lookup:Shifts",
        "lookup:ShipMethods",
        "lookup:SpecialOffers",
        "lookup:UnitMeasures",
        "lookup:StateProvinces",
    ];

    public int ClearCache()
    {
        foreach (var key in CacheKeys) cache.Remove(key);
        return CacheKeys.Length;
    }

    // ── Small-table cached lookups (< ~200 rows, use FkSelect) ──────────────

    public Task<List<LookupItem<int>>> GetAddressTypesAsync() =>
        CachedAsync("lookup:AddressTypes", async db =>
            await db.AddressTypes.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetContactTypesAsync() =>
        CachedAsync("lookup:ContactTypes", async db =>
            await db.ContactTypes.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<string>>> GetCountryRegionsAsync() =>
        CachedAsync("lookup:CountryRegions", async db =>
            await db.CountryRegions.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<string>(x.CountryRegionCode, x.CountryRegionCode + " — " + x.Name)).ToListAsync());

    public Task<List<LookupItem<string>>> GetCulturesAsync() =>
        CachedAsync("lookup:Cultures", async db =>
            await db.Cultures.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<string>(x.CultureId, x.CultureId + " — " + x.Name)).ToListAsync());

    public Task<List<LookupItem<string>>> GetCurrenciesAsync() =>
        CachedAsync("lookup:Currencies", async db =>
            await db.Currencies.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<string>(x.CurrencyCode, x.CurrencyCode + " — " + x.Name)).ToListAsync());

    public Task<List<LookupItem<short>>> GetDepartmentsAsync() =>
        CachedAsync("lookup:Departments", async db =>
            await db.Departments.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<short>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<short>>> GetLocationsAsync() =>
        CachedAsync("lookup:Locations", async db =>
            await db.Locations.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<short>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetPhoneNumberTypesAsync() =>
        CachedAsync("lookup:PhoneNumberTypes", async db =>
            await db.PhoneNumberTypes.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetProductCategoriesAsync() =>
        CachedAsync("lookup:ProductCategories", async db =>
            await db.ProductCategories.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetProductSubcategoriesAsync() =>
        CachedAsync("lookup:ProductSubcategories", async db =>
            await db.ProductSubcategories.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetProductModelsAsync() =>
        CachedAsync("lookup:ProductModels", async db =>
            await db.ProductModels.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetSalesReasonsAsync() =>
        CachedAsync("lookup:SalesReasons", async db =>
            await db.SalesReasons.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetSalesTerritoriesAsync() =>
        CachedAsync("lookup:SalesTerritories", async db =>
            await db.SalesTerritories.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name + " (" + x.GroupName + ")")).ToListAsync());

    public Task<List<LookupItem<short>>> GetScrapReasonsAsync() =>
        CachedAsync("lookup:ScrapReasons", async db =>
            await db.ScrapReasons.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<short>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<byte>>> GetShiftsAsync() =>
        CachedAsync("lookup:Shifts", async db =>
            await db.Shifts.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<byte>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetShipMethodsAsync() =>
        CachedAsync("lookup:ShipMethods", async db =>
            await db.ShipMethods.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetSpecialOffersAsync() =>
        CachedAsync("lookup:SpecialOffers", async db =>
            await db.SpecialOffers.AsNoTracking().OrderBy(x => x.Description)
                .Select(x => new LookupItem<int>(x.Id, x.Description)).ToListAsync());

    public Task<List<LookupItem<string>>> GetUnitMeasuresAsync() =>
        CachedAsync("lookup:UnitMeasures", async db =>
            await db.UnitMeasures.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<string>(x.UnitMeasureCode, x.UnitMeasureCode + " — " + x.Name)).ToListAsync());

    public Task<List<LookupItem<int>>> GetStateProvincesAsync() =>
        CachedAsync("lookup:StateProvinces", async db =>
            await db.StateProvinces.AsNoTracking().OrderBy(x => x.Name)
                .Select(x => new LookupItem<int>(x.Id, x.Name + " (" + x.CountryRegionCode + ")")).ToListAsync());

    // ── Large-table async search (1000+ rows, use FkAutocomplete) ────────────

    public async Task<IEnumerable<LookupItem<int>>> SearchProductsAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.Name.Contains(query) || x.ProductNumber.Contains(query));
        return await q.OrderBy(x => x.Name).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.Name + " (" + x.ProductNumber + ")")).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchCustomersAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Customers.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.AccountNumber.Contains(query));
        return await q.OrderBy(x => x.Id).Take(25)
            .Select(x => new LookupItem<int>(x.Id, "#" + x.Id + " — " + x.AccountNumber)).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchPersonsAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Persons.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.LastName.Contains(query) || x.FirstName.Contains(query));
        return await q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.LastName + ", " + x.FirstName)).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchEmployeesAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Employees.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.LoginID.Contains(query) || x.JobTitle.Contains(query));
        return await q.OrderBy(x => x.LoginID).Take(25)
            .Select(x => new LookupItem<int>(x.Id, "#" + x.Id + " — " + x.LoginID + " (" + x.JobTitle + ")")).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchSalesPersonsAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.SalesPersons.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            if (int.TryParse(query, out var id))
                q = q.Where(x => x.Id == id);
        }
        return await q.OrderBy(x => x.Id).Take(25)
            .Select(x => new LookupItem<int>(x.Id, "#" + x.Id + " (YTD: " + x.SalesYtd.ToString("C0") + ")")).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchVendorsAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Vendors.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.Name.Contains(query) || x.AccountNumber.Contains(query));
        return await q.OrderBy(x => x.Name).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.Name + " (" + x.AccountNumber + ")")).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchStoresAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Stores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.Name.Contains(query));
        return await q.OrderBy(x => x.Name).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.Name)).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchAddressesAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Addresses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.AddressLine1.Contains(query) || x.City.Contains(query));
        return await q.OrderBy(x => x.City).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.AddressLine1 + ", " + x.City)).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchCreditCardsAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.CreditCards.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.CardType.Contains(query) || x.CardNumber.Contains(query));
        return await q.OrderBy(x => x.CardType).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.CardType + " ..." + x.CardNumber.Substring(x.CardNumber.Length - 4))).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchSalesOrdersAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.SalesOrderHeaders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.SalesOrderNumber.Contains(query));
        return await q.OrderByDescending(x => x.OrderDate).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.SalesOrderNumber + " (" + x.OrderDate.ToString("yyyy-MM-dd") + ")")).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchPurchaseOrdersAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.PurchaseOrderHeaders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            if (int.TryParse(query, out var id))
                q = q.Where(x => x.Id == id);
        }
        return await q.OrderByDescending(x => x.OrderDate).Take(25)
            .Select(x => new LookupItem<int>(x.Id, "PO #" + x.Id + " (" + x.OrderDate.ToString("yyyy-MM-dd") + ")")).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchWorkOrdersAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.WorkOrders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            if (int.TryParse(query, out var id))
                q = q.Where(x => x.Id == id);
        }
        return await q.OrderByDescending(x => x.StartDate).Take(25)
            .Select(x => new LookupItem<int>(x.Id, "WO #" + x.Id + " (Product " + x.ProductId + ")")).ToListAsync();
    }

    public async Task<IEnumerable<LookupItem<int>>> SearchCurrencyRatesAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.CurrencyRates.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(x => x.FromCurrencyCode.Contains(query) || x.ToCurrencyCode.Contains(query));
        return await q.OrderByDescending(x => x.CurrencyRateDate).Take(25)
            .Select(x => new LookupItem<int>(x.Id, x.FromCurrencyCode + "/" + x.ToCurrencyCode + " " + x.CurrencyRateDate.ToString("yyyy-MM-dd"))).ToListAsync();
    }

    // ── Helper to resolve a single entity name for edit-mode display ─────────

    public async Task<string?> GetProductNameAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Products.AsNoTracking().Where(x => x.Id == id).Select(x => x.Name + " (" + x.ProductNumber + ")").FirstOrDefaultAsync();
    }

    public async Task<string?> GetCustomerDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Customers.AsNoTracking().Where(x => x.Id == id).Select(x => "#" + x.Id + " — " + x.AccountNumber).FirstOrDefaultAsync();
    }

    public async Task<string?> GetPersonNameAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Persons.AsNoTracking().Where(x => x.Id == id).Select(x => x.LastName + ", " + x.FirstName).FirstOrDefaultAsync();
    }

    public async Task<string?> GetEmployeeDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Employees.AsNoTracking().Where(x => x.Id == id).Select(x => "#" + x.Id + " — " + x.LoginID).FirstOrDefaultAsync();
    }

    public async Task<string?> GetVendorNameAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Vendors.AsNoTracking().Where(x => x.Id == id).Select(x => x.Name).FirstOrDefaultAsync();
    }

    public async Task<string?> GetAddressDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Addresses.AsNoTracking().Where(x => x.Id == id).Select(x => x.AddressLine1 + ", " + x.City).FirstOrDefaultAsync();
    }

    public async Task<string?> GetStoreNameAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Stores.AsNoTracking().Where(x => x.Id == id).Select(x => x.Name).FirstOrDefaultAsync();
    }

    public async Task<string?> GetSalesPersonDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.SalesPersons.AsNoTracking().Where(x => x.Id == id).Select(x => "#" + x.Id + " (YTD: " + x.SalesYtd.ToString("C0") + ")").FirstOrDefaultAsync();
    }

    public async Task<string?> GetCreditCardDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.CreditCards.AsNoTracking().Where(x => x.Id == id).Select(x => x.CardType + " ..." + x.CardNumber.Substring(x.CardNumber.Length - 4)).FirstOrDefaultAsync();
    }

    public async Task<string?> GetCurrencyRateDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.CurrencyRates.AsNoTracking().Where(x => x.Id == id).Select(x => x.FromCurrencyCode + "/" + x.ToCurrencyCode + " " + x.CurrencyRateDate.ToString("yyyy-MM-dd")).FirstOrDefaultAsync();
    }

    public async Task<string?> GetShipMethodNameAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.ShipMethods.AsNoTracking().Where(x => x.Id == id).Select(x => x.Name).FirstOrDefaultAsync();
    }

    public async Task<string?> GetSalesOrderDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.SalesOrderHeaders.AsNoTracking().Where(x => x.Id == id).Select(x => x.SalesOrderNumber + " (" + x.OrderDate.ToString("yyyy-MM-dd") + ")").FirstOrDefaultAsync();
    }

    public async Task<string?> GetPurchaseOrderDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.PurchaseOrderHeaders.AsNoTracking().Where(x => x.Id == id).Select(x => "PO #" + x.Id + " (" + x.OrderDate.ToString("yyyy-MM-dd") + ")").FirstOrDefaultAsync();
    }

    public async Task<string?> GetWorkOrderDisplayAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.WorkOrders.AsNoTracking().Where(x => x.Id == id).Select(x => "WO #" + x.Id + " (Product " + x.ProductId + ")").FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<LookupItem<string>>> SearchUsersAsync(string query)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var q = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => (u.DisplayName ?? "").Contains(query) || (u.Email ?? "").Contains(query));
        return await q.OrderBy(u => u.DisplayName ?? u.UserName).Take(25)
            .Select(u => new LookupItem<string>(u.Id, (u.DisplayName ?? u.UserName ?? u.Email) ?? u.Id))
            .ToListAsync();
    }

    public async Task<string?> GetUserDisplayAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Users.AsNoTracking().Where(u => u.Id == userId)
            .Select(u => u.DisplayName ?? u.UserName ?? u.Email).FirstOrDefaultAsync();
    }

    // ── Cache helper ─────────────────────────────────────────────────────────

    private async Task<List<LookupItem<T>>> CachedAsync<T>(string key, Func<ApplicationDbContext, Task<List<LookupItem<T>>>> factory)
    {
        return (await cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            await using var db = await dbFactory.CreateDbContextAsync();
            return await factory(db);
        }))!;
    }
}
