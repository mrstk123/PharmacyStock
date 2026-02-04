using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Enums;

namespace PharmacyStock.Application.Utilities;

/// <summary>
/// Utility class for batch status calculations
/// </summary>
public static class BatchStatusHelper
{
    /// <summary>
    /// Calculates the appropriate status for a batch based on its current state.
    /// Preserves manual statuses (Closed, Quarantined) and calculates automated statuses (Active, Expired, Depleted).
    /// </summary>
    /// <param name="batch">The medicine batch to evaluate</param>
    /// <returns>The calculated batch status</returns>
    public static BatchStatus CalculateBatchStatus(MedicineBatch batch)
    {
        var currentStatus = (BatchStatus)batch.Status;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // If manually closed (returned to supplier, disposed, written off), keep that status
        // Closed batches should not be automatically changed
        if (currentStatus == BatchStatus.Closed)
            return BatchStatus.Closed;

        // If manually quarantined, keep that status
        if (currentStatus == BatchStatus.Quarantined)
            return BatchStatus.Quarantined;

        // If quantity is 0, it's depleted (naturally used/sold out)
        if (batch.CurrentQuantity == 0)
            return BatchStatus.Depleted;

        // If expiry date has passed, it's expired (needs disposal)
        if (batch.ExpiryDate < today)
            return BatchStatus.Expired;

        // Otherwise, it's active and ready for sale
        return BatchStatus.Active;
    }
}
