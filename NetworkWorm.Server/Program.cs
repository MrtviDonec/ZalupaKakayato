using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();
builder.Services.AddSignalR();

// ============================================
// 🔄 ВЫБОР ИСТОЧНИКА ДАННЫХ
// ============================================

// Определяем, какую БД использовать
var dbSource = Environment.GetEnvironmentVariable("DB_SOURCE") ?? "supabase";

string connectionString;

if (dbSource == "local")
{
    // Локальный PostgreSQL
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__LocalConnection")
        ?? builder.Configuration.GetConnectionString("LocalConnection")
        ?? "Host=localhost;Port=5432;Database=networkworm;Username=postgres;Password=123;";
    
    Console.WriteLine($"✅ Используется ЛОКАЛЬНАЯ БД: localhost");
}
else
{
    // Supabase (по умолчанию)
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.wregsgpisgfvjplshqqo;Password=?$TpT!2y!5*R9Hv;SSL Mode=Prefer;Trust Server Certificate=true;";
    
    Console.WriteLine($"✅ Используется SUPABASE: aws-0-eu-west-1.pooler.supabase.com");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS
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

app.UseCors("AllowAll");
app.MapControllers();
app.MapHub<ChatHub>("/learningHub");

app.MapGet("/", () => new { status = "running", service = "NetworkWorm Server" });
app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow });

// Автоматическое создание БД
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine($"✅ База данных подключена успешно!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка подключения к БД: {ex.Message}");
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
