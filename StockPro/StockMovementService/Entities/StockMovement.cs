using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class StockMovement
{
    [Key]
    public int MovementId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    [Required]
    [MaxLength(50)]
    public string MovementType { get; set; } // STOCK_IN, STOCK_OUT, TRANSFER

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [MaxLength(50)]
    public string ReferenceType { get; set; } // PO, ISSUE, TRANSFER

    public int ReferenceId { get; set; }

    public double UnitCost { get; set; }

    [Required]
    public Guid PerformedBy { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; }

    [Required]
    public DateTime MovementDate { get; set; }

    public int BalanceAfter { get; set; } // optional snapshot
}