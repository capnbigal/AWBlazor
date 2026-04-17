using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Models;
using AWBlazorApp.Features.Inventory.Services;
using AWBlazorApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

file static class IdResponseExtensions
{
    public static int AsInt(this IdResponse r) => r.Id switch
    {
        int i => i,
        long l => (int)l,
        JsonElement je => je.GetInt32(),
        IConvertible c => Convert.ToInt32(c),
        _ => throw new InvalidOperationException($"Unexpected IdResponse.Id type: {r.Id?.GetType()}"),
    };
}

/// <summary>
/// Integration tests for the Phase B advanced-inventory endpoints and service layer. Exercises:
///   1. All inventory endpoint groups require auth (401 / redirect anon, 200 with API key).
///   2. The 20 canonical transaction types are seeded by <c>DatabaseInitializer</c>.
///   3. <c>IInventoryService.PostTransactionAsync</c> credits the balance for a positive-sign
///      type (RECEIPT) and enqueues an outbox row because RECEIPT has <c>EmitsJson = true</c>.
///   4. A paired MOVE decrements the from-location balance and increments the to-location
///      balance without creating a net change in the system.
/// </summary>
public class InventoryEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] InventoryEndpointGroups =
    [
        "/api/inventory-items",
        "/api/inventory-locations",
        "/api/lots",
        "/api/serial-units",
        "/api/inventory-balances",
        "/api/inventory-transactions",
        "/api/inventory-transaction-types",
        "/api/inventory-adjustments",
        "/api/inventory-outbox",
        "/api/inventory-queue",
    ];

    private static IEnumerable<string> Groups => InventoryEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task InventoryEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
    {
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await client.GetAsync(endpoint);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.Redirect).Or.EqualTo(HttpStatusCode.Found),
            $"Expected 401/redirect for {endpoint}, got {(int)response.StatusCode}");
    }

    [TestCaseSource(nameof(Groups))]
    public async Task InventoryEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task Twenty_Transaction_Types_Are_Seeded()
    {
        await using var db = await GetDbContextAsync();
        var codes = await db.InventoryTransactionTypes.AsNoTracking().Select(t => t.Code).ToListAsync();

        Assert.That(codes, Has.Count.EqualTo(20),
            "DatabaseInitializer.SeedInventoryTransactionTypesAsync should land 20 rows on first boot.");
        Assert.That(codes, Does.Contain("RECEIPT"));
        Assert.That(codes, Does.Contain("SHIP"));
        Assert.That(codes, Does.Contain("MOVE"));
        Assert.That(codes, Does.Contain("ADJUST_INC"));
        Assert.That(codes, Does.Contain("ADJUST_DEC"));

        // RECEIPT emits JSON (it's how inbound ASN partners will eventually learn about goods arrivals).
        var receipt = await db.InventoryTransactionTypes.AsNoTracking().SingleAsync(t => t.Code == "RECEIPT");
        Assert.That(receipt.Sign, Is.EqualTo((sbyte)1));
        Assert.That(receipt.EmitsJson, Is.True);

        // SHIP is negative and also emits JSON (downstream partners care).
        var ship = await db.InventoryTransactionTypes.AsNoTracking().SingleAsync(t => t.Code == "SHIP");
        Assert.That(ship.Sign, Is.EqualTo((sbyte)-1));
        Assert.That(ship.EmitsJson, Is.True);

        // PUTAWAY is internal-only; not emitted.
        var putaway = await db.InventoryTransactionTypes.AsNoTracking().SingleAsync(t => t.Code == "PUTAWAY");
        Assert.That(putaway.EmitsJson, Is.False);
    }

    [Test]
    public async Task Posting_Receipt_Credits_Balance_And_Enqueues_Outbox()
    {
        var (itemId, warehouseId) = await SeedItemAndLocationAsync(prefix: "RCPT");
        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        var result = await service.PostTransactionAsync(new PostTransactionRequest(
            TypeCode: "RECEIPT",
            InventoryItemId: itemId,
            Quantity: 42m,
            UnitMeasureCode: "EA",
            FromLocationId: null,
            ToLocationId: warehouseId,
            LotId: null,
            SerialUnitId: null,
            FromStatus: null,
            ToStatus: null,
            ReferenceType: null,
            ReferenceId: null,
            ReferenceLineId: null,
            Notes: "integration test",
            CorrelationId: null,
            OccurredAt: null), "test@integration", CancellationToken.None);

        Assert.That(result.TransactionNumber, Does.StartWith("TXN-"));
        Assert.That(result.OutboxEnqueued, Is.True, "RECEIPT has EmitsJson=true, so an outbox row should exist.");

        await using var db = await GetDbContextAsync();
        var balance = await db.InventoryBalances.AsNoTracking()
            .SingleAsync(b => b.InventoryItemId == itemId && b.LocationId == warehouseId && b.Status == BalanceStatus.Available);
        Assert.That(balance.Quantity, Is.EqualTo(42m));
        Assert.That(balance.LastTransactionAt, Is.Not.Null);

        var outbox = await db.InventoryTransactionOutbox.AsNoTracking()
            .SingleAsync(o => o.InventoryTransactionId == result.TransactionId);
        Assert.That(outbox.Status, Is.EqualTo(OutboxStatus.Pending));
        Assert.That(outbox.Payload, Does.Contain("\"type\":\"RECEIPT\"").Or.Contain("\"type\": \"RECEIPT\""));

        await CleanupItemAsync(itemId);
    }

    [Test]
    public async Task Paired_Move_Debits_From_And_Credits_To_Without_Outbox()
    {
        var (itemId, fromLocId) = await SeedItemAndLocationAsync(prefix: "MOVE", codeSuffix: "FROM");
        var toLocId = await SeedLocationAsync(prefix: "MOVE", codeSuffix: "TO");

        using var scope = Factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        // Prime inventory at the from location via a RECEIPT.
        _ = await service.PostTransactionAsync(new PostTransactionRequest(
            "RECEIPT", itemId, 10m, "EA", null, fromLocId, null, null, null, null, null, null, null, null, null, null),
            "test@integration", CancellationToken.None);

        // Paired move: 7 units from→to.
        var move = await service.PostTransactionAsync(new PostTransactionRequest(
            "MOVE", itemId, 7m, "EA", fromLocId, toLocId, null, null, null, null, null, null, null, null, null, null),
            "test@integration", CancellationToken.None);

        Assert.That(move.OutboxEnqueued, Is.False, "MOVE has EmitsJson=false; no outbox row expected.");

        await using var db = await GetDbContextAsync();
        var fromBalance = await db.InventoryBalances.AsNoTracking()
            .SingleAsync(b => b.InventoryItemId == itemId && b.LocationId == fromLocId && b.Status == BalanceStatus.Available);
        var toBalance = await db.InventoryBalances.AsNoTracking()
            .SingleAsync(b => b.InventoryItemId == itemId && b.LocationId == toLocId && b.Status == BalanceStatus.Available);
        Assert.That(fromBalance.Quantity, Is.EqualTo(3m), "Receipt 10 − move 7 = 3 at the from location.");
        Assert.That(toBalance.Quantity, Is.EqualTo(7m), "Move 7 → to location should credit 7.");

        await CleanupItemAsync(itemId);
    }

    // --- Helpers ---

    private async Task<(int itemId, int locationId)> SeedItemAndLocationAsync(string prefix, string codeSuffix = "")
    {
        await using var db = await GetDbContextAsync();
        var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.IsPrimary);

        var productId = await db.Database
            .SqlQuery<int>($"SELECT TOP 1 ProductID AS Value FROM Production.Product ORDER BY ProductID")
            .FirstAsync();

        var code = $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}{codeSuffix}".ToUpperInvariant();
        var loc = new InventoryLocation
        {
            OrganizationId = org.Id,
            Code = code,
            Name = $"{prefix} test warehouse",
            Kind = InventoryLocationKind.Warehouse,
            Path = code,
            Depth = 0,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        db.InventoryLocations.Add(loc);

        var item = new InventoryItem
        {
            ProductId = productId,
            TracksLot = false,
            TracksSerial = false,
            MinQty = 0,
            MaxQty = 1000,
            ReorderPoint = 0,
            ReorderQty = 0,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };

        // ProductId is unique — if a row already exists for the first product, reuse it.
        var existing = await db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (existing is not null)
        {
            await db.SaveChangesAsync();
            return (existing.Id, loc.Id);
        }

        db.InventoryItems.Add(item);
        await db.SaveChangesAsync();
        return (item.Id, loc.Id);
    }

    private async Task<int> SeedLocationAsync(string prefix, string codeSuffix)
    {
        await using var db = await GetDbContextAsync();
        var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.IsPrimary);
        var code = $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}{codeSuffix}".ToUpperInvariant();
        var loc = new InventoryLocation
        {
            OrganizationId = org.Id,
            Code = code,
            Name = $"{prefix} test location",
            Kind = InventoryLocationKind.Warehouse,
            Path = code,
            Depth = 0,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        db.InventoryLocations.Add(loc);
        await db.SaveChangesAsync();
        return loc.Id;
    }

    private async Task CleanupItemAsync(int itemId)
    {
        // The ledger is append-only and the balance rows reference the item, but the tests run
        // against the dev DB so we don't need to tear them down aggressively — the data is real
        // and follow-on tests will just work around it. Just null out any FK pressure so a
        // subsequent run can re-use the seed product id.
        await using var db = await GetDbContextAsync();
        var locations = await db.InventoryLocations.Where(l => l.Code.StartsWith("RCPT-") || l.Code.StartsWith("MOVE-")).ToListAsync();
        // Leave the balances and transactions in place — they're intentional history.
        _ = locations; // no-op; keep the seed records around for visibility in the dev DB.
    }

    private static string? _adminApiKey;

    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;

        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_inventory_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "inventory-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
