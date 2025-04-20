using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService.Models;
using AuthService.Data;
using System.Text;
using System.Text.Json;
using AuthService.Interfaces;
using AuthService.Services;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IPendingRegistrationStore _pendingRegistrationStore;
        private readonly HttpClient _httpClient;  
        private readonly string _emailServiceUrl = "http://localhost:5253/api/email/send";

        private readonly string _secretKey = "auth1bz_GHserver349_secretvd_key678";
        private readonly int _expirationMinutes = 60;

        public AuthController(AuthDbContext context,
                              IPendingRegistrationStore pendingRegistrationStore,
                              HttpClient httpClient)
        {
            _context = context;
            _pendingRegistrationStore = pendingRegistrationStore;
            _httpClient = httpClient;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Email))
                return BadRequest("Invalid input.");

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

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
                var response = await client.PostAsJsonAsync("http://localhost:5253/api/email/send", emailRequest);

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
    }
}
