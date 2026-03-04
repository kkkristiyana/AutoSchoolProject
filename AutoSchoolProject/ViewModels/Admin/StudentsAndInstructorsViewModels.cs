namespace AutoSchoolProject.ViewModels.Admin
{
    public class StudentRowViewModel
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CourseName { get; set; }
        public int LessonsCount { get; set; }
        public string? ProfileImagePath { get; set; }
    }

    public class InstructorRowViewModel
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CourseName { get; set; }
        public int UpcomingApprovedLessons { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? CarImagePath { get; set; }
    }
}
