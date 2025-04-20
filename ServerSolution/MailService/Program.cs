using MailService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
