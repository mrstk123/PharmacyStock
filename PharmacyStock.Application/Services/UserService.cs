using AutoMapper;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        var users = await _unitOfWork.Users.FindAsync(u => true, u => u.Role);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, u => u.Role);
        if (user == null) return null;

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Check if username exists
        var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Username == createUserDto.Username);
        if (existingUsers.Any())
        {
            throw new InvalidOperationException($"Username '{createUserDto.Username}' is already taken.");
        }

        // Generate random password
        string randomPassword = GenerateRandomPassword();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

        var user = new User
        {
            Username = createUserDto.Username,
            PasswordHash = passwordHash,
            Email = createUserDto.Email,
            FullName = createUserDto.FullName,
            RoleId = createUserDto.RoleId,
            IsActive = true
            // Handled by AuditableEntityInterceptor
            // CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveAsync();

        // Send email with credentials
        await _emailService.SendEmailAsync(user.Email, "Welcome to Pharmacy Stock",
            $"Your account has been created.\n\nUsername: {user.Username}\nPassword: {randomPassword}\n\nPlease login and change your password immediately.");

        return (await GetUserByIdAsync(user.Id))!;
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");

        if (updateUserDto.RoleId.HasValue)
        {
            user.RoleId = updateUserDto.RoleId.Value;
        }

        if (updateUserDto.Email != null) user.Email = updateUserDto.Email;
        if (updateUserDto.FullName != null) user.FullName = updateUserDto.FullName;
        if (updateUserDto.IsActive.HasValue) user.IsActive = updateUserDto.IsActive.Value;

        // Handled by AuditableEntityInterceptor
        // user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        return (await GetUserByIdAsync(user.Id))!;
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");

        user.IsActive = false;
        // Handled by AuditableEntityInterceptor
        // user.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();
    }

    public async Task ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"User with ID {id} not found.");

        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
        // Handled by AuditableEntityInterceptor
        // user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        if (!string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendEmailAsync(user.Email, "Password Changed", "Your password has been changed successfully.");
        }
    }

    private static string GenerateRandomPassword(int length = 12)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
        var random = new Random();
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = validChars[random.Next(validChars.Length)];
        }
        return new string(chars);
    }
}
