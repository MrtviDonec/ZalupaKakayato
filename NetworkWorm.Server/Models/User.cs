using System;

namespace NetworkWorm.Server.Models
{
    public class User
    {
        // Supabase использует UUID вместо int
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public int TotalScore { get; set; }
        public string ConnectionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}