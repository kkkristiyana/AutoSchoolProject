using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AutoSchoolProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

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

            int completedLessons = student.ScheduledLessons.Count(l => l.Completed);
            var required = student.Course?.RequiredPracticeLessons ?? 31;

            return new StudentProfileViewModel
            {
                FullName = $"{student.User.FirstName} {student.User.LastName}",
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                CourseName = student.Course?.Name,
                CompletedLessons = completedLessons,
                RemainingLessons = Math.Max(0, required - completedLessons)
            };
        }

        public async Task UpdateProfileAsync(ClaimsPrincipal user, EditStudentProfileViewModel model)
        {
            var student = await GetStudentAsync(user);

            student.User.FirstName = model.FirstName;
            student.User.LastName = model.LastName;
            student.User.Email = model.Email;
            student.User.UserName = model.Email;
            student.User.PhoneNumber = model.PhoneNumber;

            student.CourseId = model.CourseId;

            await _context.SaveChangesAsync();
        }

        public async Task<BookLessonViewModel> GetBookLessonAsync(ClaimsPrincipal user, int instructorId)
        {
            var student = await GetStudentAsync(user);

            var instructor = await _context.Instructors
                .Include(i => i.User)
                .FirstAsync(i => i.Id == instructorId);

            var now = DateTime.Now;

            var slots = await _context.PracticeLessons
                .Where(l => l.InstructorId == instructorId
                            && l.StudentId == null
                            && l.Status == LessonStatus.Available
                            && l.DateTime >= now
                            && (!student.CourseId.HasValue || l.CourseId == student.CourseId))
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
                InstructorName = instructor.User.FirstName + " " + instructor.User.LastName,
                StudentId = student.Id,
                CourseId = student.CourseId,
                AvailableSlots = slots
            };
        }

        public async Task BookLessonAsync(ClaimsPrincipal user, BookLessonViewModel model)
        {
            var student = await GetStudentAsync(user);

            var slot = await _context.PracticeLessons
                .FirstOrDefaultAsync(l =>
                    l.Id == model.SlotId &&
                    l.InstructorId == model.InstructorId &&
                    l.StudentId == null &&
                    l.Status == LessonStatus.Available);

            if (slot == null)
                throw new InvalidOperationException("Избраният слот вече не е наличен.");

            if (slot.DateTime < DateTime.Now)
                throw new InvalidOperationException("Не можеш да записваш час в миналото.");

            var start = slot.DateTime;
            var end = slot.DateTime.AddMinutes(slot.DurationMinutes);

            bool overlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.InstructorId == model.InstructorId &&
                l.Id != slot.Id &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < end &&
                start < l.DateTime.AddMinutes(l.DurationMinutes));

            if (overlaps)
                throw new InvalidOperationException("Този час вече е зает.");

            slot.StudentId = student.Id;
            slot.CourseId = student.CourseId;
            slot.Status = LessonStatus.Pending;
            slot.Completed = false;
            slot.Note = string.IsNullOrWhiteSpace(slot.Note) ? "Запазен час" : slot.Note;

            await _context.SaveChangesAsync();
        }

        public async Task<List<InstructorListViewModel>> GetInstructorsAsync()
        {
            return await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Course)
                .Select(i => new InstructorListViewModel
                {
                    Id = i.Id,
                    FullName = i.User.FirstName + " " + i.User.LastName,
                    PhoneNumber = i.User.PhoneNumber,
                    Email = i.User.Email,
                    CourseName = i.Course.Name
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
                FullName = instructor.User.FirstName + " " + instructor.User.LastName,
                Email = instructor.User.Email,
                PhoneNumber = instructor.User.PhoneNumber,
                SchoolName = "Autoschool Lucky-Cars EOOD"
            };
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

        public async Task<EditStudentProfileViewModel> GetEditProfileAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);

            return new EditStudentProfileViewModel
            {
                FirstName = student.User.FirstName,
                LastName = student.User.LastName,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                CourseId = student.CourseId ?? 0
            };
        }

        public async Task<List<MyLessonListItemViewModel>> GetMyLessonsAsync(ClaimsPrincipal user)
        {
            var student = await GetStudentAsync(user);

            return await _context.PracticeLessons
                .Where(l => l.StudentId == student.Id)
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .Include(l => l.Course)
                .OrderByDescending(l => l.DateTime)
                .Select(l => new MyLessonListItemViewModel
                {
                    Id = l.Id,
                    DateTime = l.DateTime,
                    InstructorName = l.Instructor != null ? (l.Instructor.User.FirstName + " " + l.Instructor.User.LastName) : null,
                    CourseName = l.Course != null ? l.Course.Name : null,
                    Status = l.Status.ToString(),
                    Completed = l.Completed,
                    Note = l.Note
                })
                .ToListAsync();
        }

        public async Task CancelLessonAsync(ClaimsPrincipal user, int lessonId)
        {
            var student = await GetStudentAsync(user);

            var lesson = await _context.PracticeLessons
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.StudentId == student.Id);

            if (lesson == null)
                throw new InvalidOperationException("Часът не е намерен.");

            if (lesson.DateTime <= DateTime.Now)
                throw new InvalidOperationException("Не можеш да отменяш минали часове.");

            if (lesson.Status != LessonStatus.Pending && lesson.Status != LessonStatus.Approved)
                throw new InvalidOperationException("Този час не може да бъде отменен.");

            lesson.Status = LessonStatus.Cancelled;
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

            var practice = await _context.PracticeLessons
                .Where(l => l.StudentId == student.Id && l.DateTime >= now)
                .Include(l => l.Instructor).ThenInclude(i => i.User)
                .OrderBy(l => l.DateTime)
                .Select(l => new SchedulePracticeRowViewModel
                {
                    Id = l.Id,
                    DateTime = l.DateTime,
                    InstructorName = l.Instructor != null ? (l.Instructor.User.FirstName + " " + l.Instructor.User.LastName) : null,
                    Status = l.Status.ToString(),
                    Completed = l.Completed
                })
                .ToListAsync();

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
    }
}
