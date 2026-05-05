using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
public class StockLevel
{
    [Key]
    public int StockId { get; set; }
    public int WarehouseId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public int ReservedQuantity { get; set; }

    public string Location { get; set; }

    public DateTime LastUpdated { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

    [NotMapped]
    public int AvailableQuantity => Quantity - ReservedQuantity;
}