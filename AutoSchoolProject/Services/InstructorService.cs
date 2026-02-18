using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.ViewModels.Instructor;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        private async Task<Instructor> GetInstructorAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.Instructors
                .Include(i => i.User)
                .FirstAsync(i => i.UserId == userId);
        }
        public async Task<InstructorDashboardViewModel> GetDashboardAsync(ClaimsPrincipal user)
        {
            var instructor = await GetInstructorAsync(user);

            var lessons = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructor.Id)
                .Include(l => l.Student).ThenInclude(s => s.User)
                .ToListAsync();

            return new InstructorDashboardViewModel
            {
                CompletedLessonsCount = lessons.Count(l => l.Completed),
                UpcomingLessons = lessons
                    .Where(l => l.DateTime >= DateTime.Now)
                    .OrderBy(l => l.DateTime)
                    .ToList()
            };
        }

    }
}
