namespace AutoSchoolProject.ViewModels.Student
{
    public class StudentProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string CourseName { get; set; }

        public int CompletedLessons { get; set; }
        public int RemainingLessons { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
