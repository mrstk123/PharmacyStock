using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;
using PharmacyStock.Application.Utilities;

namespace PharmacyStock.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public SupplierService(IUnitOfWork unitOfWork, ICacheService cache, ICurrentUserService currentUserService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(bool? isActive = null)
    {
        var cachedResult = await _cache.GetAsync<List<SupplierDto>>(CacheKeyBuilder.AllSuppliers());
        if (cachedResult != null)
        {
            return FilterSuppliers(cachedResult, isActive);
        }

        var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
        var result = _mapper.Map<List<SupplierDto>>(suppliers);

        await _cache.SetAsync(CacheKeyBuilder.AllSuppliers(), result, TimeSpan.FromHours(1));

        return FilterSuppliers(result, isActive);
    }

    private static IEnumerable<SupplierDto> FilterSuppliers(IEnumerable<SupplierDto> suppliers, bool? isActive)
    {
        if (isActive.HasValue)
        {
            return suppliers.Where(s => s.IsActive == isActive.Value);
        }
        return suppliers;
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null) return null;

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto)
    {
        var supplier = _mapper.Map<Supplier>(createSupplierDto);
        supplier.CreatedBy = _currentUserService.GetCurrentUsername();

        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveAsync();
        await InvalidateCacheAsync();

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task UpdateSupplierAsync(UpdateSupplierDto updateSupplierDto)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(updateSupplierDto.Id)
            ?? throw new Exception("Supplier not found");

        supplier.SupplierCode = updateSupplierDto.SupplierCode;
        supplier.Name = updateSupplierDto.Name;
        supplier.ContactInfo = updateSupplierDto.ContactInfo;
        supplier.IsActive = updateSupplierDto.IsActive;
        // Handled by AuditableEntityInterceptor
        // supplier.UpdatedAt = DateTime.UtcNow;
        // supplier.UpdatedBy = _currentUserService.GetCurrentUsername();

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveAsync();
        await InvalidateCacheAsync();
    }

    public async Task DeleteSupplierAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id)
            ?? throw new Exception("Supplier not found");

        supplier.IsActive = false;
        // Handled by AuditableEntityInterceptor
        // supplier.UpdatedAt = DateTime.UtcNow;
        // supplier.UpdatedBy = _currentUserService.GetCurrentUsername();

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveAsync();
        await InvalidateCacheAsync();
    }

    private async Task InvalidateCacheAsync()
    {
        await _cache.RemoveAsync(CacheKeyBuilder.AllSuppliers());
    }
}
