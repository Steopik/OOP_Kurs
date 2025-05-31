using System.Net.Http.Json;

namespace Review.Service.Services;

public class ExternalApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> BookExists(Guid bookId)
    {
        var client = _httpClientFactory.CreateClient("BookClient");
        var response = await client.GetAsync($"/book/{bookId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UserExists(Guid userId)
    {
        var client = _httpClientFactory.CreateClient("AuthClient");
        var response = await client.GetAsync($"/Auth/{userId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IsUserAdmin(Guid userId)
    {
        var client = _httpClientFactory.CreateClient("AuthClient");
        var response = await client.GetAsync($"/user/{userId}");
        if (!response.IsSuccessStatusCode)
            return false;

        var user = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        return user?.IsAdmin ?? false;
    }

    private record AuthResponseDto(bool IsAdmin);
}