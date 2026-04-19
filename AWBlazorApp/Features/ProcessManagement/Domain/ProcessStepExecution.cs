using System.ComponentModel.DataAnnotations;
using AWBlazorApp.Features.Identity.Domain; using AWBlazorApp.Features.Admin.Permissions.Domain;

namespace AWBlazorApp.Features.ProcessManagement.Domain;

public class ProcessStepExecution
{
    public int Id { get; set; }

    public int ProcessExecutionId { get; set; }

    public int ProcessStepId { get; set; }

    public ProcessStepExecutionStatus Status { get; set; }

    [MaxLength(450)]
    public string? CompletedByUserId { get; set; }

    public DateTime? CompletedDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    // Navigation
    public ProcessExecution ProcessExecution { get; set; } = null!;
    public ProcessStep ProcessStep { get; set; } = null!;
    public ApplicationUser? CompletedByUser { get; set; }
}
