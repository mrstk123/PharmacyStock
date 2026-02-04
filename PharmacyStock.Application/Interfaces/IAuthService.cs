using PharmacyStock.Application.DTOs;

namespace PharmacyStock.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest);
    Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenRequest);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
}
