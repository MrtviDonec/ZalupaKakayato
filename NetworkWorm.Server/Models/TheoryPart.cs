namespace NetworkWorm.Server.Models
{
    public class TheoryPart
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public int DurationMinutes { get; set; }
    }
}