using Microsoft.EntityFrameworkCore;

public class SupplierRepository : ISupplierRepository
{
    private readonly SupplierDbContext _context;

    public SupplierRepository(SupplierDbContext context)
    {
        _context = context;
    }

    public async Task<Supplier?> FindBySupplierId(int id)
    {
        return await _context.Suppliers.FindAsync(id);
    }

    public async Task<List<Supplier>> FindByCity(string city)
    {
        return await _context.Suppliers.Where(s => s.City == city && s.IsActive).ToListAsync();
    }

    public async Task<List<Supplier>> FindByCountry(string country)
    {
        return await _context.Suppliers.Where(s => s.Country == country && s.IsActive).ToListAsync();
    }

    public async Task<List<Supplier>> SearchByName(string name)
    {
        return await _context.Suppliers.Where(s => s.Name.Contains(name) && s.IsActive).ToListAsync();
    }

    public async Task<List<Supplier>> FindByIsActive(bool isActive)
    {
        return await _context.Suppliers.Where(s => s.IsActive == isActive).ToListAsync();
    }

    public async Task<Supplier?> FindByTaxId(string taxId)
    {
        return await _context.Suppliers.FirstOrDefaultAsync(s => s.TaxId == taxId);
    }

    public async Task<int> CountByIsActive(bool isActive)
    {
        return await _context.Suppliers.CountAsync(s => s.IsActive == isActive);
    }

    public async Task DeleteBySupplierId(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier != null)
        {
            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
        }
    }
}