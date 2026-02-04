namespace PharmacyStock.Application.DTOs;

public class ExpiryManagementDto
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int CurrentQuantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int Status { get; set; } // 0=Active, 1=Quarantined, 2=Expired, 3=Depleted, 4=Closed

    public int DaysUntilExpiry { get; set; }
    public string ExpiryStatus { get; set; } = string.Empty; // Expired, Critical, Warning, Normal
}
