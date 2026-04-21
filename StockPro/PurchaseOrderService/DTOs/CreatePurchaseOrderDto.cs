using System.ComponentModel.DataAnnotations;

public class CreatePurchaseOrderDto
{
    [Required]
    public int SupplierId { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    public DateTime? ExpectedDate { get; set; }

    public string? Notes { get; set; }

    public string? ReferenceNumber { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreatePOLineItemDto> Items { get; set; } = new();
}