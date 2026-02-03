using AutoSchoolProject.Data;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var vm = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalInstructors = await _context.Instructors.CountAsync(),
                TotalLessons = await _context.PracticeLessons.CountAsync(),
                PendingLessons = await _context.PracticeLessons.CountAsync(l => l.Status == LessonStatus.Pending),
                ApprovedUpcomingLessons = await _context.PracticeLessons.CountAsync(l => l.Status == LessonStatus.Approved && l.DateTime >= now),
                CompletedLessons = await _context.PracticeLessons.CountAsync(l => l.Completed),
                LatestLessons = await _context.PracticeLessons
                    .Include(l => l.Student).ThenInclude(s => s.User)
                    .Include(l => l.Instructor).ThenInclude(i => i.User)
                    .OrderByDescending(l => l.DateTime)
                    .Take(10)
                    .Select(l => new LessonRowViewModel
                    {
                        Id = l.Id,
                        DateTime = l.DateTime,
                        StudentName = l.Student != null ? (l.Student.User.FirstName + " " + l.Student.User.LastName) : null,
                        InstructorName = l.Instructor != null ? (l.Instructor.User.FirstName + " " + l.Instructor.User.LastName) : null,
                        Status = l.Status.ToString(),
                        Completed = l.Completed
                    })
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}