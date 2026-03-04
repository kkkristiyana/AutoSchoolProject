using Microsoft.AspNetCore.Http;

namespace AutoSchoolProject.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile file, string folder, string? oldRelativePath = null);
    }
}

