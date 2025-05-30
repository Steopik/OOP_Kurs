using Review.Service.Models;
using Review.Service.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
var urls = builder.Configuration["Urls:ThisService"];
builder.WebHost.UseUrls(urls);
// ===== Регистрация зависимостей =====


builder.Services.AddHttpClient("BookClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Urls:BookService"]);
});

builder.Services.AddHttpClient("AuthClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Urls:AuthService"]);
});



builder.Services.AddControllers();

// EF Core (если используешь SQLite для отзывов)
builder.Services.AddDbContext<ReviewDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP клиенты для внешних сервисов
builder.Services.AddHttpClient<ExternalApiService>();
builder.Services.AddScoped<ExternalApiService>();
builder.Services.AddScoped<ReviewService>();

// Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();