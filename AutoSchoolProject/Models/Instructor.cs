namespace AutoSchoolProject.Models
{
    public class Instructor
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int? CourseId { get; set; }
        public Course? Course { get; set; }
        public string? CarModel { get; set; }
        public string? CarImagePath { get; set; }

        public ICollection<PracticeLesson> PracticeLessons { get; set; }
    }
}