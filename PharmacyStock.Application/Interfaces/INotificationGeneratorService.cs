using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

/// <summary>
/// Service for generating automatic notifications based on system events
/// </summary>
public interface INotificationGeneratorService
{
    /// <summary>
    /// Generates notifications for batches approaching expiry
    /// </summary>
    Task GenerateExpiryNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates notifications for low stock items
    /// </summary>
    Task GenerateLowStockNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates notifications for expired batches
    /// </summary>
    Task GenerateExpiredBatchNotificationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs all notification generation tasks
    /// </summary>
    Task GenerateAllNotificationsAsync(CancellationToken cancellationToken = default);
}
