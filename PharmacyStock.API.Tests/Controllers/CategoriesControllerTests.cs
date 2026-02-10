using System.Net;
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
public class CategoriesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ICategoryService> _mockCategoryService;

    public CategoriesControllerTests(WebApplicationFactory<Program> factory)
    {
        _mockCategoryService = new Mock<ICategoryService>();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Bypass Auth
                services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();

                // Mock Service
                services.RemoveAll<ICategoryService>();
                services.AddScoped<ICategoryService>(_ => _mockCategoryService.Object);

                // Use InMemory Db (standard setup to avoid real db connection errors)
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
                    options.UseInMemoryDatabase("InMemoryDbForCategoriesTesting");
                });
            });
        });
    }

    [Fact]
    public async Task GetCategories_ShouldReturnOk_WithList()
    {
        // Arrange
        var client = _factory.CreateClient();
        var categories = new List<CategoryDto>
        {
            new() { Id = 1, Name = "Cat 1" },
            new() { Id = 2, Name = "Cat 2" }
        };

        _mockCategoryService.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var response = await client.GetAsync("/api/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        content.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCategory_ShouldReturnOk_WhenExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var category = new CategoryDto { Id = 1, Name = "Cat 1" };

        _mockCategoryService.Setup(x => x.GetCategoryByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var response = await client.GetAsync("/api/categories/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<CategoryDto>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("Cat 1");
    }

    [Fact]
    public async Task GetCategory_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        _mockCategoryService.Setup(x => x.GetCategoryByIdAsync(99))
            .ReturnsAsync((CategoryDto?)null);

        // Act
        var response = await client.GetAsync("/api/categories/99");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateCategoryDto { Name = "New Cat" };
        var created = new CategoryDto { Id = 10, Name = "New Cat" };

        _mockCategoryService.Setup(x => x.CreateCategoryAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(created);

        // Act
        var response = await client.PostAsJsonAsync("/api/categories", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<CategoryDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(10);
    }
}
