using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Utilities;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentUserService currentUserService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var cached = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(CacheKeyBuilder.AllCategories());

        if (cached != null)
        {
            return cached;
        }

        var categories = await _unitOfWork.Categories.GetAllAsync();
        var result = _mapper.Map<List<CategoryDto>>(categories);

        await _cacheService.SetAsync(CacheKeyBuilder.AllCategories(), result, TimeSpan.FromHours(1));

        return result;
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null) return null;

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
    {
        var category = _mapper.Map<Category>(createCategoryDto);
        category.CreatedBy = _currentUserService.GetCurrentUsername();

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveAsync();
        await InvalidateCacheAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task UpdateCategoryAsync(UpdateCategoryDto updateCategoryDto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(updateCategoryDto.Id)
            ?? throw new Exception("Category not found");

        category.Name = updateCategoryDto.Name;
        category.Description = updateCategoryDto.Description;
        category.IsActive = updateCategoryDto.IsActive;
        // Handled by AuditableEntityInterceptor
        // category.UpdatedAt = DateTime.UtcNow;
        // category.UpdatedBy = _currentUserService.GetCurrentUsername();

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveAsync();
        await InvalidateCacheAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new Exception("Category not found");

        category.IsActive = false;
        // Handled by AuditableEntityInterceptor
        // category.UpdatedAt = DateTime.UtcNow;
        // category.UpdatedBy = _currentUserService.GetCurrentUsername();

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveAsync();
        await InvalidateCacheAsync();
    }

    private async Task InvalidateCacheAsync()
    {
        await _cacheService.RemoveAsync(CacheKeyBuilder.AllCategories());
    }
}
