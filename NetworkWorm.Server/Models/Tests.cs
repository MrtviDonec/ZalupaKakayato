using System.Collections.Generic;

namespace NetworkWorm.Server.Models
{
    public class Test
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public object Questions { get; set; } // JSONB
        public int PassingScore { get; set; }
    }
}