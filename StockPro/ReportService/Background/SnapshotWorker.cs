using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class SnapshotWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SnapshotWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Taking daily snapshot...");

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IReportService>();

            await service.TakeSnapshot();

            //run once every 24 hours
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}