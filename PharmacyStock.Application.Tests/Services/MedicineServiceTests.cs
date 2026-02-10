using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Mappings;
using PharmacyStock.Application.Services;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;
using System.Linq.Expressions;

namespace PharmacyStock.Application.Tests.Services;

public class MedicineServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<IDashboardService> _mockDashboardService;
    private readonly Mock<IDashboardBroadcaster> _mockBroadcaster;
    private readonly IMapper _mapper;
    private readonly MedicineService _medicineService;

    public MedicineServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCacheService = new Mock<ICacheService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockDashboardService = new Mock<IDashboardService>();
        _mockBroadcaster = new Mock<IDashboardBroadcaster>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _medicineService = new MedicineService(
            _mockUnitOfWork.Object,
            _mockCacheService.Object,
            _mockCurrentUserService.Object,
            _mapper,
            _mockDashboardService.Object,
            _mockBroadcaster.Object
        );
    }

    [Fact]
    public async Task GetAllMedicinesAsync_ShouldReturnAllMedicines()
    {
        // Arrange
        var medicines = new List<Medicine>
        {
            new() { Id = 1, Name = "Medicine A", CategoryId = 1, Category = new Category { Name = "Cat A" } }
        };

        _mockUnitOfWork.Setup(x => x.Medicines.FindAsync(
            It.IsAny<Expression<Func<Medicine, bool>>>(),
            It.IsAny<Expression<Func<Medicine, object>>[]>()
        )).ReturnsAsync(medicines);

        // Act
        var result = await _medicineService.GetAllMedicinesAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Medicine A");
    }

    [Fact]
    public async Task GetMedicineByIdAsync_ShouldReturnMedicine_WhenExists()
    {
        // Arrange
        var medicine = new Medicine { Id = 1, Name = "Medicine A" };

        _mockUnitOfWork.Setup(x => x.Medicines.GetByIdAsync(
            1,
            It.IsAny<Expression<Func<Medicine, object>>[]>()
        )).ReturnsAsync(medicine);

        // Act
        var result = await _medicineService.GetMedicineByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Medicine A");
    }

    [Fact]
    public async Task CreateMedicineAsync_ShouldAddMedicine_AndBroadcastUpdate()
    {
        // Arrange
        var createDto = new CreateMedicineDto
        {
            Name = "New Med",
            MedicineCode = "MED001",
            CategoryId = 1
        };

        _mockCurrentUserService.Setup(x => x.GetCurrentUsername()).Returns("user");

        // Mock AddAsync
        _mockUnitOfWork.Setup(x => x.Medicines.AddAsync(It.IsAny<Medicine>()))
            .Callback<Medicine>(m => m.Id = 123) // Simulate DB generating ID
            .Returns(Task.CompletedTask);

        // Mock GetByIdAsync used at the end of Create
        _mockUnitOfWork.Setup(x => x.Medicines.GetByIdAsync(
            123,
            It.IsAny<Expression<Func<Medicine, object>>[]>()
        )).ReturnsAsync(new Medicine { Id = 123, Name = "New Med" });

        // Act
        var result = await _medicineService.CreateMedicineAsync(createDto);

        // Assert
        result.Id.Should().Be(123);

        _mockUnitOfWork.Verify(x => x.Medicines.AddAsync(It.IsAny<Medicine>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);

        // Verify broadcast
        _mockDashboardService.Verify(x => x.GetStatsAsync(), Times.Once);
        _mockBroadcaster.Verify(x => x.BroadcastStatsUpdate(It.IsAny<DashboardStatsDto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMedicineAsync_ShouldUpdateMedicine_AndBroadcastUpdate()
    {
        // Arrange
        var medicine = new Medicine
        {
            Id = 1,
            Name = "Old Name",
            CategoryId = 1,
            MedicineCode = "OLD001",
            IsActive = true
        };

        var updateDto = new UpdateMedicineDto
        {
            Id = 1,
            Name = "Updated Name",
            CategoryId = 2,
            MedicineCode = "UPD001",
            GenericName = "Generic",
            Manufacturer = "Manufacturer",
            StorageCondition = "Cool",
            UnitOfMeasure = "mg",
            IsActive = false
        };

        _mockUnitOfWork.Setup(x => x.Medicines.GetByIdAsync(1))
            .ReturnsAsync(medicine);

        // Act
        await _medicineService.UpdateMedicineAsync(updateDto);

        // Assert
        medicine.Name.Should().Be("Updated Name");
        medicine.CategoryId.Should().Be(2);
        medicine.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Medicines.Update(medicine), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);

        // Verify broadcast
        _mockDashboardService.Verify(x => x.GetStatsAsync(), Times.Once);
        _mockBroadcaster.Verify(x => x.BroadcastStatsUpdate(It.IsAny<DashboardStatsDto>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMedicineAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Medicines.GetByIdAsync(99))
            .ReturnsAsync((Medicine?)null);

        // Act
        var act = async () => await _medicineService.UpdateMedicineAsync(
            new UpdateMedicineDto { Id = 99, Name = "Test" });

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Medicine not found");
    }

    [Fact]
    public async Task DeleteMedicineAsync_ShouldSoftDelete_AndBroadcastUpdate()
    {
        // Arrange
        var medicine = new Medicine { Id = 1, Name = "Test", IsActive = true };

        _mockUnitOfWork.Setup(x => x.Medicines.GetByIdAsync(1))
            .ReturnsAsync(medicine);

        // Act
        await _medicineService.DeleteMedicineAsync(1);

        // Assert
        medicine.IsActive.Should().BeFalse();

        _mockUnitOfWork.Verify(x => x.Medicines.Update(medicine), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveAsync(default), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);

        // Verify broadcast
        _mockDashboardService.Verify(x => x.GetStatsAsync(), Times.Once);
        _mockBroadcaster.Verify(x => x.BroadcastStatsUpdate(It.IsAny<DashboardStatsDto>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMedicineAsync_ShouldThrow_WhenNotFound()
    {
        // Arrange
        _mockUnitOfWork.Setup(x => x.Medicines.GetByIdAsync(99))
            .ReturnsAsync((Medicine?)null);

        // Act
        var act = async () => await _medicineService.DeleteMedicineAsync(99);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Medicine not found");
    }
}
