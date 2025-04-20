using System.ComponentModel.DataAnnotations;

namespace AuthService.Models;

public class RegisterModel
{
    [Required]
    [UsernameValidation]
    public string Username { get; set; } = null!;

    [Required]
    [PasswordValidation]
    public string Password { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}
