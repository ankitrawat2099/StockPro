public class CreateAlertDto
{
    public int RecipientId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid? RelatedProductId { get; set; }

    public int? RelatedWarehouseId { get; set; }

    public string Channel { get; set; } = "IN_APP";
}