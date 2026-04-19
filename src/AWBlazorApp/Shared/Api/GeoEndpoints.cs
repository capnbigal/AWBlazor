using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Shared.Api;

public static class GeoEndpoints
{
    /// <summary>
    /// Reads <c>Person.Address.SpatialLocation</c> (SQL <c>geography</c>) as plain lat/lng doubles
    /// via the column's <c>.Lat</c> / <c>.Long</c> accessors. Avoids pulling NetTopologySuite into
    /// the EF model. Joins to StateProvince + SalesTerritory so markers can be grouped by territory.
    /// </summary>
    public static IEndpointRouteBuilder MapGeoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/geo")
            .WithTags("Geo")
            .RequireAuthorization("ApiOrCookie");

        group.MapGet("/addresses", async (
            ApplicationDbContext db,
            int? territoryId,
            int take,
            CancellationToken ct) =>
        {
            take = Math.Clamp(take == 0 ? 2000 : take, 1, 10_000);

            // Raw SQL projection — SpatialLocation is not in the EF model. Territory is picked up
            // via StateProvince.TerritoryID so we can group markers without a full address join.
            var sql = $@"
                SELECT TOP ({take})
                    a.AddressID        AS Id,
                    a.City             AS City,
                    sp.Name            AS StateProvince,
                    st.TerritoryID     AS TerritoryId,
                    st.Name            AS TerritoryName,
                    a.SpatialLocation.Lat  AS Latitude,
                    a.SpatialLocation.Long AS Longitude
                FROM Person.Address a
                INNER JOIN Person.StateProvince sp ON sp.StateProvinceID = a.StateProvinceID
                INNER JOIN Sales.SalesTerritory  st ON st.TerritoryID     = sp.TerritoryID
                WHERE a.SpatialLocation IS NOT NULL
                {(territoryId.HasValue ? "AND st.TerritoryID = {0}" : "")}
                ORDER BY a.AddressID";

            List<AddressMarker> rows = territoryId.HasValue
                ? await db.Database.SqlQueryRaw<AddressMarker>(sql, territoryId.Value).ToListAsync(ct)
                : await db.Database.SqlQueryRaw<AddressMarker>(sql).ToListAsync(ct);

            return Results.Ok(rows);
        })
        .WithName("ListAddressMarkers")
        .WithSummary("Latitude/longitude markers from Person.Address.SpatialLocation, grouped by sales territory.");

        group.MapGet("/territories", async (ApplicationDbContext db, CancellationToken ct) =>
        {
            var rows = await db.SalesTerritories.AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new TerritoryOption(t.Id, t.Name, t.CountryRegionCode))
                .ToListAsync(ct);
            return Results.Ok(rows);
        })
        .WithName("ListGeoTerritories");

        return app;
    }

    public sealed record AddressMarker(int Id, string City, string StateProvince, int TerritoryId, string TerritoryName, double Latitude, double Longitude);
    public sealed record TerritoryOption(int Id, string Name, string CountryRegionCode);
}
