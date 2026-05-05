using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class PurchaseOrder
{
    [Key]
    public int PoId { get; set; }

    [Required]
    public int SupplierId { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public Guid CreatedById { get; set; }

    [Required]
    public string Status { get; set; } = "DRAFT";

    public double TotalAmount { get; set; }

    public DateTime OrderDate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

    public DateTime? ExpectedDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public string? Notes { get; set; }

    public string? ReferenceNumber { get; set; }
    public List<POLineItem> Items { get; set; } = new();
}