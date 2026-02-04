using PharmacyStock.Domain.Enums;

namespace PharmacyStock.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public bool IsSystemAlert { get; set; }
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public NotificationType Type { get; set; }
    public int Priority { get; set; }
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}

public class CreateNotificationDto
{
    public int? UserId { get; set; }
    public bool IsSystemAlert { get; set; } = false;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public int Priority { get; set; } = 3;
    public int? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}
