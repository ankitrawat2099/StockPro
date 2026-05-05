using System.ComponentModel.DataAnnotations;

public class ReceiveGoodsLineItemDto
{
    [Required]
    public int LineItemId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ReceivedQty { get; set; }
}