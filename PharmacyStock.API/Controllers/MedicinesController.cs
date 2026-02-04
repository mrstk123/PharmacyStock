using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly IMedicineService _medicineService;

    public MedicinesController(IMedicineService medicineService)
    {
        _medicineService = medicineService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.MedicinesView)]
    public async Task<ActionResult<PaginatedResult<MedicineDto>>> GetMedicines([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? isActive = null, [FromQuery] string? sortField = null, [FromQuery] int? sortOrder = null)
    {
        var result = await _medicineService.GetPaginatedMedicinesAsync(page, pageSize, isActive, sortField, sortOrder);
        return Ok(result);
    }

    [HttpGet("search")]
    [Authorize(Policy = PermissionConstants.MedicinesView)]
    public async Task<ActionResult<IEnumerable<MedicineDto>>> Search([FromQuery] string q, [FromQuery] bool? isActive = null, [FromQuery] string? sortField = null, [FromQuery] int? sortOrder = null)
    {
        if (string.IsNullOrEmpty(q)) return BadRequest("Query string is required");
        var results = await _medicineService.SearchMedicinesAsync(q, isActive, sortField, sortOrder);
        return Ok(results);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionConstants.MedicinesView)]
    public async Task<ActionResult<MedicineDto>> GetMedicine(int id)
    {
        var medicine = await _medicineService.GetMedicineByIdAsync(id);
        if (medicine == null) return NotFound();
        return Ok(medicine);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.MedicinesCreate)]
    public async Task<ActionResult<MedicineDto>> CreateMedicine(CreateMedicineDto createMedicineDto)
    {
        var medicine = await _medicineService.CreateMedicineAsync(createMedicineDto);
        return CreatedAtAction(nameof(GetMedicine), new { id = medicine.Id }, medicine);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionConstants.MedicinesEdit)]
    public async Task<IActionResult> UpdateMedicine(int id, UpdateMedicineDto updateMedicineDto)
    {
        if (id != updateMedicineDto.Id) return BadRequest();
        try
        {
            await _medicineService.UpdateMedicineAsync(updateMedicineDto);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionConstants.MedicinesDelete)]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        try
        {
            await _medicineService.DeleteMedicineAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
