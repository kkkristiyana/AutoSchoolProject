namespace AutoSchoolProject.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalLessons { get; set; }
        public int PendingLessons { get; set; }
        public int ApprovedUpcomingLessons { get; set; }
        public int CompletedLessons { get; set; }


        public List<LessonRowViewModel> LatestLessons { get; set; } = new();
    }

    public class LessonRowViewModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string? StudentName { get; set; }
        public string? InstructorName { get; set; }
        public string Status { get; set; } = "";
        public bool Completed { get; set; }
    }
}
