using EWeaponRegistry.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace EWeaponRegistry.Application.DTOs.Admin;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }
}

public class UpdateUserRoleRequest
{
    [Required]
    public UserRole Role { get; set; }
}

public class UpdateUserStatusRequest
{
    [Required]
    public bool IsActive { get; set; }
}
