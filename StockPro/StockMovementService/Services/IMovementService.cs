public interface IMovementService
{
    Task RecordMovementAsync(CreateMovementDto dto);

    Task<List<StockMovement>> GetByProductAsync(Guid productId);

    Task<List<StockMovement>> GetByWarehouseAsync(int warehouseId);

    Task<List<StockMovement>> GetByTypeAsync(string type);

    Task<List<StockMovement>> GetByDateRangeAsync(DateTime start, DateTime end);

    Task<List<StockMovement>> GetByReferenceAsync(int referenceId);

    Task<List<StockMovement>> GetMovementHistoryAsync(Guid productId, int warehouseId);

    Task<int> GetStockInAsync(Guid productId);

    Task<int> GetStockOutAsync(Guid productId);

    Task<List<StockMovement>> GetAllMovementsAsync();

    Task<List<StockMovement>> GetByPerformedByAsync(Guid userId);
}