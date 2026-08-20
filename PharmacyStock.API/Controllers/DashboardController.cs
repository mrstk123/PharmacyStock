using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("action-items")]
    [Authorize(Policy = PermissionConstants.DashboardView)]
    public async Task<ActionResult<DashboardActionItemsDto>> GetActionItems()
    {
        var result = await _dashboardService.GetActionItemsAsync();
        return Ok(result);
    }

    [HttpGet("valuation")]
    [Authorize(Policy = PermissionConstants.DashboardView)]
    public async Task<ActionResult<InventoryValuationDto>> GetValuation()
    {
        var result = await _dashboardService.GetValuationAsync();
        return Ok(result);
    }

    [HttpGet("stats")]
    [Authorize(Policy = PermissionConstants.DashboardView)]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var result = await _dashboardService.GetStatsAsync();
        return Ok(result);
    }

    [HttpGet("low-stock")]
    [Authorize(Policy = PermissionConstants.DashboardView)]
    public async Task<ActionResult<List<LowStockIssueDto>>> GetLowStock([FromQuery] int threshold = 50)
    {
        var result = await _dashboardService.GetLowStockIssuesAsync(threshold);
        return Ok(result);
    }

    [HttpGet("recent-movements")]
    [Authorize(Policy = PermissionConstants.DashboardView)]
    public async Task<ActionResult<List<RecentMovementDto>>> GetRecentMovements([FromQuery] int count = 15)
    {
        var result = await _dashboardService.GetRecentMovementsAsync(count);
        return Ok(result);
    }
}
