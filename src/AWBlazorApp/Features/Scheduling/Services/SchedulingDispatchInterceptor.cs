using System.Runtime.CompilerServices;
using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AWBlazorApp.Features.Scheduling.Services;

/// <summary>
/// Reacts to SalesOrderHeader INSERTs in the SavedChanges phase and fires scheduling rule
/// dispatch against the same DbContext that just saved, so dispatcher-added alerts flow
/// through the same transaction.
///
/// Takes IServiceProvider and resolves ISchedulingDispatcher lazily to avoid a DI cycle:
/// the DbContextFactory configures itself with this interceptor, and the dispatcher
/// transitively depends on IDbContextFactory via FrozenWindowEvaluator — eager injection
/// would deadlock at DbContextFactory first-resolve.
///
/// Re-entrancy protection: the interceptor keeps its own AsyncLocal guard. When the
/// dispatcher's alert writes cause a nested SaveChanges, the interceptor sees the guard
/// set and self-skips. Added SalesOrderHeader entities are captured in SavingChanges
/// (where EntityState.Added is still visible) and keyed to the DbContext via
/// ConditionalWeakTable so SavedChanges knows exactly which SOs are new this round.
/// </summary>
public class SchedulingDispatchInterceptor : SaveChangesInterceptor
{
    private const short PilotLocation = 60;
    private static readonly AsyncLocal<bool> _inFlight = new();
    private readonly ConditionalWeakTable<DbContext, List<SalesOrderHeader>> _pendingByContext = new();

    private readonly IServiceProvider _sp;
    private readonly ILogger<SchedulingDispatchInterceptor> _log;

    public SchedulingDispatchInterceptor(IServiceProvider sp,
        ILogger<SchedulingDispatchInterceptor> log)
        => (_sp, _log) = (sp, log);

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureAddedSos(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        CaptureAddedSos(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        if (_inFlight.Value) return await base.SavedChangesAsync(eventData, result, ct);
        if (eventData.Context is not ApplicationDbContext db)
            return await base.SavedChangesAsync(eventData, result, ct);

        if (!_pendingByContext.TryGetValue(db, out var pending) || pending.Count == 0)
            return await base.SavedChangesAsync(eventData, result, ct);

        _pendingByContext.Remove(db);

        var dispatcher = _sp.GetRequiredService<ISchedulingDispatcher>();

        _inFlight.Value = true;
        try
        {
            foreach (var soh in pending)
            {
                try
                {
                    await dispatcher.OnSalesOrderCreatedAsync(soh, PilotLocation, db, ct);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Scheduling dispatch failed for SO {Id}; rethrowing to roll back.", soh.Id);
                    throw;
                }
            }

            if (db.ChangeTracker.HasChanges())
                await db.SaveChangesAsync(ct);
        }
        finally { _inFlight.Value = false; }

        return await base.SavedChangesAsync(eventData, result, ct);
    }

    private void CaptureAddedSos(DbContext? context)
    {
        if (context is null || _inFlight.Value) return;
        var added = context.ChangeTracker.Entries<SalesOrderHeader>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();
        if (added.Count > 0)
            _pendingByContext.AddOrUpdate(context, added);
    }
}
