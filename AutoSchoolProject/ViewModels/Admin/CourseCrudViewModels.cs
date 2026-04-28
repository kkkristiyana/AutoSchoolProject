using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class AdminCourseRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int RequiredPracticeLessons { get; set; }
        public int StudentsCount { get; set; }
        public int InstructorsCount { get; set; }
    }

    public class AdminCourseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Името е задължително.")]
        [StringLength(100, ErrorMessage = "Името не може да бъде по-дълго от 100 символа.")]
        [Display(Name = "Име")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Цената е задължителна.")]
        [Range(typeof(decimal), "0,01", "100000", ErrorMessage = "Цената трябва да бъде по-голяма от 0.")]
        [Display(Name = "Цена (лв)")]
        public decimal Price { get; set; }

        [Range(1, 200, ErrorMessage = "Броят задължителни часове трябва да е между 1 и 200.")]
        [Display(Name = "Задължителни часове практика")]
        public int RequiredPracticeLessons { get; set; } = 31;
    }
}