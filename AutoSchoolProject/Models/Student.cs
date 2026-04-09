using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSchoolProject.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public int? CourseId { get; set; }
        public Course? Course { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("Still studying")]
        public string StillStudying { get; set; } = "Yes";

        public ICollection<PracticeLesson>? ScheduledLessons { get; set; }
        public ICollection<TestResultListovki>? TestResults { get; set; }
        public string? ProfileImage { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
