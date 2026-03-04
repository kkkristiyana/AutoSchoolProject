using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Student
{
    public class EditStudentProfileViewModel
    {
        [Required]
        [Display(Name = "Име")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Категория")]
        public int? CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();

        public string? CurrentProfileImagePath { get; set; }

        [Display(Name = "Профилна снимка")]
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
