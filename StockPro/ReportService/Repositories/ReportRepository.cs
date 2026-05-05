using Microsoft.EntityFrameworkCore;

public class ReportRepository : IReportRepository
{
    private readonly ReportDbContext _context;

    public ReportRepository(ReportDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventorySnapshot>> FindByWarehouseId(int warehouseId)
    {
        return await _context.InventorySnapshots
            .Where(x => x.WarehouseId == warehouseId)
            .ToListAsync();
    }

    public async Task<List<InventorySnapshot>> FindByProductId(Guid productId)
    {
        return await _context.InventorySnapshots
            .Where(x => x.ProductId == productId)
            .ToListAsync();
    }

    public async Task<List<InventorySnapshot>> FindBySnapshotDate(DateOnly date)
    {
        return await _context.InventorySnapshots
            .Where(x => x.SnapshotDate == date)
            .ToListAsync();
    }

    public async Task<List<InventorySnapshot>> FindByDateBetween(DateOnly start, DateOnly end)
    {
        return await _context.InventorySnapshots
            .Where(x => x.SnapshotDate >= start && x.SnapshotDate <= end)
            .ToListAsync();
    }
    public async Task<double> SumStockValueByWarehouse(int warehouseId)
    {
        return await _context.InventorySnapshots
            .Where(x => x.WarehouseId == warehouseId)
            .SumAsync(x => x.StockValue);
    }

    // 6. Low Stock Snapshot
    public async Task<List<InventorySnapshot>> FindLowStockSnapshot()
    {
        return await _context.InventorySnapshots
            .Where(x => x.Quantity < 10).ToListAsync();
    }

    public async Task<double> AvgTurnoverByProduct(Guid productId)
    {
        var data = await _context.InventorySnapshots.Where(x => x.ProductId == productId).ToListAsync();

        if (!data.Any()) return 0;

        return data.Average(x => (double)x.StockValue);
    }

    public async Task AddAsync(InventorySnapshot snapshot)
    {
        _context.InventorySnapshots.Add(snapshot);
        await _context.SaveChangesAsync();
    }
}