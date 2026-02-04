using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;
using PharmacyStock.Domain.Entities;
using PharmacyStock.Domain.Interfaces;

namespace PharmacyStock.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(IUnitOfWork unitOfWork, IJwtProvider jwtProvider, IConfiguration configuration, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequest)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Username == loginRequest.Username && u.IsActive, u => u.Role);
        var user = users.FirstOrDefault()
            ?? throw new KeyNotFoundException("Incorrect username");

        if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Incorrect password");
        }

        var roleName = user.Role?.Name ?? string.Empty;
        var rolePermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == user.RoleId, rp => rp.Permission);
        var permissionNames = rolePermissions.Select(rp => rp.Permission.Name).ToList();

        var accessToken = _jwtProvider.GenerateAccessToken(user, roleName, permissionNames, loginRequest.RememberMe);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = GetRefreshTokenExpiry(loginRequest.RememberMe);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Id = user.Id,
            Username = user.Username,
            Role = roleName,
            IsPersistent = loginRequest.RememberMe,
            RefreshTokenExpiration = user.RefreshTokenExpiryTime.Value
        };
        return response;
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenDto refreshTokenRequest)
    {
        // Validate the expired access token to extract userId and isPersistent
        var tokenInfo = _jwtProvider.ValidateExpiredAccessToken(refreshTokenRequest.AccessToken);
        if (tokenInfo == null)
        {
            return null; // Invalid access token
        }

        var (userId, isPersistent) = tokenInfo.Value;

        // Find user by ID and verify the refresh token matches
        var users = await _unitOfWork.Users.FindAsync(u => u.Id == userId && u.RefreshToken == refreshTokenRequest.RefreshToken && u.IsActive, u => u.Role);
        var user = users.FirstOrDefault();

        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }

        var roleName = user.Role?.Name ?? string.Empty;
        var rolePermissions = await _unitOfWork.RolePermissions.FindAsync(rp => rp.RoleId == user.RoleId, rp => rp.Permission);
        var permissionNames = rolePermissions.Select(rp => rp.Permission.Name).ToList();

        var newAccessToken = _jwtProvider.GenerateAccessToken(user, roleName, permissionNames, isPersistent);
        var newRefreshToken = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = GetRefreshTokenExpiry(isPersistent);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Id = user.Id,
            Username = user.Username,
            Role = roleName,
            IsPersistent = isPersistent,
            RefreshTokenExpiration = user.RefreshTokenExpiryTime.Value
        };
    }

    private DateTime GetRefreshTokenExpiry(bool isPersistent)
    {
        double expiryDays = isPersistent
            ? double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "3")
            : 1; // 1 Day for non-remember-me sessions
        return DateTime.UtcNow.AddDays(expiryDays);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.Email == email && u.IsActive);
        var user = users.FirstOrDefault();

        if (user == null)
        {
            // Do not reveal that the user does not exist
            return;
        }

        var token = Guid.NewGuid().ToString();
        user.ResetToken = token;
        user.ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(15);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        // Generate link to the frontend reset page
        var clientAppUrl = _configuration["ClientAppUrl"] ?? "http://localhost:4200";
        string resetLink = $"{clientAppUrl}/reset-password?token={token}";
        await _emailService.SendEmailAsync(email, "Password Reset Request",
            $"Please click the following link to reset your password: {resetLink}\n\nThis link expires in 15 minutes.");
    }

    public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var users = await _unitOfWork.Users.FindAsync(u => u.ResetToken == resetPasswordDto.Token);
        var user = users.FirstOrDefault();

        if (user == null || user.ResetTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new ArgumentException("Invalid or expired token");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiryTime = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveAsync();

        if (!string.IsNullOrEmpty(user.Email))
        {
            await _emailService.SendEmailAsync(user.Email, "Password Reset Successful", "Your password has been successfully reset. If you did not initiate this change, please contact support immediately.");
        }
    }
}
