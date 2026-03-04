using AutoSchoolProject.Data;
using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Admin;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentPanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IStudentService _studentService;
        private readonly IFileStorageService _fileStorage;

        public StudentPanelController(ApplicationDbContext context, IStudentService studentService, IFileStorageService fileStorage)
        {
            _context = context;
            _studentService = studentService;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(s =>
                    (s.User.FirstName + " " + s.User.LastName).ToLower().Contains(term) ||
                    (s.User.Email ?? "").ToLower().Contains(term) ||
                    (s.User.PhoneNumber ?? "").ToLower().Contains(term));
            }

            var students = await query
                .OrderBy(s => s.User.FirstName)
                .ThenBy(s => s.User.LastName)
                .Select(s => new StudentPanelStudentRowViewModel
                {
                    StudentId = s.Id,
                    FullName = s.User.FirstName + " " + s.User.LastName,
                    Email = s.User.Email,
                    PhoneNumber = s.User.PhoneNumber,
                    CourseName = s.Course != null ? s.Course.Name : null,
                    ProfileImagePath = s.User.ProfileImagePath
                })
                .ToListAsync();

            return View(new StudentPanelIndexViewModel
            {
                Query = q,
                Students = students
            });
        }

        private async Task<(ClaimsPrincipal Principal, string StudentName, int StudentId)> ImpersonateStudentAsync(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                throw new InvalidOperationException("Невалиден studentId.");

            var identity = new ClaimsIdentity("ImpersonatedStudent");
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, student.UserId));
            identity.AddClaim(new Claim(ClaimTypes.Name, student.User.Email ?? student.UserId));

            var principal = new ClaimsPrincipal(identity);
            var name = $"{student.User.FirstName} {student.User.LastName}";
            return (principal, name, student.Id);
        }

        private void SetStudentContext(int studentId, string studentName)
        {
            ViewBag.StudentId = studentId;
            ViewBag.StudentName = studentName;
        }

        public async Task<IActionResult> Profile(int studentId)
        {
            var (principal, name, id) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(id, name);
            var model = await _studentService.GetProfileAsync(principal);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(int studentId)
        {
            var (principal, name, id) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(id, name);
            var model = await _studentService.GetEditProfileAsync(principal);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int studentId, EditStudentProfileViewModel model)
        {
            var (principal, name, id) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(id, name);

            if (!ModelState.IsValid)
            {
                var refreshed = await _studentService.GetEditProfileAsync(principal);
                model.Courses = refreshed.Courses;
                model.CurrentProfileImagePath = refreshed.CurrentProfileImagePath;
                return View(model);
            }

            if (model.ProfileImage != null)
            {
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    var newPath = await _fileStorage.SaveImageAsync(model.ProfileImage, "uploads/profile", user.ProfileImagePath);
                    model.ProfileImagePath = newPath;
                }
            }

            await _studentService.UpdateProfileAsync(principal, model);
            TempData["Success"] = "Профилът е обновен (Admin Student Panel).";
            return RedirectToAction(nameof(Profile), new { studentId = id });
        }

        public async Task<IActionResult> Instructors(int studentId)
        {
            var (_, name, id) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(id, name);
            var model = await _studentService.GetInstructorsAsync();
            return View(model);
        }

        public async Task<IActionResult> InstructorDetails(int studentId, int id)
        {
            var (_, name, sid) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(sid, name);
            var model = await _studentService.GetInstructorDetailsAsync(id);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BookLesson(int studentId, int instructorId)
        {
            var (principal, name, sid) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(sid, name);
            var model = await _studentService.GetBookLessonAsync(principal, instructorId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookLesson(int studentId, BookLessonViewModel model)
        {
            var (principal, name, sid) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(sid, name);

            if (!ModelState.IsValid)
            {
                var refreshed = await _studentService.GetBookLessonAsync(principal, model.InstructorId);
                model.AvailableSlots = refreshed.AvailableSlots;
                model.InstructorName = refreshed.InstructorName;
                return View(model);
            }

            try
            {
                await _studentService.BookLessonAsync(principal, model);
                TempData["Success"] = "Часът е запазен (Admin Student Panel).";
                return RedirectToAction(nameof(MyLessons), new { studentId = sid });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var refreshed = await _studentService.GetBookLessonAsync(principal, model.InstructorId);
                model.AvailableSlots = refreshed.AvailableSlots;
                model.InstructorName = refreshed.InstructorName;
                return View(model);
            }
        }

        public async Task<IActionResult> MyLessons(int studentId)
        {
            var (principal, name, sid) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(sid, name);
            var model = await _studentService.GetMyLessonsAsync(principal);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelLesson(int studentId, int id)
        {
            var (principal, name, sid) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(sid, name);

            try
            {
                await _studentService.CancelLessonAsync(principal, id);
                TempData["Success"] = "Часът е отменен (Admin Student Panel).";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(MyLessons), new { studentId = sid });
        }

        public async Task<IActionResult> TestResults(int studentId)
        {
            var (principal, name, sid) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(sid, name);
            var model = await _studentService.GetTestResultsAsync(principal);
            return View(model);
        }

        public async Task<IActionResult> Schedule(int studentId)
        {
            var (principal, name, sid) = await ImpersonateStudentAsync(studentId);
            SetStudentContext(sid, name);
            var model = await _studentService.GetScheduleAsync(principal);
            return View(model);
        }
    }
}
