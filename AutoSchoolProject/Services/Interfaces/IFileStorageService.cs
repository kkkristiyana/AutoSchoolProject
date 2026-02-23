namespace AutoSchoolProject.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string?> SaveFileAsync(IFormFile file, string folderName);
        void DeleteFile(string? filePath);
    }
}