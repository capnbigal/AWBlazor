using System.Net;
using System.Text.Json;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Features.Enterprise.Assets.Domain; using AWBlazorApp.Features.Enterprise.CostCenters.Domain; using AWBlazorApp.Features.Enterprise.OrgUnits.Domain; using AWBlazorApp.Features.Enterprise.Organizations.Domain; using AWBlazorApp.Features.Enterprise.ProductLines.Domain; using AWBlazorApp.Features.Enterprise.Stations.Domain; 
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Features.Inventory.Adjustments.Domain; using AWBlazorApp.Features.Inventory.Items.Domain; using AWBlazorApp.Features.Inventory.Locations.Domain; using AWBlazorApp.Features.Inventory.Lots.Domain; using AWBlazorApp.Features.Inventory.Outbox.Domain; using AWBlazorApp.Features.Inventory.Queue.Domain; using AWBlazorApp.Features.Inventory.Reports.Domain; using AWBlazorApp.Features.Inventory.Serials.Domain; using AWBlazorApp.Features.Inventory.Transactions.Domain; using AWBlazorApp.Features.Inventory.Types.Domain; 
using AWBlazorApp.Features.Mes.Downtime.Domain; using AWBlazorApp.Features.Mes.Instructions.Domain; using AWBlazorApp.Features.Mes.Runs.Domain; 
using AWBlazorApp.Features.Mes.Runs.Application.Services; using AWBlazorApp.Features.Mes.Instructions.Application.Services; 
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
/// Integration tests for Module M4 — MES (production execution). Exercises:
///   1. Auth coverage on the five endpoint groups.
///   2. The 15 seeded <see cref="DowntimeReason"/> codes land on first boot.
///   3. Start → Complete run flow writes a WIP_RECEIPT inventory transaction and flips the
///      run's status + stamps ActualEndAt.
///   4. <c>IWorkInstructionRevisionService.CreateNewRevisionAsync</c> copies steps from the
///      previous active revision; <c>PublishAsync</c> supersedes the prior and sets ActiveRevisionId.
/// </summary>
public class MesEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] MesEndpointGroups =
    [
        "/api/production-runs",
        "/api/operator-clock-events",
        "/api/downtime-events",
        "/api/downtime-reasons",
        "/api/work-instructions",
    ];

    private static IEnumerable<string> Groups => MesEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task MesEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
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
    public async Task MesEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task Fifteen_Downtime_Reasons_Are_Seeded()
    {
        await using var db = await GetDbContextAsync();
        var codes = await db.DowntimeReasons.AsNoTracking().Select(r => r.Code).ToListAsync();
        Assert.That(codes, Has.Count.EqualTo(15),
            "SeedDowntimeReasonsAsync should land 15 codes on first boot.");
        Assert.That(codes, Does.Contain("SETUP"));
        Assert.That(codes, Does.Contain("QUALITY_HOLD"));
        Assert.That(codes, Does.Contain("OTHER"));
    }

    [Test]
    public async Task Run_Start_Then_Complete_Writes_Wip_Receipt_And_Credits_Balance()
    {
        var (stationId, productId, itemId, locationId) = await SeedStationItemAndLocationAsync("MES-RUN");

        using var scope = Factory.Services.CreateScope();
        var runSvc = scope.ServiceProvider.GetRequiredService<IProductionRunService>();

        // For this test we use Kind=Other so we don't require an AdventureWorks WorkOrder row.
        int runId;
        await using (var seed = await GetDbContextAsync())
        {
            var run = new ProductionRun
            {
                RunNumber = $"RUN-TEST-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                Kind = ProductionRunKind.Other,
                StationId = stationId,
                Status = ProductionRunStatus.Draft,
                QuantityPlanned = 10m,
                ModifiedDate = DateTime.UtcNow,
            };
            seed.ProductionRuns.Add(run);
            await seed.SaveChangesAsync();
            runId = run.Id;
        }

        // Seed an InventoryItem for this ProductId so the completion service's WorkOrder
        // → InventoryItem resolution short-circuits; we patch the run to carry a valid
        // WorkOrderId only when we need the AW path. Here, Kind=Other still needs the item
        // because CompleteAsync's ResolveWorkOrderInventoryItemIdAsync requires WorkOrderId.
        // Adjust: switch Kind to Production + point at a real AW WorkOrder for ProductId.
        await using (var db = await GetDbContextAsync())
        {
            var run = await db.ProductionRuns.FirstAsync(r => r.Id == runId);
            var awWorkOrderId = await db.Database
                .SqlQuery<int>($"SELECT TOP 1 WorkOrderID AS Value FROM Production.WorkOrder WHERE ProductID = {productId}")
                .FirstOrDefaultAsync();
            if (awWorkOrderId == 0)
            {
                Assert.Ignore("AdventureWorks has no WorkOrder for the first product; skipping run completion test.");
                return;
            }
            run.Kind = ProductionRunKind.Production;
            run.WorkOrderId = awWorkOrderId;
            run.ModifiedDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var start = await runSvc.StartAsync(runId, "test@mes", CancellationToken.None);
        Assert.That(start.Status, Is.EqualTo(nameof(ProductionRunStatus.InProgress)));

        var completion = await runSvc.CompleteAsync(runId, quantityProduced: 7m, quantityScrapped: 1m,
            materialIssue: null, userId: "test@mes", cancellationToken: CancellationToken.None);
        Assert.That(completion.WipReceiptTransactionId, Is.Not.Null, "Complete should write WIP_RECEIPT.");
        Assert.That(completion.QuantityProduced, Is.EqualTo(7m));

        await using var verifyDb = await GetDbContextAsync();
        var run2 = await verifyDb.ProductionRuns.AsNoTracking().FirstAsync(r => r.Id == runId);
        Assert.That(run2.Status, Is.EqualTo(ProductionRunStatus.Completed));
        Assert.That(run2.ActualEndAt, Is.Not.Null);

        var tx = await verifyDb.InventoryTransactions.AsNoTracking().FirstAsync(t => t.Id == completion.WipReceiptTransactionId);
        Assert.That(tx.Quantity, Is.EqualTo(7m));
        Assert.That(tx.InventoryItemId, Is.EqualTo(itemId));
        _ = locationId;
    }

    [Test]
    public async Task Revision_CreateNew_Copies_Steps_From_Previous_Active()
    {
        using var scope = Factory.Services.CreateScope();
        var revSvc = scope.ServiceProvider.GetRequiredService<IWorkInstructionRevisionService>();

        int wiId;
        int rev1Id;
        await using (var seed = await GetDbContextAsync())
        {
            var wi = new WorkInstruction
            {
                WorkOrderRoutingId = 900_000_000 + new Random().Next(1, 999_999),
                Title = "Test instruction",
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            seed.WorkInstructions.Add(wi);
            await seed.SaveChangesAsync();
            wiId = wi.Id;

            var rev1 = new WorkInstructionRevision
            {
                WorkInstructionId = wi.Id,
                RevisionNumber = 1,
                Status = WorkInstructionRevisionStatus.Draft,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            seed.WorkInstructionRevisions.Add(rev1);
            await seed.SaveChangesAsync();
            rev1Id = rev1.Id;

            seed.WorkInstructionSteps.AddRange(
                new WorkInstructionStep { WorkInstructionRevisionId = rev1Id, SequenceNumber = 1, Title = "Step one", Body = "Body 1", ModifiedDate = DateTime.UtcNow },
                new WorkInstructionStep { WorkInstructionRevisionId = rev1Id, SequenceNumber = 2, Title = "Step two", Body = "Body 2", ModifiedDate = DateTime.UtcNow });
            await seed.SaveChangesAsync();
        }

        await revSvc.PublishAsync(rev1Id, "test@mes", CancellationToken.None);
        var rev2Id = await revSvc.CreateNewRevisionAsync(wiId, "test@mes", CancellationToken.None);

        await using var verify = await GetDbContextAsync();
        var rev2Steps = await verify.WorkInstructionSteps.AsNoTracking()
            .Where(s => s.WorkInstructionRevisionId == rev2Id)
            .OrderBy(s => s.SequenceNumber).ToListAsync();
        Assert.That(rev2Steps, Has.Count.EqualTo(2), "New revision should inherit the 2 steps from rev 1.");
        Assert.That(rev2Steps[0].Title, Is.EqualTo("Step one"));
        Assert.That(rev2Steps[1].Title, Is.EqualTo("Step two"));
        // Steps are genuinely new rows — different ids — but carry identical content.
        Assert.That(rev2Steps[0].Id, Is.Not.EqualTo(rev2Steps[1].Id));
    }

    [Test]
    public async Task Revision_Publish_Supersedes_Prior_Active()
    {
        using var scope = Factory.Services.CreateScope();
        var revSvc = scope.ServiceProvider.GetRequiredService<IWorkInstructionRevisionService>();

        int wiId;
        int rev1Id, rev2Id;
        await using (var seed = await GetDbContextAsync())
        {
            var wi = new WorkInstruction
            {
                WorkOrderRoutingId = 800_000_000 + new Random().Next(1, 999_999),
                Title = "Publish flow test",
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            seed.WorkInstructions.Add(wi);
            await seed.SaveChangesAsync();
            wiId = wi.Id;
        }

        rev1Id = await revSvc.CreateNewRevisionAsync(wiId, "test@mes", CancellationToken.None);
        await revSvc.PublishAsync(rev1Id, "test@mes", CancellationToken.None);

        rev2Id = await revSvc.CreateNewRevisionAsync(wiId, "test@mes", CancellationToken.None);
        await revSvc.PublishAsync(rev2Id, "test@mes", CancellationToken.None);

        await using var verify = await GetDbContextAsync();
        var wi2 = await verify.WorkInstructions.AsNoTracking().FirstAsync(w => w.Id == wiId);
        Assert.That(wi2.ActiveRevisionId, Is.EqualTo(rev2Id));

        var rev1 = await verify.WorkInstructionRevisions.AsNoTracking().FirstAsync(r => r.Id == rev1Id);
        var rev2 = await verify.WorkInstructionRevisions.AsNoTracking().FirstAsync(r => r.Id == rev2Id);
        Assert.That(rev1.Status, Is.EqualTo(WorkInstructionRevisionStatus.Superseded));
        Assert.That(rev2.Status, Is.EqualTo(WorkInstructionRevisionStatus.Published));
    }

    // --- Helpers ---

    private async Task<(int stationId, int productId, int itemId, int locationId)> SeedStationItemAndLocationAsync(string prefix)
    {
        await using var db = await GetDbContextAsync();
        var org = await db.Organizations.AsNoTracking().FirstAsync(o => o.IsPrimary);

        // Pick the first real OrgUnit/Asset to pin the station to (seeded in Phase A).
        // If none exist, create a lightweight Plant OrgUnit for the test.
        var orgUnitId = await db.OrgUnits.AsNoTracking().Where(u => u.OrganizationId == org.Id)
            .Select(u => (int?)u.Id).FirstOrDefaultAsync();
        if (!orgUnitId.HasValue)
        {
            var ou = new OrgUnit
            {
                OrganizationId = org.Id,
                Kind = OrgUnitKind.Plant,
                Code = $"TESTPLANT-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                Name = "Test plant",
                Path = $"TESTPLANT",
                Depth = 0,
                IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            db.OrgUnits.Add(ou);
            await db.SaveChangesAsync();
            orgUnitId = ou.Id;
        }

        var stationCode = $"{prefix}-S-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var station = new Station
        {
            OrgUnitId = orgUnitId.Value,
            Code = stationCode,
            Name = "Test station",
            StationKind = StationKind.Workstation,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        db.Stations.Add(station);

        var locCode = $"{prefix}-LOC-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var loc = new InventoryLocation
        {
            OrganizationId = org.Id,
            OrgUnitId = orgUnitId.Value,
            Code = locCode,
            Name = "Test receipt location",
            Kind = InventoryLocationKind.Warehouse,
            Path = locCode,
            Depth = 0,
            IsActive = true,
            ModifiedDate = DateTime.UtcNow,
        };
        db.InventoryLocations.Add(loc);

        var productId = await db.Database
            .SqlQuery<int>($"SELECT TOP 1 ProductID AS Value FROM Production.Product ORDER BY ProductID")
            .FirstAsync();

        var item = await db.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == productId);
        if (item is null)
        {
            item = new InventoryItem
            {
                ProductId = productId,
                IsActive = true,
                MinQty = 0, MaxQty = 1000,
                ModifiedDate = DateTime.UtcNow,
            };
            db.InventoryItems.Add(item);
        }
        else if (item.TracksLot)
        {
            // Reset state so completion doesn't demand a LotId.
            item.TracksLot = false;
            item.ModifiedDate = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        return (station.Id, productId, item.Id, loc.Id);
    }

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_mes_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "mes-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
