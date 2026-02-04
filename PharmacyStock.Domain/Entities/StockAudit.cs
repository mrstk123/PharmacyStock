using System;

namespace PharmacyStock.Domain.Entities;

public class StockAudit
{
    public int Id { get; set; }
    public int MedicineBatchId { get; set; }
    public string BatchNumber { get; set; } = null!;
    public string PropertyName { get; set; } = null!;
    public string OldValue { get; set; } = null!;
    public string NewValue { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public int ChangedByUserId { get; set; }
    public string ChangedByUserName { get; set; } = null!;

    public MedicineBatch MedicineBatch { get; set; } = null!;
}
