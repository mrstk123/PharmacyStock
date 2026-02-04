using System;
using System.Collections.Generic;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Domain.Entities;

public partial class Category : IAuditableEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public ICollection<ExpiryRule> ExpiryRules { get; set; } = new List<ExpiryRule>();

    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
