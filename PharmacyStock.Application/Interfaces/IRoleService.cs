using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetRolesAsync();
    Task<RoleDto> GetRoleByIdAsync(int id);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateRoleAsync(int id, CreateRoleDto dto);
    Task DeleteRoleAsync(int id);
    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<List<PermissionDto>> GetPermissionsByRoleAsync(int roleId);
    Task UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsDto dto);
}
