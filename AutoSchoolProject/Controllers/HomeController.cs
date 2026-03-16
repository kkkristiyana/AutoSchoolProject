using System.Diagnostics;
using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Public;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoSchoolProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
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
        public IActionResult ConditionsViewerOnly() => View();
        public IActionResult Contacts() => View();
        public IActionResult ContactsViewerOnly() => View();

        [HttpGet]
        public async Task<IActionResult> Apply()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action(nameof(Apply), "Home") });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action(nameof(Apply), "Home") });
            }

            var fullName = string.Join(" ", new[] { currentUser.FirstName, currentUser.LastName }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            var vm = new EnrollmentRequestCreateViewModel
            {
                FullName = fullName,
                PhoneNumber = currentUser.PhoneNumber ?? string.Empty,
                Email = currentUser.Email ?? currentUser.UserName ?? string.Empty,
                IsEmailReadOnly = true,
                PreferredStartDate = DateTime.Today.AddDays(7),
                Courses = await GetCoursesAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(EnrollmentRequestCreateViewModel model)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action(nameof(Apply), "Home") });
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action(nameof(Apply), "Home") });
            }

            model.Email = currentUser.Email ?? currentUser.UserName ?? string.Empty;
            model.IsEmailReadOnly = true;

            if (!ModelState.IsValid)
            {
                model.Courses = await GetCoursesAsync();
                return View(model);
            }

            var courseExists = await _context.Courses.AnyAsync(c => c.Id == model.CourseId);
            if (!courseExists)
            {
                ModelState.AddModelError(nameof(model.CourseId), "Невалидна категория/курс.");
                model.Courses = await GetCoursesAsync();
                return View(model);
            }

            var request = new EnrollmentRequest
            {
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber.Trim(),
                CourseId = model.CourseId,
                PreferredStartDate = model.PreferredStartDate.Date,
                Status = RequestStatus.New,
                CreatedAt = DateTime.UtcNow,
                CreatedStudentUserId = currentUser.Id
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

        private Task<List<SelectListItem>> GetCoursesAsync()
        {
            return _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Name} - {c.Price:F2} лв"
                })
                .ToListAsync();
        }
    }
}