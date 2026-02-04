using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.Application.DTOs;

public class DispenseStockDto
{
    [Required]
    public int MedicineId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    public string? Reason { get; set; }
}
