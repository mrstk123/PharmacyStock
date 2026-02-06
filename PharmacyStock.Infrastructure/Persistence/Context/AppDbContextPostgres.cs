using Microsoft.EntityFrameworkCore;
using PharmacyStock.Domain.Entities;

namespace PharmacyStock.Infrastructure.Persistence.Context;

/// <summary>
/// PostgreSQL-specific DbContext. Inherits from AppDbContext to reuse all entity configurations.
/// This allows separate migration histories for PostgreSQL.
/// </summary>
public class AppDbContextPostgres : AppDbContext
{
    public AppDbContextPostgres(DbContextOptions<AppDbContextPostgres> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base to get common entity configurations
        base.OnModelCreating(modelBuilder);

        // Apply PostgreSQL-specific configurations
        modelBuilder.Entity<MedicineBatch>(entity =>
        {
            // RowVersion is NOT a concurrency token and optional in Postgres
            entity.Property(e => e.RowVersion)
                .IsConcurrencyToken(false)
                .IsRequired(false);

            // Use the native Postgres 'xmin' for actual concurrency
            entity.Property<uint>("xmin")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        });
        
        // Configure all DateTime properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                // DateTime to timestamp with time zone
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }

                // Convert SQL Server getdate() to PostgreSQL CURRENT_TIMESTAMP
                if (property.GetDefaultValueSql() == "(getdate())")
                {
                    property.SetDefaultValueSql(
                        property.ClrType == typeof(DateOnly) || property.ClrType == typeof(DateOnly?)
                            ? "CURRENT_DATE"
                            : "CURRENT_TIMESTAMP");
                }
            }
        }
    }
}
