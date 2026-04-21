using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/purchase-orders")]
[Authorize]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _service;

    public PurchaseController(IPurchaseService service)
    {
        _service = service;
    }
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var po = new PurchaseOrder
        {
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            CreatedById = Guid.Parse(userId),
            ExpectedDate = dto.ExpectedDate,
            Notes = dto.Notes,
            ReferenceNumber = dto.ReferenceNumber,
            Status = "DRAFT",
            OrderDate = DateTime.UtcNow,
            TotalAmount = dto.Items.Sum(x => x.Quantity * x.UnitCost)
        };

        return Ok(await _service.CreatePO(po, dto.Items));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var po = await _service.GetPOById(id);
        if (po == null) return NotFound();
        return Ok(po);
    }

    [HttpGet("supplier/{supplierId}")]
    public async Task<IActionResult> GetBySupplier(int supplierId)
    {
        return Ok(await _service.GetPOsBySupplier(supplierId));
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        return Ok(await _service.GetPOsByStatus(status));
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        return Ok(await _service.GetPOsByWarehouse(warehouseId));
    }

    [HttpGet("dateRange")]
    public async Task<IActionResult> GetByDateRange(DateTime start, DateTime end)
    {
        return Ok(await _service.GetPOsByDateRange(start, end));
    }

    [HttpPut("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        await _service.ApprovePO(id);
        return Ok("Approved");
    }

    [HttpPost("{id}/receive")]
    public async Task<IActionResult> Receive(int id, [FromBody] ReceiveGoodsDto dto)
    {
        await _service.ReceiveGoods(id, dto);
        return Ok("Goods Received");
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelPO(id);
        return Ok("Cancelled");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseOrderDto dto)
    {
        var po = await _service.GetPOById(id);
        if (po == null) return NotFound();

        po.SupplierId = dto.SupplierId;
        po.WarehouseId = dto.WarehouseId;
        po.ExpectedDate = dto.ExpectedDate;
        po.Notes = dto.Notes;
        po.ReferenceNumber = dto.ReferenceNumber;

        await _service.UpdatePO(po);
        return Ok("Updated");
    }
}