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
    public class InstructorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InstructorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .Select(i => new InstructorRowViewModel
                {
                    Id = i.Id,
                    FullName = i.User.FirstName + " " + i.User.LastName,
                    Email = i.User.Email,
                    PhoneNumber = i.User.PhoneNumber,
                    CourseName = i.Course != null ? i.Course.Name : null,
                    UpcomingApprovedLessons = _context.PracticeLessons.Count(l =>
                        l.InstructorId == i.Id &&
                        l.Status == LessonStatus.Approved &&
                        l.DateTime >= now)
                })
                .OrderBy(i => i.FullName)
                .ToListAsync();

            return View(instructors);
        }
    }
}