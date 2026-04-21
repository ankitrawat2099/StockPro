using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/suppliers")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _service;

    public SupplierController(ISupplierService service)
    {
        _service = service;
    }
    [HttpPost]
    [Authorize(Roles = "ADMIN,OFFICER")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        var result = await _service.CreateSupplier(dto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetById(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllSuppliers();
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        var result = await _service.SearchSuppliers(name);
        return Ok(result);
    }

    [HttpGet("city")]
    public async Task<IActionResult> GetByCity([FromQuery] string city)
    {
        var result = await _service.GetByCity(city);
        return Ok(result);
    }

    [HttpGet("country")]
    public async Task<IActionResult> GetByCountry([FromQuery] string country)
    {
        var result = await _service.GetByCountry(country);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,OFFICER")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
    {
        var result = await _service.UpdateSupplier(id, dto);
        return Ok(result);
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateSupplier(id);
        return Ok(new { message = "Supplier deactivated successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteSupplier(id);
        return Ok(new { message = "Supplier deleted successfully" });
    }

    [HttpPut("{id}/rating")]
    [Authorize(Roles = "OFFICER")]
    public async Task<IActionResult> UpdateRating(int id, [FromQuery] double rating)
    {
        await _service.UpdateRating(id, rating);
        return Ok(new { message = "Rating updated successfully" });
    }
}