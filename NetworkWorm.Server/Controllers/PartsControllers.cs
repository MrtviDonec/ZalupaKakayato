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
    public class PartsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public PartsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("section/{sectionId}")]
        public async Task<IActionResult> GetPartsBySection(int sectionId)
        {
            var parts = await _dbContext.TheoryParts
                .Where(p => p.SectionId == sectionId)
                .OrderBy(p => p.Order)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Content,
                    p.Order,
                    p.DurationMinutes
                })
                .ToListAsync();

            return Ok(parts);
        }

        [HttpGet("{partId}/completed")]
        public async Task<IActionResult> IsPartCompleted(int partId, [FromQuery] int userId)
        {
            var isCompleted = await _dbContext.UserProgress
                .AnyAsync(up => up.PartId == partId && up.UserId == userId && up.IsCorrect);

            return Ok(new { completed = isCompleted });
        }

        [HttpPost("{partId}/complete")]
        public async Task<IActionResult> MarkPartCompleted(int partId, [FromBody] int userId)
        {
            var exists = await _dbContext.UserProgress
                .AnyAsync(up => up.PartId == partId && up.UserId == userId);

            if (!exists)
            {
                var part = await _dbContext.TheoryParts.FindAsync(partId);
                if (part != null)
                {
                    _dbContext.UserProgress.Add(new UserProgress
                    {
                        PartId = partId,
                        UserId = userId,
                        IsCorrect = true,
                        Score = 100,
                        Status = "completed",
                        CompletedAt = DateTime.UtcNow
                    });
                    await _dbContext.SaveChangesAsync();
                }
            }

            return Ok(new { success = true });
        }
    }
}