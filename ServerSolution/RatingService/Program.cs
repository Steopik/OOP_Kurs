using RatingService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы для работы с контроллерами
builder.Services.AddControllers();

// Добавляем DbContext с подключением к MySQL
builder.Services.AddDbContext<RatingDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();

// Настройка обработки запросов
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();  // Регистрируем маршруты контроллеров

app.Run();
