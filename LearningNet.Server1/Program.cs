using LearningNet.Server.Data;
using LearningNet.Server.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

app.UseCors("AllowAll");
app.MapHub<ChatHub>("/learningHub");
app.MapGet("/test", () => "марсианин я собянин отзовись ГОЙДАГОЙДАГОЙДА!");


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
app.Run($"http://0.0.0.0:{port}");