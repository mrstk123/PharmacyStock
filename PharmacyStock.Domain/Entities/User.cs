using System;
using System.Collections.Generic;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Domain.Entities;

public partial class User : IAuditableEntity
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiryTime { get; set; }

    public bool IsPersistent { get; set; }

    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
