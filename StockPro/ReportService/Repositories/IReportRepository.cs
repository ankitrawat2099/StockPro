public interface IReportRepository
{
    Task<List<InventorySnapshot>> FindByWarehouseId(int warehouseId);

    Task<List<InventorySnapshot>> FindByProductId(Guid productId);

    Task<List<InventorySnapshot>> FindBySnapshotDate(DateOnly date);

    Task<List<InventorySnapshot>> FindByDateBetween(DateOnly start, DateOnly end);

    Task<double> SumStockValueByWarehouse(int warehouseId);

    Task<List<InventorySnapshot>> FindLowStockSnapshot();

    Task<double> AvgTurnoverByProduct(Guid productId);
    Task AddAsync(InventorySnapshot snapshot);
}