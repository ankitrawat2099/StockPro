using System.ComponentModel.DataAnnotations;

public class TransferStockDto
{
    public int FromWarehouse { get; set; }
    public int ToWarehouse { get; set; }

    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}