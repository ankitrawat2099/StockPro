using Microsoft.EntityFrameworkCore;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly PurchaseDbContext _context;

    public PurchaseRepository(PurchaseDbContext context)
    {
        _context = context;
    }
    //find by id
    public async Task<PurchaseOrder> FindByPoId(int poId)
    {
        return await _context.PurchaseOrders.FirstOrDefaultAsync(x => x.PoId == poId);
    }
    //find by supp;ierid
    public async Task<List<PurchaseOrder>> FindBySupplierId(int supplierId)
    {
        return await _context.PurchaseOrders.Where(x => x.SupplierId == supplierId).ToListAsync();
    }
    //find by warehouseid
    public async Task<List<PurchaseOrder>> FindByWarehouseId(int warehouseId)
    {
        return await _context.PurchaseOrders.Where(x => x.WarehouseId == warehouseId).ToListAsync();
    }
    //find by status
    public async Task<List<PurchaseOrder>> FindByStatus(string status)
    {
        return await _context.PurchaseOrders.Where(x => x.Status == status).ToListAsync();
    }
    //find by order date 
    public async Task<List<PurchaseOrder>> FindByOrderDateBetween(DateTime start, DateTime end)
    {
        return await _context.PurchaseOrders.Where(x => x.OrderDate >= start && x.OrderDate <= end).ToListAsync();
    }
    //find by createdby id
    public async Task<List<PurchaseOrder>> FindByCreatedById(Guid userId)
    {
        return await _context.PurchaseOrders.Where(x => x.CreatedById == userId).ToListAsync();
    }
    //count by status
    public async Task<int> CountByStatus(string status)
    {
        return await _context.PurchaseOrders.CountAsync(x => x.Status == status);
    }
}