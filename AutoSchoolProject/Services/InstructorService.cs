using AutoSchoolProject.Data;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Services
{
    public class InstructorService
    {
        private readonly ApplicationDbContext _context;

        public InstructorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public InstructorDetailsViewModel GetInstructorDetails(int instructorId)
        {
            var instructor = _context.Instructors
                .Include(i => i.User)
                .FirstOrDefault(i => i.Id == instructorId);

            if (instructor == null)
                return null;

            return new InstructorDetailsViewModel
            {
                InstructorId = instructor.Id,
                FullName = $"{instructor.User.FirstName} {instructor.User.LastName}",
                Email = instructor.User.Email,
                PhoneNumber = instructor.User.PhoneNumber,
                Car = "Toyota Yaris (Manual)"
            };
        }
    }
}
