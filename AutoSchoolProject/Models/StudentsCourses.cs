namespace AutoSchoolProject.Models
{
    public class StudentsCourses
    {
        public int StudentId { get; set; }
        public string CourseId { get; set; }
        public DateOnly DateStarted { get; set; }
    }
}
