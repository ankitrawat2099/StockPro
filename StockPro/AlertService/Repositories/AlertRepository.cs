using Microsoft.EntityFrameworkCore;

public class AlertRepository : IAlertRepository
{
    private readonly AlertDbContext _context;

    public AlertRepository(AlertDbContext context)
    {
        _context = context;
    }

    public async Task<List<Alert>> FindByRecipientIdAsync(int recipientId)
    {
        return await _context.Alerts.Where(x => x.RecipientId == recipientId).ToListAsync();
    }

    public async Task<List<Alert>> FindByRecipientIdAndIsReadAsync(int recipientId, bool isRead)
    {
        return await _context.Alerts.Where(x => x.RecipientId == recipientId && x.IsRead == isRead).ToListAsync();
    }

    public async Task<int> CountByRecipientIdAndIsReadAsync(int recipientId, bool isRead)
    {
        return await _context.Alerts.CountAsync(x => x.RecipientId == recipientId && x.IsRead == isRead);
    }

    public async Task<List<Alert>> FindByTypeAsync(string type)
    {
        return await _context.Alerts.Where(x => x.Type == type).ToListAsync();
    }

    public async Task<List<Alert>> FindBySeverityAsync(string severity)
    {
        return await _context.Alerts.Where(x => x.Severity == severity).ToListAsync();
    }

    public async Task<List<Alert>> FindByRelatedProductIdAsync(Guid productId)
    {
        return await _context.Alerts.Where(x => x.RelatedProductId == productId).ToListAsync();
    }

    public async Task<List<Alert>> FindUnacknowledgedAsync()
    {
        return await _context.Alerts.Where(x => !x.IsAcknowledged).ToListAsync();
    }

    public async Task AddAsync(Alert alert)
    {
        if (alert.AlertId == 0)
        {
            await _context.Alerts.AddAsync(alert);
        }
        else
        {
            _context.Alerts.Update(alert);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteByAlertIdAsync(int alertId)
    {
        var alert = await _context.Alerts.FindAsync(alertId);
        if (alert != null)
        {
            _context.Alerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }
}