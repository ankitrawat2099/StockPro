using System.ComponentModel.DataAnnotations;

public class CreateMovementDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    [Required]
    public string MovementType { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public string ReferenceType { get; set; }

    public int ReferenceId { get; set; }

    public double UnitCost { get; set; }

    public string Notes { get; set; }
}