using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmacyStock.Domain.Interfaces;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Infrastructure.Persistence.Context;
using PharmacyStock.Infrastructure.Persistence.Repositories;
using PharmacyStock.Infrastructure.Services;

namespace PharmacyStock.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<Persistence.Interceptors.AuditableEntityInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<Persistence.Interceptors.AuditableEntityInterceptor>();
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                   .AddInterceptors(interceptor);
        });

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStockMovementSearcher, StockMovementSearcher>();

        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<ICacheService, RedisCacheService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("RedisConnection");
            options.InstanceName = "PharmacyStock_";
        });

        return services;
    }
}
