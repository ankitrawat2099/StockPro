using System.ComponentModel.DataAnnotations;

public class ReceiveGoodsDto
{
    [Required]
    [MinLength(1)]
    public List<ReceiveGoodsLineItemDto> Items { get; set; } = new();
}