using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class CreateAvailableSlotViewModel
    {
        [Required]
        [Display(Name = "Инструктор")]
        public int? InstructorId { get; set; }

        public List<SelectListItem> Instructors { get; set; } = new();

        [Required]
        [Display(Name = "Дата и час")]
        public DateTime DateTime { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [Range(30, 240)]
        [Display(Name = "Продължителност (минути)")]
        public int DurationMinutes { get; set; } = 50;

        [Display(Name = "Категория (по избор)")]
        public int? CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();

        [Display(Name = "Бележка (по избор)")]
        public string? Note { get; set; }
    }
}
