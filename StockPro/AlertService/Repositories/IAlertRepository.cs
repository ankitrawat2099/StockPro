public interface IAlertRepository
{
    Task<List<Alert>> FindByRecipientIdAsync(int recipientId);
    Task<List<Alert>> FindByRecipientIdAndIsReadAsync(int recipientId, bool isRead);
    Task<int> CountByRecipientIdAndIsReadAsync(int recipientId, bool isRead);
    Task<List<Alert>> FindByTypeAsync(string type);
    Task<List<Alert>> FindBySeverityAsync(string severity);
    Task<List<Alert>> FindByRelatedProductIdAsync(Guid productId);
    Task<List<Alert>> FindUnacknowledgedAsync();

    Task AddAsync(Alert alert);
    Task DeleteByAlertIdAsync(int alertId);
}