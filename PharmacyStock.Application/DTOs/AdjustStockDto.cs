using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.Application.DTOs;

public class AdjustStockDto
{
    [Required]
    public int BatchId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "New Quantity cannot be negative")]
    public int NewQuantity { get; set; }

    [Required(ErrorMessage = "Reason is required for audit adjustment")]
    public string Reason { get; set; } = null!;


}
