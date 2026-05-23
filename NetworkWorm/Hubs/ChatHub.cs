using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Data;
using NetworkWorm.Server.Models;
using Npgsql;

namespace NetworkWorm.Server.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ApplicationDbContext dbContext, ILogger<ChatHub> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        //public async Task TestConnection()
        //{
        //    _logger.LogInformation($"TestConnection called from client {Context.ConnectionId} !!!");
        //    await Clients.Caller.SendAsync("TestResponse", "хелло ёпта!");
        //}

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChatGroup(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
            _logger.LogInformation($"Client {Context.ConnectionId} joined chat {chatId}");
        }

        public async Task LeaveChatGroup(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task SendMessage(int chatId, string message, int userId)
        {
            _logger.LogInformation($"SendMessage called: chat={chatId}, user={userId}, message={message}");

            try
            {
                // ПРЯМОЙ SQL ЗАПРОС
                var sql = @"
            INSERT INTO chat_messages (id_chat, id_user, message, sent_at, is_read, is_edited)
            VALUES (@chatId, @userId, @message, @sentAt, false, false)
            RETURNING id_message";

                var parameters = new[]
                {
            new Npgsql.NpgsqlParameter("@chatId", chatId),
            new Npgsql.NpgsqlParameter("@userId", userId),
            new Npgsql.NpgsqlParameter("@message", message),
            new Npgsql.NpgsqlParameter("@sentAt", DateTime.UtcNow)
        };

                int messageId;
                using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sql;
                    command.Parameters.AddRange(parameters);

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    messageId = Convert.ToInt32(await command.ExecuteScalarAsync());
                }

                _logger.LogInformation($"Message saved to DB with ID: {messageId}");

                // Получаем имя пользователя через прямой SQL
                var userSql = "SELECT username FROM users WHERE id_user = @userId";
                string username = "Unknown";

                using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = userSql;
                    command.Parameters.Add(new Npgsql.NpgsqlParameter("@userId", userId));

                    if (command.Connection.State != System.Data.ConnectionState.Open)
                        await command.Connection.OpenAsync();

                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                        username = result.ToString();
                }

                // Отправляем сообщение в группу
                var messageDto = new
                {
                    Id = messageId,
                    ChatId = chatId,
                    UserId = userId,
                    Username = username,
                    Message = message,
                    SentAt = DateTime.UtcNow,
                    IsRead = false,
                    IsEdited = false,
                    EditedAt = (DateTime?)null
                };

                await Clients.Group($"chat_{chatId}").SendAsync("NewMessage", messageDto);
                _logger.LogInformation($"Message sent to group chat_{chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SendMessage: {ex.Message}");
                throw new HubException($"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        public async Task CreatePrivateChat(int teacherId, int studentId)
        {
            try
            {
                _logger.LogInformation($"CreatePrivateChat called: teacher={teacherId}, student={studentId}");

                var existingChatId = await _dbContext.ChatParticipants
                    .Where(cp => cp.UserId == teacherId)
                    .Select(cp => cp.ChatId)
                    .Intersect(
                        _dbContext.ChatParticipants
                            .Where(cp => cp.UserId == studentId)
                            .Select(cp => cp.ChatId)
                    )
                    .FirstOrDefaultAsync();

                if (existingChatId != 0)
                {
                    var existingChat = await _dbContext.Chats.FindAsync(existingChatId);
                    if (existingChat != null)
                    {
                        _logger.LogInformation($"Existing chat found: {existingChat.Id}");
                        await Clients.Caller.SendAsync("NewChatCreated", existingChat);
                        return;
                    }
                }

                var teacher = await _dbContext.Users.FindAsync(teacherId);
                var student = await _dbContext.Users.FindAsync(studentId);

                if (teacher == null || student == null)
                {
                    _logger.LogWarning("Teacher or student not found");
                    throw new HubException("Преподаватель или студент не найден");
                }

                var chatName = $"{teacher.Username} & {student.Username}";

                var newChat = new Chat
                {
                    Name = chatName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = teacherId
                };

                _dbContext.Chats.Add(newChat);
                await _dbContext.SaveChangesAsync();

                var participants = new[]
                {
                    new ChatParticipant { ChatId = newChat.Id, UserId = teacherId, JoinedAt = DateTime.UtcNow, IsTeacher = true },
                    new ChatParticipant { ChatId = newChat.Id, UserId = studentId, JoinedAt = DateTime.UtcNow, IsTeacher = false }
                };

                _dbContext.ChatParticipants.AddRange(participants);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Chat created successfully: {newChat.Id}");

                await Clients.Caller.SendAsync("NewChatCreated", newChat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePrivateChat");
                throw new HubException($"Ошибка создания чата: {ex.Message}");
            }
        }
    }
}