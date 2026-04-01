using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.Services;
using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Common;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public StudentController(IStudentService studentService, ApplicationDbContext context, IFileStorageService fileStorage)
    {
        _studentService = studentService;
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<IActionResult> Profile()
    {
        var model = await _studentService.GetProfileAsync(User);
        return View(model);
    }

    public async Task<IActionResult> Messages()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Profile));
        }

        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
        {
            TempData["Error"] = "Не е намерен курсист с този профил.";
            return RedirectToAction(nameof(Profile));
        }

        var adminMessages = await _context.EnrollmentRequests
            .Where(r => r.CreatedStudentUserId == userId
                        && r.Status != RequestStatus.New
                        && r.AdminNote != null
                        && r.AdminNote != "")
            .OrderByDescending(r => r.ProcessedAt ?? r.CreatedAt)
            .Select(r => new UserMessageItemViewModel
            {
                Title = r.Status == RequestStatus.Approved ? "Съобщение от администратора" : "Отговор на заявката за записване",
                Body = r.AdminNote!,
                CreatedAt = r.ProcessedAt ?? r.CreatedAt,
                Category = "Администрация"
            })
            .ToListAsync();

        var lessonRows = await _context.PracticeLessons
            .Include(l => l.Instructor)
                .ThenInclude(i => i.User)
            .Where(l => l.StudentId == student.Id && l.Note != null && l.Note != "")
            .OrderByDescending(l => l.DateTime)
            .ToListAsync();

        var lessonMessages = lessonRows
            .Where(l => LessonMessageFactory.IsStudentMessage(l.Note))
            .Select(l => new UserMessageItemViewModel
            {
                Title = $"Известие за час • {l.DateTime:dd.MM.yyyy HH:mm}",
                Body = LessonMessageFactory.StripPrefix(l.Note),
                CreatedAt = l.DateTime,
                Category = "Практика"
            })
            .ToList();

        var model = new UserMessagesViewModel
        {
            Heading = "Съобщения",
            EmptyMessage = "Все още нямаш съобщения.",
            Messages = adminMessages
                .Concat(lessonMessages)
                .OrderByDescending(m => m.CreatedAt)
                .ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var model = await _studentService.GetEditProfileAsync(User);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditStudentProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var refreshed = await _studentService.GetEditProfileAsync(User);
            model.CourseName = refreshed.CourseName;
            model.CurrentProfileImagePath = refreshed.CurrentProfileImagePath;
            return View(model);
        }

        if (model.ProfileImage != null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                var newPath = await _fileStorage.SaveImageAsync(model.ProfileImage, "uploads/profile", user.ProfileImagePath);
                model.ProfileImagePath = newPath;
            }
        }

        await _studentService.UpdateProfileAsync(User, model);
        TempData["Success"] = "Профилът е обновен.";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> Instructors()
    {
        var model = await _studentService.GetInstructorsAsync(User);
        return View(model);
    }

    public async Task<IActionResult> InstructorDetails(int id)
    {
        try
        {
            var model = await _studentService.GetInstructorDetailsAsync(User, id);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Instructors));
        }
    }

    [HttpGet]
    public async Task<IActionResult> BookLesson(int instructorId)
    {
        try
        {
            var model = await _studentService.GetBookLessonAsync(User, instructorId);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Instructors));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookLesson(BookLessonViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var refreshed = await _studentService.GetBookLessonAsync(User, model.InstructorId);
            model.AvailableSlots = refreshed.AvailableSlots;
            model.InstructorName = refreshed.InstructorName;
            return View(model);
        }

        try
        {
            await _studentService.BookLessonAsync(User, model);
            TempData["Success"] = "Часът е запазен.";
            return RedirectToAction(nameof(MyLessons));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);

            var refreshed = await _studentService.GetBookLessonAsync(User, model.InstructorId);
            model.AvailableSlots = refreshed.AvailableSlots;
            model.InstructorName = refreshed.InstructorName;
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> InstructorSchedule(int instructorId, DateTime start, DateTime end)
    {
        var lessons = await _studentService.GetInstructorLessonsAsync(instructorId, start, end);

        var events = lessons.Select(l => new
        {
            id = l.Id,
            title = l.Status == AutoSchoolProject.Models.Enums.LessonStatus.Available ? "Свободно" :
                    l.Status == AutoSchoolProject.Models.Enums.LessonStatus.Pending ? "Заявка" : "Заето",
            start = l.DateTime,
            end = l.DateTime.AddMinutes(l.DurationMinutes)
        });

        return Json(events);
    }

    public async Task<IActionResult> MyLessons()
    {
        var model = await _studentService.GetMyLessonsAsync(User);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelLesson(int id)
    {
        try
        {
            await _studentService.CancelLessonAsync(User, id);
            TempData["Success"] = "Часът е отменен и слотът отново е свободен.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(MyLessons));
    }

    public async Task<IActionResult> TestResults()
    {
        var model = await _studentService.GetTestResultsAsync(User);
        return View(model);
    }

    public async Task<IActionResult> Schedule()
    {
        var model = await _studentService.GetScheduleAsync(User);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetInstructorLessons(int instructorId)
    {
        var lessons = await _studentService.GetInstructorLessonsAsync(instructorId);

        var result = lessons.Select(l => new
        {
            title = "Заето",
            DateTime = l.DateTime,
            DurationMinutes = l.DurationMinutes,
            Status = l.Status.ToString()
        });

        return Json(result);
    }
}
