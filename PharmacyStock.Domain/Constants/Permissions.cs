namespace PharmacyStock.Domain.Constants;

public static class PermissionConstants
{
    // Dashboard
    public const string DashboardView = "Dashboard.View";

    // Medicines
    public const string MedicinesView = "Medicines.View";
    public const string MedicinesCreate = "Medicines.Create";
    public const string MedicinesEdit = "Medicines.Edit";
    public const string MedicinesDelete = "Medicines.Delete";

    // Categories
    public const string CategoriesView = "Categories.View";
    public const string CategoriesCreate = "Categories.Create";
    public const string CategoriesEdit = "Categories.Edit";
    public const string CategoriesDelete = "Categories.Delete";

    // Suppliers
    public const string SuppliersView = "Suppliers.View";
    public const string SuppliersCreate = "Suppliers.Create";
    public const string SuppliersEdit = "Suppliers.Edit";
    public const string SuppliersDelete = "Suppliers.Delete";

    // Stock (Batches & Inventory)
    public const string StockView = "Stock.View";
    public const string StockCreate = "Stock.Create";
    public const string StockEdit = "Stock.Edit";
    public const string StockExpiryView = "Stock.ExpiryView";

    // Stock Operations (Transactional)
    public const string StockDispense = "Stock.Dispense";
    public const string StockAdjust = "Stock.Adjust";
    public const string StockDispose = "Stock.Dispose";
    public const string StockReturn = "Stock.Return";
    public const string StockQuarantine = "Stock.Quarantine";

    // Stock Movements
    public const string StockMovementsView = "StockMovements.View";

    // Expiry Rules
    public const string ExpiryRulesView = "ExpiryRules.View";
    public const string ExpiryRulesCreate = "ExpiryRules.Create";
    public const string ExpiryRulesEdit = "ExpiryRules.Edit";
    public const string ExpiryRulesDelete = "ExpiryRules.Delete";

    // Users
    public const string UsersView = "Users.View";
    public const string UsersCreate = "Users.Create";
    public const string UsersEdit = "Users.Edit";
    public const string UsersDelete = "Users.Delete";


    // Roles
    public const string RolesView = "Roles.View";
    public const string RolesCreate = "Roles.Create";
    public const string RolesEdit = "Roles.Edit";
    public const string RolesDelete = "Roles.Delete";

    // Permissions
    public const string PermissionsView = "Permissions.View";
    public const string PermissionsAssign = "Permissions.Assign";
}
