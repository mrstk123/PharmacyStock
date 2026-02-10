using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Mappings;
using PharmacyStock.Application.Services;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;
using System.Linq.Expressions;

namespace PharmacyStock.Application.Tests.Services;

public class RoleServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly IMapper _mapper;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _roleService = new RoleService(
            _mockUnitOfWork.Object,
            _mapper
        );
    }

    [Fact]
    public async Task CreateRoleAsync_ShouldAddRole()
    {
        // Arrange
        var createDto = new CreateRoleDto { Name = "Admin", Description = "System Admin" };

        // Act
        // Mock AddAsync
        _mockUnitOfWork.Setup(x => x.Roles.AddAsync(It.IsAny<Role>()))
            .Returns(Task.CompletedTask);

        var result = await _roleService.CreateRoleAsync(createDto);

        // Assert
        result.Name.Should().Be("Admin");

        _mockUnitOfWork.Verify(x => x.Roles.AddAsync(It.Is<Role>(r =>
            r.Name == "Admin"
        )), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateRolePermissionsAsync_ShouldReplacePermissions()
    {
        // Arrange
        int roleId = 1;
        var existingPermissions = new List<RolePermission>
        {
            new() { RoleId = 1, PermissionId = 10 }
        };

        var allPermissions = new List<Permission>
        {
            new() { Id = 20, Name = "NewPerm" },
            new() { Id = 30, Name = "AnotherPerm" }
        };

        var updateDto = new UpdateRolePermissionsDto
        {
            PermissionIds = new List<int> { 20 } // Removing 10, Adding 20, Ignoring 30
        };

        // Mock getting existing role permissions
        _mockUnitOfWork.Setup(x => x.RolePermissions.FindAsync(
            It.IsAny<Expression<Func<RolePermission, bool>>>()
        )).ReturnsAsync(existingPermissions);

        // Mock getting all permissions (for validation)
        _mockUnitOfWork.Setup(x => x.Permissions.GetAllAsync())
            .ReturnsAsync(allPermissions);

        // Act
        await _roleService.UpdateRolePermissionsAsync(roleId, updateDto);

        // Assert
        // Should delete existing
        _mockUnitOfWork.Verify(x => x.RolePermissions.Delete(It.Is<RolePermission>(rp => rp.PermissionId == 10)), Times.Once);

        // Should add new
        _mockUnitOfWork.Verify(x => x.RolePermissions.AddAsync(It.Is<RolePermission>(rp => rp.PermissionId == 20 && rp.RoleId == 1)), Times.Once);

        // Should NOT add invalid/unrequested
        _mockUnitOfWork.Verify(x => x.RolePermissions.AddAsync(It.Is<RolePermission>(rp => rp.PermissionId == 30)), Times.Never);

        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnAllRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new() { Id = 1, Name = "Admin" },
            new() { Id = 2, Name = "User" }
        };

        _mockUnitOfWork.Setup(x => x.Roles.GetAllAsync())
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.GetRolesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("Admin");
    }

    [Fact]
    public async Task GetRoleByIdAsync_ShouldReturnRole_WhenExists()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Admin" };

        _mockUnitOfWork.Setup(x => x.Roles.GetByIdAsync(1))
            .ReturnsAsync(role);

        // Act
        var result = await _roleService.GetRoleByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldUpdateRole()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Old Name", Description = "Old Desc", IsActive = true };
        var updateDto = new CreateRoleDto
        {
            Name = "New Name",
            Description = "New Desc",
            IsActive = false
        };

        _mockUnitOfWork.Setup(x => x.Roles.GetByIdAsync(1))
            .ReturnsAsync(role);

        // Act
        await _roleService.UpdateRoleAsync(1, updateDto);

        // Assert
        role.Name.Should().Be("New Name");
        role.Description.Should().Be("New Desc");
        role.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Roles.Update(role), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Roles.GetByIdAsync(99))
            .ReturnsAsync((Role?)null);

        // Act
        var act = async () => await _roleService.UpdateRoleAsync(99, new CreateRoleDto { Name = "Test" });

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Role not found");
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldDeleteRole()
    {
        // Arrange
        var role = new Role { Id = 1, Name = "Test" };

        _mockUnitOfWork.Setup(x => x.Roles.GetByIdAsync(1))
            .ReturnsAsync(role);

        // Act
        await _roleService.DeleteRoleAsync(1);

        // Assert
        _mockUnitOfWork.Verify(x => x.Roles.Delete(role), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Roles.GetByIdAsync(99))
            .ReturnsAsync((Role?)null);

        // Act
        var act = async () => await _roleService.DeleteRoleAsync(99);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Role not found");
    }
}
