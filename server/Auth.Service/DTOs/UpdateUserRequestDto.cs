namespace Auth.Service.DTOs;

public class UpdateUserRequestDto
{
    public string? NewUsername { get; set; }
    public string? NewPassword { get; set; }
    public string? NewEmail { get; set; }
    public bool? IsAdmin { get; set; }
    public bool? IsSuperUser { get; set; }
}