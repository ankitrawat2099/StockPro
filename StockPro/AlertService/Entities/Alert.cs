using System.ComponentModel.DataAnnotations;

public class Alert
{
    [Key]
    public int AlertId { get; set; }

    public int RecipientId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string Severity { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid? RelatedProductId { get; set; }

    public int? RelatedWarehouseId { get; set; }

    public string Channel { get; set; } = "IN_APP";

    public bool IsRead { get; set; } = false;

    public bool IsAcknowledged { get; set; } = false;

    public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
}