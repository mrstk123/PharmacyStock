using System;
using System.Collections.Generic;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Domain.Entities;

public partial class MedicineBatch : IAuditableEntity
{
    public int Id { get; set; }

    public int MedicineId { get; set; }

    public int SupplierId { get; set; }

    public string BatchNumber { get; set; } = null!;

    public DateOnly ExpiryDate { get; set; }

    public DateOnly ReceivedDate { get; set; }

    public int InitialQuantity { get; set; }

    public int CurrentQuantity { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public int Status { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public Medicine Medicine { get; set; } = null!;

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public Supplier Supplier { get; set; } = null!;
}
