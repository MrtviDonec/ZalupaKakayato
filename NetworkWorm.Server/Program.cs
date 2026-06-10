using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрируем сервисы
builder.Services.AddControllers(); // <-- ОБЯЗАТЕЛЬНО для работы API контроллеров
builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 2. Настраиваем конвейер запросов
app.UseCors("AllowAll");

// 3. Маппим маршруты (ЭТО ГЛАВНОЕ!)
app.MapControllers(); // <-- БЕЗ ЭТОЙ СТРОКИ API НЕ РАБОТАЮТ
app.MapHub<ChatHub>("/learningHub");

// 4. Простые тестовые маршруты для проверки
app.MapGet("/", () => Results.Json(new { status = "running", service = "NetworkWorm Server" }));
app.MapGet("/test", () => "Server is alive!");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
app.Run($"http://0.0.0.0:{port}");
