using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class InventorySnapshot
{
    [Key]
    public int SnapshotId { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public double StockValue { get; set; }

    [Required]
    public DateOnly SnapshotDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}