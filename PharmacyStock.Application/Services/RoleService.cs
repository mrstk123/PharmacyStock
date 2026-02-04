using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllAsync();
        return _mapper.Map<List<RoleDto>>(roles);
    }

    public async Task<RoleDto> GetRoleByIdAsync(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
            throw new Exception("Role not found");

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive ?? true
        };

        await _unitOfWork.Roles.AddAsync(role);
        await _unitOfWork.SaveAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(int id, CreateRoleDto dto)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null)
            throw new Exception("Role not found");

        role.Name = dto.Name;
        role.Description = dto.Description;
        if (dto.IsActive.HasValue)
            role.IsActive = dto.IsActive.Value;

        _unitOfWork.Roles.Update(role);
        await _unitOfWork.SaveAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteRoleAsync(int id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id)
            ?? throw new Exception("Role not found");
        _unitOfWork.Roles.Delete(role);
        await _unitOfWork.SaveAsync();
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        return _mapper.Map<List<PermissionDto>>(permissions);
    }

    public async Task<List<PermissionDto>> GetPermissionsByRoleAsync(int roleId)
    {
        var rolePermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == roleId, rp => rp.Permission);
        var permissions = rolePermissions.Select(x => x.Permission).ToList();
        return _mapper.Map<List<PermissionDto>>(permissions);
    }

    public async Task UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsDto dto)
    {
        // 1. Remove existing permissions for this role
        var currentPermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == roleId);

        foreach (var rp in currentPermissions)
        {
            _unitOfWork.RolePermissions.Delete(rp);
        }

        // 2. Add new permissions
        if (dto.PermissionIds != null && dto.PermissionIds.Count > 0)
        {
            var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
            var validPermissionIds = allPermissions.Select(p => p.Id).ToHashSet();

            foreach (var permissionId in dto.PermissionIds)
            {
                // Verify permission exists
                if (validPermissionIds.Contains(permissionId))
                {
                    await _unitOfWork.RolePermissions.AddAsync(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    });
                }
            }
        }

        await _unitOfWork.SaveAsync();
    }
}
