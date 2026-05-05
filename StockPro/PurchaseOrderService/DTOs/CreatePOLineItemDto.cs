using System.ComponentModel.DataAnnotations;

public class CreatePOLineItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public double UnitCost { get; set; }
}