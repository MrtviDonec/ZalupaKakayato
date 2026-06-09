using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем SignalR
builder.Services.AddSignalR();

// Подключение к Supabase
var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.wregsgpisgfvjplshqqo;Password=?$TpT!2y!5*R9Hv;SSL Mode=Disable;Trust Server Certificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS для WPF клиента
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

// Настройка pipeline
app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<ChatHub>("/learningHub");

// Простые эндпоинты для проверки
app.MapGet("/", () => new { status = "running", service = "NetworkWorm Server" });
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Автоматическое создание БД
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("Database connection successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
