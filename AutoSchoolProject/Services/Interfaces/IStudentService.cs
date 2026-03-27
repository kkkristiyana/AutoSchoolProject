using AutoSchoolProject.Models;
using AutoSchoolProject.ViewModels.Student;
using System.Security.Claims;

namespace AutoSchoolProject.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentProfileViewModel> GetProfileAsync(ClaimsPrincipal user);
        Task<EditStudentProfileViewModel> GetEditProfileAsync(ClaimsPrincipal user);
        Task UpdateProfileAsync(ClaimsPrincipal user, EditStudentProfileViewModel model);

        Task<List<InstructorListViewModel>> GetInstructorsAsync();
        Task<List<InstructorListViewModel>> GetInstructorsAsync(ClaimsPrincipal user);

        Task<InstructorDetailsViewModel> GetInstructorDetailsAsync(int instructorId);
        Task<InstructorDetailsViewModel> GetInstructorDetailsAsync(ClaimsPrincipal user, int instructorId);

        Task<BookLessonViewModel> GetBookLessonAsync(ClaimsPrincipal user, int instructorId);
        Task BookLessonAsync(ClaimsPrincipal user, BookLessonViewModel model);
        Task<List<PracticeLesson>> GetInstructorLessonsAsync(int instructorId, DateTime start, DateTime end);
        Task<List<MyLessonListItemViewModel>> GetMyLessonsAsync(ClaimsPrincipal user);
        Task<StudentTestResultsViewModel> GetTestResultsAsync(ClaimsPrincipal user);
        Task CancelLessonAsync(ClaimsPrincipal user, int lessonId);
        Task<StudentScheduleViewModel> GetScheduleAsync(ClaimsPrincipal user);
        Task<List<PracticeLesson>> GetInstructorLessonsAsync(int instructorId);
    }
}
