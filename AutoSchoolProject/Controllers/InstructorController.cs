using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Services;
using AutoSchoolProject.ViewModels.Instructor;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoSchoolProject.Controllers
{
    public class InstructorController : Controller
    {
        private readonly InstructorService _instructorService;
        private readonly ApplicationDbContext _context;
        

        public InstructorController(InstructorService instructorService, ApplicationDbContext context)
        {
            _instructorService = instructorService;
            _context = context;
        }

        public IActionResult Details(int id)
        {
            var model = _instructorService.GetInstructorDetails(id);

            if (model == null)
                return NotFound();

            return View(model);
        }
        public async Task<IActionResult> Index()
        {
            var lessons = await _context.PracticeLessons
                .Include(l => l.Student)
                .Where(l => l.Instructor.UserId== User.FindFirstValue(ClaimTypes.NameIdentifier))
                .ToListAsync();

            var model = new InstructorDashboardViewModel
            {
                InstructorName = User.Identity.Name,
                UpcomingLessons = lessons.Where(l => l.DateTime > DateTime.Now).ToList(),
                PendingLessons = lessons.Where(l => l.Status == 0).ToList(),
                CompletedLessonsCount = lessons.Count(l => l.Completed == true)
            };

            return View(model);
        }
        public async Task<IActionResult> Create()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .ToListAsync();

            var model = new CreateLessonViewModel
            {
                Students = students.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.User.FirstName + " " + s.User.LastName
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLessonViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var lesson = new PracticeLesson
            {
                StudentId = model.StudentId,
                DateTime = model.DateTime,
                DurationMinutes = model.DurationMinutes,
                Status = 0
            };

            _context.Add(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Reschedule(int id)
        {
            var lesson = await _context.PracticeLessons
                .Include(l => l.Student)
                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null)
                return NotFound();

            var model = new RescheduleLessonViewModel
            {
                LessonId = lesson.Id,
                StudentName = lesson.Student.User.FirstName + " " + lesson.Student.User.LastName,
                CurrentDateTime = lesson.DateTime,
                NewDateTime = lesson.DateTime,
                DurationMinutes = lesson.DurationMinutes
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(RescheduleLessonViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var lesson = await _context.PracticeLessons.FindAsync(model.LessonId);

            if (lesson == null)
                return NotFound();

            lesson.DateTime = model.NewDateTime;
            lesson.DurationMinutes = model.DurationMinutes;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Approve(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);

            lesson.Status = Models.Enums.LessonStatus.Pending;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Complete(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);

            lesson.Status = Models.Enums.LessonStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Schedule()
        {
            return View();
        }
    }
}
