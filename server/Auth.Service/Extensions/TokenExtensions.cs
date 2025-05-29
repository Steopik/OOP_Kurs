using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Auth.Service.Extensions;
public static class TokenExtensions
{
    public static string GetUsernameFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) return null;

        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    }

    public static Guid GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim?.Value, out var id) ? id : Guid.Empty;
    }
}