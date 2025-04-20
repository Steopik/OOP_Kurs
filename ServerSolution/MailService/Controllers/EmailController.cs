using MailService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MailService.Models;

namespace MailService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] EmailRequest model)
    {
        await _emailService.SendEmailAsync(model.To, model.Subject, model.Body);
        return Ok(new { message = "Email sent successfully." });
    }
}
