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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Логин и пароль обязательны" });
            }

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.PasswordHash == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Учетная запись заблокирована" });
            }

            var token = _authService.GenerateToken(user.Id, user.Username, user.Role);

            user.LastLogin = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

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
}