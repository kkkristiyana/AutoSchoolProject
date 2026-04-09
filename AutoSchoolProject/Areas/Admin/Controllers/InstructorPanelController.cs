using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoSchoolProject.Services;

namespace AutoSchoolProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InstructorPanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public InstructorPanelController(ApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Instructors
            .Include(i => i.User)
            .Include(i => i.Course)
            .Where(i => i.IsWorking == "Yes")
            .AsQueryable();


            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(i =>
                    ((i.User.FirstName ?? string.Empty) + " " + (i.User.LastName ?? string.Empty)).ToLower().Contains(term) ||
                    (i.User.Email ?? string.Empty).ToLower().Contains(term) ||
                    (i.User.PhoneNumber ?? string.Empty).ToLower().Contains(term));
            }

            var instructors = await query
                .OrderBy(i => i.User.FirstName)
                .ThenBy(i => i.User.LastName)
                .Select(i => new InstructorPanelInstructorRowViewModel
                {
                    InstructorId = i.Id,
                    FullName = (i.User.FirstName + " " + i.User.LastName).Trim(),
                    Email = i.User.Email,
                    PhoneNumber = i.User.PhoneNumber,
                    CourseName = i.Course != null ? i.Course.Name : null,
                    ProfileImagePath = i.User.ProfileImagePath
                })
                .ToListAsync();

            return View(new InstructorPanelIndexViewModel
            {
                Query = q,
                Instructors = instructors
            });
        }

        public async Task<IActionResult> Profile(int instructorId)
        {
            var instructor = await GetInstructorWithRelationsAsync(instructorId);
            if (instructor == null) return NotFound();

            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            var now = DateTime.Now;
            var lessons = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructorId)
                .ToListAsync();

            var model = new InstructorPanelProfileViewModel
            {
                InstructorId = instructor.Id,
                FullName = GetInstructorName(instructor),
                Email = instructor.User.Email,
                PhoneNumber = instructor.User.PhoneNumber,
                CourseName = instructor.Course?.Name,
                CarModel = instructor.CarModel,
                ProfileImagePath = instructor.User.ProfileImagePath,
                CarImagePath = instructor.CarImagePath,
                PendingLessonsCount = lessons.Count(l => l.Status == LessonStatus.Pending),
                UpcomingApprovedLessonsCount = lessons.Count(l => l.Status == LessonStatus.Approved && l.DateTime >= now),
                CompletedLessonsCount = lessons.Count(l => l.Completed)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(int instructorId)
        {
            var instructor = await GetInstructorWithRelationsAsync(instructorId);
            if (instructor == null) return NotFound();

            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            var model = new AdminEditInstructorViewModel
            {
                InstructorId = instructor.Id,
                UserId = instructor.UserId,
                FirstName = instructor.User.FirstName ?? string.Empty,
                LastName = instructor.User.LastName ?? string.Empty,
                Email = instructor.User.Email ?? string.Empty,
                PhoneNumber = instructor.User.PhoneNumber,
                CourseId = instructor.CourseId,
                Courses = await GetCourseSelectListAsync(),
                CarModel = instructor.CarModel,
                CurrentProfileImagePath = instructor.User.ProfileImagePath,
                CurrentCarImagePath = instructor.CarImagePath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int instructorId, AdminEditInstructorViewModel model)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null) return NotFound();

            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            if (!ModelState.IsValid)
            {
                model.Courses = await GetCourseSelectListAsync();
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
            TempData["Success"] = "Профилът на инструктора е обновен.";
            return RedirectToAction(nameof(Profile), new { instructorId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetProfileImage(int instructorId)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null)
                return NotFound();

            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            if (!string.IsNullOrWhiteSpace(instructor.User.ProfileImagePath))
            {
                await _fileStorage.DeleteImageAsync(instructor.User.ProfileImagePath);
                instructor.User.ProfileImagePath = null;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Профилната снимка беше нулирана (Admin Instructor Panel).";
            }
            else
            {
                TempData["Error"] = "Няма зададена профилна снимка за нулиране.";
            }

            return RedirectToAction(nameof(EditProfile), new { instructorId });
        }

        public async Task<IActionResult> Lessons(int instructorId, string? status)
        {
            var instructor = await GetInstructorWithRelationsAsync(instructorId);
            if (instructor == null) return NotFound();
            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            var query = _context.PracticeLessons
                .Where(l => l.InstructorId == instructorId)
                .Include(l => l.Student).ThenInclude(s => s.User)
                .Include(l => l.Course)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LessonStatus>(status, out var parsedStatus))
            {
                query = query.Where(l => l.Status == parsedStatus);
            }

            var lessons = await query
                .OrderByDescending(l => l.DateTime)
                .Select(l => new InstructorPanelLessonRowViewModel
                {
                    Id = l.Id,
                    DateTime = l.DateTime,
                    DurationMinutes = l.DurationMinutes,
                    StudentName = l.Student != null ? ((l.Student.User.FirstName + " " + l.Student.User.LastName).Trim()) : null,
                    CourseName = l.Course != null ? l.Course.Name : null,
                    Status = l.Status.ToString(),
                    Completed = l.Completed,
                    Note = l.Note
                })
                .ToListAsync();

            return View(new InstructorPanelLessonsViewModel
            {
                Status = status,
                Lessons = lessons
            });
        }

        [HttpGet]
        public async Task<IActionResult> CreateSlot(int instructorId)
        {
            var instructor = await GetInstructorWithRelationsAsync(instructorId);
            if (instructor == null) return NotFound();
            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            return View(new InstructorPanelCreateSlotViewModel
            {
                DateTime = DateTime.Now.AddDays(1).Date.AddHours(9),
                DurationMinutes = 50,
                CourseId = instructor.CourseId,
                Courses = await GetCourseSelectListAsync()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSlot(int instructorId, InstructorPanelCreateSlotViewModel model)
        {
            var instructor = await GetInstructorWithRelationsAsync(instructorId);
            if (instructor == null) return NotFound();
            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            if (model.DateTime < DateTime.Now)
                ModelState.AddModelError(nameof(model.DateTime), "Не можеш да създаваш слот в миналото.");

            if (!ModelState.IsValid)
            {
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            var start = model.DateTime;
            var end = model.DateTime.AddMinutes(model.DurationMinutes);

            var overlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.InstructorId == instructorId &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < end &&
                start < l.DateTime.AddMinutes(l.DurationMinutes));

            if (overlaps)
            {
                ModelState.AddModelError("", "Има припокриване с друг час за този инструктор.");
                model.Courses = await GetCourseSelectListAsync();
                return View(model);
            }

            _context.PracticeLessons.Add(new PracticeLesson
            {
                InstructorId = instructorId,
                StudentId = null,
                CourseId = model.CourseId,
                DateTime = model.DateTime,
                DurationMinutes = model.DurationMinutes,
                Status = LessonStatus.Available,
                Completed = false,
                Note = string.IsNullOrWhiteSpace(model.Note) ? "Свободен слот" : model.Note
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Свободният слот е създаден.";
            return RedirectToAction(nameof(Lessons), new { instructorId, status = LessonStatus.Available.ToString() });
        }

        [HttpGet]
        public async Task<IActionResult> RescheduleLesson(int instructorId, int id)
        {
            var lesson = await _context.PracticeLessons
                .Include(l => l.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructorId);

            if (lesson == null) return NotFound();

            var instructor = await GetInstructorWithRelationsAsync(instructorId);
            if (instructor == null) return NotFound();
            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            if (!CanRescheduleLesson(lesson))
            {
                TempData["Error"] = "Този час не може да бъде пренасрочен.";
                return RedirectToAction(nameof(Lessons), new { instructorId });
            }

            return View(new InstructorPanelRescheduleLessonViewModel
            {
                LessonId = lesson.Id,
                StudentName = lesson.Student != null ? ((lesson.Student.User.FirstName + " " + lesson.Student.User.LastName).Trim()) : "Свободен слот",
                CurrentDateTime = lesson.DateTime,
                NewDateTime = lesson.DateTime,
                DurationMinutes = lesson.DurationMinutes
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RescheduleLesson(int instructorId, InstructorPanelRescheduleLessonViewModel model)
        {
            var lesson = await _context.PracticeLessons
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .Include(l => l.Student).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(l => l.Id == model.LessonId && l.InstructorId == instructorId);

            if (lesson == null) return NotFound();
            SetInstructorContext(instructorId, lesson.Instructor != null ? GetInstructorName(lesson.Instructor) : "Инструктор");

            if (!CanRescheduleLesson(lesson))
            {
                TempData["Error"] = "Този час не може да бъде пренасрочен.";
                return RedirectToAction(nameof(Lessons), new { instructorId });
            }

            if (!ModelState.IsValid)
                return View(model);

            var oldDateTime = lesson.DateTime;
            var start = model.NewDateTime;
            var end = model.NewDateTime.AddMinutes(model.DurationMinutes);

            var overlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.InstructorId == instructorId &&
                l.Id != lesson.Id &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < end &&
                start < l.DateTime.AddMinutes(l.DurationMinutes));

            if (overlaps)
            {
                ModelState.AddModelError("", "Новият час се припокрива с друг урок.");
                return View(model);
            }

            lesson.DateTime = model.NewDateTime;
            lesson.DurationMinutes = model.DurationMinutes;

            if (lesson.StudentId.HasValue)
            {
                lesson.Note = LessonMessageFactory.ForStudent(
                    $"Инструкторът пренасрочи часа ти от {oldDateTime:dd.MM.yyyy HH:mm} за {lesson.DateTime:dd.MM.yyyy HH:mm}.");
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Часът е пренасрочен.";
            return RedirectToAction(nameof(Lessons), new { instructorId });
        }


        [HttpGet]
        public async Task<IActionResult> CompleteLesson(int instructorId, int id)
        {
            var lesson = await _context.PracticeLessons
                .Include(l => l.Student).ThenInclude(s => s.User)
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructorId);

            if (lesson == null) return NotFound();
            SetInstructorContext(instructorId, lesson.Instructor != null ? GetInstructorName(lesson.Instructor) : "Инструктор");

            if (!CanCompleteLesson(lesson))
            {
                TempData["Error"] = "Само одобрен и непроведен час може да бъде отбелязан като проведен.";
                return RedirectToAction(nameof(Lessons), new { instructorId });
            }

            return View(new InstructorPanelCompleteLessonViewModel
            {
                LessonId = lesson.Id,
                StudentName = lesson.Student != null ? ((lesson.Student.User.FirstName + " " + lesson.Student.User.LastName).Trim()) : "—",
                LessonDateTime = lesson.DateTime,
                Note = lesson.Note
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteLesson(int instructorId, InstructorPanelCompleteLessonViewModel model)
        {
            var lesson = await _context.PracticeLessons
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .FirstOrDefaultAsync(l => l.Id == model.LessonId && l.InstructorId == instructorId);

            if (lesson == null) return NotFound();
            SetInstructorContext(instructorId, lesson.Instructor != null ? GetInstructorName(lesson.Instructor) : "Инструктор");

            if (!CanCompleteLesson(lesson))
            {
                TempData["Error"] = "Само одобрен и непроведен час може да бъде отбелязан като проведен.";
                return RedirectToAction(nameof(Lessons), new { instructorId });
            }

            lesson.Completed = true;
            lesson.Status = LessonStatus.Approved;
            lesson.Note = model.Note;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът е отбелязан като проведен.";
            return RedirectToAction(nameof(Lessons), new { instructorId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLesson(int instructorId, int id)
        {
            var lesson = await _context.PracticeLessons.FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructorId);
            if (lesson == null) return NotFound();

            if (!CanApproveLesson(lesson))
            {
                TempData["Error"] = "Само чакащ час може да бъде приет.";
                return RedirectToAction(nameof(Lessons), new { instructorId });
            }

            lesson.Status = LessonStatus.Approved;
            lesson.Note = LessonMessageFactory.ForStudent(
                $"Инструкторът прие часа ти за {lesson.DateTime:dd.MM.yyyy HH:mm}.");

            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът е одобрен.";
            return RedirectToAction(nameof(Lessons), new { instructorId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLesson(int instructorId, int id)
        {
            var lesson = await _context.PracticeLessons.FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructorId);
            if (lesson == null) return NotFound();

            if (!CanRejectLesson(lesson))
            {
                TempData["Error"] = "Само чакащ час може да бъде отказан.";
                return RedirectToAction(nameof(Lessons), new { instructorId });
            }

            lesson.Status = LessonStatus.Rejected;
            lesson.Note = LessonMessageFactory.ForStudent(
                $"Инструкторът отказа заявката ти за часа на {lesson.DateTime:dd.MM.yyyy HH:mm}.");

            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът е отказан.";
            return RedirectToAction(nameof(Lessons), new { instructorId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelLesson(int instructorId, int id)
        {
            var lesson = await _context.PracticeLessons.FirstOrDefaultAsync(l => l.Id == id && l.InstructorId == instructorId);
            if (lesson == null) return NotFound();

            if (!CanCancelLesson(lesson))
            {
                TempData["Error"] = "Този час не може да бъде отменен.";
                return RedirectToAction(nameof(Lessons), new { instructorId });
            }

            lesson.Status = LessonStatus.Cancelled;

            if (lesson.StudentId.HasValue)
            {
                lesson.Note = LessonMessageFactory.ForStudent(
                    $"Инструкторът отмени часа ти за {lesson.DateTime:dd.MM.yyyy HH:mm}.");
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Часът е отменен.";
            return RedirectToAction(nameof(Lessons), new { instructorId });
        }



        public async Task<IActionResult> Students(int instructorId)
        {
            var instructor = await GetInstructorWithRelationsAsync(instructorId);
            if (instructor == null) return NotFound();
            SetInstructorContext(instructor.Id, GetInstructorName(instructor));

            var studentIds = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructorId && l.StudentId != null)
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

            var rows = new List<InstructorPanelStudentProgressRowViewModel>();
            foreach (var student in students)
            {
                var completedLessons = await _context.PracticeLessons.CountAsync(l => l.StudentId == student.Id && l.Completed);
                var lastLessonDate = await _context.PracticeLessons
                    .Where(l => l.InstructorId == instructorId && l.StudentId == student.Id)
                    .OrderByDescending(l => l.DateTime)
                    .Select(l => (DateTime?)l.DateTime)
                    .FirstOrDefaultAsync();

                rows.Add(new InstructorPanelStudentProgressRowViewModel
                {
                    StudentId = student.Id,
                    FullName = ((student.User.FirstName ?? string.Empty) + " " + (student.User.LastName ?? string.Empty)).Trim(),
                    CourseName = student.Course?.Name,
                    CompletedLessons = completedLessons,
                    RemainingLessons = Math.Max(0, (student.Course?.RequiredPracticeLessons ?? 31) - completedLessons),
                    LastLessonDate = lastLessonDate
                });
            }

            return View(new InstructorPanelStudentsViewModel { Students = rows });
        }

        private async Task<Instructor?> GetInstructorWithRelationsAsync(int instructorId)
        {
            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .FirstOrDefaultAsync(i => i.Id == instructorId);
        }

        private static string GetInstructorName(Instructor instructor)
            => ((instructor.User.FirstName ?? string.Empty) + " " + (instructor.User.LastName ?? string.Empty)).Trim();

        private void SetInstructorContext(int instructorId, string instructorName)
        {
            ViewBag.InstructorId = instructorId;
            ViewBag.InstructorName = instructorName;
        }

        private async Task<List<SelectListItem>> GetCourseSelectListAsync()
        {
            return await _context.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }
        //helpers
        private static bool CanApproveLesson(PracticeLesson lesson)
    => !lesson.Completed
       && lesson.StudentId.HasValue
       && lesson.Status == LessonStatus.Pending
       && lesson.DateTime > DateTime.Now;

        private static bool CanRejectLesson(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.StudentId.HasValue
               && lesson.Status == LessonStatus.Pending
               && lesson.DateTime > DateTime.Now;

        private static bool CanCancelLesson(PracticeLesson lesson)
    => !lesson.Completed
       && lesson.DateTime > DateTime.Now
       && (lesson.Status == LessonStatus.Available
           || lesson.Status == LessonStatus.Approved);


        private static bool CanRescheduleLesson(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.DateTime > DateTime.Now
               && (lesson.Status == LessonStatus.Available
                   || lesson.Status == LessonStatus.Pending
                   || lesson.Status == LessonStatus.Approved);

        private static bool CanCompleteLesson(PracticeLesson lesson)
            => !lesson.Completed
               && lesson.StudentId.HasValue
               && lesson.Status == LessonStatus.Approved
               && lesson.DateTime <= DateTime.Now;
    }
}