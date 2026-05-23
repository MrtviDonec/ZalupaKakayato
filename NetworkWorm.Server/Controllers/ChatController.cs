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

        [HttpGet("chats")]
        public async Task<IActionResult> GetUserChats()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var query = @"
                SELECT 
                    c.id_chat,
                    COALESCE(c.chat_name, string_agg(DISTINCT u.username, ', ')) as name,
                    lm.message as last_message,
                    lm.sent_at as last_message_time,
                    COUNT(CASE WHEN cm.is_read = FALSE AND cm.user_id != @userId THEN 1 END) as unread_count,
                    COUNT(DISTINCT cp.user_id) as participant_count
                FROM chats c
                JOIN chat_participants cp ON c.id_chat = cp.chat_id
                LEFT JOIN LATERAL (
                    SELECT message, sent_at
                    FROM chat_messages
                    WHERE chat_id = c.id_chat
                    ORDER BY sent_at DESC
                    LIMIT 1
                ) lm ON TRUE
                LEFT JOIN chat_messages cm ON c.id_chat = cm.chat_id
                WHERE cp.user_id = @userId
                GROUP BY c.id_chat, c.chat_name, lm.message, lm.sent_at
                ORDER BY lm.sent_at DESC NULLS LAST";

            var chats = await _dbContext.Chats
                .FromSqlRaw(query, new Npgsql.NpgsqlParameter("@userId", userId))
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    LastMessage = "",
                    LastMessageTime = (DateTime?)null,
                    UnreadCount = 0,
                    ParticipantCount = 0
                })
                .ToListAsync();

            return Ok(chats);
        }

        [HttpGet("messages/{chatId}")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            // Проверяем, является ли пользователь участником чата
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