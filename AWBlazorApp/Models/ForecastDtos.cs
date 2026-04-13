using AWBlazorApp.Data.Entities.Forecasting;

namespace AWBlazorApp.Models;

public sealed record ForecastDefinitionDto(
    int Id,
    string Name,
    string? Description,
    ForecastDataSource DataSource,
    ForecastMethod Method,
    ForecastGranularity Granularity,
    ForecastStatus Status,
    int LookbackMonths,
    int HorizonPeriods,
    string? MethodParametersJson,
    DateTime? LastComputedDate,
    DateTime CreatedDate,
    DateTime ModifiedDate);

public sealed record CreateForecastRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ForecastDataSource DataSource { get; init; }
    public ForecastMethod Method { get; init; }
    public ForecastGranularity Granularity { get; init; }
    public int LookbackMonths { get; init; }
    public int HorizonPeriods { get; init; }
    public string? MethodParametersJson { get; init; }
}

public sealed record UpdateForecastRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public ForecastMethod? Method { get; init; }
    public int? LookbackMonths { get; init; }
    public int? HorizonPeriods { get; init; }
    public string? MethodParametersJson { get; init; }
    public ForecastStatus? Status { get; init; }
}

public sealed record ForecastDataPointDto(
    int Id,
    DateTime PeriodDate,
    decimal ForecastedValue,
    decimal? ActualValue,
    decimal? Variance,
    decimal? VariancePercent,
    DateTime? EvaluatedDate);

public static class ForecastMappings
{
    public static ForecastDefinitionDto ToDto(this ForecastDefinition entity) => new(
        entity.Id, entity.Name, entity.Description,
        entity.DataSource, entity.Method, entity.Granularity, entity.Status,
        entity.LookbackMonths, entity.HorizonPeriods, entity.MethodParametersJson,
        entity.LastComputedDate, entity.CreatedDate, entity.ModifiedDate);

    public static ForecastDefinition ToEntity(this CreateForecastRequest request) => new()
    {
        Name = request.Name,
        Description = request.Description,
        DataSource = request.DataSource,
        Method = request.Method,
        Granularity = request.Granularity,
        Status = ForecastStatus.Draft,
        LookbackMonths = request.LookbackMonths,
        HorizonPeriods = request.HorizonPeriods,
        MethodParametersJson = request.MethodParametersJson,
    };

    public static void ApplyTo(this UpdateForecastRequest request, ForecastDefinition entity)
    {
        if (request.Name is not null) entity.Name = request.Name;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Method.HasValue) entity.Method = request.Method.Value;
        if (request.LookbackMonths.HasValue) entity.LookbackMonths = request.LookbackMonths.Value;
        if (request.HorizonPeriods.HasValue) entity.HorizonPeriods = request.HorizonPeriods.Value;
        if (request.MethodParametersJson is not null) entity.MethodParametersJson = request.MethodParametersJson;
        if (request.Status.HasValue) entity.Status = request.Status.Value;
    }

    public static ForecastDataPointDto ToDto(this ForecastDataPoint entity) => new(
        entity.Id, entity.PeriodDate, entity.ForecastedValue,
        entity.ActualValue, entity.Variance, entity.VariancePercent, entity.EvaluatedDate);
}
