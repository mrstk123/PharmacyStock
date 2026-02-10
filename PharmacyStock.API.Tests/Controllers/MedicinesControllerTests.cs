using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using PharmacyStock.API.Tests.Utilities;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;

namespace PharmacyStock.API.Tests.Controllers;

[Collection("IntegrationTests")]
public class MedicinesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<IMedicineService> _mockMedicineService;

    public MedicinesControllerTests(WebApplicationFactory<Program> factory)
    {
        _mockMedicineService = new Mock<IMedicineService>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Bypass Auth
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                // Mock Service
                services.RemoveAll<IMedicineService>();
                services.AddScoped<IMedicineService>(_ => _mockMedicineService.Object);

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
                    options.UseInMemoryDatabase("InMemoryDbForMedicinesTesting");
                });
            });
        });
    }

    [Fact]
    public async Task GetMedicines_ShouldReturnOk_WithPaginatedResult()
    {
        // Arrange
        var client = _factory.CreateClient();
        var paginatedResult = new PaginatedResult<MedicineDto>(
            new List<MedicineDto>
            {
                new() { Id = 1, Name = "Med 1" },
                new() { Id = 2, Name = "Med 2" }
            },
            2, 1, 10
        );

        _mockMedicineService.Setup(x => x.GetPaginatedMedicinesAsync(1, 10, null, null, null))
            .ReturnsAsync(paginatedResult);

        // Act
        var response = await client.GetAsync("/api/medicines?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<PaginatedResult<MedicineDto>>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMedicine_ShouldReturnOk_WhenExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var medicine = new MedicineDto { Id = 1, Name = "Med 1" };

        _mockMedicineService.Setup(x => x.GetMedicineByIdAsync(1))
            .ReturnsAsync(medicine);

        // Act
        var response = await client.GetAsync("/api/medicines/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<MedicineDto>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("Med 1");
    }

    [Fact]
    public async Task CreateMedicine_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateMedicineDto
        {
            Name = "New Med",
            CategoryId = 1,
            MedicineCode = "MED001",
            UnitOfMeasure = "Box"
        };
        var created = new MedicineDto { Id = 10, Name = "New Med" };

        _mockMedicineService.Setup(x => x.CreateMedicineAsync(It.IsAny<CreateMedicineDto>()))
            .ReturnsAsync(created);

        // Act
        var response = await client.PostAsJsonAsync("/api/medicines", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Check Location header if strict, but mainly check DB/Service call
        var content = await response.Content.ReadFromJsonAsync<MedicineDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(10);
    }

    [Fact]
    public async Task DeleteMedicine_ShouldReturnNoContent()
    {
        // Arrange
        var client = _factory.CreateClient();
        _mockMedicineService.Setup(x => x.DeleteMedicineAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var response = await client.DeleteAsync("/api/medicines/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
