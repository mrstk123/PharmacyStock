namespace PharmacyStock.Application.DTOs;

public class DashboardAlertsDto
{
    public List<AlertItemDto> Critical { get; set; } = new List<AlertItemDto>();
    public List<AlertItemDto> Warning { get; set; } = new List<AlertItemDto>();
}

public class AlertItemDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public string BatchNumber { get; set; } = null!;
    public DateOnly ExpiryDate { get; set; }
    public int DaysRemaining { get; set; }
    public int CurrentQuantity { get; set; }
    public string? Message { get; set; }
}

public class InventoryValuationDto
{
    public decimal TotalValue { get; set; }
    public int TotalItems { get; set; }
    public int ActiveBatches { get; set; }
}

public class DashboardStatsDto
{
    public int TotalMedicines { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int CriticalAlerts { get; set; }
    public int WarningAlerts { get; set; }
    public int ActiveBatches { get; set; }
    public int LowStockItems { get; set; }
}

public class LowStockAlertDto
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = null!;
    public string MedicineCode { get; set; } = null!;
    public int TotalQuantity { get; set; }
    public int MinimumLevel { get; set; }
    public string CategoryName { get; set; } = null!;
}

public class RecentMovementDto
{
    public int Id { get; set; }
    public string MedicineName { get; set; } = null!;
    public string BatchNumber { get; set; } = null!;
    public string MovementType { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public DateTime PerformedAt { get; set; }
    public string PerformedBy { get; set; } = null!;
}
