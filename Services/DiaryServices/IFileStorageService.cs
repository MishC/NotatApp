using Microsoft.AspNetCore.Http;    

namespace NotatApp.Services.DiaryServices
{
    public interface IFileStorageService
    {
        Task<StoredFileResult> SaveDiaryImageAsync(IFormFile image, string userId);
        Task DeleteFileAsync(string? relativePath);
        string GetAbsolutePath(string relativePath);
    }
}