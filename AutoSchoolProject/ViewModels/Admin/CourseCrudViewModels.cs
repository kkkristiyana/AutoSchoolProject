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

        [Required, StringLength(100)]
        [Display(Name = "Име")]
        public string Name { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "100000")]
        [Display(Name = "Цена (лв)")]
        public decimal Price { get; set; }

        [Range(1, 200)]
        [Display(Name = "Задължителни часове практика")]
        public int RequiredPracticeLessons { get; set; } = 31;
    }
}
