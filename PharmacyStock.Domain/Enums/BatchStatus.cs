namespace PharmacyStock.Domain.Enums;

/// <summary>
/// Represents the status of a medicine batch
/// </summary>
public enum BatchStatus
{
    /// <summary>
    /// Ready for sale - batch has quantity and is not expired
    /// </summary>
    Active = 0,

    /// <summary>
    /// Stopped manually (Recall, Damage, Inspection)
    /// </summary>
    Quarantined = 1,

    /// <summary>
    /// Date passed, still has quantity (Needs Disposal)
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Quantity is 0 (History only). Used/sold completely before expiry
    /// </summary>
    Depleted = 3,

    /// <summary>
    /// Batch closed/removed from inventory (returned to supplier, disposed, written off)
    /// </summary>
    Closed = 4
}
