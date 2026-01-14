namespace AutoSchoolProject.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<PracticeLesson> ScheduledLessons { get; set; }
        public ICollection<TestResultListovki> TestResults { get; set; }
    }
}