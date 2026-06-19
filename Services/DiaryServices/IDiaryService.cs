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

        Task<(string absolutePath, string contentType)?> GetDiaryImageAsync(
            int id,
            string userId
        );
    }
}