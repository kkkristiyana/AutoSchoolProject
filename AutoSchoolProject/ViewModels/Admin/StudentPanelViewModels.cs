namespace AutoSchoolProject.ViewModels.Admin
{
    public class StudentPanelIndexViewModel
    {
        public string? Query { get; set; }
        public List<StudentPanelStudentRowViewModel> Students { get; set; } = new();
    }

    public class StudentPanelStudentRowViewModel
    {
        public int StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CourseName { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
