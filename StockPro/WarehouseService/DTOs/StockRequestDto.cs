using System.ComponentModel.DataAnnotations;

public class StockRequestDto
{
    public int WarehouseId { get; set; }
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public string ReferenceType { get; set; } = "";
    public int ReferenceId { get; set; }
    public string Notes { get; set; } = "";
    public double UnitCost { get; set; }
}