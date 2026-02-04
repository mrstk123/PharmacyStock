using Microsoft.EntityFrameworkCore;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Utilities;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Enums;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class StockOperationService : IStockOperationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;

    public StockOperationService(IUnitOfWork unitOfWork, ICacheService cache, ICurrentUserService currentUserService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    public async Task RemoveExpiredStockAsync(RemoveExpiredStockDto removeDto)
    {
        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(removeDto.BatchId)
            ?? throw new Exception("Batch not found.");

        int medicineId = batch.MedicineId;

        if (batch.CurrentQuantity < removeDto.Quantity)
        {
            throw new Exception("Requested quantity exceeds batch current quantity.");
        }

        batch.CurrentQuantity -= removeDto.Quantity;

        // If fully disposed, set to Closed; otherwise keep as Expired
        if (batch.CurrentQuantity == 0)
        {
            batch.Status = (int)BatchStatus.Closed;

            // Resolve notifications related to this batch
            await _notificationService.ResolveActionAsync(batch.Id, "ExpiredBatch", NotificationType.Critical);
            await _notificationService.ResolveActionAsync(batch.Id, "Batch", NotificationType.Critical);
            await _notificationService.ResolveActionAsync(batch.Id, "Batch", NotificationType.Warning);
        }
        // Note: Partial disposal keeps status as Expired

        // Handled by AuditableEntityInterceptor
        // batch.UpdatedAt = DateTime.UtcNow;
        // batch.UpdatedBy = _currentUserService.GetCurrentUsername();
        _unitOfWork.MedicineBatches.Update(batch);

        var movement = new StockMovement
        {
            MedicineBatchId = batch.Id,
            MovementType = "OUT_Expired",
            Quantity = -removeDto.Quantity,
            Reason = removeDto.Reason,
            PerformedAt = DateTime.UtcNow,
            PerformedByUserId = _currentUserService.GetCurrentUserId() ?? SystemConstants.SystemUserId
        };
        await _unitOfWork.StockMovements.AddAsync(movement);

        try
        {
            await _unitOfWork.SaveAsync();
            // Invalidate stock check cache
            await _cache.RemoveAsync(CacheKeyBuilder.StockCheck(medicineId));
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("Concurrency conflict detected while removing expired stock.");
        }
    }

    public async Task ReturnToSupplierAsync(ReturnToSupplierDto returnDto)
    {
        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(returnDto.BatchId)
            ?? throw new Exception("Batch not found.");

        int medicineId = batch.MedicineId;
        int returnQuantity = batch.CurrentQuantity; // Return entire quantity

        if (returnQuantity == 0)
        {
            throw new Exception("Cannot return batch with zero quantity.");
        }

        // Set quantity to 0 and status to Closed (batch removed from inventory)
        batch.CurrentQuantity = 0;
        batch.Status = (int)BatchStatus.Closed;
        // Handled by AuditableEntityInterceptor
        // batch.UpdatedAt = DateTime.UtcNow;
        // batch.UpdatedBy = _currentUserService.GetCurrentUsername();
        _unitOfWork.MedicineBatches.Update(batch);

        // Resolve notifications
        await _notificationService.ResolveActionAsync(batch.Id, "Batch", NotificationType.Critical);
        await _notificationService.ResolveActionAsync(batch.Id, "Batch", NotificationType.Warning);
        await _notificationService.ResolveActionAsync(batch.Id, "ExpiredBatch", NotificationType.Critical);

        var movement = new StockMovement
        {
            MedicineBatchId = batch.Id,
            MovementType = "OUT_Return",
            Quantity = -returnQuantity, // Negative for OUT movements
            Reason = returnDto.Reason,
            PerformedAt = DateTime.UtcNow,
            PerformedByUserId = _currentUserService.GetCurrentUserId() ?? SystemConstants.SystemUserId
        };
        await _unitOfWork.StockMovements.AddAsync(movement);

        try
        {
            await _unitOfWork.SaveAsync();
            // Invalidate stock check cache
            await _cache.RemoveAsync(CacheKeyBuilder.StockCheck(medicineId));
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("Concurrency conflict detected while returning stock to supplier.");
        }
    }
}
