using RatingService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ��������� ������� ��� ������ � �������������
builder.Services.AddControllers();

// ��������� DbContext � ������������ � MySQL
builder.Services.AddDbContext<RatingDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();

// ��������� ��������� ��������
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();  // ������������ �������� ������������

app.Run();
