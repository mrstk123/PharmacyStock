using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Mappings;
using PharmacyStock.Application.Services;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Tests.Services;

public class SupplierServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly IMapper _mapper;
    private readonly SupplierService _supplierService;

    public SupplierServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCache = new Mock<ICacheService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _supplierService = new SupplierService(
            _mockUnitOfWork.Object,
            _mockCache.Object,
            _mockCurrentUserService.Object,
            _mapper
        );
    }

    [Fact]
    public async Task GetAllSuppliersAsync_ShouldReturnFiltered_WhenCached()
    {
        // Arrange
        var cachedSuppliers = new List<SupplierDto>
        {
            new() { Id = 1, Name = "Active", IsActive = true },
            new() { Id = 2, Name = "Inactive", IsActive = false }
        };

        _mockCache.Setup(x => x.GetAsync<List<SupplierDto>>(It.IsAny<string>()))
            .ReturnsAsync(cachedSuppliers);

        // Act
        var resultActive = await _supplierService.GetAllSuppliersAsync(isActive: true);
        var resultAll = await _supplierService.GetAllSuppliersAsync(isActive: null);

        // Assert
        resultActive.Should().HaveCount(1);
        resultActive.First().Name.Should().Be("Active");

        resultAll.Should().HaveCount(2);
        _mockUnitOfWork.Verify(x => x.Suppliers.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateSupplierAsync_ShouldAddSupplier_AndInvalidateCache()
    {
        // Arrange
        var createDto = new CreateSupplierDto
        {
            Name = "New Supplier",
            SupplierCode = "SUP001",
            ContactInfo = "123"
        };

        _mockCurrentUserService.Setup(x => x.GetCurrentUsername()).Returns("admin");

        // Mock AddAsync
        _mockUnitOfWork.Setup(x => x.Suppliers.AddAsync(It.IsAny<Supplier>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _supplierService.CreateSupplierAsync(createDto);

        // Assert
        result.Name.Should().Be("New Supplier");

        _mockUnitOfWork.Verify(x => x.Suppliers.AddAsync(It.Is<Supplier>(s =>
            s.Name == "New Supplier" && s.CreatedBy == "admin"
        )), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSupplierAsync_ShouldUpdate_WhenFound()
    {
        // Arrange
        var supplier = new Supplier { Id = 1, Name = "Old Name" };
        var updateDto = new UpdateSupplierDto { Id = 1, Name = "New Name" };

        _mockUnitOfWork.Setup(x => x.Suppliers.GetByIdAsync(1)).ReturnsAsync(supplier);

        // Act
        await _supplierService.UpdateSupplierAsync(updateDto);

        // Assert
        supplier.Name.Should().Be("New Name");
        _mockUnitOfWork.Verify(x => x.Suppliers.Update(supplier), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSupplierAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Suppliers.GetByIdAsync(99)).ReturnsAsync((Supplier?)null);

        // Act
        var act = async () => await _supplierService.UpdateSupplierAsync(new UpdateSupplierDto { Id = 99 });

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Supplier not found");
    }

    [Fact]
    public async Task DeleteSupplierAsync_ShouldSoftDelete_AndInvalidateCache()
    {
        // Arrange
        var supplier = new Supplier { Id = 1, Name = "Test", IsActive = true };

        _mockUnitOfWork.Setup(x => x.Suppliers.GetByIdAsync(1))
            .ReturnsAsync(supplier);

        // Act
        await _supplierService.DeleteSupplierAsync(1);

        // Assert
        supplier.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Suppliers.Update(supplier), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSupplierAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Suppliers.GetByIdAsync(99))
            .ReturnsAsync((Supplier?)null);

        // Act
        var act = async () => await _supplierService.DeleteSupplierAsync(99);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Supplier not found");
    }
}
