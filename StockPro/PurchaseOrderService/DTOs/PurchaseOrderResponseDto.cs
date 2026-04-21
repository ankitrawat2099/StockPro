public class PurchaseOrderResponseDto
{
    public int PoId { get; set; }
    public int SupplierId { get; set; }
    public int WarehouseId { get; set; }
    public Guid CreatedById { get; set; }
    public string Status { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
    public List<POLineItemResponseDto> Items { get; set; } = new();
}