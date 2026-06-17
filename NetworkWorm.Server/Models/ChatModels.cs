using System;

namespace NetworkWorm.Server.Models
{
    public class Chat
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedBy { get; set; }
        
    public string? LastMessage { get; set; }        
    public DateTime? LastMessageTime { get; set; }  
    public int UnreadCount { get; set; }            
    public int ParticipantCount { get; set; }       
}

    public class ChatParticipant
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsTeacher { get; set; }
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string? Message { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? Role { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public int TotalScore { get; set; }
        public string? ConnectionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
