using PharmacyStock.Application.DTOs;
using PharmacyStock.Domain.Enums;

namespace PharmacyStock.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId);
    Task<IEnumerable<NotificationDto>> GetSystemAlertsAsync();
    Task MarkAsReadAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteNotificationAsync(int id, int userId);

    Task ResolveActionAsync(int relatedEntityId, string relatedEntityType, NotificationType type);
    Task CreateNotificationAsync(CreateNotificationDto notificationDto);

    /// <summary>
    /// Checks the total stock for a medicine against its low-stock threshold and either
    /// resolves the existing alert (if stock is sufficient) or creates/updates a low-stock
    /// notification. Returns the notification DTO if one was created or updated, null otherwise.
    /// </summary>
    Task<NotificationDto?> HandleLowStockNotificationAsync(int medicineId, int totalStock);
}
