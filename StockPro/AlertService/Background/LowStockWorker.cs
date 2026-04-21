using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public class LowStockWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public LowStockWorker(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var warehouseBaseUrl = _config["WarehouseService:BaseUrl"];
        var productBaseUrl = _config["ProductService:BaseUrl"];
        var delay = int.TryParse(_config["Worker:IntervalSeconds"], out var d) ? d : 10;

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Worker running...");

            using var scope = _serviceProvider.CreateScope();
            var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

            var client = _httpClientFactory.CreateClient();

            try
            {
                //call low stock api
                var response = await client.GetAsync($"{warehouseBaseUrl}/api/stock/low");
                Console.WriteLine($" Warehouse LowStock API: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Failed to fetch low stock");
                    continue;
                }

                var stockList = await response.Content.ReadFromJsonAsync<List<StockLevelDto>>();
                Console.WriteLine($"Items found: {stockList?.Count ?? 0}");

                if (stockList == null || !stockList.Any())
                {
                    Console.WriteLine("No low stock items");
                    continue;
                }

                foreach (var stock in stockList)
                {
                    Console.WriteLine($"\nChecking ProductId: {stock.ProductId}");
                    Console.WriteLine($"Stock Qty: {stock.Quantity}");

                    //get product
                    var productRes = await client.GetAsync($"{productBaseUrl}/api/products/{stock.ProductId}");
                    Console.WriteLine($"Product API: {productRes.StatusCode}");

                    if (!productRes.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Product API failed");
                        continue;
                    }

                    var product = await productRes.Content.ReadFromJsonAsync<ProductDto>();

                    if (product == null)
                    {
                        Console.WriteLine("Product is NULL");
                        continue;
                    }

                    Console.WriteLine($"ReorderLevel: {product.ReorderLevel}");

                    //cehck contiditon
                    if (stock.Quantity >= product.ReorderLevel)
                    {
                        Console.WriteLine("Stock is sufficient (no alert)");
                        continue;
                    }

                    Console.WriteLine("LOW STOCK DETECTED");

                    // get warehouse
                    var whRes = await client.GetAsync($"{warehouseBaseUrl}/api/warehouses/{stock.WarehouseId}");
                    Console.WriteLine($"Warehouse API: {whRes.StatusCode}");

                    if (!whRes.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Warehouse API failed");
                        continue;
                    }

                    var warehouse = await whRes.Content.ReadFromJsonAsync<WarehouseDto>();

                    if (warehouse == null)
                    {
                        Console.WriteLine("Warehouse is NULL");
                        continue;
                    }

                    Console.WriteLine($"ManagerId: {warehouse.ManagerId}");

                    // create alert
                    await alertService.SendLowStockAlertAsync(
                        stock.ProductId,
                        stock.WarehouseId,
                        stock.Quantity,
                        warehouse.ManagerId
                    );

                    Console.WriteLine("ALERT CREATED");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Worker Error: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
        }
    }
}