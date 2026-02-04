namespace PharmacyStock.Domain.Enums;

/// <summary>
/// Represents the type/category of a notification
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// General information
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning that requires attention
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Critical alert requiring immediate action
    /// </summary>
    Critical = 2,

    /// <summary>
    /// Stock-related notifications (low stock, out of stock)
    /// </summary>
    StockAlert = 3,

    /// <summary>
    /// Expiry-related notifications
    /// </summary>
    ExpiryAlert = 4
}
