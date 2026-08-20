using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IDashboardBroadcaster
{
    Task BroadcastStatsUpdate(DashboardStatsDto stats);
    Task BroadcastActionItemsUpdate(DashboardActionItemsDto actionItems);
    Task BroadcastRecentMovement(RecentMovementDto movement);
    Task BroadcastNotification(string message, string type = "info");
    Task BroadcastSystemAlert(NotificationDto notification);
}
