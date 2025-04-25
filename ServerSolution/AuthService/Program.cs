using Microsoft.EntityFrameworkCore;
using AuthService.Interfaces;
using AuthService.Services;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();

builder.Services.AddSingleton<IPendingRegistrationStore, InMemoryPendingRegistrationStore>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();


app.MapControllers();

app.Run();
