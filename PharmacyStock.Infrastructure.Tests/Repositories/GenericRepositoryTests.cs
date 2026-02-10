using PharmacyStock.Domain.Entities;
using PharmacyStock.Infrastructure.Persistence.Context;
using PharmacyStock.Infrastructure.Persistence.Repositories;

namespace PharmacyStock.Infrastructure.Tests.Repositories;

public class GenericRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly GenericRepository<Category> _repository;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        _context = new AppDbContext(options);
        _repository = new GenericRepository<Category>(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var category = new Category { Name = "Test Category" };

        // Act
        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        // Assert
        var addedCategory = await _context.Categories.FirstOrDefaultAsync();
        addedCategory.Should().NotBeNull();
        addedCategory!.Name.Should().Be("Test Category");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        _context.Categories.Add(new Category { Name = "Cat 1" });
        _context.Categories.Add(new Category { Name = "Cat 2" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingEntities()
    {
        // Arrange
        _context.Categories.Add(new Category { Name = "Match", IsActive = true });
        _context.Categories.Add(new Category { Name = "No Match", IsActive = false });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(c => c.IsActive);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Match");
    }

    [Fact]
    public async Task Delete_ShouldRemoveEntity()
    {
        // Arrange
        var category = new Category { Name = "To Delete" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        // Act
        _repository.Delete(category);
        await _context.SaveChangesAsync();

        // Assert
        var count = await _context.Categories.CountAsync();
        count.Should().Be(0);
    }
}
