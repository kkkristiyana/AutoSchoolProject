using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class ApproveEnrollmentRequestViewModel
    {
        [Required]
        public int RequestId { get; set; }

        // само за показване на резюме
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime PreferredStartDate { get; set; }

        [Required(ErrorMessage = "Въведи email.")]
        [EmailAddress(ErrorMessage = "Невалиден email.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Въведи парола.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне 6 символа.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Потвърди паролата.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? AdminNote { get; set; }
    }
}
