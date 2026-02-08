namespace AutoSchoolProject.Models
{
    public class TheorySession
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime DateTime { get; set; }
        public int DurationMinutes { get; set; } = 90;

        public string Topic { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
