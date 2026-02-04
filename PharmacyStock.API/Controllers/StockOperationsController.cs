using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockOperationsController : ControllerBase
{
    private readonly IStockOperationService _stockOperationService;
    private readonly ILogger<StockOperationsController> _logger;

    public StockOperationsController(IStockOperationService stockOperationService, ILogger<StockOperationsController> logger)
    {
        _stockOperationService = stockOperationService;
        _logger = logger;
    }

    [HttpPost("remove-expired")]
    [Authorize(Policy = PermissionConstants.StockDispose)]
    public async Task<IActionResult> RemoveExpiredStock(RemoveExpiredStockDto removeDto)
    {
        try
        {
            await _stockOperationService.RemoveExpiredStockAsync(removeDto);
            _logger.LogInformation("Expired stock removed successfully. BatchId: {BatchId}, Quantity: {Quantity}", removeDto.BatchId, removeDto.Quantity);
            return Ok(new { message = "Expired stock removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing expired stock for BatchId {BatchId}", removeDto.BatchId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("return-to-supplier")]
    [Authorize(Policy = PermissionConstants.StockReturn)]
    public async Task<IActionResult> ReturnToSupplier(ReturnToSupplierDto returnDto)
    {
        try
        {
            await _stockOperationService.ReturnToSupplierAsync(returnDto);
            _logger.LogInformation("Stock returned to supplier successfully. BatchId: {BatchId}", returnDto.BatchId);
            return Ok(new { message = "Stock returned to supplier successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning stock to supplier for BatchId {BatchId}", returnDto.BatchId);
            return BadRequest(new { message = ex.Message });
        }
    }
}
