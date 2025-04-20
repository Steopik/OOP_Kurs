using AuthService.Data;
using Microsoft.EntityFrameworkCore;
using AuthService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ������ ����������� � ���� ������
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ����������� ������������
builder.Services.AddControllers();

// ����������� ��������� ��� ��������� �����������
builder.Services.AddSingleton<IPendingRegistrationStore, InMemoryPendingRegistrationStore>();

// ����������� HttpClient ��� �������������� � ������� �������� ��������
builder.Services.AddHttpClient();

var app = builder.Build();

// ���������� ��������, ���� ����������
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();  // �������������� ���������� ��������
}

// �������� ������������
app.MapControllers();

app.Run();
