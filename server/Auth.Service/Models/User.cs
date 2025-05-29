namespace Auth.Service.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public bool IsSuperUser { get; set; } = false;
    public long TokenVersion { get; set; } = 1;
}