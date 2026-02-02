using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

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
            return View(model);

        await _studentService.BookLessonAsync(User, model);
        return RedirectToAction(nameof(Profile));
    }
    [HttpGet]
    public async Task<IActionResult> InstructorSchedule(int instructorId, DateTime start, DateTime end)
    {
        var lessons = await _studentService.GetInstructorLessonsAsync(instructorId, start, end);

        var events = lessons.Select(l => new
        {
            id = l.Id,
            title = l.Status == AutoSchoolProject.Models.Enums.LessonStatus.Pending ? "Заявка" : "Заето",
            start = l.DateTime,
            end = l.DateTime.AddMinutes(l.DurationMinutes)
        });

        return Json(events);
    }
}
