using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly IReportService _service;

    public ReportController(IReportService service)
    {
        _service = service;
    }

    // TOTAL VALUE
    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("totalValue")]
    public async Task<IActionResult> GetTotalValue()
    {
        return Ok(await _service.GetTotalStockValue());
    }

    // BY WAREHOUSE
    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("byWarehouse")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        return Ok(await _service.GetStockValueByWarehouse(warehouseId));
    }

    //TURNOVER
    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("turnover")]
    public async Task<IActionResult> GetTurnover(DateOnly start, DateOnly end)
    {
        return Ok(await _service.GetInventoryTurnover(start, end));
    }

    //LOW STOCK
    [Authorize(Roles ="MANAGER")]
    [HttpGet("lowStock")]
    public async Task<IActionResult> GetLowStock()
    {
        return Ok(await _service.GetLowStockReport());
    }

    //TOP MOVING
    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("topMoving")]
    public async Task<IActionResult> GetTopMoving()
    {
        return Ok(await _service.GetTopMovingProducts());
    }

    //SLOW MOVING
    [Authorize(Roles ="MANAGER")]
    [HttpGet("slowMoving")]
    public async Task<IActionResult> GetSlowMoving()
    {
        return Ok(await _service.GetSlowMovingProducts());
    }

    //DEAD STOCK
    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("deadStock")]
    public async Task<IActionResult> GetDeadStock()
    {
        return Ok(await _service.GetDeadStock());
    }

    //PO SUMMARY
    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("poSummary")]
    public async Task<IActionResult> GetPOSummary()
    {
        return Ok(await _service.GetPOSummary());
    }

    [Authorize(Roles ="MANAGER,ADMIN")]
    [HttpGet("generateReport")]
    public async Task<IActionResult> GenerateReport()
    {
        var file = await _service.GenerateInventoryReport();
        return File(file, "application/octet-stream", "report.txt");
    }

}