using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Instructor
{
    public class MarkCompletedViewModel
    {
        public int LessonId { get; set; }

        [Display(Name = "Бележка")]
        public string Note { get; set; }
    }
}
