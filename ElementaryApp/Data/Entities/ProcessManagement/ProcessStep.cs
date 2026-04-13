using System.ComponentModel.DataAnnotations;

namespace ElementaryApp.Data.Entities.ProcessManagement;

public class ProcessStep
{
    public int Id { get; set; }

    public int ProcessId { get; set; }

    public int SequenceNumber { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? EstimatedDurationMinutes { get; set; }

    // Navigation
    public Process Process { get; set; } = null!;
}
