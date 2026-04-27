using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Repositories;
using ProductService.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace ProductService.Services;

public class ProductServiceImpl : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ProductDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ProductServiceImpl(IProductRepository repository, ProductDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _repository = repository;
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    //mapping dto
    private ProductResponseDto Map(Product p){
        return  new(){
        ProductId = p.ProductId,
        Sku = p.Sku,
        Name = p.Name,
        Description = p.Description,
        Category = p.Category,
        Brand = p.Brand,
        UnitOfMeasure = p.UnitOfMeasure,
        CostPrice = p.CostPrice,
        SellingPrice = p.SellingPrice,
        ReorderLevel = p.ReorderLevel,
        MaxStockLevel = p.MaxStockLevel,
        LeadTimeDays = p.LeadTimeDays,
        ImageUrl = p.ImageUrl,
        IsActive = p.IsActive,
        Barcode = p.Barcode
    };}

    //create
    public async Task<ProductResponseDto> CreateProductAsync(ProductRequest req)
    {
        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            Sku = req.Sku,
            Name = req.Name,
            Description = req.Description,
            Category = req.Category,
            Brand = req.Brand,
            UnitOfMeasure = req.UnitOfMeasure,
            CostPrice = req.CostPrice,
            SellingPrice = req.SellingPrice,
            ReorderLevel = req.ReorderLevel,
            MaxStockLevel = req.MaxStockLevel,
            LeadTimeDays = req.LeadTimeDays,
            ImageUrl = req.ImageUrl,
            IsActive = true,
            Barcode = req.Barcode
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return Map(product);
    }

    //get by id
    public async Task<ProductResponseDto> GetByIdAsync(Guid id)
    {
        var p = await _repository.FindByProductIdAsync(id)?? throw new Exception("Product not found");

        return Map(p);
    }

    //get by sku
    public async Task<ProductResponseDto> GetBySkuAsync(string sku)
    {
        var p = await _repository.FindBySkuAsync(sku)?? throw new Exception("Product not found");

        return Map(p);
    }

    //category
    public async Task<List<ProductResponseDto>> GetByCategoryAsync(string category){
        return (await _repository.FindByCategoryAsync(category)).Select(Map).ToList();}

    //brand
    public async Task<List<ProductResponseDto>> GetByBrandAsync(string brand){
        return (await _repository.FindByBrandAsync(brand)).Select(Map).ToList();}

    //search
    public async Task<List<ProductResponseDto>> SearchProductsAsync(string name){
        return (await _repository.SearchByNameAsync(name)).Select(Map).ToList();
    }

    //update
    public async Task<ProductResponseDto> UpdateProductAsync(Guid id, ProductRequest req)
    {
        var p = await _repository.FindByProductIdAsync(id)?? throw new Exception("Product not found");

        p.Sku = req.Sku;
        p.Name = req.Name;
        p.Description = req.Description;
        p.Category = req.Category;
        p.Brand = req.Brand;
        p.UnitOfMeasure = req.UnitOfMeasure;
        p.CostPrice = req.CostPrice;
        p.SellingPrice = req.SellingPrice;
        p.ReorderLevel = req.ReorderLevel;
        p.MaxStockLevel = req.MaxStockLevel;
        p.LeadTimeDays = req.LeadTimeDays;
        p.ImageUrl = req.ImageUrl;
        p.Barcode = req.Barcode;

        _context.Products.Update(p);
        await _context.SaveChangesAsync();

        return Map(p);
    }

    //deactivate
    public async Task DeactivateProductAsync(Guid id)
    {
        var p = await _repository.FindByProductIdAsync(id)
            ?? throw new Exception("Product not found");

        p.IsActive = false;

        _context.Products.Update(p);
        await _context.SaveChangesAsync();
    }

    // delete
    public async Task DeleteProductAsync(Guid id)
    {
        var p = await _repository.FindByProductIdAsync(id)?? throw new Exception("Product not found");

        _context.Products.Remove(p);
        await _context.SaveChangesAsync();
    }

    //get all
    public async Task<List<ProductResponseDto>> GetAllProductsAsync()
    {
        var products = await _context.Products.ToListAsync();
        return products.Select(Map).ToList();
    }

    //barcode
    public async Task<ProductResponseDto> GetByBarcodeAsync(string barcode)
    {
        var p = await _repository.FindByBarcodeAsync(barcode)?? throw new Exception("Product not found");

        return Map(p);
    }

    //low stock
   public async Task<List<ProductResponseDto>> GetLowStockProductsAsync()
    {
        var products = await _repository.FindByIsActiveAsync(true);
        var warehouseUrl = _configuration["Services:WarehouseService"];
        
        if (string.IsNullOrEmpty(warehouseUrl)) 
        {
            // Fallback if WarehouseService is not configured
            return new List<ProductResponseDto>();
        }

        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync($"{warehouseUrl}/api/stock/all");
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<ProductResponseDto>();
        }

        var stockDataString = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var allStocks = JsonSerializer.Deserialize<List<StockLevelDto>>(stockDataString, options) ?? new List<StockLevelDto>();

        // Group total physical quantity by ProductId
        var stockPerProduct = allStocks
            .GroupBy(s => s.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.Quantity));

        var lowStock = products.Where(p => 
        {
            // If product has no stock at all, it's 0
            int currentStock = stockPerProduct.ContainsKey(p.ProductId) ? stockPerProduct[p.ProductId] : 0;
            return currentStock < p.ReorderLevel;
        }).ToList();

        return lowStock.Select(Map).ToList();
    }
}

// Minimal DTO for deserializing warehouse stock response
public record StockLevelDto(Guid ProductId, int Quantity);