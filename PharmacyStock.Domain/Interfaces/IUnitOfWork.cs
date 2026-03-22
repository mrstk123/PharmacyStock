using PharmacyStock.Domain.Entities;

namespace PharmacyStock.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Category> Categories { get; }
    IGenericRepository<ExpiryRule> ExpiryRules { get; }
    IGenericRepository<Medicine> Medicines { get; }
    IGenericRepository<MedicineBatch> MedicineBatches { get; }
    IGenericRepository<StockMovement> StockMovements { get; }
    IGenericRepository<Supplier> Suppliers { get; }
    IGenericRepository<User> Users { get; }
    IGenericRepository<Permission> Permissions { get; }
    IGenericRepository<RolePermission> RolePermissions { get; }
    IGenericRepository<Notification> Notifications { get; }
    IGenericRepository<Role> Roles { get; }
    IGenericRepository<StockAudit> StockAudits { get; }

    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}

// Should place IGenericRepository and IUnitOfWork under Domain project because If the interface references Domain entities → it belongs in Domain.
// Moving them to Application makes more sense when You follow strict DDD where Domain has no repository abstractions.