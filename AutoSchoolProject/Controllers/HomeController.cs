using System.Diagnostics;
using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index() => View();
        public IActionResult IndexForViewerOnly() => View();

        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(courses);
        }
        public async Task<IActionResult> CoursesInfoOnly()
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(courses);
        }
        public IActionResult Conditions() => View();

        public IActionResult Contacts() => View();

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            var vm = new EnrollmentRequestCreateViewModel
            {
                PreferredStartDate = DateTime.Today.AddDays(7),
                Courses = await _context.Courses
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.Name} - {c.Price:F2} лв"
                    })
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(EnrollmentRequestCreateViewModel model)
        {
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

                return View(model);
            }

            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists)
            {
                ModelState.AddModelError(nameof(model.CourseId), "Невалидна категория/курс.");

                model.Courses = await _context.Courses
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.Name} - {c.Price:F2} лв"
                    })
                    .ToListAsync();

                return View(model);
            }

           var request = new EnrollmentRequest
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                CourseId = model.CourseId,
                PreferredStartDate = model.PreferredStartDate.Date,
                Status = RequestStatus.New,
                CreatedAt = DateTime.UtcNow
            };

            _context.EnrollmentRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ApplySuccess));
        }

        public IActionResult ApplySuccess() => View();

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
