using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Регистрируем сервисы
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ ВОТ ЭТА СТРОКА НУЖНА!
builder.Services.AddScoped<NetworkWorm.Server.Services.AuthService>();

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
app.MapControllers();
app.MapHub<ChatHub>("/learningHub");
app.MapGet("/", () => "SignalR сервер работает!");
app.MapGet("/test", () => "Сервер жив!");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
app.Run($"http://0.0.0.0:{port}");
