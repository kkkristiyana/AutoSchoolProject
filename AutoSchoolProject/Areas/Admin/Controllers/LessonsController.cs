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
    public class LessonsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LessonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status)
        {
            LessonStatus? filter = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(status, out LessonStatus parsed))
                filter = parsed;

            var query = _context.PracticeLessons
                .Include(l => l.Student).ThenInclude(s => s.User)
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .AsQueryable();

            if (filter.HasValue)
                query = query.Where(l => l.Status == filter.Value);

            var lessons = await query
                .OrderByDescending(l => l.DateTime)
                .Select(l => new LessonRowViewModel
                {
                    Id = l.Id,
                    DateTime = l.DateTime,
                    StudentName = l.Student != null ? (l.Student.User.FirstName + " " + l.Student.User.LastName) : null,
                    InstructorName = l.Instructor != null ? (l.Instructor.User.FirstName + " " + l.Instructor.User.LastName) : null,
                    Status = l.Status.ToString(),
                    Completed = l.Completed
                })
                .ToListAsync();

            ViewBag.Status = status ?? "";
            return View(lessons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            lesson.Status = LessonStatus.Approved;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            lesson.Status = LessonStatus.Rejected;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            lesson.Status = LessonStatus.Cancelled;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}