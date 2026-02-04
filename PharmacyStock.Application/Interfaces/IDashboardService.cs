using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardAlertsDto> GetAlertsAsync();
    Task<InventoryValuationDto> GetValuationAsync();
    Task<DashboardStatsDto> GetStatsAsync();
    Task<List<LowStockAlertDto>> GetLowStockAlertsAsync(int threshold = 50);
    Task<List<RecentMovementDto>> GetRecentMovementsAsync(int count = 15);
    Task InvalidateAlertsCacheAsync();
}
