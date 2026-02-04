namespace PharmacyStock.Domain.Constants;

/// <summary>
/// System-wide constant values
/// </summary>
public static class SystemConstants
{
    /// <summary>
    /// System user ID used for automated operations when no user context is available.
    /// </summary>
    public const int SystemUserId = 1;

    /// <summary>
    /// System username for audit trails
    /// </summary>
    public const string SystemUsername = "System";

    /// <summary>
    /// Stock alert threshold percentages for priority calculation.
    /// When stock falls below LowStockThreshold, this determines priority levels.
    /// </summary>
    public static class StockAlertThresholds
    {
        /// <summary>
        /// Critical level: 50% of LowStockThreshold. Below this = Priority 4 (High)
        /// </summary>
        public const double CriticalPercentage = 0.5;

        /// <summary>
        /// Priority values for stock alerts
        /// </summary>
        public static class Priority
        {
            public const int OutOfStock = 5;        // Stock = 0
            public const int Critical = 4;          // Stock < 50% of threshold
            public const int Warning = 3;           // Stock >= 50% but below threshold
        }
    }
}

