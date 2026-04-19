using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Entities;

namespace ProductService.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> FindBySkuAsync(string sku)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<List<Product>> FindByCategoryAsync(string category)
    {
        return await _context.Products.Where(p => p.Category == category).ToListAsync();
    }

    public async Task<List<Product>> FindByBrandAsync(string brand)
    {
        return await _context.Products.Where(p => p.Brand == brand).ToListAsync();
    }

    public async Task<Product?> FindByProductIdAsync(Guid productId)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<List<Product>> SearchByNameAsync(string name)
    {
        return await _context.Products.Where(p => p.Name.Contains(name)).ToListAsync();
    }

    public async Task<List<Product>> FindByIsActiveAsync(bool isActive)
    {
        return await _context.Products.Where(p => p.IsActive == isActive).ToListAsync();
    }

    public async Task<Product?> FindByBarcodeAsync(string barcode)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Barcode == barcode);
    }

    public async Task<int> CountByCategoryAsync(string category)
    {
        return await _context.Products.CountAsync(p => p.Category == category);
    }
}