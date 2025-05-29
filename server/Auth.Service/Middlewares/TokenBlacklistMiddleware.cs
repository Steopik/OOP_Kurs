using Auth.Service.Services;
using Microsoft.AspNetCore.Http;

namespace Auth.Service.Middlewares;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public TokenBlacklistMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader["Bearer ".Length..].Trim();

            using var scope = _serviceProvider.CreateScope();
            var blacklist = scope.ServiceProvider.GetRequiredService<TokenBlacklistService>();

            if (await blacklist.IsInBlacklist(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Token is revoked");
                return;
            }
        }

        await _next(context);
    }
}