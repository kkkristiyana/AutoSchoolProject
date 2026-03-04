namespace AutoSchoolProject.ViewModels.Student
{
    public class InstructorDetailsViewModel
    {
        public int InstructorId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string SchoolName { get; set; } = "Автошкола Lucky-Cars EOOD";

        public string? ProfileImagePath { get; set; }

        public string? CarModel { get; set; }
        public string? CarImagePath { get; set; }
    }
}
