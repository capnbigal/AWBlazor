using System.ComponentModel.DataAnnotations;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;
using AWBlazorApp.Shared.Domain;

namespace AWBlazorApp.Features.ProcessManagement.Domain;

public class ProcessExecution : AuditableEntity
{
    public int Id { get; set; }

    public int ProcessId { get; set; }

    public DateTime ExecutionDate { get; set; }

    [MaxLength(450)]
    public string? AssignedUserId { get; set; }

    public ProcessExecutionStatus Status { get; set; }

    public DateTime? CompletedDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Navigation
    public Process Process { get; set; } = null!;
    public ApplicationUser? AssignedUser { get; set; }
    public ICollection<ProcessStepExecution> StepExecutions { get; set; } = [];
}
