using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Policy = PermissionConstants.RolesView)]
    public async Task<ActionResult<List<RoleDto>>> GetRoles()
    {
        var roles = await _roleService.GetRolesAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = PermissionConstants.RolesView)]
    public async Task<ActionResult<RoleDto>> GetRoleById(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        return Ok(role);
    }

    [HttpPost]
    [Authorize(Policy = PermissionConstants.RolesCreate)]
    public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto dto)
    {
        var role = await _roleService.CreateRoleAsync(dto);
        return Ok(role);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = PermissionConstants.RolesEdit)]
    public async Task<ActionResult<RoleDto>> UpdateRole(int id, CreateRoleDto dto)
    {
        var role = await _roleService.UpdateRoleAsync(id, dto);
        return Ok(role);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = PermissionConstants.RolesDelete)]
    public async Task<IActionResult> DeleteRole(int id)
    {
        await _roleService.DeleteRoleAsync(id);
        return NoContent();
    }

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionConstants.PermissionsView)]
    public async Task<ActionResult<List<PermissionDto>>> GetAllPermissions()
    {
        var permissions = await _roleService.GetAllPermissionsAsync();
        return Ok(permissions);
    }

    [HttpGet("{roleId}/permissions")]
    [Authorize(Policy = PermissionConstants.RolesView)]
    public async Task<ActionResult<List<PermissionDto>>> GetPermissionsByRole(int roleId)
    {
        var permissions = await _roleService.GetPermissionsByRoleAsync(roleId);
        return Ok(permissions);
    }

    [HttpPut("{roleId}/permissions")]
    [Authorize(Policy = PermissionConstants.PermissionsAssign)]
    public async Task<IActionResult> UpdateRolePermissions(int roleId, UpdateRolePermissionsDto dto)
    {
        await _roleService.UpdateRolePermissionsAsync(roleId, dto);
        return NoContent();
    }
}
