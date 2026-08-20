using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardActionItemsDto> GetActionItemsAsync();
    Task<InventoryValuationDto> GetValuationAsync();
    Task<DashboardStatsDto> GetStatsAsync();
    Task<List<LowStockIssueDto>> GetLowStockIssuesAsync(int threshold = 50);
    Task<List<RecentMovementDto>> GetRecentMovementsAsync(int count = 15);
    Task InvalidateActionItemsCacheAsync();
}
