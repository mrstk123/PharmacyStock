namespace PharmacyStock.Application.DTOs;

public class StockMovementDto
{
    public int Id { get; set; }
    public int MedicineBatchId { get; set; }
    public string MedicineName { get; set; } = null!;
    public string BatchNumber { get; set; } = null!;
    public string MovementType { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime PerformedAt { get; set; }
    public int PerformedByUserId { get; set; }
    public string? PerformedByUserName { get; set; }
    public string? Reason { get; set; }
}
