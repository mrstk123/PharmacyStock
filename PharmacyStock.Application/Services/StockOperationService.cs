using Microsoft.EntityFrameworkCore;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Utilities;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Enums;
using PharmacyStock.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PharmacyStock.Application.Services;

public class StockOperationService : IStockOperationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly IDashboardBroadcaster? _broadcaster;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<StockOperationService> _logger;

    public StockOperationService(IUnitOfWork unitOfWork, ICacheService cache, ICurrentUserService currentUserService, INotificationService notificationService, IServiceScopeFactory scopeFactory, ILogger<StockOperationService> logger, IDashboardBroadcaster? broadcaster = null)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _broadcaster = broadcaster;
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

            // Broadcast recent movement
            if (_broadcaster != null)
            {
                var recentMovementDto = new RecentMovementDto
                {
                    Id = movement.Id,
                    MedicineName = (await _unitOfWork.Medicines.GetByIdAsync(medicineId))?.Name ?? "Unknown",
                    BatchNumber = batch.BatchNumber,
                    MovementType = movement.MovementType,
                    Quantity = movement.Quantity,
                    Reason = movement.Reason,
                    PerformedAt = movement.PerformedAt,
                    PerformedBy = _currentUserService.GetCurrentUsername() ?? SystemConstants.SystemUsername
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var scopedDashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
                            await _broadcaster.BroadcastRecentMovement(recentMovementDto);
                            var stats = await scopedDashboardService.GetStatsAsync();
                            await _broadcaster.BroadcastStatsUpdate(stats);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to broadcast updates after expiring stock removal");
                    }
                });
            }
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

        // Resolve batch expiry notifications
        await _notificationService.ResolveActionAsync(batch.Id, "Batch", NotificationType.Critical);
        await _notificationService.ResolveActionAsync(batch.Id, "Batch", NotificationType.Warning);
        await _notificationService.ResolveActionAsync(batch.Id, "ExpiredBatch", NotificationType.Critical);

        // Check total medicine stock after return and handle low stock notification
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(medicineId)
            ?? throw new Exception("Medicine not found.");

        var remainingBatches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            b.MedicineId == medicineId &&
            b.IsActive &&
            b.CurrentQuantity > 0 &&
            b.Id != batch.Id); // Exclude the batch being returned

        var totalStock = remainingBatches.Sum(b => b.CurrentQuantity);
        NotificationDto? lowStockNotificationDto = null;

        if (totalStock >= medicine.LowStockThreshold)
        {
            // Stock is sufficient after return - resolve low stock alert
            await _notificationService.ResolveActionAsync(medicineId, "Medicine", NotificationType.StockAlert);
        }
        else
        {
            // Stock still below threshold - update or create notification
            var existingAlerts = await _unitOfWork.Notifications.FindAsync(n =>
                n.IsSystemAlert &&
                n.RelatedEntityId == medicine.Id &&
                n.RelatedEntityType == "Medicine" &&
                n.Type == NotificationType.StockAlert &&
                !n.IsActionTaken);

            var criticalLevel = (int)(medicine.LowStockThreshold * SystemConstants.StockAlertThresholds.CriticalPercentage);
            var priority = totalStock == 0
                ? SystemConstants.StockAlertThresholds.Priority.OutOfStock
                : totalStock < criticalLevel
                    ? SystemConstants.StockAlertThresholds.Priority.Critical
                    : SystemConstants.StockAlertThresholds.Priority.Warning;
            var title = totalStock == 0 ? "Out of Stock" : "Low Stock Alert";
            var currentMessage = totalStock == 0
                ? $"{medicine.Name} is out of stock. Immediate reorder required."
                : $"{medicine.Name} is low on stock. Current quantity: {totalStock} units.";

            if (existingAlerts.Any())
            {
                // Update existing notification
                var existingNotification = existingAlerts.First();
                existingNotification.Message = currentMessage;
                existingNotification.Title = title;
                existingNotification.Priority = priority;
                existingNotification.IsRead = false;
                _unitOfWork.Notifications.Update(existingNotification);
                await _unitOfWork.SaveAsync();

                // Prepare DTO for broadcasting
                lowStockNotificationDto = new NotificationDto
                {
                    Id = existingNotification.Id,
                    Title = existingNotification.Title,
                    Message = existingNotification.Message,
                    Type = existingNotification.Type,
                    Priority = existingNotification.Priority,
                    IsRead = existingNotification.IsRead,
                    CreatedAt = existingNotification.CreatedAt
                };
            }
            else
            {
                // Create new notification
                var notification = new Notification
                {
                    UserId = null,
                    IsSystemAlert = true,
                    Title = title,
                    Message = currentMessage,
                    Type = NotificationType.StockAlert,
                    Priority = priority,
                    IsRead = false,
                    RelatedEntityId = medicine.Id,
                    RelatedEntityType = "Medicine"
                };
                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveAsync();

                // Prepare DTO for broadcasting
                lowStockNotificationDto = new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Priority = notification.Priority,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                };
            }
        }

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

            // Broadcast recent movement
            if (_broadcaster != null)
            {
                var recentMovementDto = new RecentMovementDto
                {
                    Id = movement.Id,
                    MedicineName = (await _unitOfWork.Medicines.GetByIdAsync(medicineId))?.Name ?? "Unknown",
                    BatchNumber = batch.BatchNumber,
                    MovementType = movement.MovementType,
                    Quantity = movement.Quantity,
                    Reason = movement.Reason,
                    PerformedAt = movement.PerformedAt,
                    PerformedBy = _currentUserService.GetCurrentUsername() ?? SystemConstants.SystemUsername
                };

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var scopedDashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
                            await _broadcaster.BroadcastRecentMovement(recentMovementDto);

                            // Broadcast low stock notification if created/updated
                            if (lowStockNotificationDto != null)
                            {
                                await scopedDashboardService.InvalidateAlertsCacheAsync();
                                await _broadcaster.BroadcastSystemAlert(lowStockNotificationDto);
                                var alerts = await scopedDashboardService.GetAlertsAsync();
                                await _broadcaster.BroadcastAlertsUpdate(alerts);
                            }

                            var stats = await scopedDashboardService.GetStatsAsync();
                            await _broadcaster.BroadcastStatsUpdate(stats);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to broadcast updates after returning stock to supplier");
                    }
                });
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("Concurrency conflict detected while returning stock to supplier.");
        }
    }
}
