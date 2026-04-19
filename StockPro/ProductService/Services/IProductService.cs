using ProductService.DTOs;

namespace ProductService.Services;

public interface IProductService
{
    Task<ProductResponseDto> CreateProductAsync(ProductRequest request);
    Task<ProductResponseDto> GetByIdAsync(Guid id);
    Task<ProductResponseDto> GetBySkuAsync(string sku);
    Task<List<ProductResponseDto>> GetByCategoryAsync(string category);
    Task<List<ProductResponseDto>> GetByBrandAsync(string brand);
    Task<List<ProductResponseDto>> SearchProductsAsync(string name);
    Task<ProductResponseDto> UpdateProductAsync(Guid id, ProductRequest request);
    Task DeactivateProductAsync(Guid id);
    Task DeleteProductAsync(Guid id);
    Task<List<ProductResponseDto>> GetAllProductsAsync();
    Task<ProductResponseDto> GetByBarcodeAsync(string barcode);
    Task<List<ProductResponseDto>> GetLowStockProductsAsync();
}