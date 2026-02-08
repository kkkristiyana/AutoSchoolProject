namespace AutoSchoolProject.ViewModels.Student
{
    public class StudentScheduleViewModel
    {
        public string? CourseName { get; set; }

        public List<SchedulePracticeRowViewModel> Practice { get; set; } = new();
        public List<ScheduleTheoryRowViewModel> Theory { get; set; } = new();
    }

    public class SchedulePracticeRowViewModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string? InstructorName { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool Completed { get; set; }
    }

    public class ScheduleTheoryRowViewModel
    {
        public DateTime DateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string? Location { get; set; }
    }
}
