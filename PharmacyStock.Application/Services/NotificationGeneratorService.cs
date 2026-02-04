using Microsoft.Extensions.Logging;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Enums;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class NotificationGeneratorService : INotificationGeneratorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationGeneratorService> _logger;
    private readonly IDashboardBroadcaster? _broadcaster;
    private readonly IDashboardService _dashboardService;

    public NotificationGeneratorService(
        IUnitOfWork unitOfWork,
        ILogger<NotificationGeneratorService> logger,
        IDashboardService dashboardService,
        IDashboardBroadcaster? broadcaster = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _dashboardService = dashboardService;
        _broadcaster = broadcaster;
    }

    public async Task GenerateAllNotificationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting automatic notification generation...");

        try
        {
            await GenerateExpiryNotificationsAsync(cancellationToken);
            await GenerateLowStockNotificationsAsync(cancellationToken);
            await GenerateExpiredBatchNotificationsAsync(cancellationToken);

            _logger.LogInformation("Completed automatic notification generation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during notification generation");
            throw;
        }
    }

    public async Task GenerateExpiryNotificationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating expiry notifications...");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var batches = await _unitOfWork.MedicineBatches.FindAsync(
            b => b.IsActive && b.CurrentQuantity > 0,
            b => b.Medicine
        );

        var expiryRules = (await _unitOfWork.ExpiryRules.FindAsync(r => r.IsActive)).ToList();

        var notifications = new List<Notification>();
        var criticalCount = 0;
        var warningCount = 0;

        foreach (var batch in batches)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var medicine = batch.Medicine;
            if (medicine == null) continue;

            // Find applicable expiry rule
            var rule = expiryRules.FirstOrDefault(r => r.CategoryId == medicine.CategoryId)
                ?? expiryRules.FirstOrDefault(r => r.CategoryId == null);

            if (rule == null) continue;

            var daysUntilExpiry = batch.ExpiryDate.DayNumber - today.DayNumber;

            // Critical expiry (within critical days threshold)
            if (daysUntilExpiry > 0 && daysUntilExpiry <= rule.CriticalDays)
            {
                // Check if system alert already exists for this batch today
                var existingAlerts = await _unitOfWork.Notifications.FindAsync(n =>
                    n.IsSystemAlert &&
                    n.RelatedEntityId == batch.Id &&
                    n.RelatedEntityType == "Batch" &&
                    n.Type == NotificationType.Critical &&
                    n.CreatedAt.Date == DateTime.UtcNow.Date);

                if (!existingAlerts.Any())
                {
                    notifications.Add(new Notification
                    {
                        UserId = null, // System-wide alert
                        IsSystemAlert = true,
                        Title = "Critical Expiry Alert",
                        Message = $"{medicine.Name} (Batch {batch.BatchNumber}) expires in {daysUntilExpiry} days. Quantity: {batch.CurrentQuantity} units.",
                        Type = NotificationType.Critical,
                        Priority = 5,
                        IsRead = false,
                        // Handled by AuditableEntityInterceptor
                        // CreatedAt = DateTime.UtcNow,
                        RelatedEntityId = batch.Id,
                        RelatedEntityType = "Batch"
                    });
                    criticalCount++;
                }
            }
            // Warning expiry (within warning days threshold but not critical)
            else if (daysUntilExpiry > rule.CriticalDays && daysUntilExpiry <= rule.WarningDays)
            {
                // Check if system alert already exists for this batch today
                var existingAlerts = await _unitOfWork.Notifications.FindAsync(n =>
                    n.IsSystemAlert &&
                    n.RelatedEntityId == batch.Id &&
                    n.RelatedEntityType == "Batch" &&
                    n.Type == NotificationType.Warning &&
                    n.CreatedAt.Date == DateTime.UtcNow.Date);

                if (!existingAlerts.Any())
                {
                    notifications.Add(new Notification
                    {
                        UserId = null, // System-wide alert
                        IsSystemAlert = true,
                        Title = "Expiry Warning",
                        Message = $"{medicine.Name} (Batch {batch.BatchNumber}) expires in {daysUntilExpiry} days. Quantity: {batch.CurrentQuantity} units.",
                        Type = NotificationType.Warning,
                        Priority = 3,
                        IsRead = false,
                        // Handled by AuditableEntityInterceptor
                        // CreatedAt = DateTime.UtcNow,
                        RelatedEntityId = batch.Id,
                        RelatedEntityType = "Batch"
                    });
                    warningCount++;
                }
            }
        }

        // Bulk insert notifications
        if (notifications.Any())
        {
            foreach (var notification in notifications)
            {
                await _unitOfWork.Notifications.AddAsync(notification);
            }
            await _unitOfWork.SaveAsync(cancellationToken);

            _logger.LogInformation("Created {Count} system expiry alerts ({Critical} critical, {Warning} warning)",
                notifications.Count, criticalCount, warningCount);

            // Broadcast notification via WebSocket
            if (_broadcaster != null && notifications.Any())
            {
                await _broadcaster.BroadcastNotification(
                    $"Generated {notifications.Count} expiry alerts",
                    "info");

                // Broadcast stats update
                var stats = await _dashboardService.GetStatsAsync();
                await _broadcaster.BroadcastStatsUpdate(stats);
                var alerts = await _dashboardService.GetAlertsAsync();
                await _broadcaster.BroadcastAlertsUpdate(alerts);
            }
        }
        else
        {
            _logger.LogInformation("No new expiry alerts to create");
        }
    }

    public async Task GenerateLowStockNotificationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating low stock notifications...");

        // Fetch batches with Medicine navigation property in single query
        var batches = await _unitOfWork.MedicineBatches.FindAsync(
            b => b.IsActive && b.CurrentQuantity > 0,
            b => b.Medicine
        );

        // Calculate total quantity per medicine
        var medicineStocks = batches
            .GroupBy(b => b.MedicineId)
            .Select(g => new
            {
                MedicineId = g.Key,
                TotalQty = g.Sum(b => b.CurrentQuantity),
                Medicine = g.First().Medicine // Medicine already loaded via include
            })
            .Where(m => m.Medicine != null && m.TotalQty < m.Medicine.LowStockThreshold)
            .ToList();

        var notifications = new List<Notification>();

        foreach (var stock in medicineStocks)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var medicine = stock.Medicine;
            if (medicine == null) continue;

            // Check if system alert already exists for this medicine today
            var existingAlerts = await _unitOfWork.Notifications.FindAsync(n =>
                n.IsSystemAlert &&
                n.RelatedEntityId == medicine.Id &&
                n.RelatedEntityType == "Medicine" &&
                n.Type == NotificationType.StockAlert &&
                n.CreatedAt.Date == DateTime.UtcNow.Date);

            if (!existingAlerts.Any())
            {
                // Priority calculation based on percentage of threshold
                // - Out of stock (0) = Priority 5 (Critical)
                // - Below 50% of threshold = Priority 4 (High)
                // - Above 50% but below threshold = Priority 3 (Warning)
                var criticalLevel = (int)(medicine.LowStockThreshold * SystemConstants.StockAlertThresholds.CriticalPercentage);
                var priority = stock.TotalQty == 0
                    ? SystemConstants.StockAlertThresholds.Priority.OutOfStock
                    : stock.TotalQty < criticalLevel
                        ? SystemConstants.StockAlertThresholds.Priority.Critical
                        : SystemConstants.StockAlertThresholds.Priority.Warning;
                var title = stock.TotalQty == 0 ? "Out of Stock" : "Low Stock Alert";

                notifications.Add(new Notification
                {
                    UserId = null, // System-wide alert
                    IsSystemAlert = true,
                    Title = title,
                    Message = stock.TotalQty == 0
                        ? $"{medicine.Name} is out of stock. Immediate reorder required."
                        : $"{medicine.Name} is low on stock. Current quantity: {stock.TotalQty} units.",
                    Type = NotificationType.StockAlert,
                    Priority = priority,
                    IsRead = false,
                    // Handled by AuditableEntityInterceptor
                    // CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = medicine.Id,
                    RelatedEntityType = "Medicine"
                });
            }
        }

        if (notifications.Any())
        {
            foreach (var notification in notifications)
            {
                await _unitOfWork.Notifications.AddAsync(notification);
            }
            await _unitOfWork.SaveAsync(cancellationToken);

            _logger.LogInformation("Created {Count} system low stock alerts", notifications.Count);

            if (_broadcaster != null)
            {
                await _broadcaster.BroadcastNotification(
                    $"Generated {notifications.Count} low stock alerts",
                    "info");

                // Broadcast stats update
                var stats = await _dashboardService.GetStatsAsync();
                await _broadcaster.BroadcastStatsUpdate(stats);
                var alerts = await _dashboardService.GetAlertsAsync();
                await _broadcaster.BroadcastAlertsUpdate(alerts);
            }
        }
        else
        {
            _logger.LogInformation("No new low stock alerts to create");
        }
    }

    public async Task GenerateExpiredBatchNotificationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating expired batch notifications...");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Fetch expired batches with Medicine navigation property in single query
        var expiredBatches = await _unitOfWork.MedicineBatches.FindAsync(
            b => b.IsActive && b.CurrentQuantity > 0 && b.ExpiryDate < today,
            b => b.Medicine
        );

        var notifications = new List<Notification>();

        foreach (var batch in expiredBatches)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var medicine = batch.Medicine;
            if (medicine == null) continue;

            // Check if system alert already exists for this batch today
            var existingAlerts = await _unitOfWork.Notifications.FindAsync(n =>
                n.IsSystemAlert &&
                n.RelatedEntityId == batch.Id &&
                n.RelatedEntityType == "ExpiredBatch" &&
                n.CreatedAt.Date == DateTime.UtcNow.Date);

            if (!existingAlerts.Any())
            {
                var daysExpired = today.DayNumber - batch.ExpiryDate.DayNumber;

                notifications.Add(new Notification
                {
                    UserId = null, // System-wide alert
                    IsSystemAlert = true,
                    Title = "Expired Stock - Disposal Required",
                    Message = $"{medicine.Name} (Batch {batch.BatchNumber}) expired {daysExpired} days ago. Quantity: {batch.CurrentQuantity} units. Requires proper disposal.",
                    Type = NotificationType.Critical,
                    Priority = 5,
                    IsRead = false,
                    // Handled by AuditableEntityInterceptor
                    // CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = batch.Id,
                    RelatedEntityType = "ExpiredBatch"
                });
            }
        }

        if (notifications.Any())
        {
            foreach (var notification in notifications)
            {
                await _unitOfWork.Notifications.AddAsync(notification);
            }
            await _unitOfWork.SaveAsync(cancellationToken);

            _logger.LogInformation("Created {Count} system expired batch alerts", notifications.Count);

            if (_broadcaster != null)
            {
                await _broadcaster.BroadcastNotification(
                    $"Generated {notifications.Count} expired batch alerts",
                    "warning");

                // Broadcast stats update
                var stats = await _dashboardService.GetStatsAsync();
                await _broadcaster.BroadcastStatsUpdate(stats);
                var alerts = await _dashboardService.GetAlertsAsync();
                await _broadcaster.BroadcastAlertsUpdate(alerts);
            }
        }
        else
        {
            _logger.LogInformation("No new expired batch alerts to create");
        }
    }
}
