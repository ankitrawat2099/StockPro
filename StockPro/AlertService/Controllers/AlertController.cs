using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertController(IAlertService service)
    {
        _service = service;
    }

    [HttpGet("{recipientId}")]
    public async Task<IActionResult> Get(int recipientId)
    {
        return Ok(await _service.GetByRecipientAsync(recipientId));
    }

    [HttpGet("{recipientId}/unread")]
    public async Task<IActionResult> GetUnreadCount(int recipientId)
    {
        return Ok(await _service.GetUnreadCountAsync(recipientId));
    }

    [HttpGet("{recipientId}/unacknowledged")]
    public async Task<IActionResult> GetUnacknowledged(int recipientId)
    {
        var alerts = await _service.GetUnacknowledgedAsync();
        return Ok(alerts.Where(a => a.RecipientId == recipientId));
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        await _service.MarkAsReadAsync(id);
        return Ok("Marked as read");
    }

    [HttpPost("{recipientId}/read-all")]
    public async Task<IActionResult> MarkAllRead(int recipientId)
    {
        await _service.MarkAllReadAsync(recipientId);
        return Ok("All marked as read");
    }

    [HttpPost("{id}/ack")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        await _service.AcknowledgeAsync(id);
        return Ok("Acknowledged");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAlertAsync(id);
        return Ok("Deleted");
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAlertDto dto)
    {
        var alert = new Alert
        {
            RecipientId = dto.RecipientId, 
            Type = dto.Type,
            Severity = dto.Severity,
            Title = dto.Title,
            Message = dto.Message,
            RelatedProductId = dto.RelatedProductId,
            RelatedWarehouseId = dto.RelatedWarehouseId,
            Channel = dto.Channel
        };

        await _service.SendAlertAsync(alert);

        return Ok("Alert created");
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> Bulk([FromBody] List<CreateAlertDto> dtos)
    {
        var alerts = dtos.Select(dto => new Alert
        {
            RecipientId = dto.RecipientId,
            Type = dto.Type,
            Severity = dto.Severity,
            Title = dto.Title,
            Message = dto.Message,
            RelatedProductId = dto.RelatedProductId,
            RelatedWarehouseId = dto.RelatedWarehouseId,
            Channel = dto.Channel
        }).ToList();

        await _service.SendBulkAsync(alerts);

        return Ok("Bulk alerts sent");
    }
}