using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IInventoryService
{
    Task<StockCheckDto?> GetStockCheckAsync(int medicineId);
    Task<IEnumerable<MedicineBatchDto>> GetAllBatchesAsync();
    Task<MedicineBatchDto?> GetBatchByIdAsync(int id);
    Task<MedicineBatchDto?> GetBatchByNumberAsync(int medicineId, string batchNumber);
    Task<MedicineBatchDto> CreateBatchAsync(CreateMedicineBatchDto createBatchDto);
    Task UpdateBatchAsync(UpdateMedicineBatchDto updateBatchDto);
    Task<DispensePreviewDto> PreviewDispenseAsync(int medicineId, int quantity);
    Task<DispenseResultDto> DispenseStockAsync(DispenseStockDto dispenseDto);
    Task AdjustStockAsync(AdjustStockDto adjustDto);
    Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(DateTime? fromDate, DateTime? toDate, int? medicineId, string? movementType);
    Task<IEnumerable<ExpiryManagementDto>> GetBatchesByExpiryStatusAsync(string? status);
    Task SetBatchQuarantineAsync(int batchId, bool quarantine);
    Task<List<AlternativeMedicineDto>> GetAlternativeMedicinesAsync(int medicineId);
}
