using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Utilities;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class MedicineService : IMedicineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IDashboardService _dashboardService;
    private readonly IDashboardBroadcaster _broadcaster;

    public MedicineService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ICurrentUserService currentUserService,
        IMapper mapper,
        IDashboardService dashboardService,
        IDashboardBroadcaster broadcaster)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _dashboardService = dashboardService;
        _broadcaster = broadcaster;
    }

    public async Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync()
    {
        var medicines = await _unitOfWork.Medicines.FindAsync(m => true, m => m.Category);
        return _mapper.Map<IEnumerable<MedicineDto>>(medicines);
    }

    public async Task<PaginatedResult<MedicineDto>> GetPaginatedMedicinesAsync(int pageIndex, int pageSize, bool? isActive = null, string? sortField = null, int? sortOrder = null)
    {
        var allMedicines = await _cacheService.GetAsync<List<MedicineDto>>(CacheKeyBuilder.AllMedicines());

        if (allMedicines == null)
        {
            var medicines = await _unitOfWork.Medicines.FindAsync(m => true, m => m.Category);
            allMedicines = _mapper.Map<List<MedicineDto>>(medicines);

            await _cacheService.SetAsync(CacheKeyBuilder.AllMedicines(), allMedicines, TimeSpan.FromMinutes(10));
        }

        var query = allMedicines.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(m => m.IsActive == isActive.Value);
        }

        if (!string.IsNullOrEmpty(sortField))
        {
            var isAsc = sortOrder == 1; // 1 = ascending, -1 = descending
            query = sortField.ToLower() switch
            {
                "medicinecode" => isAsc ? query.OrderBy(m => m.MedicineCode) : query.OrderByDescending(m => m.MedicineCode),
                "name" => isAsc ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name),
                "categoryname" => isAsc ? query.OrderBy(m => m.CategoryName) : query.OrderByDescending(m => m.CategoryName),
                _ => query // Default no sort if field invalid
            };
        }
        else
        {
            // Default sort by Name Ascending
            query = query.OrderBy(m => m.Name);
        }

        var totalCount = query.Count();
        var items = query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<MedicineDto>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<IEnumerable<MedicineDto>> SearchMedicinesAsync(string query, bool? isActive = null, string? sortField = null, int? sortOrder = null)
    {
        // Use eager loading for search too
        var medicines = await _unitOfWork.Medicines.FindAsync(m =>
            m.MedicineCode.Contains(query) ||
            m.Name.Contains(query) ||
            (m.GenericName != null && m.GenericName.Contains(query)),
            m => m.Category);

        if (isActive.HasValue)
        {
            medicines = medicines.Where(m => m.IsActive == isActive.Value);
        }

        var dtos = _mapper.Map<IEnumerable<MedicineDto>>(medicines);

        if (!string.IsNullOrEmpty(sortField))
        {
            var isAsc = sortOrder == 1;
            dtos = sortField.ToLower() switch
            {
                "medicinecode" => isAsc ? dtos.OrderBy(m => m.MedicineCode) : dtos.OrderByDescending(m => m.MedicineCode),
                "name" => isAsc ? dtos.OrderBy(m => m.Name) : dtos.OrderByDescending(m => m.Name),
                "categoryname" => isAsc ? dtos.OrderBy(m => m.CategoryName) : dtos.OrderByDescending(m => m.CategoryName),
                _ => dtos
            };
        }

        return dtos;
    }

    public async Task<MedicineDto?> GetMedicineByIdAsync(int id)
    {
        var m = await _unitOfWork.Medicines.GetByIdAsync(id, m => m.Category);
        if (m == null) return null;

        return _mapper.Map<MedicineDto>(m);
    }

    public async Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto createMedicineDto)
    {
        var medicine = _mapper.Map<Medicine>(createMedicineDto);
        medicine.CreatedBy = _currentUserService.GetCurrentUsername();

        await _unitOfWork.Medicines.AddAsync(medicine);
        await _unitOfWork.SaveAsync();

        await InvalidateMedicineCacheAsync();

        // Broadcast update
        var stats = await _dashboardService.GetStatsAsync();
        await _broadcaster.BroadcastStatsUpdate(stats);

        return await GetMedicineByIdAsync(medicine.Id) ?? throw new Exception("Failed to create medicine");
    }

    public async Task UpdateMedicineAsync(UpdateMedicineDto updateMedicineDto)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(updateMedicineDto.Id)
                        ?? throw new Exception("Medicine not found");

        medicine.CategoryId = updateMedicineDto.CategoryId;
        medicine.MedicineCode = updateMedicineDto.MedicineCode;
        medicine.Name = updateMedicineDto.Name;
        medicine.GenericName = updateMedicineDto.GenericName;
        medicine.Manufacturer = updateMedicineDto.Manufacturer;
        medicine.StorageCondition = updateMedicineDto.StorageCondition;
        medicine.UnitOfMeasure = updateMedicineDto.UnitOfMeasure;
        medicine.IsActive = updateMedicineDto.IsActive;
        // Handled by AuditableEntityInterceptor
        // medicine.UpdatedAt = DateTime.UtcNow;
        // medicine.UpdatedBy = _currentUserService.GetCurrentUsername();

        _unitOfWork.Medicines.Update(medicine);
        await _unitOfWork.SaveAsync();
        await InvalidateMedicineCacheAsync();

        // Broadcast update
        var stats = await _dashboardService.GetStatsAsync();
        await _broadcaster.BroadcastStatsUpdate(stats);
    }

    public async Task DeleteMedicineAsync(int id)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(id)
                        ?? throw new Exception("Medicine not found");

        medicine.IsActive = false;
        // Handled by AuditableEntityInterceptor
        // medicine.UpdatedAt = DateTime.UtcNow;
        // medicine.UpdatedBy = _currentUserService.GetCurrentUsername();

        _unitOfWork.Medicines.Update(medicine);
        await _unitOfWork.SaveAsync();
        await InvalidateMedicineCacheAsync();

        // Broadcast update
        var stats = await _dashboardService.GetStatsAsync();
        await _broadcaster.BroadcastStatsUpdate(stats);
    }

    private async Task InvalidateMedicineCacheAsync()
    {
        await _cacheService.RemoveAsync(CacheKeyBuilder.AllMedicines());
    }

}
