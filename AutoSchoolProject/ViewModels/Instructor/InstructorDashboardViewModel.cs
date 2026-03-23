using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AutoSchoolProject.ViewModels.Instructor
{
    public class InstructorDashboardViewModel
    {
        public string InstructorName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CourseName { get; set; }
        public string? CarModel { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? CarImagePath { get; set; }
        public int PendingLessonsCount { get; set; }
        public int UpcomingApprovedLessonsCount { get; set; }
        public int CompletedLessonsCount { get; set; }
    }

    public class EditInstructorProfileViewModel
    {
        [Required(ErrorMessage = "Полето за име е задължително.")]
        [Display(Name = "Име")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето за фамилия е задължително.")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето за имейл е задължително.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
        [Display(Name = "Имейл")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Телефон")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Категория")]
        public string? CourseName { get; set; }

        [Display(Name = "Автомобил")]
        public string? CarModel { get; set; }

        [Display(Name = "Профилна снимка")]
        public IFormFile? ProfileImage { get; set; }

        [Display(Name = "Снимка на автомобила")]
        public IFormFile? CarImage { get; set; }

        public string? CurrentProfileImagePath { get; set; }
        public string? CurrentCarImagePath { get; set; }
    }

    public class InstructorCreateSlotViewModel
    {
        [Required(ErrorMessage = "Полето за дата и час е задължително.")]
        [Display(Name = "Дата и час")]
        public DateTime DateTime { get; set; }

        [Required(ErrorMessage = "Полето за продължителност е задължително.")]
        [Range(30, 240, ErrorMessage = "Продължителността трябва да е между 30 и 240 минути.")]
        [Display(Name = "Продължителност (минути)")]
        public int DurationMinutes { get; set; } = 50;

        [Display(Name = "Бележка")]
        public string? Note { get; set; }

        public string? CourseName { get; set; }
    }

    public class InstructorLessonRowViewModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? StudentName { get; set; }
        public string? CourseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusCssClass { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string? Note { get; set; }
    }

    public class InstructorPendingLessonsViewModel
    {
        public List<InstructorLessonRowViewModel> Lessons { get; set; } = new();
    }

    public class InstructorScheduleViewModel
    {
        public string? SelectedStatus { get; set; }
        public List<InstructorLessonRowViewModel> Lessons { get; set; } = new();
    }

    public class InstructorRescheduleLessonViewModel
    {
        public int LessonId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime CurrentDateTime { get; set; }

        [Required(ErrorMessage = "Полето за нова дата и час е задължително.")]
        [Display(Name = "Нова дата и час")]
        public DateTime NewDateTime { get; set; }

        [Required(ErrorMessage = "Полето за продължителност е задължително.")]
        [Range(30, 240, ErrorMessage = "Продължителността трябва да е между 30 и 240 минути.")]
        [Display(Name = "Продължителност (минути)")]
        public int DurationMinutes { get; set; }
    }

    public class InstructorCompleteLessonViewModel
    {
        public int LessonId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime LessonDateTime { get; set; }

        [Display(Name = "Бележка")]
        public string? Note { get; set; }
    }

    public class InstructorStudentRowViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public int CompletedLessons { get; set; }
        public int RemainingLessons { get; set; }
        public DateTime? LastLessonDate { get; set; }
    }

    public class InstructorStudentsViewModel
    {
        public List<InstructorStudentRowViewModel> Students { get; set; } = new();
    }
}