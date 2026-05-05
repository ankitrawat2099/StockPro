using System.ComponentModel.DataAnnotations;

public class CancelPurchaseOrderDto
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}