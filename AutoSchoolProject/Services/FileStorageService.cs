using AutoSchoolProject.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AutoSchoolProject.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;

        private const long MaxBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveImageAsync(IFormFile file, string folder, string? oldRelativePath = null)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("Файлът е празен.");

            if (file.Length > MaxBytes)
                throw new InvalidOperationException("Файлът е твърде голям (макс 5MB).");

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExt.Contains(ext))
                throw new InvalidOperationException("Позволени формати: JPG, PNG, WEBP.");

            folder = folder.Trim().TrimStart('~').TrimStart('/').TrimEnd('/');

            var root = _env.WebRootPath;
            var physicalFolder = Path.Combine(root, folder);
            Directory.CreateDirectory(physicalFolder);

            var fileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            var physicalPath = Path.Combine(physicalFolder, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            TryDeleteOld(oldRelativePath);
            return "/" + folder.Replace("\\", "/") + "/" + fileName;
        }

        private void TryDeleteOld(string? oldRelativePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(oldRelativePath)) return;
                var rel = oldRelativePath.TrimStart('/');
                var full = Path.Combine(_env.WebRootPath, rel);
                if (File.Exists(full)) File.Delete(full);
            }
            catch
            {
                //ignore
            }
        }
    }
}
