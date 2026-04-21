using Microsoft.EntityFrameworkCore;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly WarehouseDbContext _context;

    public WarehouseRepository(WarehouseDbContext context)
    {
        _context = context;
    }

    public async Task<Warehouse?> FindByWarehouseIdAsync(int id)
    {
        return await _context.Warehouses.FirstOrDefaultAsync(x => x.WarehouseId == id);
    }

    public async Task<List<Warehouse>> FindByManagerIdAsync(int managerId)
    {
        return await _context.Warehouses.Where(x => x.ManagerId == managerId).ToListAsync();
    }

    public async Task<List<Warehouse>> FindByIsActiveAsync(bool isActive)
    {
        return await _context.Warehouses.Where(x => x.IsActive == isActive).ToListAsync();
    }

    public async Task<List<Warehouse>> FindByLocationAsync(string location)
    {
        return await _context.Warehouses.Where(x => x.Location == location).ToListAsync();
    }

    public async Task<StockLevel?> FindStockByWarehouseAndProductAsync(int warehouseId, Guid productId)
    {
        return await _context.StockLevels.FirstOrDefaultAsync(x => x.WarehouseId == warehouseId && x.ProductId == productId);
    }

    public async Task<List<StockLevel>> FindLowStockItemsAsync()
    {
        return await _context.StockLevels.Where(x => (x.Quantity - x.ReservedQuantity) < 10).ToListAsync();
    }

    public async Task<int> CountByIsActiveAsync(bool isActive)
    {
        return await _context.Warehouses.CountAsync(x => x.IsActive == isActive);
    }

    public async Task<Warehouse> SaveWarehouseAsync(Warehouse warehouse)
    {
        await _context.Warehouses.AddAsync(warehouse);
        await _context.SaveChangesAsync();
        return warehouse;
    }

    public async Task UpdateWarehouseAsync(Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
        await _context.SaveChangesAsync();
    }

    public async Task SaveStockAsync(StockLevel stock)
    {
        await _context.StockLevels.AddAsync(stock);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStockAsync(StockLevel stock)
    {
        _context.StockLevels.Update(stock);
        await _context.SaveChangesAsync();
    }
}