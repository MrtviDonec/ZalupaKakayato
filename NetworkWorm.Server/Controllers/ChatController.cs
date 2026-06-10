using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Models;
using System.Security.Claims;

namespace NetworkWorm.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ChatController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var chats = await (
                from cp in _dbContext.ChatParticipants
                join c in _dbContext.Chats on cp.ChatId equals c.Id
                where cp.UserId == userId
                select new
                {
                    c.Id,
                    c.Name,
                    LastMessage = _dbContext.ChatMessages
                        .Where(m => m.ChatId == c.Id)
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefault().Message,
                    LastMessageTime = _dbContext.ChatMessages
                        .Where(m => m.ChatId == c.Id)
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefault().SentAt,
                    UnreadCount = _dbContext.ChatMessages
                        .Count(m => m.ChatId == c.Id && m.UserId != userId && !m.IsRead),
                    ParticipantCount = _dbContext.ChatParticipants.Count(p => p.ChatId == c.Id),
                    c.CreatedAt,
                    c.CreatedBy
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync();

            return Ok(chats);
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var isParticipant = await _dbContext.ChatParticipants
                .AnyAsync(cp => cp.ChatId == chatId && cp.UserId == userId);

            if (!isParticipant)
                return Forbid();

            var messages = await _dbContext.ChatMessages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Id,
                    m.ChatId,
                    m.UserId,
                    Username = _dbContext.Users.Where(u => u.Id == m.UserId).Select(u => u.Username).FirstOrDefault(),
                    m.Message,
                    m.SentAt,
                    m.IsRead,
                    m.IsEdited,
                    m.EditedAt
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}