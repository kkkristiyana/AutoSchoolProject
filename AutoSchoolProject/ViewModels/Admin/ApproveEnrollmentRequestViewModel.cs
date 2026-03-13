using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class ApproveEnrollmentRequestViewModel
    {
        [Required]
        public int RequestId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime PreferredStartDate { get; set; }

        public string UserEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Избери роля за потребителя.")]
        public string SelectedRole { get; set; } = "Student";

        public List<SelectListItem> AvailableRoles { get; set; } = new();

        public string? AdminNote { get; set; }
    }
}
