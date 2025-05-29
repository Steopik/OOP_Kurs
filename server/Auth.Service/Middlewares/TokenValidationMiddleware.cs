using Auth.Service.Models;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Service.Middlewares;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public TokenValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            await _next(context);
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        if (IsTokenInvalid(token, out var error))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(error ?? "Token is invalid or outdated");
            return;
        }

        await _next(context);
    }

    private bool IsTokenInvalid(string token, out string? error)
    {
        error = null;

        try
        {
            using var scope = _serviceProvider.CreateScope(); // создаём новый scope
            var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

            var jwtToken = new JwtSecurityToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var tokenVersionClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "token_version")?.Value;

            if (userIdClaim == null || tokenVersionClaim == null)
            {
                error = "Missing required claims in token";
                return true;
            }

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                error = "Invalid user ID in token";
                return true;
            }

            var user = dbContext.Users.Find(userId);
            if (user == null)
            {
                error = "User not found";
                return true;
            }

            if (user.TokenVersion.ToString() != tokenVersionClaim)
            {
                error = "Token version mismatch";
                return true;
            }

            return false;
        }
        catch
        {
            error = "Token validation failed";
            return true;
        }
    }
}