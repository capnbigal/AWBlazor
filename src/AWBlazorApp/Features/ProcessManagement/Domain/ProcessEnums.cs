namespace AWBlazorApp.Features.ProcessManagement.Domain;

public enum ProcessStatus
{
    Active = 0,
    Paused = 1,
    Archived = 2,
}

public enum ProcessExecutionStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3,
    Cancelled = 4,
}

public enum ProcessStepExecutionStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Skipped = 3,
}
