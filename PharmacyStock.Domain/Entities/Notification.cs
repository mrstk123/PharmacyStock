using System;
using PharmacyStock.Domain.Enums;

namespace PharmacyStock.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }

    public bool IsSystemAlert { get; set; } = false;

    public bool IsActionTaken { get; set; } = false;

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public NotificationType Type { get; set; } = NotificationType.Info;

    public int Priority { get; set; } = 3;

    public int? RelatedEntityId { get; set; }

    public string? RelatedEntityType { get; set; }

}
