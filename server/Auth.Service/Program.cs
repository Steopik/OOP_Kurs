using Microsoft.EntityFrameworkCore;
using Auth.Service.Models;
using Auth.Service.Services;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Auth.Service.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Получаем URL из конфига
var urls = builder.Configuration["Urls"];
builder.WebHost.UseUrls(urls);

// ===== РЕГИСТРАЦИЯ СЕРВИСОВ =====

// 1. База данных SQLite
builder.Services.AddDbContext<AuthDbContext>();

// 2. Redis: сначала подключение
var redis = ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis); // <-- ВАЖНО: регистрируем как IConnectionMultiplexer

// 3. RedisVerificationService — зависит от IConnectionMultiplexer
builder.Services.AddSingleton<RedisVerificationService>();

//blackList
builder.Services.AddSingleton<TokenBlacklistService>();

// 4. EmailService
builder.Services.AddHttpClient<EmailService>()
    .AddTypedClient<EmailService>((httpClient, sp) =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var baseUrl = config["EmailService:BaseUrl"];
        httpClient.BaseAddress = new Uri(baseUrl);
        return new EmailService(httpClient, config);
    });

// 5. JWT и аутентификация
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

// 6. Контроллеры
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth.Service", Version = "v1" });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader();
    });
});


var app = builder.Build();

//Middlewares
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseMiddleware<TokenValidationMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.Run();