using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Models;
using NetworkWorm.Server.Data;

namespace NetworkWorm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SectionsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public SectionsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            var sections = await _dbContext.TheorySections
                .OrderBy(s => s.Order)
                .Select(s => new
                {
                    s.Id,
                    s.Title,
                    s.Description,
                    s.Order
                })
                .ToListAsync();

            return Ok(sections);
        }

        [HttpGet("{sectionId}/progress")]
        public async Task<IActionResult> GetSectionProgress(int sectionId, [FromQuery] int userId)
        {
            var totalParts = await _dbContext.TheoryParts
                .CountAsync(p => p.SectionId == sectionId);

            var completedParts = await _dbContext.UserProgress
                .Where(up => up.UserId == userId && up.IsCorrect && up.PartId.HasValue)
                .Join(_dbContext.TheoryParts,
                    up => up.PartId.Value,
                    p => p.Id,
                    (up, p) => new { up, p })
                .CountAsync(x => x.p.SectionId == sectionId);

            var progress = totalParts > 0 ? (completedParts * 100) / totalParts : 0;

            return Ok(new { progress });
        }

        [HttpGet("{sectionId}/has-tests")]
        public async Task<IActionResult> HasTests(int sectionId)
        {
            var hasTests = await _dbContext.Tests.AnyAsync(t => t.SectionId == sectionId);
            return Ok(new { hasTests });
        }

        [HttpGet("{sectionId}/has-labs")]
        public async Task<IActionResult> HasLabs(int sectionId)
        {
            var hasLabs = await _dbContext.LabWorks.AnyAsync(l => l.SectionId == sectionId);
            return Ok(new { hasLabs });
        }

        [HttpGet("{sectionId}/has-segmentation")]
        public async Task<IActionResult> HasSegmentation(int sectionId)
        {
            var hasSegmentation = await _dbContext.SegmentationTasks.AnyAsync(st => st.SectionId == sectionId);
            return Ok(new { hasSegmentation });
        }
    }
}