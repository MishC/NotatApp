using Microsoft.AspNetCore.Http;

namespace NotatApp.Services.DiaryServices
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IHostEnvironment _env;

        public FileStorageService(IHostEnvironment env)
        {
            _env = env;
        }

        public async Task<StoredFileResult> SaveDiaryImageAsync(IFormFile image, string userId)
        {
            var allowedTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };

            if (!allowedTypes.Contains(image.ContentType))
                throw new InvalidOperationException("Only JPG, PNG and WEBP images are allowed.");

            if (image.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("Image is too large. Max size is 5 MB.");

            var uploadsRoot = Path.Combine(
                _env.ContentRootPath,
                "private-uploads",
                "diary",
                userId
            );

            Directory.CreateDirectory(uploadsRoot);

            var originalFileName = Path.GetFileName(image.FileName);
            var extension = Path.GetExtension(originalFileName);

            var safeFileName = $"{Guid.NewGuid()}{extension}";
            var absolutePath = Path.Combine(uploadsRoot, safeFileName);

            await using var stream = new FileStream(absolutePath, FileMode.Create);
            await image.CopyToAsync(stream);

            var relativePath = Path.Combine(
                "private-uploads",
                "diary",
                userId,
                safeFileName
            );

            return new StoredFileResult
            {
                ImagePath = relativePath,
                ImageContentType = image.ContentType,
                ImageFileName = originalFileName
            };
        }

        public Task DeleteFileAsync(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.CompletedTask;

            var absolutePath = Path.Combine(_env.ContentRootPath, relativePath);

            if (File.Exists(absolutePath))
                File.Delete(absolutePath);

            return Task.CompletedTask;
        }

        public string GetAbsolutePath(string relativePath)
        {
            return Path.Combine(_env.ContentRootPath, relativePath);
        }
    }
}