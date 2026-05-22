using Microsoft.EntityFrameworkCore;
using LearningNet.Server.Models;

namespace LearningNet.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблицы Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_user");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Role).HasColumnName("role");
                entity.Property(e => e.LastLogin).HasColumnName("last_login");
                entity.Property(e => e.IsActive).HasColumnName("is_active");
                entity.Property(e => e.TotalScore).HasColumnName("total_score");
                entity.Property(e => e.ConnectionId).HasColumnName("connection_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("chats");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_chat");
                entity.Property(e => e.Name).HasColumnName("chat_name");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            });

            modelBuilder.Entity<ChatParticipant>(entity =>
            {
                entity.ToTable("chat_participants");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_participant");
                entity.Property(e => e.ChatId).HasColumnName("id_chat");
                entity.Property(e => e.UserId).HasColumnName("id_user");
                entity.Property(e => e.JoinedAt).HasColumnName("joined_at");
                entity.Property(e => e.IsTeacher).HasColumnName("is_teacher");

                entity.HasIndex(cp => new { cp.ChatId, cp.UserId }).IsUnique();
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.ToTable("chat_messages");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_message");
                entity.Property(e => e.ChatId).HasColumnName("id_chat");
                entity.Property(e => e.UserId).HasColumnName("id_user");
                entity.Property(e => e.Message).HasColumnName("message");
                entity.Property(e => e.SentAt).HasColumnName("sent_at");
                entity.Property(e => e.IsRead).HasColumnName("is_read");
                entity.Property(e => e.IsEdited).HasColumnName("is_edited");
                entity.Property(e => e.EditedAt).HasColumnName("edited_at");

                entity.HasIndex(cm => cm.ChatId);
            });
        }
    }
}