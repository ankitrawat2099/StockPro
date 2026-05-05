using System.ComponentModel.DataAnnotations;

public class ReceiveGoodsDto
{
    [Required]
    public List<ReceiveGoodsLineItemDto> Items { get; set; } = new();
}