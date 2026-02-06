using Microsoft.AspNetCore.Mvc;
using PharmacyStock.Application.DTOs;
using PharmacyStock.Application.Interfaces;

namespace PharmacyStock.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, ILogger<AuthController> logger, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _authService = authService;
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto loginRequest)
    {
        try
        {
            var result = await _authService.LoginAsync(loginRequest);
            _logger.LogInformation("User {Username} logged in successfully", result!.Username);

            SetTokenCookie(result.AccessToken, result.RefreshToken, result.IsPersistent, result.RefreshTokenExpiration);

            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Unauthorized(new { message = "Incorrect username" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Incorrect password" });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponseDto>> Refresh(RefreshTokenDto? refreshTokenRequest)
    {
        var refreshToken = refreshTokenRequest?.RefreshToken ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "No refresh token provided" });
        }

        var result = await _authService.RefreshTokenAsync(new RefreshTokenDto { RefreshToken = refreshToken });
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        SetTokenCookie(result.AccessToken, result.RefreshToken, result.IsPersistent, result.RefreshTokenExpiration);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
        return Ok(new { message = "If the email exists, a password reset token has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            await _authService.ResetPasswordAsync(resetPasswordDto);
            return Ok(new { message = "Password has been reset successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    private void SetTokenCookie(string accessToken, string refreshToken, bool isPersistent, DateTime refreshTokenExpiry)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };

        if (isPersistent)
        {
            var expiryMinutes = double.Parse(_configuration["Jwt:DurationInMinutes"] ?? "60");
            cookieOptions.Expires = DateTime.UtcNow.AddMinutes(expiryMinutes);
        }
        else
        {
            cookieOptions.Expires = null; // Session cookie
        }

        Response.Cookies.Append("accessToken", accessToken, cookieOptions);

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        };

        if (isPersistent)
        {
            refreshCookieOptions.Expires = refreshTokenExpiry;
        }
        else
        {
            refreshCookieOptions.Expires = null; // Session cookie for non remember-me
        }

        Response.Cookies.Append("refreshToken", refreshToken, refreshCookieOptions);
    }
}
