using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoSchoolProject.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public InstructorController(ApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public IActionResult Index() => RedirectToAction(nameof(Profile));

        public async Task<IActionResult> Profile()
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var lessons = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructor.Id)
                .ToListAsync();

            var model = new InstructorDashboardViewModel
            {
                InstructorName = GetInstructorName(instructor),
                Email = instructor.User.Email,
                PhoneNumber = instructor.User.PhoneNumber,
                CourseName = instructor.Course?.Name,
                CarModel = instructor.CarModel,
                ProfileImagePath = instructor.User.ProfileImagePath,
                CarImagePath = instructor.CarImagePath,
                PendingLessonsCount = lessons.Count(l => l.Status == LessonStatus.Pending),
                UpcomingApprovedLessonsCount = lessons.Count(l => l.Status == LessonStatus.Approved && l.DateTime >= DateTime.Now),
                CompletedLessonsCount = lessons.Count(l => l.Completed)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var model = new EditInstructorProfileViewModel
            {
                FirstName = instructor.User.FirstName ?? string.Empty,
                LastName = instructor.User.LastName ?? string.Empty,
                Email = instructor.User.Email ?? string.Empty,
                PhoneNumber = instructor.User.PhoneNumber,
                CourseName = instructor.Course?.Name,
                CarModel = instructor.CarModel,
                CurrentProfileImagePath = instructor.User.ProfileImagePath,
                CurrentCarImagePath = instructor.CarImagePath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditInstructorProfileViewModel model)
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            if (!ModelState.IsValid)
            {
                model.CourseName = instructor.Course?.Name;
                model.CurrentProfileImagePath = instructor.User.ProfileImagePath;
                model.CurrentCarImagePath = instructor.CarImagePath;
                return View(model);
            }

            if (model.ProfileImage != null)
            {
                instructor.User.ProfileImagePath = await _fileStorage.SaveImageAsync(model.ProfileImage, "uploads/profile", instructor.User.ProfileImagePath);
            }

            if (model.CarImage != null)
            {
                instructor.CarImagePath = await _fileStorage.SaveImageAsync(model.CarImage, "uploads/cars", instructor.CarImagePath);
            }

            instructor.User.FirstName = model.FirstName.Trim();
            instructor.User.LastName = model.LastName.Trim();
            instructor.User.Email = model.Email.Trim();
            instructor.User.UserName = model.Email.Trim();
            instructor.User.PhoneNumber = model.PhoneNumber?.Trim();
            instructor.CarModel = model.CarModel?.Trim();

            await _context.SaveChangesAsync();
            TempData["Success"] = "Профилът беше обновен успешно.";
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            return View(new InstructorCreateSlotViewModel
            {
                DateTime = DateTime.Today.AddDays(1).AddHours(9),
                DurationMinutes = 50,
                CourseName = instructor.Course?.Name
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InstructorCreateSlotViewModel model)
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);
            model.CourseName = instructor.Course?.Name;

            if (model.DateTime < DateTime.Now)
            {
                ModelState.AddModelError(nameof(model.DateTime), "Не можеш да създаваш свободен слот в миналото.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var start = model.DateTime;
            var end = model.DateTime.AddMinutes(model.DurationMinutes);

            var overlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.InstructorId == instructor.Id &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < end &&
                start < l.DateTime.AddMinutes(l.DurationMinutes));

            if (overlaps)
            {
                ModelState.AddModelError(string.Empty, "Този слот се припокрива с друг час.");
                return View(model);
            }

            _context.PracticeLessons.Add(new PracticeLesson
            {
                InstructorId = instructor.Id,
                StudentId = null,
                CourseId = instructor.CourseId,
                DateTime = model.DateTime,
                DurationMinutes = model.DurationMinutes,
                Completed = false,
                Status = LessonStatus.Available,
                Note = string.IsNullOrWhiteSpace(model.Note) ? "Свободен слот" : model.Note!.Trim()
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Свободният слот беше създаден.";
            return RedirectToAction(nameof(Schedule), new { status = "Available" });
        }

        public async Task<IActionResult> PendingLessons()
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var lessons = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructor.Id && l.Status == LessonStatus.Pending)
                .Include(l => l.Student).ThenInclude(s => s.User)
                .Include(l => l.Course)
                .OrderBy(l => l.DateTime)
                .ToListAsync();

            return View(new InstructorPendingLessonsViewModel
            {
                Lessons = lessons.Select(MapLessonRow).ToList()
            });
        }

        public async Task<IActionResult> Schedule(string? status)
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var query = _context.PracticeLessons
                .Where(l => l.InstructorId == instructor.Id)
                .Include(l => l.Student).ThenInclude(s => s.User)
                .Include(l => l.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LessonStatus>(status, out var parsedStatus))
            {
                query = query.Where(l => l.Status == parsedStatus);
            }

            var lessons = await query
                .OrderByDescending(l => l.DateTime)
                .ToListAsync();

            return View(new InstructorScheduleViewModel
            {
                SelectedStatus = status,
                Lessons = lessons.Select(MapLessonRow).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Reschedule(int id)
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var lesson = await _context.PracticeLessons
                .Include(l => l.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructor.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!CanReschedule(lesson))
            {
                TempData["Error"] = "Този час не може да бъде пренасрочен.";
                return RedirectToAction(nameof(Schedule));
            }

            return View(new InstructorRescheduleLessonViewModel
            {
                LessonId = lesson.Id,
                StudentName = lesson.Student != null ? GetStudentName(lesson.Student) : "Свободен слот",
                CurrentDateTime = lesson.DateTime,
                NewDateTime = lesson.DateTime,
                DurationMinutes = lesson.DurationMinutes
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(InstructorRescheduleLessonViewModel model)
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var lesson = await _context.PracticeLessons
                .Include(l => l.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(l => l.Id == model.LessonId && l.InstructorId == instructor.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!CanReschedule(lesson))
            {
                TempData["Error"] = "Този час не може да бъде пренасрочен.";
                return RedirectToAction(nameof(Schedule));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var start = model.NewDateTime;
            var end = model.NewDateTime.AddMinutes(model.DurationMinutes);

            var overlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.InstructorId == instructor.Id &&
                l.Id != lesson.Id &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < end &&
                start < l.DateTime.AddMinutes(l.DurationMinutes));

            if (overlaps)
            {
                ModelState.AddModelError(string.Empty, "Новият час се припокрива с друг урок.");
                return View(model);
            }

            lesson.DateTime = model.NewDateTime;
            lesson.DurationMinutes = model.DurationMinutes;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът беше пренасрочен.";
            return RedirectToAction(nameof(Schedule));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var instructor = await GetCurrentInstructorAsync();
            var lesson = await _context.PracticeLessons.FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructor.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!CanApprove(lesson))
            {
                TempData["Error"] = "Само чакащ час може да бъде приет.";
                return RedirectToAction(nameof(PendingLessons));
            }

            lesson.Status = LessonStatus.Approved;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Заявката за час беше приета.";
            return RedirectToAction(nameof(PendingLessons));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var instructor = await GetCurrentInstructorAsync();
            var lesson = await _context.PracticeLessons.FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructor.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!CanReject(lesson))
            {
                TempData["Error"] = "Само чакащ час може да бъде отказан.";
                return RedirectToAction(nameof(PendingLessons));
            }

            lesson.Status = LessonStatus.Rejected;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Заявката за час беше отказана.";
            return RedirectToAction(nameof(PendingLessons));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var instructor = await GetCurrentInstructorAsync();
            var lesson = await _context.PracticeLessons.FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructor.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!CanCancel(lesson))
            {
                TempData["Error"] = "Този час не може да бъде отменен.";
                return RedirectToAction(nameof(Schedule));
            }

            lesson.Status = LessonStatus.Cancelled;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът беше отменен.";
            return RedirectToAction(nameof(Schedule));
        }

        [HttpGet]
        public async Task<IActionResult> Complete(int id)
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var lesson = await _context.PracticeLessons
                .Include(l => l.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructor.Id);

            if (lesson == null)
            {
                return NotFound();
            }

            if (!CanComplete(lesson))
            {
                TempData["Error"] = "Само одобрен и непроведен час може да бъде отбелязан като проведен.";
                return RedirectToAction(nameof(Schedule));
            }

            return View(new InstructorCompleteLessonViewModel
            {
                LessonId = lesson.Id,
                StudentName = lesson.Student != null ? GetStudentName(lesson.Student) : "—",
                LessonDateTime = lesson.DateTime,
                Note = lesson.Note
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(InstructorCompleteLessonViewModel model)
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var lesson = await _context.PracticeLessons.FirstOrDefaultAsync(l => l.Id == model.LessonId && l.InstructorId == instructor.Id);
            if (lesson == null)
            {
                return NotFound();
            }

            if (!CanComplete(lesson))
            {
                TempData["Error"] = "Само одобрен и непроведен час може да бъде отбелязан като проведен.";
                return RedirectToAction(nameof(Schedule));
            }

            lesson.Completed = true;
            lesson.Status = LessonStatus.Approved;
            lesson.Note = model.Note?.Trim();
            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът беше отбелязан като проведен.";
            return RedirectToAction(nameof(Schedule));
        }

        public async Task<IActionResult> MyStudents()
        {
            var instructor = await GetCurrentInstructorAsync();
            SetInstructorContext(instructor);

            var studentIds = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructor.Id && l.StudentId != null)
                .Select(l => l.StudentId!.Value)
                .Distinct()
                .ToListAsync();

            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .Include(s => s.User)
                .Include(s => s.Course)
                .OrderBy(s => s.User.FirstName)
                .ThenBy(s => s.User.LastName)
                .ToListAsync();

            var result = new List<InstructorStudentRowViewModel>();
            foreach (var student in students)
            {
                var completedLessons = await _context.PracticeLessons.CountAsync(l => l.StudentId == student.Id && l.Completed);
                var lastLessonDate = await _context.PracticeLessons
                    .Where(l => l.InstructorId == instructor.Id && l.StudentId == student.Id)
                    .OrderByDescending(l => l.DateTime)
                    .Select(l => (DateTime?)l.DateTime)
                    .FirstOrDefaultAsync();

                result.Add(new InstructorStudentRowViewModel
                {
                    StudentId = student.Id,
                    FullName = GetStudentName(student),
                    CourseName = student.Course?.Name,
                    CompletedLessons = completedLessons,
                    RemainingLessons = Math.Max(0, (student.Course?.RequiredPracticeLessons ?? 31) - completedLessons),
                    LastLessonDate = lastLessonDate
                });
            }

            return View(new InstructorStudentsViewModel { Students = result });
        }

        private async Task<Instructor> GetCurrentInstructorAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .FirstAsync(i => i.UserId == userId);
        }

        private void SetInstructorContext(Instructor instructor)
        {
            ViewBag.InstructorName = GetInstructorName(instructor);
        }

        private static bool CanApprove(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.StudentId.HasValue
               && lesson.Status == LessonStatus.Pending
               && lesson.DateTime > DateTime.Now;

        private static bool CanReject(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.StudentId.HasValue
               && lesson.Status == LessonStatus.Pending
               && lesson.DateTime > DateTime.Now;

        private static bool CanCancel(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.DateTime > DateTime.Now
               && (lesson.Status == LessonStatus.Available
                   || lesson.Status == LessonStatus.Pending
                   || lesson.Status == LessonStatus.Approved);

        private static bool CanReschedule(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.DateTime > DateTime.Now
               && (lesson.Status == LessonStatus.Available
                   || lesson.Status == LessonStatus.Pending
                   || lesson.Status == LessonStatus.Approved);

        private static bool CanComplete(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.StudentId.HasValue
               && lesson.Status == LessonStatus.Approved
               && lesson.DateTime <= DateTime.Now;

        private static InstructorLessonRowViewModel MapLessonRow(PracticeLesson lesson)
        {
            var statusText = lesson.Status switch
            {
                LessonStatus.Available => "Свободен слот",
                LessonStatus.Pending => "Изчаква одобрение",
                LessonStatus.Approved => "Одобрен",
                LessonStatus.Rejected => "Отказан",
                LessonStatus.Cancelled => "Отменен",
                _ => lesson.Status.ToString()
            };

            var cssClass = lesson.Status switch
            {
                LessonStatus.Available => "status-available",
                LessonStatus.Pending => "status-pending",
                LessonStatus.Approved => lesson.Completed ? "status-completed" : "status-approved",
                LessonStatus.Rejected => "status-rejected",
                LessonStatus.Cancelled => "status-cancelled",
                _ => string.Empty
            };

            return new InstructorLessonRowViewModel
            {
                Id = lesson.Id,
                DateTime = lesson.DateTime,
                DurationMinutes = lesson.DurationMinutes,
                StudentName = lesson.Student != null ? GetStudentName(lesson.Student) : null,
                CourseName = lesson.Course?.Name,
                Status = lesson.Completed ? "Проведен" : statusText,
                StatusCssClass = lesson.Completed ? "status-completed" : cssClass,
                Completed = lesson.Completed,
                Note = lesson.Note
            };
        }

        private static string GetInstructorName(Instructor instructor)
            => $"{instructor.User.FirstName} {instructor.User.LastName}".Trim();

        private static string GetStudentName(Student student)
            => $"{student.User.FirstName} {student.User.LastName}".Trim();
    }
}
