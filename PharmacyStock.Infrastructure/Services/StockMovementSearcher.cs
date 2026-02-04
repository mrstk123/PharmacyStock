using Microsoft.EntityFrameworkCore;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Infrastructure.Persistence.Context;

namespace PharmacyStock.Infrastructure.Services;

public class StockMovementSearcher : IStockMovementSearcher
{
    private readonly AppDbContext _context;

    public StockMovementSearcher(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StockMovementDto>> SearchAsync(DateTime? fromDate, DateTime? toDate, int? medicineId, string? movementType)
    {
        var query = _context.StockMovements
            .Include(sm => sm.MedicineBatch)
            .ThenInclude(mb => mb.Medicine)
            .Include(sm => sm.PerformedByUser)
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(sm => sm.PerformedAt.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(sm => sm.PerformedAt.Date <= toDate.Value.Date);

        if (medicineId.HasValue)
            query = query.Where(sm => sm.MedicineBatch.MedicineId == medicineId.Value);

        if (!string.IsNullOrEmpty(movementType))
            query = query.Where(sm => sm.MovementType == movementType);

        return await query
            .OrderByDescending(sm => sm.PerformedAt)
            .Select(sm => new StockMovementDto
            {
                Id = sm.Id,
                MedicineBatchId = sm.MedicineBatchId,
                MedicineName = sm.MedicineBatch.Medicine.Name,
                BatchNumber = sm.MedicineBatch.BatchNumber,
                MovementType = sm.MovementType,
                Quantity = sm.Quantity,
                PerformedAt = sm.PerformedAt,
                PerformedByUserId = sm.PerformedByUserId,
                PerformedByUserName = sm.PerformedByUser.Username,
                Reason = sm.Reason
            })
            .ToListAsync();
    }
}
