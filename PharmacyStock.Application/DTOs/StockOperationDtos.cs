namespace PharmacyStock.Application.DTOs;

public class RemoveExpiredStockDto
{
    public int BatchId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = null!;
}

public class ReturnToSupplierDto
{
    public int BatchId { get; set; }
    public string Reason { get; set; } = "RETURN";
}
