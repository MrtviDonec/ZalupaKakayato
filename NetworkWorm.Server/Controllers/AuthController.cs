using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Models;
using NetworkWorm.Server.Services;

namespace NetworkWorm.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AuthService _authService;

        public AuthController(ApplicationDbContext dbContext, AuthService authService)
        {
            _dbContext = dbContext;
            _authService = authService;
        }

        [HttpPost("test-db")]
        public async Task<IActionResult> TestDb()
        {
            try
            {
                var users = await _dbContext.Users.ToListAsync();
                return Ok(new { count = users.Count, users = users.Select(u => new { u.Username, u.Role }) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Логин и пароль обязательны" });
            }

            // Проверяем подключение к БД
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                Console.WriteLine($"Database connected: {canConnect}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                return StatusCode(500, new { message = "Ошибка подключения к базе данных" });
            }



            // Ищем пользователя
            var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);
    
            if (user == null)
            {
                // Логируем попытку входа
                Console.WriteLine($"Login failed for user: {request.Username}");
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }



            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Учетная запись заблокирована" });
            }



             var token = _authService.GenerateToken(user.Id.ToString(), user.Username, user.Role);



            user.LastLogin = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            Console.WriteLine($"Login successful for user: {request.Username}");

            return Ok(new
            {
                Success = true,
                Token = token,
                User = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role
                }
            });
        }
    }
    

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
