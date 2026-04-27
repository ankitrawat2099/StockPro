public class CreateMovementDto
{
    public Guid ProductId { get; set; }

    public int WarehouseId { get; set; }

    public string MovementType { get; set; } = "";

    public int Quantity { get; set; }

    public string ReferenceType { get; set; } = "";

    public int ReferenceId { get; set; }

    public double UnitCost { get; set; }

    public string Notes { get; set; } = "";

    public int BalanceAfter { get; set; }
}
