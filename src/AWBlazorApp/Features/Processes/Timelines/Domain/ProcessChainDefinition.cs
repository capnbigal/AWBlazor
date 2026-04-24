using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Processes.Timelines.Domain;

[Table("ProcessChainDefinition", Schema = "processes")]
public class ProcessChainDefinition
{
    [Key, Column("ProcessChainDefinitionID"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("Code"), MaxLength(64)] public string Code { get; set; } = "";
    [Column("Name"), MaxLength(128)] public string Name { get; set; } = "";
    [Column("Description"), MaxLength(500)] public string? Description { get; set; }
    [Column("StepsJson")] public string StepsJson { get; set; } = "[]";
    [Column("IsActive")] public bool IsActive { get; set; } = true;
    [Column("SortOrder")] public int SortOrder { get; set; }
    [Column("ModifiedDate")] public DateTime ModifiedDate { get; set; }
}
