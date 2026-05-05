using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    //CREATE
    [Authorize(Roles = "MANAGER")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequest request)
    {
        if (!ModelState.IsValid){
            return BadRequest(ModelState);}

        var result = await _service.CreateProductAsync(request);
        return Ok(result);
    }

    //get by id
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    //get by sku
     [Authorize(Roles ="ADMIN,MANAGER,STAFF,OFFICER")]
    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetBySku(string sku)
    {
        var result = await _service.GetBySkuAsync(sku);
        return Ok(result);
    }

    //get by catergory
     [Authorize(Roles ="ADMIN,MANAGER,STAFF,OFFICER")]
    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var result = await _service.GetByCategoryAsync(category);
        return Ok(result);
    }

    //get by brand
     [Authorize(Roles ="ADMIN,MANAGER,STAFF,OFFICER")]
    [HttpGet("brand/{brand}")]
    public async Task<IActionResult> GetByBrand(string brand)
    {
        var result = await _service.GetByBrandAsync(brand);
        return Ok(result);
    }

    //get by barcode
     [Authorize(Roles ="ADMIN,MANAGER,STAFF,OFFICER")]
    [HttpGet("barcode/{barcode}")]
    public async Task<IActionResult> GetByBarcode(string barcode)
    {
        var result = await _service.GetByBarcodeAsync(barcode);
        return Ok(result);
    }

    //search
     [Authorize(Roles ="ADMIN,MANAGER,STAFF,OFFICER")]
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        var result = await _service.SearchProductsAsync(name);
        return Ok(result);
    }

    //get all
     [Authorize(Roles ="ADMIN,MANAGER,STAFF,OFFICER")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllProductsAsync();
        return Ok(result);
    }

    //low stock
    [Authorize(Roles ="ADMIN,MANAGER,STAFF,OFFICER")]
    [HttpGet("lowStock")]
    public async Task<IActionResult> GetLowStock()
    {
        var result = await _service.GetLowStockProductsAsync();
        return Ok(result);
    }

    //ypdate
    [Authorize(Roles = "MANAGER")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequest request)
    {
        var result = await _service.UpdateProductAsync(id, request);
        return Ok(result);
    }

    //deactivate
    [Authorize(Roles = "MANAGER")]
    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _service.DeactivateProductAsync(id);
        return Ok(new { message = "Product deactivated successfully" });
    }

    //delete
    [Authorize(Roles = "MANAGER")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteProductAsync(id);
        return Ok(new { message = "Product deleted successfully" });
    }
}