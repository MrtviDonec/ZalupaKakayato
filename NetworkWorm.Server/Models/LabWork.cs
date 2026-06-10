using System.Collections.Generic;

namespace NetworkWorm.Server.Models
{
    public class LabWork
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public object Steps { get; set; } // JSONB
        public int TotalSteps { get; set; }
        public int PassingSteps { get; set; }
        public List<string> EquipmentList { get; set; }
        public int EstimatedTimeHours { get; set; }
        public int MaxScore { get; set; }
        public bool IsRequired { get; set; }
    }
}