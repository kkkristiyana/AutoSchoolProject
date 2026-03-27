using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [HttpGet]
        public async Task<IActionResult> CreateSlot()
        {
            var vm = new CreateAvailableSlotViewModel
            {
                Instructors = await GetInstructorSelectListAsync(),
                Courses = await GetCourseSelectListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSlot(CreateAvailableSlotViewModel model)
        {
            if (model.DateTime < DateTime.Now)
                ModelState.AddModelError(nameof(model.DateTime), "Не можеш да създаваш слот в миналото.");

            if (!ModelState.IsValid)
            {
                model.Instructors = await GetInstructorSelectListAsync();
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            var start = model.DateTime;
            var end = model.DateTime.AddMinutes(model.DurationMinutes);

            bool overlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.InstructorId == model.InstructorId &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < end &&
                start < l.DateTime.AddMinutes(l.DurationMinutes));

            if (overlaps)
            {
                ModelState.AddModelError("", "Има припокриване с друг час за този инструктор.");
                model.Instructors = await GetInstructorSelectListAsync();
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            var slot = new PracticeLesson
            {
                InstructorId = model.InstructorId,
                StudentId = null,
                CourseId = model.CourseId,
                DateTime = model.DateTime,
                DurationMinutes = model.DurationMinutes,
                Status = LessonStatus.Available,
                Completed = false,
                Note = string.IsNullOrWhiteSpace(model.Note) ? "Свободен слот" : model.Note
            };

            _context.PracticeLessons.Add(slot);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Слотът е създаден.";
            return RedirectToAction(nameof(Index), new { status = LessonStatus.Available.ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            if (!CanApprove(lesson))
            {
                TempData["Error"] = "Само чакащ час може да бъде приет.";
                return RedirectToAction(nameof(Index));
            }

            lesson.Status = LessonStatus.Approved;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът е одобрен.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            if (!CanReject(lesson))
            {
                TempData["Error"] = "Само чакащ час може да бъде отказан.";
                return RedirectToAction(nameof(Index));
            }

            lesson.Status = LessonStatus.Rejected;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът е отказан.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var lesson = await _context.PracticeLessons.FindAsync(id);
            if (lesson == null) return NotFound();

            if (!CanCancel(lesson))
            {
                TempData["Error"] = "Този час не може да бъде отменен.";
                return RedirectToAction(nameof(Index));
            }

            lesson.Status = LessonStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът е отменен.";
            return RedirectToAction(nameof(Index));
        }

        private static bool CanApprove(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.StudentId.HasValue
               && lesson.Status == LessonStatus.Pending
               && lesson.DateTime > DateTime.Now;

        private static bool CanReject(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.StudentId.HasValue
               && lesson.Status == LessonStatus.Pending
               && lesson.DateTime > DateTime.Now;

        private static bool CanCancel(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.DateTime > DateTime.Now
               && (lesson.Status == LessonStatus.Available
                   || lesson.Status == LessonStatus.Pending
                   || lesson.Status == LessonStatus.Approved);

        private async Task<List<SelectListItem>> GetInstructorSelectListAsync()
        {
            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .AsNoTracking()
                .OrderBy(i => i.User.FirstName)
                .ThenBy(i => i.User.LastName)
                .Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = $"{i.User.FirstName} {i.User.LastName} ({(i.Course != null ? i.Course.Name : "без категория")})"
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetCourseSelectListAsync()
        {
            return await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
    }
}
