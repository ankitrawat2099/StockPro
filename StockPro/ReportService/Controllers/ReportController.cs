using Microsoft.AspNetCore.Mvc;

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
    [HttpGet("totalValue")]
    public async Task<IActionResult> GetTotalValue()
    {
        return Ok(await _service.GetTotalStockValue());
    }

    // BY WAREHOUSE
    [HttpGet("byWarehouse")]
    public async Task<IActionResult> GetByWarehouse(int warehouseId)
    {
        return Ok(await _service.GetStockValueByWarehouse(warehouseId));
    }

    //TURNOVER
    [HttpGet("turnover")]
    public async Task<IActionResult> GetTurnover(DateOnly start, DateOnly end)
    {
        return Ok(await _service.GetInventoryTurnover(start, end));
    }

    //LOW STOCK
    [HttpGet("lowStock")]
    public async Task<IActionResult> GetLowStock()
    {
        return Ok(await _service.GetLowStockReport());
    }

    //TOP MOVING
    [HttpGet("topMoving")]
    public async Task<IActionResult> GetTopMoving()
    {
        return Ok(await _service.GetTopMovingProducts());
    }

    //SLOW MOVING
    [HttpGet("slowMoving")]
    public async Task<IActionResult> GetSlowMoving()
    {
        return Ok(await _service.GetSlowMovingProducts());
    }

    //DEAD STOCK
    [HttpGet("deadStock")]
    public async Task<IActionResult> GetDeadStock()
    {
        return Ok(await _service.GetDeadStock());
    }

    //PO SUMMARY
    [HttpGet("poSummary")]
    public async Task<IActionResult> GetPOSummary()
    {
        return Ok(await _service.GetPOSummary());
    }

    //GENERATE REPORT
    [HttpGet("generateReport")]
    public async Task<IActionResult> GenerateReport()
    {
        var file = await _service.GenerateInventoryReport();
        return File(file, "application/octet-stream", "report.txt");
    }
}