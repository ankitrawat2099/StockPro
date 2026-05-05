using ProductService.Entities;

namespace ProductService.Repositories;

public interface IProductRepository
{
    Task<Product?> FindBySkuAsync(string sku);
    Task<List<Product>> FindByCategoryAsync(string category);
    Task<List<Product>> FindByBrandAsync(string brand);
    Task<Product?> FindByProductIdAsync(Guid productId);
    Task<List<Product>> SearchByNameAsync(string name);
    Task<List<Product>> FindByIsActiveAsync(bool isActive);
    Task<Product?> FindByBarcodeAsync(string barcode);
    Task<int> CountByCategoryAsync(string category);
}