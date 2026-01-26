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
        Task<InstructorDetailsViewModel> GetInstructorDetailsAsync(int instructorId);

        Task<BookLessonViewModel> GetBookLessonAsync(ClaimsPrincipal user, int instructorId);
        Task BookLessonAsync(ClaimsPrincipal user, BookLessonViewModel model);
    }
}
