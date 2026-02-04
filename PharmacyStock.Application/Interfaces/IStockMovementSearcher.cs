using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IStockMovementSearcher
{
    Task<IEnumerable<StockMovementDto>> SearchAsync(DateTime? fromDate, DateTime? toDate, int? medicineId, string? movementType);
}
