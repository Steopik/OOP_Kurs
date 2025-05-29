namespace Auth.Service.DTOs;

public class AdminCreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
    public bool IsSuperUser { get; set; } = false;
}