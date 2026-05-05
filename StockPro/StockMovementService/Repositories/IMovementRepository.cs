public interface IMovementRepository
{
    Task<List<StockMovement>> FindByProductIdAsync(Guid productId);

    Task<List<StockMovement>> FindByWarehouseIdAsync(int warehouseId);

    Task<List<StockMovement>> FindByMovementTypeAsync(string type);

    Task<List<StockMovement>> FindByReferenceIdAsync(int referenceId);

    Task<List<StockMovement>> FindByMovementDateBetweenAsync(DateTime start, DateTime end);

    Task<List<StockMovement>> FindByPerformedByAsync(Guid userId);

    Task<int> CountByProductIdAndTypeAsync(Guid productId, string type);

    Task AddAsync(StockMovement movement); // needed for record
}