using Microsoft.EntityFrameworkCore;
using NetworkWorm.Server.Models;
using static System.Net.Mime.MediaTypeNames;

namespace NetworkWorm.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Существующие DbSet
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<User> Users { get; set; }

        // НОВЫЕ DbSet для контроллеров
        public DbSet<TheorySection> TheorySections { get; set; }
        public DbSet<TheoryPart> TheoryParts { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<LabWork> LabWorks { get; set; }
        public DbSet<SegmentationTask> SegmentationTasks { get; set; }
        public DbSet<UserProgress> UserProgress { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка таблицы Users
            // Настройка таблицы Users (для Supabase Auth)
                modelBuilder.Entity<User>(entity =>
                {
                    entity.ToTable("users");
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Id).HasColumnName("id");
                    entity.Property(e => e.Username).HasColumnName("username");
                    entity.Property(e => e.Email).HasColumnName("email");
                    entity.Property(e => e.PasswordHash).HasColumnName("encrypted_password");
                    entity.Property(e => e.Role).HasColumnName("role");
                    entity.Property(e => e.LastLogin).HasColumnName("last_sign_in_at");
                    entity.Property(e => e.IsActive).HasColumnName("is_active");
                    entity.Property(e => e.TotalScore).HasColumnName("total_score");
                    entity.Property(e => e.ConnectionId).HasColumnName("connection_id");
                    entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                });

            // Настройка таблицы Chats
            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("chats");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_chat");
                entity.Property(e => e.Name).HasColumnName("chat_name");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            });

            // Настройка таблицы ChatParticipants
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

            // Настройка таблицы ChatMessages
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

            // ========== НОВЫЕ НАСТРОЙКИ ==========

            // Настройка таблицы TheorySections
            modelBuilder.Entity<TheorySection>(entity =>
            {
                entity.ToTable("theory_sections");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_section");
                entity.Property(e => e.Title).HasColumnName("section_title");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Order).HasColumnName("section_order");
                entity.Property(e => e.IconPath).HasColumnName("icon_path");
                entity.Property(e => e.IsCompletedRequired).HasColumnName("is_completed_required");
            });

            // Настройка таблицы TheoryParts
            modelBuilder.Entity<TheoryPart>(entity =>
            {
                entity.ToTable("theory_parts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_part");
                entity.Property(e => e.SectionId).HasColumnName("id_section");
                entity.Property(e => e.Title).HasColumnName("part_title");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.Order).HasColumnName("part_order");
                entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");

                entity.HasOne<TheorySection>()
                    .WithMany(s => s.Parts)
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы Tests
            modelBuilder.Entity<Test>(entity =>
            {
                entity.ToTable("tests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_test");
                entity.Property(e => e.SectionId).HasColumnName("id_section");
                entity.Property(e => e.Title).HasColumnName("test_title");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Questions).HasColumnName("questions")
                    .HasColumnType("jsonb");
                entity.Property(e => e.PassingScore).HasColumnName("passing_score");

                entity.HasOne<TheorySection>()
                    .WithMany(s => s.Tests)
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы LabWorks
            modelBuilder.Entity<LabWork>(entity =>
            {
                entity.ToTable("lab_works");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_lab");
                entity.Property(e => e.SectionId).HasColumnName("id_section");
                entity.Property(e => e.Title).HasColumnName("lab_title");
                entity.Property(e => e.Description).HasColumnName("lab_description");
                entity.Property(e => e.Instructions).HasColumnName("instructions");
                entity.Property(e => e.Steps).HasColumnName("lab_steps")
                    .HasColumnType("jsonb");
                entity.Property(e => e.TotalSteps).HasColumnName("total_steps");
                entity.Property(e => e.PassingSteps).HasColumnName("passing_steps");
                entity.Property(e => e.EquipmentList).HasColumnName("equipment_list")
                    .HasColumnType("jsonb");
                entity.Property(e => e.EstimatedTimeHours).HasColumnName("estimated_time_hours");
                entity.Property(e => e.MaxScore).HasColumnName("max_score");
                entity.Property(e => e.IsRequired).HasColumnName("is_required");

                entity.HasOne<TheorySection>()
                    .WithMany(s => s.LabWorks)
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы SegmentationTasks
            modelBuilder.Entity<SegmentationTask>(entity =>
            {
                entity.ToTable("segmentation_tasks");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_segmentation");
                entity.Property(e => e.SectionId).HasColumnName("id_section");
                entity.Property(e => e.Title).HasColumnName("task_title");
                entity.Property(e => e.Description).HasColumnName("task_description");
                entity.Property(e => e.NetworkAddress).HasColumnName("network_address");
                entity.Property(e => e.Departments).HasColumnName("departments")
                    .HasColumnType("jsonb");
                entity.Property(e => e.VlanRequirements).HasColumnName("vlan_requirements")
                    .HasColumnType("jsonb");
                entity.Property(e => e.EquipmentList).HasColumnName("equipment_list")
                    .HasColumnType("jsonb");
                entity.Property(e => e.SolutionSubnetMask).HasColumnName("solution_subnet_mask");
                entity.Property(e => e.SolutionVlanAssignment).HasColumnName("solution_vlan_assignment");
                entity.Property(e => e.SolutionIpAllocation).HasColumnName("solution_ip_allocation");
                entity.Property(e => e.MaxScore).HasColumnName("max_score");
                entity.Property(e => e.DifficultyLevel).HasColumnName("difficulty_level");
                entity.Property(e => e.TaskSteps).HasColumnName("task_steps")
                    .HasColumnType("jsonb");
                entity.Property(e => e.TotalSteps).HasColumnName("total_steps");
                entity.Property(e => e.PassingSteps).HasColumnName("passing_steps");

                entity.HasOne<TheorySection>()
                    .WithMany(s => s.Tasks)
                    .HasForeignKey(e => e.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы UserProgress
            modelBuilder.Entity<UserProgress>(entity =>
            {
                entity.ToTable("user_progress");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id_progress");
                entity.Property(e => e.UserId).HasColumnName("id_user");
                entity.Property(e => e.PartId).HasColumnName("id_part");
                entity.Property(e => e.TestId).HasColumnName("id_test");
                entity.Property(e => e.LabId).HasColumnName("id_lab");
                entity.Property(e => e.SegmentationId).HasColumnName("id_segmentation");
                entity.Property(e => e.UserAnswer).HasColumnName("user_answer")
                    .HasColumnType("jsonb");
                entity.Property(e => e.IsCorrect).HasColumnName("is_correct");
                entity.Property(e => e.Score).HasColumnName("score");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.AttemptsCount).HasColumnName("attempts_count");
                entity.Property(e => e.CompletedAt).HasColumnName("completed_at");

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<TheoryPart>()
                    .WithMany()
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
