using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new AdminCourseRowViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Price = c.Price,
                    RequiredPracticeLessons = c.RequiredPracticeLessons,
                    StudentsCount = _context.Students.Count(s => s.CourseId == c.Id),
                    InstructorsCount = _context.Instructors.Count(i => i.CourseId == c.Id)
                })
                .ToListAsync();

            return View(courses);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminCourseFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCourseFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var entity = new Course
            {
                Name = model.Name.Trim(),
                Price = model.Price,
                RequiredPracticeLessons = model.RequiredPracticeLessons
            };

            _context.Courses.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Курсът е създаден.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            return View(new AdminCourseFormViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Price = course.Price,
                RequiredPracticeLessons = course.RequiredPracticeLessons
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminCourseFormViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            course.Name = model.Name.Trim();
            course.Price = model.Price;
            course.RequiredPracticeLessons = model.RequiredPracticeLessons;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Промените са запазени.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var vm = new AdminCourseRowViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Price = course.Price,
                RequiredPracticeLessons = course.RequiredPracticeLessons,
                StudentsCount = await _context.Students.CountAsync(s => s.CourseId == course.Id),
                InstructorsCount = await _context.Instructors.CountAsync(i => i.CourseId == course.Id)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var students = await _context.Students.CountAsync(s => s.CourseId == id);
            var instructors = await _context.Instructors.CountAsync(i => i.CourseId == id);
            var lessons = await _context.PracticeLessons.CountAsync(l => l.CourseId == id);
            var theory = await _context.TheorySessions.CountAsync(t => t.CourseId == id);

            if (students > 0 || instructors > 0 || lessons > 0 || theory > 0)
            {
                TempData["Error"] = "Не можеш да изтриеш курс, който се използва (има ученици/инструктори/часове/теория).";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Курсът е изтрит.";
            return RedirectToAction(nameof(Index));
        }
    }
}
