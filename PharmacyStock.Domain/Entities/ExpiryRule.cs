using System;
using System.Collections.Generic;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Domain.Entities;

public partial class ExpiryRule : IAuditableEntity
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public int WarningDays { get; set; }

    public int CriticalDays { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public Category? Category { get; set; }
}
