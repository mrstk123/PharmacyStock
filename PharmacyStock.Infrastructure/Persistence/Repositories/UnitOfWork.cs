using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;
using PharmacyStock.Infrastructure.Persistence.Context;

namespace PharmacyStock.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<ExpiryRule>? _expiryRules;
    private IGenericRepository<Medicine>? _medicines;
    private IGenericRepository<MedicineBatch>? _medicineBatches;
    private IGenericRepository<StockMovement>? _stockMovements;
    private IGenericRepository<Supplier>? _suppliers;
    private IGenericRepository<User>? _users;
    private IGenericRepository<Permission>? _permissions;
    private IGenericRepository<RolePermission>? _rolePermissions;
    private IGenericRepository<Notification>? _notifications;
    private IGenericRepository<Role>? _roles;
    private IGenericRepository<StockAudit>? _stockAudits;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<Category> Categories =>
        _categories ??= new GenericRepository<Category>(_context);

    public IGenericRepository<ExpiryRule> ExpiryRules =>
        _expiryRules ??= new GenericRepository<ExpiryRule>(_context);

    public IGenericRepository<Medicine> Medicines =>
        _medicines ??= new GenericRepository<Medicine>(_context);

    public IGenericRepository<MedicineBatch> MedicineBatches =>
        _medicineBatches ??= new GenericRepository<MedicineBatch>(_context);

    public IGenericRepository<StockMovement> StockMovements =>
        _stockMovements ??= new GenericRepository<StockMovement>(_context);

    public IGenericRepository<Supplier> Suppliers =>
        _suppliers ??= new GenericRepository<Supplier>(_context);

    public IGenericRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    public IGenericRepository<Permission> Permissions =>
        _permissions ??= new GenericRepository<Permission>(_context);

    public IGenericRepository<RolePermission> RolePermissions =>
        _rolePermissions ??= new GenericRepository<RolePermission>(_context);

    public IGenericRepository<Notification> Notifications =>
        _notifications ??= new GenericRepository<Notification>(_context);

    public IGenericRepository<Role> Roles =>
        _roles ??= new GenericRepository<Role>(_context);

    public IGenericRepository<StockAudit> StockAudits =>
        _stockAudits ??= new GenericRepository<StockAudit>(_context);

    public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
