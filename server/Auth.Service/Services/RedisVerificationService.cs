using StackExchange.Redis;
using System.Text.Json;

namespace Auth.Service.Services;

public class RedisVerificationService
{
    private readonly IDatabase _db;

    public RedisVerificationService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SaveCodeAsync(Guid userId, string code)
    {
        var expiry = TimeSpan.FromMinutes(10);
        await _db.StringSetAsync($"verification:{userId}", code, expiry);
    }

    public async Task<bool> ValidateCodeAsync(Guid userId, string code)
    {
        var storedCode = await _db.StringGetAsync($"verification:{userId}");
        return storedCode.HasValue && storedCode == code;
    }
}