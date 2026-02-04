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
}
