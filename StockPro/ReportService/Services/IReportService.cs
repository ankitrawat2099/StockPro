public interface IReportService
{
    Task TakeSnapshot();

    Task<double> GetTotalStockValue();

    Task<double> GetStockValueByWarehouse(int warehouseId);

    Task<double> GetInventoryTurnover(DateOnly start, DateOnly end);

    Task<List<InventorySnapshot>> GetLowStockReport();

    Task<Dictionary<string, int>> GetStockMovementSummary();

    Task<List<Guid>> GetTopMovingProducts();

    Task<List<Guid>> GetSlowMovingProducts();

    Task<Dictionary<int, double>> GetPOSummary();

    Task<byte[]> GenerateInventoryReport();

    Task<List<Guid>> GetDeadStock();
}