using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Student
{

    public class BookLessonViewModel
    {
        public int InstructorId { get; set; }
        public string? InstructorName { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; } = DateTime.Today;
        public int StudentId { get; set; }
        public int? CourseId { get; set; }
    }
}
