using System.ComponentModel.DataAnnotations;
using AWBlazorApp.Data;
using AWBlazorApp.Shared.Domain;
using System.ComponentModel.DataAnnotations.Schema;
using AWBlazorApp.Features.HumanResources.Domain;

namespace AWBlazorApp.Features.ProcessManagement.Domain;

public class Process : AuditableEntity
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column("DepartmentID")]
    public short DepartmentId { get; set; }

    [MaxLength(450)]
    public string? DefaultProcessorUserId { get; set; }

    public bool IsRecurring { get; set; }

    [MaxLength(50)]
    public string? CronSchedule { get; set; }

    public DateTime? NextRunDate { get; set; }

    public ProcessStatus Status { get; set; }

    // Navigation
    public Department? Department { get; set; }
    public ApplicationUser? DefaultProcessor { get; set; }
    public ICollection<ProcessStep> Steps { get; set; } = [];
    public ICollection<ProcessExecution> Executions { get; set; } = [];
}
