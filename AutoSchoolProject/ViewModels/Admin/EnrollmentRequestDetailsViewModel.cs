using AutoSchoolProject.Models.Enums;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class EnrollmentRequestDetailsViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        public DateTime PreferredStartDate { get; set; }

        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public string? AdminNote { get; set; }

        public string? CreatedStudentEmail { get; set; }
        public string? CreatedStudentUserId { get; set; }
    }
}
