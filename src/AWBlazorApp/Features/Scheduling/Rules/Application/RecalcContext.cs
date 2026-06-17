using AWBlazorApp.Features.Sales.SalesOrderHeaders.Domain;
using AWBlazorApp.Features.Scheduling.Rules.Domain;
using AWBlazorApp.Infrastructure.Persistence;

namespace AWBlazorApp.Features.Scheduling.Rules.Application;

public sealed record RecalcContext(
    ApplicationDbContext Db,
    SchedulingRule Rule,
    SalesOrderHeader Soh,
    short LocationId,
    int WeekId,
    bool InFrozenWindow,
    DateTime NowUtc);
