using Microsoft.AspNetCore.Mvc;
using Auth.Service.DTOs;
using Auth.Service.Services;
using Auth.Service.Extensions;
using System.IdentityModel.Tokens.Jwt;

namespace Auth.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var success = await _authService.Register(dto);
        return success ? Ok("Регистрация успешна. Проверьте почту.") : BadRequest("Ошибка регистрации.");
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyRequestDto dto)
    {
        var token = await _authService.VerifyEmail(dto.Username, dto.Code);
        return token != null ? Ok(new { Token = token }) : BadRequest("Неверный код или пользователь.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var token = await _authService.Login(dto);
        return token != null ? Ok(new { Token = token }) : Unauthorized();
    }


    [HttpDelete("delete/{username}")]
    public async Task<IActionResult> DeleteUser(
        string username,
        [FromHeader(Name = "Authorization")] string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized();

        var token = authHeader["Bearer ".Length..].Trim();
        var requesterUsername = TokenExtensions.GetUsernameFromToken(token);
        if (requesterUsername == null)
            return Unauthorized();

        var success = await _authService.DeleteUser(username, requesterUsername);
        return success ? Ok($"Пользователь '{username}' удалён") : BadRequest("Нет прав или пользователь не найден");
    }



    [HttpPut("update/{username}")]
    public async Task<IActionResult> UpdateUser(
        string username,
        [FromBody] UpdateUserRequestDto dto,
        [FromHeader(Name = "Authorization")] string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized();

        var token = authHeader["Bearer ".Length..].Trim();
        var requesterUsername = TokenExtensions.GetUsernameFromToken(token);
        if (requesterUsername == null)
            return Unauthorized();

        var success = await _authService.UpdateUser(username, dto, requesterUsername);
        return success ? Ok($"Пользователь '{username}' обновлён") : BadRequest("Нет прав или пользователь не найден");
    }



    [HttpPost("admin/create")]
    public async Task<IActionResult> AdminCreateUser(
    [FromBody] AdminCreateUserDto dto,
    [FromHeader(Name = "Authorization")] string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized();

        var token = authHeader["Bearer ".Length..].Trim();
        var requesterUsername = TokenExtensions.GetUsernameFromToken(token);
        if (requesterUsername == null)
            return Unauthorized();

        var (success, error) = await _authService.AdminCreateUser(dto, requesterUsername);
        if (!success)
            return BadRequest(error);

        return Ok($"Пользователь '{dto.Username}' создан");
    }


    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromHeader(Name = "Authorization")] string authHeader)
    {
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized();

        var token = authHeader["Bearer ".Length..].Trim();
        var requesterUsername = TokenExtensions.GetUsernameFromToken(token);
        if (requesterUsername == null)
            return Unauthorized();

        try
        {
            var users = await _authService.GetAllUsers(requesterUsername);
            return Ok(users);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }


  
}