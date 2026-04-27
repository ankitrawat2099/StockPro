using System.Net.Http.Headers;
using System.Net.Http.Json;

public class WarehouseServiceImpl : IWarehouseService
{
    private readonly IWarehouseRepository _repository;
    private readonly WarehouseDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public WarehouseServiceImpl(
        IWarehouseRepository repository,
        WarehouseDbContext context,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _repository = repository;
        _context = context;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    private async Task TryCreateMovementAsync(CreateMovementDto dto)
    {
        try
        {
            var movementServiceUrl = _configuration["Services:MovementService"];
            if (string.IsNullOrWhiteSpace(movementServiceUrl))
            {
                Console.WriteLine("Movement service URL is not configured.");
                return;
            }

            var token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Skipping automatic movement creation because no bearer token was found.");
                return;
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);

            var response = await client.PostAsJsonAsync($"{movementServiceUrl}/api/movements", dto);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Automatic movement creation failed: {response.StatusCode} {body}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Automatic movement creation error: {ex.Message}");
        }
    }

    public async Task<Warehouse> CreateWarehouseAsync(Warehouse warehouse)
    {
        return await _repository.SaveWarehouseAsync(warehouse);
    }

    public async Task<Warehouse> GetByIdAsync(int id)
    {
        var wh = await _repository.FindByWarehouseIdAsync(id);

        if (wh == null)
            throw new KeyNotFoundException("Warehouse not found");

        return wh;
    }


    public async Task<List<Warehouse>> GetAllWarehousesAsync()
    {
        return await _repository.FindByIsActiveAsync(true);
    }

    public async Task UpdateWarehouseAsync(Warehouse warehouse)
    {
        await _repository.UpdateWarehouseAsync(warehouse);
    }

    public async Task DeactivateWarehouseAsync(int id)
    {
        var wh = await _repository.FindByWarehouseIdAsync(id);

        if (wh == null)
            throw new KeyNotFoundException("Warehouse not found");

        wh.IsActive = false;
        await _repository.UpdateWarehouseAsync(wh);
    }

    public async Task<StockLevel> GetStockLevelAsync(int warehouseId, Guid productId)
    {
        var warehouse = await _repository.FindByWarehouseIdAsync(warehouseId);

        if (warehouse == null)
            throw new KeyNotFoundException("Warehouse not found");

        if (!warehouse.IsActive)
            throw new InvalidOperationException("Warehouse is inactive");

        var stock = await _repository.FindStockByWarehouseAndProductAsync(warehouseId, productId);

        if (stock == null)
            throw new KeyNotFoundException("Stock not found");

        return stock;
    }

