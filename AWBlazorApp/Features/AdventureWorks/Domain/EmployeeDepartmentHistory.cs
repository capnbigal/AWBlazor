using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.AdventureWorks.Domain;

/// <summary>Employee-to-department assignment history. Maps onto the pre-existing <c>HumanResources.EmployeeDepartmentHistory</c> table. 4-column composite PK = (BusinessEntityID, DepartmentID, ShiftID, StartDate). Both StartDate and EndDate are SQL <c>date</c> (not datetime).</summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(DepartmentId), nameof(ShiftId), nameof(StartDate))]
[Table("EmployeeDepartmentHistory", Schema = "HumanResources")]
public class EmployeeDepartmentHistory
{
    /// <summary>FK to <c>HumanResources.Employee.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>FK to <c>HumanResources.Department.DepartmentID</c>. Part of the composite PK.</summary>
    [Column("DepartmentID")]
    public short DepartmentId { get; set; }

    /// <summary>FK to <c>HumanResources.Shift.ShiftID</c>. Part of the composite PK.</summary>
    [Column("ShiftID")]
    public byte ShiftId { get; set; }

    /// <summary>Start of the assignment period (SQL <c>date</c>). Part of the composite PK.</summary>
    [Column("StartDate", TypeName = "date")]
    public DateTime StartDate { get; set; }

    /// <summary>End of the assignment period, null when the assignment is still current.</summary>
    [Column("EndDate", TypeName = "date")]
    public DateTime? EndDate { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
