using Microsoft.EntityFrameworkCore;

public class MovementRepository : IMovementRepository
{
    private readonly MovementDbContext _context;

    public MovementRepository(MovementDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(StockMovement movement)
    {
        await _context.StockMovements.AddAsync(movement);
        await _context.SaveChangesAsync();
    }

    public async Task<List<StockMovement>> FindByProductIdAsync(Guid productId)
    {
        return await _context.StockMovements.Where(x => x.ProductId == productId).ToListAsync();
    }

    public async Task<List<StockMovement>> FindByWarehouseIdAsync(int warehouseId)
    {
        return await _context.StockMovements.Where(x => x.WarehouseId == warehouseId).ToListAsync();
    }

    public async Task<List<StockMovement>> FindByMovementTypeAsync(string type)
    {
        return await _context.StockMovements.Where(x => x.MovementType == type).ToListAsync();
    }

    public async Task<List<StockMovement>> FindByReferenceIdAsync(int referenceId)
    {
        return await _context.StockMovements.Where(x => x.ReferenceId == referenceId).ToListAsync();
    }

    public async Task<List<StockMovement>> FindByMovementDateBetweenAsync(DateTime start, DateTime end)
    {
        return await _context.StockMovements.Where(x => x.MovementDate >= start && x.MovementDate <= end).ToListAsync();
    }

    public async Task<List<StockMovement>> FindByPerformedByAsync(Guid userId)
    {
        return await _context.StockMovements.Where(x => x.PerformedBy == userId).ToListAsync();
    }

    public async Task<int> CountByProductIdAndTypeAsync(Guid productId, string type)
    {
        return await _context.StockMovements.Where(x => x.ProductId == productId && x.MovementType == type).SumAsync(x => x.Quantity);
    }
}