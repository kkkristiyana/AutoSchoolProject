using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class AdminEditStudentViewModel
    {
        public int StudentId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required, Display(Name = "Име")]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Категория")]
        public int? CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();

        public string? CurrentProfileImagePath { get; set; }

        [Display(Name = "Профилна снимка")]
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImagePath { get; set; }
    }

    public class AdminEditInstructorViewModel
    {
        public int InstructorId { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required, Display(Name = "Име")]
        public string FirstName { get; set; } = string.Empty;

        [Required, Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Категория")]
        public int? CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();

        [Display(Name = "Модел кола")]
        public string? CarModel { get; set; }

        public string? CurrentProfileImagePath { get; set; }
        public string? CurrentCarImagePath { get; set; }

        [Display(Name = "Профилна снимка")]
        public IFormFile? ProfileImage { get; set; }

        [Display(Name = "Снимка на колата")]
        public IFormFile? CarImage { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? CarImagePath { get; set; }
    }
}
