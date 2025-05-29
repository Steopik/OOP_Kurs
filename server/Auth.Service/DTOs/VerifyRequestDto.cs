namespace Auth.Service.DTOs;

public class VerifyRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}