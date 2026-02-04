using System.ComponentModel.DataAnnotations;

namespace PharmacyStock.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;



    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public int RoleId { get; set; }
}

public class UpdateUserDto
{
    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    public int? RoleId { get; set; }

    public bool? IsActive { get; set; }


}

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;
}
