using AutoSchoolProject.Data;
using AutoSchoolProject.Models.Enums;
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
    public class InstructorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public InstructorsController(ApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .Where(i => i.IsWorking == "Yes")
                .Select(i => new InstructorRowViewModel
                {
                    Id = i.Id,
                    FullName = ((i.User.FirstName ?? string.Empty) + " " + (i.User.LastName ?? string.Empty)).Trim(),
                    Email = i.User.Email,
                    PhoneNumber = i.User.PhoneNumber,
                    CourseName = i.Course != null ? i.Course.Name : null,
                    UpcomingApprovedLessons = _context.PracticeLessons.Count(l =>
                        l.InstructorId == i.Id &&
                        l.Status == LessonStatus.Approved &&
                        l.DateTime >= now),
                    ProfileImagePath = i.User.ProfileImagePath,
                    CarImagePath = i.CarImagePath,
                    IsWorking = i.IsWorking
                })
                .OrderBy(i => i.FullName)
                .ToListAsync();

            return View(instructors);
        }

        public async Task<IActionResult> Inactive()
        {
            var now = DateTime.Now;

            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .Where(i => i.IsWorking == "No")
                .Select(i => new InstructorRowViewModel
                {
                    Id = i.Id,
                    FullName = ((i.User.FirstName ?? string.Empty) + " " + (i.User.LastName ?? string.Empty)).Trim(),
                    Email = i.User.Email,
                    PhoneNumber = i.User.PhoneNumber,
                    CourseName = i.Course != null ? i.Course.Name : null,
                    UpcomingApprovedLessons = _context.PracticeLessons.Count(l =>
                        l.InstructorId == i.Id &&
                        l.Status == LessonStatus.Approved &&
                        l.DateTime >= now),
                    ProfileImagePath = i.User.ProfileImagePath,
                    CarImagePath = i.CarImagePath,
                    IsWorking = i.IsWorking
                })
                .OrderBy(i => i.FullName)
                .ToListAsync();

            return View(instructors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsNotWorking(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            if (instructor.IsWorking == "No")
            {
                TempData["Error"] = "Този инструктор вече е в списъка Извън длъжност.";
                return RedirectToAction(nameof(Index));
            }

            instructor.IsWorking = "No";
            await _context.SaveChangesAsync();

            var fullName = ((instructor.User.FirstName ?? string.Empty) + " " + (instructor.User.LastName ?? string.Empty)).Trim();
            TempData["Success"] = $"Инструкторът {fullName} беше преместен в списъка Извън длъжност.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsWorking(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor == null)
            {
                return NotFound();
            }

            instructor.IsWorking = "Yes";
            await _context.SaveChangesAsync();

            var fullName = ((instructor.User.FirstName ?? string.Empty) + " " + (instructor.User.LastName ?? string.Empty)).Trim();
            TempData["Success"] = $"Инструкторът {fullName} беше върнат в активните инструктори.";
            return RedirectToAction(nameof(Inactive));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor == null) return NotFound();

            if (instructor.IsWorking == "No")
            {
                TempData["Error"] = "Този инструктор е в списъка Извън длъжност. Върни го на длъжност, ако искаш да го редактираш.";
                return RedirectToAction(nameof(Inactive));
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

            return View(new AdminEditInstructorViewModel
            {
                InstructorId = instructor.Id,
                UserId = instructor.UserId,
                FirstName = instructor.User.FirstName ?? string.Empty,
                LastName = instructor.User.LastName ?? string.Empty,
                Email = instructor.User.Email ?? string.Empty,
                PhoneNumber = instructor.User.PhoneNumber,
                CourseId = instructor.CourseId,
                Courses = courses,
                CarModel = instructor.CarModel,
                CurrentProfileImagePath = instructor.User.ProfileImagePath,
                CurrentCarImagePath = instructor.CarImagePath
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminEditInstructorViewModel model)
        {
            if (id != model.InstructorId) return BadRequest();

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (instructor == null) return NotFound();

            if (instructor.IsWorking == "No")
            {
                TempData["Error"] = "Този инструктор е в списъка Извън длъжност. Върни го на длъжност, ако искаш да го редактираш.";
                return RedirectToAction(nameof(Inactive));
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

                model.CurrentProfileImagePath = instructor.User.ProfileImagePath;
                model.CurrentCarImagePath = instructor.CarImagePath;
                return View(model);
            }

            if (model.ProfileImage != null)
            {
                var newPath = await _fileStorage.SaveImageAsync(model.ProfileImage, "uploads/profile", instructor.User.ProfileImagePath);
                instructor.User.ProfileImagePath = newPath;
            }

            if (model.CarImage != null)
            {
                var newPath = await _fileStorage.SaveImageAsync(model.CarImage, "uploads/cars", instructor.CarImagePath);
                instructor.CarImagePath = newPath;
            }

            instructor.User.FirstName = model.FirstName;
            instructor.User.LastName = model.LastName;
            instructor.User.Email = model.Email;
            instructor.User.UserName = model.Email;
            instructor.User.PhoneNumber = model.PhoneNumber;
            instructor.CourseId = model.CourseId;
            instructor.CarModel = model.CarModel;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Промените са запазени.";
            return RedirectToAction(nameof(Index));
        }
    }
}
