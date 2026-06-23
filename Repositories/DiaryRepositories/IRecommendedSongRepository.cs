using NotatApp.Models;

namespace NotatApp.Repositories.DiaryRepositories
{
    public interface IRecommendedSongRepository
    {
        Task<RecommendedSong> SaveForDiaryEntryAsync(int diaryEntryId, RecommendedSong song);
    }
}
