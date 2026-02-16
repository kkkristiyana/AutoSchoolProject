using System.ComponentModel.DataAnnotations;

namespace AutoSchoolProject.ViewModels.Admin
{
    public class RejectEnrollmentRequestViewModel
    {
        [Required]
        public int RequestId { get; set; }

        public string? AdminNote { get; set; }
    }
}
