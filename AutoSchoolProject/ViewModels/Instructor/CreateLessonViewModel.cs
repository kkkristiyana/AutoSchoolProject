using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoSchoolProject.ViewModels.Instructor
{
    public class CreateLessonViewModel
    {
        [Required]
        [Display(Name = "Курсист")]
        public int StudentId { get; set; }

        public List<SelectListItem> Students { get; set; }

        [Required]
        [Display(Name = "Дата и час")]
        public DateTime DateTime { get; set; }

        [Required]
        [Range(30, 240)]
        [Display(Name = "Продължителност (минути)")]
        public int DurationMinutes { get; set; }

        public int CourseId { get; set; }
    }
}
