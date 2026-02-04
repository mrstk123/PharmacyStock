using Microsoft.Extensions.DependencyInjection;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Services;

namespace PharmacyStock.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(System.Reflection.Assembly.GetExecutingAssembly());

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IMedicineService, MedicineService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IStockOperationService, StockOperationService>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IExpiryRuleService, ExpiryRuleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationGeneratorService, NotificationGeneratorService>();
        services.AddScoped<BatchStatusUpdateService>();

        return services;
    }
}
