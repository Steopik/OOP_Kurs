using System.Net.Http.Json;

namespace ReadingProgress.Service.Services;

public class ExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public ExternalApiService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    // Получаем URL из конфига
    private string GetAuthServiceUrl() => _config["Urls:AuthService"];
    private string GetBookServiceUrl() => _config["Urls:BookService"];

    // Проверяем, существует ли пользователь
    public async Task<bool> UserExists(Guid userId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{GetAuthServiceUrl()}/Auth/{userId}";

        var response = await client.GetAsync(url);
        return response.IsSuccessStatusCode;
    }

    // Проверяем, существует ли книга
    public async Task<bool> BookExists(Guid bookId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{GetBookServiceUrl()}/book/{bookId}";

        var response = await client.GetAsync(url);
        return response.IsSuccessStatusCode;
    }

    // Проверяем, админ ли пользователь
    public async Task<bool> IsUserAdmin(Guid userId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{GetAuthServiceUrl()}/Auth/{userId}";

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode) return false;

        var user = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return user?.IsAdmin ?? false;
    }

    private record AuthResponseDto(bool IsAdmin);
}