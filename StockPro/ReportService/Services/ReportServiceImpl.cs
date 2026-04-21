using System.Net.Http.Json;

public class ReportServiceImpl : IReportService
{
    private readonly IReportRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public ReportServiceImpl(IReportRepository repository,IHttpClientFactory httpClientFactory,IConfiguration config)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }
//take snapshot
    public async Task TakeSnapshot()
{
    Console.WriteLine("START SNAPSHOT");

    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var existing = await _repository.FindBySnapshotDate(today);
    if (existing.Any())
    {
        Console.WriteLine("Snapshot already exists for today");
        return;
    }

    var client = _httpClientFactory.CreateClient();

    var warehouseUrl = _config["WarehouseService:BaseUrl"];
    var productUrl = _config["ProductService:BaseUrl"];

    Console.WriteLine($"Warehouse URL: {warehouseUrl}");
    Console.WriteLine($"Product URL: {productUrl}");

    //call warehouse
    var stocks = await client.GetFromJsonAsync<List<StockLevelDto>>(
        $"{warehouseUrl}/api/stock/low"
    );

    Console.WriteLine($"Stocks fetched: {stocks?.Count ?? 0}");

    if (stocks == null || !stocks.Any())
    {
        Console.WriteLine("No stock data found");
        return;
    }

    foreach (var stock in stocks)
    {
        Console.WriteLine($"rocessing ProductId: {stock.ProductId}, Qty: {stock.Quantity}");

        //call product
        var product = await client.GetFromJsonAsync<ProductDto>(
            $"{productUrl}/api/products/{stock.ProductId}"
        );

        if (product == null)
        {
            Console.WriteLine($"Product NOT FOUND: {stock.ProductId}");
            continue;
        }

        Console.WriteLine($"Product found: {product.ProductId}, Cost: {product.CostPrice}");

        var value = stock.Quantity * product.CostPrice;

        Console.WriteLine($"Calculated Value: {value}");

        var snapshot = new InventorySnapshot
        {
            WarehouseId = stock.WarehouseId,
            ProductId = stock.ProductId,
            Quantity = stock.Quantity,
            StockValue = value,
            SnapshotDate = today
        };

        Console.WriteLine("Saving snapshot...");

        await _repository.AddAsync(snapshot);

        Console.WriteLine("Snapshot saved");
    }

    Console.WriteLine("SNAPSHOT COMPLETE");
}

    //total stock value
    public async Task<double> GetTotalStockValue()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var data = await _repository.FindBySnapshotDate(today);

        return data.Sum(x => x.StockValue);
    }

    public async Task<double> GetStockValueByWarehouse(int warehouseId)
    {
        return await _repository.SumStockValueByWarehouse(warehouseId);
    }

    public async Task<double> GetInventoryTurnover(DateOnly start, DateOnly end)
    {
        var data = await _repository.FindByDateBetween(start, end);

        if (!data.Any()) return 0;

        var avg = data.Average(x => x.StockValue);

        return avg == 0 ? 0 : (double)(data.Sum(x => x.StockValue) / avg);
    }

    public async Task<List<InventorySnapshot>> GetLowStockReport()
    {
        return await _repository.FindLowStockSnapshot();
    }

    public async Task<Dictionary<string, int>> GetStockMovementSummary()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yesterday = today.AddDays(-1);

        var todayData = await _repository.FindBySnapshotDate(today);
        var yesterdayData = await _repository.FindBySnapshotDate(yesterday);

        int stockIn = 0;
        int stockOut = 0;

        foreach (var t in todayData)
        {
            var y = yesterdayData.FirstOrDefault(x =>
                x.ProductId == t.ProductId &&
                x.WarehouseId == t.WarehouseId);

            if (y == null) continue;

            var diff = t.Quantity - y.Quantity;

            if (diff > 0) stockIn += diff;
            else stockOut += Math.Abs(diff);
        }

        return new Dictionary<string, int>
        {
            { "IN", stockIn },
            { "OUT", stockOut }
        };
    }

    public async Task<List<Guid>> GetTopMovingProducts()
    {
        var data = await _repository.FindByDateBetween(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        return data.GroupBy(x => x.ProductId).OrderByDescending(g => g.Sum(x => x.Quantity))
            .Take(5).Select(g => g.Key).ToList();
    }

    public async Task<List<Guid>> GetSlowMovingProducts()
    {
        var data = await _repository.FindByDateBetween(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)),
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        return data
            .GroupBy(x => x.ProductId).OrderBy(g => g.Sum(x => x.Quantity)).Take(5).Select(g => g.Key).ToList();
    }

    public async Task<Dictionary<int, double>> GetPOSummary()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var data = await _repository.FindBySnapshotDate(today);

        return data
            .GroupBy(x => x.WarehouseId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.StockValue)
            );
    }

    public async Task<byte[]> GenerateInventoryReport()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var data = await _repository.FindBySnapshotDate(today);

        var text = string.Join("\n", data.Select(x =>
            $"Product:{x.ProductId} Qty:{x.Quantity} Value:{x.StockValue}"
        ));

        return System.Text.Encoding.UTF8.GetBytes(text);
    }

    public async Task<List<Guid>> GetDeadStock()
    {
        var data = await _repository.FindByDateBetween(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        return data
            .GroupBy(x => x.ProductId).Where(g => g.All(x => x.Quantity == 0)).Select(g => g.Key).ToList();
    }
}