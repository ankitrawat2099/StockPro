public interface IWarehouseService
{
    Task<Warehouse> CreateWarehouseAsync(Warehouse warehouse);

    Task<Warehouse> GetByIdAsync(int id);

    Task<List<Warehouse>> GetAllWarehousesAsync();

    Task UpdateWarehouseAsync(Warehouse warehouse);

    Task DeactivateWarehouseAsync(int id);

    Task<StockLevel> GetStockLevelAsync(int warehouseId, Guid productId);

    Task UpdateStockAsync(int warehouseId, Guid productId, int qty);

    Task ReserveStockAsync(int warehouseId, Guid productId, int qty);

    Task ReleaseReservationAsync(int warehouseId, Guid productId, int qty);

    Task TransferStockAsync(int fromWarehouse, int toWarehouse, Guid productId, int qty);

    Task<List<StockLevel>> GetLowStockItemsAsync();
}