public interface ISupplierRepository
{
    Task<Supplier> FindBySupplierId(int id);

    Task<List<Supplier>> FindByCity(string city);
    Task<List<Supplier>> FindByCountry(string country);

    Task<List<Supplier>> SearchByName(string name);

    Task<List<Supplier>> FindByIsActive(bool isActive);

    Task<Supplier> FindByTaxId(string taxId);

    Task<int> CountByIsActive(bool isActive);

    Task DeleteBySupplierId(int id);
}