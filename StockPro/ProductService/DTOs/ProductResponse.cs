namespace ProductService.DTOs;

public class ProductResponseDto
{
    public Guid ProductId { get; set; }

    public string Sku { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Category { get; set; }

    public string Brand { get; set; }

    public string UnitOfMeasure { get; set; }

    public double CostPrice { get; set; }

    public double SellingPrice { get; set; }

    public int ReorderLevel { get; set; }

    public int MaxStockLevel { get; set; }

    public int LeadTimeDays { get; set; }

    public string ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public string Barcode { get; set; }
}