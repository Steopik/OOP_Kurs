using Microsoft.EntityFrameworkCore;
using Auth.Service.Models;
using Auth.Service.Services;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using Auth.Service.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// �������� URL �� �������
var urls = builder.Configuration["Urls"];
builder.WebHost.UseUrls(urls);

// ===== ����������� �������� =====

// 1. ���� ������ SQLite
builder.Services.AddDbContext<AuthDbContext>();

// 2. Redis: ������� �����������
var redis = ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis); // <-- �����: ������������ ��� IConnectionMultiplexer

// 3. RedisVerificationService � ������� �� IConnectionMultiplexer
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

// 5. JWT � ��������������
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

// 6. �����������
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