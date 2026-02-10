using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Mappings;
using PharmacyStock.Application.Services;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;
using System.Linq.Expressions;

namespace PharmacyStock.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly IMapper _mapper;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEmailService = new Mock<IEmailService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _userService = new UserService(
            _mockUnitOfWork.Object,
            _mockEmailService.Object,
            _mapper
        );
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser_AndSendEmail()
    {
        // Arrange
        var createDto = new CreateUserDto
        {
            Username = "newuser",
            Email = "new@example.com",
            FullName = "New User",
            RoleId = 2
        };

        // Mock FindAsync (check existence) -> returns empty list
        _mockUnitOfWork.Setup(x => x.Users.FindAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Expression<Func<User, object>>[]>()
        )).ReturnsAsync(new List<User>());

        // Mock AddAsync logic to set ID
        _mockUnitOfWork.Setup(x => x.Users.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => u.Id = 50)
            .Returns(Task.CompletedTask);

        // Mock GetById (return created user)
        _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(50, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(new User { Id = 50, Username = "newuser", Role = new Role { Name = "User" } });

        // Act
        var result = await _userService.CreateUserAsync(createDto);

        // Assert
        result.Username.Should().Be("newuser");

        // Verify user was added with a password hash
        _mockUnitOfWork.Verify(x => x.Users.AddAsync(It.Is<User>(u =>
            u.Username == "newuser" &&
            !string.IsNullOrEmpty(u.PasswordHash) // Hash should be generated
        )), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);

        // Verify email sent
        _mockEmailService.Verify(x => x.SendEmailAsync(
            "new@example.com",
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrow_WhenUsernameExists()
    {
        // Arrange
        var createDto = new CreateUserDto { Username = "existing" };

        // Mock FindAsync -> returns existing user (using correct overload)
        _mockUnitOfWork.Setup(x => x.Users.FindAsync(
            It.IsAny<Expression<Func<User, bool>>>()
        )).ReturnsAsync(new List<User> { new() { Username = "existing" } });

        // Act
        var act = async () => await _userService.CreateUserAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already taken*");

        _mockEmailService.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateFields()
    {
        // Arrange
        var user = new User { Id = 1, Username = "admin", RoleId = 1 };
        var updateDto = new UpdateUserDto { FullName = "Updated Name" };

        _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1)).ReturnsAsync(user);

        // Mock GetById for return
        _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1, It.IsAny<Expression<Func<User, object>>[]>()))
            .ReturnsAsync(user);

        // Act
        await _userService.UpdateUserAsync(1, updateDto);

        // Assert
        user.FullName.Should().Be("Updated Name");
        _mockUnitOfWork.Verify(x => x.Users.Update(user), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Role = new Role { Name = "Admin" } },
            new() { Id = 2, Username = "user2", Role = new Role { Name = "User" } }
        };

        _mockUnitOfWork.Setup(x => x.Users.FindAsync(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<Expression<Func<User, object>>[]>()
        )).ReturnsAsync(users);

        // Act
        var result = await _userService.GetUsersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Username.Should().Be("user1");
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldSoftDelete()
    {
        // Arrange
        var user = new User { Id = 1, Username = "test", IsActive = true };

        _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        await _userService.DeleteUserAsync(1);

        // Assert
        user.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Users.Update(user), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(99))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await _userService.DeleteUserAsync(99);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldUpdatePassword_AndSendEmail()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "test",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };

        var changeDto = new ChangePasswordDto
        {
            CurrentPassword = "oldpassword",
            NewPassword = "newpassword"
        };

        _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        await _userService.ChangePasswordAsync(1, changeDto);

        // Assert
        BCrypt.Net.BCrypt.Verify("newpassword", user.PasswordHash).Should().BeTrue();

        _mockUnitOfWork.Verify(x => x.Users.Update(user), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockEmailService.Verify(x => x.SendEmailAsync(
            "test@example.com",
            "Password Changed",
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrow_WhenCurrentPasswordWrong()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        var changeDto = new ChangePasswordDto
        {
            CurrentPassword = "wrongpassword",
            NewPassword = "newpassword"
        };

        _mockUnitOfWork.Setup(x => x.Users.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var act = async () => await _userService.ChangePasswordAsync(1, changeDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Current password is incorrect.");

        _mockEmailService.Verify(x => x.SendEmailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ), Times.Never);
    }
}
