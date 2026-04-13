using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Data.Entities.AdventureWorks;

/// <summary>
/// Employee information. Maps onto the pre-existing <c>HumanResources.Employee</c> table. The PK
/// is <c>BusinessEntityID</c> and is NOT an identity column — it's shared with
/// <c>Person.BusinessEntity</c>, so callers must supply the id explicitly on create.
///
/// The real table also has <c>OrganizationNode</c> (hierarchyid) and <c>OrganizationLevel</c>
/// (computed from it) columns which we deliberately do NOT map here. SQL Server allows both to
/// be NULL on insert, so EF will leave them alone when creating new rows from this app.
/// </summary>
[Table("Employee", Schema = "HumanResources")]
public class Employee
{
    /// <summary>Shared PK / FK to <c>Person.BusinessEntity.BusinessEntityID</c>. NOT identity.</summary>
    [Key]
    [Column("BusinessEntityID")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Column("NationalIDNumber")]
    [MaxLength(15)]
    public string NationalIDNumber { get; set; } = string.Empty;

    [Column("LoginID")]
    [MaxLength(256)]
    public string LoginID { get; set; } = string.Empty;

    [Column("JobTitle")]
    [MaxLength(50)]
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>SQL <c>date</c> type. Format: yyyy-MM-dd.</summary>
    [Column("BirthDate", TypeName = "date")]
    public DateTime BirthDate { get; set; }

    /// <summary>S = Single, M = Married.</summary>
    [Column("MaritalStatus")]
    [MaxLength(1)]
    public string MaritalStatus { get; set; } = string.Empty;

    /// <summary>F = Female, M = Male.</summary>
    [Column("Gender")]
    [MaxLength(1)]
    public string Gender { get; set; } = string.Empty;

    /// <summary>SQL <c>date</c> type. Format: yyyy-MM-dd.</summary>
    [Column("HireDate", TypeName = "date")]
    public DateTime HireDate { get; set; }

    [Column("SalariedFlag")]
    public bool SalariedFlag { get; set; }

    [Column("CurrentFlag")]
    public bool CurrentFlag { get; set; }

    [Column("VacationHours")]
    public short VacationHours { get; set; }

    [Column("SickLeaveHours")]
    public short SickLeaveHours { get; set; }

    [Column("rowguid")]
    public Guid RowGuid { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
