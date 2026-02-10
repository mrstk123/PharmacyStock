using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using PharmacyStock.API.Tests.Utilities;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;

namespace PharmacyStock.API.Tests.Controllers;

[Collection("IntegrationTests")]
public class UsersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IUserService> _mockUserService;

    public UsersControllerTests(WebApplicationFactory<Program> factory)
    {
        _mockUserService = new Mock<IUserService>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Bypass Auth
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                // Mock Service
                services.RemoveAll<IUserService>();
                services.AddScoped<IUserService>(_ => _mockUserService.Object);

                // Use InMemory Db
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<PharmacyStock.Infrastructure.Persistence.Context.AppDbContext>) ||
                    d.ServiceType == typeof(PharmacyStock.Infrastructure.Persistence.Context.AppDbContext) ||
                    d.ServiceType.Name == "AppDbContextPostgres").ToList();

                foreach (var d in descriptors)
                {
                    services.Remove(d);
                }

                services.AddDbContext<PharmacyStock.Infrastructure.Persistence.Context.AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForUsersTesting");
                });
            });
        });
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WithList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var users = new List<UserDto>
        {
            new() { Id = 1, Username = "user1" },
            new() { Id = 2, Username = "user2" }
        };

        _mockUserService.Setup(x => x.GetUsersAsync())
            .ReturnsAsync(users);

        // Act
        var response = await client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        content.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMe_ShouldReturnOk_WithCurrentUser()
    {
        // Arrange
        var client = _factory.CreateClient();
        var user = new UserDto { Id = 1, Username = "admin", RoleName = "Admin" };

        // FakePolicyEvaluator sets NameIdentifier to "1"
        _mockUserService.Setup(x => x.GetUserByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var response = await client.GetAsync("/api/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<UserDto>();
        content.Should().NotBeNull();
        content!.Username.Should().Be("admin");
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateUserDto { Username = "newuser", Email = "test@test.com", RoleId = 1 };
        var created = new UserDto { Id = 10, Username = "newuser" };

        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync(created);

        // Act
        var response = await client.PostAsJsonAsync("/api/users", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<UserDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(10);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        _mockUserService.Setup(x => x.DeleteUserAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var response = await client.DeleteAsync("/api/users/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
