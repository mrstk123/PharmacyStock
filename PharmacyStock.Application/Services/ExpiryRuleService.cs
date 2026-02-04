using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Constants;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class ExpiryRuleService : IExpiryRuleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpiryRuleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ExpiryRuleDto>> GetExpiryRulesAsync()
    {
        var rules = await _unitOfWork.ExpiryRules.FindAsync(r => true);
        return _mapper.Map<List<ExpiryRuleDto>>(rules);
    }

    public async Task<ExpiryRuleDto> GetExpiryRuleByIdAsync(int id)
    {
        var rule = await _unitOfWork.ExpiryRules.GetByIdAsync(id)
            ?? throw new Exception("Expiry Rule not found");

        return _mapper.Map<ExpiryRuleDto>(rule);
    }

    public async Task<ExpiryRuleDto> CreateExpiryRuleAsync(CreateExpiryRuleDto dto)
    {
        var existingRules = await _unitOfWork.ExpiryRules.FindAsync(r => r.CategoryId == dto.CategoryId && r.IsActive);
        if (existingRules.Any())
            throw new Exception(dto.CategoryId.HasValue
                ? "An active rule for this category already exists"
                : "An active global rule already exists");

        var rule = new ExpiryRule
        {
            CategoryId = dto.CategoryId,
            WarningDays = dto.WarningDays,
            CriticalDays = dto.CriticalDays,
            IsActive = dto.IsActive
            // Handled by AuditableEntityInterceptor
            // CreatedAt = DateTime.UtcNow,
            // CreatedBy = SystemConstants.SystemUsername
        };

        await _unitOfWork.ExpiryRules.AddAsync(rule);
        await _unitOfWork.SaveAsync();

        return await GetExpiryRuleByIdAsync(rule.Id);
    }

    public async Task<ExpiryRuleDto> UpdateExpiryRuleAsync(int id, CreateExpiryRuleDto dto)
    {
        var rule = await _unitOfWork.ExpiryRules.GetByIdAsync(id)
            ?? throw new Exception("Expiry Rule not found");

        // Check for duplicates if category changed OR if we are reactivating a rule
        if (rule.CategoryId != dto.CategoryId || (dto.IsActive && !rule.IsActive))
        {
            var existingRules = await _unitOfWork.ExpiryRules.FindAsync(r => r.CategoryId == dto.CategoryId && r.IsActive && r.Id != id);
            if (existingRules.Any())
                throw new Exception(dto.CategoryId.HasValue
                    ? "An active rule for this category already exists"
                    : "An active global rule already exists");
        }

        rule.CategoryId = dto.CategoryId;
        rule.WarningDays = dto.WarningDays;
        rule.CriticalDays = dto.CriticalDays;
        rule.IsActive = dto.IsActive;
        // Handled by AuditableEntityInterceptor
        // rule.UpdatedAt = DateTime.UtcNow;
        // rule.UpdatedBy = SystemConstants.SystemUsername;

        _unitOfWork.ExpiryRules.Update(rule);
        await _unitOfWork.SaveAsync();

        return await GetExpiryRuleByIdAsync(rule.Id);
    }

    public async Task DeleteExpiryRuleAsync(int id)
    {
        var rule = await _unitOfWork.ExpiryRules.GetByIdAsync(id)
            ?? throw new Exception("Expiry Rule not found");

        rule.IsActive = false;
        // Handled by AuditableEntityInterceptor
        // rule.UpdatedAt = DateTime.UtcNow;
        // rule.UpdatedBy = SystemConstants.SystemUsername;

        _unitOfWork.ExpiryRules.Update(rule);
        await _unitOfWork.SaveAsync();
    }
}
