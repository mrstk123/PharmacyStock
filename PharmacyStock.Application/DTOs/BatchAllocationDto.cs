namespace PharmacyStock.Application.DTOs;

public class BatchAllocationDto
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int QuantityAllocated { get; set; }
    public int RemainingAfter { get; set; }
}
