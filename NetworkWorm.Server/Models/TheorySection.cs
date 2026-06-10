using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace NetworkWorm.Server.Models
{
    public class TheorySection
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string IconPath { get; set; }
        public bool IsCompletedRequired { get; set; }

        public virtual List<TheoryPart> Parts { get; set; }
        public virtual List<Test> Tests { get; set; }
        public virtual List<LabWork> LabWorks { get; set; }
        public virtual List<SegmentationTask> Tasks { get; set; }
    }
}