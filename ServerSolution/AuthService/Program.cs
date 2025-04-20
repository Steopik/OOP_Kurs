using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using AuthService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Строка подключения к базе данных
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Регистрация контроллеров
builder.Services.AddControllers();

// Регистрация хранилища для временных регистраций
builder.Services.AddSingleton<IPendingRegistrationStore, InMemoryPendingRegistrationStore>();

// Регистрация HttpClient для взаимодействия с внешним почтовым сервисом
builder.Services.AddHttpClient();

var app = builder.Build();

// Применение миграций, если необходимо
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();  // Автоматическое применение миграций
}

// Маршруты контроллеров
app.MapControllers();

app.Run();
