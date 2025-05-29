using StackExchange.Redis;



namespace Auth.Service.Services;
public class TokenBlacklistService
{
    private readonly IDatabase _db;

    public TokenBlacklistService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    // Добавляем токен в черный список на время его жизни
    public async Task AddToBlacklist(string token, TimeSpan expiry)
    {
        await _db.StringSetAsync($"blacklist:{token}", "revoked", expiry);
    }

    // Проверяем, есть ли токен в чёрном списке
    public async Task<bool> IsInBlacklist(string token)
    {
        return await _db.StringGetAsync($"blacklist:{token}") != RedisValue.Null;
    }
}