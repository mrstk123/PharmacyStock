using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.SuppliersView)]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionConstants.SuppliersView)]
    public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null) return NotFound();
        return Ok(supplier);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.SuppliersCreate)]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(CreateSupplierDto createSupplierDto)
    {
        var supplier = await _supplierService.CreateSupplierAsync(createSupplierDto);
        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, supplier);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionConstants.SuppliersEdit)]
    public async Task<IActionResult> UpdateSupplier(int id, UpdateSupplierDto updateSupplierDto)
    {
        if (id != updateSupplierDto.Id) return BadRequest();
        try
        {
            await _supplierService.UpdateSupplierAsync(updateSupplierDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionConstants.SuppliersDelete)]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        try
        {
            await _supplierService.DeleteSupplierAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
