using AutoSchoolProject.Models.Enums;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class EnrollmentRequestRowViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime PreferredStartDate { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
