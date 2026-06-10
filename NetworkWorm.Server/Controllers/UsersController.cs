using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Models;

namespace NetworkWorm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] int excludeUserId)
        {
            var students = await _dbContext.Users
                .Where(u => u.Role == "student" && u.IsActive && u.Id != excludeUserId)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email
                })
                .OrderBy(u => u.Username)
                .ToListAsync();

            return Ok(students);
        }
    }
}