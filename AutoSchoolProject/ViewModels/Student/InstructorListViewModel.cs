namespace AutoSchoolProject.ViewModels.Student
{
    public class InstructorListViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Category { get; set; }
        public string? PhoneNumber { get; internal set; }
        public string? Email { get; internal set; }
        public string CourseName { get; internal set; }
    }
}
