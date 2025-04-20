namespace AuthService.Models;

public class PendingRegistration
{
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string ConfirmationCode { get; set; } = null!;
    public DateTime Expiration { get; set; }
}
