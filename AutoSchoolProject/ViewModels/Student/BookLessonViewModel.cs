using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoSchoolProject.ViewModels.Student
{
    public class BookLessonViewModel
    {
        public int InstructorId { get; set; }
        public string? InstructorName { get; set; }

        [Required(ErrorMessage = "Моля избери свободен час.")]
        public int SlotId { get; set; }

        public List<SelectListItem> AvailableSlots { get; set; } = new();

        public int StudentId { get; set; }
        public int? CourseId { get; set; }
    }
}
