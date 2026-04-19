using System.Net;
using System.Text.Json;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Enterprise.Domain;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Inventory.Domain;
using AWBlazorApp.Features.Inventory.Services;
using AWBlazorApp.Features.Logistics.Domain;
using AWBlazorApp.Features.Logistics.Services;
using AWBlazorApp.Features.Quality.Domain;
using AWBlazorApp.Features.Quality.Services;
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
/// Integration tests for Module M5 — Quality. Exercises:
///   1. Auth coverage on the four endpoint groups.
///   2. Inspection fail → auto-opens NCR with FK back to the inspection.
///   3. NCR Scrap disposition → posts SCRAP inventory transaction.
///   4. NCR Quarantine disposition → posts a paired Available→Quarantine MOVE.
///   5. Receipt with auto-trigger plan → InspectionTriggerHook creates a Pending inspection.
/// </summary>
public class QualityEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] QualityEndpointGroups =
    [
        "/api/inspection-plans",
        "/api/inspections",
        "/api/non-conformances",
        "/api/capa-cases",
    ];

    private static IEnumerable<string> Groups => QualityEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task QualityEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
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
    public async Task QualityEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task Inspection_Fail_AutoOpens_Ncr()
    {
        var (plan, characteristic, item, location) = await SeedPlanAndItemAsync("FAIL");
        using var scope = Factory.Services.CreateScope();
        var insp = scope.ServiceProvider.GetRequiredService<IInspectionService>();

        var inspectionId = await insp.CreateAsync(new CreateInspectionInput(
            plan.Id, InspectionSourceKind.Manual, 0, item.Id, null, 5m, "EA", "test"),
            "test@quality", CancellationToken.None);

        await insp.StartAsync(inspectionId, null, "test@quality", CancellationToken.None);

        // Record a result that will fail (numeric out of range).
        await insp.RecordResultAsync(new RecordResultInput(
            inspectionId, characteristic.Id, NumericResult: 999m, AttributeResult: null, Notes: "out of spec"),
            recordedByBusinessEntityId: null, CancellationToken.None);

        var result = await insp.CompleteAsync(inspectionId, "test@quality", CancellationToken.None);
        Assert.That(result.FinalStatus, Is.EqualTo(InspectionStatus.Fail));
        Assert.That(result.AutoNcrId, Is.Not.Null);

        await using var verify = await GetDbContextAsync();
        var ncr = await verify.NonConformances.AsNoTracking().FirstAsync(n => n.Id == result.AutoNcrId);
        Assert.That(ncr.InspectionId, Is.EqualTo(inspectionId));
        Assert.That(ncr.InventoryItemId, Is.EqualTo(item.Id));
        Assert.That(ncr.Status, Is.EqualTo(NonConformanceStatus.Open));
        _ = location;
    }

    [Test]
    public async Task Ncr_Scrap_Disposition_Posts_Scrap_Transaction()
    {
        var (_, _, item, location) = await SeedPlanAndItemAsync("SCRAP");

        // Prime stock at the location so SCRAP has something to debit.
        using var scope1 = Factory.Services.CreateScope();
        var inv = scope1.ServiceProvider.GetRequiredService<IInventoryService>();
        await inv.PostTransactionAsync(new PostTransactionRequest(
            "RECEIPT", item.Id, 50m, "EA", null, location.Id, null, null, null, null, null, null, null, null, null, null),
            "test@quality", CancellationToken.None);

        int ncrId;
        await using (var seed = await GetDbContextAsync())
        {
            var ncr = new NonConformance
            {
                NcrNumber = $"NCR-TEST-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                InventoryItemId = item.Id,
                LocationId = location.Id,
                Quantity = 3m,
                UnitMeasureCode = "EA",
                Description = "scrap test",
                Status = NonConformanceStatus.Open,
                ModifiedDate = DateTime.UtcNow,
            };
            seed.NonConformances.Add(ncr);
            await seed.SaveChangesAsync();
            ncrId = ncr.Id;
        }

        using var scope2 = Factory.Services.CreateScope();
        var ncrSvc = scope2.ServiceProvider.GetRequiredService<INonConformanceService>();
        await ncrSvc.DispositionAsync(ncrId, NonConformanceDisposition.Scrap, "scrapping defective lot", "test@quality", CancellationToken.None);

        await using var verify = await GetDbContextAsync();
        var ncr2 = await verify.NonConformances.AsNoTracking().FirstAsync(n => n.Id == ncrId);
        Assert.That(ncr2.Disposition, Is.EqualTo(NonConformanceDisposition.Scrap));
        Assert.That(ncr2.PostedTransactionId, Is.Not.Null, "Scrap should write a SCRAP inv transaction.");

        var tx = await verify.InventoryTransactions.AsNoTracking().FirstAsync(t => t.Id == ncr2.PostedTransactionId);
        Assert.That(tx.Quantity, Is.EqualTo(3m));
        var typeCode = await verify.InventoryTransactionTypes.AsNoTracking()
            .Where(tt => tt.Id == tx.TransactionTypeId).Select(tt => tt.Code).FirstAsync();
        Assert.That(typeCode, Is.EqualTo("SCRAP"));
    }

    [Test]
    public async Task Ncr_Quarantine_Disposition_Moves_Balance_To_Quarantine_Status()
    {
        var (_, _, item, location) = await SeedPlanAndItemAsync("QUAR");

        using var scope1 = Factory.Services.CreateScope();
        var inv = scope1.ServiceProvider.GetRequiredService<IInventoryService>();
        await inv.PostTransactionAsync(new PostTransactionRequest(
            "RECEIPT", item.Id, 10m, "EA", null, location.Id, null, null, null, null, null, null, null, null, null, null),
            "test@quality", CancellationToken.None);

        int ncrId;
        await using (var seed = await GetDbContextAsync())
        {
            var ncr = new NonConformance
            {
                NcrNumber = $"NCR-Q-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                InventoryItemId = item.Id,
                LocationId = location.Id,
                Quantity = 4m,
                UnitMeasureCode = "EA",
                Description = "quarantine test",
                Status = NonConformanceStatus.Open,
                ModifiedDate = DateTime.UtcNow,
            };
            seed.NonConformances.Add(ncr);
            await seed.SaveChangesAsync();
            ncrId = ncr.Id;
        }

        using var scope2 = Factory.Services.CreateScope();
        var ncrSvc = scope2.ServiceProvider.GetRequiredService<INonConformanceService>();
        await ncrSvc.DispositionAsync(ncrId, NonConformanceDisposition.Quarantine, "hold for review", "test@quality", CancellationToken.None);

        await using var verify = await GetDbContextAsync();
        var available = await verify.InventoryBalances.AsNoTracking()
            .FirstAsync(b => b.InventoryItemId == item.Id && b.LocationId == location.Id && b.Status == BalanceStatus.Available);
        var quarantine = await verify.InventoryBalances.AsNoTracking()
            .FirstOrDefaultAsync(b => b.InventoryItemId == item.Id && b.LocationId == location.Id && b.Status == BalanceStatus.Quarantine);
        Assert.That(quarantine, Is.Not.Null, "Quarantine disposition should create/credit a Quarantine balance row.");
        Assert.That(quarantine!.Quantity, Is.EqualTo(4m));
        Assert.That(available.Quantity, Is.EqualTo(6m), "Available should be debited by 4 (10 received - 4 quarantined).");
    }

    [Test]
    public async Task AutoTrigger_On_Receipt_Creates_Pending_Inspection()
    {
        var (plan, _, item, location) = await SeedPlanAndItemAsync("AUTO", autoTriggerOnReceipt: true);

        // Build a goods receipt with a line for the same product, post it through the
        // logistics service, and assert the trigger hook created a Pending inspection.
        using var scope = Factory.Services.CreateScope();
        var posting = scope.ServiceProvider.GetRequiredService<ILogisticsPostingService>();

        int receiptId;
        await using (var seed = await GetDbContextAsync())
        {
            var receipt = new GoodsReceipt
            {
                ReceiptNumber = $"RCP-AUTO-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                ReceivedLocationId = location.Id,
                Status = GoodsReceiptStatus.Draft,
                ReceivedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            seed.GoodsReceipts.Add(receipt);
            await seed.SaveChangesAsync();
            receiptId = receipt.Id;

            seed.GoodsReceiptLines.Add(new GoodsReceiptLine
            {
                GoodsReceiptId = receiptId,
                InventoryItemId = item.Id,
                Quantity = 7m,
                UnitMeasureCode = "EA",
                ModifiedDate = DateTime.UtcNow,
            });
            await seed.SaveChangesAsync();
        }

        await posting.PostReceiptAsync(receiptId, "test@quality", CancellationToken.None);

        await using var verify = await GetDbContextAsync();
        var triggered = await verify.Inspections.AsNoTracking()
            .Where(i => i.InspectionPlanId == plan.Id && i.SourceKind == InspectionSourceKind.GoodsReceiptLine)
            .OrderByDescending(i => i.Id).FirstOrDefaultAsync();
        Assert.That(triggered, Is.Not.Null, "Trigger hook should create a Pending inspection on receipt post.");
        Assert.That(triggered!.Status, Is.EqualTo(InspectionStatus.Pending));
        Assert.That(triggered.Quantity, Is.EqualTo(7m));
        Assert.That(triggered.InventoryItemId, Is.EqualTo(item.Id));
    }

    // --- Helpers ---

    private async Task<(InspectionPlan plan, InspectionPlanCharacteristic characteristic, InventoryItem item, InventoryLocation location)>
        SeedPlanAndItemAsync(string prefix, bool autoTriggerOnReceipt = false)
    {
        await using var db = await GetDbContextAsync();
        var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.IsPrimary);

        var productId = await db.Database
            .SqlQuery<int>($"SELECT TOP 1 ProductID AS Value FROM Production.Product ORDER BY ProductID")
            .FirstAsync();

        var item = await db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (item is null)
        {
            item = new InventoryItem { ProductId = productId, IsActive = true, ModifiedDate = DateTime.UtcNow };
            db.InventoryItems.Add(item);
        }
        else if (item.TracksLot)
        {
            item.TracksLot = false;
            item.ModifiedDate = DateTime.UtcNow;
        }

        var locCode = $"{prefix}-{Guid.NewGuid().ToString("N")[..6]}".ToUpperInvariant();
        var loc = new InventoryLocation
        {
            OrganizationId = org.Id,
            Code = locCode,
            Name = $"{prefix} test loc",
            Kind = InventoryLocationKind.Warehouse,
            Path = locCode,
            Depth = 0,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        db.InventoryLocations.Add(loc);

        var planCode = $"{prefix}-{Guid.NewGuid().ToString("N")[..6]}".ToUpperInvariant();
        var plan = new InspectionPlan
        {
            PlanCode = planCode,
            Name = $"{prefix} test plan",
            Scope = InspectionScope.Inbound,
            ProductId = productId,
            AutoTriggerOnReceipt = autoTriggerOnReceipt,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        db.InspectionPlans.Add(plan);
        await db.SaveChangesAsync();

        var characteristic = new InspectionPlanCharacteristic
        {
            InspectionPlanId = plan.Id,
            SequenceNumber = 1,
            Name = "Test value",
            Kind = CharacteristicKind.Numeric,
            MinValue = 0m,
            MaxValue = 10m,
            ModifiedDate = DateTime.UtcNow,
        };
        db.InspectionPlanCharacteristics.Add(characteristic);
        await db.SaveChangesAsync();

        return (plan, characteristic, item, loc);
    }

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_quality_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "quality-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
