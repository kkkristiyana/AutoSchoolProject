using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    public async Task<IActionResult> Profile()
    {
        var model = await _studentService.GetProfileAsync(User);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var model = await _studentService.GetEditProfileAsync(User);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(EditStudentProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _studentService.UpdateProfileAsync(User, model);
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> Instructors()
    {
        var model = await _studentService.GetInstructorsAsync();
        return View(model);
    }

    public async Task<IActionResult> InstructorDetails(int id)
    {
        var model = await _studentService.GetInstructorDetailsAsync(id);
        return View(model);
    }

    public async Task<IActionResult> BookLesson(int instructorId)
    {
        var model = await _studentService.GetBookLessonAsync(User, instructorId);
        return View(model);
    }

    [HttpPost]
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
            TempData["Success"] = "Часът е отменен.";
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
