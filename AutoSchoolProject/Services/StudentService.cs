using AutoSchoolProject.Data;
using AutoSchoolProject.Models;
using AutoSchoolProject.Models.Enums;
using AutoSchoolProject.ViewModels.Student;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AutoSchoolProject.Services.Interfaces;

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

            int completedLessons = student.ScheduledLessons
                .Count(l => l.Completed);

            return new StudentProfileViewModel
            {
                FullName = $"{student.User.FirstName} {student.User.LastName}",
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                CourseName = student.Course?.Name,

                CompletedLessons = completedLessons,
                RemainingLessons = 31 - completedLessons
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

        public async Task BookLessonAsync(ClaimsPrincipal user, BookLessonViewModel model)
        {
            var student = await GetStudentAsync(user);

            var lesson = new PracticeLesson
            {
                StudentId = student.Id,
                InstructorId = model.InstructorId,
                CourseId = student.CourseId,
                DateTime = model.DateTime,
                Completed = false,
                Status = LessonStatus.Pending
            };

            var duration = 50;

            bool overlaps = await _context.PracticeLessons.AnyAsync(l =>
                l.InstructorId == model.InstructorId &&
                l.Status != LessonStatus.Cancelled &&
                l.Status != LessonStatus.Rejected &&
                l.DateTime < model.DateTime.AddMinutes(duration) &&
                model.DateTime < l.DateTime.AddMinutes(l.DurationMinutes));

            if (overlaps) throw new InvalidOperationException("Този час вече е зает.");

            _context.PracticeLessons.Add(lesson);
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
        public async Task<BookLessonViewModel> GetBookLessonAsync(
            ClaimsPrincipal user,
            int instructorId)
        {
            var student = await GetStudentAsync(user);

            return new BookLessonViewModel
            {
                InstructorId = instructorId,
                StudentId = student.Id,
                CourseId = student.CourseId
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
                CourseId = (int)student.CourseId
            };
        }

    }
}