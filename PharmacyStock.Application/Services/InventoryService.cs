using Microsoft.Extensions.Logging;
using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Utilities;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Enums;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IStockMovementSearcher _searcher;
    private readonly IDashboardBroadcaster? _broadcaster;
    private readonly IDashboardService _dashboardService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<InventoryService> _logger;


    public InventoryService(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentUserService currentUserService, IMapper mapper, IStockMovementSearcher searcher, IDashboardService dashboardService, INotificationService notificationService, ILogger<InventoryService> logger, IDashboardBroadcaster? broadcaster = null)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _searcher = searcher;
        _dashboardService = dashboardService;
        _notificationService = notificationService;
        _logger = logger;
        _broadcaster = broadcaster;
    }


    public async Task<StockCheckDto?> GetStockCheckAsync(int medicineId)
    {
        var cached = await _cacheService.GetAsync<StockCheckDto>(CacheKeyBuilder.StockCheck(medicineId));

        if (cached != null)
        {
            return cached;
        }

        var medicine = await _unitOfWork.Medicines.GetByIdAsync(medicineId);
        if (medicine == null) return null;

        var batches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            b.MedicineId == medicineId &&
            b.IsActive &&
            b.CurrentQuantity > 0 &&
            b.Status != (int)BatchStatus.Expired &&      // Exclude expired batches
            b.Status != (int)BatchStatus.Quarantined,    // Exclude quarantined batches
            b => b.Supplier, b => b.Medicine);           // Eager load Supplier and Medicine

        var sortedBatches = batches.OrderBy(b => b.ExpiryDate).ToList();
        var totalQuantity = sortedBatches.Sum(b => b.CurrentQuantity);

        var result = new StockCheckDto
        {
            MedicineId = medicine.Id,
            MedicineName = medicine.Name,
            TotalQuantity = totalQuantity,
            Batches = _mapper.Map<List<MedicineBatchDto>>(sortedBatches)
        };

        await _cacheService.SetAsync(CacheKeyBuilder.StockCheck(medicineId), result, TimeSpan.FromMinutes(5));

        return result;
    }

    public async Task<IEnumerable<MedicineBatchDto>> GetAllBatchesAsync()
    {
        var batches = await _unitOfWork.MedicineBatches.FindAsync(b => true, b => b.Medicine, b => b.Supplier);
        return _mapper.Map<IEnumerable<MedicineBatchDto>>(batches);
    }

    public async Task<MedicineBatchDto?> GetBatchByIdAsync(int id)
    {
        var b = await _unitOfWork.MedicineBatches.GetByIdAsync(id, b => b.Medicine, b => b.Supplier);
        if (b == null) return null;

        return _mapper.Map<MedicineBatchDto>(b);
    }

    public async Task<MedicineBatchDto?> GetBatchByNumberAsync(int medicineId, string batchNumber)
    {
        var batches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            b.MedicineId == medicineId && b.BatchNumber == batchNumber && b.IsActive,
            b => b.Medicine, b => b.Supplier);

        var b = batches.FirstOrDefault();
        if (b == null) return null;

        return _mapper.Map<MedicineBatchDto>(b);
    }

    public async Task<MedicineBatchDto> CreateBatchAsync(CreateMedicineBatchDto createBatchDto)
    {
        var existingBatches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            b.MedicineId == createBatchDto.MedicineId && b.BatchNumber == createBatchDto.BatchNumber);

        var batch = existingBatches.FirstOrDefault();
        bool isNew = batch == null;

        if (isNew)
        {
            batch = _mapper.Map<MedicineBatch>(createBatchDto);
            batch.Status = (int)BatchStatus.Active; // New batches start as Active
            batch.CreatedBy = _currentUserService.GetCurrentUsername();
            await _unitOfWork.MedicineBatches.AddAsync(batch);
        }
        else
        {
            if (batch!.ExpiryDate != createBatchDto.ExpiryDate)
            {
                throw new InvalidOperationException($"Conflict: Batch '{createBatchDto.BatchNumber}' is already registered with expiry date {batch.ExpiryDate:dd/MM/yyyy}. You are trying to receive it with date {createBatchDto.ExpiryDate:dd/MM/yyyy}. Please verify your input.");
            }

            batch.CurrentQuantity += createBatchDto.InitialQuantity;
            batch.InitialQuantity += createBatchDto.InitialQuantity;
            // Handled by AuditableEntityInterceptor
            // batch.UpdatedAt = DateTime.UtcNow;
            // batch.UpdatedBy = _currentUserService.GetCurrentUsername();

            // Recalculate status after quantity change
            batch.Status = (int)BatchStatusHelper.CalculateBatchStatus(batch);

            _unitOfWork.MedicineBatches.Update(batch);
        }

        var movement = new StockMovement
        {
            MedicineBatch = batch,
            MovementType = "IN_Purchase",
            Quantity = createBatchDto.InitialQuantity,
            Reason = isNew ? "Initial Batch Purchase" : "Batch Quantity Append (Purchase)",
            PerformedAt = DateTime.UtcNow,
            PerformedByUserId = _currentUserService.GetCurrentUserId() ?? SystemConstants.SystemUserId
        };
        await _unitOfWork.StockMovements.AddAsync(movement);

        await _unitOfWork.SaveAsync();

        // Fetch medicine with category once for all subsequent operations
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(createBatchDto.MedicineId, m => m.Category)
            ?? throw new InvalidOperationException("Medicine not found");

        // Invalidate stock check cache for this medicine
        await _cacheService.RemoveAsync(CacheKeyBuilder.StockCheck(createBatchDto.MedicineId));

        // Check stock level and resolve alerts if needed using per-medicine threshold
        var lowStockThreshold = medicine.LowStockThreshold;

        var allBatches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            b.MedicineId == createBatchDto.MedicineId &&
            b.IsActive &&
            b.CurrentQuantity > 0);

        var totalStock = allBatches.Sum(b => b.CurrentQuantity);

        if (totalStock >= lowStockThreshold)
        {
            // Resolve "Low Stock Alert" (Warning) and "Out of Stock" (Critical)
            await _notificationService.ResolveActionAsync(createBatchDto.MedicineId, "Medicine", NotificationType.StockAlert);
        }

        var dto = await GetBatchByIdAsync(batch.Id) ?? throw new Exception("Failed to process batch");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expiryRules = await _unitOfWork.ExpiryRules.FindAsync(r => r.CategoryId == medicine.CategoryId && r.IsActive);
        var rule = expiryRules.FirstOrDefault();

        int alertDays = rule?.WarningDays ?? 30;

        if (createBatchDto.ExpiryDate < today.AddDays(alertDays))
        {
            dto.Warning = "Warning: Receiving short-dated stock.";
        }

        // Broadcast dashboard updates
        if (_broadcaster != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var stats = await _dashboardService.GetStatsAsync();
                    await _broadcaster.BroadcastStatsUpdate(stats);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to broadcast dashboard update after batch creation");
                }
            });
        }

        return dto;
    }

    public async Task UpdateBatchAsync(UpdateMedicineBatchDto updateBatchDto)
    {
        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(updateBatchDto.Id)
                    ?? throw new Exception("Batch not found");

        int medicineId = batch.MedicineId;

        var changes = new List<StockAudit>();
        int userId = _currentUserService.GetCurrentUserId() ?? SystemConstants.SystemUserId;
        string userName = _currentUserService.GetCurrentUsername() ?? string.Empty;

        // 1. Check Batch Number (though usually not editable, but good to track if changed)
        if (batch.BatchNumber != updateBatchDto.BatchNumber)
        {
            changes.Add(new StockAudit
            {
                MedicineBatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                PropertyName = "BatchNumber",
                OldValue = batch.BatchNumber,
                NewValue = updateBatchDto.BatchNumber,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                ChangedByUserName = userName
            });
            batch.BatchNumber = updateBatchDto.BatchNumber;
        }

        // 2. Check Expiry Date
        if (batch.ExpiryDate != updateBatchDto.ExpiryDate)
        {
            changes.Add(new StockAudit
            {
                MedicineBatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                PropertyName = "ExpiryDate",
                OldValue = batch.ExpiryDate.ToString("yyyy-MM-dd"),
                NewValue = updateBatchDto.ExpiryDate.ToString("yyyy-MM-dd"),
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                ChangedByUserName = userName
            });
            batch.ExpiryDate = updateBatchDto.ExpiryDate;
        }

        // 3. Check Prices
        if (batch.PurchasePrice != updateBatchDto.PurchasePrice)
        {
            changes.Add(new StockAudit
            {
                MedicineBatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                PropertyName = "PurchasePrice",
                OldValue = batch.PurchasePrice.ToString("F2"),
                NewValue = updateBatchDto.PurchasePrice.ToString("F2"),
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                ChangedByUserName = userName
            });
            batch.PurchasePrice = updateBatchDto.PurchasePrice;
        }

        if (batch.SellingPrice != updateBatchDto.SellingPrice)
        {
            changes.Add(new StockAudit
            {
                MedicineBatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                PropertyName = "SellingPrice",
                OldValue = batch.SellingPrice.ToString("F2"),
                NewValue = updateBatchDto.SellingPrice.ToString("F2"),
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                ChangedByUserName = userName
            });
            batch.SellingPrice = updateBatchDto.SellingPrice;
        }

        // 4. Check Status/Active (Optional audit, depending on requirements. For now, just update)
        batch.Status = (int)updateBatchDto.Status;
        batch.IsActive = updateBatchDto.IsActive;

        // Handled by AuditableEntityInterceptor
        // batch.UpdatedAt = DateTime.UtcNow;
        // batch.UpdatedBy = userName;

        _unitOfWork.MedicineBatches.Update(batch);

        if (changes.Count > 0)
        {
            foreach (var audit in changes)
            {
                await _unitOfWork.StockAudits.AddAsync(audit);
            }
        }

        await _unitOfWork.SaveAsync();

        // Invalidate stock check cache
        await _cacheService.RemoveAsync(CacheKeyBuilder.StockCheck(medicineId));
    }

    public async Task<DispensePreviewDto> PreviewDispenseAsync(int medicineId, int quantity)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(medicineId)
            ?? throw new InvalidOperationException("Medicine not found");

        var batches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            b.MedicineId == medicineId &&
            b.IsActive &&
            b.CurrentQuantity > 0 &&
            b.Status != (int)BatchStatus.Expired &&      // Exclude expired batches
            b.Status != (int)BatchStatus.Quarantined);   // Exclude quarantined batches

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var allBatches = batches.ToList();

        if (allBatches.Count == 0)
        {
            return new DispensePreviewDto
            {
                MedicineId = medicineId,
                MedicineName = medicine.Name,
                RequestedQuantity = quantity,
                TotalAvailable = 0,
                CanDispense = false,
                Message = "No stock available for this medicine.",
                BatchAllocations = new List<BatchAllocationDto>()
            };
        }

        var expiredBatches = allBatches.Where(b => b.ExpiryDate < today).ToList();
        if (expiredBatches.Count == allBatches.Count)
        {
            return new DispensePreviewDto
            {
                MedicineId = medicineId,
                MedicineName = medicine.Name,
                RequestedQuantity = quantity,
                TotalAvailable = 0,
                CanDispense = false,
                Message = "Cannot dispense: All available stock has expired.",
                BatchAllocations = new List<BatchAllocationDto>()
            };
        }

        var validBatches = allBatches
            .Where(b => b.ExpiryDate >= today)
            .OrderBy(b => b.ExpiryDate)
            .ToList();

        int totalAvailable = validBatches.Sum(b => b.CurrentQuantity);
        var allocations = new List<BatchAllocationDto>();
        int remainingToAllocate = quantity;

        foreach (var batch in validBatches)
        {
            if (remainingToAllocate <= 0) break;

            int quantityFromBatch = Math.Min(batch.CurrentQuantity, remainingToAllocate);

            allocations.Add(new BatchAllocationDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate.ToDateTime(TimeOnly.MinValue),
                QuantityAllocated = quantityFromBatch,
                RemainingAfter = batch.CurrentQuantity - quantityFromBatch
            });

            remainingToAllocate -= quantityFromBatch;
        }

        bool canDispense = totalAvailable >= quantity;
        string? message = null;

        if (!canDispense)
        {
            if (expiredBatches.Count != 0)
            {
                int expiredQuantity = expiredBatches.Sum(b => b.CurrentQuantity);
                message = $"Insufficient non-expired stock. Available: {totalAvailable}, Requested: {quantity}. " +
                         $"Note: {expiredQuantity} units have expired.";
            }
            else
            {
                message = $"Insufficient active stock. Available: {totalAvailable}, Requested: {quantity}";
            }
        }

        return new DispensePreviewDto
        {
            MedicineId = medicineId,
            MedicineName = medicine.Name,
            RequestedQuantity = quantity,
            TotalAvailable = totalAvailable,
            CanDispense = canDispense,
            Message = message,
            BatchAllocations = allocations
        };
    }

    public async Task<DispenseResultDto> DispenseStockAsync(DispenseStockDto dispenseDto)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(dispenseDto.MedicineId)
            ?? throw new InvalidOperationException("Medicine not found");

        // 1. Fetch active batches (exclude expired and quarantined)
        var batches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            b.MedicineId == dispenseDto.MedicineId &&
            b.IsActive &&
            b.CurrentQuantity > 0 &&
            b.Status != (int)BatchStatus.Expired &&      // Exclude expired batches
            b.Status != (int)BatchStatus.Quarantined);   // Exclude quarantined batches

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // 2. Check if any batches exist and if they're all expired
        var allBatches = batches.ToList();
        if (allBatches.Count == 0)
        {
            throw new InvalidOperationException("No stock available for this medicine.");
        }

        var expiredBatches = allBatches.Where(b => b.ExpiryDate < today).ToList();
        if (expiredBatches.Count == allBatches.Count)
        {
            throw new InvalidOperationException("Cannot dispense: All available stock has expired. Please dispose of expired medicine properly.");
        }

        // 3. Filter out expired and sort by ExpiryDate (FEFO)
        var validBatches = allBatches
            .Where(b => b.ExpiryDate >= today)
            .OrderBy(b => b.ExpiryDate)
            .ToList();

        // 4. Validation
        int totalAvailable = validBatches.Sum(b => b.CurrentQuantity);
        if (totalAvailable < dispenseDto.Quantity)
        {
            if (expiredBatches.Count != 0)
            {
                int expiredQuantity = expiredBatches.Sum(b => b.CurrentQuantity);
                throw new InvalidOperationException(
                    $"Insufficient non-expired stock. Available: {totalAvailable}, Requested: {dispenseDto.Quantity}. " +
                    $"Note: {expiredQuantity} units have expired and cannot be dispensed.");
            }
            throw new InvalidOperationException($"Insufficient active stock. Available: {totalAvailable}, Requested: {dispenseDto.Quantity}");
        }

        // 5. Dispense Logic
        int remainingToDispense = dispenseDto.Quantity;
        var movements = new List<StockMovement>();
        var batchAllocations = new List<BatchAllocationDto>();
        var depletedBatchIds = new List<int>();

        foreach (var batch in validBatches)
        {
            if (remainingToDispense <= 0) break;

            int quantityFromBatch = Math.Min(batch.CurrentQuantity, remainingToDispense);

            // Track allocation for result
            batchAllocations.Add(new BatchAllocationDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                ExpiryDate = batch.ExpiryDate.ToDateTime(TimeOnly.MinValue),
                QuantityAllocated = quantityFromBatch,
                RemainingAfter = batch.CurrentQuantity - quantityFromBatch
            });

            // Deduct
            batch.CurrentQuantity -= quantityFromBatch;
            // Handled by AuditableEntityInterceptor
            // batch.UpdatedAt = DateTime.UtcNow;
            // batch.UpdatedBy = _currentUserService.GetCurrentUsername();

            // Recalculate status after quantity change
            batch.Status = (int)BatchStatusHelper.CalculateBatchStatus(batch);

            if (batch.CurrentQuantity == 0)
            {
                depletedBatchIds.Add(batch.Id);
            }

            // Log Movement
            movements.Add(new StockMovement
            {
                MedicineBatchId = batch.Id,
                MovementType = "OUT_Dispense",
                Quantity = -quantityFromBatch,  // Negative for OUT movements
                Reason = dispenseDto.Reason ?? "Dispensed",
                PerformedAt = DateTime.UtcNow,
                PerformedByUserId = _currentUserService.GetCurrentUserId() ?? SystemConstants.SystemUserId
            });

            remainingToDispense -= quantityFromBatch;
            _unitOfWork.MedicineBatches.Update(batch);
        }

        foreach (var movement in movements)
        {
            await _unitOfWork.StockMovements.AddAsync(movement);
        }
        await _unitOfWork.SaveAsync();

        // Resolve notifications for depleted batches
        foreach (var batchId in depletedBatchIds)
        {
            await _notificationService.ResolveActionAsync(batchId, "Batch", NotificationType.Warning);
            await _notificationService.ResolveActionAsync(batchId, "Batch", NotificationType.Critical);
        }

        // Invalidate cache
        await _cacheService.RemoveAsync(CacheKeyBuilder.StockCheck(dispenseDto.MedicineId));

        // Broadcast dashboard updates
        if (_broadcaster != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var stats = await _dashboardService.GetStatsAsync();
                    await _broadcaster.BroadcastStatsUpdate(stats);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to broadcast dashboard update after dispense");
                }
            });
        }

        // Return detailed result
        return new DispenseResultDto
        {
            MedicineId = dispenseDto.MedicineId,
            MedicineName = medicine.Name,
            TotalDispensed = dispenseDto.Quantity,
            BatchAllocations = batchAllocations,
            PerformedAt = DateTime.UtcNow,
            PerformedBy = _currentUserService.GetCurrentUsername() ?? SystemConstants.SystemUsername
        };
    }

    public async Task AdjustStockAsync(AdjustStockDto adjustDto)
    {
        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(adjustDto.BatchId, b => b.Medicine)
                    ?? throw new InvalidOperationException("Batch not found");

        int currentQty = batch.CurrentQuantity;
        int newQty = adjustDto.NewQuantity;
        int delta = newQty - currentQty;

        // Update Batch
        batch.CurrentQuantity = newQty;

        if (delta == 0) return; // No change

        // Handled by AuditableEntityInterceptor
        // batch.UpdatedAt = DateTime.UtcNow;
        // batch.UpdatedBy = _currentUserService.GetCurrentUsername();

        // Log Movement
        var movement = new StockMovement
        {
            MedicineBatchId = batch.Id,
            MovementType = "ADJUSTMENT",
            Quantity = delta, // Signed value (+ or -)
            SnapshotQuantity = newQty,
            Reason = adjustDto.Reason,
            PerformedAt = DateTime.UtcNow,
            PerformedByUserId = _currentUserService.GetCurrentUserId() ?? SystemConstants.SystemUserId
        };

        _unitOfWork.MedicineBatches.Update(batch);
        await _unitOfWork.StockMovements.AddAsync(movement);
        await _unitOfWork.SaveAsync();

        // Invalidate cache
        await _cacheService.RemoveAsync(CacheKeyBuilder.StockCheck(batch.MedicineId));

        // Check stock level for low stock alerts (medicine already loaded via include)
        var medicine = batch.Medicine;
        if (medicine != null)
        {
            var allBatches = await _unitOfWork.MedicineBatches.FindAsync(b =>
                b.MedicineId == batch.MedicineId &&
                b.IsActive &&
                b.CurrentQuantity > 0);

            var totalStock = allBatches.Sum(b => b.CurrentQuantity);

            if (totalStock >= medicine.LowStockThreshold)
            {
                // Stock is sufficient - resolve any existing low stock alerts
                await _notificationService.ResolveActionAsync(batch.MedicineId, "Medicine", NotificationType.StockAlert);
            }
            else if (delta < 0) // Stock decreased and is now below threshold
            {
                // Check if alert already exists for today
                var existingAlerts = await _unitOfWork.Notifications.FindAsync(n =>
                    n.IsSystemAlert &&
                    n.RelatedEntityId == medicine.Id &&
                    n.RelatedEntityType == "Medicine" &&
                    n.Type == NotificationType.StockAlert &&
                    !n.IsActionTaken &&
                    n.CreatedAt.Date == DateTime.UtcNow.Date);

                if (!existingAlerts.Any())
                {
                    // Priority calculation based on percentage of threshold
                    // - Out of stock (0) = Priority 5 (Critical)
                    // - Below 50% of threshold = Priority 4 (High)
                    // - Above 50% but below threshold = Priority 3 (Warning)
                    var criticalLevel = (int)(medicine.LowStockThreshold * SystemConstants.StockAlertThresholds.CriticalPercentage);
                    var priority = totalStock == 0
                        ? SystemConstants.StockAlertThresholds.Priority.OutOfStock
                        : totalStock < criticalLevel
                            ? SystemConstants.StockAlertThresholds.Priority.Critical
                            : SystemConstants.StockAlertThresholds.Priority.Warning;
                    var title = totalStock == 0 ? "Out of Stock" : "Low Stock Alert";

                    var notification = new Notification
                    {
                        UserId = null,
                        IsSystemAlert = true,
                        Title = title,
                        Message = totalStock == 0
                            ? $"{medicine.Name} is out of stock. Immediate reorder required."
                            : $"{medicine.Name} is low on stock. Current quantity: {totalStock} units.",
                        Type = NotificationType.StockAlert,
                        Priority = priority,
                        IsRead = false,
                        // Handled by AuditableEntityInterceptor
                        // CreatedAt = DateTime.UtcNow,
                        RelatedEntityId = medicine.Id,
                        RelatedEntityType = "Medicine"
                    };

                    await _unitOfWork.Notifications.AddAsync(notification);
                    await _unitOfWork.SaveAsync();
                }
            }
        }

        // Broadcast dashboard updates
        if (_broadcaster != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var stats = await _dashboardService.GetStatsAsync();
                    await _broadcaster.BroadcastStatsUpdate(stats);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to broadcast dashboard update after stock adjustment");
                }
            });
        }
    }

    public async Task<IEnumerable<StockMovementDto>> GetStockMovementsAsync(DateTime? fromDate, DateTime? toDate, int? medicineId, string? movementType)
    {
        return await _searcher.SearchAsync(fromDate, toDate, medicineId, movementType);
    }

    public async Task<IEnumerable<ExpiryManagementDto>> GetBatchesByExpiryStatusAsync(string? status)
    {
        // Get all active batches
        var batches = (await _unitOfWork.MedicineBatches.FindAsync(b => b.IsActive && b.CurrentQuantity > 0)).ToList();

        // Get all medicines, suppliers, and expiry rules
        var medicines = (await _unitOfWork.Medicines.GetAllAsync()).ToDictionary(m => m.Id);
        var suppliers = (await _unitOfWork.Suppliers.GetAllAsync()).ToDictionary(s => s.Id);
        var categories = (await _unitOfWork.Categories.GetAllAsync()).ToDictionary(c => c.Id);
        var expiryRules = (await _unitOfWork.ExpiryRules.FindAsync(r => r.IsActive)).ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = new List<ExpiryManagementDto>();

        foreach (var batch in batches)
        {
            if (!medicines.TryGetValue(batch.MedicineId, out var medicine)) continue;
            if (!suppliers.TryGetValue(batch.SupplierId, out var supplier)) continue;

            // Find applicable rule: category-specific first, then global
            var rule = expiryRules.FirstOrDefault(r => r.CategoryId == medicine.CategoryId)
                ?? expiryRules.FirstOrDefault(r => r.CategoryId == null);

            if (rule == null) continue; // Skip if no rule found

            var daysUntilExpiry = batch.ExpiryDate.DayNumber - today.DayNumber;
            string expiryStatus;

            if (daysUntilExpiry < 0)
            {
                expiryStatus = "N/A"; // Expired batches - urgency not applicable (already expired)
            }
            else if (daysUntilExpiry <= rule.CriticalDays)
            {
                expiryStatus = "Critical";
            }
            else if (daysUntilExpiry <= rule.WarningDays)
            {
                expiryStatus = "Warning";
            }
            else
            {
                expiryStatus = "Normal";
            }

            // Apply filter based on status parameter
            bool includeInResult = status switch
            {
                "expired" => daysUntilExpiry < 0, // Batches that have passed expiry date
                "expiring-soon" => daysUntilExpiry >= 0 && (expiryStatus == "Critical" || expiryStatus == "Warning"), // Only non-expired batches approaching expiry
                "in-date" => expiryStatus == "Normal", // Only batches with Normal urgency (not expired, not expiring soon)
                null => true, // All active stock
                _ => false
            };

            if (includeInResult)
            {
                var categoryName = categories.TryGetValue(medicine.CategoryId, out var category)
                    ? category.Name
                    : "Uncategorized";

                result.Add(new ExpiryManagementDto
                {
                    Id = batch.Id,
                    MedicineId = batch.MedicineId,
                    MedicineName = medicine.Name,
                    CategoryName = categoryName,
                    SupplierId = batch.SupplierId,
                    SupplierName = supplier.Name,
                    BatchNumber = batch.BatchNumber,
                    ExpiryDate = new DateTime(batch.ExpiryDate.Year, batch.ExpiryDate.Month, batch.ExpiryDate.Day),
                    CurrentQuantity = batch.CurrentQuantity,
                    PurchasePrice = batch.PurchasePrice,
                    SellingPrice = batch.SellingPrice,
                    Status = (int)BatchStatusHelper.CalculateBatchStatus(batch), // Add actual batch status
                    DaysUntilExpiry = daysUntilExpiry,
                    ExpiryStatus = expiryStatus
                });
            }
        }

        // Sort by days until expiry (most urgent first)
        return result.OrderBy(r => r.DaysUntilExpiry).ToList();
    }

    public async Task SetBatchQuarantineAsync(int batchId, bool quarantine)
    {
        var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(batchId);
        if (batch == null)
            throw new InvalidOperationException($"Batch with ID {batchId} not found.");

        if (quarantine)
        {
            // Set to quarantined
            batch.Status = (int)BatchStatus.Quarantined;
        }
        else
        {
            // Recalculate status (will set to Active, Expired, or Depleted based on current state)
            batch.Status = (int)BatchStatusHelper.CalculateBatchStatus(batch);
        }

        batch.UpdatedAt = DateTime.UtcNow;
        batch.UpdatedBy = _currentUserService.GetCurrentUsername();
        _unitOfWork.MedicineBatches.Update(batch);

        await _unitOfWork.SaveAsync();

        // Invalidate cache for this medicine
        await _cacheService.RemoveAsync(CacheKeyBuilder.StockCheck(batch.MedicineId));
    }

    public async Task<List<AlternativeMedicineDto>> GetAlternativeMedicinesAsync(int medicineId)
    {
        // Get the selected medicine
        var selectedMedicine = await _unitOfWork.Medicines.GetByIdAsync(medicineId);
        if (selectedMedicine == null || string.IsNullOrWhiteSpace(selectedMedicine.GenericName))
        {
            return new List<AlternativeMedicineDto>();
        }

        // Find all medicines with the same generic name (case-insensitive)
        var alternatives = (await _unitOfWork.Medicines.FindAsync(m =>
            m.Id != medicineId &&
            m.IsActive &&
            m.GenericName != null &&
            m.GenericName.ToLower() == selectedMedicine.GenericName.ToLower())).ToList();

        if (!alternatives.Any())
        {
            return new List<AlternativeMedicineDto>();
        }

        // Batch fetch: Get all active batches for alternative medicines in a single query
        var medicineIds = alternatives.Select(m => m.Id).ToList();
        var batches = await _unitOfWork.MedicineBatches.FindAsync(b =>
            medicineIds.Contains(b.MedicineId) &&
            b.IsActive &&
            b.CurrentQuantity > 0);

        // Group batches by MedicineId and sum quantities in memory
        var stockTotals = batches
            .GroupBy(b => b.MedicineId)
            .ToDictionary(g => g.Key, g => g.Sum(b => b.CurrentQuantity));

        // Build result - only include medicines with available stock
        return alternatives
            .Where(m => stockTotals.ContainsKey(m.Id) && stockTotals[m.Id] > 0)
            .Select(medicine => new AlternativeMedicineDto
            {
                MedicineId = medicine.Id,
                MedicineName = medicine.Name,
                MedicineCode = medicine.MedicineCode,
                Manufacturer = medicine.Manufacturer,
                TotalAvailableStock = stockTotals[medicine.Id]
            })
            .OrderByDescending(a => a.TotalAvailableStock)
            .ToList();
    }
}


