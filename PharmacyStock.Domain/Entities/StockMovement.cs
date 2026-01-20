using System;
using System.Collections.Generic;

namespace PharmacyStock.Domain.Entities;

public partial class StockMovement
{
    public int Id { get; set; }

    public int MedicineBatchId { get; set; }

    public int PerformedByUserId { get; set; }

    public string MovementType { get; set; } = null!;

    public int Quantity { get; set; }

    public string? Reason { get; set; }

    public string? ReferenceNo { get; set; }

    public int? SnapshotQuantity { get; set; }

    public DateTime PerformedAt { get; set; }

    public virtual MedicineBatch MedicineBatch { get; set; } = null!;

    public virtual User PerformedByUser { get; set; } = null!;
}
