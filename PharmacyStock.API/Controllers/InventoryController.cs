using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    [HttpGet("stock-check/{medicineId}")]
    [Authorize(Policy = PermissionConstants.StockView)]
    public async Task<ActionResult<StockCheckDto>> GetStockCheck(int medicineId)
    {
        var result = await _inventoryService.GetStockCheckAsync(medicineId);
        if (result == null) return NotFound("Medicine not found");
        return Ok(result);
    }

    [HttpGet("batches")]
    [Authorize(Policy = PermissionConstants.StockView)]
    public async Task<ActionResult<IEnumerable<MedicineBatchDto>>> GetBatches()
    {
        var batches = await _inventoryService.GetAllBatchesAsync();
        return Ok(batches);
    }

    [HttpGet("batches/{id}")]
    [Authorize(Policy = PermissionConstants.StockView)]
    public async Task<ActionResult<MedicineBatchDto>> GetBatch(int id)
    {
        var batch = await _inventoryService.GetBatchByIdAsync(id);
        if (batch == null) return NotFound();
        return Ok(batch);
    }

    [HttpGet("check-batch")]
    [Authorize(Policy = PermissionConstants.StockView)]
    public async Task<ActionResult<MedicineBatchDto>> CheckBatch([FromQuery] int medicineId, [FromQuery] string batchNumber)
    {
        var batch = await _inventoryService.GetBatchByNumberAsync(medicineId, batchNumber);
        if (batch == null) return NotFound();
        return Ok(batch);
    }

    [HttpPost("batches")]
    [Authorize(Policy = PermissionConstants.StockCreate)]
    public async Task<ActionResult<MedicineBatchDto>> CreateBatch(CreateMedicineBatchDto createBatchDto)
    {
        try
        {
            var batch = await _inventoryService.CreateBatchAsync(createBatchDto);
            _logger.LogInformation("Created new batch {BatchId} for medicine {MedicineId}", batch.Id, createBatchDto.MedicineId);
            return CreatedAtAction(nameof(GetBatch), new { id = batch.Id }, batch);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create batch for medicine {MedicineId}: {Message}", createBatchDto.MedicineId, ex.Message);
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("batches/{id}")]
    [Authorize(Policy = PermissionConstants.StockEdit)]
    public async Task<IActionResult> UpdateBatch(int id, UpdateMedicineBatchDto updateBatchDto)
    {
        if (id != updateBatchDto.Id) return BadRequest();
        try
        {
            await _inventoryService.UpdateBatchAsync(updateBatchDto);
            _logger.LogInformation("Updated batch {BatchId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch {BatchId}", id);
            return NotFound();
        }
    }

    [HttpPatch("batches/{id}/quarantine")]
    [Authorize(Policy = PermissionConstants.StockQuarantine)]
    public async Task<IActionResult> QuarantineBatch(int id, bool quarantine)
    {
        try
        {
            await _inventoryService.SetBatchQuarantineAsync(id, quarantine);
            _logger.LogInformation("Set quarantine status for batch {BatchId} to {Quarantine}", id, quarantine);
            return Ok(new { message = $"Batch {(quarantine ? "quarantined" : "activated")} successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to change quarantine status for batch {BatchId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing quarantine status for batch {BatchId}", id);
            return StatusCode(500, new { message = "An error occurred while updating batch status." });
        }
    }

    [HttpPost("dispense/preview")]
    [Authorize(Policy = PermissionConstants.StockDispense)]
    public async Task<ActionResult<DispensePreviewDto>> PreviewDispense(PreviewDispenseRequest request)
    {
        try
        {
            var preview = await _inventoryService.PreviewDispenseAsync(request.MedicineId, request.Quantity);
            return Ok(preview);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while previewing dispense." });
        }
    }

    [HttpPost("dispense")]
    [Authorize(Policy = PermissionConstants.StockDispense)]
    public async Task<ActionResult<DispenseResultDto>> DispenseStock(DispenseStockDto dispenseDto)
    {
        try
        {
            var result = await _inventoryService.DispenseStockAsync(dispenseDto);
            _logger.LogInformation("Dispensed {Quantity} units of medicine {MedicineId}.",
                dispenseDto.Quantity, dispenseDto.MedicineId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to dispense stock for medicine {MedicineId}", dispenseDto.MedicineId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispensing stock for medicine {MedicineId}", dispenseDto.MedicineId);
            return StatusCode(500, new { message = "An error occurred while dispensing stock." });
        }
    }
    [HttpPost("adjust")]
    [Authorize(Policy = PermissionConstants.StockAdjust)]
    public async Task<IActionResult> AdjustStock(AdjustStockDto adjustDto)
    {
        _logger.LogInformation("AdjustStock called for Batch {BatchId}. NewQty: {NewQuantity}, Reason: {Reason}",
            adjustDto.BatchId, adjustDto.NewQuantity, adjustDto.Reason);

        try
        {
            await _inventoryService.AdjustStockAsync(adjustDto);
            _logger.LogInformation("Stock adjusted successfully for Batch {BatchId}", adjustDto.BatchId);
            return Ok(new { message = "Stock adjusted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to adjust stock for Batch {BatchId}", adjustDto.BatchId);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for Batch {BatchId}", adjustDto.BatchId);
            return StatusCode(500, new { message = "An error occurred while adjusting stock." });
        }
    }

    [HttpGet("movements")]
    [Authorize(Policy = PermissionConstants.StockMovementsView)]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetMovements(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? medicineId,
        [FromQuery] string? movementType)
    {
        var movements = await _inventoryService.GetStockMovementsAsync(fromDate, toDate, medicineId, movementType);
        return Ok(movements);
    }

    [HttpGet("expiry-management")]
    [Authorize(Policy = PermissionConstants.StockExpiryView)]
    public async Task<ActionResult<IEnumerable<ExpiryManagementDto>>> GetExpiryManagement([FromQuery] string? status)
    {
        var batches = await _inventoryService.GetBatchesByExpiryStatusAsync(status);
        return Ok(batches);
    }

    [HttpGet("alternatives/{medicineId}")]
    [Authorize]
    public async Task<ActionResult<List<AlternativeMedicineDto>>> GetAlternatives(int medicineId)
    {
        try
        {
            var alternatives = await _inventoryService.GetAlternativeMedicinesAsync(medicineId);
            return Ok(alternatives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching alternatives for medicine {MedicineId}", medicineId);
            return StatusCode(500, new { message = "An error occurred while fetching alternatives." });
        }
    }
}
