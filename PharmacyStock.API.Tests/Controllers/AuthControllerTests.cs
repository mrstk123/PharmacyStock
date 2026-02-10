using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;

namespace PharmacyStock.API.Tests.Controllers;

[Collection("IntegrationTests")]
public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IAuthService> _mockAuthService;

    public AuthControllerTests(WebApplicationFactory<Program> factory)
    {
        _mockAuthService = new Mock<IAuthService>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace the real IAuthService with our mock
                services.RemoveAll<IAuthService>(); // Remove existing
                services.AddScoped<IAuthService>(_ => _mockAuthService.Object); // Add mock

                // Remove the real DbContext registration (Generic and Postgres/SQL variants)
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<PharmacyStock.Infrastructure.Persistence.Context.AppDbContext>) ||
                    d.ServiceType == typeof(PharmacyStock.Infrastructure.Persistence.Context.AppDbContext) ||
                    d.ServiceType.Name == "AppDbContextPostgres").ToList();

                foreach (var d in descriptors)
                {
                    services.Remove(d);
                }

                // Add InMemory DbContext
                services.AddDbContext<PharmacyStock.Infrastructure.Persistence.Context.AppDbContext>(options =>
                {
                    options.ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginDto = new LoginRequestDto { Username = "admin", Password = "password" };
        var expectedResponse = new LoginResponseDto
        {
            AccessToken = "fake-jwt-token",
            RefreshToken = "fake-refresh-token",
            Username = "admin",
            Role = "Admin",
            IsPersistent = true,
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(7)
        };

        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().Be("fake-jwt-token");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginDto = new LoginRequestDto { Username = "admin", Password = "wrong-password" };

        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
