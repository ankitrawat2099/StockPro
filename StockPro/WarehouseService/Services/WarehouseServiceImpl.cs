public class WarehouseServiceImpl : IWarehouseService
{
    private readonly IWarehouseRepository _repository;
    private readonly WarehouseDbContext _context;

    public WarehouseServiceImpl(IWarehouseRepository repository, WarehouseDbContext context)
    {
        _repository = repository;
        _context = context;
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

    public async Task UpdateStockAsync(int warehouseId, Guid productId, int qty)
    {
        var warehouse = await _repository.FindByWarehouseIdAsync(warehouseId);

        if (warehouse == null)
            throw new KeyNotFoundException("Warehouse not found");

        if (!warehouse.IsActive)
            throw new InvalidOperationException("Warehouse is inactive");

        var stock = await _repository.FindStockByWarehouseAndProductAsync(warehouseId, productId);

        if (stock == null)
        {
            stock = new StockLevel
            {
                WarehouseId = warehouseId,
                ProductId = productId,
                Quantity = qty,
                ReservedQuantity = 0,
                Location = warehouse.Location
            };

            await _repository.SaveStockAsync(stock);
        }
        else
        {
            stock.Quantity += qty;

            if (stock.Quantity < 0)
                throw new ArgumentException("Stock cannot be negative");

            await _repository.UpdateStockAsync(stock);
        }
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
}