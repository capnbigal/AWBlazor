using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AWBlazorApp.Features.Inventory.Types.Domain;

/// <summary>
/// Seed-only reference table. 20 codes land here on first boot. The <see cref="Sign"/> drives
/// balance arithmetic (+1 adds, -1 subtracts, 0 is a paired move that zeroes out across legs);
/// <see cref="EmitsJson"/> decides whether a posted transaction gets an outbox row for downstream
/// JSON publication.
/// </summary>
[Table("InventoryTransactionType", Schema = "inv")]
public class InventoryTransactionType
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [MaxLength(32)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public sbyte Sign { get; set; }

    public bool RequiresApproval { get; set; }

    public bool EmitsJson { get; set; }

    public bool IsActive { get; set; } = true;
}

public static class InventoryTransactionTypeCodes
{
    public const string Receipt       = "RECEIPT";
    public const string Putaway       = "PUTAWAY";
    public const string Pick          = "PICK";
    public const string Pack          = "PACK";
    public const string Ship          = "SHIP";
    public const string AdjustInc     = "ADJUST_INC";
    public const string AdjustDec     = "ADJUST_DEC";
    public const string Move          = "MOVE";
    public const string Scrap         = "SCRAP";
    public const string ReturnCust    = "RETURN_CUST";
    public const string ReturnVend    = "RETURN_VEND";
    public const string Count         = "COUNT";
    public const string CycleCount    = "CYCLE_COUNT";
    public const string WipIssue      = "WIP_ISSUE";
    public const string WipReceipt    = "WIP_RECEIPT";
    public const string Assembly      = "ASSEMBLY";
    public const string Disassembly   = "DISASSEMBLY";
    public const string TransferOut   = "TRANSFER_OUT";
    public const string TransferIn    = "TRANSFER_IN";
    public const string Rework        = "REWORK";
}
