using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.Services.Interfaces;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoSchoolProject.Services
{
    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Student> GetStudentAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            return await _context.Students
                .Include(s => s.User)
                .Include(s => s.Course)
                .Include(s => s.ScheduledLessons)
                .FirstAsync(s => s.UserId == userId);
        }

        public async Task<StudentProfileViewModel> GetProfileAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);

            int completedLessons = student.ScheduledLessons?.Count(l => l.Completed) ?? 0;
            var required = student.Course?.RequiredPracticeLessons ?? 31;
            var adminMessage = await _context.EnrollmentRequests
                .Where(r => r.CreatedStudentUserId == student.UserId
                            && r.Status == RequestStatus.Approved
                            && !string.IsNullOrWhiteSpace(r.AdminNote))
                .OrderByDescending(r => r.ProcessedAt ?? r.CreatedAt)
                .Select(r => r.AdminNote)
                .FirstOrDefaultAsync();

            return new StudentProfileViewModel
            {
                FullName = $"{student.User.FirstName} {student.User.LastName}".Trim(),
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                CourseName = student.Course?.Name,
                CompletedLessons = completedLessons,
                RemainingLessons = Math.Max(0, required - completedLessons),
                ProfileImagePath = student.User.ProfileImagePath,
                AdminMessage = adminMessage
            };
        }

        public async Task<EditStudentProfileViewModel> GetEditProfileAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);

            return new EditStudentProfileViewModel
            {
                FirstName = student.User.FirstName ?? string.Empty,
                LastName = student.User.LastName ?? string.Empty,
                Email = student.User.Email ?? string.Empty,
                PhoneNumber = student.User.PhoneNumber,
                CourseName = student.Course?.Name,
                CurrentProfileImagePath = student.User.ProfileImagePath
            };
        }

        public async Task UpdateProfileAsync(ClaimsPrincipal user, EditStudentProfileViewModel model)
        {
            var student = await GetStudentAsync(user);

            student.User.FirstName = model.FirstName.Trim();
            student.User.LastName = model.LastName.Trim();
            student.User.Email = model.Email.Trim();
            student.User.UserName = model.Email.Trim();
            student.User.PhoneNumber = model.PhoneNumber?.Trim();

            if (!string.IsNullOrWhiteSpace(model.ProfileImagePath))
            {
                student.User.ProfileImagePath = model.ProfileImagePath;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<InstructorListViewModel>> GetInstructorsAsync()
        {
            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .OrderBy(i => i.User.FirstName)
                .ThenBy(i => i.User.LastName)
                .Select(i => new InstructorListViewModel
                {
                    Id = i.Id,
                    FullName = (i.User.FirstName + " " + i.User.LastName).Trim(),
                    PhoneNumber = i.User.PhoneNumber,
                    Email = i.User.Email,
                    CourseName = i.Course != null ? i.Course.Name : "—",
                    Category = i.Course != null ? i.Course.Name : "—"
                })
                .ToListAsync();
        }

        public async Task<List<InstructorListViewModel>> GetInstructorsAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);

            if (!student.CourseId.HasValue)
            {
                return new List<InstructorListViewModel>();
            }

            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .Where(i => i.CourseId == student.CourseId)
                .OrderBy(i => i.User.FirstName)
                .ThenBy(i => i.User.LastName)
                .Select(i => new InstructorListViewModel
                {
                    Id = i.Id,
                    FullName = (i.User.FirstName + " " + i.User.LastName).Trim(),
                    PhoneNumber = i.User.PhoneNumber,
                    Email = i.User.Email,
                    CourseName = i.Course != null ? i.Course.Name : "—",
                    Category = i.Course != null ? i.Course.Name : "—"
                })
                .ToListAsync();
        }

        public async Task<InstructorDetailsViewModel> GetInstructorDetailsAsync(int instructorId)
        {
            var instructor = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .FirstAsync(i => i.Id == instructorId);

            return new InstructorDetailsViewModel
            {
                InstructorId = instructor.Id,
                FullName = (instructor.User.FirstName + " " + instructor.User.LastName).Trim(),
                Email = instructor.User.Email,
                PhoneNumber = instructor.User.PhoneNumber,
                SchoolName = "Автошкола Lucky-Cars EOOD",
                ProfileImagePath = instructor.User.ProfileImagePath,
                CarModel = instructor.CarModel,
                CarImagePath = instructor.CarImagePath
            };
        }

        public async Task<InstructorDetailsViewModel> GetInstructorDetailsAsync(ClaimsPrincipal user, int instructorId)
        {
            var student = await GetStudentAsync(user);

            if (!student.CourseId.HasValue)
            {
                throw new InvalidOperationException("Все още нямаш зададена категория от администратор.");
            }

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .FirstOrDefaultAsync(i => i.Id == instructorId && i.CourseId == student.CourseId);

            if (instructor == null)
            {
                throw new InvalidOperationException("Този инструктор не е за твоята категория.");
            }

            return new InstructorDetailsViewModel
            {
                InstructorId = instructor.Id,
                FullName = (instructor.User.FirstName + " " + instructor.User.LastName).Trim(),
                Email = instructor.User.Email,
                PhoneNumber = instructor.User.PhoneNumber,
                SchoolName = "Автошкола Lucky-Cars EOOD",
                ProfileImagePath = instructor.User.ProfileImagePath,
                CarModel = instructor.CarModel,
                CarImagePath = instructor.CarImagePath
            };
        }

        public async Task<BookLessonViewModel> GetBookLessonAsync(ClaimsPrincipal user, int instructorId)
        {
            var student = await GetStudentAsync(user);

            if (!student.CourseId.HasValue)
            {
                throw new InvalidOperationException("Все още нямаш зададена категория от администратор.");
            }

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .FirstOrDefaultAsync(i => i.Id == instructorId && i.CourseId == student.CourseId);

            if (instructor == null)
            {
                throw new InvalidOperationException("Не можеш да записваш час при инструктор от друга категория.");
            }

            var now = DateTime.Now;

            var slots = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructorId
                            && l.StudentId == null
                            && l.Status == LessonStatus.Available
                            && l.DateTime >= now
                            && l.CourseId == student.CourseId)
                .OrderBy(l => l.DateTime)
                .Take(50)
                .Select(l => new SelectListItem
                {
                    Value = l.Id.ToString(),
                    Text = l.DateTime.ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();

            return new BookLessonViewModel
            {
                InstructorId = instructorId,
                InstructorName = (instructor.User.FirstName + " " + instructor.User.LastName).Trim(),
                StudentId = student.Id,
                CourseId = student.CourseId,
                AvailableSlots = slots
            };
        }

        public async Task BookLessonAsync(ClaimsPrincipal user, BookLessonViewModel model)
        {
            var student = await GetStudentAsync(user);

            if (!student.CourseId.HasValue)
            {
                throw new InvalidOperationException("Все още нямаш зададена категория от администратор.");
            }

            var slot = await _context.PracticeLessons
                .Include(l => l.Instructor)
                .FirstOrDefaultAsync(l =>
                    l.Id == model.SlotId &&
                    l.InstructorId == model.InstructorId &&
                    l.StudentId == null &&
                    l.Status == LessonStatus.Available);

            if (slot == null)
            {
                throw new InvalidOperationException("Избраният слот вече не е наличен.");
            }

            if (slot.DateTime < DateTime.Now)
            {
                throw new InvalidOperationException("Не можеш да записваш час в миналото.");
            }

            if (slot.Instructor?.CourseId != student.CourseId || slot.CourseId != student.CourseId)
            {
                throw new InvalidOperationException("Не можеш да записваш час при инструктор от друга категория.");
            }

            var start = slot.DateTime;
            var end = slot.DateTime.AddMinutes(slot.DurationMinutes);

            bool studentOverlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.StudentId == student.Id &&
                l.Id != slot.Id &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < end &&
                start < l.DateTime.AddMinutes(l.DurationMinutes));

            if (studentOverlaps)
            {
                throw new InvalidOperationException("Имаш друг час, който се припокрива с този.");
            }

            slot.StudentId = student.Id;
            slot.CourseId = student.CourseId;
            slot.Status = LessonStatus.Pending;
            slot.Completed = false;

            if (string.IsNullOrWhiteSpace(slot.Note) || string.Equals(slot.Note, "Свободен слот", StringComparison.OrdinalIgnoreCase))
            {
                slot.Note = "Запазен час";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<PracticeLesson>> GetInstructorLessonsAsync(int instructorId, DateTime start, DateTime end)
        {
            return await _context.PracticeLessons
                .Where(l => l.InstructorId == instructorId
                    && l.DateTime >= start && l.DateTime <= end
                    && l.Status != LessonStatus.Cancelled
                    && l.Status != LessonStatus.Rejected)
                .ToListAsync();
        }

        public async Task<List<MyLessonListItemViewModel>> GetMyLessonsAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);

            var lessons = await _context.PracticeLessons
                .Where(l => l.StudentId == student.Id)
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .Include(l => l.Course)
                .OrderByDescending(l => l.DateTime)
                .ToListAsync();

            return lessons.Select(l => new MyLessonListItemViewModel
            {
                Id = l.Id,
                DateTime = l.DateTime,
                InstructorName = l.Instructor != null ? ((l.Instructor.User.FirstName + " " + l.Instructor.User.LastName).Trim()) : null,
                CourseName = l.Course != null ? l.Course.Name : null,
                Status = GetLessonStatusText(l.Status, l.Completed),
                Completed = l.Completed,
                Note = l.Note
            }).ToList();
        }

        public async Task CancelLessonAsync(ClaimsPrincipal user, int lessonId)
        {
            var student = await GetStudentAsync(user);

            var lesson = await _context.PracticeLessons
                .Include(l => l.Instructor)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.StudentId == student.Id);

            if (lesson == null)
            {
                throw new InvalidOperationException("Часът не е намерен.");
            }

            if (lesson.DateTime <= DateTime.Now)
            {
                throw new InvalidOperationException("Не можеш да отменяш минали часове.");
            }

            if (lesson.Status != LessonStatus.Pending && lesson.Status != LessonStatus.Approved)
            {
                throw new InvalidOperationException("Този час не може да бъде отменен.");
            }

            lesson.StudentId = null;
            lesson.Completed = false;
            lesson.Status = LessonStatus.Available;
            lesson.CourseId = lesson.Instructor?.CourseId ?? lesson.CourseId;

            if (string.IsNullOrWhiteSpace(lesson.Note) || string.Equals(lesson.Note, "Запазен час", StringComparison.OrdinalIgnoreCase))
            {
                lesson.Note = "Свободен слот";
            }

            await _context.SaveChangesAsync();
        }

        public async Task<StudentTestResultsViewModel> GetTestResultsAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);

            var results = await _context.TestResultListovki
                .Where(r => r.StudentId == student.Id)
                .OrderByDescending(r => r.Date)
                .Select(r => new TestResultRowViewModel
                {
                    Date = r.Date,
                    Score = r.Score
                })
                .ToListAsync();

            return new StudentTestResultsViewModel
            {
                FullName = $"{student.User.FirstName} {student.User.LastName}",
                CourseName = student.Course?.Name,
                Results = results
            };
        }

        public async Task<StudentScheduleViewModel> GetScheduleAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);
            var now = DateTime.Now;

            var practiceLessons = await _context.PracticeLessons
                .Where(l => l.StudentId == student.Id && l.DateTime >= now)
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .OrderBy(l => l.DateTime)
                .ToListAsync();

            var practice = practiceLessons.Select(l => new SchedulePracticeRowViewModel
            {
                Id = l.Id,
                DateTime = l.DateTime,
                InstructorName = l.Instructor != null ? ((l.Instructor.User.FirstName + " " + l.Instructor.User.LastName).Trim()) : null,
                Status = GetLessonStatusText(l.Status, l.Completed),
                Completed = l.Completed
            }).ToList();

            var theory = new List<ScheduleTheoryRowViewModel>();
            if (student.CourseId.HasValue)
            {
                theory = await _context.TheorySessions
                    .Where(t => t.CourseId == student.CourseId.Value && t.DateTime >= now)
                    .OrderBy(t => t.DateTime)
                    .Select(t => new ScheduleTheoryRowViewModel
                    {
                        DateTime = t.DateTime,
                        DurationMinutes = t.DurationMinutes,
                        Topic = t.Topic,
                        Location = t.Location
                    })
                    .ToListAsync();
            }

            return new StudentScheduleViewModel
            {
                CourseName = student.Course?.Name,
                Practice = practice,
                Theory = theory
            };
        }

        public async Task<List<PracticeLesson>> GetInstructorLessonsAsync(int instructorId)
        {
            return await _context.PracticeLessons
                .Where(l => l.InstructorId == instructorId
                    && l.Status != LessonStatus.Cancelled
                    && l.Status != LessonStatus.Rejected)
                .ToListAsync();
        }

        private static string GetLessonStatusText(LessonStatus status, bool completed)
        {
            if (completed)
            {
                return "Проведен";
            }

            return status switch
            {
                LessonStatus.Pending => "Изчаква одобрение",
                LessonStatus.Approved => "Одобрен",
                LessonStatus.Rejected => "Отказан",
                LessonStatus.Cancelled => "Отменен",
                LessonStatus.Available => "Свободен слот",
                _ => status.ToString()
            };
        }
    }
}
