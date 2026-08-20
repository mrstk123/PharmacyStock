
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Utilities;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Enums;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public DashboardService(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<DashboardActionItemsDto> GetActionItemsAsync()
    {
        var cachedActionItems = await _cache.GetAsync<DashboardActionItemsDto>(CacheKeyBuilder.DashboardActionItems());
        if (cachedActionItems != null)
        {
            return cachedActionItems;
        }

        // Fetch unresolved system alerts from Notifications table (Single Source of Truth)
        var notifications = await _unitOfWork.Notifications.FindAsync(n =>
            n.IsSystemAlert &&
            !n.IsActionTaken &&
            (n.Type == NotificationType.Critical || n.Type == NotificationType.Warning || n.Type == NotificationType.StockIssue) &&
            (n.RelatedEntityType == "Batch" || n.RelatedEntityType == "ExpiredBatch" || n.RelatedEntityType == "Medicine"));

        // Get related IDs
        var batchIds = notifications.Where(n => n.RelatedEntityType == "Batch" || n.RelatedEntityType == "ExpiredBatch")
                                    .Select(n => n.RelatedEntityId ?? 0).Distinct().ToList();
        var medicineIdsFromAlerts = notifications.Where(n => n.RelatedEntityType == "Medicine")
                                                 .Select(n => n.RelatedEntityId ?? 0).Distinct().ToList();

        var batches = await _unitOfWork.MedicineBatches.FindAsync(b => batchIds.Contains(b.Id), b => b.Medicine);
        var batchDict = batches.ToDictionary(b => b.Id);

        // Get medicine names from batches and additional medicines from alerts
        var medicinesFromBatches = batches.Where(b => b.Medicine != null).ToDictionary(b => b.Medicine.Id, b => b.Medicine.Name);

        // Fetch additional medicines for low stock alerts (not in batches)
        var additionalMedicineIds = medicineIdsFromAlerts.Except(medicinesFromBatches.Keys).ToList();
        var additionalMedicines = additionalMedicineIds.Any()
            ? (await _unitOfWork.Medicines.FindAsync(m => additionalMedicineIds.Contains(m.Id))).ToDictionary(m => m.Id, m => m.Name)
            : new Dictionary<int, string>();

        var medicines = medicinesFromBatches.Concat(additionalMedicines).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var actionItems = new DashboardActionItemsDto();

        foreach (var notification in notifications)
        {
            ActionItemDto? actionItem = null;

            // Handle Batch/ExpiredBatch Alerts
            if ((notification.RelatedEntityType == "Batch" || notification.RelatedEntityType == "ExpiredBatch") &&
                notification.RelatedEntityId.HasValue &&
                batchDict.TryGetValue(notification.RelatedEntityId.Value, out var batch) &&
                batch.CurrentQuantity > 0)
            {
                var daysRemaining = (batch.ExpiryDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;
                var medicineName = batch.Medicine?.Name ?? "Unknown";

                actionItem = new ActionItemDto
                {
                    MedicineId = batch.MedicineId,
                    MedicineName = medicineName,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = batch.ExpiryDate,
                    DaysRemaining = daysRemaining,
                    CurrentQuantity = batch.CurrentQuantity,
                    Message = notification.Message
                };
            }
            // Handle Medicine (Low Stock) Alerts
            else if (notification.RelatedEntityType == "Medicine" &&
                     notification.RelatedEntityId.HasValue &&
                     medicines.TryGetValue(notification.RelatedEntityId.Value, out var medName))
            {
                actionItem = new ActionItemDto
                {
                    MedicineId = notification.RelatedEntityId.Value,
                    MedicineName = medName,
                    BatchNumber = "N/A",
                    ExpiryDate = DateOnly.MinValue,
                    DaysRemaining = 0,
                    CurrentQuantity = 0,
                    Message = notification.Message
                };
            }

            if (actionItem != null)
            {
                // Classify based on Type or Priority
                if (notification.Type == NotificationType.Critical ||
                   (notification.Type == NotificationType.StockIssue && notification.Priority >= 4))
                {
                    actionItems.Alerts.Add(actionItem);
                }
                else if (notification.Type == NotificationType.Warning ||
                        (notification.Type == NotificationType.StockIssue && notification.Priority < 4))
                {
                    actionItems.Warnings.Add(actionItem);
                }
            }
        }

        await _cache.SetAsync(CacheKeyBuilder.DashboardActionItems(), actionItems, TimeSpan.FromMinutes(30));

        return actionItems;
    }

    public async Task<InventoryValuationDto> GetValuationAsync()
    {
        var batches = await _unitOfWork.MedicineBatches.FindAsync(b => b.IsActive && b.CurrentQuantity > 0);

        return new InventoryValuationDto
        {
            TotalValue = batches.Sum(b => b.CurrentQuantity * b.PurchasePrice),
            TotalItems = batches.Sum(b => b.CurrentQuantity),
            ActiveBatches = batches.Count()
        };
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        // Explicitly filter for active medicines only
        var medicines = await _unitOfWork.Medicines.FindAsync(m => m.IsActive == true);
        var batches = await _unitOfWork.MedicineBatches.FindAsync(b => b.IsActive && b.CurrentQuantity > 0);
        var actionItems = await GetActionItemsAsync();

        // Create medicine dictionary for threshold lookup
        var medicineDict = medicines.ToDictionary(m => m.Id);

        // Calculate medicines with low stock using per-medicine threshold
        var medicineStocks = batches
            .GroupBy(b => b.MedicineId)
            .Select(g => new { MedicineId = g.Key, TotalQty = g.Sum(b => b.CurrentQuantity) })
            .Count(m => medicineDict.TryGetValue(m.MedicineId, out var med) && m.TotalQty < med.LowStockThreshold);

        return new DashboardStatsDto
        {
            TotalMedicines = medicines.Count(),
            TotalInventoryValue = batches.Sum(b => b.CurrentQuantity * b.PurchasePrice),
            CriticalAlerts = actionItems.Alerts.Count,
            WarningAlerts = actionItems.Warnings.Count,
            ActiveBatches = batches.Count(),
            LowStockItems = medicineStocks
        };
    }

    public async Task<List<LowStockIssueDto>> GetLowStockIssuesAsync(int threshold = 50)
    {
        // Fetch batches with Medicine and Category includes in single query
        var batches = await _unitOfWork.MedicineBatches.FindAsync(
            b => b.IsActive && b.CurrentQuantity > 0,
            b => b.Medicine
        );

        // Group batches by medicine and calculate total quantity
        var medicineStocks = batches
            .GroupBy(b => b.MedicineId)
            .Select(g => new
            {
                MedicineId = g.Key,
                TotalQty = g.Sum(b => b.CurrentQuantity),
                Medicine = g.First().Medicine
            })
            .ToList();

        var result = new List<LowStockIssueDto>();
        foreach (var item in medicineStocks)
        {
            if (item.Medicine == null) continue;
            var medicineThreshold = item.Medicine.LowStockThreshold > 0 ? item.Medicine.LowStockThreshold : threshold;

            if (item.TotalQty < medicineThreshold)
            {
                result.Add(new LowStockIssueDto
                {
                    MedicineId = item.Medicine.Id,
                    MedicineName = item.Medicine.Name,
                    MedicineCode = item.Medicine.MedicineCode,
                    TotalQuantity = item.TotalQty,
                    MinimumLevel = medicineThreshold,
                    CategoryName = item.Medicine.Category?.Name ?? "Unknown"
                });
            }
        }

        return result.OrderBy(r => r.TotalQuantity).ToList();
    }

    public async Task<List<RecentMovementDto>> GetRecentMovementsAsync(int count = 5)
    {
        // Fetch movements with includes for MedicineBatch, Medicine (via MedicineBatch), and User
        var movements = await _unitOfWork.StockMovements.FindAsync(
            m => true,
            query => query.OrderByDescending(m => m.PerformedAt),
            m => m.MedicineBatch.Medicine,
            m => m.PerformedByUser
        );

        return movements
            .Take(count)
            .Select(m =>
            {
                var batch = m.MedicineBatch;
                var medicine = batch?.Medicine;
                var user = m.PerformedByUser;

                return new RecentMovementDto
                {
                    Id = m.Id,
                    MedicineName = medicine?.Name ?? "Unknown",
                    BatchNumber = batch?.BatchNumber ?? "Unknown",
                    MovementType = m.MovementType,
                    Quantity = m.Quantity,
                    Reason = m.Reason,
                    PerformedAt = m.PerformedAt,
                    PerformedBy = user?.Username ?? "Unknown"
                };
            })
            .ToList();
    }

    public async Task InvalidateActionItemsCacheAsync()
    {
        await _cache.RemoveAsync(CacheKeyBuilder.DashboardActionItems());
    }
}
