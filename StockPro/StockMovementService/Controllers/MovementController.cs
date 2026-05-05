using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/movements")]
[Authorize]
public class MovementController : ControllerBase
{
    private readonly IMovementService _service;

    public MovementController(IMovementService service)
    {
        _service = service;
    }

[Authorize(Roles ="STAFF")]
    [HttpPost]
   public async Task<IActionResult> Record([FromBody] CreateMovementDto dto)
{
    await _service.RecordMovementAsync(dto);
    return Ok("Movement recorded");
}
[Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        return Ok(await _service.GetByProductAsync(productId));
    }
[Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("warehouse/{warehouseId}")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        return Ok(await _service.GetByWarehouseAsync(warehouseId));
    }

[Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("type")]
    public async Task<IActionResult> GetByType([FromQuery] string type)
    {
        return Ok(await _service.GetByTypeAsync(type));
    }
    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("date")]
    public async Task<IActionResult> GetByDateRange(DateTime start, DateTime end)
    {
        return Ok(await _service.GetByDateRangeAsync(start, end));
    }

[Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("reference/{referenceId}")]
    public async Task<IActionResult> GetByReference(int referenceId)
    {
        return Ok(await _service.GetByReferenceAsync(referenceId));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(Guid productId, int warehouseId)
    {
        return Ok(await _service.GetMovementHistoryAsync(productId, warehouseId));
    }
[Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("stockin/{productId}")]
    public async Task<IActionResult> GetStockIn(Guid productId)
    {
        return Ok(await _service.GetStockInAsync(productId));
    }
[Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("stockout/{productId}")]
    public async Task<IActionResult> GetStockOut(Guid productId)
    {
        return Ok(await _service.GetStockOutAsync(productId));
    }
[Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllMovementsAsync());
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(Guid userId)
    {
        return Ok(await _service.GetByPerformedByAsync(userId));
    }
}