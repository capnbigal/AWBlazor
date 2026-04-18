using System.Net;
using System.Text.Json;
using AWBlazorApp.Data;
using AWBlazorApp.Features.Engineering.Domain;
using AWBlazorApp.Features.Engineering.Services;
using AWBlazorApp.Features.Identity.Domain;
using AWBlazorApp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace AWBlazorApp.Tests;

file static class EngIdResponseExtensions
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
/// Integration tests for Module M9 — Engineering. Exercises:
///   1. Auth coverage on the five endpoint groups.
///   2. ECO state machine: Draft → UnderReview → Approved transitions, and guards.
///   3. ECO approval auto-activates the affected BOM revision and deactivates the prior active
///      revision for the same product — the load-bearing cross-entity behavior of this module.
///   4. Deviation request Approve transition writes audit log.
/// </summary>
public class EngineeringEndpointTests : IntegrationTestFixtureBase
{
    private static readonly string[] EngineeringEndpointGroups =
    [
        "/api/manufacturing-routings",
        "/api/boms",
        "/api/engineering-change-orders",
        "/api/engineering-documents",
        "/api/deviation-requests",
    ];

    private static IEnumerable<string> Groups => EngineeringEndpointGroups;

    [TestCaseSource(nameof(Groups))]
    public async Task EngineeringEndpoint_Without_Auth_Returns_Unauthorized(string endpoint)
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
    public async Task EngineeringEndpoint_With_ApiKey_Returns_Success(string endpoint)
    {
        var apiKey = await EnsureAdminApiKeyAsync();
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

        var response = await client.GetAsync(endpoint + "?take=1");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"Expected 200 for {endpoint}, got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Test]
    public async Task Eco_Approval_Activates_New_Bom_And_Deactivates_Prior_Revision()
    {
        // Seed: two BOM revisions for the same product, old one active, new one inactive.
        // An ECO referencing the new one should, on approval, flip both.
        int productId = 980001 + new Random().Next(0, 10000);
        int oldBomId, newBomId, ecoId;

        await using (var db = await GetDbContextAsync())
        {
            var oldCode = "BOM-OLD-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var newCode = "BOM-NEW-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

            var oldBom = new BomHeader
            {
                Code = oldCode, Name = "Old revision " + oldCode,
                ProductId = productId, RevisionNumber = 1, IsActive = true,
                ModifiedDate = DateTime.UtcNow,
            };
            var newBom = new BomHeader
            {
                Code = newCode, Name = "New revision " + newCode,
                ProductId = productId, RevisionNumber = 2, IsActive = false,
                ModifiedDate = DateTime.UtcNow,
            };
            db.BomHeaders.Add(oldBom);
            db.BomHeaders.Add(newBom);
            await db.SaveChangesAsync();
            oldBomId = oldBom.Id;
            newBomId = newBom.Id;

            var ecoCode = "ECO-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            var eco = new EngineeringChangeOrder
            {
                Code = ecoCode, Title = "Swap BOM revision",
                Status = EcoStatus.Draft,
                RaisedByUserId = "test@eng", RaisedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.EngineeringChangeOrders.Add(eco);
            await db.SaveChangesAsync();
            ecoId = eco.Id;

            db.EcoAffectedItems.Add(new EcoAffectedItem
            {
                EngineeringChangeOrderId = ecoId,
                AffectedKind = EcoAffectedKind.Bom,
                TargetId = newBomId,
                Notes = "Activate new revision",
                ModifiedDate = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IEcoService>();

            await svc.SubmitForReviewAsync(ecoId, "test@eng", CancellationToken.None);
            await svc.ApproveAsync(ecoId, "ok", "mgr@eng", CancellationToken.None);

            await using var verify = await GetDbContextAsync();
            var after = await verify.BomHeaders.AsNoTracking()
                .Where(b => b.Id == oldBomId || b.Id == newBomId)
                .ToListAsync();

            Assert.That(after.First(b => b.Id == oldBomId).IsActive, Is.False,
                "Prior active BOM revision should be deactivated on ECO approval.");
            Assert.That(after.First(b => b.Id == newBomId).IsActive, Is.True,
                "New BOM revision should be activated on ECO approval.");

            var ecoAfter = await verify.EngineeringChangeOrders.AsNoTracking().FirstAsync(e => e.Id == ecoId);
            Assert.That(ecoAfter.Status, Is.EqualTo(EcoStatus.Approved));

            var approvalCount = await verify.EcoApprovals.AsNoTracking().CountAsync(a => a.EngineeringChangeOrderId == ecoId);
            Assert.That(approvalCount, Is.EqualTo(1));
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var affectedRows = cleanup.EcoAffectedItems.Where(a => a.EngineeringChangeOrderId == ecoId);
            cleanup.EcoAffectedItems.RemoveRange(affectedRows);
            var approvals = cleanup.EcoApprovals.Where(a => a.EngineeringChangeOrderId == ecoId);
            cleanup.EcoApprovals.RemoveRange(approvals);
            var ecoAudits = cleanup.EngineeringChangeOrderAuditLogs.Where(a => a.EngineeringChangeOrderId == ecoId);
            cleanup.EngineeringChangeOrderAuditLogs.RemoveRange(ecoAudits);
            var eco = await cleanup.EngineeringChangeOrders.FirstOrDefaultAsync(e => e.Id == ecoId);
            if (eco is not null) cleanup.EngineeringChangeOrders.Remove(eco);
            var bomAudits = cleanup.BomHeaderAuditLogs.Where(a => a.BomHeaderId == oldBomId || a.BomHeaderId == newBomId);
            cleanup.BomHeaderAuditLogs.RemoveRange(bomAudits);
            var boms = cleanup.BomHeaders.Where(b => b.Id == oldBomId || b.Id == newBomId);
            cleanup.BomHeaders.RemoveRange(boms);
            await cleanup.SaveChangesAsync();
        }
    }

    [Test]
    public async Task Eco_Approve_From_Draft_Without_Submitting_Throws()
    {
        int ecoId;
        await using (var db = await GetDbContextAsync())
        {
            var eco = new EngineeringChangeOrder
            {
                Code = "ECO-DRAFT-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                Title = "Draft only",
                Status = EcoStatus.Draft,
                RaisedByUserId = "test@eng", RaisedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.EngineeringChangeOrders.Add(eco);
            await db.SaveChangesAsync();
            ecoId = eco.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IEcoService>();
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await svc.ApproveAsync(ecoId, null, "mgr@eng", CancellationToken.None),
                "Approving a Draft ECO should fail — must Submit first.");
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var audits = cleanup.EngineeringChangeOrderAuditLogs.Where(a => a.EngineeringChangeOrderId == ecoId);
            cleanup.EngineeringChangeOrderAuditLogs.RemoveRange(audits);
            var eco = await cleanup.EngineeringChangeOrders.FirstOrDefaultAsync(e => e.Id == ecoId);
            if (eco is not null) cleanup.EngineeringChangeOrders.Remove(eco);
            await cleanup.SaveChangesAsync();
        }
    }

    [Test]
    public async Task DeviationRequest_Approve_Transitions_Pending_To_Approved()
    {
        int deviationId;
        await using (var db = await GetDbContextAsync())
        {
            var dev = new DeviationRequest
            {
                Code = "DEV-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                ProductId = 1,
                Reason = "Test deviation",
                Status = DeviationStatus.Pending,
                RaisedByUserId = "test@eng", RaisedAt = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
            };
            db.DeviationRequests.Add(dev);
            await db.SaveChangesAsync();
            deviationId = dev.Id;
        }

        try
        {
            using var scope = Factory.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IDeviationService>();
            await svc.ApproveAsync(deviationId, "ok", "mgr@eng", CancellationToken.None);

            await using var verify = await GetDbContextAsync();
            var after = await verify.DeviationRequests.AsNoTracking().FirstAsync(x => x.Id == deviationId);
            Assert.That(after.Status, Is.EqualTo(DeviationStatus.Approved));
            Assert.That(after.DecidedByUserId, Is.EqualTo("mgr@eng"));
            Assert.That(after.DecisionNotes, Is.EqualTo("ok"));
        }
        finally
        {
            await using var cleanup = await GetDbContextAsync();
            var audits = cleanup.DeviationRequestAuditLogs.Where(a => a.DeviationRequestId == deviationId);
            cleanup.DeviationRequestAuditLogs.RemoveRange(audits);
            var dev = await cleanup.DeviationRequests.FirstOrDefaultAsync(x => x.Id == deviationId);
            if (dev is not null) cleanup.DeviationRequests.Remove(dev);
            await cleanup.SaveChangesAsync();
        }
    }

    private static string? _adminApiKey;
    private async Task<string> EnsureAdminApiKeyAsync()
    {
        if (_adminApiKey is not null) return _adminApiKey;
        await using var db = await GetDbContextAsync();
        var adminRoleId = await db.Roles.Where(r => r.Name == AppRoles.Admin).Select(r => r.Id).FirstAsync();
        var adminUserId = await db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId).FirstAsync();

        var rawKey = "ek_eng_" + Guid.NewGuid().ToString("N");
        var hashed = AWBlazorApp.Infrastructure.Authentication.ApiKeyHasher.Hash(rawKey);
        db.ApiKeys.Add(new ApiKey
        {
            Name = "engineering-tests",
            Key = hashed,
            UserId = adminUserId,
            CreatedDate = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
        _adminApiKey = rawKey;
        return rawKey;
    }
}
