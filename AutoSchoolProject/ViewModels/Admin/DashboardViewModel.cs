namespace AutoSchoolProject.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int ActiveStudents { get; set; }
        public int FinishedStudents { get; set; }
        public int ActiveInstructors { get; set; }
        public int InactiveInstructors { get; set; }

        public int TotalLessons { get; set; }
        public int PendingLessons { get; set; }
        public int ApprovedUpcomingLessons { get; set; }
        public int CompletedLessons { get; set; }

        public List<LessonRowViewModel> LatestLessons { get; set; } = new();
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
    }

    public class LessonRowViewModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string? StudentName { get; set; }
        public string? InstructorName { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool Completed { get; set; }
    }
}
