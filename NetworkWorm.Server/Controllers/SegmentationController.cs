using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using System.Text.Json;
using NetworkWorm.Server.Models;

namespace NetworkWorm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SegmentationController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public SegmentationController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("section/{sectionId}")]
        public async Task<IActionResult> GetSegmentationBySection(int sectionId)
        {
            var task = await _dbContext.SegmentationTasks
                .Where(st => st.SectionId == sectionId)
                .Select(st => new
                {
                    st.Id,
                    st.Title,
                    st.Description,
                    st.NetworkAddress,
                    st.EquipmentList,
                    st.SolutionSubnetMask,
                    st.SolutionVlanAssignment,
                    st.SolutionIpAllocation,
                    st.MaxScore,
                    st.DifficultyLevel,
                    st.TaskSteps,
                    st.TotalSteps,
                    st.PassingSteps
                })
                .FirstOrDefaultAsync();

            return Ok(task);
        }

        [HttpGet("{taskId}/progress")]
        public async Task<IActionResult> GetSegmentationProgress(int taskId, [FromQuery] int userId)
        {
            var progress = await _dbContext.UserProgress
                .FirstOrDefaultAsync(up => up.SegmentationId == taskId && up.UserId == userId);

            if (progress == null)
                return Ok(new { status = "not_started", score = 0 });

            return Ok(new
            {
                status = progress.Status,
                score = progress.Score,
                userAnswers = progress.UserAnswer
            });
        }

        [HttpPost("save-progress")]
        public async Task<IActionResult> SaveSegmentationProgress([FromBody] JsonElement request)
        {
            try
            {
                int taskId = request.GetProperty("TaskId").GetInt32();
                int userId = request.GetProperty("UserId").GetInt32();
                int totalScore = request.GetProperty("TotalScore").GetInt32();
                string status = request.GetProperty("Status").GetString();
                var userAnswers = request.GetProperty("UserAnswers").ToString();

                var existing = await _dbContext.UserProgress
                    .FirstOrDefaultAsync(up => up.SegmentationId == taskId && up.UserId == userId);

                if (existing == null)
                {
                    _dbContext.UserProgress.Add(new UserProgress
                    {
                        SegmentationId = taskId,
                        UserId = userId,
                        Score = totalScore,
                        Status = status,
                        UserAnswer = userAnswers,
                        CompletedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existing.Score = totalScore;
                    existing.Status = status;
                    existing.UserAnswer = userAnswers;
                    existing.CompletedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("save-answers")]
        public async Task<IActionResult> SaveSegmentationAnswers([FromBody] JsonElement request)
        {
            try
            {
                int taskId = request.GetProperty("TaskId").GetInt32();
                int userId = request.GetProperty("UserId").GetInt32();
                var userAnswers = request.GetProperty("UserAnswers").ToString();

                var existing = await _dbContext.UserProgress
                    .FirstOrDefaultAsync(up => up.SegmentationId == taskId && up.UserId == userId);

                if (existing != null)
                {
                    existing.UserAnswer = userAnswers;
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteSegmentation([FromBody] JsonElement request)
        {
            try
            {
                int taskId = request.GetProperty("TaskId").GetInt32();
                int userId = request.GetProperty("UserId").GetInt32();
                int score = request.GetProperty("Score").GetInt32();

                var existing = await _dbContext.UserProgress
                    .FirstOrDefaultAsync(up => up.SegmentationId == taskId && up.UserId == userId);

                if (existing != null)
                {
                    existing.Score = score;
                    existing.Status = "completed";
                    existing.CompletedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}