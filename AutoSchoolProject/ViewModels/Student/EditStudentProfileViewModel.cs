using Microsoft.AspNetCore.Http;
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

        [Display(Name = "Категория")]
        public string? CourseName { get; set; }

        public string? CurrentProfileImagePath { get; set; }

        [Display(Name = "Профилна снимка")]
        public IFormFile? ProfileImage { get; set; }

        public string? ProfileImagePath { get; set; }
    }
}
