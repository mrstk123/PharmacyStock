using BCrypt.Net;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace PharmacyStock.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void SeedUsers(AppDbContext context)
    {
        // Seed Roles first
        if (!context.Roles.Any())
        {
            var roles = new List<Role>
            {
                new Role { Name = Roles.Admin, Description = "Administrator with full access" },
                new Role { Name = Roles.Pharmacist, Description = "Pharmacist with limited access" }
            };
            context.Roles.AddRange(roles);
            context.SaveChanges();
        }

        var adminRole = context.Roles.FirstOrDefault(r => r.Name == Roles.Admin);
        var pharmacistRole = context.Roles.FirstOrDefault(r => r.Name == Roles.Pharmacist);

        var usersToObject = new List<User>();

        if (!context.Users.Any(u => u.Username == SystemConstants.SystemUsername))
        {
            var systemUser = new User
            {
                Id = SystemConstants.SystemUserId,
                Username = SystemConstants.SystemUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random, unguessable password
                RoleId = adminRole!.Id,
                IsActive = false, // Not for login, only for audit trails
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemConstants.SystemUsername,
                UpdatedBy = SystemConstants.SystemUsername,
                Email = "system@pharmacy.internal",
                FullName = "System (Automated Operations)"
            };
            usersToObject.Add(systemUser);
        }

        // Seed Admin
        if (!context.Users.Any(u => u.Username == "admin"))
        {
            var adminUser = new User
            {
                Id = 2,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                RoleId = adminRole!.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemConstants.SystemUsername,
                UpdatedBy = SystemConstants.SystemUsername,
                Email = "admin@pharmacy.com",
                FullName = "Admin"
            };
            usersToObject.Add(adminUser);
        }

        // Seed Pharmacist
        if (!context.Users.Any(u => u.Username == "pharmacist"))
        {
            var pharmacistUser = new User
            {
                Id = 3,
                Username = "pharmacist",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pharmacist"),
                RoleId = pharmacistRole!.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = SystemConstants.SystemUsername,
                UpdatedBy = SystemConstants.SystemUsername,
                Email = "pharmacist@pharmacy.com",
                FullName = "Pharmacist"
            };
            usersToObject.Add(pharmacistUser);
        }

        if (usersToObject.Count > 0)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                context.Users.AddRange(usersToObject);

                // Allow explicit values for identity column
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users ON");

                context.SaveChanges();

                // Reset to default behavior
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users OFF");

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public static void SeedPermissions(AppDbContext context)
    {
        var allPermissions = new[]
        {
            // Medicines
            new Permission { Name = PermissionConstants.MedicinesView, Description = "View medicines" },
            new Permission { Name = PermissionConstants.MedicinesCreate, Description = "Create medicines" },
            new Permission { Name = PermissionConstants.MedicinesEdit, Description = "Edit medicines" },
            new Permission { Name = PermissionConstants.MedicinesDelete, Description = "Delete medicines" },

            // Categories
            new Permission { Name = PermissionConstants.CategoriesView, Description = "View categories" },
            new Permission { Name = PermissionConstants.CategoriesCreate, Description = "Create categories" },
            new Permission { Name = PermissionConstants.CategoriesEdit, Description = "Edit categories" },
            new Permission { Name = PermissionConstants.CategoriesDelete, Description = "Delete categories" },

            // Suppliers
            new Permission { Name = PermissionConstants.SuppliersView, Description = "View suppliers" },
            new Permission { Name = PermissionConstants.SuppliersCreate, Description = "Create suppliers" },
            new Permission { Name = PermissionConstants.SuppliersEdit, Description = "Edit suppliers" },
            new Permission { Name = PermissionConstants.SuppliersDelete, Description = "Delete suppliers" },

            // Stock (Batches & Inventory)
            new Permission { Name = PermissionConstants.StockView, Description = "View stock and batches" },
            new Permission { Name = PermissionConstants.StockCreate, Description = "Create stock batches" },
            new Permission { Name = PermissionConstants.StockEdit, Description = "Edit stock batches" },
            new Permission { Name = PermissionConstants.StockExpiryView, Description = "View expiry management" },

            // Stock Operations
            new Permission { Name = PermissionConstants.StockDispense, Description = "Dispense stock" },
            new Permission { Name = PermissionConstants.StockAdjust, Description = "Adjust stock quantity/price" },
            new Permission { Name = PermissionConstants.StockDispose, Description = "Dispose expired stock" },
            new Permission { Name = PermissionConstants.StockReturn, Description = "Return stock to supplier" },
            new Permission { Name = PermissionConstants.StockQuarantine, Description = "Quarantine/Unquarantine stock" },

            // Expiry Rules
            new Permission { Name = PermissionConstants.ExpiryRulesView, Description = "View expiry rules" },
            new Permission { Name = PermissionConstants.ExpiryRulesCreate, Description = "Create expiry rules" },
            new Permission { Name = PermissionConstants.ExpiryRulesEdit, Description = "Edit expiry rules" },
            new Permission { Name = PermissionConstants.ExpiryRulesDelete, Description = "Delete expiry rules" },

            // Reports (Stock Movements)
            new Permission { Name = PermissionConstants.StockMovementsView, Description = "View stock movement reports" },

            // Users
            new Permission { Name = PermissionConstants.UsersView, Description = "View users" },
            new Permission { Name = PermissionConstants.UsersCreate, Description = "Create users" },
            new Permission { Name = PermissionConstants.UsersEdit, Description = "Edit users" },
            new Permission { Name = PermissionConstants.UsersDelete, Description = "Delete users" },

            // Roles
            new Permission { Name = PermissionConstants.RolesView, Description = "View roles" },
            new Permission { Name = PermissionConstants.RolesCreate, Description = "Create roles" },
            new Permission { Name = PermissionConstants.RolesEdit, Description = "Edit roles" },
            new Permission { Name = PermissionConstants.RolesDelete, Description = "Delete roles" },

            // Permissions
            new Permission { Name = PermissionConstants.PermissionsView, Description = "View permissions" },
            new Permission { Name = PermissionConstants.PermissionsAssign, Description = "Assign permissions to roles" },

            // Dashboard
            new Permission { Name = PermissionConstants.DashboardView, Description = "View dashboard stats" }
        };

        // 1. Ensure Permissions Exist
        foreach (var p in allPermissions)
        {
            if (!context.Permissions.Any(x => x.Name == p.Name))
            {
                context.Permissions.Add(p);
            }
        }

        if (context.ChangeTracker.HasChanges())
        {
            context.SaveChanges();
        }

        // 2. Map Roles to Permissions
        var adminRole = context.Roles.FirstOrDefault(r => r.Name == Roles.Admin);
        var pharmacistRole = context.Roles.FirstOrDefault(r => r.Name == Roles.Pharmacist);

        if (adminRole != null)
        {
            // Admin gets ALL
            var adminPermissions = context.Permissions.ToList();
            EnsureRolePermissions(context, adminRole.Id, adminPermissions);
        }

        if (pharmacistRole != null)
        {
            // Pharmacist gets:
            // - Medicines.* (All)
            // - Categories.View
            // - Suppliers.View
            // - Suppliers.View
            // - Stock.* (All - covers View, Create Batch, Operations)
            // - ExpiryRules.View
            // - Reports.View
            // - Dashboard.View
            var pharmacistAllowedPrefixes = new[] { "Medicines.", "Stock." };
            var pharmacistAllowedExact = new[] { "Categories.View", "Suppliers.View", "ExpiryRules.View", "StockMovements.View", "Dashboard.View" };

            var pharmacistPermissions = context.Permissions
                .AsEnumerable() // Client-side filtering as 'StartsWith' with list is tricky in EF
                .Where(p => pharmacistAllowedPrefixes.Any(prefix => p.Name.StartsWith(prefix)) ||
                            pharmacistAllowedExact.Contains(p.Name))
                .ToList();

            EnsureRolePermissions(context, pharmacistRole.Id, pharmacistPermissions);
        }

        if (context.ChangeTracker.HasChanges())
        {
            context.SaveChanges();
        }
    }

    private static void EnsureRolePermissions(AppDbContext context, int roleId, List<Permission> permissions)
    {
        foreach (var p in permissions)
        {
            if (!context.RolePermissions.Any(rp => rp.RoleId == roleId && rp.PermissionId == p.Id))
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = p.Id
                });
            }
        }
    }
}
