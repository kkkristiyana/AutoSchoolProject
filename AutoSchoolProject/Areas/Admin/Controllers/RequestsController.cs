using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RequestsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? status)
        {
            RequestStatus? filter = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(status, out RequestStatus parsed))
                filter = parsed;

            var query = _context.EnrollmentRequests
                .Include(r => r.Course)
                .AsNoTracking()
                .AsQueryable();

            if (filter.HasValue)
                query = query.Where(r => r.Status == filter.Value);

            var rows = await query
                .OrderBy(r => r.Status)
                .ThenByDescending(r => r.CreatedAt)
                .Select(r => new EnrollmentRequestRowViewModel
                {
                    Id = r.Id,
                    FullName = r.FullName,
                    PhoneNumber = r.PhoneNumber,
                    CourseName = r.Course.Name,
                    PreferredStartDate = r.PreferredStartDate,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    ProcessedAt = r.ProcessedAt
                })
                .ToListAsync();

            ViewBag.Status = status ?? "";
            return View(rows);
        }

        public async Task<IActionResult> Details(int id)
        {
            var req = await _context.EnrollmentRequests
                .Include(r => r.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();

            string? createdEmail = null;
            if (!string.IsNullOrWhiteSpace(req.CreatedStudentUserId))
            {
                createdEmail = await _context.Users
                    .Where(u => u.Id == req.CreatedStudentUserId)
                    .Select(u => u.Email)
                    .FirstOrDefaultAsync();
            }

            var vm = new EnrollmentRequestDetailsViewModel
            {
                Id = req.Id,
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                CourseId = req.CourseId,
                CourseName = req.Course.Name,
                PreferredStartDate = req.PreferredStartDate,
                Status = req.Status,
                CreatedAt = req.CreatedAt,
                ProcessedAt = req.ProcessedAt,
                AdminNote = req.AdminNote,
                CreatedStudentUserId = req.CreatedStudentUserId,
                CreatedStudentEmail = createdEmail
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _context.EnrollmentRequests
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (req == null) return NotFound();
            if (req.Status != RequestStatus.New)
                return RedirectToAction(nameof(Details), new { id });

            var vm = new ApproveEnrollmentRequestViewModel
            {
                RequestId = req.Id,
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                CourseName = req.Course.Name,
                PreferredStartDate = req.PreferredStartDate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApproveEnrollmentRequestViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var req = await _context.EnrollmentRequests
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.Id == model.RequestId);

            if (req == null) return NotFound();

            if (req.Status != RequestStatus.New)
            {
                ModelState.AddModelError("", "Тази заявка вече е обработена.");
                return View(model);
            }

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Този email вече съществува.");
                return View(model);
            }

            //split name -> FirstName/LastName
            var parts = (req.FullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.Length > 0 ? parts[0] : "Student";
            var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : "User";

            //гарантираме роля Student
            if (!await _roleManager.RoleExistsAsync("Student"))
                await _roleManager.CreateAsync(new IdentityRole("Student"));

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    PhoneNumber = req.PhoneNumber,
                    FirstName = firstName,
                    LastName = lastName
                };

                var createRes = await _userManager.CreateAsync(user, model.Password);
                if (!createRes.Succeeded)
                {
                    foreach (var e in createRes.Errors)
                        ModelState.AddModelError("", e.Description);

                    await tx.RollbackAsync();
                    return View(model);
                }

                var roleRes = await _userManager.AddToRoleAsync(user, "Student");
                if (!roleRes.Succeeded)
                {
                    foreach (var e in roleRes.Errors)
                        ModelState.AddModelError("", e.Description);

                    await tx.RollbackAsync();
                    return View(model);
                }

                //създаваме Student профил към курса от заявката
                var student = new Student
                {
                    UserId = user.Id,
                    CourseId = req.CourseId
                };
                _context.Students.Add(student);

                //маркираме заявката
                req.Status = RequestStatus.Approved;
                req.ProcessedAt = DateTime.UtcNow;
                req.AdminNote = model.AdminNote;
                req.CreatedStudentUserId = user.Id;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return RedirectToAction(nameof(Details), new { id = req.Id });
            }
            catch
            {
                await tx.RollbackAsync();
                ModelState.AddModelError("", "Грешка при одобряване. Опитай отново.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _context.EnrollmentRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return NotFound();

            if (req.Status != RequestStatus.New)
                return RedirectToAction(nameof(Details), new { id });

            return View(new RejectEnrollmentRequestViewModel { RequestId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(RejectEnrollmentRequestViewModel model)
        {
            var req = await _context.EnrollmentRequests.FirstOrDefaultAsync(r => r.Id == model.RequestId);
            if (req == null) return NotFound();

            if (req.Status != RequestStatus.New)
                return RedirectToAction(nameof(Details), new { id = model.RequestId });

            req.Status = RequestStatus.Rejected;
            req.ProcessedAt = DateTime.UtcNow;
            req.AdminNote = model.AdminNote;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.RequestId });
        }
    }
}
