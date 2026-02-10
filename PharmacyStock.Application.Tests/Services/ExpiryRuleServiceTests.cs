using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Mappings;
using PharmacyStock.Application.Services;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;
using System.Linq.Expressions;

namespace PharmacyStock.Application.Tests.Services;

public class ExpiryRuleServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly IMapper _mapper;
    private readonly ExpiryRuleService _expiryRuleService;

    public ExpiryRuleServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _expiryRuleService = new ExpiryRuleService(
            _mockUnitOfWork.Object,
            _mapper
        );
    }

    [Fact]
    public async Task CreateExpiryRuleAsync_ShouldCreate_WhenNoDuplicateExists()
    {
        // Arrange
        var createDto = new CreateExpiryRuleDto
        {
            CategoryId = 1,
            WarningDays = 30,
            CriticalDays = 10,
            IsActive = true
        };

        // Mock FindAsync (check duplicates) -> returns empty
        _mockUnitOfWork.Setup(x => x.ExpiryRules.FindAsync(
            It.IsAny<Expression<Func<ExpiryRule, bool>>>(),
            It.IsAny<Expression<Func<ExpiryRule, object>>[]>()
        )).ReturnsAsync(new List<ExpiryRule>());

        // Mock Add + GetById
        _mockUnitOfWork.Setup(x => x.ExpiryRules.AddAsync(It.IsAny<ExpiryRule>()))
            .Callback<ExpiryRule>(r => r.Id = 100)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.ExpiryRules.GetByIdAsync(100, It.IsAny<Expression<Func<ExpiryRule, object>>[]>()))
            .ReturnsAsync(new ExpiryRule { Id = 100, CategoryId = 1 });

        // Act
        var result = await _expiryRuleService.CreateExpiryRuleAsync(createDto);

        // Assert
        result.Id.Should().Be(100);
        _mockUnitOfWork.Verify(x => x.ExpiryRules.AddAsync(It.IsAny<ExpiryRule>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateExpiryRuleAsync_ShouldThrow_WhenDuplicateExists()
    {
        // Arrange
        var createDto = new CreateExpiryRuleDto { CategoryId = 1, IsActive = true };

        // Mock FindAsync -> returns existing rule
        _mockUnitOfWork.Setup(x => x.ExpiryRules.FindAsync(
            It.Is<Expression<Func<ExpiryRule, bool>>>(expr => true)
        )).ReturnsAsync(new List<ExpiryRule> { new() { Id = 5, CategoryId = 1, IsActive = true } });

        // Act
        var act = async () => await _expiryRuleService.CreateExpiryRuleAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*active*rule*already exists*");
    }

    [Fact]
    public async Task DeleteExpiryRuleAsync_ShouldSoftDelete()
    {
        // Arrange
        var rule = new ExpiryRule { Id = 1, IsActive = true };
        _mockUnitOfWork.Setup(x => x.ExpiryRules.GetByIdAsync(1)).ReturnsAsync(rule);

        // Act
        await _expiryRuleService.DeleteExpiryRuleAsync(1);

        // Assert
        rule.IsActive.Should().BeFalse();
        _mockUnitOfWork.Verify(x => x.ExpiryRules.Update(rule), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetExpiryRulesAsync_ShouldReturnAllRules()
    {
        // Arrange
        var rules = new List<ExpiryRule>
        {
            new() { Id = 1, CategoryId = 1, WarningDays = 30, CriticalDays = 10 },
            new() { Id = 2, CategoryId = null, WarningDays = 60, CriticalDays = 20 }
        };

        _mockUnitOfWork.Setup(x => x.ExpiryRules.FindAsync(
            It.IsAny<Expression<Func<ExpiryRule, bool>>>(),
            It.IsAny<Expression<Func<ExpiryRule, object>>[]>()
        )).ReturnsAsync(rules);

        // Act
        var result = await _expiryRuleService.GetExpiryRulesAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().WarningDays.Should().Be(30);
    }

    [Fact]
    public async Task UpdateExpiryRuleAsync_ShouldUpdateRule()
    {
        // Arrange
        var rule = new ExpiryRule
        {
            Id = 1,
            CategoryId = 1,
            WarningDays = 30,
            CriticalDays = 10,
            IsActive = true
        };

        var updateDto = new CreateExpiryRuleDto
        {
            CategoryId = 2,
            WarningDays = 60,
            CriticalDays = 20,
            IsActive = false
        };

        // Mock GetByIdAsync for update
        _mockUnitOfWork.Setup(x => x.ExpiryRules.GetByIdAsync(1))
            .ReturnsAsync(rule);

        // Mock FindAsync for duplicate check (no duplicates)
        _mockUnitOfWork.Setup(x => x.ExpiryRules.FindAsync(
            It.IsAny<Expression<Func<ExpiryRule, bool>>>(),
            It.IsAny<Expression<Func<ExpiryRule, object>>[]>()
        )).ReturnsAsync(new List<ExpiryRule>());

        // Mock GetByIdAsync for return
        _mockUnitOfWork.Setup(x => x.ExpiryRules.GetByIdAsync(1, It.IsAny<Expression<Func<ExpiryRule, object>>[]>()))
            .ReturnsAsync(new ExpiryRule { Id = 1, CategoryId = 2, WarningDays = 60, CriticalDays = 20 });

        // Act
        var result = await _expiryRuleService.UpdateExpiryRuleAsync(1, updateDto);

        // Assert
        rule.CategoryId.Should().Be(2);
        rule.WarningDays.Should().Be(60);
        rule.CriticalDays.Should().Be(20);
        rule.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.ExpiryRules.Update(rule), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateExpiryRuleAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.ExpiryRules.GetByIdAsync(99))
            .ReturnsAsync((ExpiryRule?)null);

        // Act
        var act = async () => await _expiryRuleService.UpdateExpiryRuleAsync(99, new CreateExpiryRuleDto());

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Expiry Rule not found");
    }
}
