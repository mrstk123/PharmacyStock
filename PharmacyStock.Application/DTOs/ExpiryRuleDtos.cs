namespace PharmacyStock.Application.DTOs;

public class ExpiryRuleDto
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; } = "Global";
    public int WarningDays { get; set; }
    public int CriticalDays { get; set; }
    public bool IsActive { get; set; }
}

public class CreateExpiryRuleDto
{
    public int? CategoryId { get; set; }
    public int WarningDays { get; set; }
    public int CriticalDays { get; set; }
    public bool IsActive { get; set; } = true;
}
