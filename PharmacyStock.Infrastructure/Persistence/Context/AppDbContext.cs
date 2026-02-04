using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PharmacyStock.Domain.Entities;

namespace PharmacyStock.Infrastructure.Persistence.Context;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ExpiryRule> ExpiryRules { get; set; }

    public virtual DbSet<Medicine> Medicines { get; set; }

    public virtual DbSet<MedicineBatch> MedicineBatches { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<StockAudit> StockAudits { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; } = null!;

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Categories_Name").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
        });

        modelBuilder.Entity<ExpiryRule>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.ExpiryRules)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_ExpiryRules_Categories");
        });

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasIndex(e => e.MedicineCode, "UQ_Medicines_MedicineCode").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.GenericName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Manufacturer).HasMaxLength(100);
            entity.Property(e => e.MedicineCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.StorageCondition).HasMaxLength(100);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Medicines)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Medicines_Categories");
        });

        modelBuilder.Entity<MedicineBatch>(entity =>
        {
            entity.HasIndex(e => new { e.ExpiryDate, e.Status }, "IX_MedicineBatches_ExpiryDate_Status");

            // Index for stock queries (commonly used in GetStockCheck, GetAlternatives)
            entity.HasIndex(e => new { e.MedicineId, e.IsActive, e.CurrentQuantity }, "IX_MedicineBatches_MedicineId_IsActive_CurrentQuantity");

            entity.Property(e => e.BatchNumber).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ReceivedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasOne(d => d.Medicine).WithMany(p => p.MedicineBatches)
                .HasForeignKey(d => d.MedicineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicineBatches_Medicines");

            entity.HasOne(d => d.Supplier).WithMany(p => p.MedicineBatches)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicineBatches_Suppliers");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Permissions_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name, "UQ_Roles_Name").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }, "UQ_RolePermissions_RoleId_PermissionId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RolePermissions_Roles");


            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RolePermissions_Permissions");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            // Index for movement history queries
            entity.HasIndex(e => new { e.PerformedAt, e.MedicineBatchId }, "IX_StockMovements_PerformedAt_MedicineBatchId");

            entity.Property(e => e.MovementType).HasMaxLength(20);
            entity.Property(e => e.PerformedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Reason).HasMaxLength(255);
            entity.Property(e => e.ReferenceNo).HasMaxLength(50);

            entity.HasOne(d => d.MedicineBatch).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.MedicineBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_MedicineBatches");

            entity.HasOne(d => d.PerformedByUser).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.PerformedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Users");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.SupplierCode, "UQ_Suppliers_SupplierCode").IsUnique();

            entity.Property(e => e.ContactInfo).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SupplierCode).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RefreshToken).HasMaxLength(255);
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            // Index for system alert queries (commonly used in GetAlerts, GenerateNotifications)
            entity.HasIndex(e => new { e.IsSystemAlert, e.IsActionTaken, e.Type, e.RelatedEntityType }, "IX_Notifications_SystemAlert_ActionTaken_Type_EntityType");

            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
        // SeedData(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /*
    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2026, 1, 28, 0, 0, 0, DateTimeKind.Utc);
        const string systemUser = "SYSTEM";

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Tablets", Description = "Solid dosage forms", IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Category { Id = 2, Name = "Syrups", Description = "Liquid dosage forms", IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser }
        );

        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, SupplierCode = "SUP001", Name = "MedSupply Co", ContactInfo = "medsupply@example.com | +1-555-0101", IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Supplier { Id = 2, SupplierCode = "SUP002", Name = "HealthPlus Distributors", ContactInfo = "healthplus@example.com | +1-555-0102", IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Supplier { Id = 3, SupplierCode = "SUP003", Name = "CareWell Pharma", ContactInfo = "carewell@example.com | +1-555-0103", IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Supplier { Id = 4, SupplierCode = "SUP004", Name = "PrimeMed Suppliers", ContactInfo = "primemed@example.com | +1-555-0104", IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Supplier { Id = 5, SupplierCode = "SUP005", Name = "LifeLine Medicals", ContactInfo = "lifeline@example.com | +1-555-0105", IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser }
        );

        modelBuilder.Entity<ExpiryRule>().HasData(
            new ExpiryRule { Id = 1, CategoryId = null, WarningDays = 90, CriticalDays = 30, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new ExpiryRule { Id = 2, CategoryId = 1, WarningDays = 180, CriticalDays = 60, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new ExpiryRule { Id = 3, CategoryId = 2, WarningDays = 60, CriticalDays = 15, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser }
        );

        modelBuilder.Entity<Medicine>().HasData(
            new Medicine { Id = 1, CategoryId = 1, MedicineCode = "TAB001", Name = "Paracetamol 500", GenericName = "Paracetamol", Manufacturer = "ABC Pharma", StorageCondition = "Store below 30°C", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 2, CategoryId = 1, MedicineCode = "TAB002", Name = "Calpol 500", GenericName = "Paracetamol", Manufacturer = "GSK", StorageCondition = "Store below 30°C", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 3, CategoryId = 2, MedicineCode = "SYP001", Name = "Paracetamol Syrup", GenericName = "Paracetamol", Manufacturer = "Cipla", StorageCondition = "Do not refrigerate", UnitOfMeasure = "Bottle", LowStockThreshold = 20, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 4, CategoryId = 1, MedicineCode = "TAB003", Name = "Amoxil 250", GenericName = "Amoxicillin", Manufacturer = "Pfizer", StorageCondition = "Store in a cool dry place", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 5, CategoryId = 1, MedicineCode = "TAB004", Name = "Mox 250", GenericName = "Amoxicillin", Manufacturer = "Sun Pharma", StorageCondition = "Store below 25°C", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 6, CategoryId = 1, MedicineCode = "TAB005", Name = "Cetzine 10", GenericName = "Cetirizine", Manufacturer = "GSK", StorageCondition = "Store below 25°C", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 7, CategoryId = 2, MedicineCode = "SYP002", Name = "Cetirizine Syrup", GenericName = "Cetirizine", Manufacturer = "Dr Reddy's", StorageCondition = "Store below 30°C", UnitOfMeasure = "Bottle", LowStockThreshold = 20, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 8, CategoryId = 1, MedicineCode = "TAB006", Name = "Brufen 400", GenericName = "Ibuprofen", Manufacturer = "Abbott", StorageCondition = "Store below 30°C", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 9, CategoryId = 1, MedicineCode = "TAB007", Name = "Ibugesic 400", GenericName = "Ibuprofen", Manufacturer = "Cipla", StorageCondition = "Store below 30°C", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 10, CategoryId = 2, MedicineCode = "SYP003", Name = "Ibuprofen Syrup", GenericName = "Ibuprofen", Manufacturer = "Abbott", StorageCondition = "Store below 25°C", UnitOfMeasure = "Bottle", LowStockThreshold = 20, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 11, CategoryId = 1, MedicineCode = "TAB008", Name = "Azee 500", GenericName = "Azithromycin", Manufacturer = "Cipla", StorageCondition = "Store below 30°C", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 12, CategoryId = 1, MedicineCode = "TAB009", Name = "Zithro 500", GenericName = "Azithromycin", Manufacturer = "FDC Ltd", StorageCondition = "Store in a cool dry place", UnitOfMeasure = "Strip", LowStockThreshold = 50, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser },
            new Medicine { Id = 13, CategoryId = 2, MedicineCode = "SYP004", Name = "Azithromycin Syrup", GenericName = "Azithromycin", Manufacturer = "Pfizer", StorageCondition = "Store below 25°C", UnitOfMeasure = "Bottle", LowStockThreshold = 20, IsActive = true, CreatedAt = now, CreatedBy = systemUser, UpdatedAt = now, UpdatedBy = systemUser }
        );
    }
    */

}
