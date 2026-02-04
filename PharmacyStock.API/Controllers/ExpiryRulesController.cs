using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpiryRulesController : ControllerBase
{
    private readonly IExpiryRuleService _expiryRuleService;

    public ExpiryRulesController(IExpiryRuleService expiryRuleService)
    {
        _expiryRuleService = expiryRuleService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.ExpiryRulesView)]
    public async Task<ActionResult<List<ExpiryRuleDto>>> GetExpiryRules()
    {
        var rules = await _expiryRuleService.GetExpiryRulesAsync();
        return Ok(rules);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionConstants.ExpiryRulesView)]
    public async Task<ActionResult<ExpiryRuleDto>> GetExpiryRuleById(int id)
    {
        var rule = await _expiryRuleService.GetExpiryRuleByIdAsync(id);
        return Ok(rule);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.ExpiryRulesCreate)]
    public async Task<ActionResult<ExpiryRuleDto>> CreateExpiryRule(CreateExpiryRuleDto dto)
    {
        var rule = await _expiryRuleService.CreateExpiryRuleAsync(dto);
        return Ok(rule);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionConstants.ExpiryRulesEdit)]
    public async Task<ActionResult<ExpiryRuleDto>> UpdateExpiryRule(int id, CreateExpiryRuleDto dto)
    {
        var rule = await _expiryRuleService.UpdateExpiryRuleAsync(id, dto);
        return Ok(rule);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionConstants.ExpiryRulesDelete)]
    public async Task<IActionResult> DeleteExpiryRule(int id)
    {
        await _expiryRuleService.DeleteExpiryRuleAsync(id);
        return NoContent();
    }
}
