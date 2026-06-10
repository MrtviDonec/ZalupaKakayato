using System.Collections.Generic;

namespace NetworkWorm.Server.Models
{
    public class SegmentationTask
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string NetworkAddress { get; set; }
        public object Departments { get; set; } // JSONB
        public object VlanRequirements { get; set; } // JSONB
        public List<string> EquipmentList { get; set; }
        public string SolutionSubnetMask { get; set; }
        public string SolutionVlanAssignment { get; set; }
        public string SolutionIpAllocation { get; set; }
        public int MaxScore { get; set; }
        public int DifficultyLevel { get; set; }
        public object TaskSteps { get; set; } // JSONB
        public int TotalSteps { get; set; }
        public int PassingSteps { get; set; }
    }
}