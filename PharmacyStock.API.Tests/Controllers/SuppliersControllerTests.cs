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
public class SuppliersControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ISupplierService> _mockSupplierService;

    public SuppliersControllerTests(WebApplicationFactory<Program> factory)
    {
        _mockSupplierService = new Mock<ISupplierService>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Bypass Auth
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                // Mock Service
                services.RemoveAll<ISupplierService>();
                services.AddScoped<ISupplierService>(_ => _mockSupplierService.Object);

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
                    options.UseInMemoryDatabase("InMemoryDbForSuppliersTesting");
                });
            });
        });
    }

    [Fact]
    public async Task GetSuppliers_ShouldReturnOk_WithList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var suppliers = new List<SupplierDto>
        {
            new() { Id = 1, Name = "Supplier 1" },
            new() { Id = 2, Name = "Supplier 2" }
        };

        _mockSupplierService.Setup(x => x.GetAllSuppliersAsync(It.IsAny<bool?>()))
            .ReturnsAsync(suppliers);

        // Act
        var response = await client.GetAsync("/api/suppliers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
        content.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSupplier_ShouldReturnOk_WhenExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var supplier = new SupplierDto { Id = 1, Name = "Supplier 1" };

        _mockSupplierService.Setup(x => x.GetSupplierByIdAsync(1))
            .ReturnsAsync(supplier);

        // Act
        var response = await client.GetAsync("/api/suppliers/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SupplierDto>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("Supplier 1");
    }

    [Fact]
    public async Task CreateSupplier_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateSupplierDto { Name = "New Supplier", SupplierCode = "SUP001", ContactInfo = "John Doe, 123456" };
        var created = new SupplierDto { Id = 10, Name = "New Supplier" };

        _mockSupplierService.Setup(x => x.CreateSupplierAsync(It.IsAny<CreateSupplierDto>()))
            .ReturnsAsync(created);

        // Act
        var response = await client.PostAsJsonAsync("/api/suppliers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<SupplierDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(10);
    }

    [Fact]
    public async Task DeleteSupplier_ShouldReturnNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        _mockSupplierService.Setup(x => x.DeleteSupplierAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var response = await client.DeleteAsync("/api/suppliers/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