    public async Task UpdateStockAsync(StockRequestDto dto)
    {
        var warehouse = await _repository.FindByWarehouseIdAsync(dto.WarehouseId);

        if (warehouse == null)
            throw new KeyNotFoundException("Warehouse not found");

        if (!warehouse.IsActive)
            throw new InvalidOperationException("Warehouse is inactive");

        var stock = await _repository.FindStockByWarehouseAndProductAsync(dto.WarehouseId, dto.ProductId);

        if (stock == null)
        {
            stock = new StockLevel
            {
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                ReservedQuantity = 0,
                Location = warehouse.Location
            };

            await _repository.SaveStockAsync(stock);
        }
        else
        {
            stock.Quantity += dto.Quantity;

            if (stock.Quantity < 0)
                throw new ArgumentException("Stock cannot be negative");

            await _repository.UpdateStockAsync(stock);
        }

        await TryCreateMovementAsync(new CreateMovementDto
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            MovementType = dto.Quantity >= 0 ? "STOCK_IN" : "STOCK_OUT",
            Quantity = Math.Abs(dto.Quantity),
            BalanceAfter = stock.Quantity,
            ReferenceType = string.IsNullOrWhiteSpace(dto.ReferenceType) ? "WAREHOUSE" : dto.ReferenceType,
            ReferenceId = dto.ReferenceId,
            UnitCost = dto.UnitCost,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? (dto.Quantity >= 0 ? "Auto-created from warehouse stock update." : "Auto-created from warehouse stock deduction.") : dto.Notes
        });
    }

    public async Task ReserveStockAsync(int warehouseId, Guid productId, int qty)
    {
        var warehouse = await _repository.FindByWarehouseIdAsync(warehouseId);
        var stock = await _repository.FindStockByWarehouseAndProductAsync(warehouseId, productId);

        if (warehouse == null)
            throw new KeyNotFoundException("Warehouse not found");

        if (!warehouse.IsActive)
            throw new InvalidOperationException("Warehouse is inactive");

        if (stock == null)
            throw new KeyNotFoundException("Stock not found");

        if (stock.AvailableQuantity < qty)
            throw new ArgumentException("Not enough stock");

        stock.ReservedQuantity += qty;
        await _repository.UpdateStockAsync(stock);
    }

    public async Task ReleaseReservationAsync(int warehouseId, Guid productId, int qty)
    {
        var warehouse = await _repository.FindByWarehouseIdAsync(warehouseId);
        var stock = await _repository.FindStockByWarehouseAndProductAsync(warehouseId, productId);

        if (warehouse == null)
            throw new KeyNotFoundException("Warehouse not found");

        if (!warehouse.IsActive)
            throw new InvalidOperationException("Warehouse is inactive");

        if (stock == null)
            throw new KeyNotFoundException("Stock not found");

        if (stock.ReservedQuantity < qty)
            throw new ArgumentException("Invalid release quantity");

        stock.ReservedQuantity -= qty;
        await _repository.UpdateStockAsync(stock);
    }

    public async Task TransferStockAsync(int fromWarehouse, int toWarehouse, Guid productId, int qty)
    {
        using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            var source = await _repository.FindStockByWarehouseAndProductAsync(fromWarehouse, productId);
            var target = await _repository.FindStockByWarehouseAndProductAsync(toWarehouse, productId);

            var sourceWarehouse = await _repository.FindByWarehouseIdAsync(fromWarehouse);
            var targetWarehouse = await _repository.FindByWarehouseIdAsync(toWarehouse);

            if (sourceWarehouse == null || targetWarehouse == null)
                throw new KeyNotFoundException("Warehouse not found");

            if (!sourceWarehouse.IsActive || !targetWarehouse.IsActive)
                throw new InvalidOperationException("Warehouse is inactive");

            if (source == null)
                throw new KeyNotFoundException("Source stock not found");

            if (source.AvailableQuantity < qty)
                throw new ArgumentException("Insufficient stock");

            source.Quantity -= qty;
            await _repository.UpdateStockAsync(source);

            if (target == null)
            {
                target = new StockLevel
                {
                    WarehouseId = toWarehouse,
                    ProductId = productId,
                    Quantity = qty,
                    ReservedQuantity = 0,
                    Location = targetWarehouse.Location
                };

                await _repository.SaveStockAsync(target);
            }
            else
            {
                target.Quantity += qty;
                await _repository.UpdateStockAsync(target);
            }

            await tx.CommitAsync();

            await TryCreateMovementAsync(new CreateMovementDto
            {
                ProductId = productId,
                WarehouseId = fromWarehouse,
                MovementType = "TRANSFER_OUT",
                Quantity = qty,
                BalanceAfter = source.Quantity,
                ReferenceType = "TRANSFER",
                ReferenceId = toWarehouse,
                Notes = $"Auto-created from transfer to warehouse {toWarehouse}."
            });

            await TryCreateMovementAsync(new CreateMovementDto
            {
                ProductId = productId,
                WarehouseId = toWarehouse,
                MovementType = "TRANSFER_IN",
                Quantity = qty,
                BalanceAfter = target.Quantity,
                ReferenceType = "TRANSFER",
                ReferenceId = fromWarehouse,
                Notes = $"Auto-created from transfer from warehouse {fromWarehouse}."
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
    public async Task<List<StockLevel>> GetLowStockItemsAsync()
    {
        return await _repository.FindLowStockItemsAsync();
    }

    public async Task<List<StockLevel>> GetAllStockAsync()
    {
        return await _repository.GetAllStockAsync();
    }
}
