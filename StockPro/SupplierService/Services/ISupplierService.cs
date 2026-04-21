public interface ISupplierService
{
    Task<SupplierResponseDto> CreateSupplier(CreateSupplierDto dto);

    Task<SupplierResponseDto> GetById(int id);

    Task<List<SupplierResponseDto>> GetAllSuppliers();

    Task<List<SupplierResponseDto>> SearchSuppliers(string name);

    Task<List<SupplierResponseDto>> GetByCity(string city);

    Task<List<SupplierResponseDto>> GetByCountry(string country);

    Task<SupplierResponseDto> UpdateSupplier(int id, UpdateSupplierDto dto);

    Task DeactivateSupplier(int id);

    Task DeleteSupplier(int id);

    Task UpdateRating(int supplierId, double rating);
}