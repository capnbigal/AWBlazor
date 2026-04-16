using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AWBlazorApp.Features.HumanResources.Domain;

/// <summary>Employee pay rate history. Maps onto the pre-existing <c>HumanResources.EmployeePayHistory</c> table. Composite PK = (BusinessEntityID, RateChangeDate).</summary>
[PrimaryKey(nameof(BusinessEntityId), nameof(RateChangeDate))]
[Table("EmployeePayHistory", Schema = "HumanResources")]
public class EmployeePayHistory
{
    /// <summary>FK to <c>HumanResources.Employee.BusinessEntityID</c>. Part of the composite PK.</summary>
    [Column("BusinessEntityID")]
    public int BusinessEntityId { get; set; }

    /// <summary>Date the pay rate changed. Part of the composite PK.</summary>
    [Column("RateChangeDate")]
    public DateTime RateChangeDate { get; set; }

    /// <summary>Pay rate. SQL <c>money</c>.</summary>
    [Column("Rate", TypeName = "money")]
    public decimal Rate { get; set; }

    /// <summary>1 = Monthly, 2 = Biweekly.</summary>
    [Column("PayFrequency")]
    public byte PayFrequency { get; set; }

    [Column("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}
