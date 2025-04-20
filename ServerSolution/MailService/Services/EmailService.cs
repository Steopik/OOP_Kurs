using MailService.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService : IEmailService
{
    private readonly SmtpClient _smtpClient;
    private readonly string _fromEmail;

    public EmailService(IConfiguration configuration)
    {
        _fromEmail = configuration["EmailSettings:From"];
        _smtpClient = new SmtpClient(configuration["EmailSettings:SmtpHost"], 
            int.Parse(configuration["EmailSettings:SmtpPort"]))
        {
            Credentials = new NetworkCredential(
                configuration["EmailSettings:Username"],
                configuration["EmailSettings:Password"]
            ),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var mailMessage = new MailMessage(_fromEmail, toEmail, subject, body)
        {
            IsBodyHtml = true
        };

        await _smtpClient.SendMailAsync(mailMessage);
    }
}
