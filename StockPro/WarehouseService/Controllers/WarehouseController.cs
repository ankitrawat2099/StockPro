using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api")]
[Authorize]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _service;

    public WarehouseController(IWarehouseService service)
    {
        _service = service;
    }
    [Authorize(Roles = "ADMIN")]
    [HttpPost("warehouses")]
    public async Task<IActionResult> CreateWarehouse([FromBody] WarehouseDto dto)
    {
        var warehouse = new Warehouse
        {
            Name = dto.Name,
            Location = dto.Location,
            Address = dto.Address,
            ManagerId = dto.ManagerId,
            Capacity = dto.Capacity,
            Phone = dto.Phone,
            IsActive = true
        };

        var result = await _service.CreateWarehouseAsync(warehouse);
        return Ok(result);
    }

[Authorize(Roles ="STAFF,MANAGER,ADMIN,OFFICER")]
    [HttpGet("warehouses")]
    public async Task<IActionResult> GetAllWarehouses()
    {
        var result = await _service.GetAllWarehousesAsync();
        return Ok(result);
    }
    [AllowAnonymous]
    [HttpGet("warehouses/{id}")]
    public async Task<IActionResult> GetWarehouse(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }
    [Authorize(Roles ="ADMIN")]
    [HttpPut("warehouses/{id}")]
    public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] WarehouseDto dto)
    {
        var warehouse = await _service.GetByIdAsync(id);

        warehouse.Name = dto.Name;
        warehouse.Location = dto.Location;
        warehouse.Address = dto.Address;
        warehouse.ManagerId = dto.ManagerId;
        warehouse.Capacity = dto.Capacity;
        warehouse.Phone = dto.Phone;

        await _service.UpdateWarehouseAsync(warehouse);

        return Ok("Warehouse updated");
    }
[Authorize(Roles ="ADMIN")]
    [HttpDelete("warehouses/{id}")]
    public async Task<IActionResult> DeleteWarehouse(int id)
    {
        await _service.DeactivateWarehouseAsync(id);
        return Ok("Warehouse deactivated");
    }
 [Authorize(Roles ="STAFF")]
    [HttpPost("stock/update")]
    public async Task<IActionResult> UpdateStock([FromBody] StockRequestDto dto)
    {
        await _service.UpdateStockAsync(dto);
        return Ok("Stock updated");
    }
[Authorize(Roles ="STAFF,MANAGER,ADMIN,OFFICER")]
    [HttpGet("stock/{warehouseId}/{productId}")]
    public async Task<IActionResult> GetStock(int warehouseId, Guid productId)
    {
        var result = await _service.GetStockLevelAsync(warehouseId, productId);
        return Ok(result);
    }

 [Authorize(Roles ="STAFF")]
    [HttpPost("stock/reserve")]
    public async Task<IActionResult> ReserveStock([FromBody] StockRequestDto dto)
    {
        await _service.ReserveStockAsync(dto.WarehouseId, dto.ProductId, dto.Quantity);
        return Ok("Stock reserved");
    }
[Authorize(Roles ="STAFF")]
    [HttpPost("stock/release")]
    public async Task<IActionResult> ReleaseStock([FromBody] StockRequestDto dto)
    {
        await _service.ReleaseReservationAsync(dto.WarehouseId, dto.ProductId, dto.Quantity);
        return Ok("Reservation released");
    }

[Authorize(Roles ="STAFF,MANAGER")]
    [HttpPost("stock/transfer")]
    public async Task<IActionResult> TransferStock([FromBody] TransferStockDto dto)
    {
        await _service.TransferStockAsync(
            dto.FromWarehouse,
            dto.ToWarehouse,
            dto.ProductId,
            dto.Quantity
        );

        return Ok("Stock transferred");
    }
[AllowAnonymous]
    [HttpGet("stock/low")]
    public async Task<IActionResult> GetLowStock()
    {
        var result = await _service.GetLowStockItemsAsync();
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("stock/all")]
    public async Task<IActionResult> GetAllStock()
    {
        var result = await _service.GetAllStockAsync();
        return Ok(result);
    }
}