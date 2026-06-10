using System;

namespace NetworkWorm.Server.Models
{
    public class UserProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? PartId { get; set; }
        public int? TestId { get; set; }
        public int? LabId { get; set; }
        public int? SegmentationId { get; set; }
        public string UserAnswer { get; set; } // JSONB
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public string Status { get; set; }
        public int AttemptsCount { get; set; }
        public DateTime? CompletedAt { get; set; }
        public virtual TheoryPart Part { get; set; }
        public virtual Test Test { get; set; }
        public virtual LabWork Lab { get; set; }
        public virtual SegmentationTask Segmentation { get; set; }
        public virtual User User { get; set; }
    }
}