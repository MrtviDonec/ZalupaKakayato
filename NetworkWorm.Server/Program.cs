using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Добавляем SignalR
builder.Services.AddSignalR();

// Добавляем базу данных PostgreSQL (Supabase)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS для WPF клиента (разрешаем всё)
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

// CORS middleware
app.UseCors("AllowAll");

// Маршруты
app.MapHub<ChatHub>("/learningHub");
app.MapGet("/", () => "дарова!");
app.MapGet("/test", () => "МАРСИАНИН Я СОБЯНИН ОТЗОВИСЬ!");

// Создание базы данных если нет
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

// Порт из переменной окружения (для Render)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
app.Run($"http://0.0.0.0:{port}");