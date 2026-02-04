using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/stockmovements")]
public class StockMovementsController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public StockMovementsController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("movements")]
    [Authorize(Policy = PermissionConstants.StockMovementsView)]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetMovements(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? movementType)
    {
        var result = await _inventoryService.GetStockMovementsAsync(startDate, endDate, null, movementType);
        return Ok(result);
    }
}
