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

    private static async Task<T> SafeAsync<T>(Func<Task<T>> factory, T fallback)
    {
        try
        {
            return await factory();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Report query fallback: {ex.Message}");
            return fallback;
        }
    }
//take snapshot
    public async Task TakeSnapshot()
    {
        Console.WriteLine("START SNAPSHOT");

        var istNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(istNow);

        var existing = await _repository.FindBySnapshotDate(today);
        if (existing.Any())
        {
            Console.WriteLine("Snapshot already exists for today");
            return;
        }

        var client = _httpClientFactory.CreateClient();
        var warehouseUrl = _config["WarehouseService:BaseUrl"];
        var productUrl = _config["ProductService:BaseUrl"];

        // Call warehouse to get ALL stock
        var stocks = await client.GetFromJsonAsync<List<StockLevelDto>>($"{warehouseUrl}/api/stock/all");

        if (stocks == null || !stocks.Any())
        {
            Console.WriteLine("No stock data found");
            return;
        }

        foreach (var stock in stocks)
        {
            // Call product to get cost
            var product = await client.GetFromJsonAsync<ProductDto>($"{productUrl}/api/products/{stock.ProductId}");
            if (product == null) continue;

            var snapshot = new InventorySnapshot
            {
                WarehouseId = stock.WarehouseId,
                ProductId = stock.ProductId,
                Quantity = stock.Quantity,
                StockValue = stock.Quantity * product.CostPrice,
                SnapshotDate = today
            };

            await _repository.AddAsync(snapshot);
        }

        Console.WriteLine("SNAPSHOT COMPLETE");
    }

    //total stock value
    public async Task<double> GetTotalStockValue()
    {
        return await SafeAsync(async () =>
        {
            var istNow = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(istNow);
            var data = await _repository.FindBySnapshotDate(today);

            return data.Sum(x => x.StockValue);
        }, 0);
    }

    public async Task<double> GetStockValueByWarehouse(int warehouseId)
    {
        return await SafeAsync(() => _repository.SumStockValueByWarehouse(warehouseId), 0d);
    }

    public async Task<double> GetInventoryTurnover(DateOnly start, DateOnly end)
    {
        return await SafeAsync(async () =>
        {
            var data = await _repository.FindByDateBetween(start, end);

            if (!data.Any()) return 0;

            var avg = data.Average(x => x.StockValue);

            return avg == 0 ? 0 : (double)(data.Sum(x => x.StockValue) / avg);
        }, 0d);
    }

    public async Task<List<InventorySnapshot>> GetLowStockReport()
    {
        return await SafeAsync(() => _repository.FindLowStockSnapshot(), new List<InventorySnapshot>());
    }

    public async Task<Dictionary<string, int>> GetStockMovementSummary()
    {
        var istNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        var today = DateOnly.FromDateTime(istNow);
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
        return await SafeAsync(async () =>
        {
            var istNow = DateTime.UtcNow;
            var data = await _repository.FindByDateBetween(
                DateOnly.FromDateTime(istNow.AddDays(-7)),
                DateOnly.FromDateTime(istNow)
            );

            return data.GroupBy(x => x.ProductId).OrderByDescending(g => g.Sum(x => x.Quantity))
                .Take(5).Select(g => g.Key).ToList();
        }, new List<Guid>());
    }

    public async Task<List<Guid>> GetSlowMovingProducts()
    {
        return await SafeAsync(async () =>
        {
            var istNow = DateTime.UtcNow;
            var data = await _repository.FindByDateBetween(
                DateOnly.FromDateTime(istNow.AddDays(-7)),
                DateOnly.FromDateTime(istNow)
            );

            return data
                .GroupBy(x => x.ProductId).OrderBy(g => g.Sum(x => x.Quantity)).Take(5).Select(g => g.Key).ToList();
        }, new List<Guid>());
    }

    public async Task<Dictionary<int, double>> GetPOSummary()
    {
        return await SafeAsync(async () =>
        {
            var istNow = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(istNow);

            var data = await _repository.FindBySnapshotDate(today);

            return data
                .GroupBy(x => x.WarehouseId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.StockValue)
                );
        }, new Dictionary<int, double>());
    }

    public async Task<byte[]> GenerateInventoryReport()
    {
        return await SafeAsync(async () =>
        {
            var istNow = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(istNow);
            var data = await _repository.FindBySnapshotDate(today);

            var text = string.Join("\n", data.Select(x =>
                $"Product:{x.ProductId} Qty:{x.Quantity} Value:{x.StockValue}"
            ));

            return System.Text.Encoding.UTF8.GetBytes(text);
        }, System.Text.Encoding.UTF8.GetBytes("No report data available."));
    }

    public async Task<List<Guid>> GetDeadStock()
    {
        return await SafeAsync(async () =>
        {
            var istNow = DateTime.UtcNow;
            var data = await _repository.FindByDateBetween(
                DateOnly.FromDateTime(istNow.AddDays(-30)),
                DateOnly.FromDateTime(istNow)
            );

            return data
                .GroupBy(x => x.ProductId).Where(g => g.All(x => x.Quantity == 0)).Select(g => g.Key).ToList();
        }, new List<Guid>());
    }
}
