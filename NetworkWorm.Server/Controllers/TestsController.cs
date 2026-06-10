using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Models;
using System.Text.Json;

namespace NetworkWorm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public TestsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("section/{sectionId}")]
        public async Task<IActionResult> GetTestsBySection(int sectionId)
        {
            var tests = await _dbContext.Tests
                .Where(t => t.SectionId == sectionId)
                .OrderBy(t => t.Id)  // ← Вместо t.Order используем t.Id
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.PassingScore,
                    // Если Questions это JSONB массив вопросов
                    Questions = t.Questions
                })
                .ToListAsync();

            return Ok(tests);
        }

        [HttpPost("save-results")]
        public async Task<IActionResult> SaveTestResults([FromBody] JsonElement request)
        {
            try
            {
                int userId = request.GetProperty("UserId").GetInt32();
                int sectionId = request.GetProperty("SectionId").GetInt32();
                int earnedPoints = request.GetProperty("EarnedPoints").GetInt32();
                bool passed = request.GetProperty("Passed").GetBoolean();
                var answers = request.GetProperty("Answers");

                var parts = await _dbContext.TheoryParts
                    .Where(p => p.SectionId == sectionId)
                    .OrderBy(p => p.Order)
                    .ToListAsync();

                foreach (var part in parts)
                {
                    var existing = await _dbContext.UserProgress
                        .FirstOrDefaultAsync(up => up.PartId == part.Id && up.UserId == userId);

                    if (existing == null)
                    {
                        _dbContext.UserProgress.Add(new UserProgress
                        {
                            PartId = part.Id,
                            UserId = userId,
                            IsCorrect = true,
                            Score = earnedPoints / parts.Count,
                            Status = passed ? "completed" : "pending",
                            CompletedAt = DateTime.UtcNow
                        });
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}