using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class ProductRequest
{
    [Required]
    public string Sku { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    public string Category { get; set; }

    public string Brand { get; set; }

    public string UnitOfMeasure { get; set; }

    [Range(0, 1000000)]
    public double CostPrice { get; set; }

    [Range(0, 1000000)]
    public double SellingPrice { get; set; }

    [Range(0, 10000)]
    public int ReorderLevel { get; set; }

    [Range(0, 100000)]
    public int MaxStockLevel { get; set; }

    [Range(0, 365)]
    public int LeadTimeDays { get; set; }

    public string ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public string Barcode { get; set; }
}