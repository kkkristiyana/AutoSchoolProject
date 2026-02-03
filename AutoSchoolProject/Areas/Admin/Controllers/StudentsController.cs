using AutoSchoolProject.Data;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Course)
                .Select(s => new StudentRowViewModel
                {
                    Id = s.Id,
                    FullName = s.User.FirstName + " " + s.User.LastName,
                    Email = s.User.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    CourseName = s.Course != null ? s.Course.Name : null,
                    LessonsCount = _context.PracticeLessons.Count(l => l.StudentId == s.Id)
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return View(students);
        }
    }
}