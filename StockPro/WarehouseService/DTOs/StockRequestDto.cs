using System.ComponentModel.DataAnnotations;

public class StockRequestDto
{
    public int WarehouseId { get; set; }
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}