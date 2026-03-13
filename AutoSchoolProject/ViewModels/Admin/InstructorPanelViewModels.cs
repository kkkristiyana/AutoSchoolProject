using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class InstructorPanelIndexViewModel
    {
        public string? Query { get; set; }
        public List<InstructorPanelInstructorRowViewModel> Instructors { get; set; } = new();
    }

    public class InstructorPanelInstructorRowViewModel
    {
        public int InstructorId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CourseName { get; set; }
        public string? ProfileImagePath { get; set; }
    }

    public class InstructorPanelProfileViewModel
    {
        public int InstructorId { get; set; }
        public string FullName { get; set; } = string.Empty;
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

    public class InstructorPanelLessonsViewModel
    {
        public string? Status { get; set; }
        public List<InstructorPanelLessonRowViewModel> Lessons { get; set; } = new();
    }

    public class InstructorPanelLessonRowViewModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? StudentName { get; set; }
        public string? CourseName { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public string? Note { get; set; }
    }

    public class InstructorPanelCreateSlotViewModel
    {
        [Required]
        [Display(Name = "Дата и час")]
        public DateTime DateTime { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [Range(30, 240, ErrorMessage = "Продължителността трябва да е между 30 и 240 минути.")]
        [Display(Name = "Продължителност (минути)")]
        public int DurationMinutes { get; set; } = 50;

        [Display(Name = "Категория")]
        public int? CourseId { get; set; }

        public List<SelectListItem> Courses { get; set; } = new();

        [Display(Name = "Бележка")]
        public string? Note { get; set; }
    }

    public class InstructorPanelRescheduleLessonViewModel
    {
        public int LessonId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime CurrentDateTime { get; set; }

        [Required]
        [Display(Name = "Нова дата и час")]
        public DateTime NewDateTime { get; set; }

        [Required]
        [Range(30, 240, ErrorMessage = "Продължителността трябва да е между 30 и 240 минути.")]
        [Display(Name = "Продължителност (минути)")]
        public int DurationMinutes { get; set; }
    }

    public class InstructorPanelCompleteLessonViewModel
    {
        public int LessonId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime LessonDateTime { get; set; }

        [Display(Name = "Бележка към курсиста")]
        public string? Note { get; set; }
    }

    public class InstructorPanelStudentsViewModel
    {
        public List<InstructorPanelStudentProgressRowViewModel> Students { get; set; } = new();
    }

    public class InstructorPanelStudentProgressRowViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? CourseName { get; set; }
        public int CompletedLessons { get; set; }
        public int RemainingLessons { get; set; }
        public DateTime? LastLessonDate { get; set; }
    }
}