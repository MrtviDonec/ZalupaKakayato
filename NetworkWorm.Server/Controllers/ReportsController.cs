using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using System.Text;
using NetworkWorm.Server.Models;

namespace NetworkWorm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ReportsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GenerateReport([FromQuery] int type, [FromQuery] int userId)
        {
            var result = new StringBuilder();

            switch (type)
            {
                case 0: // Статистика пользователей
                    var userStats = await _dbContext.Users
                        .Select(u => new
                        {
                            u.Username,
                            u.Role,
                            IsActive = u.IsActive ? "Да" : "Нет",
                            CompletedTasks = _dbContext.UserProgress.Count(up => up.UserId == u.Id),
                            AvgScore = _dbContext.UserProgress.Where(up => up.UserId == u.Id).Average(up => (double?)up.Score) ?? 0,
                            LastLogin = u.LastLogin != null ? u.LastLogin.Value.ToString("dd.MM.yyyy") : "Никогда"
                        })
                        .OrderByDescending(u => u.AvgScore)
                        .ToListAsync();

                    result.AppendLine("Статистика пользователей");
                    result.AppendLine(new string('-', 80));
                    foreach (var u in userStats)
                    {
                        result.AppendLine($"{u.Username,-20} | {u.Role,-10} | {u.IsActive,-5} | {u.CompletedTasks,-15} | {u.AvgScore:F2}");
                    }
                    break;

                case 2: // Рейтинг студентов
                    var rating = await _dbContext.Users
                        .Where(u => u.Role == "student" && u.IsActive)
                        .Select(u => new
                        {
                            u.Username,
                            CompletedTests = _dbContext.UserProgress.Count(up => up.UserId == u.Id && up.TestId != null),
                            AvgScore = _dbContext.UserProgress.Where(up => up.UserId == u.Id).Average(up => (double?)up.Score) ?? 0
                        })
                        .OrderByDescending(u => u.AvgScore)
                        .Take(20)
                        .ToListAsync();

                    int rank = 1;
                    result.AppendLine("Рейтинг студентов");
                    result.AppendLine(new string('-', 80));
                    foreach (var s in rating)
                    {
                        result.AppendLine($"{rank++,-5} | {s.Username,-25} | {s.CompletedTests,-15} | {s.AvgScore:F2}");
                    }
                    break;

                default:
                    result.AppendLine("Отчет сформирован");
                    break;
            }

            return Ok(result.ToString());
        }
    }
}