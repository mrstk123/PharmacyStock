using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Mappings;
using PharmacyStock.Application.Services;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly IMapper _mapper;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCacheService = new Mock<ICacheService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _categoryService = new CategoryService(
            _mockUnitOfWork.Object,
            _mockCacheService.Object,
            _mockCurrentUserService.Object,
            _mapper
        );
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnFromCache_WhenCacheExists()
    {
        // Arrange
        var cachedCategories = new List<CategoryDto>
        {
            new() { Id = 1, Name = "Cached Category" }
        };

        _mockCacheService.Setup(x => x.GetAsync<IEnumerable<CategoryDto>>(It.IsAny<string>()))
            .ReturnsAsync(cachedCategories);

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Should().BeEquivalentTo(cachedCategories);
        _mockUnitOfWork.Verify(x => x.Categories.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnFromDbAndSetCache_WhenCacheMiss()
    {
        // Arrange
        var dbCategories = new List<Category>
        {
            new() { Id = 1, Name = "Db Category" }
        };

        _mockCacheService.Setup(x => x.GetAsync<IEnumerable<CategoryDto>>(It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<CategoryDto>)null);

        _mockUnitOfWork.Setup(x => x.Categories.GetAllAsync())
            .ReturnsAsync(dbCategories);

        // Act
        var result = await _categoryService.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Db Category");

        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<TimeSpan?>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test Category" };

        _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _categoryService.GetCategoryByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldAddCategory_AndInvalidateCache()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "New Category", Description = "Desc" };
        var username = "testuser";

        _mockCurrentUserService.Setup(x => x.GetCurrentUsername())
            .Returns(username);

        // Mock AddAsync to avoid NullReferenceException
        _mockUnitOfWork.Setup(x => x.Categories.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _categoryService.CreateCategoryAsync(createDto);

        // Assert
        result.Name.Should().Be("New Category");

        _mockUnitOfWork.Verify(x => x.Categories.AddAsync(It.Is<Category>(c =>
            c.Name == "New Category" &&
            c.CreatedBy == username
        )), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);

        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateCategory_AndInvalidateCache()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Old Name", Description = "Old Desc", IsActive = true };
        var updateDto = new UpdateCategoryDto
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Desc",
            IsActive = false
        };

        _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        await _categoryService.UpdateCategoryAsync(updateDto);

        // Assert
        category.Name.Should().Be("Updated Name");
        category.Description.Should().Be("Updated Desc");
        category.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Categories.Update(category), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _categoryService.UpdateCategoryAsync(
            new UpdateCategoryDto { Id = 99, Name = "Test" });

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Category not found");
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldSoftDelete_AndInvalidateCache()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Test", IsActive = true };

        _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        await _categoryService.DeleteCategoryAsync(1);

        // Assert
        category.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Categories.Update(category), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Categories.GetByIdAsync(99))
            .ReturnsAsync((Category?)null);

        // Act
        var act = async () => await _categoryService.DeleteCategoryAsync(99);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Category not found");
    }
}
