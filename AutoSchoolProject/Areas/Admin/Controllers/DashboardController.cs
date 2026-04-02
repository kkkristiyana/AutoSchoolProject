using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorage;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorage)
        {
            _context = context;
            _userManager = userManager;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var currentUser = await _userManager.GetUserAsync(User);

            var vm = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalInstructors = await _context.Instructors.CountAsync(),
                TotalLessons = await _context.PracticeLessons.CountAsync(),
                PendingLessons = await _context.PracticeLessons.CountAsync(l => l.Status == LessonStatus.Pending),
                ApprovedUpcomingLessons = await _context.PracticeLessons.CountAsync(l => l.Status == LessonStatus.Approved && l.DateTime >= now),
                CompletedLessons = await _context.PracticeLessons.CountAsync(l => l.Completed),
                //CurrentProfileImagePath = currentUser?.ProfileImagePath,
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
