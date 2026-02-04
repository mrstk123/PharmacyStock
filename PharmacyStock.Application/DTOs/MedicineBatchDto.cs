using PharmacyStock.Domain.Enums;

namespace PharmacyStock.Application.DTOs;

public class MedicineBatchDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public string BatchNumber { get; set; } = null!;
    public DateOnly ExpiryDate { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public int InitialQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public BatchStatus Status { get; set; }
    public bool IsActive { get; set; }
    public string? Warning { get; set; }
}

public class CreateMedicineBatchDto
{
    public int MedicineId { get; set; }
    public int SupplierId { get; set; }
    public string BatchNumber { get; set; } = null!;
    public DateOnly ExpiryDate { get; set; }
    public int InitialQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
}

public class UpdateMedicineBatchDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = null!;
    public DateOnly ExpiryDate { get; set; }
    public BatchStatus Status { get; set; }
    public bool IsActive { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
}
