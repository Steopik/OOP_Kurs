using Microsoft.EntityFrameworkCore;
using ReadingProgress.Service.Models;
using ReadingProgress.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== Регистрация сервисов =====
builder.Services.AddControllers();

// EF Core + SQLite
builder.Services.AddDbContext<ProgressDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// HTTP клиенты
builder.Services.AddHttpClient<ExternalApiService>();
builder.Services.AddScoped<ExternalApiService>();
builder.Services.AddScoped<ProgressService>();

// Swagger
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