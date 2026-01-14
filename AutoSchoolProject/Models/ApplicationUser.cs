using Microsoft.AspNetCore.Identity;

namespace AutoSchoolProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Instructor Instructor { get; set; }
        public Student Student { get; set; }
    }
}