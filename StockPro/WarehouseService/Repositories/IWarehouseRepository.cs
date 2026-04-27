public interface IWarehouseRepository
{
    Task<Warehouse?> FindByWarehouseIdAsync(int id);

    Task<List<Warehouse>> FindByManagerIdAsync(int managerId);

    Task<List<Warehouse>> FindByIsActiveAsync(bool isActive);

    Task<List<Warehouse>> FindByLocationAsync(string location);

    Task<StockLevel?> FindStockByWarehouseAndProductAsync(int warehouseId, Guid productId);

    Task<List<StockLevel>> FindLowStockItemsAsync();

    Task<List<StockLevel>> GetAllStockAsync();

    Task<int> CountByIsActiveAsync(bool isActive);

    Task<Warehouse> SaveWarehouseAsync(Warehouse warehouse);

    Task UpdateWarehouseAsync(Warehouse warehouse);

    Task SaveStockAsync(StockLevel stock);

    Task UpdateStockAsync(StockLevel stock);
}