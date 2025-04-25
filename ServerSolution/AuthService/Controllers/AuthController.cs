using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Models;
using System.Text;
using System.Text.Json;
using AuthService.Interfaces;
using AuthService.Services;
using BCrypt.Net;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IPendingRegistrationStore _pendingRegistrationStore;
    private readonly HttpClient _httpClient;  
    private readonly string _emailServiceUrl = "http://localhost:5253/api/email/send";
    private readonly string _userUrl = "http://localhost:5034/api/Users";
    private readonly IJwtService _jwtService;



    public AuthController(IPendingRegistrationStore pendingRegistrationStore,
                          HttpClient httpClient,
                          IJwtService jwtService)
    {
        _pendingRegistrationStore = pendingRegistrationStore;
        _httpClient = httpClient;
        _jwtService = jwtService;

    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Email))
            return BadRequest("Invalid input.");

        var users = await GetUsersByIdAsync();
        var existingUser = users.FirstOrDefault(u => u.Username == model.Username);
        if (existingUser != null)
            return Conflict(new { message = "Username already exists" });

        var hashedPassword = PasswordHasher.HashPassword(model.Password);

        var confirmationCode = GenerateConfirmationCode();
        var expiration = DateTime.UtcNow.AddMinutes(10); 

        var pendingRegistration = new PendingRegistration
        {
            Username = model.Username,
            PasswordHash = hashedPassword,
            Email = model.Email,
            ConfirmationCode = confirmationCode,
            Expiration = expiration
        };

        _pendingRegistrationStore.Save(pendingRegistration);

        await SendConfirmationEmail(model.Email, confirmationCode);

        return Ok(new { message = "User registered. Please check your email for confirmation." });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] EmailConfirmationModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Code))
            return BadRequest("Invalid input.");

        var pendingRegistration = _pendingRegistrationStore.Get(model.Email);
        if (pendingRegistration == null)
            return NotFound(new { message = "Registration not found." });

        if (pendingRegistration.Expiration < DateTime.UtcNow)
            return BadRequest(new { message = "Confirmation code expired." });

        if (pendingRegistration.ConfirmationCode != model.Code)
            return BadRequest(new { message = "Invalid confirmation code." });

        var user = new User
        {
            Username = pendingRegistration.Username,
            Email = pendingRegistration.Email,
            PasswordHash = pendingRegistration.PasswordHash
        };

        var HttpAns = await CreateUser(user);

        _pendingRegistrationStore.Remove(model.Email);

        return Ok(new { message = "Email confirmed. User registered successfully." });
    }

    private async Task SendConfirmationEmail(string email, string code)
    {
        var emailRequest = new
        {
            To = email,
            Subject = "Confirmation Code",
            Body = $"Your confirmation code is: {code}"
        };

        using var client = new HttpClient();
        try
        {
            var response = await client.PostAsJsonAsync(_emailServiceUrl, emailRequest);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error sending email confirmation.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error sending email: {ex.Message}");
        }
    }

    private string GenerateConfirmationCode()
    {
        var rng = new Random();
        return rng.Next(100000, 999999).ToString();
    }

    private async Task<User?> GetUserByIdAsync(int userId)
    {
        var response = await _httpClient.GetAsync($"{_userUrl}/{userId}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return user;
    }


    private async Task<List<User>?> GetUsersByIdAsync()
    {
        var response = await _httpClient.GetAsync($"{_userUrl}");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return users;
    }

    private async Task<HttpResponseMessage> CreateUser(User user)
    {
        var json = JsonSerializer.Serialize(user);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await _httpClient.PostAsync($"{_userUrl}", content);
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel request)
    {
        var users = await GetUsersByIdAsync();
        var user = users.FirstOrDefault(u => u.Username == request.Username);

        if (user == null || PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password");
        }

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token });
    }

}
