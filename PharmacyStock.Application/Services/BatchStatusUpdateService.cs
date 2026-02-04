using Microsoft.Extensions.Logging;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Utilities;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Enums;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

/// <summary>
/// Service to update batch statuses based on current date and quantity
/// </summary>
public class BatchStatusUpdateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BatchStatusUpdateService> _logger;

    public BatchStatusUpdateService(IUnitOfWork unitOfWork, ILogger<BatchStatusUpdateService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Updates all batch statuses based on current state
    /// </summary>
    public async Task UpdateAllBatchStatusesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting batch status update...");

        try
        {
            var batches = await _unitOfWork.MedicineBatches.FindAsync(b => b.IsActive);
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            int updatedCount = 0;
            int expiredCount = 0;
            int depletedCount = 0;
            int activeCount = 0;

            foreach (var batch in batches)
            {
                var oldStatus = (BatchStatus)batch.Status;
                var newStatus = BatchStatusHelper.CalculateBatchStatus(batch);

                if (oldStatus != newStatus)
                {
                    batch.Status = (int)newStatus;
                    // Handled by AuditableEntityInterceptor
                    // batch.UpdatedAt = DateTime.UtcNow;
                    // batch.UpdatedBy = SystemConstants.SystemUsername;

                    _unitOfWork.MedicineBatches.Update(batch);
                    updatedCount++;

                    // Track status changes
                    switch (newStatus)
                    {
                        case BatchStatus.Expired:
                            expiredCount++;
                            break;
                        case BatchStatus.Depleted:
                            depletedCount++;
                            break;
                        case BatchStatus.Active:
                            activeCount++;
                            break;
                    }

                    _logger.LogDebug("Batch {BatchId} status changed from {OldStatus} to {NewStatus}",
                        batch.Id, oldStatus, newStatus);
                }
            }

            if (updatedCount > 0)
            {
                await _unitOfWork.SaveAsync(cancellationToken);
                _logger.LogInformation(
                    "Updated {Count} batch statuses: {Expired} expired, {Depleted} depleted, {Active} active",
                    updatedCount, expiredCount, depletedCount, activeCount);
            }
            else
            {
                _logger.LogInformation("No batch status updates needed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch statuses");
            throw;
        }
    }
}

