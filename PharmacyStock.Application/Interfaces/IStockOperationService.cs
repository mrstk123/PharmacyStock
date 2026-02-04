using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IStockOperationService
{
    Task RemoveExpiredStockAsync(RemoveExpiredStockDto removeDto);
    Task ReturnToSupplierAsync(ReturnToSupplierDto returnDto);
}
