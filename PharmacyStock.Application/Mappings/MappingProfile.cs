using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Domain.Entities;

using PharmacyStock.Domain.Enums;

namespace PharmacyStock.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Medicine Mappings
        CreateMap<Medicine, MedicineDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Unknown"));

        CreateMap<CreateMedicineDto, Medicine>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Category Mappings
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Supplier Mappings
        CreateMap<Supplier, SupplierDto>();
        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Inventory/Batch Mappings
        CreateMap<MedicineBatch, MedicineBatchDto>()
            .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : "Unknown"))
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Unknown"));

        CreateMap<CreateMedicineBatchDto, MedicineBatch>()
            .ForMember(dest => dest.CurrentQuantity, opt => opt.MapFrom(src => src.InitialQuantity))
            .ForMember(dest => dest.ReceivedDate, opt => opt.MapFrom(src => DateOnly.FromDateTime(DateTime.UtcNow)))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => BatchStatus.Active))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Stock Check Mappings
        CreateMap<MedicineBatch, MedicineBatchDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : "Unknown"));

        // User Mappings
        CreateMap<User, UserDto>()
             .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : "Unknown"));

        // Role Mappings
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty));
        CreateMap<CreateRoleDto, Role>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? true))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        // Permission Mappings
        CreateMap<Permission, PermissionDto>()
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty));

        // ExpiryRule Mappings
        CreateMap<ExpiryRule, ExpiryRuleDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src =>
                src.CategoryId.HasValue && src.Category != null ? src.Category.Name : "Global"));

        // Notification Mappings
        CreateMap<Notification, NotificationDto>();
    }
}
