namespace AutoSchoolProject.ViewModels.Student
{
    public class MyLessonListItemViewModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string? InstructorName { get; set; }
        public string? CourseName { get; set; }
        public string Status { get; set; } = "";
        public bool Completed { get; set; }
        public string? Note { get; set; }
    }
}
