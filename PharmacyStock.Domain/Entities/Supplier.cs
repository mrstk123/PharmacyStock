using System;
using System.Collections.Generic;

namespace PharmacyStock.Domain.Entities;

public partial class Supplier
{
    public int Id { get; set; }

    public string SupplierCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? ContactInfo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public virtual ICollection<MedicineBatch> MedicineBatches { get; set; } = new List<MedicineBatch>();
}
