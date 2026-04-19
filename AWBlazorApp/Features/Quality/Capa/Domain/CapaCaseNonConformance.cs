using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Quality.Capa.Domain;

/// <summary>
/// Many-to-many join between a <see cref="CapaCase"/> and the <see cref="NonConformance"/>
/// records it investigates. Kept as a separate table (rather than a direct FK on NCR) because
/// one recurring issue can legitimately span multiple NCRs and we want to attach them as the
/// investigation progresses.
/// </summary>
[Table("CapaCaseNonConformance", Schema = "qa")]
public class CapaCaseNonConformance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int CapaCaseId { get; set; }
    public int NonConformanceId { get; set; }

    public DateTime LinkedAt { get; set; }
}
