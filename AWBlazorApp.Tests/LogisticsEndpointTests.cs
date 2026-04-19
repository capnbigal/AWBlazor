using System.Net;
using System.Text.Json;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Services;
using AWBlazorApp.Features.Logistics.Domain;
using AWBlazorApp.Features.Logistics.Services;
using AWBlazorApp.Shared.Dtos;
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
/// Integration tests for Module M3 — Logistics. Exercises:
///   1. Auth coverage on the three endpoint groups.
///   2. End-to-end receipt workflow: create header + line, then post → asserts inventory
///      balance is credited and the line carries back the InventoryTransaction id.
///   3. Paired transfer workflow: from-balance debits, to-balance credits, both transactions
///      share the same CorrelationId on the header.
///   4. Auto-create-Lot behavior: when a receipt line's item tracks lots and no LotId is
///      provided, posting auto-creates an inv.Lot row whose code includes the receipt number.
/// </summary>
public class LogisticsEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] LogisticsEndpointGroups =
    [
        "/api/goods-receipts",
        "/api/shipments",
        "/api/stock-transfers",
    ];

    private static IEnumerable<string> Groups => LogisticsEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task LogisticsEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
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
    public async Task LogisticsEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task Receipt_Post_Credits_Inventory_Balance_And_Backfills_Transaction_Id()
    {
        var (itemId, locationId) = await SeedItemAndLocationAsync("RCP-TEST");
        using var scope = Factory.Services.CreateScope();
        var posting = scope.ServiceProvider.GetRequiredService<ILogisticsPostingService>();

        int receiptId, lineId;
        await using (var seedDb = await GetDbContextAsync())
        {
            var receipt = new GoodsReceipt
            {
                ReceiptNumber = $"RCP-TEST-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                ReceivedLocationId = locationId,
                Status = GoodsReceiptStatus.Draft,
                ReceivedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            seedDb.GoodsReceipts.Add(receipt);
            await seedDb.SaveChangesAsync();
            receiptId = receipt.Id;

            seedDb.GoodsReceiptLines.Add(new GoodsReceiptLine
            {
                GoodsReceiptId = receiptId,
                InventoryItemId = itemId,
                Quantity = 12.5m,
                UnitMeasureCode = "EA",
                ModifiedDate = DateTime.UtcNow,
            });
            await seedDb.SaveChangesAsync();
            lineId = await seedDb.GoodsReceiptLines.Where(l => l.GoodsReceiptId == receiptId).Select(l => l.Id).FirstAsync();
        }

        var result = await posting.PostReceiptAsync(receiptId, "test@logistics", CancellationToken.None);
        Assert.That(result.LinesPosted, Is.EqualTo(1));
        Assert.That(result.TransactionIds, Has.Count.EqualTo(1));

        await using var verifyDb = await GetDbContextAsync();
        var header = await verifyDb.GoodsReceipts.AsNoTracking().FirstAsync(r => r.Id == receiptId);
        Assert.That(header.Status, Is.EqualTo(GoodsReceiptStatus.Posted));
        Assert.That(header.PostedAt, Is.Not.Null);

        var line = await verifyDb.GoodsReceiptLines.AsNoTracking().FirstAsync(l => l.Id == lineId);
        Assert.That(line.PostedTransactionId, Is.EqualTo(result.TransactionIds[0]));

        // Balance reflects the receipt — note the test seeds a fresh location so the row is new.
        var balance = await verifyDb.InventoryBalances.AsNoTracking()
            .FirstAsync(b => b.InventoryItemId == itemId && b.LocationId == locationId && b.Status == BalanceStatus.Available);
        Assert.That(balance.Quantity, Is.EqualTo(12.5m));
    }

    [Test]
    public async Task Transfer_Post_Pairs_Out_And_In_With_Same_CorrelationId()
    {
        var (itemId, fromLocationId) = await SeedItemAndLocationAsync("TRF-FROM");
        var toLocationId = await SeedExtraLocationAsync("TRF-TO");

        using var scope = Factory.Services.CreateScope();
        var inventory = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var posting = scope.ServiceProvider.GetRequiredService<ILogisticsPostingService>();

        // Prime stock at the from-location with a direct RECEIPT.
        _ = await inventory.PostTransactionAsync(new PostTransactionRequest(
            "RECEIPT", itemId, 20m, "EA", null, fromLocationId, null, null, null, null, null, null, null, null, null, null),
            "test@logistics", CancellationToken.None);

        int transferId, lineId;
        await using (var seedDb = await GetDbContextAsync())
        {
            var transfer = new StockTransfer
            {
                TransferNumber = $"TRF-TEST-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                FromLocationId = fromLocationId,
                ToLocationId = toLocationId,
                Status = StockTransferStatus.Draft,
                InitiatedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            seedDb.StockTransfers.Add(transfer);
            await seedDb.SaveChangesAsync();
            transferId = transfer.Id;

            seedDb.StockTransferLines.Add(new StockTransferLine
            {
                StockTransferId = transferId,
                InventoryItemId = itemId,
                Quantity = 8m,
                UnitMeasureCode = "EA",
                ModifiedDate = DateTime.UtcNow,
            });
            await seedDb.SaveChangesAsync();
            lineId = await seedDb.StockTransferLines.Where(l => l.StockTransferId == transferId).Select(l => l.Id).FirstAsync();
        }

        var result = await posting.PostTransferAsync(transferId, "test@logistics", CancellationToken.None);
        Assert.That(result.LinesPosted, Is.EqualTo(1));
        Assert.That(result.TransactionIds, Has.Count.EqualTo(2), "Paired transfer should write two transactions per line.");

        await using var verifyDb = await GetDbContextAsync();
        var header = await verifyDb.StockTransfers.AsNoTracking().FirstAsync(t => t.Id == transferId);
        Assert.That(header.Status, Is.EqualTo(StockTransferStatus.Completed));
        Assert.That(header.CorrelationId, Is.Not.Null);

        var line = await verifyDb.StockTransferLines.AsNoTracking().FirstAsync(l => l.Id == lineId);
        Assert.That(line.FromTransactionId, Is.Not.Null);
        Assert.That(line.ToTransactionId, Is.Not.Null);

        // Both legs should share the header's CorrelationId.
        var fromTx = await verifyDb.InventoryTransactions.AsNoTracking().FirstAsync(t => t.Id == line.FromTransactionId);
        var toTx = await verifyDb.InventoryTransactions.AsNoTracking().FirstAsync(t => t.Id == line.ToTransactionId);
        Assert.That(fromTx.CorrelationId, Is.EqualTo(header.CorrelationId));
        Assert.That(toTx.CorrelationId, Is.EqualTo(header.CorrelationId));

        // From-balance debited 8, to-balance credited 8. From originally had 20.
        var fromBal = await verifyDb.InventoryBalances.AsNoTracking()
            .FirstAsync(b => b.InventoryItemId == itemId && b.LocationId == fromLocationId && b.Status == BalanceStatus.Available);
        var toBal = await verifyDb.InventoryBalances.AsNoTracking()
            .FirstAsync(b => b.InventoryItemId == itemId && b.LocationId == toLocationId && b.Status == BalanceStatus.Available);
        Assert.That(fromBal.Quantity, Is.EqualTo(12m));
        Assert.That(toBal.Quantity, Is.EqualTo(8m));
    }

    [Test]
    public async Task Receipt_Post_Auto_Creates_Lot_When_Item_Tracks_Lots()
    {
        var (itemId, locationId) = await SeedItemAndLocationAsync("LOT-ITEM", tracksLot: true);
        using var scope = Factory.Services.CreateScope();
        var posting = scope.ServiceProvider.GetRequiredService<ILogisticsPostingService>();

        int receiptId;
        await using (var seedDb = await GetDbContextAsync())
        {
            var receipt = new GoodsReceipt
            {
                ReceiptNumber = $"RCP-LOT-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                ReceivedLocationId = locationId,
                Status = GoodsReceiptStatus.Draft,
                ReceivedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            seedDb.GoodsReceipts.Add(receipt);
            await seedDb.SaveChangesAsync();
            receiptId = receipt.Id;

            seedDb.GoodsReceiptLines.Add(new GoodsReceiptLine
            {
                GoodsReceiptId = receiptId,
                InventoryItemId = itemId,
                Quantity = 5m,
                UnitMeasureCode = "EA",
                ModifiedDate = DateTime.UtcNow,
                // No LotId — service should auto-create one.
            });
            await seedDb.SaveChangesAsync();
        }

        await posting.PostReceiptAsync(receiptId, "test@logistics", CancellationToken.None);

        await using var verifyDb = await GetDbContextAsync();
        var line = await verifyDb.GoodsReceiptLines.AsNoTracking().FirstAsync(l => l.GoodsReceiptId == receiptId);
        Assert.That(line.LotId, Is.Not.Null, "Auto-create should have stamped a LotId on the line.");

        var lot = await verifyDb.Lots.AsNoTracking().FirstAsync(l => l.Id == line.LotId);
        Assert.That(lot.InventoryItemId, Is.EqualTo(itemId));
        Assert.That(lot.LotCode, Does.StartWith("RCP-LOT-"));
    }

    // --- Helpers ---

    private async Task<(int itemId, int locationId)> SeedItemAndLocationAsync(string prefix, bool tracksLot = false)
    {
        await using var db = await GetDbContextAsync();
        var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.IsPrimary);

        var productId = await db.Database
            .SqlQuery<int>($"SELECT TOP 1 ProductID AS Value FROM Production.Product ORDER BY ProductID")
            .FirstAsync();

        var code = $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}".ToUpperInvariant();
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

        var existing = await db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
        InventoryItem item;
        if (existing is not null)
        {
            // Tests share the same product row (UNIQUE constraint on ProductId leaves no choice).
            // Sync TracksLot to whatever the current test expects so order doesn't matter.
            if (existing.TracksLot != tracksLot)
            {
                existing.TracksLot = tracksLot;
                existing.ModifiedDate = DateTime.UtcNow;
            }
            item = existing;
        }
        else
        {
            item = new InventoryItem
            {
                ProductId = productId,
                TracksLot = tracksLot,
                MinQty = 0,
                MaxQty = 1000,
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            db.InventoryItems.Add(item);
        }

        await db.SaveChangesAsync();
        return (item.Id, loc.Id);
    }

    private async Task<int> SeedExtraLocationAsync(string prefix)
    {
        await using var db = await GetDbContextAsync();
        var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.IsPrimary);
        var code = $"{prefix}-{Guid.NewGuid().ToString("N")[..8]}".ToUpperInvariant();
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

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_logistics_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "logistics-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
