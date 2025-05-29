using System.Net.Http.Json;
using System.Text.Json;

namespace Auth.Service.Services;

public class EmailService
{
    private readonly HttpClient _httpClient;
    private readonly string _sendEndpoint;

    public EmailService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _sendEndpoint = configuration["EmailService:SendEndpoint"];
    }

    public async Task SendVerificationCode(string email, string code)
    {
        var payload = new
        {
            To = email,
            Subject = "Подтверждение регистрации",
            Body = $"Ваш код подтверждения: {code}"
        };

        var content = JsonContent.Create(payload);
        await _httpClient.PostAsync(_sendEndpoint, content);
    }
}