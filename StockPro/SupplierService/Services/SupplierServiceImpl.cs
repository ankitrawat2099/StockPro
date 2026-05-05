public class SupplierServiceImpl : ISupplierService
{
    private readonly ISupplierRepository _repository;
    private readonly SupplierDbContext _context;

    public SupplierServiceImpl(ISupplierRepository repository, SupplierDbContext context)
    {
        _repository = repository;
        _context = context;
    }
    public async Task<SupplierResponseDto> CreateSupplier(CreateSupplierDto dto)
    {
        if (!string.IsNullOrEmpty(dto.TaxId))
        {
            var existing = await _repository.FindByTaxId(dto.TaxId);
            if (existing != null)
                throw new Exception("Supplier with same TaxId exists");
        }

        var supplier = new Supplier
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            TaxId = dto.TaxId,
            PaymentTerms = dto.PaymentTerms,
            LeadTimeDays = dto.LeadTimeDays,
            Rating = 0,
            IsActive = true
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task<SupplierResponseDto> GetById(int id)
    {
        var supplier = await _repository.FindBySupplierId(id);

        if (supplier == null)
            throw new Exception("Supplier not found");

        return MapToDto(supplier);
    }
    public async Task<List<SupplierResponseDto>> GetAllSuppliers()
    {
        var suppliers = await _repository.FindByIsActive(true);
        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<List<SupplierResponseDto>> SearchSuppliers(string name)
    {
        var suppliers = await _repository.SearchByName(name);
        return suppliers.Select(MapToDto).ToList();
    }
    public async Task<List<SupplierResponseDto>> GetByCity(string city)
    {
        var suppliers = await _repository.FindByCity(city);
        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<List<SupplierResponseDto>> GetByCountry(string country)
    {
        var suppliers = await _repository.FindByCountry(country);
        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<SupplierResponseDto> UpdateSupplier(int id, UpdateSupplierDto dto)
    {
        var supplier = await _repository.FindBySupplierId(id);

        if (supplier == null)
            throw new Exception("Supplier not found");

        if (!string.IsNullOrEmpty(dto.Name)) supplier.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Email)) supplier.Email = dto.Email;
        if (!string.IsNullOrEmpty(dto.Phone)) supplier.Phone = dto.Phone;
        if (!string.IsNullOrEmpty(dto.City)) supplier.City = dto.City;
        if (!string.IsNullOrEmpty(dto.Country)) supplier.Country = dto.Country;
        if (!string.IsNullOrEmpty(dto.PaymentTerms)) supplier.PaymentTerms = dto.PaymentTerms;
        if (dto.LeadTimeDays > 0) supplier.LeadTimeDays = dto.LeadTimeDays;

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();

        return MapToDto(supplier);
    }

    public async Task DeactivateSupplier(int id)
    {
        var supplier = await _repository.FindBySupplierId(id);

        if (supplier == null)
            throw new Exception("Supplier not found");

        supplier.IsActive = false;

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSupplier(int id)
    {
        await _repository.DeleteBySupplierId(id);
    }

    public async Task UpdateRating(int supplierId, double rating)
    {
        if (rating < 0 || rating > 5)
            throw new Exception("Invalid rating");

        var supplier = await _repository.FindBySupplierId(supplierId);

        if (supplier == null)
            throw new Exception("Supplier not found");

        supplier.Rating = Math.Round((supplier.Rating + rating) / 2, 2);

        _context.Suppliers.Update(supplier);
        await _context.SaveChangesAsync();
    }

    private SupplierResponseDto MapToDto(Supplier s)
    {
        return new SupplierResponseDto
        {
            SupplierId = s.SupplierId,
            Name = s.Name,
            ContactPerson = s.ContactPerson,
            Email = s.Email,
            Phone = s.Phone,
            City = s.City,
            Country = s.Country,
            PaymentTerms = s.PaymentTerms,
            LeadTimeDays = s.LeadTimeDays,
            Rating = s.Rating,
            IsActive = s.IsActive
        };
    }
}