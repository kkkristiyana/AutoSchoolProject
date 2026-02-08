using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Instructor
{
    public class RescheduleLessonViewModel
    {
        public int LessonId { get; set; }
        public string StudentName { get; set; }
        public DateTime CurrentDateTime { get; set; }

        [Required]
        [Display(Name = "Нова дата и час")]
        [DataType(DataType.DateTime)]
        public DateTime NewDateTime { get; set; }

        [Required]
        [Range(30, 240, ErrorMessage = "Продължителността трябва да е между 30 и 240 минути.")]
        [Display(Name = "Продължителност (минути)")]
        public int DurationMinutes { get; set; }

    }
}
