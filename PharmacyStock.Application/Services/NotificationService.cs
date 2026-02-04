using AutoMapper;
using Microsoft.Extensions.Logging;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Enums;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;
    private readonly IDashboardBroadcaster? _broadcaster;
    private readonly IDashboardService _dashboardService;
    private readonly IMapper _mapper;

    public NotificationService(
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger,
        IDashboardService dashboardService,
        IMapper mapper,
        IDashboardBroadcaster? broadcaster = null)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _dashboardService = dashboardService;
        _mapper = mapper;
        _broadcaster = broadcaster;
    }

    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(int userId)
    {
        // Fetch user-specific notifications AND system alerts
        var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId || n.IsSystemAlert);
        var sortedNotifications = notifications.OrderByDescending(n => n.CreatedAt);
        return _mapper.Map<IEnumerable<NotificationDto>>(sortedNotifications);
    }

    public async Task<IEnumerable<NotificationDto>> GetSystemAlertsAsync()
    {
        // Fetch system-wide alerts (dashboard alerts) that haven't been resolved
        var systemAlerts = await _unitOfWork.Notifications.FindAsync(n => n.IsSystemAlert && !n.IsActionTaken);
        var sortedAlerts = systemAlerts.OrderByDescending(n => n.Priority).ThenByDescending(n => n.CreatedAt);
        return _mapper.Map<IEnumerable<NotificationDto>>(sortedAlerts);
    }

    public async Task ResolveActionAsync(int relatedEntityId, string relatedEntityType, NotificationType type)
    {
        var notifications = await _unitOfWork.Notifications.FindAsync(n =>
            n.IsSystemAlert &&
            !n.IsActionTaken &&
            n.RelatedEntityId == relatedEntityId &&
            n.RelatedEntityType == relatedEntityType &&
            n.Type == type);

        bool anyUpdated = false;
        foreach (var notification in notifications)
        {
            notification.IsActionTaken = true;
            _unitOfWork.Notifications.Update(notification);
            anyUpdated = true;
        }

        if (anyUpdated)
        {
            await _unitOfWork.SaveAsync();

            if (_broadcaster != null)
            {
                // Invalidate dashboard cache so new stats are fetched fresh
                await _dashboardService.InvalidateAlertsCacheAsync();

                // Push real-time updates to dashboard
                var stats = await _dashboardService.GetStatsAsync();
                var alerts = await _dashboardService.GetAlertsAsync();

                await _broadcaster.BroadcastStatsUpdate(stats);
                await _broadcaster.BroadcastAlertsUpdate(alerts);
            }
        }
    }

    public async Task CreateNotificationAsync(CreateNotificationDto notificationDto)
    {
        var notification = new Notification
        {
            UserId = notificationDto.UserId,
            IsSystemAlert = notificationDto.IsSystemAlert,
            Title = notificationDto.Title,
            Message = notificationDto.Message,
            IsRead = false,
            // Handled by AuditableEntityInterceptor
            // CreatedAt = DateTime.UtcNow,
            Type = notificationDto.Type,
            Priority = notificationDto.Priority,
            RelatedEntityId = notificationDto.RelatedEntityId,
            RelatedEntityType = notificationDto.RelatedEntityType
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveAsync();
    }

    public async Task MarkAsReadAsync(int id, int userId)
    {
        var notifications = await _unitOfWork.Notifications.FindAsync(n => n.Id == id && (n.UserId == userId || n.IsSystemAlert));
        var notification = notifications.FirstOrDefault();

        if (notification == null) return;

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveAsync();
    }

    public async Task MarkAllAsReadAsync(int userId)
    {
        // Mark all notifications for this user (user-specific + system alerts) as read
        var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId || n.IsSystemAlert);

        foreach (var notification in notifications)
        {
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
            }
        }

        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteNotificationAsync(int id, int userId)
    {
        var notifications = await _unitOfWork.Notifications.FindAsync(n => n.Id == id && n.UserId == userId);
        var notification = notifications.FirstOrDefault();

        if (notification == null) return;

        _unitOfWork.Notifications.Delete(notification);
        await _unitOfWork.SaveAsync();
    }
}
