using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class POLineItem
{
    [Key]
    public int LineItemId { get; set; }

    [Required]
    public int PoId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }
    public double UnitCost { get; set; }
    public double TotalCost { get; set; }

    public int ReceivedQty { get; set; } = 0;
}