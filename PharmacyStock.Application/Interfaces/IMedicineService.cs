using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IMedicineService
{
    Task<IEnumerable<MedicineDto>> GetAllMedicinesAsync(bool? isActive = null);
    Task<PaginatedResult<MedicineDto>> GetPaginatedMedicinesAsync(int pageIndex, int pageSize, string? searchQuery = null, bool? isActive = null, string? sortField = null, int? sortOrder = null);
    Task<MedicineDto?> GetMedicineByIdAsync(int id);
    Task<MedicineDto> CreateMedicineAsync(CreateMedicineDto createMedicineDto);
    Task UpdateMedicineAsync(UpdateMedicineDto updateMedicineDto);
    Task DeleteMedicineAsync(int id);
}
