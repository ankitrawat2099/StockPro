public class StockLevelDto
{
    public int StockId { get; set; }
    public int WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}