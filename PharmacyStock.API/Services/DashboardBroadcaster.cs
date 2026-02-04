using Microsoft.AspNetCore.SignalR;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;

namespace PharmacyStock.API.Services;

public class DashboardBroadcaster : IDashboardBroadcaster
{
    private readonly IHubContext<Hubs.DashboardHub> _hubContext;

    public DashboardBroadcaster(IHubContext<Hubs.DashboardHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastStatsUpdate(DashboardStatsDto stats)
    {
        await _hubContext.Clients.All.SendAsync("StatsUpdated", stats);
    }

    public async Task BroadcastAlertsUpdate(DashboardAlertsDto alerts)
    {
        await _hubContext.Clients.All.SendAsync("AlertsUpdated", alerts);
    }

    public async Task BroadcastRecentMovement(RecentMovementDto movement)
    {
        await _hubContext.Clients.All.SendAsync("MovementAdded", movement);
    }

    public async Task BroadcastNotification(string message, string type = "info")
    {
        await _hubContext.Clients.All.SendAsync("Notification", new { message, type, timestamp = DateTime.UtcNow });
    }

    public async Task BroadcastSystemAlert(NotificationDto notification)
    {
        await _hubContext.Clients.All.SendAsync("NotificationAdded", notification);
    }
}
