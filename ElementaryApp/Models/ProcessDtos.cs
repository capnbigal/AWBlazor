using ElementaryApp.Data.Entities.ProcessManagement;

namespace ElementaryApp.Models;

public sealed record ProcessDto(
    int Id,
    string Name,
    string? Description,
    short DepartmentId,
    string? DefaultProcessorUserId,
    bool IsRecurring,
    string? CronSchedule,
    ProcessStatus Status,
    DateTime? NextRunDate,
    DateTime CreatedDate,
    DateTime ModifiedDate);

public sealed record CreateProcessRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public short DepartmentId { get; init; }
    public string? DefaultProcessorUserId { get; init; }
    public bool IsRecurring { get; init; }
    public string? CronSchedule { get; init; }
}

public sealed record UpdateProcessRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public short? DepartmentId { get; init; }
    public string? DefaultProcessorUserId { get; init; }
    public bool? IsRecurring { get; init; }
    public string? CronSchedule { get; init; }
    public ProcessStatus? Status { get; init; }
}

public sealed record ProcessStepDto(
    int Id,
    int ProcessId,
    int SequenceNumber,
    string Name,
    string? Description,
    int? EstimatedDurationMinutes);

public sealed record CreateProcessStepRequest
{
    public int SequenceNumber { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
}

public sealed record UpdateProcessStepRequest
{
    public int? SequenceNumber { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
}

public sealed record ProcessExecutionDto(
    int Id,
    int ProcessId,
    DateTime ExecutionDate,
    string? AssignedUserId,
    ProcessExecutionStatus Status,
    DateTime? CompletedDate,
    string? Notes,
    int CompletedStepCount,
    int TotalStepCount);

public sealed record ProcessStepExecutionDto(
    int Id,
    int ProcessExecutionId,
    int ProcessStepId,
    string StepName,
    int SequenceNumber,
    ProcessStepExecutionStatus Status,
    string? CompletedByUserId,
    DateTime? CompletedDate,
    string? Notes);

public sealed record UpdateStepExecutionRequest
{
    public ProcessStepExecutionStatus Status { get; init; }
    public string? Notes { get; init; }
}

public static class ProcessMappings
{
    public static ProcessDto ToDto(this Process entity) => new(
        entity.Id, entity.Name, entity.Description,
        entity.DepartmentId, entity.DefaultProcessorUserId,
        entity.IsRecurring, entity.CronSchedule, entity.Status,
        entity.NextRunDate, entity.CreatedDate, entity.ModifiedDate);

    public static Process ToEntity(this CreateProcessRequest request) => new()
    {
        Name = request.Name,
        Description = request.Description,
        DepartmentId = request.DepartmentId,
        DefaultProcessorUserId = request.DefaultProcessorUserId,
        IsRecurring = request.IsRecurring,
        CronSchedule = request.CronSchedule,
        Status = ProcessStatus.Active,
    };

    public static void ApplyTo(this UpdateProcessRequest request, Process entity)
    {
        if (request.Name is not null) entity.Name = request.Name;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.DepartmentId.HasValue) entity.DepartmentId = request.DepartmentId.Value;
        if (request.DefaultProcessorUserId is not null) entity.DefaultProcessorUserId = request.DefaultProcessorUserId;
        if (request.IsRecurring.HasValue) entity.IsRecurring = request.IsRecurring.Value;
        if (request.CronSchedule is not null) entity.CronSchedule = request.CronSchedule;
        if (request.Status.HasValue) entity.Status = request.Status.Value;
    }

    public static ProcessStepDto ToDto(this ProcessStep entity) => new(
        entity.Id, entity.ProcessId, entity.SequenceNumber,
        entity.Name, entity.Description, entity.EstimatedDurationMinutes);

    public static ProcessStep ToEntity(this CreateProcessStepRequest request, int processId) => new()
    {
        ProcessId = processId,
        SequenceNumber = request.SequenceNumber,
        Name = request.Name,
        Description = request.Description,
        EstimatedDurationMinutes = request.EstimatedDurationMinutes,
    };

    public static void ApplyTo(this UpdateProcessStepRequest request, ProcessStep entity)
    {
        if (request.SequenceNumber.HasValue) entity.SequenceNumber = request.SequenceNumber.Value;
        if (request.Name is not null) entity.Name = request.Name;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.EstimatedDurationMinutes.HasValue) entity.EstimatedDurationMinutes = request.EstimatedDurationMinutes.Value;
    }

    public static ProcessExecutionDto ToDto(this ProcessExecution entity) => new(
        entity.Id, entity.ProcessId, entity.ExecutionDate,
        entity.AssignedUserId, entity.Status, entity.CompletedDate, entity.Notes,
        entity.StepExecutions.Count(se => se.Status is ProcessStepExecutionStatus.Completed or ProcessStepExecutionStatus.Skipped),
        entity.StepExecutions.Count);

    public static ProcessStepExecutionDto ToDto(this ProcessStepExecution entity) => new(
        entity.Id, entity.ProcessExecutionId, entity.ProcessStepId,
        entity.ProcessStep.Name, entity.ProcessStep.SequenceNumber,
        entity.Status, entity.CompletedByUserId, entity.CompletedDate, entity.Notes);
}
