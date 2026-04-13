using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>Company departments (Engineering, HR, Production, ...). Maps onto the pre-existing <c>HumanResources.Department</c> table in AdventureWorks2022. PK is a <c>smallint</c>.</summary>
[Table("Department", Schema = "HumanResources")]
public class Department
{
    [Key]
    [Column("DepartmentID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Column("Name")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Executive-group name that owns the department (Executive General and Administration, Research and Development, Sales and Marketing, ...).</summary>
    [Column("GroupName")]
    [MaxLength(50)]
    public string GroupName { get; set; } = string.Empty;

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
