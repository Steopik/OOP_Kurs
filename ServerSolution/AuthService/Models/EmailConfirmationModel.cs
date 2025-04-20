namespace AuthService.Models;

public class EmailConfirmationModel
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}
