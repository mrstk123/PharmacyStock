namespace PharmacyStock.Application.Utilities;

/// <summary>
/// Centralized cache key generation to avoid magic strings and ensure consistency.
/// </summary>
public static class CacheKeyBuilder
{
    // Prefixes for different entity types
    private const string StockCheckPrefix = "stock_check_";
    private const string MedicinePrefix = "medicine_";
    private const string CategoryPrefix = "category_";
    private const string AllMedicinesKey = "all_medicines";
    private const string AllCategoriesKey = "all_categories";
    private const string AllSuppliersKey = "all_suppliers";
    private const string DashboardAlertsKey = "dashboard_alerts";

    /// <summary>
    /// Generates cache key for stock check data.
    /// </summary>
    public static string StockCheck(int medicineId) => $"{StockCheckPrefix}{medicineId}";

    /// <summary>
    /// Generates cache key for a single medicine.
    /// </summary>
    public static string Medicine(int id) => $"{MedicinePrefix}{id}";

    /// <summary>
    /// Generates cache key for all medicines list.
    /// </summary>
    public static string AllMedicines() => AllMedicinesKey;

    /// <summary>
    /// Generates cache key for a single category.
    /// </summary>
    public static string Category(int id) => $"{CategoryPrefix}{id}";

    /// <summary>
    /// Generates cache key for all categories list.
    /// </summary>
    public static string AllCategories() => AllCategoriesKey;

    /// <summary>
    /// Generates cache key for all suppliers list.
    /// </summary>
    public static string AllSuppliers() => AllSuppliersKey;

    /// <summary>
    /// Generates cache key for dashboard alerts.
    /// </summary>
    public static string DashboardAlerts() => DashboardAlertsKey;
}
