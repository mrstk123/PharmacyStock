using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task ChangePasswordAsync(int id, ChangePasswordDto changePasswordDto);
    Task DeleteUserAsync(int id);
}
