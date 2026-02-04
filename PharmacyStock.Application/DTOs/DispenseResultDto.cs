namespace PharmacyStock.Application.DTOs;

public class DispenseResultDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public int TotalDispensed { get; set; }
    public List<BatchAllocationDto> BatchAllocations { get; set; } = new List<BatchAllocationDto>();
    public DateTime PerformedAt { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
}
