using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            string? linkedEmail = null;
            string? assignedRole = null;
            if (!string.IsNullOrWhiteSpace(req.CreatedStudentUserId))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == req.CreatedStudentUserId);
                if (user != null)
                {
                    linkedEmail = user.Email;
                    var roles = await _userManager.GetRolesAsync(user);
                    assignedRole = roles.FirstOrDefault(r => r == "Student" || r == "Instructor" || r == "Admin")
                                   ?? roles.FirstOrDefault();
                }
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
                CreatedStudentEmail = linkedEmail,
                AssignedRole = assignedRole
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

            var linkedUser = !string.IsNullOrWhiteSpace(req.CreatedStudentUserId)
                ? await _userManager.FindByIdAsync(req.CreatedStudentUserId)
                : null;

            var vm = new ApproveEnrollmentRequestViewModel
            {
                RequestId = req.Id,
                FullName = req.FullName,
                PhoneNumber = req.PhoneNumber,
                CourseName = req.Course.Name,
                PreferredStartDate = req.PreferredStartDate,
                UserEmail = linkedUser?.Email ?? string.Empty,
                SelectedRole = "Student",
                AvailableRoles = GetAvailableRoleOptions()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApproveEnrollmentRequestViewModel model)
        {
            model.AvailableRoles = GetAvailableRoleOptions();

            if (!ModelState.IsValid)
                return View(model);

            var req = await _context.EnrollmentRequests
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.Id == model.RequestId);

            if (req == null) return NotFound();

            if (req.Status != RequestStatus.New)
            {
                ModelState.AddModelError("", "Тази заявка вече е обработена.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(req.CreatedStudentUserId))
            {
                ModelState.AddModelError("", "Заявката не е свързана с влязъл потребител.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(req.CreatedStudentUserId);
            if (user == null)
            {
                ModelState.AddModelError("", "Свързаният потребител не беше намерен.");
                return View(model);
            }

            model.UserEmail = user.Email ?? user.UserName ?? string.Empty;

            var parts = (req.FullName ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.Length > 0 ? parts[0] : "User";
            var lastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;

            if (!await _roleManager.RoleExistsAsync(model.SelectedRole))
                await _roleManager.CreateAsync(new IdentityRole(model.SelectedRole));

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                user.Email = model.UserEmail;
                user.UserName = model.UserEmail;
                user.EmailConfirmed = true;
                user.PhoneNumber = req.PhoneNumber;
                user.FirstName = firstName;
                user.LastName = lastName;

                var updateRes = await _userManager.UpdateAsync(user);
                if (!updateRes.Succeeded)
                {
                    foreach (var e in updateRes.Errors)
                        ModelState.AddModelError("", e.Description);

                    await tx.RollbackAsync();
                    return View(model);
                }

                foreach (var role in new[] { "Student", "Instructor" })
                {
                    if (role != model.SelectedRole && await _userManager.IsInRoleAsync(user, role))
                    {
                        var removeRoleResult = await _userManager.RemoveFromRoleAsync(user, role);
                        if (!removeRoleResult.Succeeded)
                        {
                            foreach (var e in removeRoleResult.Errors)
                                ModelState.AddModelError("", e.Description);

                            await tx.RollbackAsync();
                            return View(model);
                        }
                    }
                }

                if (!await _userManager.IsInRoleAsync(user, model.SelectedRole))
                {
                    var roleRes = await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    if (!roleRes.Succeeded)
                    {
                        foreach (var e in roleRes.Errors)
                            ModelState.AddModelError("", e.Description);

                        await tx.RollbackAsync();
                        return View(model);
                    }
                }

                if (model.SelectedRole == "Student")
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                    if (student == null)
                    {
                        student = new Student
                        {
                            UserId = user.Id,
                            CourseId = req.CourseId
                        };
                        _context.Students.Add(student);
                    }
                    else
                    {
                        student.CourseId = req.CourseId;
                    }
                }
                else if (model.SelectedRole == "Instructor")
                {
                    var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == user.Id);
                    if (instructor == null)
                    {
                        instructor = new Instructor
                        {
                            UserId = user.Id,
                            CourseId = req.CourseId
                        };
                        _context.Instructors.Add(instructor);
                    }
                    else
                    {
                        instructor.CourseId = req.CourseId;
                    }
                }

                req.Status = RequestStatus.Approved;
                req.ProcessedAt = DateTime.UtcNow;
                req.AdminNote = model.AdminNote;
                req.CreatedStudentUserId = user.Id;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "Заявката е одобрена и ролята е зададена към съществуващия потребител.";
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

        private static List<SelectListItem> GetAvailableRoleOptions() => new()
        {
            new SelectListItem { Value = "Student", Text = "Курсист" },
            new SelectListItem { Value = "Instructor", Text = "Инструктор" }
        };

    }
}