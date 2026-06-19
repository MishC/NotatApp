using NotatApp.Models;

namespace NotatApp.Repositories.DiaryRepositories
{
    public interface IDiaryRepository
    {
        Task<List<DiaryEntry>> GetByDateAsync(string userId, DateOnly date);

        Task<DiaryEntry?> GetByIdAsync(int id, string userId);

        Task AddAsync(DiaryEntry entry);

        Task SaveChangesAsync();
    }
}