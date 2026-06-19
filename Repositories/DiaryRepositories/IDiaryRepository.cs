using NotatApp.Models;

namespace NotatApp.Repositories.DiaryRepositories
{
    public interface IDiaryRepository
    {
        Task<List<DiaryEntry>> GetByDateAsync(string userId, DateOnly date);

        Task<DiaryEntry?> GetByIdAsync(int id, string userId);

        Task<DiaryPage?> GetPageByIdAsync(int pageId, string userId);

        Task AddAsync(DiaryEntry entry);

        Task AddPageAsync(DiaryPage page);

        Task SaveChangesAsync();

        Task DeleteAsync(DiaryEntry entry);

        Task DeletePageAsync(DiaryPage page);

        Task DeleteByDateAsync(List<DiaryEntry> entries);
    }
}
