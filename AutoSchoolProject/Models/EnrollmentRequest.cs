using AutoSchoolProject.Models.Enums;

namespace AutoSchoolProject.Models
{
    public class EnrollmentRequest
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public DateTime PreferredStartDate { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        public string? AdminNote { get; set; }

        public string? CreatedStudentUserId { get; set; }
    }
}
