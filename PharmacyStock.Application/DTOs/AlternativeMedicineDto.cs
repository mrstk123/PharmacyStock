namespace PharmacyStock.Application.DTOs;

public class AlternativeMedicineDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public string MedicineCode { get; set; } = null!;
    public string? Manufacturer { get; set; }
    public int TotalAvailableStock { get; set; }
}
