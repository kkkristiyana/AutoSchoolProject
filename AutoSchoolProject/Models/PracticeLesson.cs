using AutoSchoolProject.Models.Enums;

namespace AutoSchoolProject.Models
{
    public class PracticeLesson
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }

        public int? InstructorId { get; set; }
        public Instructor Instructor { get; set; }

        public int? StudentId { get; set; }
        public Student Student { get; set; }

        public int? CourseId { get; set; }
        public Course Course { get; set; }

        public bool Completed { get; set; }
        public string Note { get; set; }
        public LessonStatus Status { get; set; }
        public int DurationMinutes { get; set; } =50;
    }
}