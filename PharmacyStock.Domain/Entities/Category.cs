using System;
using System.Collections.Generic;

namespace PharmacyStock.Domain.Entities;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<ExpiryRule> ExpiryRules { get; set; } = new List<ExpiryRule>();

    public virtual ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
