
public class AlertServiceImpl : IAlertService
{
    private readonly IAlertRepository _repository;
    private readonly IConfiguration _config;

    public AlertServiceImpl(IAlertRepository repository, IConfiguration config)
    {
        _repository = repository;
        _config = config;
    }
    //send alert
    public async Task SendAlertAsync(Alert alert)
    {
        if (alert == null)
            throw new ArgumentException("Invalid alert");

        alert.CreatedAt = DateTime.UtcNow;
        alert.IsRead = false;
        alert.IsAcknowledged = false;

        await _repository.AddAsync(alert);

        if (alert.Severity?.ToUpper() == "CRITICAL"){
            await SendEmailAsync(alert);}
    }

    //low stock alert
    public async Task SendLowStockAlertAsync(Guid productId, int warehouseId, int qty, int recipientId)
    {
        var existingAlerts = await _repository.FindByRelatedProductIdAsync(productId);

        var alreadyExists = existingAlerts.Any(a =>
            a.RelatedWarehouseId == warehouseId &&
            !a.IsAcknowledged
        );

        if (alreadyExists)
        {
            Console.WriteLine("Alert already exists, skipping...");
            return;
        }

        var alert = new Alert
        {
            RecipientId = recipientId,
            Type = "LOW_STOCK",
            Severity = "WARNING",
            Title = "Low Stock Alert",
            Message = $"Product {productId} low in warehouse {warehouseId}. Qty: {qty}",
            RelatedProductId = productId,
            RelatedWarehouseId = warehouseId,
            Channel = "IN_APP"
        };

        await SendAlertAsync(alert);
    }

    //send bulk
    public async Task SendBulkAsync(List<Alert> alerts)
    {
        foreach (var alert in alerts)
            await SendAlertAsync(alert);
    }

    //mark as read
    public async Task MarkAsReadAsync(int alertId)
    {
        var alerts = await _repository.FindUnacknowledgedAsync();

        var alert = alerts.FirstOrDefault(x => x.AlertId == alertId);

        if (alert == null)
            throw new KeyNotFoundException("Alert not found");

        alert.IsRead = true;

        await _repository.AddAsync(alert);
    }

    //mark all read
    public async Task MarkAllReadAsync(int recipientId)
    {
        var alerts = await _repository.FindByRecipientIdAsync(recipientId);

        foreach (var alert in alerts)
        {
            if (!alert.IsRead)
            {
                alert.IsRead = true;
                await _repository.AddAsync(alert);
            }
        }
    }
    public async Task AcknowledgeAsync(int alertId)
    {
        var alerts = await _repository.FindUnacknowledgedAsync();

        var alert = alerts.FirstOrDefault(x => x.AlertId == alertId);

        if (alert == null)
            throw new KeyNotFoundException("Alert not found");

        alert.IsAcknowledged = true;

        await _repository.AddAsync(alert);
    }

    public async Task<List<Alert>> GetByRecipientAsync(int recipientId)
    {
        return await _repository.FindByRecipientIdAsync(recipientId);
    }

    public async Task<int> GetUnreadCountAsync(int recipientId)
    {

        return await _repository.CountByRecipientIdAndIsReadAsync(recipientId, false);
    }

    public async Task<List<Alert>> GetUnacknowledgedAsync()
    {
        return await _repository.FindUnacknowledgedAsync();
    }

    public async Task DeleteAlertAsync(int alertId)
    {
         await _repository.DeleteByAlertIdAsync(alertId);
    }

    public Task SendEmailAsync(Alert alert)
    {
        Console.WriteLine($" EMAIL → User {alert.RecipientId} | {alert.Title} | {alert.Message}");
        return Task.CompletedTask;
    }
}