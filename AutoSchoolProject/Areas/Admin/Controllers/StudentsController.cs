using AutoSchoolProject.Data;
using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public StudentsController(ApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Course)
                .Where(s => s.StillStudying == "Yes")
                .Select(s => new StudentRowViewModel
                {
                    Id = s.Id,
                    FullName = ((s.User.FirstName ?? string.Empty) + " " + (s.User.LastName ?? string.Empty)).Trim(),
                    Email = s.User.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    CourseName = s.Course != null ? s.Course.Name : null,
                    LessonsCount = _context.PracticeLessons.Count(l => l.StudentId == s.Id),
                    ProfileImagePath = s.User.ProfileImagePath,
                    StillStudying = s.StillStudying
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return View(students);
        }

        public async Task<IActionResult> Finished()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Course)
                .Where(s => s.StillStudying == "No")
                .Select(s => new StudentRowViewModel
                {
                    Id = s.Id,
                    FullName = ((s.User.FirstName ?? string.Empty) + " " + (s.User.LastName ?? string.Empty)).Trim(),
                    Email = s.User.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    CourseName = s.Course != null ? s.Course.Name : null,
                    LessonsCount = _context.PracticeLessons.Count(l => l.StudentId == s.Id),
                    ProfileImagePath = s.User.ProfileImagePath,
                    StillStudying = s.StillStudying
                })
                .OrderBy(s => s.FullName)
                .ToListAsync();

            return View(students);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsFinished(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            if (student.StillStudying == "No")
            {
                TempData["Error"] = "Този курсист вече е преместен при завършилите / прекъсналите.";
                return RedirectToAction(nameof(Index));
            }

            student.StillStudying = "No";
            await _context.SaveChangesAsync();

            var fullName = ((student.User.FirstName ?? string.Empty) + " " + (student.User.LastName ?? string.Empty)).Trim();
            TempData["Success"] = $"Курсистът {fullName} беше преместен при завършилите / прекъсналите.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsStudying(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            student.StillStudying = "Yes";
            await _context.SaveChangesAsync();

            var fullName = ((student.User.FirstName ?? string.Empty) + " " + (student.User.LastName ?? string.Empty)).Trim();
            TempData["Success"] = $"Курсистът {fullName} беше върнат в активните курсисти.";
            return RedirectToAction(nameof(Finished));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            if (student.StillStudying == "No")
            {
                TempData["Error"] = "Този курсист е в списъка Завършили / прекъснали. Върни го в активните, ако искаш да го редактираш.";
                return RedirectToAction(nameof(Finished));
            }

            var courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Name} - {c.Price:F2} лв"
                })
                .ToListAsync();

            return View(new AdminEditStudentViewModel
            {
                StudentId = student.Id,
                UserId = student.UserId,
                FirstName = student.User.FirstName ?? string.Empty,
                LastName = student.User.LastName ?? string.Empty,
                Email = student.User.Email ?? string.Empty,
                PhoneNumber = student.User.PhoneNumber,
                CourseId = student.CourseId,
                Courses = courses,
                CurrentProfileImagePath = student.User.ProfileImagePath
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminEditStudentViewModel model)
        {
            if (id != model.StudentId) return BadRequest();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            if (student.StillStudying == "No")
            {
                TempData["Error"] = "Този курсист е в списъка Завършили / прекъснали. Върни го в активните, ако искаш да го редактираш.";
                return RedirectToAction(nameof(Finished));
            }

            if (!ModelState.IsValid)
            {
                model.Courses = await _context.Courses
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.Name} - {c.Price:F2} лв"
                    })
                    .ToListAsync();

                model.CurrentProfileImagePath = student.User.ProfileImagePath;
                return View(model);
            }

            if (model.ProfileImage != null)
            {
                var newPath = await _fileStorage.SaveImageAsync(model.ProfileImage, "uploads/profile", student.User.ProfileImagePath);
                student.User.ProfileImagePath = newPath;
            }

            student.User.FirstName = model.FirstName;
            student.User.LastName = model.LastName;
            student.User.Email = model.Email;
            student.User.UserName = model.Email;
            student.User.PhoneNumber = model.PhoneNumber;
            student.CourseId = model.CourseId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Промените са запазени.";
            return RedirectToAction(nameof(Index));
        }
    }
}
