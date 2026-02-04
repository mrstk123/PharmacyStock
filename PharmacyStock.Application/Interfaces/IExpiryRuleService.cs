using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IExpiryRuleService
{
    Task<List<ExpiryRuleDto>> GetExpiryRulesAsync();
    Task<ExpiryRuleDto> GetExpiryRuleByIdAsync(int id);
    Task<ExpiryRuleDto> CreateExpiryRuleAsync(CreateExpiryRuleDto dto);
    Task<ExpiryRuleDto> UpdateExpiryRuleAsync(int id, CreateExpiryRuleDto dto);
    Task DeleteExpiryRuleAsync(int id);
}
