using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using System.Security.Claims;

namespace PharmacyStock.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMyNotifications()
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var notifications = await _notificationService.GetMyNotificationsAsync(userId.Value);
        return Ok(notifications);
    }

    [HttpGet("system-alerts")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetSystemAlerts()
    {
        var systemAlerts = await _notificationService.GetSystemAlertsAsync();
        return Ok(systemAlerts);
    }

    [HttpPost]
    [AllowAnonymous] // Allow system or other services to create notifications without user context if needed, but ideally restricted
    public async Task<ActionResult> CreateNotification(CreateNotificationDto createNotificationDto)
    {
        await _notificationService.CreateNotificationAsync(createNotificationDto);
        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(userId.Value);
        return Ok();
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _notificationService.MarkAsReadAsync(id, userId.Value);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(int id)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null) return Unauthorized();

        await _notificationService.DeleteNotificationAsync(id, userId.Value);
        return Ok();
    }
}
