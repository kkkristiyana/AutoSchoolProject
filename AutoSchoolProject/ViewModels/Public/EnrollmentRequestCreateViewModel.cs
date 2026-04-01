using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoSchoolProject.ViewModels.Public
{
    public class EnrollmentRequestCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Въведи име и фамилия.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "Името трябва да е между 3 и 80 символа.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Въведи телефон.")]
        [Phone(ErrorMessage = "Невалиден телефон.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Липсва имейлът на влезлия потребител.")]
        [EmailAddress(ErrorMessage = "Невалиден имейл.")]
        public string Email { get; set; } = string.Empty;

        public bool IsEmailReadOnly { get; set; }

        [Required(ErrorMessage = "Избери категория/курс.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Избери предпочитана дата за старт.")]
        [DataType(DataType.Date)]
        public DateTime PreferredStartDate { get; set; } = DateTime.Today.AddDays(7);

        public List<SelectListItem> Courses { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PreferredStartDate.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Предпочитаната начална дата не може да е в миналото.",
                    new[] { nameof(PreferredStartDate) });
            }
        }
    }
}
