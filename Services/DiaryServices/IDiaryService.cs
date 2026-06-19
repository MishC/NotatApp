using NotatApp.Models;

namespace NotatApp.Services.DiaryServices
{
    public interface IDiaryService
    {
        Task<List<DiaryEntry>> GetDiaryEntriesAsync(string userId, DateOnly date);

        Task<DiaryEntry> CreateDiaryEntryAsync(
            string userId,
            CreateDiaryEntryDto dto
        );

        Task<bool> UpdateDiaryEntryAsync(
            int id,
            UpdateDiaryEntryDto dto,
            string userId
        );

        Task<DiaryPage?> CreateDiaryPageAsync(
            int entryId,
            CreateDiaryPageDto dto,
            string userId
        );

        Task<bool> UpdateDiaryPageAsync(
            int pageId,
            UpdateDiaryPageDto dto,
            string userId
        );

        Task<bool> DeleteDiaryPageAsync(int pageId, string userId);

        Task<(string absolutePath, string contentType)?> GetDiaryImageAsync(
            int id,
            string userId
        );

        Task<(string absolutePath, string contentType)?> GetDiaryPageImageAsync(
            int pageId,
            string userId
        );

        Task<bool> DeleteDiaryEntryByIdAsync(int id, string userId);

        Task<bool> DeleteDiaryEntriesByDateAsync(string userId, DateOnly date);
    }
}
