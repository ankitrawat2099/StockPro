public class POLineItemResponseDto
{
    public int LineItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public double UnitCost { get; set; }
    public double TotalCost { get; set; }
    public int ReceivedQty { get; set; }
}