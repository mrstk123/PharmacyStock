using System;
using System.Collections.Generic;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Domain.Entities;

public partial class Medicine : IAuditableEntity
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string MedicineCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? GenericName { get; set; }

    public string? Manufacturer { get; set; }

    public string? StorageCondition { get; set; }

    public string UnitOfMeasure { get; set; } = null!;

    public int LowStockThreshold { get; set; } = 50;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<MedicineBatch> MedicineBatches { get; set; } = new List<MedicineBatch>();
}
