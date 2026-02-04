namespace PharmacyStock.Application.DTOs;

public class DispensePreviewDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int TotalAvailable { get; set; }
    public List<BatchAllocationDto> BatchAllocations { get; set; } = new List<BatchAllocationDto>();
    public bool CanDispense { get; set; }
    public string? Message { get; set; }
}
