using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(bool? isActive = null);
    Task<SupplierDto?> GetSupplierByIdAsync(int id);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto createSupplierDto);
    Task UpdateSupplierAsync(UpdateSupplierDto updateSupplierDto);
    Task DeleteSupplierAsync(int id);
}
