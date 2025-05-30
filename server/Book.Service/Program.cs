using Book.Service.Models;
using Book.Service.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Получаем URL из конфига
var urls = builder.Configuration["Urls"];
builder.WebHost.UseUrls(urls);

// ===== Регистрация зависимостей =====

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// EF Core + SQLite
builder.Services.AddDbContext<BookDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Поддержка контроллеров
builder.Services.AddControllers();

// Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PDF и книжные сервисы
builder.Services.AddScoped<PdfService>();
builder.Services.AddScoped<BookService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization(); 
app.MapControllers();

await app.RunAsync();

Console.WriteLine($"Сервис запущен по адресу: {urls}");