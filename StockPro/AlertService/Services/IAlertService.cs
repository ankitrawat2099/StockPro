public interface IAlertService
{
    Task SendAlertAsync(Alert alert);
    Task SendLowStockAlertAsync(Guid productId, int warehouseId, int qty, int recipientId);
    Task SendBulkAsync(List<Alert> alerts);

    Task MarkAsReadAsync(int alertId);
    Task MarkAllReadAsync(int recipientId);
    Task AcknowledgeAsync(int alertId);

    Task<List<Alert>> GetByRecipientAsync(int recipientId);
    Task<int> GetUnreadCountAsync(int recipientId);
    Task<List<Alert>> GetUnacknowledgedAsync();

    Task DeleteAlertAsync(int alertId);
    Task SendEmailAsync(Alert alert);
}