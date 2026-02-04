namespace PharmacyStock.Application.DTOs;

public class StockCheckDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public int TotalQuantity { get; set; }
    public List<MedicineBatchDto> Batches { get; set; } = new List<MedicineBatchDto>();
}
